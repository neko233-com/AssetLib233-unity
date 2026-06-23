# AssetLib233 全能力矩阵

## 目标模型

- `AssetGroup`: AssetLib233 的顶层资源组。`login`、`default`、`story`、`voice`、`cg` 都是独立 AssetGroup。
- `AssetCollector`: 一个 AssetGroup 下 N 个收集器。每个收集器按 PackRule 生成一个或多个 AB / RawBundle / ArchiveBundle。
- `AssetManifest`: 每个 AssetGroup 一个清单，保存 asset -> bundle、bundle -> depends、tags、hash、crc、size、bundle type、encryption。

## Runtime 能力

- 初始化模式：EditorSimulate / Host / Web / Offline。
- 文件系统：Local / Web / MiniGame SDK / Archive。
- 包类型：AssetBundle / RawBundle / ArchiveBundle。
- 查询：Address、AssetPath、Tag、BundleName。
- 加载：Asset、SubAsset、AllAssets、Scene、BundleFile、Instantiate。
- 生命周期：Handle 引用计数、Bundle 引用计数、依赖引用、自动卸载、强制卸载、按 owner 释放。
- 下载：按 AssetGroup、按 Tag、按 Address、预下载、断点续传、失败重试、并发固定 10、小游戏平台专用 FS。
- 缓存：CacheIndex、hash/size/crc 校验、清理未使用、清理指定 AssetGroup / tag / address。
- Manifest：二进制清单、Tags、后续 string table / hash index / offset table。
- 调试：下载进度、Bundle 引用、资源引用、Manifest 查询、包体分析、缓存分析。

## Editor 能力

- Build Profile：AssetGroup / Collector 配置。
- Collector Inspector 扩展：后续提供 `IAssetCollectorInspector233`。
- 收集器搜索：输入资源路径定位 AssetGroup + Collector。
- 构建报告：bundle size、依赖、重复资源、Top N、tag 分布。
- 模拟下载：Editor 下 Library 缓存标记，不污染工程根目录。

## 平台能力

- 微信小游戏：只使用 `WX` 单宏识别平台，MiniGame FS，`WXDownloadSettings.SetMaxConcurrent(10)`。
- 抖音小游戏：`DOUYINMINIGAME`，Tiktok MiniGame FS。
- TapTap 小游戏：`TAPMINIGAME`，TapTap MiniGame FS。
- iOS / Android / PC / PS5：Host / Offline，后续接平台 BuiltinFileAccessor。

## 与当前项目边界

禁止合并 `login` 首包、`default` 主资源包、`story` 等内容包下载流程。AssetLib233 只提供底层能力，各启动链路仍保持现有分层。
