using System.Collections;
using UnityEngine;
using System;
using System.IO;

namespace Meow.AssetUpdater.Core
{
    public enum SourceType
    {
        RemotePath,
        PersistentPath,
        StreamingPath
    }

    [Serializable]
    public enum BuildPlatform
    {
        Windows,
        OSX,
        Andriod,
        iOS,
        OtherPlatform,
    }

    public class Utils
    {
        #region --SingletonImplement--

        private static Utils _instance = null;

        public static Utils Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new Utils();
                }
                return _instance;
            }
        }

        #endregion

        public void WriteBytesTo(SourceType source, string relativePath, byte[] bytes)
        {
            switch (source)
            {
                case SourceType.RemotePath:
                case SourceType.StreamingPath:
                    Debug.LogErrorFormat("The Path {0} can't be written", source);
                    break;
                case SourceType.PersistentPath:
                    var fullPath = Path.Combine(Application.persistentDataPath, relativePath);
                    var directoryPath = fullPath.Substring(0, fullPath.LastIndexOf(Path.DirectorySeparatorChar));
                    var fileName = fullPath.Substring(fullPath.LastIndexOf(Path.DirectorySeparatorChar) + 1,
                        fullPath.Length - fullPath.LastIndexOf(Path.DirectorySeparatorChar) - 1);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    File.WriteAllBytes(Path.Combine(directoryPath, fileName), bytes);
                    break;
            }
        }

#if UNITY_EDITOR
        public void WriteBytesTo(string fullPath, byte[] bytes)
        {
            var directoryPath = fullPath.Substring(0, fullPath.LastIndexOf(Path.DirectorySeparatorChar));
            var fileName = fullPath.Substring(fullPath.LastIndexOf(Path.DirectorySeparatorChar) + 1,
                fullPath.Length - fullPath.LastIndexOf(Path.DirectorySeparatorChar) - 1);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllBytes(Path.Combine(directoryPath, fileName), bytes);
        }
#endif

        public static BuildPlatform GetBuildPlatform(RuntimePlatform platform)
        {
            BuildPlatform buildPlatform;
            switch (platform)
            {
                case RuntimePlatform.Android:
                    buildPlatform = BuildPlatform.Andriod;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    buildPlatform = BuildPlatform.iOS;
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    buildPlatform = BuildPlatform.OSX;
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    buildPlatform = BuildPlatform.Windows;
                    break;
                default:
                    Debug.LogErrorFormat("You are Running in the platform [{0}] which not support", Application.platform);
                    buildPlatform = BuildPlatform.OtherPlatform;
                    break;
            }
            return buildPlatform;
        }

        public static BuildPlatform GetBuildPlatform()
        {
            return GetBuildPlatform(Application.platform);
        }
        
#if UNITY_EDITOR
        public static UnityEditor.BuildTarget GetBuildTarget(BuildPlatform platform)
        {
            UnityEditor.BuildTarget target = UnityEditor.BuildTarget.NoTarget;
            switch (platform)
            {
                case BuildPlatform.Andriod:
                    target = UnityEditor.BuildTarget.Android;
                    break;
                case BuildPlatform.iOS:
                    target = UnityEditor.BuildTarget.iOS;
                    break;
                case BuildPlatform.OSX:
                    target = UnityEditor.BuildTarget.StandaloneOSXUniversal;
                    break;
                case BuildPlatform.Windows:
                    target = UnityEditor.BuildTarget.StandaloneWindows;
                    break;
                case BuildPlatform.OtherPlatform:
                    target = UnityEditor.BuildTarget.NoTarget;
                    break;
            }
            return target;
        }
#endif

        public static string GetWWWStreamingAssetPath(string relativePath)
        {
            string path;

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = Application.streamingAssetsPath;
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    path = "file://" + Application.streamingAssetsPath;
                    break;

                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    path = "file://" + Application.dataPath + "/StreamingAssets";
                    break;

                default:
                    path = "file://" + Application.streamingAssetsPath;
                    break;
            }

            return Path.Combine(path, relativePath);
        }

        public static string GetWWWPersistentPath(string relativePath)
        {
            string path = "file://" + Application.persistentDataPath;
            return Path.Combine(path, relativePath);
        }
    }
}