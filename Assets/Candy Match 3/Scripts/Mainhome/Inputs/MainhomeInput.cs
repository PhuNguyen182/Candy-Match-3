using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CandyMatch3.Scripts.Mainhome.Inputs
{
    public class MainhomeInput : MonoBehaviour
    {
        [SerializeField] private Camera inputObserverCamera;

        private Vector2 _delta, _deltaTemp;
        private Vector2 _pointerPosition;
        private PlayerInput _inputActions;

        #region Cached UI Overlap Checking Variables
        private List<RaycastResult> _results = new();
        private PointerEventData _eventDataCurrentPosition;
        #endregion

        public bool IsActived { get; set; }
        public bool IsDragging { get; private set; }
        public Vector2 PointerPosition { get; private set; }
        public Vector2 Delta => _deltaTemp;

        private void Awake()
        {
            _inputActions = new();

            _inputActions.Player.Position.started += InputPositionHandle;
            _inputActions.Player.Position.performed += InputPositionHandle;
            _inputActions.Player.Position.canceled += InputPositionHandle;

            _inputActions.Player.Press.started += PressingHandle;
            _inputActions.Player.Press.performed += PressingHandle;
            _inputActions.Player.Press.canceled += PressingHandle;

            _inputActions.Player.Delta.started += InputMovementDeltaHandle;
            _inputActions.Player.Delta.performed += InputMovementDeltaHandle;
            _inputActions.Player.Delta.canceled += InputMovementDeltaHandle;
        }

        private void InputMovementDeltaHandle(InputAction.CallbackContext context)
        {
            if (IsActived)
            {
                _delta = context.ReadValue<Vector2>();
                _deltaTemp.x = _delta.x / Screen.width;
                _deltaTemp.y = _delta.y / Screen.height;
            }

            else
                _deltaTemp = Vector2.zero;
        }

        private void PressingHandle(InputAction.CallbackContext context)
        {
            IsDragging = IsActived ? context.ReadValueAsButton() : false;
        }

        private void InputPositionHandle(InputAction.CallbackContext context)
        {
            if (IsActived)
            {
                _pointerPosition = context.ReadValue<Vector2>();

                if (inputObserverCamera != null)
                    PointerPosition = inputObserverCamera.ScreenToWorldPoint(_pointerPosition);
            }

            else PointerPosition = Vector2.zero;
        }

        public bool IsPointerOverlapUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            return EventSystem.current == null ? false : EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
            return IsPointerOverUIObject();
#endif
        }

        public bool IsPointerOverUIObject()
        {
            if (EventSystem.current == null)
                return false;

            _results.Clear();
            _eventDataCurrentPosition = new(EventSystem.current);
            _eventDataCurrentPosition.position = new(PointerPosition.x, PointerPosition.y);
            EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
            return _results.Count > 0;
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }
    }
}
