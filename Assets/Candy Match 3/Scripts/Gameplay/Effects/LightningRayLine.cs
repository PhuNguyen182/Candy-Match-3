using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.UpdateHandlerPattern;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    [RequireComponent(typeof(LineRenderer))]
    public class LightningRayLine : MonoBehaviour, IUpdateHandler
    {
        [SerializeField] private int segmentCount = 5;
        [SerializeField] private LineRenderer rayline;

        [Header("Colors")]
        [SerializeField] private Vector4 blueColor;
        [SerializeField] private Vector4 greenColor;
        [SerializeField] private Vector4 orangeColor;
        [SerializeField] private Vector4 purpleColor;
        [SerializeField] private Vector4 redColor;
        [SerializeField] private Vector4 yellowColor;
        [SerializeField] private Vector4 specialColor;

        [Header("Animation Modifier")]
        [SerializeField] private float ampllitude = 0.5f;
        [SerializeField] private float originalPhase = 45;
        [SerializeField] private float angularFrequency = 90;
        [SerializeField] private float rotateSpeed = 1;

        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public bool IsActive { get; set; }

        private float _phi = 0;
        private float _additionPhi = 0;
        private float _omega = 0;
        private float _rotateAngle = 0;

        private Vector3 _direction;
        private Vector3 _segmentLength;
        private Vector3 _segmentPosition;
        private Vector3 _varianceVector;
        private Vector3[] _positions;

        private MaterialPropertyBlock _propertyBlock;
        private readonly int _colorProperty = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _propertyBlock = new();
            _positions = new Vector3[segmentCount + 2];
            AlignOnStart();

            _phi = originalPhase * Mathf.Deg2Rad;
            _omega = angularFrequency * Mathf.Deg2Rad;

            UpdateHandlerManager.Instance.AddUpdateBehaviour(this);
        }

        private void OnEnable()
        {
            IsActive = true;
        }

        public void OnUpdate(float deltaTime)
        {
            UpdateLine();
        }

        public void SetColor(CandyColor candyColor, bool isSpecial)
        {
            Vector4 colorVector;

            if (!isSpecial)
            {
                colorVector = candyColor switch
                {
                    CandyColor.Blue => blueColor,
                    CandyColor.Green => greenColor,
                    CandyColor.Orange => orangeColor,
                    CandyColor.Purple => purpleColor,
                    CandyColor.Red => redColor,
                    CandyColor.Yellow => yellowColor,
                    _ => new Vector4()
                };
            }

            else
                colorVector = specialColor;

            Color color = new(colorVector.x, colorVector.y, colorVector.z, colorVector.w);

            rayline.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(_colorProperty, color);
            rayline.SetPropertyBlock(_propertyBlock);
        }

        public void SetPhaseStep(int phaseStep)
        {
            _additionPhi = originalPhase * phaseStep * Mathf.Deg2Rad;
        }

        private void AlignOnStart()
        {
            _direction = EndPosition - StartPosition;
            _segmentLength = _direction / (segmentCount + 1);
            _positions[0] = StartPosition;

            for (int i = 0; i < segmentCount; i++)
            {
                _segmentPosition = StartPosition + _segmentLength * (i + 1);
                _positions[i] = _segmentPosition;
            }

            _positions[_positions.Length - 1] = EndPosition;
        }

        private void UpdateLine()
        {
            _direction = EndPosition - StartPosition;
            _segmentLength = _direction / (segmentCount + 1);

            _positions[0] = StartPosition;
            _positions[_positions.Length - 1] = EndPosition;

            for (int i = 1; i <= segmentCount; i++)
            {
                _segmentPosition = StartPosition + _segmentLength * i;
                _varianceVector = Vector3.up * ampllitude * Mathf.Sin(_omega * Time.time * rotateSpeed + (i * _phi + _additionPhi));

                _rotateAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                _varianceVector = Quaternion.Euler(0, 0, _rotateAngle) * _varianceVector;

                _positions[i] = _varianceVector + _segmentPosition;
            }

            rayline.positionCount = _positions.Length;
            rayline.SetPositions(_positions);
        }

        private void Clear()
        {
            rayline.positionCount = 0;
            Array.Clear(_positions, 0, _positions.Length);
            _propertyBlock.Clear();
        }

        private void OnDisable()
        {
            IsActive = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            rayline ??= GetComponent<LineRenderer>();
        }
#endif

        private void OnDestroy()
        {
            Clear();
            UpdateHandlerManager.Instance.RemoveUpdateBehaviour(this);
        }
    }
}
