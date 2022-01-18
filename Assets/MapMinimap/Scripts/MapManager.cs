using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MapMinimap
{
    /// <summary>
    /// Class to manage the map UI in game
    /// </summary>

    public class MapManager : MonoBehaviour
    {
        public GameObject map_ui;
        public GameObject event_system;

        public UnityAction onOpenMap;
        public UnityAction onCloseMap;

        [HideInInspector]
        public bool show_player_warning = true;

        private MapSceneData scene_data;
        private float update_timer = 0f;
        private int icon_reveal_index = 0;

        private static MapManager _instance;

        private void Awake()
        {
            _instance = this;

            MapUI ui = FindObjectOfType<MapUI>();
            if (ui == null)
                Instantiate(map_ui);

            EventSystem evt_sys = FindObjectOfType<EventSystem>();
            if (evt_sys == null)
                Instantiate(event_system);

            MapData.LoadLast();

            if (scene_data == null)
                scene_data = new MapSceneData();
        }

        private void Start()
        {
            MapUI.Get().onShow += () => { if (onOpenMap != null) onOpenMap.Invoke(); };
            MapUI.Get().onHide += () => { if (onCloseMap != null) onCloseMap.Invoke(); };

            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings == null)
            {
                Debug.Log("There are no MapLevelSettings in the scene, make sure to add one!");
            }

            MapIcon player = MapIcon.GetPlayer();
            if (player == null && show_player_warning)
            {
                Debug.Log("There is no MapIcon set to player in the scene, map fog won't be revealed!");
            }
        }

        private void Update()
        {
            MapData mdata = MapData.Get();
            if (mdata != null)
                scene_data = mdata.GetSceneData(MapTool.GetCurrentScene());

            MapControls controls = MapControls.Get();
            if (controls && controls.IsPressMap())
            {
                MapUI.Get().Toggle();
            }

            MapLevelSettings settings = MapLevelSettings.Get();
            update_timer += Time.deltaTime;
            if (settings != null && update_timer > settings.data.fog_update_rate)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            //Check if reveal fog
            MapLevelSettings settings = MapLevelSettings.Get();
            MapIcon player = MapIcon.GetPlayer();
            if (settings && settings.IsValid() && settings.data.fog && player != null)
            {
                if (player != null)
                {
                    Vector3 player_pos = player.GetWorldPos();
                    Vector2Int fog_point = MapTool.GetClosestFogPoint(player_pos);
                    scene_data.Reveal(fog_point, settings.data.fog_reveal_radius);
                }

                foreach (MapReveal reveal in MapReveal.GetAll())
                {
                    Vector3 rpos = reveal.GetWorldPos();
                    Vector2Int fog_point = MapTool.GetClosestFogPoint(rpos);
                    scene_data.Reveal(fog_point, settings.data.fog_reveal_radius);
                }
            }

            //Refresh icons in fog
            if (settings && settings.IsValid() && player != null)
            {
                int nb_icons = MapIcon.GetAll().Count;
                int reveal_min = icon_reveal_index;
                int reveal_max = icon_reveal_index + settings.data.icon_refresh_count; //Only refresh 100 icons per SlowUpdate, for performance
                icon_reveal_index = reveal_max < nb_icons ? reveal_max : 0;

                int index = 0;
                foreach (MapIcon icon in MapIcon.GetAll())
                {
                    if (!icon.IsRevealed())
                    {
                        if(index >= reveal_min && index < reveal_max)
                            icon.RefreshRevealed(); //Refresh reveal with slow function
                        if ((player.GetWorldPos() - icon.GetWorldPos()).magnitude < settings.data.fog_reveal_radius)
                            icon.Reveal(); //Force reveal if near player
                    }
                    index++;
                }
            }
        }

        public void OpenMap()
        {
            MapUI.Get().Show();
        }

        public void CloseMap()
        {
            MapUI.Get().Hide();
        }

        //World position
        public void RevealAt(Vector3 pos, float radius)
        {
            Vector2Int fog_point = MapTool.GetClosestFogPoint(pos);
            scene_data.Reveal(fog_point, radius);
        }

        public void RevealAllMap()
        {
            scene_data.Reveal(Vector2Int.zero, 100000f);
        }

        //Check if position (in the scene) is revealed (very slow, bad performance, avoid using in loop)
        public bool IsRevealed(Vector3 world_pos)
        {
            world_pos.y = 0f;
            foreach (KeyValuePair<Vector2IData, float> point in scene_data.fog_reveal)
            {
                Vector3 fpos = MapTool.GetFogWorldPos(point.Key);
                float dist = (fpos - world_pos).magnitude;
                if (dist < point.Value)
                    return true;
            }
            return false;
        }

        //Check if map position is revealed (from -1 to 1 pos) (very slow, bad performance, avoid using in loop)
        public bool IsRevealedMap(Vector2 map_pos) {
            Vector3 world_pos = MapTool.MapToWorldPos(map_pos);
            return IsRevealed(world_pos);
        }

        public MapIcon AddIcon(string id, Sprite icon, Vector3 world_pos, MapIconType type = MapIconType.Default)
        {
            return MapIcon.Create(id, icon, world_pos, type);
        }

        public MapIcon AddIcon(string id, Sprite icon, Transform parent, MapIconType type = MapIconType.Default)
        {
            return MapIcon.Create(id, icon, parent, type);
        }

        public void DeleteIcon(string id)
        {
            MapIcon.Delete(id);
        }

        public MapSceneData GetMapSceneData()
        {
            return scene_data;
        }

        public static MapManager Get()
        {
            return _instance;
        }
    }

}
