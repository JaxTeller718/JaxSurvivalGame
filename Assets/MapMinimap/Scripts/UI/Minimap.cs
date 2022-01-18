using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MapMinimap
{

    public class Minimap : UIPanel
    {
        public float map_zoom = 0.5f;
        public bool open_on_click = true;

        public UnityAction<Vector2> onClick; //Vector2 is the map position -1 to 1

        private MapViewer viewer;

        private static Minimap _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            viewer = GetComponentInChildren<MapViewer>();
            Show(true);
        }

        protected override void Start()
        {
            base.Start();

            if (viewer != null)
                viewer.onClick += (Vector2 pos) => { if (onClick != null) onClick.Invoke(pos); };

            MapManager manager = MapManager.Get();
            if (manager == null)
            {
                Debug.LogError("There are no MapManager in the scene, make sure to add the prefab to the scene!");
                gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            base.Update();

            MapIcon player = MapIcon.GetPlayer();
            if (player != null && viewer != null)
                viewer.SetMapPosition(player.GetMapPos());

            if(viewer != null)
                viewer.SetMapZoom(map_zoom);
        }

        public void OnClickMap()
        {
            if(open_on_click)
                MapManager.Get().OpenMap();
        }

        public MapViewer GetViewer()
        {
            return viewer;
        }

        public static Minimap Get()
        {
            return _instance;
        }
    }

}
