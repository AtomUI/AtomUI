# AtomUI 文档信息架构设计

> 本文是 2026-08-10 的设计过程记录，保留当时的结论原文。其中**入口文件命名为 `index.md`** 的结论已被推翻：
> 现行规则是任何文档目录的入口文件统一命名为 `overview.md`。命名与拆分规范的现行所有者是
> [文档结构与命名规范](../../engineering/contributing/documentation-structure-guidelines.md)。
> 本文其余部分（目录职责、事实所有权、分类边界）继续有效。

## 设计定位

本文定义 AtomUI 仓库文档的全局信息架构。文档按知识责任和长期所有权分类，不再根据当前源码目录、文件标题或
一次任务的产生位置决定归属。

重构后的文档体系必须同时服务以下读者：

- 希望理解 AtomUI 整体架构和跨模块系统的维护者。
- 希望理解某个源码项目或 NuGet 包内部职责的模块维护者。
- 希望开发、定制或维护具体 Control 的控件作者。
- 希望按步骤使用主题、窗口、跨控件基础设施和 Gallery 能力的应用开发者。
- 需要依赖稳定格式、协议和公共 API 定义的工具及第三方包作者。
- 负责构建、测试、发布、AOT、诊断和文档生成流程的工程贡献者。

## 设计原则

1. 一个稳定事实只能有一个正式所有者，其他文档通过链接引用，不复制完整规则。
2. `architecture/` 只保存跨两个以上源码模块、运行时层或构建层的当前系统架构。
3. `modules/` 与源码项目或发布包对应，只描述单个模块的职责、源码组织、注册入口和内部实现。
4. `controls/` 只保存单个 Control 或紧密 Control 家族的设计、实现、Token 和变更记录。
5. 使用步骤、示例和操作流程属于 `guides/`，不能反向定义架构契约。
6. 机器、工具和第三方作者需要精确依赖的格式与协议属于 `reference/`。
7. 开发规范、工作流、检查清单和维护纪律属于 `engineering/`。
8. 方案比较、设计过程、实施计划和阶段进度继续保存在 `docs/developments/`。
9. 不建立独立 `decisions/`；已生效结论直接写入对应正式文档。
10. `docs/developments/` 的目录名和 `specs/`、`plans/`、`progress/` 结构保持不变。
11. 自动生成文档集中到 `generated/`，生成文件不得手工修改。
12. 正式架构文档只描述当前设计，不包含候选方案、实施阶段、完成状态或历史讨论。

## 目标目录

```text
docs/
|-- index.md
|-- architecture/
|   |-- index.md
|   |-- foundations/
|   |   |-- index.md
|   |   |-- dependency-graph.md
|   |   |-- runtime-platforms.md
|   |   |-- startup-and-registration.md
|   |   `-- build-and-packaging.md
|   `-- systems/
|       |-- index.md
|       |-- theming/
|       |-- localization/
|       |-- control-infrastructure/
|       |-- rendering/
|       `-- windowing/
|-- modules/
|-- controls/
|-- guides/
|-- reference/
|-- engineering/
|-- gallery/
|-- releases/
|-- strategy/
|-- AI/
|   |-- index.md
|   `-- generated/
|       |-- index.md
|       |-- llms.config.json
|       `-- llms/
`-- superpowers/
    |-- specs/
    |-- plans/
    `-- progress/
