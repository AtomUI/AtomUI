# Overlay Dialog mask 动画卡顿问题记录

更新日期：2026-10-29（Asia/Shanghai）。**状态：已实施 BitmapCache 内容缓存（CSD 窗口），渲染线程成本显著下降；
稳定 60FPS 仍未在负载与其他平台下验收。** 最新结论见文末“BitmapCache 重新实施（2026-10-29）”，下文早期段落为历史记录。

本文集中保存 Overlay Dialog＋mask 专项的复现、证据、失败方案和交接边界。
早期 NavMenu/Wayland 调度过程及 Windows 测试清单继续保留在
[原研发日志](2026-10-28-nav-menu-layout-motion-investigation.md)。
本文整理已有测量，没有因本次文档更新重新运行性能实验或回归测试。

## 当前状态与不可越过的边界

| 项目 | 当前状态 |
| --- | --- |
| 工作区 | `/atomware/workspace/projects/dotnet/AtomUI`，`feature/motion`；本次记录时 HEAD 为 `a1f63a29fb54454fdc66ed6da5253aeffbbecaa7`，实际验证对象还包含未提交改动，不能只用 HEAD 标识产物 |
| 保留的实现 | 此前 AtomUI.Native Wayland EGL 帧调度适配器及 Desktop 版本/ABI 校验、桥接和默认启用策略 |
| 当前 mask | 原 `FadeInMotion` / `FadeOutMotion` 更新 `Visual.Opacity`；本轮没有保留新的缓存或合成线程播放实现 |
| 最新无缓存呈现测量 | 预热后每方向 20 次，打开 59.35FPS、关闭 59.76FPS；仍有约 50/33ms 长帧，不能宣称稳定 60FPS |
| BitmapCache 候选 | 绘制成本下降，但透明文字、真正小数 bounds 和圆角出现像素差异；产品候选已撤回 |
| composition mask 诊断 | 没有展示稳定收益，起止时钟与 UI teardown 未对齐；临时原型已清理 |
| 自编译 renderer 路线 | 用户明确禁止自行编译或维护 Avalonia 包；临时源码构建已停止、清理，未启动该宿主，未实施 retention |
| 依赖与提交 | 工作区 Avalonia 依赖、DLL、NuGet 缓存未替换；保留原有未提交改动；未 commit 或 push |

必须保留真实页面、Dialog 内容、文字、背景、布局、裁剪、命中、交互和动画时长。
关闭 mask 动画、缩短动画、删减页面、冻结背景、关闭 ClearType、猜测并补填背景颜色，均不是本问题的合格修复。
后续只在 AtomUI 自身与现有 Avalonia 包公开 API 范围内继续；允许只读依赖源码，禁止转向自维护框架包。
本记录及设计草案中的历史实验许可，均不能覆盖用户后来收紧的边界。

## 问题表现与可复现条件

原始反馈包括 NavMenu 侧栏、三层子菜单和 Overlay Dialog＋mask 卡顿；本专项优先处理最后一项。
真实 mask 与 Dialog 本体同时运动时明显不够连续；用户曾反馈缓存候选“最新一次，看着流畅很多”，
但该体感不能覆盖随后发现的文字、圆角正确性失败。

重建复现时应保持以下步骤和条件：

1. 从当前脏工作区构建并启动 Debug Gallery，显式传入 `--windowing-platform=wayland`，不附加调试器。
   记录 SDK、运行配置、程序集实际路径、MVID、SHA256；不能误启动安装版或旧目录的同名 Gallery。
2. 进入真实 Gallery Modal 页面，保留整页内容与原始 BasicDialog。原诊断宿主加载 Gallery 原始 AppBuilder，
   将 BasicDialog 设为 `IsModal=true` 后走正常开关路径；该示例原本为 modeless，直接打开默认示例不能替代本次 mask 场景。
