using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapMinimap
{
    /// <summary>
    /// Map zone should always be defined in the XZ plane
    /// </summary>

    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class MapZone : MonoBehaviour
    {
        private BoxCollider collide;

        private void Awake()
        {
            Renderer render = GetComponent<Renderer>();
            if (render != null)
                render.enabled = false;

            collide = GetComponent<BoxCollider>();
            collide.enabled = !Application.isPlaying;
            collide.center = Vector3.zero;
            collide.size = Vector3.one;
        }

        public void SetBounds(Bounds bound)
        {
            transform.position = bound.center;
            transform.localScale = bound.size;
        }

        public Vector3 GetCenter()
        {
            return transform.position;
        }

        public float GetRotation()
        {
            return transform.rotation.eulerAngles.y;
        }

        public Vector2 GetSize()
        {
            return new Vector2(transform.localScale.x, transform.localScale.z);
        }

        public Vector2 GetExtents()
        {
            return new Vector2(transform.localScale.x / 2f, transform.localScale.z / 2f);
        }

        //Check if point is inside the map zone
        public bool IsInside(Vector3 pos)
        {
            Vector3 lpos = transform.InverseTransformPoint(pos);
            return lpos.x < 0.5f && lpos.z < 0.5f
                && lpos.x > -0.5f && lpos.z > -0.5f;
        }

        //Return position of point in -1/1 range
        public Vector2 GetNormalizedPos(Vector3 pos)
        {
            Vector3 lpos = transform.InverseTransformPoint(pos);
            Vector2 map_pos = new Vector2(lpos.x * 2f, lpos.z * 2f);
            return map_pos;
        }

        //Return world position from the normalized map position
        public Vector3 GetWorldPosition(Vector2 map_pos)
        {
            Vector3 lpos = new Vector3(map_pos.x * 0.5f, 0f, map_pos.y * 0.5f);
            return transform.TransformPoint(lpos);
        }
    }

}