```

`docs/developments/` 不参与正式文档导航和事实所有权判断，但继续保留当前路径与工作方式。

## 目录职责

### Architecture

`architecture/` 描述跨模块系统的当前结构、数据流、生命周期、兼容性和验证不变量。

- `foundations/` 保存解决方案依赖、运行平台、启动注册、构建和打包边界。
- `systems/theming/` 保存主题运行时、Token、Semantic Part 和跨模块主题契约。
- `systems/localization/` 保存运行时、Generator、Build Tasks 和语言包共同组成的本地化系统。
- `systems/control-infrastructure/` 保存异步加载、过滤、响应式等跨 Control 基础设施。
- `systems/rendering/` 保存边框、视觉层和覆盖层等跨控件渲染模型。
- `systems/windowing/` 保存跨 Native、Core 和 Desktop Controls 的窗口级架构。

架构文档可以链接模块实现、Reference 和 Engineering 规范，但不能依赖 Guide、Generated 或 Superpowers 才能成立。

### Modules

`modules/` 中每个一级子目录必须对应一个明确源码项目、发布包或稳定工具模块。入口统一使用 `index.md`，包含：

- 模块职责与非职责。
- 源码目录与主要 owner。
- 注册、加载或发布入口。
- 对上游和下游模块的依赖关系。
- 指向全局架构、Reference 和 Engineering 的导航。

模块内部的 generator、platform backend、provider 或 host 实现可以继续保留专题文档。跨模块公共规则不得只存在于
模块目录。

### Controls

`controls/` 继续使用现有平台、分类和 Control 目录模型。平台和分类目录使用 `index.md`；单 Control 目录继续使用：

```text
overview.md
implementation.md
<topic>-design.md
token.md
changelog.md
```

跨多个无紧密继承关系 Control 的系统能力不放入任一 Control 目录，应进入 `architecture/systems/control-infrastructure/`。

### Guides

`guides/` 面向任务组织内容，例如主题定制、异步加载接入、过滤接入、跨平台 Window 定制。Guide 可以解释架构并
提供示例，但不得成为 API、协议或设计不变量的唯一来源。

### Reference

`reference/` 保存精确定义，包括：

- Theme Definition XML 版本协议与 Schema。
- Localization Catalog、XLIFF 和语言包协议。
- 需要长期稳定引用的公共 API 模型。

Reference 文件必须标明版本、合法输入、错误边界和兼容策略，不包含源码实现 walkthrough。

### Engineering

`engineering/` 按贡献者任务重新分组：

```text
engineering/
|-- index.md
|-- contributing/
|-- development/
|-- platforms/
|-- workflows/
|-- case-studies/
`-- tooling/
```

Control 开发、Token、AOT、编译器诊断属于 `development/`；Issue、Changelog、文档和 Agent 协作属于
`contributing/`；平台开发环境和发布验证属于 `platforms/` 或 `workflows/`。

### Gallery、Releases、Strategy 与 Generated

- `gallery/` 只保存 AtomUIGallery 的组织、ShowCase authoring、平台移植和本地化维护文档。
- `releases/` 按版本保存 API change 和迁移说明。
- `strategy/` 保存不属于工程架构的产品与生态战略材料。
- `generated/llms/` 保存 LLMS Generator 输出；配置归构建或工具入口，输出禁止手改。

## 现有文档迁移

### Core 与主题

- `modules/core/theme-system.md` 移至 `architecture/systems/theming/runtime.md`。
- `modules/core/semantic-part-system.md` 移至 `architecture/systems/theming/semantic-parts.md`。
- `modules/core/theme-architecture-and-customization.md` 收敛为 `guides/theming/customization.md`；正式契约继续引用
  theming runtime 和 Semantic Part 架构。
- `modules/core/theme-definition-xml.md` 以及配套 `schemas/`、`examples/` 移至 `reference/theming/`。
- `modules/core/theme-algorithm-enum-design.md` 的稳定结论并入主题架构；设计过程由 `docs/developments/` 保留。
- `modules/core/index.md` 只描述 `AtomUI.Core` 项目，不再拥有整个主题系统的跨模块架构。

### Control 基础设施

- `modules/controls-shared/responsive-system.md` 移至
  `architecture/systems/control-infrastructure/responsive.md`。
- 根目录 `AsyncLoadingArchitecture.md` 和 `FilteringArchitecture.md` 分别拆成架构契约与使用指南。
- `modules/controls-shared/index.md` 只描述 `AtomUI.Controls.Shared` 的实现所有权，并链接上述架构。
- 根目录 `PopupAnchorScopeGuide.md` 移至 `engineering/development/popup-anchor-scope.md`。

### Localization

- 系统 overview、runtime、generation/build、language packs、diagnostics/testing 移至
  `architecture/systems/localization/`。
