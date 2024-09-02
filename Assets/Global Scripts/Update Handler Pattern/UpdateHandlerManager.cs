using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalScripts.UpdateHandlerPattern
{
    public class UpdateHandlerManager : Singleton<UpdateHandlerManager>
    {
        private HashSet<IUpdateHandler> _updateHandlers = new();
        private HashSet<IFixedUpdateHandler> _fixedUpdateHandlers = new();
        private HashSet<ILateUpdateHandler> _lateUpdateHandlers = new();

        private void Update()
        {
            if (_updateHandlers.Count <= 0)
                return;

            foreach (IUpdateHandler updateHandler in _updateHandlers)
            {
                if(updateHandler.IsActive)
                    updateHandler.OnUpdate(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_fixedUpdateHandlers.Count <= 0)
                return;

            foreach (IFixedUpdateHandler fixedUpdateHandler in _fixedUpdateHandlers)
            {
                if (fixedUpdateHandler.IsActive)
                    fixedUpdateHandler.OnFixedUpdate();
            }
        }

        public void AddUpdateBehaviour(IUpdateHandler handler)
        {
            _updateHandlers.Add(handler);
        }

        public void AddFixedUpdateBehaviour(IFixedUpdateHandler handler)
        {
            _fixedUpdateHandlers.Add(handler);
        }

        public void AddLateUpdateBehaviour(ILateUpdateHandler handler)
        {
            _lateUpdateHandlers.Add(handler);
        }

        public void RemoveUpdateBehaviour(IUpdateHandler handler)
        {
            _updateHandlers.Remove(handler);
        }

        public void RemoveFixedUpdateBehaviour(IFixedUpdateHandler handler)
        {
            _fixedUpdateHandlers.Remove(handler);
        }

        public void RemoveLateUpdateBehaviour(ILateUpdateHandler handler)
        {
            _lateUpdateHandlers.Remove(handler);
        }

        private void OnDestroy()
        {
            _updateHandlers.Clear();
            _fixedUpdateHandlers.Clear();
            _lateUpdateHandlers.Clear();
        }
    }
}
