using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITASK_ADDRESSABLE_SUPPORT
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Cysharp.Threading.Tasks;
#endif

namespace GlobalScripts.Utils
{
    public static class AddressablesUtils
    {
#if UNITASK_ADDRESSABLE_SUPPORT
        public static async UniTask LoadSceneViaAddressable(string key, LoadSceneMode loadMode = LoadSceneMode.Single
            , bool activateOnLoad = true, int priority = 100, CancellationToken cancellationToken = default)
        {
            bool isKeyValid = await IsKeyValid(key);

            if (isKeyValid)
                await Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority).WithCancellation(cancellationToken);
        }

        public static async UniTask<bool> DownloadContent(string key, bool autoRelease = true, CancellationToken cancellationToken = default)
        {
            bool isKeyValid = await IsKeyValid(key);
            
            if(isKeyValid)
            {
                bool isDownloadable = await IsDownloadable(key);

                if (isDownloadable)
                {
                    await Addressables.DownloadDependenciesAsync(key, autoRelease)
                                      .WithCancellation(cancellationToken);
                    return true;
                }
            }

            return false;
        }

        public static async UniTask<bool> DownloadContent(List<string> key, bool autoRelease = true, CancellationToken cancellationToken = default)
        {
            bool isKeyValid = await IsKeyValid(key);

            if (isKeyValid)
            {
                bool isDownloadable = await IsDownloadable(key);

                if (isDownloadable)
                {
                    await Addressables.DownloadDependenciesAsync(key, autoRelease)
                                      .WithCancellation(cancellationToken);
                    return true;
                }
            }

            return false;
        }

        public static async UniTask<bool> DeleteContent(string key, bool autoRelease = true)
        {
            bool isKeyValid = await IsKeyValid(key);

            if (isKeyValid)
            {
                await Addressables.ClearDependencyCacheAsync(key, autoRelease);
                return true;
            }

            return false;
        }

        public static async UniTask<bool> DeleteContent(List<string> key)
        {
            bool isKeyValid = await IsKeyValid(key);

            if (isKeyValid)
            {
                await Addressables.CleanBundleCache(key);
                return true;
            }

            return false;
        }

        public static async UniTask<bool> IsDownloadable(string key)
        {
            long size = await Addressables.GetDownloadSizeAsync(key);
            return size > 0;
        }

        public static async UniTask<bool> IsDownloadable(List<string> key)
        {
            long size = await Addressables.GetDownloadSizeAsync(key);
            return size > 0;
        }

        public static async UniTask<bool> IsKeyValid(string key)
        {
            IList<IResourceLocation> locations = await Addressables.LoadResourceLocationsAsync(key);
            return locations.Count > 0;
        }

        public static async UniTask<bool> IsKeyValid(List<string> key)
        {
            IList<IResourceLocation> locations = await Addressables.LoadResourceLocationsAsync(key);
            return locations.Count > 0;
        }
#endif
    }
}
