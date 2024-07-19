using GlobalScripts.Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalScripts
{
    public class AppInitializer : Singleton<AppInitializer>, IService
    {
        protected override void OnAwake()
        {
            Initialize();
        }

        public void Initialize()
        {
            InitializeService.Instance.Initialize();
        }
    }
}
