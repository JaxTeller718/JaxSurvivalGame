using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MapMinimap
{
    public enum MapIconType
    {
        Default = 0,
        Player=5,
        Important = 10, //Will display over fog
    }

    /// <summary>
    /// Show icon on the map
    /// </summary>

    public class MapIcon : MonoBehaviour
    {
        public string id; //Not necessary, but useful if you want to access a specific MapIcon by script
        public MapIconType type;
        public Sprite icon;

        [Header("Transform")]
        public bool autorotate = false; //Warning, slow performance, use for a few icons only like player
        public bool autoscale = false;
        public float scale = 1f;

        [Header("Description")]
        public string title;
        [TextArea(4, 5)]
        public string desc;

        public UnityAction onClick; //When clicking on that icon in the map

        private static List<MapIcon> icon_list = new List<MapIcon>();
        private static MapIcon player_icon = null;

        private MapLevelSettings map_settings;
        private Transform trans;
        private Vector2 map_pos; //Only for static icons
        private bool revealed = false; //If this has been revealed from fog

        private void Awake()
        {
            trans = transform;
            icon_list.Add(this);
            if (type == MapIconType.Player)
                player_icon = this;
        }

        private void OnDestroy()
        {
            icon_list.Remove(this);
        }

        private void Start()
        {
            RefreshPosition();   
        }

        public void RefreshRevealed()
        {
            revealed = MapManager.Get().IsRevealed(trans.position); //Slow
        }

        public void Reveal()
        {
            revealed = true;
        }

        private void RefreshPosition() {
            map_settings = MapLevelSettings.Get();
            if (map_settings != null && map_settings.IsValid())
                map_pos = map_settings.zone.GetNormalizedPos(trans.position);
        }

        //Return -1 to 1 map position
        public Vector2 GetMapPos()
        {
            if (gameObject != null)
            {
                if (gameObject.isStatic)
                    return map_pos; //Already calculated at start
                if (map_settings != null && map_settings.IsValid())
                    return map_settings.zone.GetNormalizedPos(trans.position);
            }
            return Vector2.zero;
        }

        public Vector3 GetWorldPos()
        {
            return trans.position;
        }

        public float GetRotation()
        {
            float map_rot = map_settings != null ? map_settings.zone.GetRotation() : 0f;
            Quaternion lookRot = Quaternion.LookRotation(trans.forward, Vector3.up);
            return lookRot.eulerAngles.y - map_rot;
        }

        //Return if map icon is revealed from fog, this function may not return the accurate answer as its not refreshed every frame, which makes performance faster
        //If you want to know the immediate value, use MapManager.Get().IsRevealed, but that one function has bad performance
        public bool IsRevealed()
        {
            return revealed || IsImportant(); //Important icons show above fog
        }

        public bool IsImportant()
        {
            return type == MapIconType.Player || type == MapIconType.Important;
        }

        public bool IsIconVisible()
        {
            return icon != null && gameObject.activeSelf;
        }

        public bool HasDescription()
        {
            return !string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(desc);
        }

        //Title and desc in one
        public string GetTitleDesc()
        {
            string separator = !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(desc) ? ": " : "";
            return title + separator + desc;
        }

        public static MapIcon GetPlayer()
        {
            return player_icon;
        }

        public static MapIcon Get(string id)
        {
            foreach (MapIcon marker in icon_list)
            {
                if (marker.id == id)
                    return marker;
            }
            return null;
        }

        public static List<MapIcon> GetAll()
        {
            return icon_list;
        }

        //Add icon to fixed position
        public static MapIcon Create(string id, Sprite icon, Vector3 world_pos, MapIconType type = MapIconType.Default)
        {
            GameObject marker = new GameObject("MapIcon-" + id);
            marker.transform.position = world_pos;
            MapIcon micon = marker.AddComponent<MapIcon>();
            micon.id = id;
            micon.type = type;
            micon.icon = icon;
            return micon;
        }

        //Add icon to follow position
        public static MapIcon Create(string id, Sprite icon, Transform parent, MapIconType type = MapIconType.Default)
        {
            GameObject marker = new GameObject("MapIcon-" + id);
            marker.transform.SetParent(parent);
            marker.transform.localPosition = Vector3.zero;
            MapIcon micon = marker.AddComponent<MapIcon>();
            micon.id = id;
            micon.type = type;
            micon.icon = icon;
            return micon;
        }

        //Delete map icon
        public static void Delete(string id)
        {
            MapIcon icon = MapIcon.Get(id);
            if (icon != null)
                Destroy(icon.gameObject);
        }

        public static void RefreshAll()
        {
            foreach (MapIcon icon in icon_list)
                icon.RefreshPosition();
        }
    }

}
