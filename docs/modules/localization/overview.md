# AtomUI.Localization 模块

`AtomUI.Localization` 是 AtomUI 的应用级本地化运行时项目。它提供语言身份、Catalog registry、不可变 Snapshot、
LanguageManager、Localizer 和 Avalonia 动态资源桥，不拥有 XLIFF 构建、语言包打包或主题状态。

## 职责

- 定义 `LanguageTag`、`LanguageDefinition`、`LanguageState` 和 `LanguageSnapshot`。
- 注册编译期生成的 Catalog 与 Translation Bundle descriptor。
- 构建并原子发布当前语言的完整资源快照。
- 提供 `ILanguageManager`、`ILocalizer` 和 XAML 资源扩展运行时。
- 管理 Culture、FlowDirection、回退和资源 Provider 生命周期。

## 源码目录

| 目录 | 职责 |
| --- | --- |
| `Catalog/` | Catalog schema、descriptor 和 registry |
| `Resources/` | Avalonia resource extension、provider 和 resource key |
| `Services/` | bootstrap、builder、host、manager、localizer 和日志入口 |
| `LanguageData/` | LanguageTags Generator 的固定数据输入 |
| 项目根目录 | language identity、definition、state、snapshot 和回退模型 |

## 对外关系

- `AtomUI.Core` 引用本模块并在根 Builder 中暴露本地化配置入口。
- `AtomUI.Generator` 生成 Catalog、Bundle、模块注册和应用 bootstrap。
- `AtomUI.Toolchain` 的 Tasks profile（产物 `AtomUI.Build.Tasks.dll`）负责静态语言包文件与打包副作用。
- Control 包只注册自身 Catalog 和内置 Bundle，不实现独立语言状态。

## 应用资源接入边界

LocalizationHost 拥有语言服务组合，LanguageResourceProvider 提供当前语言的 Avalonia 资源投影。
Core 的 ApplicationScope 负责物理挂载和 FlowDirection Style，LanguageManager 不接管 Application 的主题字典。
主题与语言的共同宿主契约见[应用资源宿主设计](../../architecture/systems/theming/application-resource-host.md)，
实现证据及平台验收边界见[资源宿主验证](../../architecture/systems/theming/application-resource-host-verification.md)。

## 相关文档

- [本地化系统架构](../../architecture/systems/localization/overview.md)
- [本地化运行时架构](../../architecture/systems/localization/runtime.md)
- [本地化生成与构建](../../architecture/systems/localization/generation-and-build.md)
- [公共 API](../../reference/localization/public-api.md)
- [Catalog 与 XLIFF](../../reference/localization/catalog-and-xliff.md)