- Catalog、XLIFF 和公共协议移至 `reference/localization/`。
- `modules/localization/index.md` 重新描述 `AtomUI.Localization` 运行时项目本身。
- 已完成迁移记录和翻译审核不进入正式架构；继续由 `docs/developments/` 或版本迁移文档承载。

### Generator

- `modules/generator/semantic-part-generator.md` 保留为 Generator 实现文档，并链接全局 Semantic Part 架构。
- `scoped-resource-host-generator.md` 中的开发范式移至 `engineering/development/`；Generator 输入、输出和诊断仍由
  `modules/generator/` 描述。

### Native、Windowing 与 GalleryBase

- Native 项目架构和平台 API 保留在 `modules/native/`。
- Window 跨平台使用说明移至 `guides/windowing/`。
- 跨 Native 与 Desktop Controls 的 live-resize 契约移至 `architecture/systems/windowing/`。
- Desktop Controls、DataGrid、ColorPicker、Extras、Fonts、Icons 和 GalleryBase 的模块文档继续保留。
- GalleryBase 正式文档只描述当前模块；迁移阶段和完成状态继续由 `docs/developments/` 保存。

### 其他顶层目录

- `docs/overview.md` 改为 `docs/index.md`。
- `docs/architecture/` 现有文件按 `foundations/` 和 `systems/rendering/` 重组。
- `docs/CrossPlatformUIFrameworkStrategicNarrative.md` 移至 `docs/strategy/`。
- `docs/release-notes/` 改为 `docs/releases/`，并增加版本索引。
- AI 工具消费层统一由 `docs/AI/` 承载；配置和生成产物位于 `docs/AI/generated/`，LLMS 输出目录为
  `docs/AI/generated/llms/`。
- `docs/developments/` 完全保留，不创建 `.agents/work-items` 或 `architecture/decisions`。

## 索引与命名

- `docs/`、正式一级目录、Architecture 分组、Module、Control 平台和 Control 分类使用 `index.md` 作为导航入口。
- 单 Control 目录继续使用 `overview.md`，避免破坏控件文档和 LLMS 输入契约。
- 文件和目录使用小写 kebab-case；`AI` 作为顶层领域缩写保留大写，项目名只在标题和正文中保留正式大小写。
- 索引只维护导航、职责和推荐阅读顺序，不复制子文档内容。
- 同一正式文档只能由一个索引标记为 canonical，其他索引只能链接。

## 依赖方向

```text
architecture -> modules / reference / engineering
modules      -> architecture / reference / engineering
controls     -> architecture / modules / engineering
guides       -> architecture / controls / reference
gallery      -> architecture / modules / controls / engineering
releases     -> architecture / modules / controls
generated    -> canonical source documents
superpowers  -> any repository evidence
```

反向约束：正式架构、模块、Control、Guide 和 Reference 不能把 `generated/` 或 `superpowers/` 当作唯一事实来源。

## 迁移与兼容

迁移使用 `git mv` 保留历史，并在同一阶段更新：

- Markdown 相对链接和导航索引。
- 根 `AGENTS.md` 与工程规范中的 Required Reading。
- 仓库 Skills 中硬编码的文档路径。
- LLMS Generator 配置、测试和输出目录。
- 源码注释、脚本和测试中的文档路径。

不保留内容重复的兼容副本。Git 历史承担旧路径追踪；仓库内所有引用必须在迁移提交中更新到新路径。

## 验证要求

重构完成后必须验证：

1. 所有正式目录具有明确入口和唯一职责。
2. 仓库内不存在旧路径引用和大小写不一致的文档路径。
3. Markdown 相对链接全部可解析。
4. Control `overview.md`、`implementation.md`、`token.md`、`changelog.md` 契约未被破坏。
5. LLMS Generator 使用新配置和新输出目录，生成输入覆盖不减少。
6. `docs/developments/` 结构保持不变。
7. `git diff --check` 通过。
8. LLMS Generator tests 和 verify 命令执行，并如实记录既有 stale output 或测试基线问题。
