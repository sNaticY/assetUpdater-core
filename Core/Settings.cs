using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Meow.AssetUpdater.Core
{
    public class Settings : ScriptableObject
    {
        private const string SettingsAssetName = "AssetUpdaterSettings";
        private const string SettingsAssetPath = "Assets/AssetUpdater/Resources";
        private const string SettingsAssetExtension = ".asset";

        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = Resources.Load(SettingsAssetName) as Settings;
                    if (ReferenceEquals(instance, null))
                    {
                        // If not found, autocreate the asset object.
                        instance = CreateInstance<Settings>();
#if UNITY_EDITOR
                        UnityEditor.AssetDatabase.CreateAsset(instance,
                            Path.Combine(SettingsAssetPath, SettingsAssetName) + SettingsAssetExtension);
#endif
                    }
                }
                return instance;
            }
        }

        [SerializeField] private string _remoteUrl = "http://localhost/";

        [SerializeField] private string _versionFileName = "version_file.bytes";

        [SerializeField] private string _relativePath = "MainUpdater";
        
        [SerializeField] private BuildPlatform _currentPlatform = BuildPlatform.OSX;
        
        [SerializeField] private List<string> _loaclBundles = new List<string>();

        public static string RemoteUrl
        {
            get { return Instance._remoteUrl; }
            set { Instance._remoteUrl = value; }
        }

        public static string VersionFileName
        {
            get { return Instance._versionFileName; }
            set { Instance._versionFileName = value; }
        }

        public static string RelativePath
        {
            get { return Instance._relativePath; }
            set { Instance._relativePath = value; }
        }

        public static List<string> LoaclBundles
        {
            get { return instance._loaclBundles; }
        }

        public static BuildPlatform Platform
        {
            get { return Instance._currentPlatform; }
            set { Instance._currentPlatform = value; }
        }

        public static void SetBundleToLocal(string bundleName)
        {
            if (!Instance._loaclBundles.Contains(bundleName))
            {
                Instance._loaclBundles.Add(bundleName);
            }
        }

        public static void SetBundleToRemote(string bundleName)
        {
            if (Instance._loaclBundles.Contains(bundleName))
            {
                Instance._loaclBundles.Remove(bundleName);
            }
        }
    }
}