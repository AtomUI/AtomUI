# 控件性能优化第二轮：全量审查与守卫

日期：2026-11-14。分支 `perf/control-performance-audit`，基点 `release/6.0` `f11459de0`。前置记录：
[全量评估](2026-11-14-control-performance-audit.md)、[Icon 优化](2026-11-14-icon-performance.md)。

## 约束

用户要求：所有控件按同一标准检查能否优化；优化绝对不能导致视觉差异和资源泄漏。据此本轮：

- 每项改动都与基点构建做 A/B 对比，不靠推理判断“视觉不变”；
- 顺带发现的既有视觉问题只记录不修；
- 有行为差异或无可测收益的候选一律回退。

## 守卫

| 守卫 | 覆盖 | 判定 |
| --- | --- | --- |
| 页面视觉 | 76 个 Gallery 页面，关闭延迟加载后整页截图，两侧各两次 | 同构建两次截图不同的像素视为动画遮罩，其余像素必须一致 |
| 状态视觉 | 227 个可渲染控件类型 × 默认态、各视觉枚举取值、视觉布尔属性、禁用态、7 个强制伪类 | 同上 |
| 页面泄漏 | 76 个页面，各进入离开 3 次后强制 GC | 存活实例数不得多于基线 |
| 控件泄漏 | 229 个控件类型，各 20 个实例显示后关窗 | 同上；`--leak-selftest` 确认检测能发现泄漏 |

工具与用法见 [tools/performances/README.md](../../../tools/performances/README.md)。

## 合入的改动

| 改动 | 位置 | 依据 |
| --- | --- | --- |
| Icon 去掉冗余模板 `Border`；隐藏的加载图标不跟踪祖先 | `Icon.cs`、`IconTheme.axaml` | 见 Icon 记录；`Render` 复现原背景输出 |
| ImagePreviewer 示例灰度图只编码一次 | Gallery `ImagePreviewerViewModel` | 结果只取决于内置资源 |
| 文本测量缓存 | `TextUtils.CalculateTextSizeCached`、`FontCollectionLifetime.Generation`；DatePicker/TimePicker 宽度计算 | 键含文本、字体参数、解析出的字形、字体集合代数和当前文化，结果与重新测量相同（测试覆盖） |
| Button 进入主题树前不抛异常 | `Button.ButtonThemeTokenReader.TryCreate` | 与抛出条件相同的非抛出预检；附加到树时照常配置（测试覆盖，旧代码上失败） |
| `MotionGate` 只在宿主自身可见时跟踪祖先 | `MotionGate.cs` | 宿主隐藏时结果恒为不运行；Skeleton、Spin、MarqueeLabel 共用 |
| Button 图标可见性改为内部属性 | `Button.EffectiveIconVisible`，Button/DropdownButton 7 处模板 | 与原 converter 同一规则，同为 Template 优先级 |
| 14 处 `RelativeSource TemplatedParent` 设置器改为 `TemplateBinding` | Button、DropdownButton、ProgressBar、CircleProgress | 源为同类型 Avalonia 属性，目标单向，设置器优先级不变 |
| Rate 星标去掉包裹 `Border` | `RateItemTheme.axaml` | 背景与命中由 Panel 承担，面积相同 |
| 去掉 6 个重复的附加区内边距样式 | `InputControlFrameTheme.axaml` | 派生主题有同选择器同值的样式且后声明 |
| Pagination 导航项在模板应用时创建 | `PaginationNav.cs` | 面板在 ItemsPresenter 测量时才创建，容器准备顺序和数量不变 |

## 验证

- 视觉：三批改动累计后，状态守卫 227 个控件类型全部一致；页面守卫 76 页中被标记的页面，差异像素全部位于同构建内会变化的动画像素旁，
  或经重截确认为截帧时刻（Carousel、TreeView 重截 3 次两侧完全一致）。唯一稳定存在的差异是 BorderBeam 页代码图标的抗锯齿 1 到 2 级，
  来自持续动画触发的局部重绘与 Icon 少一个子视觉之间的合成差异，其余 72 页上同样的图标逐像素一致。
- 泄漏：两侧页面存活 0/76，控件类型存活 0/229。
- 测试：全量回归 14 个项目，首轮只有 `TextInputLifecycleTests` 的 3 个嵌入输入框用例失败（见下方回退项），回退后相关 701 个用例通过。
  新增测试：文本测量缓存 9、Button 创建无异常 1、MotionGate 宿主可见性 3、Icon 13。

## 测量

基点构建与当前构建分别放在不同目录，交替运行。分配量不受机器负载影响，是主要依据；计时期间机器 1 分钟负载在 15 到 80 之间，只作参考。

### 控件级分配（每类型 100 实例，默认态）

