# AtomUI 控件创建性能全量评估

日期：2026-11-14。分支 `perf/control-performance-audit`，基点 `release/6.0` `f11459de0`。macOS arm64（Apple M1 Max），
net10.0，Release，Avalonia 12.1.6。本次只做评估，没有产品代码改动。

## 结论

1. Gallery 页面创建确实重：76 个 ShowCase 稳态重复导航的中位数是 32 ms，20 个页面超过 50 ms，72 个超过一帧（16.7 ms）。
   首次访问约为稳态的 2 倍。这是 Release、Headless、不含 GPU 上屏的数字，真实窗口只会更慢。
2. “所有控件本身都慢”不成立。按每个 visual 的成本算，AtomUI 全部控件的中位数与 Avalonia 原生 Fluent 持平
   （18.4 µs、11.7 KB 对 14.1 µs、11.3 KB）。
3. 慢集中在三处：`Icon` 基元、输入类复合控件、以及分配量带来的 GC。它们叠加后让 Button、Select、DatePicker、
   LineEdit 这些最常用的控件比 Fluent 同名控件重 2.7 到 6.9 倍。
4. TKCode.Arc 使用同一个 Avalonia 版本和同一种 DynamicResource Token 机制，没有更便宜的框架路径。它的差别是每个控件的
   visual 更少、少见部件延迟创建。它的画廊不在该仓库，体感差距中有多少来自画廊本身无法从源码判断。

## 测量口径

- 页面级：`AtomUI.GalleryPerformance --audit`。一个进程依次导航全部 ShowCase，只计导航触发和布局泵的时间与当前线程分配，
  就绪判定用的视觉树统计不计入。稳态取 12 次中的最小值与中位数。
- 控件级：`AtomUI.ControlBaseline`。每个可默认构造的控件类型建 100 个实例放进 `WrapPanel`，计构造、挂载、套样式、套模板、
  布局的总成本，减去空窗口底噪，取 9 次最小值。同一套代码分别在 AtomUI 主题和 Fluent 主题的独立进程里跑。
- 热点：macOS `sample` 原生 CPU 采样，配合 `DOTNET_PerfMapEnabled=1` 的 JIT 符号表。
- 机器负载：测量期间其他会话在跑测试，1 分钟负载在 15 到 160 之间波动。计时以低负载时段的最小值为准；
  分配量和 visual 数不受负载影响，是更稳的信号。全量物化那一轮负载最高，其计时不作结论，只用分配量。
- 冷进程首次导航只有 Button 的 3 个样本（约 619 ms），属于冒烟数据。Debug 构建和真实窗口渲染没有测。

EventPipe 采样（`dotnet-sampled-thread-time`）在这里不可用于叶子归因：样本落在运行时安全点，`Monitor.Enter_Slowpath`
和 `PollGC` 占了 80% 的自耗时。它曾把 `EnabledControlPackage.FirstUse()` 的锁显示成 10%，原生采样下
`StartupResourceProvider.TryGetResource` 整条链只有 1.1%。该锁不是热点。

## 页面级结果

默认口径（Gallery 的分批延迟挂载生效，只物化视口内的 ShowCaseItem），按稳态最小值排序的前 15 个：

| ShowCase | Visuals | 首次访问 ms | 稳态最小 ms | 稳态中位 ms | µs/visual | 每次分配 MB |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| imagepreviewer | 649 | 430.3 | 288.8 | 305.5 | 445.0 | 20.8 |
| dateviewer | 4034 | 184.1 | 130.9 | 148.9 | 32.5 | 54.7 |
| datepicker | 2003 | 147.8 | 97.7 | 107.8 | 48.8 | 36.9 |
| treeview | 1997 | 163.2 | 95.5 | 115.7 | 47.8 | 31.8 |
| datagrid | 3269 | 263.0 | 92.0 | 97.3 | 28.1 | 37.5 |
| lineedit | 1788 | 439.9 | 83.2 | 138.7 | 46.5 | 27.8 |
| select | 1691 | 138.7 | 76.8 | 79.4 | 45.4 | 29.3 |
| numberupdown | 1710 | 147.5 | 70.5 | 76.5 | 41.2 | 27.8 |
| button | 1449 | 83.1 | 66.5 | 72.7 | 45.9 | 28.4 |
| form | 1798 | 107.1 | 65.5 | 74.5 | 36.4 | 26.6 |
| buttonspinner | 1106 | 76.1 | 62.0 | 66.2 | 56.1 | 22.8 |
| treeselect | 1370 | 111.8 | 61.6 | 68.7 | 45.0 | 23.0 |
| qrcode | 804 | 104.5 | 60.2 | 62.8 | 74.9 | 10.9 |
| transfer | 1323 | 116.1 | 55.7 | 63.4 | 42.1 | 20.9 |
| tabcontrol | 1411 | 86.9 | 55.1 | 57.8 | 39.1 | 20.2 |

