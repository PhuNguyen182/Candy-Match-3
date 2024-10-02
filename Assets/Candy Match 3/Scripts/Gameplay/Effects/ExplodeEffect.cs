using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.UpdateHandlerPattern;
using GlobalScripts.Pool;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    [RequireComponent(typeof(AutoDespawn))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExplodeEffect : MonoBehaviour, IUpdateHandler
    {
        [SerializeField] private AutoDespawn autoDespawn;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private const float StartingTime = 0.2f;

        private int _waveId;
        private int _timerId;
        private int _magnitudeId;
        private int _distortionStrengthId;
        private int _propagationSpeedId;

        private float _timer = 0;
        private float _duration = 0;

        private MaterialPropertyBlock _propertyBlock;

        public bool IsActive { get; set; }

        private void Awake()
        {
            _waveId = Shader.PropertyToID("_Wave");
            _timerId = Shader.PropertyToID("_Timer");
            _magnitudeId = Shader.PropertyToID("_Magnitude");
            _distortionStrengthId = Shader.PropertyToID("_Distortion_Strength");
            _propagationSpeedId = Shader.PropertyToID("_Propagation_Speed");

            _propertyBlock = new MaterialPropertyBlock();
            UpdateHandlerManager.Instance.AddUpdateBehaviour(this);
        }

        private void OnEnable()
        {
            _timer = StartingTime;
            IsActive = true;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_timer < _duration)
            {
                PlayAtTime(_timer);
                _timer += Time.deltaTime;
            }
        }

        public void PlayExplodeEffect(float distortionStrength, float propagationSpeed, float wave, float magnitude)
        {
            spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_waveId, wave);
            _propertyBlock.SetFloat(_magnitudeId, magnitude);
            _propertyBlock.SetFloat(_distortionStrengthId, distortionStrength);
            _propertyBlock.SetFloat(_propagationSpeedId, propagationSpeed);
            spriteRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetDuration(float duration)
        {
            _duration = duration;
            autoDespawn.SetDuration(duration + Time.deltaTime);
        }

        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale / 1.3f;
        }

        private void PlayAtTime(float time)
        {
            spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_timerId, time);
            spriteRenderer.SetPropertyBlock(_propertyBlock);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            autoDespawn ??= GetComponent<AutoDespawn>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
        }
#endif

        private void OnDisable()
        {
            IsActive = false;
        }

        private void OnDestroy()
        {
            _propertyBlock.Clear();
            UpdateHandlerManager.Instance.RemoveUpdateBehaviour(this);
        }
    }
}
