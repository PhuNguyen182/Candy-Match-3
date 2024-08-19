using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public abstract class BaseItem : BaseItemEntity, IBlockItem
    {
        [SerializeField] protected int itemId = 0;
        [SerializeField] protected ItemType itemType = ItemType.None;
        [SerializeField] protected CandyColor candyColor = CandyColor.None;
        [SerializeField] protected TargetEnum targetType = TargetEnum.None;
        [SerializeField] protected ItemGraphics itemGraphics;

        protected CancellationToken destroyToken;

        public int ItemID => itemId;

        public abstract bool CanBeReplace { get; }

        public abstract bool IsMatchable { get; }

        public abstract bool IsMoveable { get; }

        public ItemType ItemType => itemType;

        public TargetEnum TargetType => targetType;

        public CandyColor CandyColor => candyColor;

        public Vector3 WorldPosition => transform.position;

        public Vector3Int GridPosition { get; set; }

        private void Awake()
        {
            destroyToken = this.GetCancellationTokenOnDestroy();

            OnAwake();
        }

        private void OnEnable()
        {
            OnAppear();
        }

        private void Start()
        {
            OnStart();
            InitOriginalMessages();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnAppear() { }

        protected virtual void OnStart() { }

        public virtual void OnDisappear() { }

        public virtual void OnRelease() { }

        public virtual void SetMatchable(bool isMatchable) { }

        protected void InitOriginalMessages() { }

        public virtual void InitMessages() { }

        public virtual void ReleaseItem() { }

        public virtual void ResetItem()
        {
            InitMessages();
        }

        public virtual UniTask ItemBlast()
        {
            return UniTask.CompletedTask;
        }

        public string GetName()
        {
            return $"Name: {gameObject.name}, position: {WorldPosition}, is active: {isActiveAndEnabled}, type: {itemType}";
        }

        public void SetItemType(ItemType itemType)
        {
            this.itemType = itemType;
        }

        public void SetWorldPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetItemID(int itemId)
        {
            this.itemId = itemId;
        }

        private void OnDisable()
        {
            OnDisappear();
        }

        private void OnDestroy()
        {
            OnRelease();
        }
    }
}