3. 保持打开 300ms、关闭 200ms，以及原 Surface 动画。独立记录打开与关闭；静态 mask、固定 Dialog 本体仅作临时隔离对照。
4. 确认实际 surface 是否为 `AtomUI.Native.WaylandPreparedFrameSurface`；未设置或 true 默认尝试安装，false 显式关闭。
   兼容性失败仍回退，故开关值和“启用日志”不能独自证明每个实际目标已经接入。
5. 在原生 Wayland、60Hz、5/3 缩放下测量；每进程固定预热 2 次，每方向至少 20 次。
   不并行运行构建、测试、截图或其他诊断窗口，不跨动作空闲拼接，不丢弃长帧。

旧临时诊断宿主已删除，上述是复现条件，不表示仓库中仍有可运行的诊断脚本。
下列各阶段数据有不同统计窗口；必须先核对口径，不能拼接成同一个“当前帧率”。

本节补齐此前没有测量的 mask 渐变。用户批准实时背景缓存草案后曾反馈“最新一次，看着流畅很多”；
该反馈保留为真实窗口体感证据。随后扩展原生像素覆盖发现文字和窗口圆角差异，故撤回本轮缓存产品代码。
**mask 性能修复尚未完成。不能把体感、默认页面近似相同或生命周期单测通过当成全部验收。**
已有 Native 适配器及其默认启用策略保留；本轮没有修改它、Motion clock、NavMenu 算法、依赖 DLL 或 NuGet 缓存。

## 实际产物、场景与口径

- 当前 `feature/motion` 脏工作区，Debug/net10.0，Avalonia 12.1.4 / NWayland 0.11.0。
  独立临时宿主加载当前工作区 Gallery 的原始 AppBuilder、Modal 页面及 BasicDialog 内容，并把该 Overlay
  示例设为 modal。页面、Dialog 内容、Surface 动画和背景保持一致，打开 300ms、关闭 200ms。
- 原生 Wayland、60Hz、RenderScaling=5/3，实际 ClientSize=1300.2×900 DIP，目标 2167×1500 像素。
  GPU 为 Intel Arc (MTL)，Mesa 26.0.8，GL 4.6 Core，samples=0、stencil=8。
- 每份 JSON 保留已加载程序集路径、MVID、SHA256 与实际 surface 类型。开关启用时接入
  `AtomUI.Native.WaylandPreparedFrameSurface`；关闭时为原始 Wayland EGL surface。没有只凭 AppContext 值判定接入。
  最终无缓存宿主中 Gallery、Desktop.Controls、Core、Native 的 DLL SHA256 全部匹配工作区产物。
- 每个独立进程固定预热 2 次，每方向测 20 次，动作窗口 650ms、间隔 80ms。正式呈现测量不与构建、
  测试、截图或其他诊断窗口并行。反馈在实际 attach/提交前申请，按开始绘制时的动作 ID 归属。
  初始四路比较及最终独立比较均保留长帧，discarded=0。
- 整个动作观察窗：同一动作的相邻 presented 时间，不跨空闲段拼接。渐变活动范围：从该动作第一帧
  到出现中间 mask opacity 后的第一个终态帧，打开为 1，关闭为 0 或从根移除；起始未知 opacity 不推造，
  内部长间隔不裁掉，仅排除终态后的静止尾段。两个范围不能直接互比。
- FPS=1000×间隔数/间隔毫秒总和；分位数为 nearest-rank。除 <17.2ms 的计时检查，还检查 presentation
  sequence 增量，报告跨过的刷新次数。另保留首次呈现延迟与每轮结果，绘制完成不等于上屏。

## 适配器 × mask 单变量比较

以下为最初四路运行的整个动作观察窗口径；static 只用于临时因果对照，Surface 动画未关闭。
该临时分支已删除，未作为产品修复。每方向 20 次：

