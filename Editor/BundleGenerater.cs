using System;
using System.IO;
using System.Security.Cryptography;
using LitJson;
using Meow.AssetUpdater.Core;
using UnityEditor;
using UnityEngine;

namespace Meow.AssetUpdater.Editor
{
    public static class BundleGenerater
    {
        /// <summary>
        /// Generate AssetBundles and Generate version_file.bytes to AssetBundleServer Path
        /// </summary>
        public static void Generate(string relativePath, string poolPath, string serverPath, BuildPlatform platform)
        {
            // calculate assetbundle output path
            var platformPath = platform.ToString();
            var bundleBuildPath = Path.Combine(poolPath, platformPath);
            if (Directory.Exists(bundleBuildPath))
            {
                Directory.Delete(bundleBuildPath, true);
            }

            // build assetbundles and get manifest
            var manifest = BuildAssetBundles(bundleBuildPath);

            // calculate assetbundle server path
            var bundleCopyPath = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "StreamingAssets"), relativePath), platformPath);
            if (Directory.Exists(bundleCopyPath))
            {
                Directory.Delete(bundleCopyPath, true);
            }

            // copy assetbundles to streamingAsset path
            CopyAssetBundles(manifest, bundleBuildPath, bundleCopyPath, platformPath, true);

            // generate VersionFile for streaming aseets
            GenerateVersionFile(manifest, relativePath, bundleCopyPath, platformPath, true);

            bundleCopyPath = Path.Combine(Path.Combine(serverPath, relativePath), platformPath);
            if (Directory.Exists(bundleCopyPath))
            {
                Directory.Delete(bundleCopyPath, true);
            }

            // copy assetbundles to asset server path
            CopyAssetBundles(manifest, bundleBuildPath, bundleCopyPath, platformPath, false);

            // generate VersionFile for asset server
            GenerateVersionFile(manifest, relativePath, bundleCopyPath, platformPath, false);


            
        }

        private static AssetBundleManifest BuildAssetBundles(string outputPath)
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
            Directory.CreateDirectory(outputPath);

            var manifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, Utils.GetBuildTarget(Settings.Platform));
            return manifest;
        }

        private static void CopyAssetBundles(AssetBundleManifest manifest, string fromRootPath, string destRootPath, string manifestName,
            bool localLimit)
        {
            // copy all assetbundles to destination directory
            foreach (var bundle in manifest.GetAllAssetBundles())
            {
                if (localLimit && !Settings.LoaclBundles.Contains(bundle))
                {
                    continue;
                }
                var destPath = Path.Combine(destRootPath, bundle);
                var fromPath = Path.Combine(fromRootPath, bundle);
                Utils.Instance.WriteBytesTo(destPath, File.ReadAllBytes(fromPath));
                File.Copy(fromPath, destPath, true);
            }
            if (!Directory.Exists(destRootPath))
            {
                Directory.CreateDirectory(destRootPath);
            }
            // copy main manifest file to destination directory
            File.Copy(Path.Combine(fromRootPath, manifestName), Path.Combine(destRootPath, manifestName), true);
        }

        private static void GenerateVersionFile(AssetBundleManifest manifest, string relativePath, string outputPath, string manifestName,
            bool localLimit)
        {
            VersionInfo versionInfo = new VersionInfo();
            versionInfo.VersionNum = localLimit
                ? long.Parse(DateTime.Now.ToString("yyMMddHHmmss"))
                : long.Parse(DateTime.Now.ToString("yyMMddHHmmss")) + 1;
            versionInfo.RelativePath = relativePath;

            // fill version file with normal assetbundle infomation
            foreach (var bundle in manifest.GetAllAssetBundles())
            {
                if (localLimit && !Settings.LoaclBundles.Contains(bundle))
                {
                    continue;
                }
                var bytes = File.ReadAllBytes(Path.Combine(outputPath, bundle));
                FillBundleInfo(manifest, bundle, bytes, ref versionInfo);
            }

            // fill version file with assetbundle manifest infomation
            var manifestBytes = File.ReadAllBytes(Path.Combine(outputPath, manifestName));
            FillBundleInfo(manifest, manifestName, manifestBytes, ref versionInfo);

            // write version file with json
            string verJson = JsonMapper.ToJson(versionInfo);
            File.WriteAllText(Path.Combine(outputPath, Settings.VersionFileName), verJson);
        }

        private static void FillBundleInfo(AssetBundleManifest manifest, string name, byte[] bytes, ref VersionInfo versionInfo)
        {
            var bundleInfo = new BundleInfo();

            // write assetbundle normal infomation
            bundleInfo.Name = name;
            bundleInfo.Size = bytes.Length;
            MD5 md5 = new MD5CryptoServiceProvider();
            bundleInfo.Md5Code = Convert.ToBase64String(md5.ComputeHash(bytes));
            bundleInfo.Dependencies = manifest.GetAllDependencies(name);

            // relate assetbundle name with asset path
            var assetBundle = AssetBundle.LoadFromMemory(bytes);
            foreach (var content in assetBundle.GetAllAssetNames())
            {
                versionInfo.BundlePath.Add(content.ToLower(), name);
            }

            foreach (var content in assetBundle.GetAllScenePaths())
            {
                versionInfo.BundlePath.Add(content.ToLower(), name);
            }
            assetBundle.Unload(true);
            // add bundleInfo to versionInfo
            versionInfo.Bundles.Add(bundleInfo);
        }
    }
}