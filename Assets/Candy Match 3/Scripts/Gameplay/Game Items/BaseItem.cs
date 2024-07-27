using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public abstract class BaseItem : BaseItemEntity
    {
        [SerializeField] protected int itemId;
        [SerializeField] protected ItemType itemType;
        [SerializeField] protected ItemGraphics itemGraphics;

        protected CancellationToken destroyToken;

        private void Awake()
        {
            destroyToken = this.GetCancellationTokenOnDestroy();

            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnStart() { }

        protected void ResetItem()
        {

        }

        private void OnDestroy()
        {
            
        }
    }
}
