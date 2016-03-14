using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Controls
    {
        public bool Left, Right;
        public Vector2 WorldMouseCoord;

        public static Controls Poll()
        {
            Controls c = new Controls
            {
                Left = Input.GetKey(KeyCode.A),
                Right = Input.GetKey(KeyCode.D)
            };


            if (Camera.main != null)
            {
                c.WorldMouseCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                c.WorldMouseCoord = Vector2.up;
            }


            return c;
        }

        public override string ToString()
        {
            return string.Format("[Controls] Left: {0}, Right: {1}, Mouse: {2}", Left, Right, WorldMouseCoord);
        }
    }
}
