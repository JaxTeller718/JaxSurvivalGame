using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MapMinimap
{
    /// <summary>
    /// Renders the map
    /// </summary>

    public class MapRenderer : MonoBehaviour
    {
        [Header("Map")]
        public Material material;
        public int render_texture_size = 4096;
        public bool multi_resolution = true;
        public float refresh_rate = 0.2f;
        public float refresh_rate_icon = 0.5f;

        [Header("Icons")]
        public float icon_scale = 1f;
        public bool show_player = true;

        private RenderTexture render_texture;
        private MapViewer viewer;
        private FogRenderer fog;
        private RawImage image;
        private RectTransform rect;
        private RectTransform rect_child;
        private Vector2Int texture_size;
        private float aspect_ratio = 1f;
        private float update_timer = 99f;
        private float icon_update_timer = 99f;
        private bool inited = false;
        private bool rendered = false;

        private List<MapIcon> active_icons = new List<MapIcon>(); 

        void Awake()
        {
            viewer = GetComponentInParent<MapViewer>();
            fog = GetComponent<FogRenderer>(); //May be null
            image = GetComponent<RawImage>();
            rect = GetComponent<RectTransform>();
            texture_size = new Vector2Int(render_texture_size, render_texture_size);
            image.enabled = false;

            if (fog && !multi_resolution)
                fog.multi_resolution = false;
        }

        private void Start()
        {
            if (!inited)
                InitRenderer();

            RefreshActiveIcons();
        }

        private void OnDestroy()
        {
            if(render_texture != null)
                render_texture.Release();
        }

        private void InitRenderer()
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.map != null && settings.zone != null)
            {
                //Rescale if not square
                aspect_ratio = settings.map.width / (float)settings.map.height;
                if (aspect_ratio > 1f)
                    texture_size = new Vector2Int(render_texture_size, Mathf.RoundToInt(render_texture_size / aspect_ratio));
                else
                    texture_size = new Vector2Int(Mathf.RoundToInt(render_texture_size * aspect_ratio), render_texture_size);

                render_texture = new RenderTexture(texture_size.x, texture_size.y, 0, RenderTextureFormat.ARGB32);
                image.texture = render_texture;
                inited = true;

                if (multi_resolution)
                {
                    GameObject render = new GameObject("MapRenderMulti");
                    rect_child = render.AddComponent<RectTransform>();
                    rect_child.SetParent(rect);
                    rect_child.transform.localPosition = Vector3.zero;
                    rect_child.transform.localRotation = Quaternion.identity;
                    rect_child.localScale = Vector3.one;
                    rect_child.anchoredPosition = Vector3.zero;
                    rect_child.sizeDelta = rect.sizeDelta + Vector2.one * 2f;
                    RawImage img = render.AddComponent<RawImage>();
                    img.material = image.material;
                    img.texture = image.texture;
                    image.enabled = false;
                    image = img;
                }
            }
        }

        void Update()
        {
            update_timer += Time.deltaTime;
            if (update_timer > refresh_rate)
            {
                update_timer = 0f;
                InitAndRender();
            }

            icon_update_timer += Time.deltaTime;
            if (icon_update_timer > refresh_rate_icon)
            {
                icon_update_timer = 0f;
                RefreshActiveIcons();
            }
        }

        public void InitAndRender()
        {
            if (fog != null)
                fog.InitAndRender();

            if (!inited)
                InitRenderer();

            if(inited)
                RenderMap();
        }

        public void RenderMap()
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null)
            {
                //Precalculate values
                float factor_scale = viewer.GetFactorScale();
                Vector2 map_pos = viewer.GetCurrentPos();
                Vector2 render_pos = Vector2.zero;
                Vector2 render_size = new Vector2(render_texture.width, render_texture.height);

                if (multi_resolution)
                {
                    float aspect_ratio = render_size.x / render_size.y;
                    Vector3 ascale = new Vector3(aspect_ratio, 1f, 1f);
                    if (render_size.x > render_size.y)
                        ascale = new Vector3(1f, 1f / aspect_ratio, 1f);

                    rect_child.anchoredPosition = viewer.GetPixelPos();
                    rect_child.localScale = ascale * 2f / factor_scale;
                    render_size = render_size * factor_scale * 0.5f;
                    Vector2 offset_pos = new Vector2(map_pos.x * render_texture.width, -map_pos.y * render_texture.height) * factor_scale * 0.5f * 0.5f;
                    render_pos = new Vector2(render_texture.width * 0.5f - render_size.x * 0.5f, render_texture.height * 0.5f - render_size.y * 0.5f) - offset_pos;
                }

                bool draw_fog = settings.data.fog && fog != null;
                bool fog_ready = draw_fog && fog.IsRendered();

                if (!draw_fog || fog_ready)
                {
                    RenderTexture.active = render_texture;
                    GL.PushMatrix();
                    GL.LoadPixelMatrix(0, render_texture.width, render_texture.height, 0);
                    GL.Clear(true, true, fog ? fog.fog_color : Color.black);

                    material.SetPass(0);

                    //Draw map
                    Graphics.DrawTexture(new Rect(render_pos.x, render_pos.y, render_size.x, render_size.y), settings.map);

                    //Draw icons
                    RenderMapIcons(false);

                    //Draw fog
                    if (fog_ready)
                    {
                        if (fog.multi_resolution)
                            Graphics.DrawTexture(new Rect(0f, 0f, render_texture.width, render_texture.height), fog.GetTexture());
                        else
                            Graphics.DrawTexture(new Rect(render_pos.x, render_pos.y, render_size.x, render_size.y), fog.GetTexture());
                    }

                    //Draw icons over fog
                    RenderMapIcons(true);

                    GL.PopMatrix();
                    RenderTexture.active = null;
                    image.enabled = true;
                    rendered = true;
                }
            }
        }

        private void RenderMapIcons(bool over_fog)
        {
            MapLevelSettings settings = MapLevelSettings.Get();

            //Precalculate values
            float factor_scale = viewer.GetFactorScale();
            Vector2 map_pos = viewer.GetCurrentPos();
            Vector2 pos_offset = Vector2.zero;
            Vector2 icon_size = Vector2.one * settings.data.icon_size * icon_scale;
            float zoom_max_icon = Mathf.Max(settings.data.zoom_max * 0.5f, 2f);
            float zoom_max_icon_minus = zoom_max_icon - 1f;
            float sqrt_zoom = Mathf.Sqrt(viewer.GetCurrentZoom());
            float pos_mult = 1f;

            if (multi_resolution)
            {
                icon_size = icon_size * factor_scale * 0.5f;
                pos_mult = factor_scale * 0.5f;
                pos_offset = new Vector2(map_pos.x * render_texture.width, -map_pos.y * render_texture.height) * factor_scale * 0.5f * 0.5f;
            }

            //Add icons
            foreach (MapIcon icon in active_icons)
            {
                if (icon != null && over_fog == icon.IsImportant())
                {
                    Vector2 pos = icon.GetMapPos();
                    pos = (pos * pos_mult + Vector2.one) * 0.5f; //Convert to 0 to 1
                    pos.x = pos.x * render_texture.width; //Scale to render texture
                    pos.y = (1f - pos.y) * render_texture.height;

                    Vector2 isize = icon_size * icon.scale;
                    if (icon.autoscale)
                        isize = isize * (zoom_max_icon - sqrt_zoom * zoom_max_icon_minus);

                    //Center anchor
                    pos = pos - pos_offset - isize * 0.5f;

                    if (icon.autorotate)
                    {
                        GL.PushMatrix();
                        Matrix4x4 tMat = Matrix4x4.TRS(new Vector3(pos.x + isize.x * 0.5f, pos.y + isize.y * 0.5f, 0f), Quaternion.identity, Vector3.one);
                        Matrix4x4 rMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, icon.GetRotation()), Vector3.one);
                        GL.MultMatrix(tMat * rMat * Matrix4x4.Inverse(tMat) * GL.modelview);
                        Graphics.DrawTexture(new Rect(pos.x, pos.y, isize.x, isize.y), icon.icon.texture);
                        GL.PopMatrix();
                    }
                    else
                    {
                        Graphics.DrawTexture(new Rect(pos.x, pos.y, isize.x, isize.y), icon.icon.texture);
                    }
                }
            }
        }

        //Keep a list of active icons for faster looping in other functions
        private void RefreshActiveIcons()
        {
            active_icons.Clear();
            foreach (MapIcon icon in MapIcon.GetAll())
            {
                if (icon.IsIconVisible())
                {
                    if (show_player || icon.type != MapIconType.Player)
                    {
                        Vector2 pos = icon.GetMapPos();
                        if (viewer.IsInsideView(pos, 0.2f))
                        {
                            active_icons.Add(icon);
                        }
                    }
                }
            }
        }

        public RenderTexture GetTexture()
        {
            return render_texture;
        }

        public Vector2Int GetTextureSize()
        {
            return texture_size;
        }

        public bool IsRendered()
        {
            return rendered;
        }
    }

}
