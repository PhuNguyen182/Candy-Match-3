using System;
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
        protected IDisposable messageDisposable;

        public int ItemID => itemId;
        public bool IsLocking { get; set; }

        public abstract bool Replacable { get; }

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
            InitCommonMessages();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnAppear() { }

        protected virtual void OnStart() { }

        public virtual void OnDisappear() { }

        public virtual void OnRelease() { }

        public virtual void SetMatchable(bool isMatchable) { }

        protected void InitCommonMessages() { }

        public virtual void InitMessages() { }

        public virtual void ReleaseItem() { }

        public virtual void ResetItem()
        {
            InitMessages();
            IsLocking = false;
        }

        public virtual UniTask ItemBlast()
        {
            return UniTask.CompletedTask;
        }

        public string GetName()
        {
            string colorCode = GetColor();
            string activeCode = isActiveAndEnabled ? "#ffffff" : "#000000";
            return $"Name: <b><color=#ffffff>{gameObject.name}</color></b>" +
                $", --- Grid Position: <color=#ffffff>{GridPosition}</color>" +
                $", --- World Position: <color=#ffffff>{WorldPosition}</color>" +
                $". --- Is Active: <b><color={activeCode}>{isActiveAndEnabled}</color></b>" +
                $", --- Type: <b><color={colorCode}>{itemType}</color></b>";
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

        private string GetColor()
        {
            string colorCode = "";

            colorCode = candyColor switch
            {
                CandyColor.Red => "#F73540",
                CandyColor.Green => "#47D112",
                CandyColor.Orange => "#FA8500",
                CandyColor.Purple => "#CE2AEF",
                CandyColor.Yellow => "#F8D42A",
                CandyColor.Blue => "#23AAFB",
                _ => "#000000"
            };

            return colorCode;
        }

        private void OnDisable()
        {
            OnDisappear();
        }

        private void OnDestroy()
        {
            OnRelease();
            messageDisposable?.Dispose();
        }
    }
}
