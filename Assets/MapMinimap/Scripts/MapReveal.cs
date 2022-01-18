using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{

    /// <summary>
    /// Attach to any object to make the object reveal fog
    /// </summary>
    
    public class MapReveal : MonoBehaviour
    {

        private Transform trans;

        private static List<MapReveal> list = new List<MapReveal>();

        void Awake()
        {
            list.Add(this);
            trans = transform;
        }

        void OnDestroy()
        {
            list.Remove(this);
        }

        public Vector3 GetWorldPos()
        {
            return trans.position;
        }

        public static List<MapReveal> GetAll()
        {
            return list;
        }
    }
}
