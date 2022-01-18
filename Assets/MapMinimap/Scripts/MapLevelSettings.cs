using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{
    /// <summary>
    /// Contains map settings for a specific scene
    /// </summary>

    public class MapLevelSettings : MonoBehaviour
    {
        public Texture map;
        public MapZone zone;
        public MapSettingsData data;

        private static MapLevelSettings _instance;

        private void Awake()
        {
            _instance = this;

            if (zone == null)
                Debug.LogError("Zone is not defined in MapLevelSettings!");
            if (data == null)
                Debug.LogError("Data settings are not defined in MapLevelSettings!");
        }

        public bool IsValid()
        {
            return zone != null && data != null;
        }

        public static MapLevelSettings Get()
        {
            return _instance;
        }
    }

}
