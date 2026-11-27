# AtomUI Mobile 文档架构设计

## 文档状态

- 设计日期：2026-08-11
- 设计状态：已完成方案评审，等待书面规格复核
- 适用范围：`AtomUI.Mobile.Controls`、Mobile Runtime、iOS/Android 工程环境、Mobile Gallery 与 Mobile LLMS
- 前置设计：[AtomUI Mobile Controls 总体设计](2026-08-10-atomui-mobile-controls-design.md)

本文定义 AtomUI Mobile 文档体系的长期信息架构、事实所有权、预实现状态表达、迁移范围和验证边界。本文是设计过程
记录，不是正式 Mobile 架构的唯一事实来源；实施完成后，稳定结论必须进入 `docs/architecture/`、`docs/modules/`、
`docs/controls/`、`docs/engineering/` 和 `docs/gallery/` 下的正式文档。

## 背景

AtomUI Mobile 已经确定采用与 `AtomUI.Desktop.Controls` 平行的 `AtomUI.Mobile.Controls` 产品包，首个实现平台为 iOS，
架构和公共 API 从 Foundation 开始同时支持 iOS 与 Android。当前仓库尚不存在 Mobile Controls 源码项目，但已经存在：

- Mobile Controls 总体设计。
- `docs/controls/mobile/overview.md` 占位入口。
- 一份混合环境配置、签名、资源、构建、模拟器、真机和排障的 446 行 iOS 文档。
- 面向 Desktop/Browser Gallery、并直接引用 `AtomUI.Desktop.Controls` 的 `AtomUI.Toolkits.GalleryBase`。
- 输出路径以 Desktop Control 名称为中心的 LLMS 配置和生成产物。

如果继续把所有移动端内容放进一个占位页或平台环境长文档，会混淆跨模块架构、源码包职责、单控件契约、宿主操作流程
和一次验证记录。Mobile 文档必须在源码批量实现前建立清晰边界，否则 82 个控件、两个平台和 Gallery/LLMS 配套内容会
快速形成重复事实和不可验证的支持声明。

## 目标

1. 为 Mobile Runtime、`AtomUI.Mobile.Controls`、具体 Mobile Control、iOS/Android 工程环境和 Mobile Gallery 建立唯一事实所有者。
2. 让文档明确区分已批准目标架构、当前源码可用性、平台验证和发布状态。
3. 复用 AtomUI 现有主题、Token、本地化、Control 文档和 LLMS 规范，不建立第二套文档协议。
4. 为后续 Foundation 与四个控件波次提供可以逐步填充、同步和验证的稳定目录。
5. 保持 Desktop 文档、Desktop LLMS 输出和现有 Gallery 文档路径兼容。
6. 把稳定平台规则与特定 Xcode、设备、版本和视觉校准证据分离。

## 非目标

- 本轮设计不创建 `AtomUI.Mobile.Controls` 源码项目或 Gallery Host。
- 本轮设计不实现 Mobile LLMS Generator 支持。
- 不在没有源码、Theme、测试和 Gallery 证据时创建 82 个空的单控件目录。
- 不复制 ant-design-mobile 官网文档、React API 或 CSS variable 表。
- 不把 iOS-first 解释为 Android 文档和验证可以无限期缺席。
- 不重写 Desktop Control 文档目录，也不移动现有 Desktop LLMS 输出。
- 不把 `docs/developments/` 作为正式架构、API 或平台规则的唯一来源。

## 设计原则

