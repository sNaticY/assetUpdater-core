using System;
using System.Collections.Generic;
using System.IO;
using Meow.AssetUpdater.Core;
using LitJson;
using UnityEngine;

namespace Meow.AssetUpdater
{
    public class UpdateOperation : CustomYieldInstruction
    {
        /// <summary>
        /// the download progress of current downloading file (0,1)
        /// </summary>
        public float SingleProgress{ get { return _downloadOperation == null ? 0 : _downloadOperation.Progress; } }
        
        /// <summary>
        /// the download progress of all files (0,1)
        /// </summary>
        public float TotalProgress { get { return (float)TotalownloadedSize / TotalSize; } }
        
        /// <summary>
        /// the number of bytes you have downloaded for current downloading file
        /// </summary>
        public int SingleDownloadedSize { get { return _downloadOperation == null || _currentUpdatingBundle.Value == null ? 1 : (int)(_downloadOperation.Progress * _currentUpdatingBundle.Value.Size); } }
        
        /// <summary>
        /// the size of current downloading file
        /// </summary>
        public int SingleSize { get { return _currentUpdatingBundle.Value == null ? 1 : _currentUpdatingBundle.Value.Size; } }
        
        /// <summary>
        /// the number of bytes you have downloaded for all files
        /// </summary>
        public long TotalownloadedSize { get { return _writedSize + SingleDownloadedSize; } }
        
        /// <summary>
        /// the size of all files
        /// </summary>
        public long TotalSize { get; private set; }

        /// <summary>
        /// the count of remaining assetbundls
        /// </summary>
        public int RemainBundleCount
        {
            get { return _updateBundleQueue.Count; }
        }
        
        public bool IsDone { get; private set; }

        private long _writedSize;
        
        private MainUpdater _updater;
        private readonly VersionInfo _originVersionInfo;
        private readonly VersionInfo _sourceVersionInfo;

        private readonly Queue<KeyValuePair<SourceType, BundleInfo>> _updateBundleQueue = new Queue<KeyValuePair<SourceType, BundleInfo>>();
        private KeyValuePair<SourceType, BundleInfo> _currentUpdatingBundle;
        
        private DownloadOperation _downloadOperation;

        public UpdateOperation(MainUpdater updater, VersionInfo originVersion, VersionInfo sourceVersion, SourceType souceType)
        {
            _updater = updater;
            _originVersionInfo = originVersion;
            _sourceVersionInfo = sourceVersion;
            
#if UNITY_EDITOR
            if (!MainUpdater.IsSimulationMode)
#endif
            {
                if (originVersion.VersionNum < sourceVersion.VersionNum)
                {
                    foreach (var sourceBundle in sourceVersion.Bundles)
                    {
                        var isContain = false;
                        foreach (var originBundle in originVersion.Bundles)
                        {
                            if (originBundle.Name == sourceBundle.Name)
                            {
                                if (originBundle.Md5Code != sourceBundle.Md5Code)
                                {
                                    _updateBundleQueue.Enqueue(new KeyValuePair<SourceType, BundleInfo>(souceType, sourceBundle));
                                    TotalSize += sourceBundle.Size;
                                }
                                isContain = true;
                                break;
                            }
                        }
                        if (!isContain)
                        {
                            _updateBundleQueue.Enqueue(new KeyValuePair<SourceType, BundleInfo>(souceType, sourceBundle));
                            TotalSize += sourceBundle.Size;
                        }
                    }
                }
            }
        }

        public override bool keepWaiting
        {
            get
            {
#if UNITY_EDITOR
                if (MainUpdater.IsSimulationMode)
                {
                    IsDone = true;
                }
                else
#endif
                {
                    if (_downloadOperation == null)
                    {
                        if (_updateBundleQueue.Count > 0)
                        {
                            _currentUpdatingBundle = _updateBundleQueue.Dequeue();
                            _downloadOperation = new DownloadOperation(_updater, _currentUpdatingBundle.Key, CalcPath(_currentUpdatingBundle.Value.Name));
                            _updater.StartCoroutine(_downloadOperation);
                        }
                        else
                        {
                            IsDone = true;
                        }
                    }
                    else
                    {
                        if (_downloadOperation.IsDone)
                        {
                            Utils.Instance.WriteBytesTo(SourceType.PersistentPath, CalcPath(_currentUpdatingBundle.Value.Name),
                                _downloadOperation.Bytes);
                            _originVersionInfo.UpdateBundle(_currentUpdatingBundle.Value);
                            var bytes = System.Text.Encoding.ASCII.GetBytes(JsonMapper.ToJson(_originVersionInfo));
                            Utils.Instance.WriteBytesTo(SourceType.PersistentPath, CalcPath(_updater.VersionFileName), bytes);

                            _writedSize += _downloadOperation.Bytes.Length;

                            if (_updateBundleQueue.Count > 0)
                            {
                                _currentUpdatingBundle = _updateBundleQueue.Dequeue();
                                _downloadOperation = new DownloadOperation(_updater, _currentUpdatingBundle.Key, CalcPath(_currentUpdatingBundle.Value.Name));
                                _updater.StartCoroutine(_downloadOperation);
                            }
                            else
                            {
                                _originVersionInfo.UpdateVersion(_sourceVersionInfo);
                                bytes = System.Text.Encoding.ASCII.GetBytes(JsonMapper.ToJson(_originVersionInfo));
                                Utils.Instance.WriteBytesTo(SourceType.PersistentPath, CalcPath(_updater.VersionFileName), bytes);
                                IsDone = true;
                            }
                        }
                    }
                }
                return !IsDone;
            }
        }


        private string CalcPath(string fileName)
        {
            var rootPath = Path.Combine(_updater.ProjectName, Utils.GetBuildPlatform().ToString());
            var path = Path.Combine(rootPath, fileName);
            return path;
        }
    }
}