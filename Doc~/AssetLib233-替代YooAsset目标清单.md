# AssetLib233 替代 YooAsset 目标清单

## 必须更好用

- 用户只接 `AssetLib233.Instance`。
- `login` 首组快启，登录后 N 个 AssetGroup 聚合下载。
- 多 AssetGroup 只显示一条 Loading。
- 核心打包后自动产出报告并弹窗，支持飞书通知。
- CDN 发布保持项目链路；可选 `Plugin_CDN_Deploy` 独立承载旧一条龙方案。

## 必须更适合小游戏

- 微信小游戏优先，`WX` 单宏识别平台。
- 默认并发 10。
- 支持单文件下载和批量并发下载。
- 不要求用户改第三方源码。
- 不依赖 YooAsset。

## 必须更高性能

- 主线程异步 Tick。
- 热路径 NonAlloc。
- 框架内部 ListPool。
- 自动 Asset GC + 手动 Asset GC。
- 新版本 Manifest 不再引用的 AB 可清理。

## 必须更开放

- `IAssetLib233BuildCompressionStrategy`：压缩策略。
- `IAssetLib233BuildPackRule`：打包规则。
- `IAssetLib233BuildVerifier`：产物校验。
- `IAssetLib233DownloadTransport`：平台下载传输层。
- `IAssetLib233EditorBuildNotifier`：构建后通知，可接飞书 / 企业微信 / 自建平台。
- `Plugin_CDN_Deploy`：外部 CDN 工具可选插件化，不污染核心库。
