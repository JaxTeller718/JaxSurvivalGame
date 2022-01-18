using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MapMinimap
{
    /// <summary>
    /// Renders the map
    /// </summary>

    public class MapViewer : MonoBehaviour
    {
        public float move_speed = 2f;
        public float zoom_speed = 1f;
        public float zoom_speed_scroll = 4f;
        public float zoom_speed_touch = 20f;
        public float hover_dist = 0.02f;
        public bool locked = false; //If true, manual movement/zoom is locked

        public UIPanel desc_box;
        public Text description;
        public Image cursor;

        public UnityAction<Vector2> onClick; //Vector2 is the map position -1 to 1
        public UnityAction<Vector2> onClickHold;
        public UnityAction<MapIcon> onClickIcon; 

        private MapRenderer map_render;
        private FogRenderer fog_render;
        private Canvas canvas;
        private RectTransform viewer_rect;
        private RectTransform map_rect;
        private Vector2 panel_size;
        private Vector2 map_render_size;

        private float current_zoom; //Normalized 0 to 1
        private Vector2 current_pos; //Normalized -1 to 1
        private float target_zoom; //Normalized 0 to 1
        private Vector2 target_pos; //Normalized -1 to 1
        private Vector2 last_pos;
        private bool can_drag = false;
        private MapIcon hover_icon = null;
        private float update_timer = 0f;

        void Awake()
        {
            map_render = GetComponentInChildren<MapRenderer>();
            fog_render = GetComponentInChildren<FogRenderer>();
            canvas = GetComponentInParent<Canvas>();
            viewer_rect = GetComponent<RectTransform>();
            map_rect = map_render.GetComponent<RectTransform>();
            if (cursor != null)
                cursor.enabled = false;
        }

        private void Start()
        {
            RefreshMap();
        }

        void Update()
        {
            MapControls controls = MapControls.Get();

            if (!locked)
                MoveMap();

            if (cursor != null)
                cursor.enabled = !locked && !controls.IsUseMouse();

            //Click on map/icon
            Vector2 pointing_map_pos = controls.IsUseMouse() ? ScreenToMapPos(Input.mousePosition) : GetMapAim();
            bool press_accept = Input.GetMouseButtonDown(0) || controls.IsPressAccept();
            bool press_hold = Input.GetMouseButton(0);
            if (press_accept && Input.touchCount < 2 && IsInsideView(pointing_map_pos))
            {
                //Debug.Log(mouse_map_pos);
                if (onClick != null)
                    onClick.Invoke(pointing_map_pos);
                if (hover_icon != null && onClickIcon != null)
                    onClickIcon.Invoke(hover_icon);
                if (hover_icon != null && hover_icon.onClick != null)
                    hover_icon.onClick.Invoke();
            }

            if (press_hold && IsInsideView(pointing_map_pos))
            {
                if (onClickHold != null)
                    onClickHold.Invoke(pointing_map_pos);
            }

            //Lerp
            current_pos = Vector2.Lerp(current_pos, target_pos, 5f * Time.deltaTime);
            current_zoom = Mathf.Lerp(current_zoom, target_zoom, 5f * Time.deltaTime);

            //Set position
            float pixel_scale = GetPixelScale();
            Vector2 anchor_pos = -current_pos * pixel_scale * map_render_size / 2f;
            map_rect.anchoredPosition = anchor_pos;
            map_rect.localScale = Vector3.one * pixel_scale;

            update_timer += Time.deltaTime;
            if (update_timer > 0.25f)
            {
                update_timer = 0f;
                RefreshHoverDesc();
            }
        }

        private void MoveMap()
        {
            MapControls controls = MapControls.Get();

            //Move controls
            Vector2 move_control = controls.GetMove();
            float scale_speed_mult = (1f / map_render.transform.localScale.y);
            target_pos += move_control * move_speed * scale_speed_mult * Time.deltaTime;
            target_pos = new Vector2(Mathf.Clamp(target_pos.x, -1f, 1f), Mathf.Clamp(target_pos.y, -1f, 1f));

            //Mouse Down
            if (Input.GetMouseButtonDown(0))
            {
                last_pos = ScreenToRectPos(Input.mousePosition);
                can_drag = true;
            }

            if (Input.touchCount >= 2)
            {
                can_drag = false; //Cancel mouse draging when 2+ fingers
            }

            //Mouse drag
            if (can_drag && Input.GetMouseButton(0))
            {
                Vector2 mPos = ScreenToRectPos(Input.mousePosition);
                Vector2 diff = mPos - last_pos;
                Vector2 tpos = map_rect.anchoredPosition + diff * GetPixelScale() * scale_speed_mult;
                SetMapAnchorPosition(tpos);
                last_pos = ScreenToRectPos(Input.mousePosition);
            }

            //Zoom controls
            target_zoom += Input.mouseScrollDelta.y * zoom_speed_scroll * Time.deltaTime;
            target_zoom += controls.GetZoomValue() * zoom_speed * Time.deltaTime;
            target_zoom += controls.GetZoomTouchValue() * zoom_speed_touch * Time.deltaTime;
            target_zoom = Mathf.Clamp01(target_zoom);
        }

        private void RefreshHoverDesc()
        {
            MapControls controls = MapControls.Get();

            MapLevelSettings settings = MapLevelSettings.Get();
            bool has_fog = settings && settings.data.fog;

            Vector2 pointing_map_pos = controls.IsUseMouse() ? ScreenToMapPos(Input.mousePosition) : GetMapAim();
            hover_icon = null;

            //Hovering icons
            if (IsInsideView(pointing_map_pos))
            {
                float min_dist = hover_dist;
                foreach (MapIcon icon in MapIcon.GetAll())
                {
                    if (icon.IsIconVisible())
                    {
                        Vector2 map_pos = icon.GetMapPos();
                        if (!has_fog || icon.IsRevealed())
                        {
                            float dist = (map_pos - pointing_map_pos).magnitude;
                            if (dist < min_dist)
                            {
                                min_dist = dist;
                                hover_icon = icon;
                            }
                        }
                    }
                }
            }

            //Description box
            if (desc_box != null)
                desc_box.SetVisible(hover_icon != null && hover_icon.HasDescription());
            if (hover_icon != null && description != null)
            {
                description.text = hover_icon.GetTitleDesc();
            }
        }

        public void RefreshMap()
        {
            map_render_size = new Vector2(map_render.GetTextureSize().x, map_render.GetTextureSize().y);
            map_rect.sizeDelta = map_render_size;
            map_rect.anchoredPosition = Vector2.zero;
            panel_size = viewer_rect.rect.size;
            current_zoom = 0.5f;
            target_zoom = 0.5f;
            current_pos = Vector2.zero;
            target_pos = Vector2.zero;
            
            float min_scale = panel_size.y / map_render_size.y;
            map_rect.localScale = Vector3.one * min_scale;
            last_pos = ScreenToRectPos(Input.mousePosition);

            MapIcon player = MapIcon.GetPlayer();
            if (player != null)
                SetMapPosition(player.GetWorldPos());

            //Force Render now
            map_render.InitAndRender();
        }

        //World position
        public void SetMapPosition(Vector3 world_pos)
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
            {
                Vector2 norm_pos = settings.zone.GetNormalizedPos(world_pos);
                SetMapPosition(norm_pos);
            }
        }

        //from -1 to 1
        public void SetMapPosition(Vector2 normalized_pos)
        {
            target_pos = normalized_pos;
            current_pos = normalized_pos;
            Vector2 anchor_pos = -current_pos * GetPixelScale() * map_render_size / 2f;
            map_rect.anchoredPosition = anchor_pos;
        }

        public void SetMapAnchorPosition(Vector2 anchor_pos)
        {
            map_rect.anchoredPosition = anchor_pos;
            current_pos = -anchor_pos * 2f / map_render_size / GetPixelScale();
            current_pos = new Vector2(Mathf.Clamp(current_pos.x, -1f, 1f), Mathf.Clamp(current_pos.y, -1f, 1f));
            target_pos = current_pos;
        }

        //From 0 to 1
        public void SetMapZoom(float val)
        {
            current_zoom = val;
            target_zoom = val;
            map_rect.localScale = Vector3.one * GetPixelScale();
        }

        public float GetCurrentZoom()
        {
            return current_zoom;
        }

        public Vector2 GetCurrentPos()
        {
            return current_pos;
        }

        public Vector2 GetPixelPos()
        {
            return current_pos * map_render_size / 2f;
        }

        public RectTransform GetRect()
        {
            return viewer_rect;
        }

        //Map pos is -1 to 1
        public bool IsInsideView(Vector2 map_pos, float offset=0f)
        {
            Vector2 rect_pos = MapToRectPos(map_pos);
            float resize = 0.5f + offset;
            Vector2 size = panel_size * resize;
            return rect_pos.x < size.x && rect_pos.x > -size.x && rect_pos.y < size.y && rect_pos.y > -size.y;
        }

        public float GetPixelScale()
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            float pixel_scale = 1f;
            if (settings != null && settings.IsValid())
            {
                float min_scale = panel_size.y / map_render_size.y;
                float max_scale = min_scale * settings.data.zoom_max;
                pixel_scale = (1f - current_zoom) * min_scale + current_zoom * max_scale;
            }
            return pixel_scale;
        }

        //A value between 1 and zoom_max, based on current zoom
        public float GetFactorScale()
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
                return current_zoom * (settings.data.zoom_max-1f) + 1f;
            return 1f;
        }

        //Center of the map
        public Vector2 GetMapAim()
        {
            return RectToMapPos(Vector2.zero);
        }

        public Vector2 WorldToCanvasPos(Vector3 world_pos)
        {
            Vector2 map_pos = MapTool.WorldToMapPos(world_pos);
            return new Vector2(map_pos.x * map_render_size.x / 2f, map_pos.y * map_render_size.y / 2f);
        }

        public Vector2 ScreenToMapPos(Vector2 screen_pos)
        {
            Vector2 rect_pos = ScreenToRectPos(screen_pos);
            Vector2 map_pos = RectToMapPos(rect_pos);
            return map_pos;
        }

        public Vector2 RectToMapPos(Vector2 rect_pos)
        {
            Vector2 map_pos;
            map_pos.x = rect_pos.x * 2f / (map_render_size.x * map_rect.localScale.x);
            map_pos.y = rect_pos.y * 2f / (map_render_size.y * map_rect.localScale.y);
            return map_pos + current_pos;
        }

        public Vector2 MapToRectPos(Vector2 map_pos)
        {
            Vector2 rpos = map_pos - current_pos;
            rpos.x = rpos.x * map_render_size.x * map_rect.localScale.x * 0.5f;
            rpos.y = rpos.y * map_render_size.y * map_rect.localScale.y * 0.5f;
            return rpos;
        }

        //Screen pos to anchored pos
        public Vector2 ScreenToRectPos(Vector2 screen_pos)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {
                Vector2 anchor_pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(viewer_rect, screen_pos, canvas.worldCamera, out anchor_pos);
                return anchor_pos;
            }
            else
            {
                Vector2 anchor_pos = screen_pos - new Vector2(viewer_rect.position.x, viewer_rect.position.y);
                anchor_pos = new Vector2(anchor_pos.x / viewer_rect.lossyScale.x, anchor_pos.y / viewer_rect.lossyScale.y);
                return anchor_pos;
            }
        }

        //Screen pos (-1 to 1)
        public Vector2 ScreenPosToScreenPercent(Vector2 screen_pos)
        {
            Vector2 pos = new Vector2(screen_pos.x / Screen.width, screen_pos.y / Screen.height);
            return pos * 2f - Vector2.one;
        }

        public MapRenderer GetMapRenderer()
        {
            return map_render;
        }

        public FogRenderer GetFogRenderer()
        {
            return fog_render;
        }
    }

}
