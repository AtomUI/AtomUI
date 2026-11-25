# 输入 frame 状态画刷改为主题变量

日期：2026-11-14。分支 `perf/design-language-input-frame`，基点 `feature/design-language` `adddb426a`。

## 背景

`InputControlFrameTheme` 用 57 条样式、70 处 `OwnerTokenResource` 表达 variant × 有效状态 × 交互状态矩阵。所有输入家族控件至少有一个 frame，部分有两个：

- 嵌入式 TextBox 里还有一个 frame。
- 日期、时间选择器各有自己的输入框。

上限实验：把矩阵整块删掉、只留 Outlined 默认值，分配可减少 11%–13%。这个实验不保留视觉，只用来估算收益空间。

## 改动

1. **variant × 状态改为主题变量。** frame 在 C# 中按 `StyleVariant` 与 `EffectiveStatus` 选出 6 个表面变量：`FrameBorderBrush`、`FrameBackground`、`FrameHoverBorderBrush`、`FrameHoverBackground`、`FrameActiveBorderBrush`、`FrameActiveShadow`。
   - 变量由 AtomUI.Core 新增的 `OwnerTokenSet` 供值，资源键、owner 解析、回落和主题发布的增量判断都与 `OwnerTokenBinding` 相同。每个 frame 只有一组订阅。
   - 条目在样式第一次读取时才解析：只在 hover、focus、pressed 中读取的变量，在该状态出现之前不做资源查找。
   - 主题只按 variant 伪类与 hover / focus-within / pressed 把变量投射到表面属性。
   - 投射样式的选择器条件与原矩阵一一对应，各属性的优先级类别（StyleTrigger 还是 Style）和声明顺序都不变。
   - disabled、附加区和根画刷样式原样保留。
2. **变量用 DirectProperty，不经 Avalonia 绑定，并由一个 set 统一供值。** 实现迭代过四版：
   - **StyledProperty 加 LocalValue 绑定：** 每个 frame 的 value store 数组变大，常驻内存几乎没降（374.6 → 370.7 KB）。
   - **每个变量一个 accessor：** 常驻内存降了，但每个 frame 有 5 组订阅。TextBox（只有一个普通 frame）连续 6 轮比基线慢 6%–8%。剖析显示，配置变量占 TextBox 创建的 2.9%，几乎全部在 accessor 的订阅上。
   - **一个 set 统一供值：** 订阅降到每个 frame 一组。
   - **set 条目按需解析：** 普通状态只做旧主题同样的两次资源查找。最终版 TextBox 在 10 轮配对测量中 5 轮更快，配对比中位数 1.004，即持平。
3. **删除 6 条附加区内边距样式。** `AddOnDecoratedBoxTheme` 在其后声明了相同选择器、相同值的样式；直接以 `InputControlFrameTheme` 为基础的另一个主题 `OtpLineEditCellTheme` 没有附加区。

## 定制影响

按分支规则检查：

- **读 Token：** 读的是 owner 的有效值，资源键与 `OwnerTokenResource` 相同。
- **主题变化时重算：** accessor 订阅 frame 自身的资源、主题变体和 owner 变化，变化时重新取值。
- **写入方式：** 不写公开属性的本地值，变量是 internal，投射靠主题 Setter。
- **状态映射：** 状态映射仍由选择器完成。
- **动画：** `TokenTransitions` 不变。

新增测试 `InputFrameThemeVariableTests` 覆盖 5 种情况：

- 作用域配置覆盖错误/警告色；
- `Controls[LineEdit]` 覆盖 Filled 错误背景；
- 运行时替换配置；
- variant 切换时只重配需要的变量；
- 未挂树时变量没有值。

效果测试的 drift guard 改为统计主题 AXAML 读取与 `InputControlFrame.cs` 代码读取的并集。

## 验证

- **表面值矩阵：** `AtomUI.ControlBaseline --frame-matrix`，覆盖 13 个输入宿主 × 4 variant × 3 status × 8 种交互状态，加上 Large / Small 两档尺寸，共 7680 行。逐行比较 frame 与附加区的边框、背景、阴影、内边距、圆角和可见性，两侧完全一致。
- **视觉守卫（最终版本）：** 76 个页面与 225 个可渲染控件类型中，被标记的差异都可以用动画时刻解释，没有任何输入 frame 表面出现差异：
  - `AutoCompleteSearchEdit` 默认态搜索按钮背景差 1 级、图标灰度略有不同，是颜色过渡在截图时刻的进度不同：同一构建内该区域也会变化；把稳定循环延长到 60 轮让过渡结束后，AutoComplete、AutoCompleteSearchEdit、LineEdit、SearchEdit 两侧完全一致。
  - BorderBeam 页 13 个像素差 1 级，来自持续动画；
  - 其余被标记的页面和类型，差异像素都位于动画像素旁。
- **泄漏：** 两侧页面存活 0/76，控件类型存活 0/229。
- **测试：** 最终版本全量回归（14 个项目，不含默认排除的集成测试）12500 个测试全部通过。开发过程中：
  - 效果测试的 drift guard 改为统计 AXAML 与代码读取的并集；
  - OTP 宿主排除表删去 frame 主题已不再读取的三个内边距 Token；
  - 新增 `OwnerTokenSetTests`（4 个）与 `InputFrameThemeVariableTests`（5 个）。

## 收益

每个类型 100 个实例，默认态，取两轮中位数。

| 控件 | 前 KB | 后 KB | 变化 |
| --- | ---: | ---: | ---: |
| TextBox | 425.5 | 379.0 | −10.9% |
| OtpLineEdit | 2577.2 | 2311.1 | −10.3% |
| LineEdit | 550.3 | 501.4 | −8.9% |
| NumericUpDown | 1107.1 | 1010.8 | −8.7% |
| ButtonSpinner / RangeTimePicker | 608.3 / 1598.1 | 556.2 / 1461.2 | −8.6% |
| TimePicker / TextArea | 1120.8 / 518.2 | 1027.8 / 475.4 | −8.3% |
| Select / TreeSelect | 1184.2 / 1179.4 | 1088.5 / 1083.7 | −8.1% |
| DatePicker | 1163.6 | 1070.7 | −8.0% |
| RangeDatePicker / ComboBox | 1732.8 / 616.3 | 1595.6 / 567.6 | −7.9% |
| AutoComplete | 641.4 | 592.7 | −7.6% |
| Mentions | 613.1 | 570.4 | −7.0% |
| Cascader | 814.6 | 763.0 | −6.3% |
| SearchEdit | 857.7 | 809.6 | −5.6% |

常驻对象（gcdump 每实例差值，确定性）：LineEdit 5530 → 4709（−14.8%），Select 10540 → 9003（−14.6%）。

常驻字节每次快照波动较大：LineEdit 约 373–380 → 340–363 KB，Select 约 740–810 → 628–652 KB。

计时期间机器负载在 15 到 55 之间，单控件计时只作参考，不据此声明百分比。

## 未做

- **附加区状态样式（8 条）改为变量。** 附加区的默认画刷在 `AddOnDecoratedBoxTheme` 中是不带条件的 Style 优先级。若投射样式在所有 variant 下都生效，默认态的优先级会从 Style 升到 StyleTrigger，应用层不带条件的 Style（如 Semantic Part 样式）将无法再覆盖它。要等价就必须保留现有条件结构，剩余收益有限。
- **Select、Cascader、TreeSelect 的 `IsDropDownOpen` 边框样式和日期选择器主题。** 这些是控件专属的小矩阵，单项收益小。