1. 一个稳定事实只有一个正式所有者，其他入口只保留摘要和链接。
2. 文档目录按知识职责组织，不按一次实施波次或当前源码文件数量组织。
3. 预实现架构可以进入正式文档，但必须明确说明它是已批准目标契约，不得暗示已有可用包。
4. 实现、平台验证和发布是不同证据维度，任何文档不得用“已支持”替代精确状态。
5. Mobile 复用 AtomUI Global Token、Theme Algorithm、`ThemeConfigProvider` 和 Localization 运行时，不定义独立全局主题引擎。
6. iOS 与 Android 使用平行文档结构；平台命令只有在真实环境验证后才能标记为已验证流程。
7. 单 Control 文档必须由真实实现证据驱动，不能先批量生成空壳。
8. Gallery 文档反映真实项目依赖，不能因为名称中有 `GalleryBase` 就宣称它当前可被 Mobile 复用。
9. Desktop 与 Mobile 的 LLMS 配置和输出具有独立所有权，同名 Control 不能发生目录碰撞。
10. 过程、候选方案、阶段计划和一次性验证数据留在 `docs/developments/`；正式文档只保留稳定结论。

## 事实所有权

| 文档区域 | 唯一职责 | 不承担的职责 |
| --- | --- | --- |
| `docs/architecture/systems/mobile/` | 跨 Core、Controls、Native、Mobile Controls 和平台宿主的运行时契约与不变量 | 单个包的源码索引、单控件 API、Xcode/Gradle 操作步骤 |
| `docs/modules/mobile-controls/` | `AtomUI.Mobile.Controls` 的依赖、注册、源码 ownership、平台 adapter 和打包边界 | 重复 Mobile Runtime 系统设计、列举所有控件 API |
| `docs/controls/mobile/` | Mobile 平台导航、分类目录和有真实证据的单 Control 文档 | 包注册、跨控件 Runtime 总体设计、平台签名流程 |
| `docs/engineering/contributing/` | Mobile 文档创建、证据、同步和完成标准 | 具体 Control 当前契约、平台环境快照 |
| `docs/engineering/platforms/apple-ios/` | 稳定的 iOS 环境、签名、资源、构建、模拟器、真机和排障规则 | Mobile Control 架构、一次设备校准值 |
| `docs/engineering/platforms/android/` | 稳定的 Android 环境、签名、资源、构建、模拟器、真机和排障规则 | 未验证命令、iOS 特有流程 |
| `docs/gallery/platforms/mobile-gallery.md` | Mobile Gallery 主体、iOS/Android Host、验证角色和与 GalleryBase 的边界 | Control API 真相、平台工具链完整教程 |
| `docs/engineering/tooling/llms-generator-design.md` | 多项目配置、输入输出、路径隔离和验证协议 | Mobile Control 内容本身、手工生成产物 |
| `docs/developments/specs/` | 方案比较、批准设计和范围决策 | 当前源码能力声明 |
| `docs/developments/plans/` | 可执行实施步骤、顺序和验证命令 | 长期架构契约 |
| `docs/developments/progress/` | 日期化的平台验证、设备快照、性能结果和阶段进度 | 跨版本稳定规则 |

## 目标目录

```text
docs/
|-- architecture/
|   `-- systems/
|       `-- mobile/
|           |-- overview.md
|           |-- runtime-and-platform-capabilities.md
|           |-- navigation-and-overlays.md
|           |-- gesture-coordination.md
|           |-- theming-localization-and-api.md
|           `-- verification.md
|-- modules/
|   `-- mobile-controls/
|       |-- overview.md
|       |-- source-ownership.md
|       |-- startup-and-packaging.md
|       `-- platform-adapters.md
|-- controls/
|   `-- mobile/
|       |-- overview.md
|       |-- general/
|       |   `-- overview.md
|       |-- layout/
|       |   `-- overview.md
|       |-- navigation/
|       |   `-- overview.md
|       |-- data-entry/
|       |   `-- overview.md
|       |-- data-display/
|       |   `-- overview.md
|       |-- feedback/
|       |   `-- overview.md
|       `-- other/
|           `-- overview.md
|-- engineering/
|   |-- contributing/
|   |   `-- mobile-documentation-guidelines.md
|   `-- platforms/
|       |-- overview.md
|       |-- apple-ios/
|       |   |-- overview.md
|       |   |-- toolchain-and-signing.md
|       |   |-- project-resources-and-launch.md
|       |   |-- simulator-and-device.md
|       |   |-- build-and-validation.md
|       |   `-- troubleshooting.md
|       `-- android/
|           |-- overview.md
|           |-- toolchain-and-signing.md
|           |-- emulator-and-device.md
|           |-- build-and-validation.md
|           `-- troubleshooting.md
|-- gallery/
|   `-- platforms/
|       |-- overview.md
|       `-- mobile-gallery.md
|-- AI/
|   `-- generated/
|       |-- llms.config.json
|       |-- llms/
|       |-- mobile-llms.config.json       # Foundation 阶段创建
|       `-- mobile-llms/                  # Foundation 阶段生成
`-- superpowers/
    |-- specs/
    |-- plans/
    `-- progress/
        `-- YYYY-MM-DD-<platform>-<scenario>-validation.md
```