| 实际路径 / mask | 打开 FPS | 关闭 FPS | 打开 P95 / 最大间隔 | 关闭 P95 / 最大间隔 |
| --- | ---: | ---: | ---: | ---: |
| 适配器 / 渐变 | 41.77 | 47.50 | 33.34 / 50.00ms | 33.34 / 83.33ms |
| 适配器 / 静态 | 59.01 | 53.80 | 16.67 / 100.00ms | 33.33 / 83.34ms |
| 原 EGL 路径 / 渐变 | 22.89 | 23.13 | 66.67 / 83.34ms | 66.66 / 233.33ms |
| 原 EGL 路径 / 静态 | 51.71 | 47.36 | 33.33 / 50.01ms | 50.00 / 66.66ms |

只让 mask 渐变、保持 Dialog 本体静止的额外诊断仍有约 16.68ms 的绘制中位数，布局没有随帧发生。
异步 GL_TIME_ELAPSED 查询（没有每帧 glFinish）的 131 个样本：GPU 中位 14.707ms、P95 20.186ms、
最大 24.561ms；composition render 132 个样本中位 14.388ms、P95 19.188ms。
同次少量 Measure/Arrange 观测中位约 0.637/0.187ms；阈值采样不能当作全部控件布局调用数。
GPU 与 CPU/Skia 时间互相重叠，不能相加。证据支持先处理 mask 变化引发的昂贵背景重绘，
没有证明所有卡顿只属于 GPU、Wayland 或 Motion。扩大 GPU 资源缓存到 128MiB 没有给出有价值改善；
按绘制操作分类也没有确认某一个窗口阴影就是唯一根因。

## 实时 BitmapCache 原型收益与局限

初期直接设置 CacheMode 未同步到已附加的 composition visual，属于无效缓存探针，不能作为性能反证。
核对当前依赖源码后，在有效模式变化时显式 InvalidateVisual，真实 server cache 才建立。
随后候选实现为 Window 的当前 ContentFrame 提供引用计数 lease，modal animated 默认宿主获取，
嵌套 Dialog 独立释放，应用 CacheMode 优先级保留，退出后清理。产品候选未关闭或缩短任何动画。

同一后期预热阶段的无缓存对照与缓存候选，按 mask 中间态绘制取样：

| 指标 | 无缓存 | 缓存候选 | 公式 | 改善 | 结论 |
| --- | ---: | ---: | --- | ---: | --- |
| 逐帧绘制区间中位数，mask 中间态 | 12.202ms / 572 样本 | 3.470ms / 583 样本 | (基线−候选)/基线 | 71.6% | 绘制成本下降，候选随后因像素差异撤回 |
| GPU elapsed 中位数，独立详细运行 | 13.411ms / 156 样本 | 8.548ms / 671 样本 | (基线−候选)/基线 | 36.3% | 次数不同的诊断观测，不代表稳定帧率提升 |

后期轻量运行的实际渐变活动区间，每方向 20 次：

| 路径 | 打开 FPS / 单刷新 | 关闭 FPS / 单刷新 | 打开 P99 / 最大 | 关闭 P99 / 最大 |
| --- | ---: | ---: | ---: | ---: |
| 缓存候选 | 59.20 / 366/370 | 58.16 / 246/253 | 33.32 / 49.99ms | 33.34 / 49.99ms |
| 同阶段局部 CacheMode=null 对照 | 59.52 / 367/369 | 58.09 / 236/243 | 16.68 / 50.00ms | 33.33 / 50.01ms |

候选打开有 18/20 轮未漏刷新，关闭 13/20；分别漏 5/8 次刷新。无缓存对照分别漏 3/8 次。
因此不能将最初 41.77FPS 与后期 59.20FPS 的全部变化归因于缓存，后期无缓存也接近 60。
候选首次呈现延迟中位打开 43.85ms、关闭 38.38ms，P95 57.34/54.67ms。
绘制更便宜没有变成可靠的呈现改善，不能仅用平均 FPS 宣称稳定 60。

详细时间线同时出现：关闭撤掉缓存后终态帧绘制 12–21ms；以及仍保留缓存、绘制约 6.49ms、GPU 约 8.84ms，
提交后仍等约 40.06ms 才呈现的帧。后者只是已定位到绘制之后的等待，尚不能判定 driver/compositor 的具体责任。
原始 callback、attach、submit 与 presented 时间均保留，未通过删尾帧把关闭恢复成本隐藏掉。

