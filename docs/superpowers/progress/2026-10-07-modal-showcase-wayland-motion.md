# ModalShowCase Overlay 动画逐例检查（2026-10-07）

> 版本边界：以下逐例帧间隔与缓存 A/B 数据均来自禁用 mask 动画之前。用户随后要求 mask 不加动画；该行为调整见文末，不能把旧数据当作当前静态 mask 版本的性能结果。

全部 Overlay 入口已检查，但不能据此说卡顿已经全部解决。两个无 mask 的例子在采到的区间较平稳；17 个带 mask 的动画入口仍有不同程度的长帧。已经修正的提前截断/终态跳变问题，与剩余的绘制节奏问题需要分开看。

## 环境与方法

- Linux Wayland 当前会话，Debug/net10.0，Avalonia 12.1.3；当前工作区 GalleryApplication / WorkspaceWindow，实际客户区 1536×873 DIP，缩放 1.6667。程序集由当前源码重建，包含本次 OverlayDialogPresenter 修复。
- 21 个点击入口各执行首次打开和两次复开：19 个有动画（57 次打开及关闭），2 个主动关闭动画的语义样式例子（6 次打开及关闭）；另检查 2 个常开、无动画的语义预览。
- 保留 Gallery 原始配置、内容、滚动位置对应的真实按钮锚点。基础和静态 Overlay Dialog API 本来就是 IsModal=false；五个静态 MessageBox 没有显式按钮锚点，只淡入淡出。没有统一强加 mask 或 X/Y 动画。
- 在当前源码 Gallery 中通过真实按钮的 Click 路由触发，执行真实示例处理器；该方法不包含操作系统鼠标输入及指针命中测试。
- 可见触发按钮附加一个 1×1 的空 Composition custom visual，在实际 OnRender 中采集服务端 opacity、mask、scale 和 Surface 到窗口的矩阵。采样 visual 每个动画更新请求重绘；这是带少量观测开销的渲染通过间隔，不是屏幕录制，也不是显示器呈现 FPS。
- 200 ms 动画时长保持原样。表内均为毫秒；均值按三次采到的间隔汇总，P95 使用最近秩方法。>25 ms 作为相对 60 Hz（16.7 ms）的长帧观察阈值，不是跨设备性能标准。
- 统计包含可获得的前一初态帧以及首次到达终态的帧，避免漏掉收尾停顿。57 次动画打开中有 14 次、57 次关闭中有 19 次采样在动画已推进后才开始，这些起始片段未被观测；首次观测时间仅是运动开始时间的上界，不能作为准确启动延迟。详细表保留每次结果和前置帧覆盖情况。
- 异步确认、关闭前校验和自动倒计时的业务等待，不混入动画帧间隔。加载示例按自身状态运行：首次 IsLoading=true，后两次复开为 false。
- 初轮 OnAnimationFrameUpdate 数据因加载示例存在高频非绘制回调，仅用于诊断；本报告所有流畅度数值来自重新完整运行的 OnRender 数据。

## 逐例结果

下表“开/关平均”和“开/关最大”均包含可观测的动画边界。评价只适用于这次采到的区间。