正式目录的入口统一使用 `overview.md`。`index-cn.md` 只属于 LLMS 生成物，不能用于人工维护的目录入口。

## Mobile 系统架构文档

### `overview.md`

系统入口只保留 Mobile 的设计定位、跨模块边界、总体依赖图、核心运行对象和阅读顺序。它必须明确：

- `AtomUI.Mobile.Controls` 与 `AtomUI.Desktop.Controls` 平行，不互相依赖。
- `AtomUI.Native` 提供能力，不拥有 Control 策略、Token 或默认视觉。
- 每个 `TopLevel` 的 `MobileViewportContext`、`MobileOverlayManager` 和手势协调职责。
- iOS-first 是实施顺序，不是公共架构只支持 iOS。
- 当前包是否存在，以及本文哪些内容属于预实现目标契约。

详细算法、平台矩阵和验证表必须链接到专题文件，不能全部堆回入口。

### `runtime-and-platform-capabilities.md`

该文档拥有 `MobileViewportContext`、Safe Area、系统栏、input pane、方向、display scale、字体缩放、Reduce Motion、
前后台通知和 capability contract。它定义状态 owner、更新顺序、UI thread 边界、TopLevel attach/detach 和能力缺失时的
退化，不记录具体 iOS/Android API 调用代码。

### `navigation-and-overlays.md`

该文档拥有基于 Avalonia Page 体系的导航事务、系统返回优先级、Overlay session 状态机、z-order、mask、focus、取消、
关闭原因和 Host detach 规则。单个 Popup、Dialog 或 Toast 文档只能描述自身 Public API 如何消费这套系统，不能复制
状态机。

### `gesture-coordination.md`

该文档拥有方向锁定、pointer ownership、nested-scroll handoff、速度、rubberband、snap point、多指、取消和 Reduce Motion
降级。Swiper、SwipeAction、FloatingPanel、PullToRefresh 和 ImageViewer 文档引用该契约，并只补充控件自己的 gesture
policy。

### `theming-localization-and-api.md`

该文档拥有 Mobile 对 AtomUI 全局系统的映射：

| 能力 | Mobile 设计 |
| --- | --- |
| Global Token | 直接复用现有 AtomUI Global Token |
| Theme Algorithm | 直接复用现有 Theme Algorithm |
| 局部主题作用域 | 复用 `ThemeConfigProvider` 和资源作用域 |
| Mobile 共享语义 | 只有至少三个 Mobile Control 共享且应用级覆盖有价值时才新增 Mobile Alias Token |
| 单控件差异 | 使用 Mobile Control Own Token |
| 运行时状态 | Safe Area、键盘遮挡、方向、打开状态和 gesture progress 不进入 Token |
| Localization | 复用现有 Catalog、Snapshot、fallback 和 Generator；Mobile 包拥有自己的文案资源 |
| 上游 `ConfigProvider` | 映射到现有主题、本地化、启动 options、Control 属性、DataTemplate 和 ControlTheme，不复制同名控件 |

该文档同时维护 React/Web 语义到 Avalonia API 的稳定映射原则，但不冻结尚未完成控件族设计的全部属性名。

### `verification.md`