## 原生像素失败与撤回理由

用实际窗口 GL framebuffer 在 settled open 后读取 RGBA，对比完全相同的对照帧；读取运行不计入性能数据。
这不是 RenderTargetBitmap，也不代替真实窗口视觉与输入验收。

| 原生场景 / 实验 | 不同像素数 | 最大通道差 | 判断 |
| --- | ---: | ---: | --- |
| 完整 Gallery 默认尺寸 | 22 / 3,250,500 | 1 | 仅该场景近似相同，不代表普遍正确 |
| 初始缓存的小数缩放 resize | 46,150 | 124 | 整层重采样，失败 |
| 极小 RenderAtScale guard 后 resize | 20 | 1 | 只消除近整数浮点下溢 |
| 同 guard 的 live 前景文字变化 | 22 | 1 | 更新可见，但不足以覆盖透明文字 |
| 稀疏小数尺寸 / transformed child | 456 | 121 | 真正小数 subtree bounds 仍失败 |
| 稀疏透明文字＋完整透明 extent | 1,968 | 96 | 尺寸完整后文字仍变化，失败 |
| 自定义 ContentFrameLayer 背景＋extent | 1,960 | 87 | 仍有文字差异，失败 |
| 父层补相同 opaque 背景，未启缓存 | 40 | 133 | 底部圆角覆盖变化，失败 |

当前 Avalonia 精确源码 `2d79ba55fae1b1e71c69f93a668080c9029e6a31`：

- `src/Avalonia.Base/Rendering/Composition/Server/ServerCompositionVisual/ServerCompositionVisualCache.cs`
  的尺寸计算将 scaled subtree bounds 截断成整数，非整数判断把 fWidth 与自己比较；最后再把整个 bitmap
  映射回原 bounds。`RenderAtScale=1.00000001` 仅修复近整数下溢，对真实小数 bounds 无效。
- 同文件从透明 clear 的新 offscreen layer 重画内容，再回绘。透明背景上的 glyph 不必与原窗口中、
  已有背景上的 glyph 等价；原生图中出现 LCD 颜色边缘与灰阶边缘差异，即使 EnableClearType=true。
  `SkiaBackendContext.CreateOffscreenRenderTarget` 的开关只是允许 LCD，不证明实际 glyph pixels 相同。
- `ServerCompositionVisual.Update.cs` 的 subtree bounds 来自绘制与子树的并集及裁剪，不保证是完整、
  物理像素对齐的缓存范围。单纯透明 extent 或把目标上移到父 Panel 不建立这项保证。
- AtomUI 的 CSD `PART_WindowFrame` 实际背景由 drawn-decorations theme 设置，不能仅从 Window.Background
  推断。`WindowVisualLayerClip` 的 trailing clip 扩展也不是 frame paint 的精确边界，重复 opaque 填充会改圆角覆盖。

这属于缓存边界与背景合成方式的问题，不能再用 Gallery 特判、关闭 ClearType、改裁剪或补猜测颜色覆盖。
源码审查还发现普通应用 Style 在弹窗期间加入时被 class-trigger setter 压住，改成普通优先级的附加状态 binding 后
临时用例通过；空模板/失败换模板也补齐释放。但这些生命周期改进无法抵消原生渲染失败。

已仅撤回本轮在此前干净的 Window.cs、WindowTheme、OverlayDialogPresenter 中添加的 cache wiring，删除两个
新增 cache helper，恢复四份控件正式文档。已有 Native、Desktop.Platform 和模块文档逐文件 SHA256 前后相同。
设计草案保留为有日期的失败/决策记录，不再称为已实现契约。

## 无缓存收尾对照与合成线程诊断

撤回缓存后，又在相同工作区 DLL 上比较原 UI mask 与临时 composition mask，每方向 20 次。
两边都关闭逐属性字符串日志，保留实际 server opacity、attach 状态和 presentation；未并行构建：