| 控件 | Visuals | 前 KB | 后 KB | 变化 |
| --- | --- | ---: | ---: | ---: |
| Pagination | 7 → 7 | 381.8 | 58.0 | −84.8% |
| SearchOutlined（Icon） | 2 → 1 | 38.2 | 26.1 | −31.6% |
| DatePicker | 48 → 47 | 996.1 | 821.0 | −17.6% |
| TimePicker | 48 → 47 | 977.7 | 812.6 | −16.9% |
| RangeDatePicker | 76 → 74 | 1461.8 | 1277.6 | −12.6% |
| RangeTimePicker | 76 → 74 | 1351.9 | 1259.7 | −6.8% |
| LoadingOutlined（Icon） | 2 → 1 | 38.1 | 29.8 | −21.6% |
| DropdownButton | 14 → 13 | 288.3 | 258.7 | −10.3% |
| SplitButton | 25 → 24 | 528.1 | 477.6 | −9.6% |
| Button | 11 → 11 | 198.7 | 182.7 | −8.1% |
| Result | 12 → 11 | 142.4 | 134.2 | −5.8% |
| Rate | 35 → 30 | 419.2 | 396.7 | −5.4% |
| ProgressBar | 15 → 15 | 251.9 | 243.7 | −3.3% |
| SearchEdit | 38 → 37 | 746.9 | 715.3 | −4.2% |
| NumericUpDown | 55 → 53 | 974.8 | 951.1 | −2.4% |
| Select / TreeSelect | 53 → 52 | 1006.4 / 1002.6 | 991.1 / 987.2 | −1.5% |
| LineEdit | 30 → 30 | 498.9 | 493.5 | −1.1% |

均为最终代码（嵌入输入框精简模板已回退）的实测值。

### 页面级（76 页，3 轮交替，每轮 8 次稳态导航）

| 指标 | 前 | 后 | 变化 |
| --- | ---: | ---: | ---: |
| 每轮全部页面一次导航的分配 | 1176 MB | 1113 MB | −5.4%，没有任何页面分配增加 |
| 全部页面 visual 数 | 74687 | 72303 | −3.2% |
| 全部页面稳态耗时之和 | 3248 ms | 2915 ms | −10.2%（其中 ImagePreviewer −254 ms；其余页面合计 −2.6%） |

页面举例：ImagePreviewer 276.7 → 22.3 ms（−92%）；DropdownButton 30.6 → 22.9 ms（−25%，每次导航 GC 2 → 1）；DatePicker 107.1 → 93.2 ms（−13%）；
Button 61.1 → 55.0 ms（−10%），分配 −18%。

首轮里 11 个页面看似慢 5% 到 13%。负载降到 15 到 25 后用 5 轮交替复测，其中 10 个落在 ±3% 噪声内或转为更快；Mentions 再用 8 轮测得中位 +11%
但单轮差值从 −5.8 到 +15.9 ms、3/8 轮更快，控件级测量 Mentions 耗时 1392 → 1389 µs、分配完全相同（没有任何改动触及它），判定为页面级噪声，
记为“未能证明为零”。

## 回退或未做

| 候选 | 结论 | 原因 |
| --- | --- | --- |
| 嵌入输入框精简模板（每个约 100 KB、7 个视觉） | 回退；2026-11-14 决定保留能力 | `TextInputLifecycleTests` 把嵌入输入框纳入完整文本框契约（开启清除按钮、内嵌内容后必须可用）；当前宿主虽不使用，删除即丧失能力 |
| `DrawPens` 改为不创建（每个 Icon 约 8.5 KB） | 未做，待决策 | 文档规定为派生契约，需独立兼容性评审 |
| DateViewer 单元格去掉根 `Border` | 回退 | Compact 模式下内层 Grid 居中，整格可点击区域会缩小 |
| 输入框错误/警告状态样式合并 | 回退 | 实测分配无变化：未激活样式没有可测成本 |
| MenuItem 三路 MultiBinding 改内部属性 | 未做 | `IsTopLevel` 是无变更通知的 CLR 属性，读取时机不同无法保证等价 |
| TreeView 节点开关按需创建默认图标 | 未做 | 公开的 `ExpandIcon` 等 getter 返回值会变化 |
| Tag 颜色重复应用 | 未做 | 收益每个彩色标签几个对象，不值得增加状态 |
| AutoComplete、Mentions 候选列表首次打开时创建 | 未做 | 允许的弹层例外，但历史上同类改动出过重开与筛选状态问题，需单独排期并完整覆盖生命周期 |
| Cascader 视图延迟创建 | 不建议 | 关闭态的多选标签依赖视图里的数据逻辑，需先拆出非视觉辅助类 |
| `IconTemplatePresenter` 按需构建选中图标 | 未做 | 收益未测，需要新增构建时机状态 |
| 文本输入光标对象共享 | 未做 | 每次模板实例化新建 `Cursor`，无头环境测不出成本，需真机评估 |

## 审查中发现的既有问题（未修）

- `ListBoxItemTheme.axaml` 的选中指示器样式选择器写成 `IconPresenter`，实际元素是 `IconTemplatePresenter`，样式从不生效。
- `IconTemplatePresenter` 替换图标时不释放旧图标的 7 个绑定，旧图标随宿主存活。
- `TimeViewCellTheme.axaml` 禁用态前景样式指向不存在的 `#PART_ContentPresenter`。
- `SimplePagination` 点击时 `CurrentPageChanged` 触发两次；`Pagination.SetupTotalInfoText` 的起始序号比 Ant Design 少 1（待核实）。
- Dashed 按钮在 `ShadowsFrame` 与 `Frame` 上各画一次背景，半透明背景叠加两次（待对照 Ant Design）。
- `Button.MeasureOverride` 在测量中写本地 `CornerRadius`，会覆盖用户值。
- ProgressBar 的 `TrailColor` 以本地值写 `GrooveBrush`，禁用态颜色失效。
- `AddOnDecoratedBox` 默认状态下可能清掉用户在附加区图标上设置的本地画刷（待核实）。
- `OtpLineEditCell` 每次调用都新建 `Cursor` 且不释放旧值。
- 多处 `_ignore*` 标志（Cascader、CascaderView、AbstractSelect、Select），与性能技能规则冲突，未改。
