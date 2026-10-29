# ButtonSpinner Token 设计

本文档定义 ButtonSpinner 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ButtonSpinner 整体架构见 [ButtonSpinner 桌面版架构设计](overview.md)，内部实现原理见 [ButtonSpinner 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ButtonSpinner Changelog](changelog.md)。

## 1. 定位

ButtonSpinner Own 表达手柄宽度、上下箭头尺寸及手柄背景/描边/状态颜色。根排版、圆角、输入表面和动效沿各自的有效 Global 与共享 frame 合同，不复制成这些 Own 字段。Token 不承载数值、草稿、选择、校验或 Spin 状态。

当前 Token scope：

- `ButtonSpinnerToken`，scope id 为 `ButtonSpinner`，源码位于 `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 手柄几何 | 上下手柄的宽度与箭头尺寸。 | `HandleWidth`、`HandleIconSize` |
| 手柄颜色 | 普通/Filled背景、分隔描边、hover图标与active背景；禁用规则由状态主题提供。 | `HandleBg`、`HandleActiveBg`、`HandleHoverColor`、`HandleBorderColor`、`FilledHandleBg` |
| 历史兼容资源 | 保留默认90、类型、key、公开编号、slot及配置/资源读写；内置布局不读取，不映射或别名到实例Width。 | `ControlWidth`（Legacy） |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

八项历史默认采用 DependencyDefaultsV2、声明修订1。`HandleWidth`默认读取有效 Global.ControlHeightSM，`HandleIconSize`默认读取有效 Global.FontSize/2；`HandleBg`、`HandleActiveBg`、`HandleHoverColor`、`HandleBorderColor`分别来自有效 Global.ColorBgContainer、ColorFillAlter、ColorPrimary、ColorBorder。

`FilledHandleBg`是唯一Own依赖：将有效Global.ColorFillSecondary叠加到有效Own.HandleBg，继续使用既有ColorUtils.OnBackground公式。显式HandleBg（含透明值）参与派生，显式FilledHandleBg优先，清除后按当前上游重算；两个输入均透明时结果透明。不按数值相等猜测显式存在状态。

## 3. 控件专项模型中的 Token 使用

ButtonSpinner 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 ButtonSpinner Token 时必须评估以下范围：

- `ButtonSpinner`
- `ButtonSpinnerHandle`
- `ButtonSpinnerDecoratedBox`
- `NumericUpDown`的Input/Spinner两种模式。Input的上下手柄读取这里的Own；Spinner模式的内嵌加减图标继续使用原Global.IconSize，按钮宽度保持随高度形成方形，不把HandleIconSize机械套到另一种图形职责。
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |

## 根几何与 Effective Global 消费

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 默认值从该 Control 的 Effective Global 读取，再以原生主题优先级投射到实际框体。共享框体继续负责状态色、focus/hover shadow、variant 与 CompactSpace；不能把它描述为跳过 owner 的全局几何入口。字号和其他消费链按各控件当前模板分别定义。本次未增加 Own 字段或改变 Token identity。详见 [共享输入架构的根几何合同](../../data-entry/input-control-architecture-design.md#81-根表面几何与预设默认值)。

附加区的背景/描边、padding/行高及内容面板间距消费实际owner的Effective Global；附加图标三档尺寸为IconSizeSM/IconSize/IconSizeLG，不新增Own字段。内部包装层的Custom只保留基础IconSize，显式尺寸优先；公开IconPresenter的画刷仍属于它自己的identity。NumericUpDown的内部spinner按外层NumericUpDown解析共享区域的Global，不能与独立公开ButtonSpinner混同。
