using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{

    public class MapTool
    {
        //Return the closest fog waypoint that can be saved as 'revealed' in the data, in world coord
        public static Vector2Int GetClosestFogPoint(Vector3 world_pos)
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
            {
                Vector3 map_pos = settings.zone.GetNormalizedPos(world_pos);
                return GetClosestFogPointMap(map_pos);
            }
            return Vector2Int.zero;
        }

        //Return the closest fog waypoint that can be saved as 'revealed' in the data, in map coord (-1 to 1)
        public static Vector2Int GetClosestFogPointMap(Vector2 map_pos)
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
            {
                Vector3 pos = (map_pos + Vector2.one) * 0.5f;  //Convert from 0 to 1
                int x = Mathf.RoundToInt(pos.x * (settings.data.fog_precision));
                int y = Mathf.RoundToInt(pos.y * (settings.data.fog_precision));
                return new Vector2Int(x, y);
            }
            return Vector2Int.zero;
        }

        //Return map coordinates (-1 to 1) of fog points in data
        public static Vector2 GetFogMapPos(Vector2Int fog_point)
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
            {
                float x = fog_point.x / (float)(settings.data.fog_precision);
                float y = fog_point.y / (float)(settings.data.fog_precision);
                return new Vector2(x * 2f - 1f, y * 2f - 1f);
            }
            return Vector2.zero;
        }

        //Return world coordinates (scene position) of fog points in data
        public static Vector3 GetFogWorldPos(Vector2Int fog_point)
        {
            Vector2 map_pos = GetFogMapPos(fog_point);
            return MapToWorldPos(map_pos);
        }

        //Return normalized pos in -1, 1
        public static Vector2 WorldToMapPos(Vector3 world_pos)
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
            {
                Vector2 norm_pos = settings.zone.GetNormalizedPos(world_pos);
                return norm_pos;
            }
            return Vector2.zero;
        }

        //Return scene 3D position (from a map pos from -1 to 1)
        public static Vector3 MapToWorldPos(Vector2 map_pos)
        {
            MapLevelSettings settings = MapLevelSettings.Get();
            if (settings != null && settings.IsValid())
            {
                Vector3 world_pos = settings.zone.GetWorldPosition(map_pos);
                return world_pos;
            }
            return Vector3.zero;
        }

        public static string GetCurrentScene()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }

}
