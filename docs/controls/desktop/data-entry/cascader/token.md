# Cascader Token 设计

Cascader 的 12 个 Own 字段使用 DependencyDefaultsV2、声明修订 1。默认函数只读取有效 Global，显式 Own 值（包括 0 和透明色）保留存在状态，撤回后按当前 Global 重新求值。名称、类型、资源 key、公开编号和 slot 保持兼容。ControlWidth、ControlItemWidth、DropdownHeight 的常量默认分别为 184、111、180；OptionPadding 继续使用 Round((ControlHeight-FontHeight)/2)，不改变取整顺序。

箭头组合尺寸消费Cascader有效Global.IconSize，常规各尺寸仍14；0与清除更新真实模板。颜色继续沿有效ColorTextQuaternary/专用Suffix Style继承，清除按钮已有owner入口不变；不能因复用SelectHandle而读取Select或公开IconPresenter自身的尺寸配置。公开Form反馈与箭头的间距读取Cascader有效Global.SpacingXS，预设最小高度与自然内容测量保持。

多选标签的横向/行间距及响应式面板间距消费 Cascader 有效 Global.SpacingXXS；数量指示文字消费 ColorTextDescription。0与清除按现有资源更新，不新增 Own 字段，也不因复用 Select 内部资产而读取 Select identity 的同名配置。

本文档记录 Cascader 专属 Token 的语义、分类和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计见 [Cascader 桌面版架构设计](overview.md)，实现原理见 [Cascader 桌面版实现原理](implementation.md)，变化记录见 [Cascader Changelog](changelog.md)。通用 Token 规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

`CascaderToken` 描述 Cascader 输入弹层和级联选项的尺寸、列宽、选项状态色、padding、过滤高亮和内部元素间距。它只表达控件主题常量，不表达实例选择状态、当前展开路径、当前过滤值、当前 loading 状态或最大选择数量状态。

## 2. Token 分类

| 分类 | Token | 语义 | 主要消费位置 |
| --- | --- | --- | --- |
| 尺寸 | `ControlWidth` | HorizontalAlignment 非 Stretch 时的 Cascader 主题宽度，实例 Width 可覆盖。 | `CascaderTheme.axaml` root style。 |
| 尺寸 | `ControlItemWidth` | 单列选项默认宽度，也是普通 popup 最小宽度基线。 | `CascaderTheme.axaml`、level list 主题。 |
| 尺寸 | `DropdownHeight` | PopupFrame 的主题最大高度，默认 180；0 表示零高度上限。 | `CascaderTheme.axaml` 的 Frame Style Setter。 |
| 尺寸 | `HeaderHeight` | 选项 header 最小高度。 | `CascaderViewItemTheme.axaml`。 |
| 间距 | `OptionPadding` | 选项内部 padding。 | `CascaderViewItemTheme.axaml`。 |
| 间距 | `MenuPadding` | 单列菜单 padding。 | `CascaderViewLevelListTheme.axaml`。 |
| 间距 | `ItemHeaderSpacing` | checkbox、icon、header、expand icon 之间的列间距。 | `CascaderViewItemTheme.axaml`。 |
| 状态色 | `OptionHoverBg` | pointerover 选项背景。 | `CascaderViewItemTheme.axaml`。 |
| 状态色 | `OptionSelectedBg` | expanded / selected 选项背景。 | `CascaderViewItemTheme.axaml`。 |
| 状态色 | `OptionSelectedColor` | expanded / selected 选项文本色。 | `CascaderViewItemTheme.axaml`。 |
| 字重 | `OptionSelectedFontWeight` | expanded / selected 选项字重。 | `CascaderViewItemTheme.axaml`。 |
| 过滤 | `FilterHighlightColor` | 过滤命中文本高亮色。 | `CascaderTheme.axaml`、`CascaderViewTheme.axaml`。 |

## 3. 控件专项模型中的 Token 使用