| 例子 | 实际动效 | 开/关平均 ms | 开/关最大 ms | 分析 |
| --- | --- | ---: | ---: | --- |
| 基础 Overlay | 无 mask，锚点缩放与 X/Y 位移 | 16.6 / 16.7 | 22.6 / 17.3 | 采到的区间较平稳，开关均接近 16.7 ms；无 mask。三次开启均未捕获初始段，不评价首段。 |
| 异步关闭 | 有 mask，锚点缩放与 X/Y 位移 | 21.9 / 21.2 | 34.5 / 34.1 | 仍有约 33–34 ms 长帧，轻度不稳。按确定后约 3 秒的业务等待不计入动画间隔。 |
| 声明式确认消息框 | 有 mask，锚点缩放与 X/Y 位移 | 30.7 / 27.2 | 38.1 / 35.3 | 开启动画多数帧接近 33 ms，关闭也有明显长帧；复开第三次有所改善，仍不稳定。 |
| 声明式信息消息框 | 有 mask，锚点缩放与 X/Y 位移 | 29.2 / 30.9 | 35.1 / 34.5 | 开关都在约 30 ms，带 mask 的窗口和背景过渡均显得分帧明显。 |
| 声明式成功消息框 | 有 mask，锚点缩放与 X/Y 位移 | 28.6 / 30.1 | 34.9 / 34.9 | 开关均低于平稳 60 Hz 的节奏，第三次尤其接近 33 ms 一帧。 |
| 声明式错误消息框 | 有 mask，锚点缩放与 X/Y 位移 | 32.8 / 30.1 | 37.0 / 35.3 | 开关持续接近 30–33 ms；轨迹正常，绘制节奏明显偏慢。 |
| 声明式警告消息框 | 有 mask，锚点缩放与 X/Y 位移 | 24.8 / 27.1 | 35.0 / 34.7 | 波动较大，第三次打开比前两次更顺；不能用单次较好结果认定稳定流畅。 |
| 静态确认消息框 | 有 mask，仅淡入淡出 | 30.4 / 32.5 | 49.4 / 34.4 | 只有淡入淡出，仍接近 30–33 ms，开启有 49.4 ms 间隔；卡顿并不要求存在 X/Y 位移。 |
| 静态信息消息框 | 有 mask，仅淡入淡出 | 28.5 / 26.8 | 50.9 / 34.8 | 纯淡入淡出仍不稳定，包含首尾边界后发现 50.9 ms 长帧。 |
| 静态成功消息框 | 有 mask，仅淡入淡出 | 32.8 / 31.3 | 49.5 / 49.7 | 开关都出现约 50 ms 长帧；即使没有位移也能观察到过渡不均匀。 |
| 静态错误消息框 | 有 mask，仅淡入淡出 | 29.4 / 31.7 | 34.9 / 34.7 | 开关在 29–32 ms，持续偏慢，未发现位置或缩放异常。 |
| 静态警告消息框 | 有 mask，仅淡入淡出 | 31.9 / 33.3 | 42.2 / 36.5 | 关闭三次都约 33 ms，纯淡出也明显低于 60 Hz 参考节奏。 |
| 加载中 | 有 mask，锚点缩放与 X/Y 位移 | 31.1 / 30.2 | 51.9 / 47.5 | 本轮最大间隔 51.9 ms，仍有明显长帧。首次打开骨架加载，后两次按示例自身状态复开为普通内容；关闭在加载结束后触发。 |
| 自定义页脚消息框 | 有 mask，锚点缩放与 X/Y 位移 | 27.5 / 26.0 | 35.0 / 34.8 | 开关约 26–28 ms，有反复的约 33 ms 间隔；自定义页脚不影响正确关闭，但不算顺滑。 |
| 可拖动对话框 | 有 mask，锚点缩放与 X/Y 位移 | 34.0 / 33.1 | 41.2 / 35.1 | 三次开关均约 33–34 ms，偏慢且稳定复现；X/Y 轨迹连续。本轮评估打开/关闭，不是拖动中的帧率。 |
| 倒计时自动关闭 | 有 mask，锚点缩放与 X/Y 位移 | 29.1 / 32.5 | 50.9 / 47.2 | 打开和自动关闭都有长帧；倒计时等待单独排除，关闭动画自身仍约 32.5 ms 一帧。 |
| 自定义按钮属性（禁用） | 有 mask，锚点缩放与 X/Y 位移 | 32.3 / 31.6 | 50.7 / 35.4 | 打开出现 50.7 ms 长帧，开关均偏慢；页脚保持禁用，通过标题栏关闭按钮完成关闭。 |
| 静态 Overlay Dialog API | 无 mask，锚点缩放与 X/Y 位移 | 16.9 / 16.6 | 24.1 / 17.7 | 无 mask，采到的开关区间接近 16.7 ms，较平稳；开启初始段未捕获，不评价首段。 |
| 异步关闭前校验 | 有 mask，锚点缩放与 X/Y 位移 | 31.0 / 34.3 | 50.2 / 50.2 | 第一次确认被校验拒绝，第二次确认成功关闭；业务校验等待排除后，关闭动画仍约 34 ms，出现 50.2 ms 间隔。 |
| Dialog 语义样式 | 动画关闭 | — | — | IsMotionEnabled=false，三次正常打开/关闭；瞬时切换是示例配置，不作为动画掉帧。 |
| MessageBox 语义样式 | 动画关闭 | — | — | IsMotionEnabled=false，三次正常打开/关闭；瞬时切换是示例配置，不作为动画掉帧。 |
| Dialog 常开语义预览 | 动画关闭、IsPinnedOpen=true | — | — | Overlay owner 已打开、固定常开，配置检查通过；没有开关动画可评估。 |
| MessageBox 常开语义预览 | 动画关闭、IsPinnedOpen=true | — | — | Overlay owner 已打开、固定常开，配置检查通过；没有开关动画可评估。 |

