using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Controls
    {
        public bool Left, Right;
        public Vector2 WorldMouseCoord;

        public Controls Poll()
        {
            Left = Input.GetKey(KeyCode.A);
            Right = Input.GetKey(KeyCode.D);
            return this;
        }

        public override string ToString()
        {
            return string.Format("[Controls] Left: {0}, Right: {1}, Mouse: {2}", Left, Right, WorldMouseCoord);
        }
    }
}
