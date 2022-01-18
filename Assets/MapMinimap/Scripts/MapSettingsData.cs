using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{

    [CreateAssetMenu(fileName = "MapSettings", menuName = "Data/MapSettings", order = 10)]
    public class MapSettingsData : ScriptableObject
    {
        [Header("Zoom")]
        public float zoom_max = 4f;

        [Header("Icons")]
        public int icon_size = 32;
        public int icon_refresh_count = 100;

        [Header("Fog")]
        public bool fog = true;
        public float fog_reveal_radius = 10f;
        public int fog_precision = 100;
        public float fog_update_rate = 0.5f;
    }

}
