# AtomUI.Generator 模块概览

> 控件注册部分对应本地已实现的 TypeMap 生成体系。验证与发布状态由
> [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)维护；本页不表示新生成接口已经发布。

`AtomUI.Generator` 是普通 Roslyn 源生成器，以 Analyzer 参与包与应用的日常编译，不是应用运行时依赖。
控件包的 Token、Semantic Part、主题资产和注册片段来自同一普通生成模型；发布时由官方链接器决定可达类型。
Browser 的映射转换属于独立的 [TypeMap Linker 模块](../typemap-linker/overview.md)。

## 职责

- 从 Global Token 和可选 `[ControlDesignToken]` 定义生成强类型 schema、资源键、owner identity 与投影代码。
- 验证标记抽象 Token 与 `sealed` 终端 Token 的继承契约，将跨程序集属性扁平化到终端 schema。
- 发现具备真实主题、Token 或语义契约的 public/internal Control，生成逐控件注册片段及单项 descriptor factory。
- 从 `AtomUIThemeDefinition` 的声明输入生成强类型主题包注册与默认赋值，解析静态字面量；派生算法留在 Core 运行时。
- 解析包内编译型 AXAML 的主题导出、资源作用域和显式包含关系，生成资产元数据与静态 factory。
- 生成包注册 helper、条件 TypeMap、包 marker 和映射 accessor；应用编译根据已解析引用的 marker 自动生成引导。
- 从 `[LanguageCatalog]` enum 与 XLIFF 2.1 生成语言资源、Catalog/Bundle 注册和应用 bootstrap。
- 生成数据成员访问器、scoped resource host 等静态代码，减少运行时反射与手工清单。

注册生成不分析应用 C#/AXAML usage，不构造调用图或依赖闭包，不产生应用注册计划或伴随协议文件。
包是 provider、语言与初始化的边界；目录布局不决定裁剪粒度。

## 生成能力与边界

| 能力 | 说明 |
| --- | --- |
| Token/schema 生成 | 生成 exact CLR type、owner-bearing identity、强类型资源键与单项 descriptor；抽象 Token 不产生运行时身份 |
| 主题定义生成 | 从 XML 声明及 Token 契约生成注册与类型化默认值，保留运行时配置及算法语义 |
| 主题资产生成 | 生成主题导出、RequiredTokenOwners、资产 factory 与顺序元数据；资源 wrapper 由构建任务提供 |
| 控件注册生成 | 将 Control/Token/Style 等保留条件映射到同一逻辑片段，full 与选中路径使用相同 factory |
| 包引导生成 | 仅读已解析程序集的包 marker，生成具体 Group 的 assembly target；不调用可选包入口 |
| `LocalizationGenerator` / `LanguageTagsGenerator` | 编译语言契约和固定语言数据，生成模块注册、应用 bootstrap 与标准语言元数据 |
| `DataMemberAccessorGenerator` | 生成 AOT 友好的数据访问器，不属于控件注册选择后端 |
| `ScopedResourceHostGenerator` | 生成非 Visual `AvaloniaObject` 的资源宿主生命周期代码 |

## 代码所有权

下表列出当前源码的职责组织。

| 目录 | 职责 |
| --- | --- |
| `Registration/` | 普通注册值模型、片段/marker/accessor/包 helper 与引用引导 |
| `DesignToken/`、`ThemeSchema/` | Token 发现、身份、资源键、schema 与单项 factory |
| `ThemeDefinitions/`、`ThemeDefinitions/Literals/` | 主题声明、构建契约、静态字面量解析与 C# 注册/赋值生成 |
| `ThemeAssets/` | 主题导出、词法资源依赖、静态资产元数据与 factory |
| `SemanticParts/` | 语义声明、路由验证、专用 Style 和逐控件 descriptor |
| `Localization/` | Catalog/XLIFF/Bundle 编译、语言模块与应用 bootstrap |
| `DataMemberAccessors/` | 数据成员访问器 Generator、Analyzer 和 Writer |
| `ResourceHost/` | scoped resource host Generator、类型模型和 Writer |

## 专题导航

| 文档 | 所有权 |
| --- | --- |
| [控件注册生成](control-registration.md) | 本模块的注册输入、输出、增量模型和验证边界 |
| [Semantic Part Generator](semantic-part-generator.md) | 语义声明、AXAML 路由验证与专用 Style 生成 |
| [Scoped Resource Host Generator](scoped-resource-host-generator.md) | scoped resource host 的识别、生成与生命周期 |
| [编译主题包](../../architecture/systems/theming/compiled-theme-packages/compilation.md) | 类型化注册与默认赋值、字面量解析、构建契约和 AOT 保留边界 |

跨模块契约见 [TypeMap 注册体系](../../architecture/foundations/aot-typemap-registration.md)、
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)和
[TypeMap 生成 ABI](../../reference/aot/typemap-contract.md)。包作者使用
[第三方控件包指南](../../guides/theming/third-party-control-packages.md)。

## 维护要求

生成输出是源模型的结果，不作为普通源码手工修改。新增 Control、Own Token、主题资产或 Semantic Part 后，验证单项 factory、
条件映射和完整注册结果来自同一事实源。只有资源的 internal presenter 不要求伪造公开 Token identity。
多个 Control 引用同一资产时，保留一个 AssetId/factory，不能复制字典改变资源覆盖关系。

增量结果使用稳定的值模型、metadata name 与规范化路径，不持久缓存 Compilation、SemanticModel 或 symbol。
生成器之间共享模型或通过确定命名的 partial hook 协作，不读取另一生成器的输出作为输入。

普通生成器、资源 wrapper、Build Tasks 进程隔离及 Localization 工具继续随产品构建资产自动交付。
验证必须包含独立 NuGet 消费，确认这些工具不进入应用运行时输出。
