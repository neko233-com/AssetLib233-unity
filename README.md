# AssetLib233-unity

独立 Unity / 团结引擎资源热更库。

核心模型：

- `AssetLib233.Instance`: 用户 facade 单例。
- `AssetGroup`: 顶层热更资源组。
- `AssetCollector`: 一个资源组内的收集器，决定资源如何切成 AB / RawBundle / ArchiveBundle。
- `AssetManifest`: 二进制清单，保存地址、标签、依赖、hash、crc、size。
- `Plugin_MiniGame_WX` / `Plugin_MiniGame_DouYin` / `Plugin_MiniGame_TapTap`: 小游戏平台插件。
- `Plugin_UniTask`: UniTask 扩展，Runtime 不强依赖 UniTask。

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
