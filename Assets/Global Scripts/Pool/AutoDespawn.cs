using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.UpdateHandlerPattern;

namespace GlobalScripts.Pool
{
    public class AutoDespawn : MonoBehaviour, IUpdateHandler
    {
        [SerializeField] private float duration = 1;

        private float _timer = 0;

        public bool IsActive { get; set; }

        private void Awake()
        {
            UpdateHandlerManager.Instance.AddUpdateBehaviour(this);
        }

        private void OnEnable()
        {
            _timer = 0;
            IsActive = true;
        }

        public void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= duration)
            {
                _timer = 0;
                SimplePool.Despawn(this.gameObject);
            }
        }

        public void SetDuration(float duration)
        {
            this.duration = duration;
        }

        private void OnDisable()
        {
            IsActive = false;
        }

        private void OnDestroy()
        {
            UpdateHandlerManager.Instance?.RemoveUpdateBehaviour(this);
        }
    }
}
