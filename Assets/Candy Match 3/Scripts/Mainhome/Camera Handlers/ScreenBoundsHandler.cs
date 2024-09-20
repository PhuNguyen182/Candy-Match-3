using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CandyMatch3.Scripts.Mainhome.CameraHandlers
{
    public class ScreenBoundsHandler : MonoBehaviour
    {
        [SerializeField] private Bounds bounds;
        [SerializeField] private SpriteRenderer minBackground;
        [SerializeField] private SpriteRenderer maxBackground;

        [Header("Extended")]
        [SerializeField] private Vector2 minExtend = new(0, -1.5f);
        [SerializeField] private Vector2 maxExtend = new(0, 4.5f);

        public Bounds ScreenBounds => bounds;

        [Button]
        public void CalculateBounds()
        {
            Vector2 min = minBackground.bounds.min;
            Vector2 max = maxBackground.bounds.max;

            bounds = new Bounds
            {
                min = min + minExtend,
                max = max + maxExtend
            };
        }
    }
}
