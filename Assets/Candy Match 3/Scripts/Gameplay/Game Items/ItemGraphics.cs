using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public class ItemGraphics : MonoBehaviour
    {
        [SerializeField] private Animator itemAnimator;
        
        [Header("Item Sprites")]
        [SerializeField] private SpriteRenderer bottomState;
        [SerializeField] private SpriteRenderer itemRenderer;
        [SerializeField] private SpriteRenderer topState;

        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _propertyBlock = new();
        }

        #region Modify Material Property
        public void SetIntegerRendererProperty(int propertyId, int value)
        {
            _propertyBlock.SetInteger(propertyId, value);
            itemRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetFloatRendererProperty(int propertyId, float value)
        {
            _propertyBlock.SetFloat(propertyId, value);
            itemRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetTextureRendererProperty(int propertyId, Texture2D value)
        {
            _propertyBlock.SetTexture(propertyId, value);
            itemRenderer.SetPropertyBlock(_propertyBlock);
        }
        #endregion

        #region Modify Item Display Images
        public void SetItemSprite(Sprite sprite)
        {
            itemRenderer.sprite = sprite;
        }

        public void SetBottomStateSprite(Sprite sprite)
        {
            bottomState.sprite = sprite;
        }

        public void SetTopStateSprite(Sprite sprite)
        {
            topState.sprite = sprite;
        }
        #endregion

        private void OnDestroy()
        {
            _propertyBlock.Clear();
        }
    }
}
