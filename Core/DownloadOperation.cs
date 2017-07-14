using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Meow.AssetUpdater.Core
{
    public class DownloadOperation : CustomYieldInstruction
    {
        private readonly string _targetUrl;
        private WWW _www;

        public float Progress
        {
            get { return _www.progress; }
        }

        public AssetBundle AssetBundle
        {
            get { return _www.assetBundle; }
        }

        public byte[] Bytes
        {
            get { return _www.bytes; }
        }

        public string Text
        {
            get { return _www.text; }
        }
        
        public bool IsDone { get; private set; }
        
        public DownloadOperation(MainUpdater updater, SourceType source, string path)
        {
            _targetUrl = string.Empty;
            switch (source)
            {
                case SourceType.RemotePath:
                    _targetUrl = Path.Combine(updater.RemoteUrl, path);
                    break;
                case SourceType.PersistentPath:
                    _targetUrl = Utils.GetWWWPersistentPath(path);
                    break;
                case SourceType.StreamingPath:
                    _targetUrl = Utils.GetWWWStreamingAssetPath(path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("source", source, null);
            }
        }
        
        public override bool keepWaiting
        {
            get
            {
                if (_www == null)
                {
                    _www = new WWW(_targetUrl);
                }
                IsDone = _www.isDone;
                return !IsDone;
            }
        }
    }
}