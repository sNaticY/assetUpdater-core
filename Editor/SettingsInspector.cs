using System.IO;
using System.Linq.Expressions;
using Meow.AssetUpdater.Core;
using UnityEditor;
using UnityEngine;

namespace Meow.AssetUpdater.Editor
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : UnityEditor.Editor
    {
        private readonly GUIContent _remoteUrlText = new GUIContent("Remote Url", "Remote Aseetbundles URL");
        private readonly GUIContent _versionFileNameText = new GUIContent("Version File Name", "The Version File You Generate");
        private readonly GUIContent _relativePathText = new GUIContent("Relative Path", "");
        private readonly GUIContent _targetPlatformText = new GUIContent("Target Platform", "Run app against the preview service");
        private readonly GUIContent _localBundleText = new GUIContent("Local Bundles", "The Assetbundles that would be copied to StreamingPath");
        private readonly GUIContent _remoteBundleText = new GUIContent("Remote Bundles", "The Assetbundles would be copied to AssetServer Path");

        private bool _isLocalFoldOut;
        private bool _isRemoteFoldOut;

        public override void OnInspectorGUI()
        {
            var downloaPath = Path.Combine(Path.Combine(Path.Combine(Settings.RemoteUrl, Settings.RelativePath), Settings.Platform.ToString()),
                Settings.VersionFileName);
            EditorGUILayout.HelpBox(string.Format("You will download the assetbundles version file at url : {0}", downloaPath), MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            Settings.RemoteUrl = EditorGUILayout.TextField(_remoteUrlText, Settings.RemoteUrl).Trim();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            Settings.RelativePath = EditorGUILayout.TextField(_relativePathText, Settings.RelativePath).Trim();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            Settings.Platform = (BuildPlatform) EditorGUILayout.EnumPopup(_targetPlatformText, Settings.Platform);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            Settings.VersionFileName = EditorGUILayout.TextField(_versionFileNameText, Settings.VersionFileName).Trim();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _isLocalFoldOut = EditorGUILayout.Foldout(_isLocalFoldOut, _localBundleText);
            if (_isLocalFoldOut)
            {
                for (int i = 0; i < Settings.LoaclBundles.Count; i++)
                {
                    var bundle = Settings.LoaclBundles[i];
                    EditorGUILayout.BeginHorizontal();
                    string[] assetsPath = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
                    string toopTips = "Content Assets:";
                    foreach (var path in assetsPath)
                    {
                        toopTips = toopTips + "\n - " + path + "\t";
                    }
                    bool toggle = EditorGUILayout.ToggleLeft(new GUIContent(bundle, toopTips), true);
                    if (!toggle)
                    {
                        Settings.SetBundleToRemote(bundle);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space();

            _isRemoteFoldOut = EditorGUILayout.Foldout(_isRemoteFoldOut, _remoteBundleText);
            if (_isRemoteFoldOut)
            {
                foreach (var bundle in AssetDatabase.GetAllAssetBundleNames())
                {
                    if (!Settings.LoaclBundles.Contains(bundle))
                    {
                        EditorGUILayout.BeginHorizontal();
                        bool toggle = EditorGUILayout.ToggleLeft(bundle, false);
                        if (toggle)
                        {
                            Settings.SetBundleToLocal(bundle);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.HelpBox(
                "Build Assetbundles to folder AssetBundlePool, generate versionfile and copy assetbundles to AssetBundlePool folder",
                MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("\tBuild AssetBundles\t"))
            {
                var index = Application.dataPath.LastIndexOf(Path.DirectorySeparatorChar);
                var assetbundlePoolPath = Path.Combine(Application.dataPath.Substring(0, index), "AssetBundlePool");
                var assetbundleServerPath = Path.Combine(Application.dataPath.Substring(0, index), "AssetBundleServer");
                BundleGenerater.Generate(Settings.RelativePath, assetbundlePoolPath, assetbundleServerPath, Settings.Platform);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(Settings.Instance);
                AssetDatabase.SaveAssets();
            }
        }
    }
}