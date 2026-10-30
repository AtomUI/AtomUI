# 覆盖层内容缓存

本文拥有 Window 内容缓存的契约：全窗口覆盖层（Overlay Dialog 遮罩、Drawer 面板与遮罩）动画期间，
窗口内容以 BitmapCache 合成，而不是每帧重绘。消费者为 [Modal](../../../controls/desktop/feedback/modal/implementation.md)
与 [Drawer](../../../controls/desktop/feedback/drawer/implementation.md)，宿主为
[Window](../../../controls/desktop/window/window/implementation.md)。

## 1. 问题

Avalonia 12.1.4 的合成器没有保留图层能力。遮罩透明度或面板位置每变化一帧，脏区内其下方的整页内容都要重新栅格化；
真实 Gallery 页面在 5/3 缩放下渲染线程中位约 9ms、慢帧超过 20ms。在合成线程播放透明度也不能减少这部分重绘。

## 2. 框架缓存的行为与精确条件

依据 `ServerCompositionVisualCache`（12.1.4）：

| 框架行为 | 不满足条件时的后果 | 本契约的条件 |
| --- | --- | --- |
| 层尺寸为截断的 `子树 bounds × scale`（向上取整判断恒为真），再拉伸回原 bounds | 小数尺寸整层重采样 | 缓存根位于整设备像素（容差 0.001），`ClipToBounds` 固定子树 bounds；`RenderAtScale = 1 + 1e-12` 防止浮点截断丢失一像素 |
| `SnapsToDevicePixels` 对世界原点直接 `Math.Floor` | 61.99999999999999 取到 61，整层错位 1 像素 | 关闭框架吸附，由宿主自行校验整像素 |
| 层先清为透明再绘制内容 | 文字落在透明像素上，与直接绘制在 frame 上的结果不同 | 缓存内首个子元素绘制 frame 的实际画刷，使所有文字位于不透明底；`EnableClearType = true` |
| 同一抗锯齿圆角若在缓存内外各画一次 | 圆角边缘覆盖率翻倍 | 底板按 frame 圆角内缩 2 设备像素（方角 frame 不内缩） |

## 3. 结构

CSD Window 模板的内容根命名为 `PART_ContentCacheHost`（`Panel`），其首个子元素为默认隐藏的 `PART_ContentBackdrop`
（`WindowContentBackdrop`）。覆盖层位于 `VisualLayerManager` 的图层中，是内容根的兄弟，不在缓存内。

`WindowContentBackdrop` 在启用时读取装饰层 `PART_WindowFrame`（位于 `TopLevelHost` 下，不在 Window 模板内）的
实际 `Background` 与 `CornerRadius`，在自身坐标系绘制内缩后的圆角矩形，不硬编码或推测颜色。

## 4. 租约

`Window.AcquireContentCache(Visual overlay)` 返回引用计数租约；首个租约启用缓存，最后一个释放时恢复直接绘制。
以下任一情况返回 null，调用方保持直接绘制：

- 模板没有缓存根与底板（非 CSD 模板，例如 X11、macOS）；
- `overlay` 位于缓存根之内（例如 Drawer 图层被注入页面内的 `ScrollContentPresenter` / `ContentControl`），此时每帧都会失效缓存；
- frame 画刷不是完全不透明的纯色，或 `WindowFrameLayer` 有内容；
- 缓存根原点或尺寸不在整设备像素上。

消费者只在动画期间持有租约：Overlay Dialog 在 modal 遮罩打开、关闭动画期间；Drawer 在打开、关闭 Motion 全程（无遮罩时面板滑动同样重绘背景）。
模板重新应用时缓存状态复位。

## 5. 验证结论与已知差异

本机 Wayland、5/3、Debug、真实 Gallery：

- 原生 GPU 回读对照（提交前读取后缓冲区，2167×1500）：Modal 页缓存与直接绘制 3 像素差、最大 1；
  Drawer 页 7 像素差，其中 4 个最大 34 级，位于主按钮内白色图标边缘，成因未定位，仅在缓存生效的动画期间且处于遮罩下。
- 渲染线程 `avalonia.comp.render.time`（同一进程交替对照）：Overlay Dialog＋mask 中位 9.3–9.7ms → 2.3–2.6ms，>10ms 帧 48–138 → 0–3；
  Drawer 中位 8.4–10.9ms → 2.4–3.7ms，>10ms 帧 54–121 → 1–14。

未验证：Release/NativeAOT、120Hz、Windows（使用 CSD 模板，应可启用）、X11/macOS（未启用）。上屏帧率受机器负载影响大，
缓存提供每帧余量，不是 60FPS 保证。
