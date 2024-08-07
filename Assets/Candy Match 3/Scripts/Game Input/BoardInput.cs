using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CandyMatch3.Scripts.Gameplay.GameInput
{
    public class BoardInput : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask gridCellLayer;

        private PlayerInput _playerInput;

        public LayerMask GridCellLayer => gridCellLayer;
        public Vector3 WorldMoudePosition { get; private set; }
        public Vector3 ScreenMousePosition { get; private set; }
        public bool IsPress => _playerInput.Player.Tap.WasPressedThisFrame();

        private void Awake()
        {
            _playerInput = new();
            _playerInput.Player.Position.started += InputPositionHandle;
            _playerInput.Player.Position.performed += InputPositionHandle;
            _playerInput.Player.Position.canceled += InputPositionHandle;
        }

        private void OnEnable()
        {
            _playerInput.Enable();
        }

        private void InputPositionHandle(InputAction.CallbackContext context)
        {
            ScreenMousePosition = context.ReadValue<Vector2>();
            Vector3 worldMouse = mainCamera.ScreenToWorldPoint(ScreenMousePosition);
            worldMouse.z = 0;

            WorldMoudePosition = worldMouse;
        }

        private void OnDisable()
        {
            _playerInput.Disable();
        }

        private void OnDestroy()
        {
            _playerInput.Dispose();
        }
    }
}
