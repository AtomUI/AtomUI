# ButtonSpinner Token 设计

本文档定义 ButtonSpinner 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ButtonSpinner 整体架构见 [ButtonSpinner 桌面版架构设计](overview.md)，内部实现原理见 [ButtonSpinner 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ButtonSpinner Changelog](changelog.md)。

## 1. 定位

ButtonSpinner Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ButtonSpinnerToken`，scope id 为 `ButtonSpinner`，源码位于 `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ControlWidth`、`HandleWidth`、`HandleIconSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | 按源码 Token 语义维护 |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `HandleBg`、`HandleActiveBg`、`HandleHoverColor`、`HandleBorderColor`、`FilledHandleBg` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

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
