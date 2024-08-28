using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalScripts.Utils
{
    public static class VersionCheckUtils
    {
        private const string DefaultVersion = "0.0.0";
        private const string VersionKey = "AppVersionKey";

        public static bool CheckNewLocalVersion()
        {
            string currentVersion = Application.version;
            string savedVersion = PlayerPrefs.GetString(VersionKey, DefaultVersion);

            bool isNewVersion = string.CompareOrdinal(currentVersion, savedVersion) != 0;
            return isNewVersion;
        }

        public static void SaveCurrentLocalVersion()
        {
            string currentVersion = Application.version;
            PlayerPrefs.SetString(VersionKey, currentVersion);
        }
    }
}