Window 宿主的基础窗口、自定义 Dialog 页脚、窗口/自定义 View 静态 API、窗口语义样式和窗口语义预览不属于本轮 Overlay 范围。自定义页脚的 MessageBox、关闭前校验 API 均属于 Overlay，已包含。

## 每次打开与关闭的节奏

顺序为首次 / 复开 1 / 复开 2。“前置帧”指采到首个运动帧之前的初态，不代表整个最初渲染阶段已被完整记录。

| 例子 | 开平均 ms（3 次） | 关平均 ms（3 次） | 开 P95 ms | 关 P95 ms | 开前置帧（3 次） | 长帧数/间隔数：开；关 |
| --- | --- | --- | ---: | ---: | --- | --- |
| 基础 Overlay | 16.2 / 17.2 / 16.6 | 16.7 / 16.7 / 16.7 | 21.9 | 17.3 | 缺 / 缺 / 缺 | 0/34；0/36 |
| 异步关闭 | 24.5 / 22.0 / 19.6 | 22.0 / 19.8 / 21.9 | 34.4 | 33.7 | 有 / 有 / 有 | 9/30；8/31 |
| 声明式确认消息框 | 33.9 / 32.6 / 26.5 | 33.1 / 33.4 / 20.0 | 35.9 | 35.1 | 有 / 缺 / 有 | 18/21；14/22 |
| 声明式信息消息框 | 30.7 / 30.6 / 27.0 | 28.7 / 31.0 / 33.2 | 34.9 | 34.4 | 缺 / 缺 / 有 | 15/20；17/20 |
| 声明式成功消息框 | 27.2 / 26.2 / 33.3 | 28.7 / 28.7 / 33.3 | 34.9 | 34.7 | 有 / 缺 / 缺 | 15/21；16/20 |
| 声明式错误消息框 | 31.2 / 33.4 / 33.9 | 30.9 / 33.4 / 27.0 | 35.2 | 34.6 | 有 / 缺 / 有 | 19/20；17/21 |
| 声明式警告消息框 | 27.2 / 31.6 / 18.7 | 31.3 / 27.0 / 24.0 | 34.5 | 34.6 | 有 / 有 / 有 | 12/26；15/24 |
| 静态确认消息框 | 30.1 / 30.5 / 30.8 | 33.4 / 33.3 / 30.9 | 35.4 | 34.4 | 有 / 有 / 有 | 17/21；18/19 |
| 静态信息消息框 | 29.4 / 27.9 / 28.3 | 24.9 / 28.5 / 27.2 | 35.6 | 34.5 | 有 / 有 / 有 | 16/24；14/23 |
| 静态成功消息框 | 32.3 / 32.7 / 33.5 | 28.1 / 33.3 / 33.1 | 34.8 | 49.7 | 有 / 有 / 有 | 20/21；15/19 |
| 静态错误消息框 | 29.6 / 29.2 / 29.5 | 33.1 / 29.5 / 33.3 | 34.5 | 34.3 | 有 / 有 / 有 | 18/24；18/20 |
| 静态警告消息框 | 34.7 / 30.4 / 30.5 | 33.4 / 33.4 / 32.9 | 34.4 | 36.5 | 有 / 有 / 有 | 19/21；18/18 |
| 加载中 | 33.1 / 26.2 / 35.6 | 27.3 / 33.1 / 30.9 | 44.9 | 35.0 | 缺 / 有 / 有 | 16/20；16/21 |
| 自定义页脚消息框 | 30.3 / 30.1 / 23.4 | 25.0 / 25.0 / 28.4 | 34.5 | 33.9 | 有 / 有 / 有 | 16/23；13/23 |
| 可拖动对话框 | 34.6 / 33.8 / 33.6 | 33.1 / 33.0 / 33.1 | 41.2 | 35.1 | 有 / 有 / 缺 | 19/19；18/18 |
| 倒计时自动关闭 | 25.8 / 32.0 / 29.9 | 33.1 / 33.1 / 31.5 | 35.8 | 47.2 | 有 / 有 / 有 | 15/23；17/19 |
| 自定义按钮属性（禁用） | 31.3 / 31.5 / 34.3 | 33.5 / 30.9 / 30.7 | 40.3 | 34.7 | 有 / 有 / 有 | 18/21；18/20 |
| 静态 Overlay Dialog API | 16.7 / 16.6 / 17.3 | 16.7 / 16.6 / 16.6 | 17.5 | 17.3 | 缺 / 缺 / 缺 | 0/35；0/36 |
| 异步关闭前校验 | 32.7 / 28.2 / 32.6 | 36.0 / 33.4 / 33.6 | 35.3 | 50.2 | 有 / 有 / 有 | 18/22；18/19 |

