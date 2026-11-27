# Masonry 性能问题调查（2026-11-05）

状态：P0/P1/P2 已修复，待评审；未提交。依赖版本：Avalonia 12.1.5。

## 结论摘要

shortest-column 分配本身不是瓶颈（单次纯面板重排 N=1000 约 1 ms）。严重问题来自两处：

1. **P0 布局死循环**：`MasonryPanel` 测量宽度与排列宽度不一致、且子项 DesiredSize 随宽度变化时，进入永久布局循环。
2. **P1 动效编排随 N 线性放大**：每个位移项各自创建一个 `Animation`，包括视口外项目。

## P0：Measure/Arrange 宽度不一致导致的布局循环

触发条件（同时满足）：

- 面板 Measure 约束宽度 ≠ Arrange 最终宽度。常见来源：
  - 小数测量约束（`Grid *,*` 奇数宽度、小数宽度宿主）。Arrange 经 `LayoutHelper.RoundLayoutSizeUp` 向上取整，Measure 约束不取整。
  - 可横向滚动的 `ScrollViewer`：以无限宽度 Measure，再以 `max(viewport, desired)` Arrange。
- 子项 DesiredSize（宽或高任一维）依赖约束宽度，例如 `Image` Uniform/UniformToFill、Skeleton、换行文本、按约束宽度回报尺寸的内容。

机制：

1. `ArrangeOverride` 在宽度不同时按最终宽度重测子项，子项 DesiredSize 随之变化。
2. `Layoutable.Measure` 调用父级 `ChildDesiredSizeChanged`。面板此时不在 measuring，于是 `InvalidateMeasure`。
3. 下一轮按 `PreviousMeasure`（旧宽度）重测，子项回到旧尺寸；Arrange 又按新宽度重测，如此往复。
4. 达到 `LayoutQueue` 每轮 10 次上限后记录 "Layout cycle detected"。`EndLoop` 把面板重新入队，面板保持 measure-invalid，窗口内任意后续布局请求都会再跑 10 轮。

实测（40 项、4 列，宽度相关子项，临时探针）：

| 场景 | 首次显示子项 Measure 次数 | 面板 Measure/Arrange 次数 | 每次无关布局请求子项 Measure 次数 | Layout cycle 告警 |
| --- | ---: | ---: | ---: | ---: |
| 宿主 Width=800（对照） | 40 | 1 / 1 | 0 | 0 |
| 宿主 Width=800.4 | 1680 | 21 / 21 | 800 | 1 |
| `Grid *,*`，总宽 1001 | 1680 | 21 / 21 | 800 | 1 |
| 横向 ScrollViewer 1600 | 1680 | 21 / 21 | 800 | 1 |
| A/B：面板记住排列宽度（已回退） | 80 | 2 / 2 | 0 | 0 |

Gallery 现状：MasonryShowCase 在缩放 1/1.25/1.5/1.75、窗口宽 1280/1366/1441 下，6 个面板的测量宽度均等于排列宽度（图片已加载），未触发循环。原因是外层 `ShowCaseMasonryPanel` 已有同类修复（3b737a1ae），且示例项占满整行；Gallery 因此掩盖了该缺陷。

A/B 修复方向：Measure 约束宽度不变时，按上次排列有效宽度测量子项；DesiredSize 宽度仍按约束解析，避免无限宽度场景横向 extent 粘滞；约束变化、断点变化或 detached 时清除记录。该方向已验证：循环消失，Masonry 既有 36 个布局/动效测试全部通过。实验改动已回退。

## P1：动效按位移项逐个创建 Animation

`ArrangeOverride` 对每个子项调用 `ApplyItemMotion`。凡是发生位移且当前没有动效的项，都会新建一套对象：`Animation`、2 个 KeyFrame、3 个 `TransformOperations`、`CancellationTokenSource` 和 async 状态机。是否在视口内不影响这一行为。列顶部单项高度变化（例如图片加载完成）会移动同列后续所有项，约 N/列数 个。

实测（N=1000、4 列，headless，不含合成/渲染开销）：

| 场景 | 动效开启 UI 忙碌 | 动效关闭 UI 忙碌 | 分配（开/关） | 最大并发 glide |
| --- | ---: | ---: | ---: | ---: |
| 40 次“图片加载”高度变化，间隔 16 ms | 241–335 ms | 22–33 ms | 32.8–37.6 MB / 2.9 MB | 995 |
| 连续拖拽缩放，30 步 × 4 DIP，间隔 16 ms | 150 ms | 64 ms | 28.4 MB / 8.3 MB | 749 |
| 单次缩放步（约 750 项开始 glide）的布局帧 | 18.9–24.4 ms | 1.4–2.2 ms | — | — |

N=200 时开/关差异处于噪声范围，问题只在大数据量下显著。

候选方向：

- 只对旧/新矩形与有效视口（含余量）相交的项做 glide/appear。每个面板只保留一个 `EffectiveViewportChanged` 订阅，视口外项瞬时就位，可见行为不变。
- 或改用合成层隐式 Offset 动画，UI 线程每帧零开销。测试改写成本更高。

## P2：glide 续滑分支不可达

`ApplyItemMotion` 仅在 `!_itemMotions.ContainsKey(child)` 时调用 `StartGlideMotion`，因此 `StartGlideMotion` 中的“读取当前平移、取消旧动画后续滑”分支永远不会执行。滑动中再次变位的项保留过期偏移，产生视觉跳变。`MasonryMotionTests` 没有覆盖该路径。

## 其他观察

