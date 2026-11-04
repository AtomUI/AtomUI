# DataGrid Token 设计

本文档定义 DataGrid 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。DataGrid 整体架构见 [DataGrid 桌面版架构设计](overview.md)，内部实现原理见 [DataGrid 桌面版实现原理](implementation.md)，设计和契约变化记录见 [DataGrid Changelog](changelog.md)。

## 1. 定位

DataGrid Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `DataGridToken`，scope id 为 `DataGrid`，字段声明位于 `src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs`，默认函数位于 `DataGridToken.Defaults.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `CellFontSize`、`CellFontSizeMD`、`CellFontSizeSM`、`SelectionColumnWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `CellPadding`、`CellPaddingMD`、`CellPaddingSM`、`PaginationMargin`、`PaginationMarginSM`、`PaginationPanelMargin`、`PaginationPanelMarginSM` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `HeaderBg`、`HeaderColor`、`HeaderSortActiveBg`、`HeaderSortHoverBg`、`BodySortBg`、`RowHoverBg` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `HeaderBorderRadius`、`LeftFrozenShadows`、`RightFrozenShadows` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

DataGrid 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

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

### 5.1 规范字段与历史入口

以下历史`Table*`属性通过显式别名元数据映射到规范字段；属性、slot、TokenKind、typed key及资源键继续保留。配置先在每层归一，跨层按现行优先级覆盖；同层同值归一，不同值诊断冲突。Compiler在默认依赖求值及显式覆盖后，把规范值复制到历史字段，再发布同一snapshot，历史AXAML消费者因而读取有效规范值。

| 历史入口 | 规范字段 | 实际消费区域 |
| --- | --- | --- |
| `TableBorderColor` | `BorderColor` | cell/row网格线 |
| `TableExpandedRowBg` | `RowExpandedBg` | 行详情框体 |
| `TableFilterDropdownBg` | `FilterDropdownBg` | menu/tree过滤器Presenter |
| `TableFontSize` | `CellFontSize` | Large单元格/详情文字 |
| `TableFontSizeMiddle` | `CellFontSizeMD` | Middle单元格/详情文字 |
| `TableFontSizeSmall` | `CellFontSizeSM` | Small单元格/详情文字 |
| `TableFooterBg` | `FooterBg` | footer背景 |
| `TableFooterTextColor` | `FooterColor` | footer文字 |
| `TableHeaderBg` | `HeaderBg` | header及表头框体背景 |
| `TableHeaderTextColor` | `HeaderColor` | header文字 |
| `TablePadding` | `CellPadding` | Large单元格/详情间距 |
| `TablePaddingMiddle` | `CellPaddingMD` | Middle单元格/详情间距 |
| `TablePaddingSmall` | `CellPaddingSM` | Small单元格/详情间距 |
| `TableRadius` | `HeaderBorderRadius` | DataGrid圆角 |
| `TableRowHoverBg` | `RowHoverBg` | row hover/拖动背景 |
| `TableSelectedRowBg` | `RowSelectedBg` | selected row背景 |
| `TableSelectedRowHoverBg` | `RowSelectedHoverBg` | selected row hover背景 |

其他`Table*`字段按各自语义维护，不按前缀归为别名。DataGrid使用DependencyDefaultsV2、revision 1：78个既有字段中的17项别名由规范值投影，其余61项拥有静态默认函数，保留字段名称、类型、slot及资源键。默认公式与模板尺寸分支保持原合同；明确Own值保留存在位，合法零值与未配置状态分开。Own依赖默认值及版本边界见[Own Token设计](../../../../architecture/systems/theming/own-token-evaluation.md)。同层新旧入口不同值在绑定边界报错，应用应合并为一个入口或相同值。

### 5.2 非别名字段的默认依赖