## 轨迹、mask 与终态分析

- 57 次动画打开均到达 Scale=1、Surface opacity=1；所有 modal mask 到达 opacity=1。63 次点击入口均完成关闭，未遗留上一次 presenter。
- 对 Surface opacity 和 mask 分别检查：采样区间内未出现反向变化或处于中间值时连续绘制但数值不推进的停滞。不是只依靠 mask 变化判定整个动画正常。
- 锚点动画的 Scale 与 Surface opacity 符合共享缓动对应关系，最大误差小于 4×10⁻⁸；X/Y 与 Scale 的仿射轨迹最大拟合误差小于 0.001 DIP。采到的轨迹中没有独立位置跳变。长帧仍会让相邻可见位置间距变大，轨迹正确不等于观感顺滑。
- 五个静态 MessageBox 在采到的整个开关动画内保持 Scale=1、X/Y 不变，仍有约 30 ms 甚至约 50 ms 的绘制间隔。因此结果不支持“卡顿仅由 X/Y 位移动画叠加引起”；带 mask 场景的共同绘制开销是更值得继续定位的方向。各例背景与内容不同，此处不是 mask 开/关的同场景因果 A/B。

## 本次修复的收益与边界

完成计时现在从动画批次被渲染线程接收后开始，Scale 初始基值也会同步，避免延迟提交时提前停止动画、缩放中心复位后发生跳变。原有 200 ms、缓动、触发按钮锚点和模板保持不变。

| 指标 | 修复前 | 修复后 | 计算 | 改善 | 结论 |
| --- | ---: | ---: | --- | ---: | --- |
| 同一 Wayland 场景延迟提交 150 ms 后的错误终态 | 4/4 次 | 0/4 次 | (4−0)/4 | 100%（仅此复现） | 修复提前截断及残留缩放，不代表帧率提升 |
| 延迟提交后的最终缩放 | 0.784–0.867 | 1.000 | 目标值 1 | 正确到达终态 | X/Y 复位正确 |
| 动画提交等待期间取消 | 追加验证 | 4/4 次通过 | 服务端 Scale、Surface/mask opacity 均为 1 | 正确性验证 | 不是耗时收益 |

该对照使用偏离弹窗中心的 Gallery BasicDialog 按钮，仅在临时对照中启用 modal mask。上面的逐例表采用 Gallery 原始配置，不能把两组数据混作同一性能基线。本轮不声明总体提速百分比，也不声明所有例子已流畅。

## 源码复核与剩余问题（用户要求继续从源头优化）

剩余流畅度优化尚未完成。以下区分已确认的机制与尚未证明的根因，不把大面积重绘本身当作可直接删除的错误。

### 动画链路

- `OverlayDialogPresenter.TryRunCompositorOpeningMotionAsync` 对 Surface 启动 Scale/Opacity，对 mask 启动 Opacity；锚点引起的 X/Y 位移由 CenterPoint 与 Scale 共同形成，没有另外并行执行一条位置动画。
- `CreateSurfaceMotion(...).RunAsync` 仅在 compositor 主路径返回 false 时执行，是互斥回退。`OverlayDialogMaskTheme` 的 Opacity Transition 作用于 mask 控件本身；主路径动画作用于外层 `PART_MaskMotionActor` 的 CompositionVisual，因此这里没有两条 mask 透明度动画叠加。
- `ApplyMaskBounds` 保持 mask 覆盖 owner 的区域。Surface 的位置矩阵在布局/拖动等位置变化时设置，不是开启动画逐帧重新布局。
- `ShadowsAwareContainer.PopupFrameRenderer` 与正文分离，绘制圆角及模糊阴影；逐帧绘制可能重走阴影路径，但定向复用实验没有证明它是本次长帧的主因。

### 实际后端与依赖源码

运行时读取确认：`Avalonia.Skia.SkiaGpuRenderTarget`，`Avalonia.OpenGL.Egl.EglContext`，驱动为 `Mesa Intel(R) Arc(tm) Graphics (MTL) / Intel`。先前根据沙箱内没有 `/dev/dri` 推测软件帧缓冲是错误线索；当前没有走 `WaylandFramebuffer`，其共享内存分配不能用于解释本次卡顿。