- 非虚拟化：N=3000 简单子项首次显示约 217 ms，同时启动 3000 个 appear 动效。属于架构边界，不是算法缺陷。
- 纯面板重排 O(N·列数)：N=200 约 0.2 ms，N=1000–3000 约 1 ms；单项高度变化 0.7–2.8 ms。

## 证据位置

- `src/AtomUI.Desktop.Controls/Masonry/MasonryPanel.cs`：`MeasureOverride` / `ArrangeOverride` 的重测分支，`ApplyItemMotion` / `StartGlideMotion`。
- Avalonia 12.1.5：`Layout/Layoutable.cs` 中的 `Measure` → `ChildDesiredSizeChanged`，以及 `ArrangeCore` 的 `RoundLayoutSizeUp`；`Layout/LayoutQueue.cs` 中 `Enqueue` 的 10 次上限与 `EndLoop` 重新入队。
- 临时探针：已在修复验证后删除。

## 修复结果（2026-11-05）

改动：

- P0：`MasonryPanel` 记录上次 Arrange 有效宽度；Measure 输入宽度不变时按该宽度测量子项，DesiredSize 宽度仍按约束解析。输入宽度变化、断点变化或 detached 时清除记录。
- P1：面板持有单个 `EffectiveViewportChanged` 订阅；只在有效视口四周各扩一个视口尺寸的范围内创建 appear/glide 动效。视口通知到达后，结束远离范围的运行中动效。
- P2：滑动中再次变位按“当前平移 + 本次位移”续滑。每次动效运行使用独立状态：新状态先预置，再取消旧状态，结束时统一释放预置句柄。这同时修复了 detached 取消后 glide 预置句柄不释放的问题。

前后对照（同一临时探针，单次 headless 运行，smoke 级）：

| 指标 | 修复前 | 修复后 | 变化 |
| --- | ---: | ---: | ---: |
| 宽度不一致首次显示，子项 Measure 次数（40 项） | 1680 | 80 | -95% |
| 宽度不一致时每次无关布局请求，子项 Measure 次数 | 800 | 0 | -100% |
| N=1000，40 次图片加载：UI 忙碌 | 241–335 ms | 108–111 ms | 约 -55%~-67% |
| N=1000，40 次图片加载：分配 | 32.8–37.6 MB | 6.3 MB | 约 -81% |
| N=1000，40 次图片加载：最大并发动效 | 995 | 60 | -94% |
| N=1000，30 步拖拽缩放：UI 忙碌 | 150 ms | 73–77 ms | 约 -50% |
| N=1000，30 步拖拽缩放：最大并发动效 | 749 | 34 | -95% |
| N=1000，单次缩放布局帧 | 18.9–24.4 ms | 10.3 ms | 约 -50% |
| N=3000 首次显示 | 217 ms | 80 ms | 约 -63% |
| N=3000 首次显示：入场动效数 | 3000 | 66 | -98% |

N=200 时变化处于噪声范围。

验证：

- `test.py run --scope iterate --path src/AtomUI.Desktop.Controls/Masonry`：44 通过。
- AtomUIGallery.Tests Masonry 相关测试：23 通过。
- 新增 2 个常驻测试，修复前均失败，修复后通过：`MasonryLayoutTests.Arrange_Width_Different_From_Measure_Width_Does_Not_Cycle_Or_Keep_Stale_Extent` 与 `MasonryMotionTests.Item_Moved_Again_While_Gliding_Continues_From_Current_Visual_Position`。

未做：真实桌面 Gallery 人工走查，以及 ≥10 cold 样本的正式基准。

## 录屏反馈与最终方案（2026-11-05 晚）

用户录屏显示：上一版修复后，拖拽缩放时图片明显落后于布局位置。缩小窗口时列间距变大、第 4 列被裁切；放大窗口时 item 互相重叠。松手后恢复正常。

根因：P2 续滑让每个缩放帧都按“当前位置 + 新位移”重建可见 item 的 glide。item 用 300 ms CubicEaseOut 持续追赶布局位置，并且每帧新建 `Animation`。原代码在滑动中会忽略新位移，滞后反而小。

脚本化渲染验证（Rendering.Tests，Skia headless）：

- 场景：真实 MasonryShowCase 图片示例（本地图片），60 帧拖拽缩放（1280→1040→1280，每步 8 DIP，间隔 16 ms）。
- 指标：每帧统计 item 视觉滞后（RenderTransform 平移）、视觉重叠对数和 UI 忙碌时间，并保存渲染帧。
- 像素对照：同一帧与“禁用动效”渲染帧逐像素比较，以差异像素比例作为视觉偏差。

| 版本 | UI 忙碌 | 每帧最大滞后均值 / 最差 | 渲染帧与正确布局差异（最大） |
| --- | ---: | ---: | ---: |
| 原代码（有 glide） | 3041 ms | 2.1 / 7.0 DIP | 0% |
| 上一版修复（续滑） | 3116 ms | 11.0 / 15.8 DIP | 4.4% |
| 最终：重新布局不做 item 动画 | 2040–2132 ms | 0 / 0 | 0% |
| 参考：完全禁用动效 | 1751–1918 ms | 0 / 0 | — |

最终方案（按用户要求）：

- 重新布局不做任何 item 动画。内容高度、宽度、列数、断点、增删 item 引起的位置变化都直接就位。
- 删除 glide、续滑和全局 `ITransform` 关键帧插值器注册。
- 保留入场淡入（限制在视口附近）与离场 ghost 淡出。
- P0 布局循环修复保持不变。

测试处置：

- 5 个 glide 相关测试（滑动、释放、续滑、宽度变化、零时长）随功能删除，由 `Relayout_Moves_Existing_Items_Without_Animation` 替代。
- 视觉探针为临时测试，已删除。
