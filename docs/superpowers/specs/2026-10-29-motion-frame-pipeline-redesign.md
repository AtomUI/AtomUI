# Motion 帧流水线重构设计

日期：2026-10-29。状态：用户已确认方案；步骤 2 已实施；步骤 3 的帧驱动、空帧保活、合成变换通道与 Wayland 呈现时间采样已实施；
步骤 4 以内容缓存实施并接入 Overlay Dialog 与 Drawer；另修复 Wayland 启动按缩放 1 首次布局的问题。Windows/macOS 帧计时、NavMenu 根宽度迁移与步骤 1、5 未开始。
本文为过程记录；正式契约见 `architecture/systems/motion/frame-pipeline.md`、`architecture/systems/control-infrastructure/overlay-content-cache.md`
与 `modules/native/linux.md`。
边界：只使用 AtomUI 与 Avalonia 12.1.4 现有包的公开 API，平台层允许已有形态的反射适配；不自编译或维护 Avalonia 包。
不保留 Motion 公共 API 兼容层。源码依据来自本地参考副本 Avalonia `2d79ba55fae1b1e71c69f93a668080c9029e6a31`。

## 目标

一帧动画的成本由四项相乘：每帧推进次数、每次推进的布局量、每次推进的重绘面积、绘制完成后能否赶上刷新。
本方案同时压缩四项，并让 Windows、macOS、Linux 共享收益：

1. 每次显示刷新只采样一次，采样时间为预计上屏时间。
2. 重绘区域只等于真实变化像素，不为实现便利失效到窗口根。
3. 优先使用最便宜的通道：合成层变换 > 渲染属性 > 布局。
4. 帧计时是 AtomUI 的正式平台子系统，不依赖合成批次 `Processed` 反馈。
5. 全窗半透明效果使用独立图层，由系统合成器混合。

## 平台事实

| 平台 | 渲染循环驱动 | 过密采样 | 上屏节奏 |
| --- | --- | --- | --- |
| Windows（WinUI/DirectComposition） | `WaitForCommitCompletion` / DXGI `WaitForVBlank` | 无 | 系统合成器 |
| macOS | CVDisplayLink（`inOutputTime` 未传给 Avalonia） | 无 | 系统 |
| X11 | 60 帧 `SleepLoopRenderTimer` | 无 | 计时器，不对齐 VSync |
| Wayland | 批次到达即唤醒 | 有 | 先等 frame callback 再准备下一帧 |

所有平台都存在：`MediaContext.RenderCore` 在推进未产生提交时退回 16ms `DispatcherTimer`，不对齐刷新；
Avalonia 以推进时刻秒表时间采样，而非预计上屏时间。

## 架构

```text
FrameTimingSource（AtomUI.Native，按平台实现，只用系统 API）
  ├─ Windows：DwmGetCompositionTimingInfo
  ├─ macOS：AtomUI 自有 CVDisplayLink（只读时间，不驱动渲染）
  ├─ Wayland：wp_presentation feedback（复用现有适配器）
  └─ 降级：由推进间隔估算刷新周期并对齐相位
        ↓ 预计上屏时间、刷新周期
MotionFrameDriver（Core，每 TopLevel 一个，只订阅一次 Avalonia 时钟）
  ├─ 按预计上屏时间批量采样，每帧零分配
  ├─ 播放期间的空帧调用 Compositor.RequestCompositionBatchCommitAsync()，避开 16ms 计时器
  ├─ 通道 C：CompositionVisual 的 Translation / Scale / RotationAngle / CenterPoint
  ├─ 通道 R：Opacity（窗口内 mask 使用专用 CompositionSolidColorVisual 子 visual）
  └─ 通道 L：内容视口布局，只失效自身
图层：Modal mask + Dialog 进入带 owner 的透明 Avalonia 窗口；不支持时退回窗口内 overlay
Wayland 专项：渲染循环唤醒对齐 frame callback；提前准备、按回调提交
```

## 关键依据

- 通道 C：`Visual.SynchronizeCompositionProperties` 只覆盖 `Offset/Size/Opacity/TransformMatrix` 等，
  不写 `Translation/Scale/RotationAngle/CenterPoint`（`composition-schema.xml` 公开）。命中测试读取合成回读变换。
  客户端直接写入可读当前值、可自行判定完成，避免 12.1.4 合成关键帧动画缺少完成通知与当前值回读的问题。
  应用 RenderTransform 与动效变换处于不同通道，可删除“独立 RenderTransform 来源”限制和模板隔离 surface。
  代价：命中回读晚一帧；`TranslatePoint` 不含动效变换，与 CSS transform 语义一致。
- 通道 L：`InvalidateArrange`/`InvalidateMeasure` 调用 `InvalidateVisual`，`CompositingRenderer.UpdateCore`
  无差异比较地替换绘制指令，合成端把该节点整棵子树边界加入重绘区域。
- mask：12.1.4 合成端没有保留图层能力；透明度变化会让其下方整窗重新栅格化，合成线程动画也无法降低该成本。

## 实施步骤

| 步骤 | 内容 | 平台 | 状态 |
| --- | --- | --- | --- |
| 1 | 测量：每帧重绘面积（DirtyRects）、每帧推进次数、16ms 计时器路径次数、上屏间隔；Release 基线 | 全部 | 未开始 |
| 2 | 内容视口只失效自身，并精确修复框架排列缺口 | 全部 | 已实施，见[验证记录](../../architecture/systems/motion/verification.md) |
| 3 | MotionFrameDriver、通道 C、空帧保活、帧计时（先 Windows/macOS，Wayland 复用） | 全部 | 驱动/保活/通道 C、Wayland 帧计时已实施；Windows/macOS 帧计时未开始 |
| 4 | mask 期间降低背景重绘成本：改为 BitmapCache 内容缓存（CSD 窗口），独立透明窗口方案搁置；Drawer 同样接入 | CSD 窗口 | 已实施，正式契约见 `architecture/systems/control-infrastructure/overlay-content-cache.md` |
| 5 | Wayland 渲染循环对齐 frame callback，实验开关转正式子系统 | Linux | 未开始 |

删除清单（随对应步骤执行）：`MotionProgress` 与 MotionPlayback 中的 Avalonia `Animation`、
`MotionPresentation` 的 `MatrixTransform` / RenderTransform 路径与透明度 `BehaviorSubject` 绑定、
独立 RenderTransform 报错、NavMenu 根宽度的直接 `Animation`、`AtomUI.Experimental.WaylandFramePacing` 开关。

## 验收

每个平台独立达到有效 FPS ≥59 且 ≥99% 间隔在单刷新周期内；120Hz 设备另测。
上屏数据：Windows PresentMon、macOS Instruments Animation Hitches、Wayland `wp_presentation`。
不以缩短时长、删除效果、冻结背景或改变布局方式达标。

## 风险

- `DwmGetCompositionTimingInfo` 在多显示器/VRR 下返回主输出计时，需按窗口所在输出校准。
- 自建 CVDisplayLink 需随窗口切换显示器重建。
- mask 独立窗口的跨平台输入、焦点与输入法风险最高，必须先通过能力验证。
- 平台层反射随 Avalonia 升级需重新校验版本与成员签名。