依赖版本按 NuGet 元数据核对为 Avalonia 12.1.3，commit `8eeda4f6f546165b3f72e63c9f42247abb306905`。本地参考仓库 commit 不同；下面的外部实现依据使用解析版本对应的官方源码，避免把本地较新实现混作当前依赖。

- Surface/mask 变化形成渲染脏区域。mask 覆盖整窗，因此在变化期间需要重新合成其覆盖区内的背景；这解释了工作量为何扩大，不单独证明主要耗时来源。
- 当前 GPU target 不承诺保留前帧内容，`ServerCompositionTarget` 使用离屏层保存场景，再复制到窗口绘制目标。[当前版本 ServerCompositionTarget](https://github.com/AvaloniaUI/Avalonia/blob/8eeda4f6f546165b3f72e63c9f42247abb306905/src/Avalonia.Base/Rendering/Composition/Server/ServerCompositionTarget.cs)
- GPU 会话退出包括 Skia flush 和 EGL 会话提交；Wayland WSI 在 swap 前请求 frame callback，并设 SwapInterval(0)，已跳过基类额外 waits。不能未经证明就把它描述成双重垂直同步或多余等待。[GlRenderTarget](https://github.com/AvaloniaUI/Avalonia/blob/8eeda4f6f546165b3f72e63c9f42247abb306905/src/Skia/Avalonia.Skia/Gpu/OpenGl/GlRenderTarget.cs)、[WaylandEglWsiSurface](https://github.com/AvaloniaUI/Avalonia/blob/8eeda4f6f546165b3f72e63c9f42247abb306905/src/Avalonia.Wayland/Server/Transient/Rendering/WaylandEglWsiSurface.cs)
- Avalonia 的 compositor render 计时位于 drawing context 的 Dispose 之前，因此该计数不包含末尾的全部 flush/swap 成本。不能以该计数很小就断定 GPU/提交开销很小。[计时范围](https://github.com/AvaloniaUI/Avalonia/blob/8eeda4f6f546165b3f72e63c9f42247abb306905/src/Avalonia.Base/Rendering/Composition/Server/ServerCompositionTarget.cs)

### 同场景对照与淘汰方案

使用相同 BasicDialog 内容、按钮锚点、owner 尺寸和 200 ms 时长，保留 X/Y 位移。各组均在单个进程中交替运行各六次；这是定向诊断，未作为多进程稳定提速的证明。数字仍为已捕获区间的绘制间隔均值，单位 ms。

| 对照 | 基线开/关 | 对照开/关 | 判断及处置 |
| --- | ---: | ---: | --- |
| 同场景无 mask → 有 mask | 16.5 / 16.6 | 22.7 / 21.7 | 支持 mask 对该场景帧节奏有影响；不隔离其与 modal 状态的所有差异 |
| modal 背景内容缓存 | 24.5 / 23.1 | 26.2 / 26.4 | 未改善，删除实验 |
| modal 独立阴影层缓存 | 23.3 / 23.3 | 22.8 / 22.6 | 变化小且 P95 仍约 34 ms，不足以证明稳定收益，删除实验 |
| modal 整体 Surface 层缓存 | 22.4 / 22.9 | 23.7 / 25.2 | 未改善，删除实验 |

加诊断计数的另一次 mask 开关对照同样复现差异（无 mask 16.8/16.9 ms，有 mask 26.4/25.6 ms），也显示绝对数值存在波动，不能跨不同轮次拼接基线计算提速。

一次完整重复开关调用栈采样中，Wayland 工作线程既有绘制调用，也有 drawing context 退出及事件等待；该采样包含空闲段，不能用其占比直接宣称动画期间的 GPU 瓶颈。当前证据不足以在绘制、flush/swap、驱动及桌面 compositor 调度之间给出唯一根因。

三个缓存候选没有达到保留标准，正式源码不增加缓存或截图，也没有冻结背景、减少 X/Y 位移、修改动画时长、降低渲染尺寸或切换用户后端。下一步需要能覆盖 GPU 执行及实际呈现时间的证据，再决定是否调整渲染分层或 Avalonia 后端；不继续堆叠未经验证的控件缓存。

## 回归与交付状态

- 临时探针、跟踪工具、采样文件及中间计划已清理，没有新增常驻测试。清理后执行 compositor motion、motion anchor、Dialog lifecycle 定向测试：30/30 通过，耗时 9 秒。
- 扩展的 Dialog presenter/anchor/lifecycle 定向测试：96 通过、1 失败。失败为 `Modal_With_Stretch_Steps_Keeps_Its_Natural_Size_In_Different_Owner_Widths`，期望中心 Y=325、实际 Y=316；该用例关闭动画。临时还原 HEAD 源码执行同一用例，得到相同失败；修复源码随后恢复，未放宽或修改测试断言。
- 当前 Gallery Debug 构建通过，0 warning、0 error。独立代码审查未发现本次实现的重要缺陷，逐例数据与源码结论也完成独立复核。
- 本轮仅保留动画提交时序与 Scale 终态正确性修复；未证明稳定帧率收益，剩余流畅度问题尚未解决。三个缓存候选均已删除，未提交 git commit。

清理后的定向验证命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~OverlayDialogPresenterCompositorMotionTests|FullyQualifiedName~DialogMotionAnchorTests|FullyQualifiedName~DialogLifecycleTests'
```

本轮停止继续试改的依据为 [atomui-control-performance](../../../.agents/skills/atomui-control-performance/SKILL.md) 的硬约束：“For the same scoped target, three implementation-and-measurement rounds without primary speed metric improvement = stop.” 三种缓存实现与测量均未证明主要帧节奏指标的稳定改善，因此撤除这些实验，保留已验证的正确性修复。这一约束不是流畅度优化完成的依据，也不能替代对实际 GPU 执行与呈现耗时的后续定位。

## 后续行为调整：移除 mask 动画

用户明确要求“mask 先不加动画”。据此移除 compositor 与 fallback 的 mask 淡入淡出，以及 mask 主题的 Opacity transition 和仅服务于该 transition 的内部 IsMotionEnabled 属性。模板、Semantic Part 拓扑、遮罩范围与点击门控保留；mask 打开时直接显示，关闭期间保持固定透明度和输入遮挡，随 presenter 一并移除。Surface 的 X/Y 锚点位移、缩放与透明度动画继续执行。

复用已有首帧测试，把 mask 初态预期更新为 1：改动前失败，实际为 0。另在测试项目 Temporary/StaticDialogMask 中执行临时验证，覆盖 anchored/unanchored × 正常关闭/打开中途关闭四组，读取实际 compositor 服务端 opacity。改动前四组均因 mask 中间透明度失败（0.075–0.251）；改动后连同首帧测试共 5/5 通过，耗时 5 秒。验证同时断言 Surface 在开关中处于中间透明度、任务尚未结束、mask 关闭前仍附着且关闭后移除。

临时四组验证已删除；只更新已有首帧契约测试，不增加常驻用例。此处证明静态 mask 行为和原有 Surface motion 保留，不代表当前 Wayland 呈现帧率或逐例流畅度已经重新测量。

清理后验证：

- 定向运行 Overlay presenter/compositor、anchor、lifecycle、mask closable 与 Semantic Part 六类现有测试，排除本报告前文已在 HEAD 复现的 Stretch Steps 布局失败：118 项中 117 通过、1 失败，耗时 20 秒。失败项为 `DialogLifecycleTests.Closed_Overlay_Session_Releases_Surface_When_A_Custom_Button_Is_Retained`，Surface 弱引用仍存活；该用例 `IsMotionEnabled=false`。未修改断言或生产释放代码，同一工作树单独复查该项通过，耗时 683 ms。组合运行的失败尚未稳定重现，不能将本轮称为一次性全部通过，也没有证明它是已有问题。
- `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --no-restore` 通过：0 warning、0 error，18.09 秒。
- `git diff --check` 通过。独立审查未发现代码层面的重要问题，发现的 mask 动画旧文档表述已同步修正。
- 临时验证与专用日志均清理，未添加永久测试、缓存、运行时反射或资源订阅，未创建提交。

定向命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter '(FullyQualifiedName~OverlayDialogPresenterCompositorMotionTests|FullyQualifiedName~OverlayDialogPresenterTests|FullyQualifiedName~DialogMotionAnchorTests|FullyQualifiedName~DialogLifecycleTests|FullyQualifiedName~DialogMaskClosableTests|FullyQualifiedName~DialogSemanticPartTests)&FullyQualifiedName!~Modal_With_Stretch_Steps_Keeps_Its_Natural_Size_In_Different_Owner_Widths'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-build --no-restore --filter FullyQualifiedName~Closed_Overlay_Session_Releases_Surface_When_A_Custom_Button_Is_Retained
```