- 76 页合计稳态 3.15 s；中位数 32.2 ms；每 visual 中位数 37 µs、15.8 KB。
- 首次访问是进程内第一次进入该页，受访问顺序影响（`lineedit` 排第一，承担了共享的 JIT）。
- `imagepreviewer` 是孤立的异常值，每 visual 成本是其他页面的 10 倍，需要单独排查。
- 关闭延迟挂载后的分配量：`datagrid` 194 MB、`form` 142 MB、`dateviewer` 130 MB、`calendar` 118 MB、`lineedit` 104 MB。

Gallery 现有的延迟挂载是每批 2 个 ShowCaseItem、以 `Loaded` 优先级排队。所以真实窗口里这几十到上百毫秒被拆到多帧，
表现为内容逐批冒出来。

## 控件级结果

AtomUI 202 个类型、Fluent 126 个类型测量成功（`FlyoutHost`、`PopupConfirm` 因 Content 类型不接受字符串而跳过）。

同名控件对比，单实例成本：

| 控件 | AtomUI visuals | Fluent visuals | AtomUI µs | Fluent µs | 时间倍数 | AtomUI KB | Fluent KB | 分配倍数 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DatePicker | 48 | 15 | 2791 | 403 | 6.9 | 1004 | 200 | 5.0 |
| Separator | 5 | 2 | 42 | 8 | 5.1 | 59 | 18 | 3.2 |
| ListBoxItem | 10 | 3 | 126 | 27 | 4.6 | 88 | 39 | 2.3 |
| SplitButton | 25 | 13 | 1291 | 297 | 4.4 | 529 | 175 | 3.0 |
| Carousel | 19 | 9 | 471 | 106 | 4.4 | 213 | 115 | 1.9 |
| ComboBox | 31 | 19 | 1571 | 439 | 3.6 | 560 | 230 | 2.4 |
| ListBox | 13 | 10 | 452 | 150 | 3.0 | 205 | 134 | 1.5 |
| Button | 11 | 3 | 319 | 120 | 2.7 | 198 | 50 | 4.0 |
| ProgressBar | 15 | 10 | 498 | 186 | 2.7 | 248 | 143 | 1.7 |
| Calendar | 455 | 289 | 19047 | 7537 | 2.5 | 6543 | 3149 | 2.1 |
| NumericUpDown | 55 | 50 | 2653 | 1356 | 2.0 | 974 | 542 | 1.8 |
| CheckBox | 12 | 12 | 198 | 230 | 0.9 | 140 | 131 | 1.1 |
| RadioButton | 8 | 9 | 112 | 127 | 0.9 | 84 | 108 | 0.8 |
| Slider | 7 | 21 | 142 | 403 | 0.4 | 93 | 196 | 0.5 |
| ScrollBar | 10 | 26 | 167 | 366 | 0.5 | 87 | 228 | 0.4 |

没有 Fluent 对应物、单实例最重的一组（默认关闭态）：

| 控件 | Visuals | µs | KB | µs/visual |
| --- | ---: | ---: | ---: | ---: |
| Select | 53 | 3031 | 1006 | 57.2 |
| TreeSelect | 53 | 2824 | 1002 | 53.3 |
| TimePicker | 48 | 2697 | 995 | 56.2 |
| SearchEdit | 38 | 2412 | 747 | 63.5 |
| Cascader | 32 | 2129 | 716 | 66.5 |
| Mentions | 32 | 1621 | 561 | 50.6 |
| AutoComplete | 33 | 1563 | 580 | 47.4 |
| LineEdit | 30 | 1381 | 499 | 46.0 |
| StepsItem | 22 | 1281 | 534 | 58.2 |
| Alert | 15 | 922 | 261 | 61.5 |
| Pagination | 7 | 788 | 382 | 112.5 |

输入类控件每 visual 46 到 66 µs，是全库中位数 18.4 µs 的 2.5 到 3.5 倍。Fluent 的 `TextBox` 是 22 个 visual、540 µs、273 KB。
202 个类型里有 111 个单实例低于 150 µs。

基元：

| 基元 | Visuals | µs | KB |
| --- | ---: | ---: | ---: |
| `LoadingOutlined`（Icon） | 2 | 106 | 38.4 |
| `SearchOutlined`（Icon） | 2 | 84 | 38.5 |
| `TextBlock` | 1 | 43 | 11.6 |
| `IconPresenter`（空） | 1 | 12 | 4.1 |
| `DashedBorder` | 1 | 9 | 3.4 |
| `PixelAlignedBorder` | 1 | 6 | 3.4 |

