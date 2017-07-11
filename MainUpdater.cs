using System;
using System.Collections;
using System.IO;
using System.Runtime.Versioning;
using Meow.AssetUpdater.Core;
using LitJson;
using UnityEngine;

namespace Meow.AssetUpdater
{
    public class MainUpdater : MonoBehaviour
    {
        #region MonoSingletonImplement

        private static MainUpdater _instance;

        public static MainUpdater Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = GameObject.Find("MainUpdater");
                    if (go == null)
                    {
                        go = new GameObject("MainUpdater");
                    }
                    _instance = go.GetComponent<MainUpdater>();
                    if (_instance == null)
                    {
                        _instance = go.AddComponent<MainUpdater>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        private static VersionInfo _remoteVersionInfo;
        private static VersionInfo _persistentVersionInfo;
        private static VersionInfo _streamingVersionInfo;
        
#if UNITY_EDITOR
        private static int _isSimulationMode = -1;
        
        public static bool IsSimulationMode
        {
            get
            {
                if (_isSimulationMode == -1)
                    _isSimulationMode = UnityEditor.EditorPrefs.GetBool("AssetUpdaterSimulationMode", true) ? 1 : 0;

                return _isSimulationMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != _isSimulationMode)
                {
                    _isSimulationMode = newValue;
                    UnityEditor.EditorPrefs.SetBool("AssetUpdaterSimulationMode", value);
                }
            }
        }
#endif
        
        /// <summary>
        /// Initialize global settings of AssetUpdater
        /// </summary>
        /// <param name="remoteUrl">the assetbundle server url you want to download from</param>
        /// <param name="relativePath">the relative directory of your assetbundle root path</param>
        /// <param name="versionFileName">name of version file, default by "versionfile.bytes"</param>
        public static void Initialize(string remoteUrl, string relativePath, string versionFileName)
        {
            Settings.RemoteUrl = remoteUrl;
            Settings.RelativePath = relativePath;
            Settings.VersionFileName = versionFileName;
        }
        
        /// <summary>
        /// download all version files which in remote url, persistentDataPath and streamingAssetPath. perpare for downloading
        /// </summary>
        /// <returns></returns>
        public static IEnumerator LoadAllVersionFiles()
        {
#if UNITY_EDITOR
            if (!IsSimulationMode)
#endif
            {
                var vFRoot = Path.Combine(Settings.RelativePath, Utils.GetBuildPlatform(Application.platform).ToString());
                var vFPath = Path.Combine(vFRoot, Settings.VersionFileName);

                var op = new DownloadOperation(SourceType.RemotePath, vFPath);
                yield return op;
                _remoteVersionInfo = JsonMapper.ToObject<VersionInfo>(op.Text);
                if (_remoteVersionInfo == null)
                {
                    Debug.LogError("Can not download remote version file");
                }

                op = new DownloadOperation(SourceType.PersistentPath, vFPath);
                yield return op;
                _persistentVersionInfo = JsonMapper.ToObject<VersionInfo>(op.Text);
                if (_persistentVersionInfo == null)
                {
                    _persistentVersionInfo = new VersionInfo();
                }

                op = new DownloadOperation(SourceType.StreamingPath, vFPath);
                yield return op;
                _streamingVersionInfo = JsonMapper.ToObject<VersionInfo>(op.Text);
                if (_streamingVersionInfo == null)
                {
                    _streamingVersionInfo = new VersionInfo();
                }
            }
        }
        
        /// <summary>
        /// download files from streamingAssetPath to persistentDataPath, then update versionfile in persistentDataPath
        /// </summary>
        /// <returns>updateOperation object contains current downloading information</returns>
        public static UpdateOperation UpdateFromStreamingAsset()
        {
            return new UpdateOperation(_persistentVersionInfo, _streamingVersionInfo, SourceType.StreamingPath);
        }

        public static UpdateOperation UpdateFromRemoteAsset()
        {
            return new UpdateOperation(_persistentVersionInfo, _remoteVersionInfo, SourceType.RemotePath);
        }
        
        /// <summary>
        /// get assetbundle name by input asset path
        /// </summary>
        /// <param name="path">asset path</param>
        /// <returns>assetbundle name</returns>
        public static string GetAssetbundleNameByAssetPath(string path)
        {
            if (_persistentVersionInfo.BundlePath.ContainsKey(path))
            {
                return _persistentVersionInfo.BundlePath[path];
            }
            else
            {
                Debug.LogError("Given asset path is not exist in any downloaded assetbundles");
                return "";
            }
        }

        public static string GetAssetbundleRootPath(bool forWWW)
        {
            var path = Path.Combine(Path.Combine(Application.persistentDataPath, Settings.RelativePath), Utils.GetBuildPlatform().ToString());
            
            if (forWWW)
            {
                path = Path.Combine("file://", path);
            }
            return path;
        }

        public static string GetManifestName()
        {
            return Utils.GetBuildPlatform().ToString();
        }

    }
}