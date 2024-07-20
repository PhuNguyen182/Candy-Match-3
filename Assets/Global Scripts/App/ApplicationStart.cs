using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.UpdateHandlerPattern;
using GlobalScripts.Audios;

namespace GlobalScripts.App
{
    public static class ApplicationStart
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            RegisterServicesBeforeSceneLoad();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            RegisterServicesAfterSceneLoad();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashScene()
        {
            RegisterServicesBeforeSplashScene();
        }

        private static void RegisterServicesBeforeSceneLoad() { }

        private static void RegisterServicesBeforeSplashScene() { }

        private static void RegisterServicesAfterSceneLoad()
        {
            Register<AppInitializer>("App/App Initializer");
            Register<UpdateHandlerManager>("Handlers/Update Behaviour Handler");
            Register<MusicManager>("Managers/Music Manager");
        }

        private static T Register<T>(string serviceName) where T : Component
        {
            T service = Resources.Load<T>(serviceName);
            T instance = Object.Instantiate(service);
            Object.DontDestroyOnLoad(instance);
            return service;
        }
    }
}