更正（同日复测）：上表计时在 1 分钟负载 50 到 160 时测得，偏高。负载约 12 时，Icon 约 27 µs、38.5 KB，
Fluent `PathIcon` 30 µs、28.5 KB，Fluent `Button` 182 µs、52 KB。Icon 在时间上并不比原生图标重，分配多约 35%；
早先“一个 Icon 接近一个完整 Fluent Button”的说法不成立。分配量不受负载影响，上表 KB 列仍然有效。

## 热点归因

Button 页稳态导航的原生采样（8641 个样本）：

| 自耗时归属 | 占比 |
| --- | ---: |
| GC（分配触发的回收） | 34.5% |
| BCL | 13.8% |
| Avalonia 属性系统 | 11.1% |
| Avalonia 样式（选择器、激活器、Setter） | 7.2% |
| JIT、类型加载、运行时桩 | 5.3% |
| AtomUI 库自身代码 | 3.3% |

按调用链包含时间看：模板实例化 `TemplateContent.Load` 23%，文本排版 `TextBlock.CreateTextLayout` 13.9%，
`MultiBindingExpression.StartCore` 8.0%，`BindingExpression.OnNodeError` 4.8%，`DynamicResourceExpression` 启动与重算合计约 6.8%，
`Icon.OnInitialized` 4.1%、`Icon.OnLoaded` 1.9%、`TrackEffectiveVisibility` 1.3%。

Select 页：GC 21.2%，属性系统 17.7%，样式 7.2%，AtomUI 自身 4.1%；`StyleBase.Attach` 5.7%，表达式节点绑定 4.5%，
Gallery 延迟项的 Skeleton 占位约 2% 到 5%。

AtomUI 自己的 C# 代码只占 3% 到 4%。成本主要是 AtomUI 的模板和主题让 Avalonia 做的工作量：visual 数、绑定数、
动态资源表达式数，以及由此产生的分配。

### Button 模板诊断实验

临时删除模板部件，测单实例成本，测完已还原。分配量在 200 实例口径下测得，计时取同一低负载时段：

| 变体 | 单实例 KB | 相对基线 | 单实例 µs |
| --- | ---: | ---: | ---: |
| 基线 | 205.7 | | 319 |
| 去掉图标可见性 `MultiBinding` | 197.8 | −4% | 318 |
| 再去掉隐藏的 `LoadingOutlined` | 149.0 | −28% | 218 |
| 再去掉 `WaveSpiritDecorator` | 136.0 | −34% | 222 |
| 再去掉 `Transitions` Setter | 136.0 | −34% | 228 |
| 再去掉 9 个 `RelativeSource Self` 变体画刷绑定和阴影绑定 | 119.0 | −42% | 180 |

隐藏的加载图标一项占 Button 分配的 24%、时间的约 31%。`Transitions` Setter 在创建期没有可测成本。
去掉全部四项后仍有 119 KB，是 Fluent Button 的 2.4 倍，剩余差距来自 9 个 visual 对 3 个。

这和 `atomui-control-performance` 里 Theme Static Rule 的成本表不一致：该表把隐藏节点的代价记为“一个 Visual 实例加一次模板展开”。
`IsVisible=False` 省的是布局和渲染，实例化、套样式、建绑定、订阅照付。对 Icon 这种重基元，实测是所在控件的四分之一。

## 根因

1. **Icon 基元过重。** [Icon.cs](../../../src/AtomUI.Core/Controls/Icon/Icon.cs) 是 `TemplatedControl`，模板只有一个 `Border`，
   实际内容在 `Render` 里画。每个实例有 9 个动态 Token Setter；`OnInitialized` 创建 5 个可变 `Pen`（`AvaloniaObject`），
   之后每次画刷属性变化再 `new Pen`；配置了加载动画的图标在挂载和 `Loaded` 时各注册一次可见性跟踪，
   每次沿祖先链对每一层订阅 `PropertyChanged`（Gallery 里深度 30 到 46）。Button 页有 66 个隐藏的 `LoadingOutlined`。
2. **输入类复合控件 visual 多且每 visual 贵。** `LineEdit` 30 个、`Select` 53 个、`DatePicker` 48 个，关闭态就是 1 MB 级。
3. **分配量。** 每 visual 12 到 20 KB，Button 页一次导航 28 MB、4 次 gen0 回收，GC 占创建时间的 21% 到 35%。
4. **Button 自身。** 变体画刷走 9 个 `{Binding …, RelativeSource=Self}` Setter 加一个阴影绑定（8%），`WaveSpiritDecorator` 常驻（6%），
   图标可见性 `MultiBinding`（4%）。
