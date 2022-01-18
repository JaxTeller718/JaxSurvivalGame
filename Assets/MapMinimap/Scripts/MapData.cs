using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{
    [System.Serializable]
    public class MapSceneData
    {
        public Dictionary<Vector2IData, float> fog_reveal = new Dictionary<Vector2IData, float>();

        public void Reveal(Vector2IData pos, float radius)
        {
            if (!fog_reveal.ContainsKey(pos) || fog_reveal[pos] < radius)
                fog_reveal[pos] = radius;
        }

        public bool IsRevealed(Vector2IData pos)
        {
            return fog_reveal.ContainsKey(pos);
        }

        public void FixData()
        {
            if (fog_reveal == null)
                fog_reveal = new Dictionary<Vector2IData, float>();
        }
    }

    /// <summary>
    /// Map data that is saved
    /// </summary>

    [System.Serializable]
    public class MapData
    {
        public const string VERSION = "1.00";

        public string filename;
        public string version;
        public DateTime last_save;

        public Dictionary<string, MapSceneData> scenes_data = new Dictionary<string, MapSceneData>();

        public static string file_loaded = "";
        public static MapData map_data = null;

        public MapData(string filename)
        {
            this.filename = filename;
            version = VERSION;
            last_save = DateTime.Now;
        }

        public void FixData()
        {
            if (scenes_data == null)
                scenes_data = new Dictionary<string, MapSceneData>();

            foreach (KeyValuePair<string, MapSceneData> pair in scenes_data)
                pair.Value.FixData();
        }

        public MapSceneData GetSceneData(string scene)
        {
            if (scenes_data.ContainsKey(scene))
            {
                return scenes_data[scene];
            }
            else
            {
                MapSceneData data = new MapSceneData();
                scenes_data[scene] = data;
                return data;
            }
        }

        // --- Save/load/new --------

        public void Save()
        {
            Save(file_loaded, this);
        }

        public static void Save(string filename, MapData data)
        {
            if (!string.IsNullOrEmpty(filename) && data != null)
            {
                data.filename = filename;
                data.last_save = DateTime.Now;
                data.version = VERSION;
                map_data = data;
                file_loaded = filename;

                SaveSystem.SaveFile<MapData>(filename, data);
                SaveSystem.SetLastSave(filename);
            }
        }

        public static void NewGame()
        {
            NewGame(GetLastSave()); //default name
        }

        //You should reload the scene right after NewGame
        public static MapData NewGame(string filename)
        {
            file_loaded = filename;
            map_data = new MapData(filename);
            map_data.FixData();
            return map_data;
        }

        public static MapData Load(string filename)
        {
            if (map_data == null || file_loaded != filename)
            {
                map_data = SaveSystem.LoadFile<MapData>(filename);
                if (map_data != null)
                {
                    file_loaded = filename;
                    map_data.FixData();
                }
            }
            return map_data;
        }

        public static MapData LoadLast()
        {
            return AutoLoad(GetLastSave());
        }

        //Load if found, otherwise new game
        public static MapData AutoLoad(string filename)
        {
            if (map_data == null)
                map_data = Load(filename);
            if (map_data == null)
                map_data = NewGame(filename);
            return map_data;
        }

        public static string GetLastSave()
        {
            string name = SaveSystem.GetLastSave();
            if (string.IsNullOrEmpty(name))
                name = "player"; //Default name
            return name;
        }

        public static void Unload()
        {
            map_data = null;
            file_loaded = "";
        }

        public static void Delete(string filename)
        {
            if (file_loaded == filename)
            {
                map_data = new MapData(filename);
                map_data.FixData();
            }

            SaveSystem.DeleteFile(filename);
        }

        public static bool IsLoaded()
        {
            return map_data != null && !string.IsNullOrEmpty(file_loaded);
        }

        public static MapData Get()
        {
            return map_data;
        }

    }

}