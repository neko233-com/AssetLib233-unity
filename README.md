# AssetLib233-unity

独立 Unity / 团结引擎资源热更库。

文档: https://neko233-com.github.io/AssetLib233-unity/

仓库: https://github.com/neko233-com/AssetLib233-unity

## 安装

UPM Git 一行安装：

```text
https://github.com/neko233-com/AssetLib233-unity.git
```

无 Git 导入，一行 PowerShell 下载到当前 Unity 项目：

```powershell
powershell -ExecutionPolicy Bypass -Command "iwr https://raw.githubusercontent.com/neko233-com/AssetLib233-unity/main/Tools/install-assetlib233.ps1 -OutFile $env:TEMP/install-assetlib233.ps1; & $env:TEMP/install-assetlib233.ps1 -ProjectRoot (Get-Location).Path"
```

核心模型：

- `AssetLib233.Instance`: 用户 facade 单例。
- `AssetGroup`: 顶层热更资源组。
- `AssetCollector`: 一个资源组内的收集器，决定资源如何切成 AB / RawBundle / ArchiveBundle。
- `AssetManifest`: 二进制清单，保存地址、标签、依赖、hash、crc、size。
- `Plugin_MiniGame_WX` / `Plugin_MiniGame_DouYin` / `Plugin_MiniGame_TapTap`: 小游戏平台插件。
- `Plugin_UniTask`: UniTask 扩展，Runtime 不强依赖 UniTask。
- `AssetLib233StartupPlan`: 一个 login 快速首组 + 登录后 N 个 AssetGroup。
- `AssetLib233AssetGcService`: 自动 Asset GC + 手动 Asset GC。
- `AssetLib233EditorPublishPipeline`: 打包、上传 CDN、刷新 CDN、报告、溯源的一条龙发布流水线。

## 快速示例

```csharp
AssetLib233PackageConfig config = new AssetLib233PackageConfig();
config.PackageName = "default";
config.PlayMode = EnumAssetLib233PlayMode.Host;
config.DefaultHostServer = "https://cdn.example.com/default";
config.FallbackHostServer = "https://backup.example.com/default";

AssetLib233.Instance.Initialize();
AssetLib233.Instance.InitializeGroup(config);

AssetHandle233<GameObject> handle =
    AssetLib233.Instance.LoadAssetAsync<GameObject>("default", "ui/main.prefab");
```

## 异步支持

- callback: `operation.OnCompleted(...)`
- C# await: `await operation`
- UniTask: `await operation.ToUniTask()`

## 文档

纯 HTML 文档位于 `docs/index.html`，可直接作为 GitHub Pages 使用。

## .local 私密配置

复制 `Samples~/AssetLib233.publish.local.example.json` 到项目根目录：

```text
AssetLib233.publish.local.json
```

该文件用于配置本机 SSH、CDN、上传工具、刷新工具、溯源服务器，不提交 Git。也可用环境变量指定：

```powershell
$env:ASSETLIB233_LOCAL_CONFIG="D:/Private/AssetLib233.publish.local.json"
```

## Agent-first 发布

```powershell
powershell -ExecutionPolicy Bypass -File Assets/neko233/AssetLib233/Tools/agent-publish.ps1 -UnityPath "D:/Unity/Editor/Unity.exe" -ProjectRoot "D:/Project"
```

发布完成后会在 `.local` 的 `reportRoot` 下生成独立报告 JSON，可回溯每次 build / upload / refresh 的命令、日志、退出码。

若配置了 `cdnTraceServerUrl`，发布报告会以 JSON POST 到溯源服务器。鉴权 token 从 `cdnTraceTokenEnvName` 指向的环境变量读取，不写入仓库。