`CascaderTheme.axaml` 使用 `ControlWidth`、`ControlItemWidth`、`DropdownHeight` 和 `FilterHighlightColor` 定义外层输入宽度、普通 popup 最小宽度、最大高度和过滤高亮。

Popup 的最大高度另受名义行高 × DisplayPageSize + popup 上下 padding 的预算约束。两个约束同时参与自然布局，较小者限制实际高度；内容较少时自然收缩。Own 上限位于 ControlTheme Style Setter，专用 CascaderPopupRootStyle 可按原生优先级覆盖合并后的主题默认。

`CascaderViewLevelListTheme.axaml` 使用 `ControlItemWidth` 和 `MenuPadding` 定义每列的结构尺寸。级联列宽属于 Cascader 的核心视觉契约，不应被单个 item 模板硬编码覆盖。

`CascaderViewItemTheme.axaml` 使用 `HeaderHeight`、`OptionPadding` 和 `ItemHeaderSpacing` 决定选项高度、内部留白和列间距。`OptionHoverBg`、`OptionSelectedBg`、`OptionSelectedColor` 和 `OptionSelectedFontWeight` 只服务选项交互状态。

`CascaderViewTheme.axaml` 使用 CascaderToken 与 SharedToken 协作完成空状态、滚动和过滤结果视觉。

## 4. 控件家族影响

Cascader 复用输入家族的 `InputControlFrame` / AddOnDecoratedBox、SelectHandle、SelectTagAwareTextBox、PopupHost 和 Form feedback 视觉。输入表面的高度、边框、状态色、focus 视觉、placeholder 和 `Custom` 尺寸基线来自输入家族共享规则，不在 `CascaderToken` 中重复定义。

Popup 阴影、popup 圆角和 anchor margin 使用 `PopupHostToken`。CascaderToken 只定义 Cascader 自己的列宽、高度、选项状态和过滤高亮，不接管 PopupHost 的通用视觉。

## 5. 兼容性要求

- Token 名称、类型和语义不得在未授权情况下改变。
- 不把运行时选择状态、当前路径、过滤文本、最大选择数量状态或 loading 状态写成 Token。
- `Custom` 尺寸不新增 Cascader 专属 Token 分支，未显式覆盖时继续使用输入家族的 `Middle` 基线。
- 选项高度和 padding 变化必须同时验证 checkbox、icon、header、展开图标和 loading 图标对齐。
- 列宽变化必须同时验证普通级联列、过滤结果、空状态和 `IsPopupMatchSelectWidth`。
- 过滤高亮色变化必须验证 light / dark theme 下的文本可读性。

## 6. 验证策略

Token 或主题改动至少验证：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Cascader
dotnet build controlgallery/AtomUIGallery/AtomUIGallery.csproj --no-restore
git diff --check
```

视觉走查重点：

- Large / Middle / Small / Custom 下输入高度、tag 高度和选项行高度。
- 单选、多选、过滤、空状态和异步 loading popup。
- disabled、pointerover、expanded、checked 和 max count reached 状态。

## 根几何与 Effective Global 消费

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 默认值从该 Control 的 Effective Global 读取，再以原生主题优先级投射到实际框体。共享框体继续负责状态色、focus/hover shadow、variant 与 CompactSpace；不能把它描述为跳过 owner 的全局几何入口。字号和其他消费链按各控件当前模板分别定义。本次未增加 Own 字段或改变 Token identity。详见 [共享输入架构的根几何合同](../input-control-architecture-design.md#81-根表面几何与预设默认值)。

内部 add-on 的通用状态颜色使用 Cascader 有效 Global：`ColorBorder`、`ColorError`/`ColorWarning` 与对应 BorderHover、`ColorFillTertiary`/`ColorFillSecondary`、Error/Warning 的 Bg/BgHover。它们不属于新增 Own；主题对原键的选择和状态条件保持。
