# DataGrid Token 设计

本文档定义 DataGrid 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。DataGrid 整体架构见 [DataGrid 桌面版架构设计](overview.md)，内部实现原理见 [DataGrid 桌面版实现原理](implementation.md)，设计和契约变化记录见 [DataGrid Changelog](changelog.md)。

## 1. 定位

DataGrid Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `DataGridToken`，scope id 为 `DataGrid`，源码位于 `src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `CellFontSize`、`CellFontSizeMD`、`CellFontSizeSM`、`SelectionColumnWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `CellPadding`、`CellPaddingMD`、`CellPaddingSM`、`PaginationMargin`、`PaginationMarginSM`、`PaginationPanelMargin`、`PaginationPanelMarginSM` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `HeaderBg`、`HeaderColor`、`HeaderSortActiveBg`、`HeaderSortHoverBg`、`BodySortBg`、`RowHoverBg`、`RowSelectedBg`、`RowSelectedColor` |

`RowSelectedColor` 是选中行文字色，默认取全局 `ControlItemColorActive`（默认等于 `ColorText`），设在 `DataGridRow` 的
`Foreground` 上由单元格继承；与 `RowSelectedBg` / `RowSelectedHoverBg`（`ControlItemBgActive` / `Hover`）成对。
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `HeaderBorderRadius`、`LeftFrozenShadows`、`RightFrozenShadows` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

DataGrid 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

### 3.1 有效全局值与派生字段

`Controls[DataGrid].Tokens` 的全局覆盖作用于表格自身及其行、单元格、表头、分组头、排序/筛选指示器和筛选弹层。
内置 Spin 的遮罩/指示器，以及嵌套的公开 Button、CheckBox、LineEdit、Pagination 等，保留各自的控件身份。
`BorderColor` 在派生 `TableBorderColor` 前应用；表头排序色、排序列底色、分割线色和筛选悬停色同样通过对应
`Table*` 别名消费，显式设置公开字段或内部别名均能更新实际视觉。

`ExpandIconBg` / `TableExpandIconBg` 控制详情展开器背景；`FilterDropdownMenuBg` 控制菜单筛选内容的背景；
`HeaderFilterHoverBg` / `TableHeaderFilterActiveBg` 控制筛选按钮悬停背景；`SortIndicatorLayoutMargin` 控制排序图标边距。
分组头图标尺寸、单侧间距、线宽及普通单元格/筛选弹层的分隔线读取有效全局值，单侧 margin 的方向保持不变。

### 3.2 Reserved 字段

以下字段保留 schema 和默认计算，不计入 Active 承诺面，设置后没有视觉效果：

| 字段 | 保留原因 |
| --- | --- |
| `ExpandIconMargin` | antd 行内缩进后的展开图标布局；AtomUI 使用独立详情列。 |
| `ExpandIconHalfInner` | antd CSS 图标的绝对定位；AtomUI 使用 Canvas 居中的固定加减号。 |
| `ExpandIconSize` | antd 未缩放的 CSS 图标盒；AtomUI 的最终盒尺寸由 `SizeMS` 决定。 |
| `ExpandIconScale` | antd CSS 缩放；AtomUI 直接控制最终尺寸。 |
| `FixedHeaderSortActiveBg`、`TableFixedHeaderSortActiveBg` | AtomUI 没有额外的 sticky 表头副本，排序背景使用普通表头字段。 |
| `TableFilterDropdownHeight` | 当前筛选弹层按实际候选内容测量。 |
| `TableFilterDropdownSearchWidth` | 当前筛选弹层没有搜索编辑器。 |
| `SelectionColumnWidth`、`TableSelectionColumnWidth` | 选择列按真实 selector 和 CellPadding 自动测量；旧固定默认宽度会缩窄或裁剪现有布局。 |
| `TableExpandColumnWidth` | 详情列按展开器和 CellPadding 自动测量，显式列宽仍经 Column.Width 设置。 |

## 4. 控件家族影响

调整 DataGrid Token 时必须评估以下范围：

- `DataGrid`
- `DataGridColumn`
- `DataGridRow`
- `DataGridCell`
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
