# AtomUI Mobile 文档规范

本文档定义 AtomUI Mobile 架构、模块、Control、平台工程和 Gallery 文档的责任边界、证据状态和同步要求。
通用目录与命名规则见 [文档结构与命名规范](documentation-structure-guidelines.md)，单 Control 文件契约见
[AtomUI Control 文档规范](control-documentation-guidelines.md)。本文只补充 Mobile 专项规则。

## 定位

AtomUI Mobile 文档同时承担两种责任：在源码实现前保存已经批准的目标契约，在源码实现后准确反映可用能力和验证证据。
这两种责任不能混写。目标设计可以约束 Foundation 和控件波次，但不能被表述为已存在的项目、API、平台支持或发布结果。

Mobile 文档遵守以下不变量：

- `AtomUI.Mobile.Controls` 与 `AtomUI.Desktop.Controls` 平行，Mobile 不依赖 Desktop。
- iOS 是首个实现平台，公共架构从 Foundation 开始同时考虑 iOS 和 Android。
- Mobile 复用 AtomUI Global Token、Theme Algorithm、`ThemeConfigProvider` 和 Localization 运行时。
- Safe Area、键盘遮挡、方向、打开状态、选择状态和 gesture progress 是运行时状态，不是 Token。
- 当前 `AtomUI.Toolkits.GalleryBase` 直接依赖 Desktop Controls，不作为 Mobile Gallery Foundation 的依赖。
- Desktop 与 Mobile Control 文档和 Gallery 证据分别拥有独立路径。

## 文档路由

| 内容 | 正式 owner | 不应放入 |
| --- | --- | --- |
| Mobile Runtime、Viewport、Overlay、Gesture 和跨平台能力契约 | `docs/architecture/systems/mobile/` | 单 Control 文档、平台环境指南 |
| `AtomUI.Mobile.Controls` 包、注册、源码 ownership、adapter 和打包 | `docs/modules/mobile-controls/` | Runtime 总体架构、单 Control API |
| Mobile 平台导航、分类兼容清单和具体 Control 契约 | `docs/controls/mobile/` | 签名流程、包注册、跨控件状态机 |
| iOS/Android 工具链、签名、构建、模拟器和真机 | `docs/engineering/platforms/` | Control API、Token 和 Gallery 内容模型 |
| Mobile Gallery 内容主体及 iOS/Android Host 责任 | `docs/gallery/platforms/mobile-gallery.md` | 完整平台工具链和 Control 契约 |
| 方案、实施计划和日期化验证证据 | `docs/superpowers/` | 正式架构和当前能力的唯一来源 |

稳定事实只由一个正式 owner 维护。其他入口保留摘要和链接，不复制完整状态机、平台矩阵、Token 规则或验证清单。

## 预实现架构规则

当 `AtomUI.Mobile.Controls` 尚未存在时，Mobile Architecture、Module、Controls 和 Gallery 正式入口使用以下 notice：

> 状态：预实现架构。本文定义已批准的目标契约，不表示当前仓库已经包含或发布 `AtomUI.Mobile.Controls`。

notice 只在文档入口出现一次。正文直接描述目标契约，但必须遵守：

- 不把目标项目列入当前解决方案项目表。
- 不展示虚构的 `.csproj`、源码文件清单、生成 API、测试项目或 Gallery route。
- 不使用“已经支持”“可直接使用”“已验证”“即将发布”等没有证据的措辞。
- Approved target API 名称必须明确属于目标契约；源码落地后再按真实声明校准。
- 一个入口只有在其描述的源码和验证证据实际存在后才能移除 notice，不能全局批量移除。

## 证据与状态模型

移动能力的状态按维度独立维护：

| 维度 | 初始状态 | 允许提升状态的证据 |
| --- | --- | --- |
| Design | `能力已纳入总体设计` | 已批准的系统或控件族设计 |
| Source | `未实现` | Public API、Theme、注册入口和实际项目源码 |
| iOS | `未验证` | 可追溯的 Simulator 或真机场景、日期和结果 |
| Android | `未验证` | 可追溯的 Emulator 或真机场景、日期和结果 |
| Release | `未验证` | 对应平台 Release/AOT/trim 产物构建并成功启动 |
| Publication | `未发布` | 可追溯的 NuGet 或应用版本与发布日期 |

状态不能互相替代：Headless 测试不等于平台验证，Simulator 不等于真机结论，compile 不等于 Release/AOT，iOS 证据
不等于 Android 证据。状态提升必须附证据链接；证据失效或契约变化时必须回退状态并说明重新验证条件。

正式平台文档保存稳定规则。精确 Xcode、SDK、设备、系统版本、机器路径、性能数据和视觉校准值进入日期化的
`docs/superpowers/progress/YYYY-MM-DD-<platform>-<scenario>-validation.md`。

## Mobile Control 分类与兼容清单

Mobile Control 使用七个分类：

| 分类 | 职责 |
| --- | --- |
| `general` | 高频基础操作和通用视觉原子 |
| `layout` | 空间、对齐、分区和安全区域 |
| `navigation` | 页面、分区、步骤和当前位置切换 |
| `data-entry` | 获取、编辑、选择、验证和提交数据 |
| `data-display` | 展示结构化内容、媒体、状态数据和集合 |
| `feedback` | 结果、进度、加载、确认和临时覆盖反馈 |
| `other` | 横跨多种分类且以移动交互基础能力为主的能力 |

