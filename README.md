# AssetUpdater

AssetUpdater 为开发者提供了方便的生成以及下载 Assetbundle 的工作流。借助该插件，开发者可以一键式的生成 Assetbundle 并在自己的项目中使用几行代码来完成从特定链接下载需要被更新的所有 Assetbundle。

[![License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/sNaticY/AssetUpdater-Core/blob/master/LICENSE)
[![License](https://img.shields.io/badge/pre--release-v0.9-orange.svg)](https://github.com/sNaticY/AssetUpdater-Core/releases)

# 安装

* 方法一：在 Unity 工程 Assets 目录下新建文件夹 AssetUpdater， 下载 Zip 后将所有文件解压至该文件夹即可。
* 方法二：直接下载 Release 最新版本的 AssetUpater.unitypackage 并将其导入工程即可
* 方法三：在 Unity 工程中添加 git submodule 并设置相对路径为 Assets/AssetUpdater。

# 快速入门

该插件的主要功能分为 ”生成 Assetbundle“ 和 ”下载 Assetbundle“ 两部分。

### 生成 Assetbundle

导入插件后单击 Unity 菜单栏中的 `Window/AssetUpdater Settings`即可在 Inspector 中显示设置面板。如下图所示：

![AssetUpdater settings](http://osccnzbxn.bkt.clouddn.com/17070701.png)

当你为某些文件设置`assetbundle name`后，该 assetbundle 就会出现在`Local Bundles`或`Remote Bundles`中，其中`Local Bundles`中的 assetbundle 会在生成后被额外复制到`streamingassets`目录下。在打包时随安装包发布，并生成独特的 versionfile 做记录，防止资源重复下载。

选择平台后点击最下方`Build Assetbundles`按钮即可生成 Assetbundle 及其版本信息文件。其中原始的 assetbundle 及其 assetbundlemanifest 文件会被放置在与 Unity 工程中`Assets`目录同级的`AssetBundlePool`文件夹，并将 assetbundle 文件按照原始目录结构复制到与 Unity 工程中`Assets`目录同级的`AssetBundleServer`文件夹中，同时在该文件夹生成版本信息文件，版本信息文件默认文件名为`versionfile.bytes`。`AssetBundleServer`文件夹中的全部内容放置在你的 http 文件服务器中供客户端下载即可。

> 如需要本地测试则可以在`AssetBundleServer`目录运行`sudo python -m SimpleHTTPServer 80`命令，即可开启本地 http 文件服务，访问 http://localhost 即可下载文件。

### 下载 Assetbundle

在`AssetUpdater Seetings`中设置`remote url`为你的 http 文件服务器地址，如需本地测试则设置为 http://localhost 即可。

在 Unity 工程中添加如下代码。

```csharp
using Meow.AssetUpdater.Core;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	// Use this for initialization
	IEnumerator Start()
	{
		yield return MainUpdater.LoadAllVersionFiles();
		yield return MainUpdater.UpdateFromStreamingAsset();
		yield return MainUpdater.UpdateFromRemoteAsset();
	}	
}
```

`MainUpdater.LoadAllVersionFiles()`会加载本地 streamingAssetPath 和 persistentDataPath （如果不存在则会自动生成一个）以及 remote url 中所有的 versionfile。

`MainUpdater.UpdateFromStreamingAsset()`则会对比 persistentDataPath 和 streamingAssetPath 中的 versionfile 并将不存在或版本更新的 assetbundle 文件复制至 persistentDataPath 中。复制后将会更新 persistentDataPath 对应的 versionfile 中的信息并将其写入磁盘。每复制完一个 assetbundle 并将其写入磁盘后会立即更新 versionfile。因此即使下载过程中断也不会导致已下载的 assetbundle 下次启动时被重复下载。

`MainUpdater.UpdateFromRemoteAsset()`会对比 persistentDataPath 和 remote Url 中的 versionfile，将不存在或版本更新的 assetbundle 文件复制至 persistentDataPath 中。在生成 assetbundle 时勾选至 Local Bundles 的 assetbundle 文件在上一步已被更新至 persistentDataPath 因此不会重复下载，除非 remote 中的 assetbundle 版本更新。每更新完一个 assetbundle 并将其写入磁盘后会立即更新 versionfile。因此即使下载过程中断也不会导致已下载的 assetbundle 下次启动时被重复下载。

# 主要接口

## Meow.AssetUpdater.Core.MainUpdater

```csharp
/// <summary>
/// 初始化 AssetUpdater 全局设置，可用于覆盖 AssetUpdater Settings 中的设置。
/// </summary>
/// <param name="remoteUrl">远程 Assetbundle 服务器地址</param>
/// <param name="relativePath">放置 assetbundle 及 versionfile 的相对路径</param>
/// <param name="versionFileName">versionfile 的命名，默认为 "versionfile.bytes"</param>
public static void Initialize(string remoteUrl, string relativePath, string versionFileName);
  
/// <summary>
/// 下载位于远程服务器、persistentDataPath 以及 streamingAssetPath 中的所有 versionfile，
/// 用于对比文件状态生成下载列表准备下载
/// </summary>
/// <returns>返回 IEnumrator 可在 coroutine 中通过 "yield return" 调用以实现异步操作</returns>
public static IEnumerator LoadAllVersionFiles();

/// <summary>
/// 从 StreamingAssetPath 或 RemoteUrl 下载 assetbundle 到 persistentDataPath，
/// 之所以分成两个函数是因为只有 UpdateFromRemoteAsset() 会消耗流量。因此开发者可以考虑
/// 通过返回的 UpdateOperation 中的对象进行 UI 提示等。
/// </summary>
/// <returns>返回 UpdateOperation 对象提供当前 Update 信息，如下载进度，剩余文件大小等</returns>
public static UpdateOperation UpdateFromStreamingAsset();
public static UpdateOperation UpdateFromRemoteAsset();

/// <summary>
/// 根据资源路径获取其所在的 assetbundle 的名称
/// </summary>
/// <param name="path">资源路径</param>
/// <returns>返回 assetbundle 的名称</returns>
public static string GetAssetbundleNameByAssetPath(string path)
```

## Meow.AssetUpdater.Core.UpdateOperation

```csharp
/// <summary>
/// 当前正在下载的单个文件的进度 (0,1)
/// </summary>
public float SingleProgress{ get; }

/// <summary>
/// 总下载进度 (0,1)
/// </summary>
public float TotalProgress { get; }

/// <summary>
/// 当前正在下载的单个文件的已下载部分的大小
/// </summary>
public int SingleDownloadedSize { get; }

/// <summary>
/// 当前正在下载的单个文件的原始大小
/// </summary>
public int SingleSize { get; }

/// <summary>
/// 已下载的所有文件的大小
/// </summary>
public long TotalownloadedSize { get; }

/// <summary>
/// 所有欲下载的文件的总大小
/// </summary>
public long TotalSize { get; }

/// <summary>
/// 剩余文件的数量
/// </summary>
public int RemainBundleCount { get; }
```

# 
