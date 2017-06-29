using System;
using System.Collections;
using System.IO;
using System.Runtime.Versioning;
using LitJson;
using UnityEngine;

namespace Meow.AssetUpdater.Core
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

        public static void Initialize(string remoteUrl, string relativePath, string versionFileName)
        {
            Settings.RemoteUrl = remoteUrl;
            Settings.RelativePath = relativePath;
            Settings.VersionFileName = versionFileName;
        }

        public static IEnumerator LoadAllVersionFiles()
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
        
        public static UpdateOperation UpdateFromStreamingAsset()
        {
            return new UpdateOperation(_persistentVersionInfo, _streamingVersionInfo, SourceType.StreamingPath);
        }

        public static UpdateOperation UpdateFromRemoteAsset()
        {
            return new UpdateOperation(_persistentVersionInfo, _remoteVersionInfo, SourceType.RemotePath);
        }

    }
}