以下8项保留独立slot和明确配置能力，未配置时按当前规范Own值求默认：`TableHeaderCellSplitColor ← HeaderSplitColor`、`TableHeaderSortBg ← HeaderSortActiveBg`、`TableHeaderSortHoverBg ← HeaderSortHoverBg`、`TableBodySortBg ← BodySortBg`、`TableFixedHeaderSortActiveBg ← FixedHeaderSortActiveBg`、`TableHeaderFilterActiveBg ← HeaderFilterHoverBg`、`TableSelectionColumnWidth ← SelectionColumnWidth`、`TableExpandIconBg ← ExpandIconBg`。

明确配置派生字段时，该值优先；清除后恢复当前依赖默认。其他尺寸和颜色公式继续读取各自Global输入，共用局部算式不构成额外Own依赖。默认计划不负责把尚无实际消费者的历史槽位接入模板。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |


## 6. 实际消费与范围边界

DataGrid 保留78个Own字段与17个显式别名。48个Active字段和17个Legacy别名到达实际模板、行状态或列资源；默认依赖求值不等于存在绘制消费者。

### 6.1 明确配置入口

以下既有字段只在配置明确提供时覆盖历史基线，清除后恢复原行为：

| 字段 | 实际目标 | 未配置时 |
| --- | --- | --- |
| `SelectionColumnWidth` | 非Visual选择列的Width默认 | 保留Auto及原列宽约束；调用方Width优先 |
| `ExpandIconBg` / `ExpandIconSize` / `ExpandIconMargin` | 展开按钮的实际Frame背景、控件宽高和margin | 背景Transparent、原SizeMS、原margin；9 DIP加减号保持固定 |
| `FixedHeaderSortActiveBg` | 实际冻结排序header及其hover/shadow分支 | 按原HeaderSortActiveBg/HeaderSortHoverBg/HeaderBg分支 |
| `HeaderFilterHoverBg` | filter indicator hover背景 | 原ColorBgTextHover |
| `FilterDropdownMenuBg` | 实际DataGridFilterMenuItem背景 | 原Menu ItemBg，原hover/checked/disabled状态继续优先 |
| `SortIndicatorLayoutMargin` | 实际排序indicator margin | 原margin |

规范Own的别名归一、独立派生覆盖及逐层清除按第5节；不把名称相似的独立Table字段新增为别名。

### 6.2 Global、字体与Style优先级

根DataGrid消费其注册Global；内部asset-only header、排序/过滤indicator、展开及操作/拖动主题锚定实际DataGrid。公开Cell、Row、RowHeader、RowGroupHeader保留各自Global身份，明确FontFamily/ColorText按各自配置消费，未明确时继承实际parent。列明确文字属性、控件Local值和应用Style按Avalonia原优先级保留。

Large/Middle/Small继续使用各自Own字体与间距。Custom回退等于原Middle，但不再由Middle条件选择器占有，适用的header/cell/rowheader/details/title/footer/pagination可由应用Style或生成的专用Semantic Part Style定制。这不改变虚拟化、冻结、滚动条、数据和业务状态，也不构成全部Custom组合承诺。

### 6.3 无绘制消费者的兼容槽位

以下13字段保留名称、类型、默认公式、资源编号、slot和编辑入口；当前不声明可调节绘制能力：

- 固定glyph历史局部量：`ExpandIconHalfInner`、`ExpandIconScale`。
- 独立历史镜像：`TableHeaderCellSplitColor`、`TableHeaderSortBg`、`TableHeaderSortHoverBg`、`TableBodySortBg`、`TableFixedHeaderSortActiveBg`、`TableHeaderFilterActiveBg`、`TableSelectionColumnWidth`、`TableExpandIconBg`。
- 未接线的历史尺寸：`TableFilterDropdownHeight`、`TableFilterDropdownSearchWidth`、`TableExpandColumnWidth`。

独立历史镜像的默认仍跟随第5.2节指定输入，明确配置仍可独立存储与读取；不会因此投射到模板。固定glyph、Focus笔和拖动指示线等几何不机械改成Token。工程核定与限定审查状态见[DataGrid家族记录](../../../../superpowers/plans/2026-10-22-custom-theme/data-grid-family-exit.md)。
