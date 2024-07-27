using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public class ItemGraphics : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer itemRenderer;
        [SerializeField] private Animator itemAnimator;

        public void SetSprite(Sprite sprite)
        {
            itemRenderer.sprite = sprite;
        }
    }
}
