using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Meow.AssetUpdater.Core
{
    public class DownloadOperation : CustomYieldInstruction
    {
        private readonly WWW www;

        public float Progress
        {
            get { return www.progress; }
        }

        public AssetBundle AssetBundle
        {
            get { return www.assetBundle; }
        }

        public byte[] Bytes
        {
            get { return www.bytes; }
        }

        public string Text
        {
            get { return www.text; }
        }
        
        public bool IsDown
        {
            get { return www.isDone; }
        }
        
        public DownloadOperation(MainUpdater updater, SourceType source, string path)
        {
            string targetUrl = string.Empty;
            switch (source)
            {
                case SourceType.RemotePath:
                    targetUrl = Path.Combine(updater.RemoteUrl, path);
                    break;
                case SourceType.PersistentPath:
                    targetUrl = Utils.GetWWWPersistentPath(path);
                    break;
                case SourceType.StreamingPath:
                    targetUrl = Utils.GetWWWStreamingAssetPath(path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("source", source, null);
            }
            www = new WWW(targetUrl);
        }
        
        public override bool keepWaiting
        {
            get { return !www.isDone; }
        }
    }
}