该文档拥有 Contract、Headless、Visual、Platform、Release 五层验证模型，以及双平台、无障碍、IME、Viewport、资源释放、
性能和 AOT 门禁。具体测试命令和项目路径只有在相应工程存在后才能写入；预实现阶段先定义必须证明的不变量。

## Mobile Controls 模块文档

`docs/modules/mobile-controls/` 只对应未来 `src/AtomUI.Mobile.Controls` 项目和 NuGet 包。

| 文件 | 内容 |
| --- | --- |
| `overview.md` | 模块职责、非职责、上游依赖、下游宿主、注册入口摘要和专题导航 |
| `source-ownership.md` | 稳定源码目录、Runtime/Controls/Themes/Localization/Platform 等 owner、internal/public 边界 |
| `startup-and-packaging.md` | `UseMobileControls`、Generator 资产注册、多目标框架、NuGet 依赖、AOT/trimming 和发布边界 |
| `platform-adapters.md` | capability contract、Headless/iOS/Android adapter 的实现归属、生命周期和缺失能力处理 |

模块文档在源码项目不存在时使用预实现状态说明，不展示虚构的 `.csproj`、namespace 文件清单或已生成 API。Foundation
落地后，文件结构必须根据真实源码同步，不允许让总体设计中的候选名称长期冒充实现事实。

## Mobile Control 文档

### 平台入口

`docs/controls/mobile/overview.md` 从占位页升级为 Mobile Control 文档导航，包含：

- 当前可用性和证据状态。
- 七个分类入口。
- 单 Control 文档创建条件。
- 与 Mobile 系统架构、Mobile Controls 模块、Gallery 和平台工程文档的边界。
- 82 个上游控件能力和 `ConfigProvider` 职责映射的总数、范围与分类导航，不在平台入口维护第二份逐项状态表。

### 分类规则

Mobile 使用与 Desktop 对齐的七个通用分类：`general`、`layout`、`navigation`、`data-entry`、`data-display`、
`feedback`、`other`。Mobile 不创建 `window` 分类，因为移动宿主的系统栏、Safe Area、返回和生命周期属于 Runtime/Platform
能力，而不是面向应用的 Desktop Window Control 家族。

| 分类 | 放置标准 | 代表能力 |
| --- | --- | --- |
| `general` | 高频基础操作和通用视觉原子 | Button、Avatar、Tag |
| `layout` | 只负责空间、对齐、分区和安全区域 | AutoCenter、Grid、Space、SafeArea |
| `navigation` | 表达页面、分区、步骤或当前位置切换 | NavBar、TabBar、Tabs、SideBar、Steps |
| `data-entry` | 获取、编辑、选择、验证或提交用户数据 | Input、Form、Picker、Checkbox、Slider |
| `data-display` | 展示结构化内容、媒体、状态数据或集合 | Card、List、Calendar、Swiper、ImageViewer |
| `feedback` | 告知结果、进度、加载、确认或临时覆盖反馈 | Dialog、Toast、Loading、Progress、Result |
| `other` | 横跨多种分类且以移动交互基础能力为主 | PullToRefresh、SwipeAction、InfiniteScroll |

一个 Control 家族只进入一个主分类。Picker/View、Cascader/View、Calendar/Picker/View 等紧密家族保持同目录；分类页可以
交叉链接，但不得复制单控件契约。具体归类以创建第一份真实控件文档时评审的主用户职责为准，并在兼容清单中保持稳定。

七个分类 `overview.md` 分区拥有完整兼容清单。每个上游能力恰好由一个分类页维护以下字段：上游名称、AtomUI Mobile
对应 Control 或职责映射、API 复用 Tier、实施波次、设计/源码/iOS/Android/Release/发布状态和证据链接。平台入口只聚合
分类计数和链接；单 Control 文档只解释当前契约，不复制兼容状态历史。`ConfigProvider` 作为系统职责映射记录在平台入口，
并链接 `theming-localization-and-api.md`，不放入任何 Control 分类。

### 单 Control 文档创建门禁