5. **Gallery 外壳。** 每个 ShowCaseItem 带代码和展开图标按钮、分隔线、延迟项的 Skeleton 占位；`dateviewer` 一页 4034 个 visual；
   `imagepreviewer` 需单独排查。

## TKCode.Arc 对比

仅阅读源码（HEAD `695a85d`），没有构建和运行，无实测数据。

| 方面 | TKCode.Arc | AtomUI |
| --- | --- | --- |
| Avalonia | 12.1.5 | 12.1.6 |
| Token 机制 | `DynamicResourceExtension` 包装，5,346 处引用 | 同一机制，3,730 处引用 |
| 模板规模 | 266 个模板，中位数 5 个元素；Button 模板 3 个元素，运行时约 6 个 visual | Button 11 个 visual |
| 少见部件 | 焦点环、加载图标、弹层内容、卡片页脚等首次使用时在代码里创建 | 声明在模板里，用 `IsVisible` 隐藏 |
| 过渡动画 | 273 个 `Transitions` 块全部挂在 `Loaded` 之后才置位的伪类下 | 93 个 `Transitions` 块；Button 的挂在 `[IsMotionEnabled=True]` 下，实验中创建期无可测成本 |
| 自绘 | 329 个主题里 63 个无模板，133 个文件重写 `Render` | 以模板组合为主 |
| 选择器 | 变体用伪类 | 变体用属性选择器 |
| 画廊 | 在另一个仓库；更新日志称预览和示例“进入视口才创建” | 分批延迟挂载 |

它的更新日志自述“500 个按钮分配 52.7 MB”，约 105 KB 每个，口径未知；AtomUI 实测 198 KB 每个。按每 visual 折算两者接近，
差距主要来自 visual 数量。它的画廊体感不能据此下结论。

## 建议的优化顺序

均未实施，收益为诊断实验给出的上限。

1. **Icon 瘦身。** 画笔按需创建或改用不可变画笔，可见性跟踪只注册一次。只改内部 C#，不动模板和行为。
   所有带图标的控件都受益，Button 的上限约为分配 −24%、时间 −31%。
   已实施：去掉冗余模板 `Border`、隐藏加载图标不再跟踪祖先；`DrawPens` 因派生契约保持原样。结果见
   [2026-11-14-icon-performance.md](2026-11-14-icon-performance.md)。
2. **Button 变体画刷和 WaveSpirit。** 上限约为分配再 −14%。涉及绑定优先级，需按 Binding Priority Guardrails 列出全部绑定后再动。
3. **输入类模板审计。** 以 `AddOnDecoratedBox` 和 `LineEdit` 为起点，逐个核对 visual 和绑定。
4. **Gallery 页面。** 排查 `imagepreviewer`；评估 Skeleton 占位和 ShowCaseItem 外壳的成本。
5. 首次访问成本（JIT）另案，本次只确认了约 2 倍的量级。

## 工具改动

- `AtomUI.GalleryPerformance`：62 个 ShowCase 的路由类型名和 XAML 路径还指向重组前的 `AtomUIGallery.ShowCases.Views.*`，
  导航后永远等不到路由，已改为当前类型；补了 14 个缺失的 ShowCase；新增 `--audit` 模式
  （[GalleryAuditProbe.cs](../../../tools/performances/AtomUI.GalleryPerformance/GalleryAuditProbe.cs)）。
- 新增 [AtomUI.ControlBaseline](../../../tools/performances/AtomUI.ControlBaseline/Program.cs)。
- 9 个 ShowCase（avatar、imagepreviewer、statistic、result、calendar、steps、numberupdown、colorpicker、menu）的专用就绪条件
  已与当前页面不符，审计时用 `--audit-loose-ready` 绕过，条件本身没有修。

## 复现

```bash
dotnet build tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj -c Release --framework net10.0
dotnet build tools/performances/AtomUI.ControlBaseline/AtomUI.ControlBaseline.csproj -c Release --framework net10.0

dotnet .artifacts/bin/Release/net10.0/AtomUI.GalleryPerformance.dll --audit --audit-loose-ready \
  --iterations 12 --warmup 2 --timeout-ms 40000 --json /tmp/atomui-audit.json --markdown /tmp/atomui-audit.md

dotnet .artifacts/bin/Release/net10.0/AtomUI.ControlBaseline.dll --mode fluent --json /tmp/baseline-fluent.json
dotnet .artifacts/bin/Release/net10.0/AtomUI.ControlBaseline.dll --mode atomui --json /tmp/baseline-atomui.json
```