Mobile 不创建 `window` 分类。系统栏、Safe Area、系统返回和生命周期属于 Mobile Runtime/Platform，不是 Desktop Window
Control 家族。

每项上游能力恰好由一个分类 `overview.md` 维护。兼容清单使用固定字段：

```text
Capability | AtomUI mapping | API strategy | Wave | Design | Source | iOS | Android | Release | Publication | Evidence
```

API strategy 只允许：

- `Tier 1 候选`：现有跨产品抽象可能完整承接公共状态和语义，仍需控件族设计确认。
- `Tier 2 候选`：共享 model/coordinator，Mobile Control 和 Theme 独立。
- `Tier 3`：移动语义占主导，设计独立 Mobile API。
- `波次设计未冻结`：总体能力已经纳入，但 API 复用层级必须由对应控件族设计决定。

分类页拥有逐项状态；平台入口只聚合分类数量和导航。单 Control 文档描述当前契约，不复制兼容清单的阶段状态。
上游 `ConfigProvider` 映射到现有 AtomUI Theme、Localization、启动 options、Control 属性、`DataTemplate` 和
`ControlTheme`，不作为第 83 个 Control 进入分类。

## 单 Control 文档创建门禁

不得预先批量创建空的 Mobile Control 目录。只有以下证据在同一变更中均可审查时，才创建
`docs/controls/mobile/<category>/<control>/`：

1. Control 或紧密 Control 家族已经存在可审查的 Public API 源码。
2. 已存在默认 ControlTheme，或有明确的无 Theme 设计证据。
3. 已存在 Contract/Headless 测试入口。
4. Mobile Gallery 已有结构化 API、Token 和稳定 ShowCase 的承载位置。
5. iOS 与 Android 的适用验证状态可以准确填写；未完成的平台明确为 `未验证`。

门禁满足后继续遵守标准文件契约：

```text
overview.md
implementation.md
token.md          # 有专属 Token 时创建
changelog.md
<topic>-design.md # 仅在稳定专项设计需要独立维护时创建
```

上游文档、截图、Demo 或总体设计不能替代 Public API、Theme、测试和 Gallery 证据。源码、主题、测试和文档冲突时，先
确定目标契约，再同步修正；不能选择最方便的一份作为事实。

## 平台文档规则

iOS 与 Android 使用平行的责任模型，但验证状态允许不同：

- iOS/Android 公共语义、Control API、Localization 文案和 Theme 契约保持一致。
- 平台差异通过 capability adapter 和 platform profile 表达，不散落在 Control 文档中。
- 工具链文档只记录稳定约束；版本组合必须有官方约束或真实执行证据。
- 未执行的命令不能标记为标准命令，未启动的产物不能标记为 Release 验证通过。
- Simulator/Emulator、真机、屏幕阅读器、IME、旋转、前后台和 Host 重建分别记录证据。
- iOS-first 不允许删除 Android 列、Android adapter 边界或 Android 验证要求。

## Gallery 隔离

Mobile Gallery 文档必须反映真实依赖：

- Mobile Gallery content 拥有 ShowCase、API table、Token table 和兼容状态。
- iOS Host 与 Android Host 只拥有平台启动、生命周期、签名/部署和 adapter 注册。
- 当前 Desktop-bound `AtomUI.Toolkits.GalleryBase` 不能作为 Mobile Foundation 依赖。
- 共享 Gallery contracts 只有在出现真实、稳定、无 Desktop 语义的重复后才能另行设计。


## 同步矩阵

| 变更 | 必须同步 |
| --- | --- |
| Mobile Runtime contract | Mobile Architecture、Module 摘要、受影响 Control 摘要、验证文档 |
| 包依赖、注册或 adapter | Module、dependency graph、runtime platforms、Native 边界、AOT/打包说明 |
| Public API 或行为 | Control overview/implementation/changelog、Gallery API/ShowCase、Contract tests、兼容状态 |
| Theme、Token 或 Semantic Part | Control overview/token/implementation、Theme tests、Gallery Token 表 |
| iOS/Android 证据 | 分类兼容清单、平台文档或日期化证据、Release 状态 |
| Gallery Host 结构 | Mobile Gallery 文档、平台入口、受影响 ShowCase ownership |

Foundation 和每个控件波次结束时，逐项检查 Architecture、Module、Controls、Engineering Platforms 和 Gallery 影响。没有影响的领域应明确为无变更，不能默认遗漏。

## 验证清单

- Mobile 当前状态和目标架构是否明确分开。
- 每个稳定事实是否只有一个正式 owner。
- 是否没有创建无源码/Theme/测试/Gallery 证据的单 Control 目录。
- 七个分类是否完整覆盖能力清单且没有重复。
- iOS、Android、Release 和 Publication 是否分别表达。
- Theme 文档是否复用现有全局系统，运行时状态是否保持在 Token 之外。
- Gallery 文档是否反映当前 Desktop-bound GalleryBase 依赖。
- 所有人工维护目录是否使用 `overview.md`。
- 相对链接、旧路径扫描和 `git diff --check` 是否通过。