不得预先创建 82 个空目录。只有同时满足下列条件，才创建
`docs/controls/mobile/<category>/<control>/`：

1. Control 或紧密 Control 家族已经有可审查的 Public API 源码。
2. 至少存在默认 ControlTheme 或明确的无 Theme 设计证据。
3. 至少存在 Contract/Headless 测试入口。
4. Mobile Gallery 中已经有 API、Token 和稳定 ShowCase 的承载位置。
5. iOS 与 Android 的适用验证状态能够被准确描述；未完成的平台必须明确标记，不得省略。

创建后遵循现有单 Control 文件契约：

```text
overview.md
implementation.md
token.md          # 有专属 Token 时创建
changelog.md
<topic>-design.md # 仅在稳定专项设计确实需要独立维护时创建
```

源码、Theme、测试或 Gallery 只满足一部分时，分类 `overview.md` 的计划清单可以记录该能力，但不能创建内容空洞的
`implementation.md`、`token.md` 或 `changelog.md`。

## Mobile 文档贡献规范

新增 `docs/engineering/contributing/mobile-documentation-guidelines.md`，它补充而不复制现有
[文档结构与命名规范](../../engineering/contributing/documentation-structure-guidelines.md)和
[Control 文档规范](../../engineering/contributing/control-documentation-guidelines.md)。该规范必须定义：

- Mobile 系统、模块、Control、平台和 Gallery 文档的路由表。
- 预实现状态 notice 的固定语义。
- 单 Control 文档证据门禁与同步矩阵。
- iOS/Android 平台声明规则。
- 上游 ant-design-mobile 资料只能作为能力映射输入，不能覆盖 AtomUI 源码事实。
- 同名 Desktop/Mobile Control 的文档、Gallery 和 LLMS 路径隔离。
- Foundation 和每个控件波次完成时必须同步的正式文档范围。

全局 Control 文档规范只增加 Mobile 专项规范入口和平台无关的共同规则，不复制 Mobile 目录树全文。

## 当前可用性与证据表达

### 预实现 notice

在 `AtomUI.Mobile.Controls` 尚不存在时，所有正式 Mobile Architecture、Module、Controls 和 Gallery 入口顶部使用一致语义：

> 状态：预实现架构。本文定义已批准的目标契约，不表示当前仓库已经包含或发布 `AtomUI.Mobile.Controls`。

notice 不进入每个普通段落，也不使用“即将”“很快”等无法验证的措辞。对应源码完成后，只能在实际证据覆盖的文档中
移除 notice，不能一次性全局删除。

### 状态维度

移动端能力使用下列独立状态，不合并成一个百分比或单一“完成”字段：

| 维度 | 可以声明的证据 |
| --- | --- |
| 设计 | 已批准架构或控件族设计 |
| 源码 | 项目、Public API、Theme 与注册入口实际存在 |
| iOS 验证 | 指定 Simulator/真机场景和日期化证据存在 |
| Android 验证 | 指定 Emulator/真机场景和日期化证据存在 |
| Release | 对应平台 Release/AOT/trim 产物已构建并启动 |
| 发布 | NuGet/应用版本和发布时间可追溯 |

“跨平台支持”“iOS 可用”“Android 可用”和“已发布”只能在相应证据存在时使用。Headless 测试不能替代平台验证，iOS
Simulator 不能替代 iOS 真机结论，Android compile 不能替代 Emulator 或真机行为结论。

## iOS 文档拆分

`docs/engineering/platforms/overview.md` 作为工程平台文档入口，导航 iOS、Android、Windows/Linux NativeAOT 等平台资料，
不复制各平台工具链步骤。

现有 `docs/engineering/platforms/apple-ios-development-environment.md` 先通过 `git mv` 移至
`docs/engineering/platforms/apple-ios/overview.md`，再从中提取专题文件，以保留主要文件历史。

