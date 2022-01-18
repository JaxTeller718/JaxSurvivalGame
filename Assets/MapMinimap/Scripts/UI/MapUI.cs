using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MapMinimap
{
    /// <summary>
    /// UI that displays the map
    /// </summary>

    public class MapUI : UIPanel
    {
        public UnityAction<Vector2> onClick; //Vector2 is the map position -1 to 1

        private MapViewer viewer;
        private Canvas canvas;

        private static MapUI _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            canvas = GetComponent<Canvas>();
            viewer = GetComponentInChildren<MapViewer>();

            Hide(true);
        }

        protected override void Start()
        {
            base.Start();

            if (canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;

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

            if (IsVisible())
            {
                MapControls controls = MapControls.Get();

                //Cancel
                if (controls.IsPressCancel())
                    Hide();

            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            viewer.RefreshMap();
        }

        public MapViewer GetViewer()
        {
            return viewer;
        }

        public static MapUI Get()
        {
            return _instance;
        }
    }

}