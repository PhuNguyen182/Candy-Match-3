using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Service;

namespace GlobalScripts.App
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