| 目标文件 | 稳定内容 |
| --- | --- |
| `overview.md` | 适用范围、文档导航、最小环境模型、验证层级和不覆盖范围 |
| `toolchain-and-signing.md` | Xcode/.NET workload、Command Line Tools、证书、App ID、profile、keychain 和 CI 边界 |
| `project-resources-and-launch.md` | iOS Host 目录、AppDelegate、Info.plist、asset catalog、AppIcon、LaunchScreen 和 Avalonia resource URI |
| `simulator-and-device.md` | Simulator/真机发现、安装、启动、截图、锁屏和设备配对规则 |
| `build-and-validation.md` | Debug/Release、RID、签名参数、AOT、产物检查和启动性能判断 |
| `troubleshooting.md` | 稳定错误模式、诊断顺序和已验证修复原则 |

精确 Xcode/.NET/Avalonia/iOS 版本、设备型号、证书名称、Bundle ID、构建耗时和 LaunchScreen 的 `0.31`、`44.1`
等校准值不属于稳定规则。它们移动到日期化的
`docs/developments/progress/YYYY-MM-DD-apple-ios-<scenario>-validation.md`。正式文档可以链接最近证据，但不能把某次
机器快照写成长期最低版本或全设备通用公式。

拆分时不保留旧路径兼容副本。仓库内链接、`AGENTS.md` Required Reading 和 Skills 中的硬编码路径必须在同一迁移中更新。

## Android 文档架构

Android 与 iOS 使用平行责任模型，但不为了目录对称而复制未经验证的命令。

| 文件 | 内容 |
| --- | --- |
| `overview.md` | 适用范围、Host 边界、阅读顺序、当前验证状态 |
| `toolchain-and-signing.md` | .NET Android workload、SDK/JDK、签名身份、keystore 和 CI 责任 |
| `emulator-and-device.md` | AVD、adb、安装、启动、日志、截图和真机连接 |
| `build-and-validation.md` | Debug/Release、RID/ABI、trim/AOT、包产物和启动验证 |
| `troubleshooting.md` | 已复现的工具链、签名、部署、Activity 生命周期和资源问题 |

初始 Android 文档可以定义需要验证的契约和证据清单，但具体命令、版本组合和成功结论必须来自官方工具约束或真实执行
证据。未验证内容使用“验证要求”表达，不使用“标准命令”“已支持”或“推荐版本”措辞。

## Mobile Gallery 文档

`docs/gallery/platforms/overview.md` 作为 Gallery 平台宿主文档入口，区分 Desktop、Browser、iOS 和 Android 的宿主责任，
不拥有具体 ShowCase 或平台工具链契约。

`docs/gallery/platforms/mobile-gallery.md` 定义未来 Gallery 的三层 ownership：

```text
Mobile Gallery content
  -> ShowCases / API tables / Token tables / compatibility status
  -> iOS Host
  -> Android Host
```

当前 `AtomUI.Toolkits.GalleryBase` 直接引用 `AtomUI.Desktop.Controls`，因此 Foundation 不允许 Mobile Gallery 引用它。
Mobile Gallery 第一阶段使用独立、最小的移动 Shell 和平台 Host。现有 GalleryBase 文档必须同步说明：它目前是 Desktop/
Browser Gallery 的产品中立工具层，但依赖边界仍然是 Desktop-bound，不能作为 Mobile 的现成共享层。

只有 Mobile Gallery 与现有 GalleryBase 出现真实、稳定、无 Desktop 语义的重复后，才为共享 Gallery contracts 编写独立设计。
该设计可以抽取新的低层包或重构现有包，但不属于本次文档架构或 Mobile Foundation 的预设前提。

Desktop Gallery 不承担 Mobile 最终验收。Headless 或 Desktop 开发预览可以作为辅助证据，但 Safe Area、返回、IME、触摸、
前后台和屏幕阅读器结论必须来自 iOS/Android Host。

## Mobile LLMS 隔离

当前 Desktop 配置拥有 `docs/AI/generated/llms/`，其中单控件输出使用
`controls/<control>/index-cn.md` 和 `semantic-cn.md`。如果 Mobile 直接写入同一根目录，同名 Button、Avatar、List 等会发生
路径碰撞。

