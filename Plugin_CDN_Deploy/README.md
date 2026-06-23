# AssetLib233 Plugin_CDN_Deploy

可选 CDN 发布插件。核心 AssetLib233 不依赖本插件；当前项目继续使用原 YooAsset 时代的 CDN 发布链路，本插件默认不参与编译。

## 启用方式

`AssetLib233.Plugin_CDN_Deploy.Editor.asmdef` 带有 `ASSETLIB233_CDN_DEPLOY` 宏约束。只有项目显式添加该宏时，插件内编辑器代码才会编译。

## 能力

- `.local` 私密配置：SSH、CDN、token、服务器路径不进 AssetLib233 仓库。
- 多渠道映射：按平台、宏、环境映射旧 CDN Go 工具配置名。
- Provider 扩展：默认火山引擎中国、阿里云、腾讯云、AWS、Custom，用户可注册自定义 provider。
- Agent-first：打包、上传、刷新、预热、报告、溯源可由外部命令串起来。
- 工具入口：`Tools/agent-publish.ps1`、`Tools/agent-validate.ps1`。

## 当前项目约定

本项目不用本插件发布 CDN。资源上传、刷新、预热仍走项目已有工具链，AssetLib233 核心只负责 AB、Manifest、version、下载、加载、GC、诊断。
