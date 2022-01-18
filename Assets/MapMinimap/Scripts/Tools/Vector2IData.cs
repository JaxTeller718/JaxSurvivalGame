using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{

    [System.Serializable]
    public struct Vector2IData
    {
        public int x;
        public int y;

        public Vector2IData(int iX, int iY)
        {
            x = iX;
            y = iY;
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", x, y);
        }

        //Convert to real vector
        public static implicit operator Vector2Int(Vector2IData rValue)
        {
            return new Vector2Int(rValue.x, rValue.y);
        }

        public static implicit operator Vector2IData(Vector2Int rValue)
        {
            return new Vector2IData(rValue.x, rValue.y);
        }
    }

}