| 路径 | 打开 FPS / 单刷新 | 关闭 FPS / 单刷新 | 打开最大间隔 | 关闭最大间隔 |
| --- | ---: | ---: | ---: | ---: |
| 原 UI mask，无缓存 | 59.35 / 364/367 | 59.76 / 248/249 | 49.99ms | 33.34ms |
| 临时 composition mask，无缓存 | 58.72 / 361/368 | 59.76 / 247/248 | 49.99ms | 33.33ms |

原 UI 路径首次呈现延迟中位打开 44.63ms、关闭 41.61ms；P95 为 54.37/60.29ms。
这一单次预热基线达到既定 ≥59FPS / ≥99% 单刷新间隔的统计阈值，但未产生新的产品修复，
仍有 33–50ms 长帧；初始与后期运行差异明显，不能凭这一进程声称问题已解决。

composition 诊断仅把 mask 的 MotionProgress.Tick 停止写 UI opacity，再以原线性曲线的剩余名义时长在 server 求值；
Dialog 本体、背景树和原 UI progress engine 均保留。它并非完整的合成线程 Motion 实现：
server 的起始/完成时钟与 UI session teardown 未对齐，关闭最后 attached 样本 opacity 中位仍约 0.105。
因此这些数据只说明本次隔离实验没有展示收益，不能作合成线程播放正确性的证明，也不能排除未来正确实现的价值。
该原型没有进入产品，没有擅自替换全局 clock。

## 测试处置、清理与后续边界

临时测试位于 `tests/AtomUI.Desktop.Controls.Tests/Temporary/OverlayBackgroundRetention/`，真实编译发现执行。
初始无缓存及人为禁用 lease 的 RED，之后修正 effective cache、普通 Style 优先级和空模板释放的 RED/GREEN 均已记录；
最终 14/14 通过，覆盖真实动画关闭完成前持有、嵌套 lease、运行时选项、应用 local/style cache 接管、取消、异常、
owner close、重模板和 live text/resize 的控件状态。这些是专项生命周期用例，无法保护本次 native 像素问题；
随候选撤回而删除，不晋升常驻，全局测试净新增 0。

清理后精确验证：

- Window/Dialog filter：497 通过、4 失败、共 501、0 skipped，约 20 秒。失败仍是上节已独立复现的四项
  Headless 位置问题及相同 9 DIP 偏差，未当作本轮新增回归，也未扩展修复它们。
- `WindowingPlatformDetectionTests`：29/29 通过；复用同一最新构建产物，不重跑全量。
- 测试构建有既存两处 NavMenu xUnit1051 和 NU1900 漏洞源连通性警告；未改 NuGet 源或隐藏警告。
- 候选未获原生正确性验收且已删除，未为其继续执行 AOT publish；不能沿用上一阶段适配器 AOT 结果宣称
  新缓存已通过 AOT。当前保留适配器的源文件未变，原有 AOT 证据仍仅对应原先边界。

必要呈现 JSON、程序集 manifest、统计汇总、原生失败图与红绿/最终日志保留在
`.artifacts/performance/overlay-mask-20261029/`，其中 `measurement-index.json`、`candidate-disposition.json`、
`framebuffer-comparison.json` 和 `composition-diagnostic-comparison.json` 分别说明口径、撤回边界、像素与时钟限制。
临时宿主、测试及诊断代码按清理流程移除；仅保留必要结果，不保留可运行工程。

