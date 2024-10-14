using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.GameManagers;
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
            Setup();
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
            Register<AudioManager>("Managers/Music Manager");
            Register<GameManager>("Managers/Game Manager");
        }

        private static T Register<T>(string serviceName) where T : Component
        {
            T service = Resources.Load<T>(serviceName);
            T instance = Object.Instantiate(service);
            Object.DontDestroyOnLoad(instance);
            return service;
        }

        private static void Setup()
        {
#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }
    }
}
