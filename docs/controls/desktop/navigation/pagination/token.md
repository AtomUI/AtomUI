# Pagination Token 设计

本文档定义 Pagination 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Pagination 整体架构见 [Pagination 桌面版架构设计](overview.md)，内部实现原理见 [Pagination 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Pagination Changelog](changelog.md)。

## 1. 定位

Pagination Own 只表达页码条目、布局和跳转输入的视觉变量，不保存当前页、总数、页大小、输入草稿、焦点或导航状态。

当前 Token scope：

- `PaginationToken`，identity 为 `AtomUI:Pagination`，14项字段，源码位于 `src/AtomUI.Desktop.Controls/Pagination/PaginationToken.cs`。

`SimplePagination` 有独立 Global identity、没有 Own；现有模板显式复用 Pagination Own 的尺寸／间距及输入背景，不把 CLR 继承当作 identity 继承。

## 2. Token 分类

- 页码颜色：`ItemBg` 为普通页码背景、`ItemActiveBg` 为选中背景，均默认读取有效 Global.ColorBgContainer；`ItemActiveBgDisabled` 读取 ControlItemBgActiveDisabled，`ItemActiveColorDisabled` 读取 ColorTextDisabled。前后导航、跳页图标及其他状态继续保留各自 Shared 资源职责。
- 尺寸：`ItemSize` 读取 ControlHeight，供 Large／Middle／Custom 页码使用；`ItemSizeSM` 读取 ControlHeightSM。`PaginationQuickJumperInputWidth`=ControlHeightLG×1.25，供非Small跳转输入使用；`PaginationMiniQuickJumperInputWidth`=ControlHeightLG×1.1，供Small使用。普通跳转设置Width；简洁编辑输入设置MinWidth，允许内容变宽。
- 间距：`PaginationLayoutSpacing`=UniformlyMarginXS，`PaginationLayoutMiniSpacing`=UniformlyMarginXXS/2。`PaginationItemPaddingInline` 保留当前 `new Thickness(UniformlyMarginXXS×1.5)`，实际是四边相同，不因名称Inline改动公式。尺寸／间距单位为DIP，明确0保留。
- 简洁输入背景：`ItemInputBg` 读取有效 Pagination Global.ColorBgContainer，消费范围为Simple编辑输入的enabled／Outlined绘制帧。disabled及其他输入变体保持原LineEdit规则；该字段不改变普通QuickJumperBar输入、焦点边框或阴影。

兼容保留两项：`ItemLinkBg` 保留ColorBgContainer默认，当前没有bordered导航入口或默认消费者；`InputOutlineOffset` 保留Thickness(0)，未定义新的桌面偏移语义，焦点继续使用有效Global.InputActiveShadow路线。保留名称、类型、资源与配置往返，不将其标为当前有效编辑项，不新增变体或偏移API。

## 3. 控件专项模型中的 Token 使用

14项以 DependencyDefaultsV2／Revision1 的静态默认函数求值，只读取有效Global或常量，没有Own→Own依赖。Control专属Global进入Pagination有效输入，明确Own优先；删除配置后恢复当前Global派生值，保留所有key、枚举数值与dense slot。

页码与布局仍由原ControlTheme消费者承担。Small宽度在现有模板样式中选择Mini资源。Simple输入背景声明在既有QuickJumpEdit实例的AXAML Styles内，以一层模板选择器到达AddOnDecoratedBox；保留LineEdit隐式主题、StyleKey及禁用／焦点规则，不通过代码取得节点回写属性。

## 4. 控件家族影响

调整 Pagination Token 时必须评估以下范围：

- `Pagination`
- `SimplePagination`
- `AbstractPagination`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 保持14项默认公式及资源身份；兼容保留不等于删除字段、弃用API或移除配置能力。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 核对真实Compiler明暗28默认、原始／投射类型、key与slot，覆盖Global／Control Global／14Own／0／撤回。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 核对四尺寸、普通／简洁、只读／禁用、输入／焦点及跳页；自然A/A、1DIP、新旧和恢复分别记录。 |

Headless、实际原生窗口、NativeAOT与其他平台分别验收；不把资源通过或单个fixture成功作为完整分页家族／原生最终像素通过。
