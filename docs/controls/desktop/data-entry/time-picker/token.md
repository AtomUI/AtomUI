# TimePicker Token 设计

本文档定义 TimePicker 相关控件 Token 的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。TimePicker 整体架构见 [TimePicker 桌面版架构设计](overview.md)，内部实现原理见 [TimePicker 桌面版实现原理](implementation.md)，设计和契约变化记录见 [TimePicker Changelog](changelog.md)。

## 1. 定位

TimePicker Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `TimePickerToken`，scope id 为 `TimePicker`，源码位于 `src/AtomUI.Desktop.Controls/TimePicker/TimePickerToken.cs`。
- `RangeTimePickerToken`，scope id 为 `RangeTimePicker`，源码位于 `src/AtomUI.Desktop.Controls/TimePicker/RangeTimePickerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ItemHeight`、`ItemWidth`、`PeriodHostWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `ItemPadding`、`ItemMargin`、`ButtonsMargin`、`HeaderMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 读所属 Picker 的有效全局 Token |

字段状态：

- `ButtonsMargin`（Now / OK 按钮区与时间列的间距）与 `PeriodHostWidth`（上午/下午列宽，默认等于 `ItemWidth`）接通消费（Active）。
  按钮区原先借读 `DatePicker.ButtonsPanelMargin`，默认值相同。
- 新增 `ItemMargin`（时间单元左右外边距，默认 `UniformlyMarginXXS`，antd `marginInline: marginXXS`），替换 `4,0` 字面量。
- `ItemBackgroundInset` 控制 TimePicker 和 RangeTimePicker 选中、悬停和键盘导航高亮在行内的留白。默认值为零。Shadcn 主题在上下各内缩 `LineWidth`，使用原有行距和命中区域。
- `CenterItemContent` 控制 TimePicker 和 RangeTimePicker 时间列文字是否水平居中。Shadcn 启用居中并将默认内边距设为零。其他主题沿用左对齐的默认值，仍可独立配置 `ItemPadding`。
- RangeTimePicker 的 RangePickerArrowMargin/RangePickerIndicatorThickness 与 RangeMarginToAnchor 使用 RangeTimePicker 身份；原 InfoPickerInput 配置应迁移到实际 range 终端。

TimeView 与 TimeViewCell 位于注册控件 ScrollViewer 内；TimeView 把所属 Picker 的 Token owner 转发给各列，TimeViewCell 是 Token owner
透明的注册控件，因此单元的颜色、圆角与过渡时长跟随所属 Picker（原先借读 `ListView` 的 Own Token，现读所属 Picker 的全局 Token，默认
值不变）。TimeView 与 TimeViewCell 的具体 Own 样式输入由当前 TimePicker/RangeTimePicker/DatePicker/RangeDatePicker 设置，跨终端不借来源键。HeaderMargin 只用于显示时间 header 的两个日期时间终端，不添加给时间选择器。

输入框在弹层打开时保持 `ColorText`（antd 一致）；指针预览时间（`:choosing`）时，预览写入的输入框文字为 `ColorTextPlaceholder`。

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

TimePicker 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

时间列默认 `ItemWidth=ControlHeightLG × 1.4`，`ItemHeight=ControlHeight-4`，时段列默认同宽；默认高度基线为 28、列宽为 56。`ItemPadding` 将行文本按列宽与行高的差值定位，行两侧间距与圆角由 TimeViewCell 主题表达。候选、悬停、输入有效性和 `IsNeedConfirm` / `IsChangeOnScroll` 不进入 Token。

## 4. 控件家族影响

调整 TimePicker Token 时必须评估以下范围：

- `TimePicker`、`RangeTimePicker`
- `TimePickerPresenter`、`TimeView`、`TimeViewCell`
- `DatePicker` / `RangeDatePicker` 的日期时间弹层（复用 TimeView）
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 删除或重命名已生成的 TokenKind、TokenResource key 属于 Breaking 变更，必须经裁决并登记到本文档与根 CHANGELOG 的 Breaking 段（如
  已删除的 `RangePickerArrowMargin`、`RangePickerIndicatorThickness`）。
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

## 独立 Token 身份与共享计算

ItemHeight/ItemWidth 的显式基值分别在四终端守卫，再调用共享纯函数派生 ItemPadding；PeriodHostWidth 跟随该终端 ItemWidth。原 TimePicker.HeaderMargin 移到 DatePicker/RangeDatePicker，时间 footer ButtonsMargin 只属于 TimePicker/RangeTimePicker。

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `PopupArrowVisible` | `true` | 弹层是否绘制箭头（`AbstractTimeViewToken`） |