关于保留“已合成背景”的后续草案及其撤回，见
[背景保留设计记录](../specs/2026-10-29-overlay-dialog-background-retention-design.md#withdrawn-renderer-source-experiment-retain-the-composed-backdrop)。
用户已明确禁止自行编译或维护 Avalonia 包；该草案不能作为继续实施渲染器改动的授权。

## 已确认、尚未确认与排除范围

| 判断 | 证据与边界 |
| --- | --- |
| 原 Wayland 调度路径影响连续性，现有适配器有收益 | 初始四路对照均记录实际 surface 与呈现反馈；不能用侧栏 57–59FPS 替代 mask 验收 |
| mask 渐变引发的背景绘制/GPU 工作是有测量支持的主要开销之一 | 静态 mask 对照、固定 Dialog 的 mask-only 实验、异步 GPU 查询以及缓存前后绘制耗时共同支持；不能据此判定唯一根因 |
| 本次 mask-only 卡顿无需持续布局即可发生 | Dialog 本体静止时仍有约 16.68ms 绘制中位数；不等于布局在其他动画中没有成本 |
| 减少绘制耗时未保证呈现连续 | 缓存候选仍有漏刷新，存在绘制较快而提交后等待约 40.06ms 的帧 |
| 尚未区分的因素 | UI 采样与合成批次节奏、真实提交/回调时序、GPU/driver/compositor 等待，以及不同进程预热与诊断开销造成的结果变化 |
| 不能下的结论 | “只属于 Wayland”“只属于 Motion”“某一个阴影是根因”“composition 动画没有价值”“平均接近60即已修复” |

当前已证实若干成本和错误边界，但尚未形成同时满足外观、生命周期和稳定呈现的产品根因修复。

## 精确验证命令与既有失败

以下是清理候选和临时测试后**此前实际执行**的聚焦命令及结果，本次文档整理没有重跑：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Window.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Dialog.'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-build --no-restore --filter FullyQualifiedName~WindowingPlatformDetectionTests
```

Window/Dialog 共 501 项：497 通过，4 失败，0 skipped。4 项失败的完整标识为：

- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogPresenterTests.Modal_With_Stretch_Steps_Keeps_Its_Natural_Size_In_Different_Owner_Widths`
- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogOwnerResizeTests.Non_draggable_natural_dialog_restores_its_viewport_and_center_through_owner_resize(dark: True)`
- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogOwnerResizeTests.Non_draggable_natural_dialog_restores_its_viewport_and_center_through_owner_resize(dark: False)`
- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogOwnerResizeTests.Owner_resize_temporarily_clamps_surface_without_rewriting_user_offsets`

这些失败已在修改前 `a1f63a29f` 的独立源码副本中以相同 9 DIP Y 偏差复现
（预期 325/410/530，实际 316/401/521），属于本机 Headless 基线问题。
不能报告模块回归全部通过，也不能误报为本次新增回归。
平台聚焦测试为 29/29 通过；本轮未自行运行全量回归。

## 证据目录与阅读顺序

原始结果是本地取证材料，不是可运行工具或产品源码；`.artifacts` 不保证随文档提交分发。
先前帧调度证据保留在 `../../../.artifacts/performance/wayland-native-pacing-20261028/`。
本专项以下路径均相对于仓库根的 `.artifacts/performance/overlay-mask-20261029/`：

| 文件 | 用途 |
| --- | --- |
| [measurement-index.json](../../../.artifacts/performance/overlay-mask-20261029/measurement-index.json) | 运行参数、原始结果摘要、哈希与实际接入 surface 的总索引 |
| `on-fade.json`、`on-static.json`、`off-fade.json`、`off-static.json` 及对应 `.summary.json` | 初始适配器开关 × mask 静态/渐变的四路原始呈现对照 |
| `maskonly-isolate.json`、`gpu-query-fade.json` | Dialog 本体固定的隔离实验、异步 GPU elapsed 与时间线 |
| `product-on-fade.json`、`product-on-uncached-fade.json` 及对应 `.active-summary.json` | 已撤回缓存候选与同阶段无缓存的 20 轮渐变活动区间比较 |
| `product-timeline.json`、`product-uncached-timeline.json` | 绘制、GPU、提交与实际呈现的独立详细诊断；两份运行次数不同 |
| [framebuffer-comparison.json](../../../.artifacts/performance/overlay-mask-20261029/framebuffer-comparison.json) 与 `framebuffers/*.png`、`*.json` | 原生 framebuffer 像素失败、几何及图像哈希；截图/readback 不混入性能测量 |
| `clean-uncached-ui.json`、`composition-mask-diagnostic.json` 及对应 `.active-summary.json` | 最终无缓存 UI mask 与临时 composition mask 的独立 20 轮运行 |
| [composition-diagnostic-comparison.json](../../../.artifacts/performance/overlay-mask-20261029/composition-diagnostic-comparison.json) | 两组 manifest、时钟接管限制、关闭前最后 opacity 等解释材料 |
| [candidate-disposition.json](../../../.artifacts/performance/overlay-mask-20261029/candidate-disposition.json) | 候选撤回范围、恢复到原状态的文件、保留文件 SHA256 |
| [final-window-dialog.log](../../../.artifacts/performance/overlay-mask-20261029/final-window-dialog.log)、[final-platform.log](../../../.artifacts/performance/overlay-mask-20261029/final-platform.log) | 清理后 497/4 与 29/29 的原始聚焦验证日志 |

原始事件包含动作 ID、线程/时戳、UI 属性或布局采样、draw/server opacity/attach/cache 状态，
以及反馈的 submit、presented、refresh、sequence 等字段。绘制与 GPU 查询并非每个文件都同时启用；
分析时检查该文件字段与采样范围，不能把不同运行的时间相加。

## 用户收紧实现边界后的处置

用户明确要求“不要自己编译 avalonia 的方式，我们不能自己维护 avalonia 包”。
该要求覆盖此前对独立渲染器源码原型的许可，后续不能因历史草案再启动这条路线。

当时仅完成 `/tmp/atomui-renderer-retention-20261029` 中精确 12.1.4 Avalonia.Base 与诊断宿主的构建，
尚未启动宿主、未实现 renderer retention、没有新的原生窗口或性能结果。
该目录的导出源码、程序集和宿主均已删除；此前 `/tmp/atomui-mask-investigation-20261029` 也已删除。
工作区 Avalonia 依赖、安装 DLL、NuGet 缓存未替换，未创建需要长期维护的 Avalonia 包或分支。

本专项撤回后，Window/Dialog/Motion 没有留下新的优化产品代码；保留的是已有 Wayland 适配器。
保护清单中的 12 份既有源文件/模块文档 SHA256 在清理后与输入一致。
临时测试净新增 0，未 commit/push，未重置工作区。

## 未完成验收与继续调查的条件

| 项目 | 必须补齐的证据 |
| --- | --- |
| 稳定 60Hz 呈现 | 同一真实场景、同配置、每方向至少20次，报告 FPS、P95/P99/最大间隔、单刷新比例、漏刷新、逐轮结果与首次呈现延迟；跨独立进程核对差异 |
| 呈现判定 | 延续已声明的有效 FPS ≥59、≥99% 单刷新间隔门槛，结合 presentation sequence 和长帧；单次预热结果达到门槛不代表已有修复或稳定复现 |
| 外观与实时背景 | 原生窗口检查透明文字、分数缩放与变换、resize、自定义 frame layer、主题/文字/滚动更新、圆角裁剪；保留原 alpha 与内容 |
| 生命周期与交互 | 快速反向/取消、嵌套 Dialog、模板替换、owner resize/关闭、失败释放；真实鼠标命中、mask 点击关闭、键盘焦点与滚动，不能只用脚本状态代替 |
| 真实闪烁验收 | 真实窗口观察与必要录屏/人工复核；离屏截图或 framebuffer 对比无法证明交互和时间上的闪烁已消失 |
| Windows | 原日志已有对照测试清单，尚未执行；不能将 Linux 结果推广为 Windows 结论 |
| 120Hz / AOT | 无真实120Hz实测；新修复若出现，需做与其边界匹配的 AOT/裁剪验证，不能沿用旧适配器结果代替 |
| 最终清理与聚焦验证 | 临时用例先验证再删除，只对实际修改模块执行精确 filter 或带明确路径的 iterate；未授权不跑全量 |

下一轮先核对当前工作区产物、实际目标和呈现时间线，在现有依赖与 AtomUI 实现范围内定位剩余成本。
优先把 UI 属性推进、背景绘制/GPU 与绘制后等待关联到同一动作和帧；需要提出新方案时，先解释其
正确性边界与可测量依据。没有候选同时满足上述条件前，本问题保持“未修复”。

## BitmapCache 重新实施（2026-10-29）

用户提出 BitmapCache 仍可行。重读 12.1.4 `ServerCompositionVisualCache` 后，早期三项像素失败均有可控成因：

| 早期失败 | 源码成因 | 本次条件 |
| --- | --- | --- |
| 小数 bounds 整层重采样 | 层尺寸为截断的 `bounds×scale`（向上取整判断自比较恒为真），再拉伸回原 bounds | 缓存根位于整设备像素，`ClipToBounds` 固定子树 bounds；`RenderAtScale=1.00000001` 防截断 |
| 透明文字差异 | 层先清为透明；文字落在透明层像素上与直接绘制在 frame 上的结果不同 | 缓存内首个子元素绘制 frame 的实际画刷（不猜颜色），使所有文字位于不透明底；`EnableClearType=true` |
| 圆角覆盖变化 | 同一抗锯齿圆角被父层与缓存各画一次 | 底板按 frame 圆角内缩 2 设备像素，frame 的抗锯齿边缘只画一次 |
| （新发现）整层错位 1 像素 | `SnapsToDevicePixels` 对世界原点直接 `Math.Floor`，37.2×5/3=61.99999999999999 被取到 61 | 关闭框架吸附；启用前自行校验原点/尺寸为整像素（容差 0.001） |

实现：CSD 模板内容根命名为 `PART_ContentCacheHost`，首个子元素为隐藏的 `WindowContentBackdrop`；
`Window.AcquireContentCache()` 为引用计数租约，仅在 frame 画刷为完全不透明纯色、无 WindowFrameLayer、内容根位于整设备像素时启用，
否则返回 null 保持原路径。`OverlayDialogPresenter` 只在 modal mask 打开/关闭动画期间持有租约。非 CSD（X11、macOS）模板没有该内容根，暂不启用。

验证（本机 Wayland、60Hz、5/3、Debug、真实 Gallery Modal 页）：

- Headless Skia 临时比对（4 种缩放）：带底板时缓存与直接绘制仅 160–313 像素差，基本 ±1；关闭 ClearType 则上万像素、最大 141。
- 原生 GPU 回读（提交前读取后缓冲区，2167×1500）：直接/直接 0 差异；缓存/直接 **3 像素差、最大 1**；释放后 0 差异。
  首轮因框架吸附整层错位 1 像素（159,406 像素差），关闭吸附后达到上述结果。
- Overlay Dialog＋mask，远端触发点，每变体每方向 20 次，同一进程 4 组交替：

| | 渲染线程中位 | P95 | 最大 | >10ms 帧 |
| --- | ---: | ---: | ---: | ---: |
| 无缓存 | 9.3–9.7ms | 13.2–15.1ms | 16.8–27.5ms | 48–138 |
| 缓存 | **2.3–2.6ms** | **4.5–8.1ms** | **6.7–13.0ms** | **0–3** |

渲染时间为 `avalonia.comp.render.time`（渲染线程，不含 GPU 执行）。本组进程两种变体上屏均约 59FPS；此前同机无缓存曾降至 42–48FPS，
说明上屏结果受机器负载影响大，缓存提供的是每帧 8–14ms 的余量而非保证。缓存组仍有 0–7 次漏刷新，来源未定位到渲染线程。
未验证：Release/NativeAOT、120Hz、X11/macOS（未启用）、Windows（CSD 模板理论可用）、真实交互与视觉录像。
临时 headless 用例、原生探针和回读/内容标注钩子均已删除；无新增永久测试。