Foundation 阶段采用独立配置和独立输出根：

```text
Desktop config:  docs/AI/generated/llms.config.json
Desktop output:  docs/AI/generated/llms/

Mobile config:   docs/AI/generated/mobile-llms.config.json
Mobile output:   docs/AI/generated/mobile-llms/
```

不把 Mobile 输出嵌套到现有 `docs/AI/generated/llms/mobile/`，因为 Desktop verifier 当前拥有整个 Desktop `outputRoot`，
嵌套目录会模糊 stale-file ownership。两个配置使用不同 `projectId`、`docsRoot`、`galleryRoot`、`sourceRoots` 和输出根，
分别 generate/verify；Desktop 的现有路径保持不变。

本次文档实施只更新设计文档中的扩展边界，不创建配置、生成产物或修改工具代码。Mobile LLMS 接入属于 Foundation
实施计划；只有真实单 Control 文档满足输入契约后才进入生成清单。

## 正式文档同步矩阵

| 现有文档 | 实施时的同步内容 |
| --- | --- |
| `AGENTS.md` | 增加 Mobile 架构、Mobile 文档规范和新的 iOS/Android 平台入口；替换旧 iOS 路径 |
| `docs/overview.md` | 增加 Mobile 系统、模块、Control 和平台阅读入口 |
| `docs/architecture/overview.md` | 在总体分层和依赖图中加入 Mobile Controls 与移动宿主，并标注当前可用性 |
| `docs/architecture/systems/overview.md` | 增加 Mobile 系统入口和与 Theming/Localization/Rendering 的边界 |
| `docs/architecture/foundations/runtime-platforms.md` | 增加 iOS/Android host、capability adapter 和平台验证责任 |
| `docs/architecture/foundations/dependency-graph.md` | 区分当前项目图和已批准目标依赖；不得把未创建项目列为当前解决方案成员 |
| `docs/modules/overview.md` | 增加 Mobile Controls 模块入口并标记预实现状态 |
| `docs/modules/native/overview.md` | 增加 Mobile capability consumer 导航，不让 Native 拥有产品策略 |
| `docs/modules/native/architecture.md` | 补充未来 Mobile adapter 的 internal 边界和生命周期职责 |
| `docs/controls/overview.md` | 把 Mobile 从空占位说明升级为正式平台导航 |
| `docs/controls/mobile/overview.md` | 重写为状态、分类、证据门禁和跨文档导航入口 |
| `docs/engineering/overview.md` | 增加 Mobile 文档规范、iOS 和 Android 平台入口 |
| `docs/gallery/overview.md` | 增加 Mobile Gallery 入口并区分 Desktop/Browser 与 Mobile Host |
| `docs/modules/toolkits-gallery-base/overview.md` | 明确当前项目直接依赖 Desktop Controls，不作为 Mobile Foundation 依赖 |
| `docs/modules/toolkits-gallery-base/architecture.md` | 修正“产品中立”与实际 Desktop-bound 依赖之间的边界表述 |
| `docs/engineering/contributing/control-documentation-guidelines.md` | 链接 Mobile 专项规范，补充多平台证据和同名 Control 隔离原则 |
| `docs/engineering/tooling/llms-generator-design.md` | 定义 Desktop/Mobile 独立配置和输出 ownership；实现状态保持准确 |
| `.agents/skills/atomui-control-documentation/SKILL.md` | 增加 Mobile 专项规范读取条件和证据门禁，避免技能批量创建空目录 |

同步只修改与 Mobile 边界直接相关的段落，不借机重构其他系统文档。

## 实施阶段

### Phase 1：正式架构和导航

创建 Mobile 系统架构、Mobile Controls 模块、Mobile Control 分类入口、Mobile 文档贡献规范和 Mobile Gallery 文档。所有
预实现页面使用统一 notice，目录入口使用 `overview.md`；同时补齐 `docs/gallery/platforms/overview.md`。

