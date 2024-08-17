using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using CandyMatch3.Scripts.Gameplay.GridCells;

namespace CandyMatch3.Scripts.Gameplay.GameInput
{
    public class BoardInput : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask gridCellLayer;

        private PlayerInput _playerInput;

        public Vector3 WorldMoudePosition { get; private set; }
        public Vector3 ScreenMousePosition { get; private set; }

        public bool IsDragging { get; private set; }
        public bool IsPressed => IsPointerDown();
        public bool IsReleased => IsPointerUp();

        private void Awake()
        {
            _playerInput = new();

            _playerInput.Player.Position.started += InputPositionHandle;
            _playerInput.Player.Position.performed += InputPositionHandle;
            _playerInput.Player.Position.canceled += InputPositionHandle;

            _playerInput.Player.Press.started += OnPressHandle;
            _playerInput.Player.Press.performed += OnPressHandle;
            _playerInput.Player.Press.canceled += OnPressHandle;
        }

        private void OnEnable()
        {
            _playerInput.Enable();
            EnhancedTouchSupport.Enable();
        }

        private bool IsPointerDown()
        {
            return _playerInput.Player.Press.WasPressedThisFrame();
        }

        private bool IsPointerUp()
        {
            return _playerInput.Player.Press.WasReleasedThisFrame();
        }

        private void OnPressHandle(InputAction.CallbackContext context)
        {
            IsDragging = context.ReadValueAsButton();
        }

        private void InputPositionHandle(InputAction.CallbackContext context)
        {
            ScreenMousePosition = context.ReadValue<Vector2>();
            Vector3 worldMouse = mainCamera.ScreenToWorldPoint(ScreenMousePosition);
            worldMouse.z = 0;

            WorldMoudePosition = worldMouse;
        }

        public GridCellView GetGridCellView()
        {
            Collider2D gridCollider = Physics2D.OverlapPoint(WorldMoudePosition, gridCellLayer);

            if (gridCollider != null && gridCollider.TryGetComponent<GridCellView>(out var gridCellView))
            {
                return gridCellView;
            }

            return null;
        }

        public bool IsUIOverlayed()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            _playerInput.Disable();
        }

        private void OnDestroy()
        {
            _playerInput.Dispose();
        }
    }
}
