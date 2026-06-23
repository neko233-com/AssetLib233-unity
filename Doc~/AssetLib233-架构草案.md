# AssetLib233 架构草案

AssetLib233 是项目级独立资源系统，底层为自研 AssetGroup / Collector / Manifest / FileSystem / Provider。实现思路参考 YooAsset 3.x 的工程组织和资源包模型，但不依赖 YooAsset 程序集。

## 分层

- `Runtime`: 平台无关主入口、AssetGroup、Collector、Manifest、文件系统、缓存、下载、热更、Provider、调试。
- `Plugin_MiniGame_WX`: 微信小游戏平台插件。
- `Plugin_MiniGame_DouYin`: 抖音小游戏平台插件。
- `Plugin_MiniGame_TapTap`: TapTap 小游戏平台插件。
- `Editor`: UI Toolkit 设置窗口和设置资产创建菜单。

## 平台

- PC / iOS / Android / PS5: 默认 HostPlayMode / OfflinePlayMode。
- WebGL: 默认 WebPlayMode。
- 微信小游戏: `WX` 宏，接微信 MiniGame FS，下载并发固定 10。
- TapTap 小游戏: `TAPMINIGAME` 宏，接 TapTap MiniGame FS。
- 抖音小游戏: `DOUYINMINIGAME` 宏，接 Tiktok MiniGame FS。

## 项目边界

AssetLib233 不合并 login 首包与 default/story 内容包流程。现有 `HotUpdateManager`、`HotUpdateDefaultPackageDownloader233`、`ContentPackManager233` 仍保留职责边界。后续迁移时只替换各自内部初始化 / Manifest / 下载 / 加载后端，不改变启动任务组分层。
