using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MapMinimap
{
    public enum MapCaptureMode
    {
        Editor=0, //Captured manually by pressing a button in the editor
        Runtime = 5, //Captured once at runtime when the scene is loaded
        RuntimeUpdate = 10, //Captured every frame and updated in real time, Warning: can be slow
    }

    public enum MapSaveMode
    {
        File = 0,  //Editor mode only, will save to PNG file
        RenderTexture = 5, //Will save to a render texture
    }

    /// <summary>
    /// Class to auto generate the map from your scene and creating a PNG image, EDITOR only
    /// </summary>

    public class MapCapture : MonoBehaviour
    {
        public MapCaptureMode mode;

        [Header("Zone")]
        public MapZone zone;

        [Header("Camera")]
        public Camera capture_camera;

        [Header("Save Target")]
        public MapSaveMode save_mode;

        [HideInInspector] public int file_size = 2048;
        [HideInInspector] public string save_folder = "MapMinimap/Maps";
        [HideInInspector] public RenderTexture render_texture;

        private bool initialized = false;

        private void Awake()
        {
            if (mode == MapCaptureMode.Editor)
            {
                //Disable camera and script in Editor mode
                enabled = false;
                if(capture_camera != null)
                    capture_camera.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            enabled = false; //Run only first frame
            FirstUpdate();
        }

        private void FirstUpdate()
        {
            if (mode == MapCaptureMode.Runtime || mode == MapCaptureMode.RuntimeUpdate)
            {
                AutoSetup();
                CaptureMapTexture();
            }
        }

        //Automatically setup the zone bounds and the settings, then capture the map
        //This function is ran from Update to allow other initializations from Start functions to have occured
        public void AutoSetup()
        {
            if (initialized)
                return;

            initialized = true;

            //Find bounding box
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
                bounds.Encapsulate(r.bounds);

            //Set zone bounds
            zone.SetBounds(bounds);

            //Refresh settings
            MapLevelSettings settings = FindObjectOfType<MapLevelSettings>();
            settings.zone = zone;
            settings.map = render_texture;

            MapIcon.RefreshAll(); //Refresh all icons because zone changed

            //Create camera if it doesnt exist
            CreateCamera();
        }

        public void CreateCamera()
        {
            if (capture_camera == null)
            {
                //Create camera
                GameObject capture_cam = new GameObject("CaptureCam");
                capture_cam.transform.SetParent(transform);
                Camera camera = capture_cam.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.nearClipPlane = 1f;
                camera.farClipPlane = 500f;
                camera.orthographic = true;
                camera.orthographicSize = zone.GetSize().y / 2f;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.useOcclusionCulling = false;
                camera.renderingPath = RenderingPath.Forward;
                capture_cam.transform.position = zone.GetCenter() + Vector3.up * 50f;
                capture_cam.transform.rotation = Quaternion.Euler(90f, zone.GetRotation(), 0f);
                camera.targetTexture = null;
                capture_camera = camera;
            }
        }

        public void ResetCamera()
        {
            if (capture_camera != null)
            {
                DestroyObj(capture_camera.gameObject);
                capture_camera = null;
                CreateCamera();
            }
        }

        private Texture2D CaptureMapFile()
        {
            Vector2 map_size = zone.GetSize();
            int sizeX = GetMultOf4(Mathf.RoundToInt(file_size * map_size.x / map_size.y));
            int sizeY = GetMultOf4(file_size);

            //Render in texture
            RenderTexture temp_render_texture = new RenderTexture(sizeX, sizeY, 0);
            temp_render_texture.isPowerOfTwo = false;
            capture_camera.gameObject.SetActive(true);
            capture_camera.targetTexture = temp_render_texture;
            capture_camera.Render();

            RenderTexture.active = temp_render_texture;
            Texture2D texture = new Texture2D(sizeX, sizeY);
            texture.ReadPixels(new Rect(0, 0, sizeX, sizeY), 0, 0);
            texture.Apply();
            RenderTexture.active = null;

            capture_camera.targetTexture = null;
            capture_camera.gameObject.SetActive(false);
            DestroyObj(temp_render_texture);

            return texture;
        }

        private Texture CaptureMapTexture()
        {
            capture_camera.gameObject.SetActive(true);
            capture_camera.targetTexture = render_texture;
            capture_camera.Render();
            capture_camera.targetTexture = mode != MapCaptureMode.Editor ? render_texture : null;
            capture_camera.gameObject.SetActive(mode == MapCaptureMode.RuntimeUpdate);
            return render_texture;
        }

        public void CaptureMapAndSave()
        {
            if (zone == null || capture_camera == null)
            {
                Debug.LogError("A zone and camera must be set to use this feature");
                return;
            }

            if (save_mode == MapSaveMode.RenderTexture && render_texture == null)
            {
                Debug.LogError("A render texture must be assigned!");
                return;
            }

            if (save_mode == MapSaveMode.File)
            {
                //Capture texture and save
                Texture2D texture = CaptureMapFile();
                SaveToPNG(texture);
                DestroyObj(texture);
            }

            if (save_mode == MapSaveMode.RenderTexture)
            {
                CaptureMapTexture();
                SelectObject(render_texture);
            }
        }

        public Sprite SaveToPNG(Texture2D texture)
        {
            Sprite sprite = null;
#if UNITY_EDITOR
            byte[] bytes = texture.EncodeToPNG();
            string folder = Application.dataPath + "/" + save_folder;
            string file = SceneManager.GetActiveScene().name + ".png";
            File.WriteAllBytes(folder + "/" + file, bytes);

            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath("Assets/" + save_folder + "/" + file) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
            }

            sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + save_folder + "/" + file);
            SelectObject(sprite);
#endif
            return sprite;
        }

        private void SelectObject(Object obj)
        {
#if UNITY_EDITOR
            Object[] selection = new Object[1];
            selection[0] = obj;
            Selection.objects = selection;
#endif
        }

        private void DestroyObj(Object obj)
        {
            if (Application.isPlaying)
                Destroy(obj); //Runtime 
            else 
                DestroyImmediate(obj); //Editor 
        }

        public int GetMultOf4(int size)
        {
            int extra = size % 4;
            return size - extra;
        }
    }
}
