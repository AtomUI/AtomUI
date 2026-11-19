# Cascader Token 设计

本文档记录 Cascader 专属 Token 的语义、分类和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计见 [Cascader 桌面版架构设计](overview.md)，实现原理见 [Cascader 桌面版实现原理](implementation.md)，变化记录见 [Cascader Changelog](changelog.md)。通用 Token 规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

`CascaderToken` 描述 Cascader 输入弹层和级联选项的尺寸、列宽、选项状态色、padding、过滤高亮和内部元素间距。它只表达控件主题常量，不表达实例选择状态、当前展开路径、当前过滤值、当前 loading 状态或最大选择数量状态。

## 2. Token 分类

| 分类 | Token | 语义 | 主要消费位置 |
| --- | --- | --- | --- |
| 尺寸 | `ControlWidth` | Cascader 输入控件默认宽度。 | `CascaderTheme.axaml` root style。 |
| 尺寸 | `ControlItemWidth` | 单列选项默认宽度，也是普通 popup 最小宽度基线。 | `CascaderTheme.axaml`、level list 主题。 |
| 尺寸 | `DropdownHeight` | 弹层默认最大高度。 | `AbstractSelect` 读取并提供给弹层模板。 |
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

Cascader 默认使用 `DropdownHeight` 限制弹层高度。调用方显式设置 `DisplayPageSize` 时，则用页大小和候选项高度计算上限。

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

## 7. 主题读法与归属（Select 家族迁移）

- `CascaderTheme` 与 `CascaderAddOnDecoratedBoxTheme` 以 `CascaderTokenResource` 读取 Global Token，`Controls[Cascader]` 的覆盖作用于
  输入表面、弹层框体、选中文本和占位文字；共用部件 `SelectHandle`、`SelectMaxCountIndicator`、`SelectTagAwareTextBox` 读取宿主
  （Cascader）的有效值。
- `CascaderView` 与 `CascaderViewItem` 是注册控件，各自以 `CascaderViewTokenResource` / `CascaderViewItemTokenResource` 读取 Global
  Token（如分隔线、圆角、图标色），跟随 `Controls[CascaderView]` / `Controls[CascaderViewItem]`；它们读取的 CascaderToken（选项
  状态色、列宽、padding）跟随 `Controls[Cascader]`，独立使用的 CascaderView 同样读取 CascaderToken。
- 多选内容 padding 与标签视觉仍复用 SelectToken（跟随 `Controls[Select]`），见 [Select Token](../select/token.md) §4。
- 根 `BorderBrush` / `Background` 经输入帧的 `HostBorderBrush` / `HostBackground` 投射，不再以 LocalValue 中继。
- 禁用选项的前缀图标改用 `ColorTextDisabled` 着色（原为 `Opacity="0.3"` 字面量）；`CascaderViewItem` 未绑定到模板的
  `BorderBrush` Setter 已移除，前缀图标尺寸沿用 `FontSizeIcon`（原 `IconSize` Setter 被同主题后续 Setter 覆盖，从未生效）。

生效与隔离测试：`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/SelectFamilyEffectTests.Cascader.cs`、
`SelectFamilyEffectTests.Isolation.cs`。

## 独立 Token 身份与共享计算

Select、Cascader、TreeSelect 的标签高度/背景、禁用标签、字体和单/多选 padding 共享 AbstractSelectInputToken，
各终端从自己的有效 Global/Own 计算并经具体内部样式输入应用；`Controls[Select]` 不再改变 Cascader/TreeSelect 的输入表面。
Cascader Small 空多选使用 Small padding。TreeSelect 弹层内的内部树跟随 TreeSelect，实际树展示字段由其 Own 配置，
通过纯值函数复用树公式，不把树字段添加给 Select/Cascader；过滤高亮默认仍为 TreeSelect 的 ColorPrimary。
TreeSelectTreeView 是无身份部件，不再借 TreeView 的 StyleKey 截断宿主 owner；公开 TreeView 和 CascaderView 仍各自独立。