### Phase 2：平台文档迁移

使用 `git mv` 迁移并拆分 iOS 长文档，提取日期化验证数据；建立 Android 平行文档，严格区分已验证流程和验证要求。
同时补齐 `docs/engineering/platforms/overview.md`，作为工程平台文档的统一导航。

### Phase 3：全局同步

按同步矩阵更新根导航、架构、模块、Native、Controls、Engineering、GalleryBase、Gallery、Control 文档规范、LLMS 设计和
Agent Skill。该阶段仍不修改运行时源码、LLMS 工具或生成产物。

### Phase 4：文档验证

检查目录入口、相对链接、旧路径引用、预实现状态、重复事实、空 Control 目录和 Git whitespace。确认 Desktop 文档及 LLMS
输出路径没有被移动。

### Phase 5：Foundation 后续同步

Foundation 源码实现时再移除有证据页面的预实现 notice，补齐真实源码索引、启动入口、平台 adapter、Gallery Host、测试
命令和 Mobile LLMS 配置。后续每个控件波次按证据门禁逐个创建 Control 文档。

## 验证要求

文档架构实施至少执行：

```bash
git diff --check
rg -n "apple-ios-development-environment\.md" AGENTS.md docs .agents \
  --glob '!docs/developments/**'
find docs/architecture/systems/mobile docs/modules/mobile-controls docs/controls/mobile \
  docs/engineering/platforms/apple-ios docs/engineering/platforms/android -type d \
  ! -exec test -f '{}/overview.md' ';' -print
test -f docs/engineering/platforms/overview.md
test -f docs/gallery/platforms/overview.md
dotnet run --project tools/AtomUI.Docs.LLMsGenerator/AtomUI.Docs.LLMsGenerator.csproj \
  -- verify --config docs/AI/generated/llms.config.json
```

还必须人工确认：

1. 所有新建的人工维护目录都有 `overview.md`，没有人工维护的 `index.md`。
2. 正式 Mobile 文档不把总体设计 spec 当作唯一事实来源。
3. 没有创建无源码证据的单 Control 目录。
4. `AtomUI.Mobile.Controls` 未实现时，没有文档声称包已存在、已发布或已完成双平台验证。
5. iOS 稳定规则与日期化环境/设备/校准证据已分离。
6. Android 未验证命令没有被描述为标准流程。
7. Mobile Gallery 没有依赖当前 Desktop-bound GalleryBase 的设计承诺。
8. Mobile LLMS 与 Desktop LLMS 的 config、outputRoot 和同名 Control 路径不会碰撞。
9. 主题文档明确复用 AtomUI Global Token、Theme Algorithm 和 `ThemeConfigProvider`，没有第二套 Mobile 全局主题引擎。
10. 所有链接、导航和 Required Reading 指向新路径，不保留重复兼容文档。

## 最终不变量

1. `architecture/systems/mobile` 拥有跨模块 Mobile Runtime 契约。
2. `modules/mobile-controls` 只拥有 `AtomUI.Mobile.Controls` 项目和包的实现边界。
3. `controls/mobile` 只为有源码、Theme、测试和 Gallery 证据的 Control 创建完整文档。
4. iOS 与 Android 使用平行文档责任，但验证状态可以不同且必须如实表达。
5. Mobile 复用 AtomUI 的 Global Token、Theme Algorithm、`ThemeConfigProvider` 和 Localization 系统。
6. 运行时 Safe Area、键盘、方向、打开状态和手势进度不写成 Token。
7. 当前 `AtomUI.Toolkits.GalleryBase` 不被 Mobile Gallery 引用。
8. Desktop 与 Mobile LLMS 配置和输出根独立，同名 Control 不碰撞。
9. 正式规则、过程计划、日期化证据和生成产物分别由正式 docs、superpowers 和 generated 目录拥有。
10. 文档中的“实现”“验证”“Release”“发布”都有独立、可追溯证据。
