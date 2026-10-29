# Wayland 帧准备与提交适配层设计

日期：2026-10-28；更新：2026-10-29。状态：第一阶段已实现；用户进一步明确要求在兼容的 Wayland 后端默认启用，
实测仍未达到全部性能验收条件，未提交。结果见[调查记录](../progress/2026-10-28-nav-menu-layout-motion-investigation.md)。

## 目标与证据

利用 `AtomUI.Native` 承接原生 Wayland 的 EGL 帧调度能力，通过受控反射接入当前 Avalonia，
解决等待 frame callback 时无法准备下一帧、持续布局动画容易落入约 30FPS 节奏的问题。
保持 NavMenu 的真实宽度/高度布局、文字排版、交互和动画时长。验收包含侧栏展开/收起和三个可展开层级的子菜单。

已有[调查记录](../progress/2026-10-28-nav-menu-layout-motion-investigation.md)证明了调度干预的因果关系：
提前准备一帧、收到 callback 后再提交，同时合并过密的 UI pulse，临时 DLL 原型的侧栏复测约 59FPS，
三层子菜单长轮次约 57FPS；恢复原依赖又回到约 27–30FPS。这不是纯反射实现的测试结果，也没有达到全部场景稳定 60FPS。

本设计先把已验证的渲染干预改成可独立验证的适配器。120Hz、Modal mask、文字和背景闪烁仍须另行取证，
不能依据侧栏 FPS 宣称这些问题已修复。

## 方案比较与选择

| 方式 | 收益 | 代价与结论 |
| --- | --- | --- |
| 修改并维护 Avalonia Wayland 源码补丁 | 能直接修改 readiness、session、callback 的完整契约 | 需要维护和发布框架依赖；保留为上游修复路径 |
| AtomUI.Native 静态适配器 + Desktop 受控反射 | 不改已发布 Avalonia DLL，能够接管 EGL session 与 callback 的连接点 | 依赖内部 ABI，必须锁定版本并验证 NativeAOT；按用户提出的方向优先尝试 |
| 仅在 Native 拦截 `eglSwapBuffers` 或增加定时器 | 接近原生调用 | 不会解除托管层的 readiness 门控，无法单独修复已定位原因，不采用 |

选择第二种方向。用户在本设计提出后指示“开始实现”，按下面的分层与阶段划分执行；第二阶段尚未实施。

`AtomUI.Native` 当前是依赖 Avalonia、NWayland 的 C# 包，不必为了这个问题新增 C++ 库。
运行路径使用静态编译类；不用 Harmony、运行时 IL 改写、Reflection.Emit、动态代理或替换 NuGet 缓存。

## 范围与模块责任

第一阶段只改变原生 Wayland 的 EGL WSI 绘制/提交路径，保留当前动画采样。
通过单变量测试确定收益，再决定是否进入第二阶段的动画采样合并。

| 模块 | 建议组件 | 责任 |
| --- | --- | --- |
| Desktop.Controls | `WaylandFramePacingCompatibility` | 已知内部类型/成员探测、版本校验、构造绑定后的桥接对象 |
| Desktop.Controls | `WaylandFramePacingRenderInterface` | 转发 Skia render interface/context，在 surface 进入 Skia 时进行包装 |
| Desktop.Controls | `DesktopAppBuilderExtensions` | 在 Wayland 初始化前安装；记录启用/拒绝原因；保持现有 dispatcher clock 修正 |
| Native/Linux | `WaylandPreparedFrameSurface`、target/session | 实现静态 `IGlPlatformSurface` 适配，保持 GL 状态、准备和提交边界 |
| Native/Linux | `WaylandFramePacer`、`WaylandFrameCallbackRelay` | 每 surface 的有界待提交状态、callback 转发、生命周期清理 |
| Core/Motion 与 NavMenu | 第一阶段无改动 | 保持动画参数、布局语义和现有调用路径 |

遵循 [Native Linux 边界](../../modules/native/linux.md)：Native 不进行框架发现或决定启用策略。
Desktop 把绑定好的方法/字段访问封装成 internal bridge 传给 Native；这些 bridge 在 worker 上调用。
既有 `WaylandWindowReflectionExtensions` 绕过 worker marshalling 的做法不作为新实现范式。
不建立 Native 到 Core 的依赖，不新增 public API。

## 接入点一：包装 render interface

当前 `AppBuilder` 先运行 rendering initializer，再运行 windowing initializer。
在 `UseWaylandBackend` 包装函数中，调用原 `initializeWayland()` 之前：

1. 探测已安装的 Skia `IPlatformRenderInterface` 和全部兼容条件。
2. 安装静态 forwarding decorator，除 backend context 创建外，其余能力原样转发。
3. 包装 `CreateBackendContext` 的返回值。在 `CreateRenderTarget` 和 `IsReadyToCreateRenderTarget`
   两条路径中使用同一 surface 映射，避免创建和 readiness 判断不一致。
4. 仅包装确切的 `WaylandEglWsiSurface`，其余 surface 保持原顺序和实例；dmabuf、软件、X11 不改。

`PlatformRenderInterfaceContextManager` 在初次使用和 context 重建时都通过注册接口创建 backend，
因此新窗口、新 Popup 和 context 重建可以走同一入口。不替换窗口 `_handle` 或只修改首个窗口的缓存。
context 的 feature 查询、PublicFeatures、offscreen target、IsLost、Dispose 等行为完整转发。
同一 surface 的包装实例保持身份稳定；context 释放不得提前释放由窗口拥有的协议对象。

## 接入点二：原 EGL target 保有资源，自定义 session 控制 swap

保留原 `WaylandEglWsiSurface.CreateGlRenderTarget(context)` 创建的 target，让原实现继续拥有
`wl_egl_window` 和 `EglSurface` 的创建、resize、销毁。反射绑定其 `EnsureWindow(PixelSize)`，
调用后读取 `_eglSurface`。自定义 target 不调用原 `BeginDrawCore`，因为其中会再次检查阻止提前绘制的状态。

新 session 遵循原 WSI 的 GL 初始化契约：检查 context loss、MakeCurrent、绑定默认 framebuffer、
设置 viewport 和适用的 GL_BACK 状态，并保存恢复上下文的 lease。保持原 WSI 的 SkipWaits 行为。
session Dispose 执行 flush 并恢复上下文；由 pacer 决定立即提交还是占据唯一的待提交槽。
不调用原 session Dispose，因为它无条件执行 SwapBuffers，替换其 beforeSwap delegate 不能阻止 swap。

提交时重新 MakeCurrent，在同一个 worker 同步调用段内依次：

```text
检查待提交帧仍有效
→ SwapInterval(0)
→ 调用原 surface.OnBeforeNewBufferAttached(sceneInfo)
→ 包装该调用创建的 frame callback listener
→ EglSurface.SwapBuffers()
→ 恢复 GL context
```

原 `OnBeforeNewBufferAttached` 继续拥有 configure ack、window geometry、viewport、buffer scale、
子 Popup 初次映射等协议状态。它必须紧邻真正的 swap，不能在提前绘制时调用。
不新增第二条 Wayland 连接，不手动调用另一遍 `wl_surface.commit`，不在正常路径增加 `glFinish`。

## 接入点三：callback relay 与有界状态

NWayland 的 `WlProxy._listener` 是可写的 `IWlEventsListener` 字段。
静态 relay 实现这个接口，收到事件时先调用原 listener，再通知 pacer。
原 `FrameListener.Done` 仍负责唤醒 render loop、释放 callback、清除 `_frameCallback`。
通知 pacer 的时机必须在这些操作完成之后，否则下一次提交会覆盖仍未清理的 callback。

| 当前状态 | 允许的操作 | 结果 |
| --- | --- | --- |
| 无 pending callback，无 prepared frame | 准备并立即提交 | 等待 callback |
| 有 pending callback，无 prepared frame | 额外准备一帧，flush 后暂存 | 等待 callback，槽已占用 |
| 有 pending callback，有 prepared frame | 停止继续绘制该 surface | 等待 callback |
| callback 到达，有有效 prepared frame | 原 Done 后提交该帧 | 产生新的 callback，释放准备槽 |
| callback 到达，无 prepared frame | 保留原唤醒行为 | 等待新场景更新 |

ready 判断仍须覆盖连接、WlSurface、初次 configure、context loss、target Dispose；
只将“callback pending”这一条件改为“callback pending 且准备槽已占用”。
不可把所有 NotReady 都强制改成 Ready。无更新时不主动制造帧，不形成常驻 busy loop。

pacer 按 worker 侧 persistent WSurface 身份管理，不按单个 target 管理。
target 重建时可能仍有旧 callback；relay 必须通知当前 pacer，而不是已经销毁的旧 target。
同一个 callback 最多包装一次，保留其原 listener，不层层叠加 relay。
首次接管时若已有 callback，必须纳入同一个 pacer。默认每 surface 最多一个 callback、一个 prepared frame。

## 线程、失效与异常

- surface 读取、NWayland listener 交换、EGL 绘制和 swap 都发生在 Avalonia Wayland worker 线程。
  UI 线程只负责初始化和必要的生命周期请求，跨线程请求走 Avalonia worker client；不能从 UI 直接读写真实 surface。
- prepared frame 保存目标实例、surface/connection generation、sceneInfo、pixel/logical size、scale 和 configure 状态快照。
  提交前比较 worker 侧连接、configure serial、缩放及相关 geometry 状态。变化时丢弃旧准备结果，不用旧尺寸 ack 新 configure。
- 失效导致已经被 compositor 视为“画完”的一帧没有提交时，必须重新要求绘制。
  首版可将该 target 标为 Corrupted 并唤醒 render loop，复用 `ServerCompositionTarget` 的 target 重建和 redraw 路径；
  这是低频 resize/configure 恢复路径，不应用于逐帧节流。是否需要更小的 redraw 桥接由实测决定。
- Native lease 加入原 surface 的 render-target 生命周期集合；断线/销毁使 generation 失效，清空 prepared 引用并释放 owner。
  原 owner 和 lease 的 Dispose 必须幂等，不能假设集合遍历顺序。GL context、EGL surface、窗口资源不得重复销毁。
- 隐藏、关闭、Popup unmap、context loss 后不能提交旧缓冲区；恢复时重新检查 configure 和连接。
  对最小化/遮挡没有 callback 的情况，不用 UI 定时器强行 swap；准备槽有界，恢复后继续。
- 初始化拒绝时完全保留原实现并输出一次原因。运行中异常不能转到同一个已经开始的原 session 继续 Dispose。
  BeforeAttach 前的失败可失效 target；进入协议 staging 后失败须区分是否已经 commit，并验证完整清理和恢复方式。
  不得通过吞异常或盲目清空 `_frameCallback` 继续提交。该异常恢复的可实现性是原型门槛，未验证不能开放默认启用。

源码审查特别确认：当前 `EglSurface.SwapBuffers` 和 `EglInterface.SwapBuffers` 都返回 void，
没有向调用方保留 EGLBoolean；Wayland worker 的运行期重连由连接重置触发，不能假定 callback 中抛异常会自动重连。
原型的首项技术门槛是取得可靠的提交失败证据、保证 relay 异常不会逃逸到协议分发，并验证失败 callback 的所有权清理。
必要时由 Native 对同一 EGL 实例使用静态互操作保留 swap 返回值，但必须核对加载器与 ABI，不能另建 EGL/Wayland 环境。
若无法在适配层安全恢复，则停止默认集成，将此结果用于决定是否必须修复上游后端；不以异常吞噬通过验收。

## 反射清单与 AOT

第一阶段的主要已知成员如下；字段访问缓存 FieldInfo，方法尽可能绑定 delegate，不做逐帧类型扫描。
字段 GetValue/SetValue 仍可能出现在每帧路径，必须测开销，不能宣称热路径完全没有反射。

| 目标 | 用途 |
| --- | --- |
| `WaylandEglWsiSurface._surface` | 将包装绑定到真实 worker surface |
| 原 WSI target 的 `EnsureWindow`、`_eglSurface`、disposed/corruption 状态 | 复用资源所有权和 resize，检查可用性 |
| `WSurface` 的连接、WlSurface、frame callback、生命周期注册和 before-attach 方法 | readiness、提交、释放与恢复 |
| `WXdgShellSurface._initialConfigureAcknowledged`、`_pendingAckSerial` 及 scale/geometry 状态 | 避免提前绘制绕过 configure 或提交过期帧 |
| `NWayland.WlProxy._listener` | 安装静态 relay，保留原事件处理 |

当前检查基线是 Avalonia `12.1.4` / `2d79ba55fae1b1e71c69f93a668080c9029e6a31` 与 NWayland `0.11.0`。
已对本地 Avalonia 源码核对启动顺序、接口和 WSI/WSurface 的相关方法；最新本地 main 的其他 app-id/attribute 差异不用于扩展支持范围。
版本兼容应同时检查 assembly 身份和关键成员签名。版本升级默认拒绝接管，经过源码及运行验证后再扩展。

使用精确的 DynamicDependency 或现有链接配置保留实际访问的字段、方法、构造函数；只保留列出的边界，
不使用整包 preserve。遵循 [AOT 规范](../../engineering/development/aot-programming-guidelines.md)处理分析器证据，
不靠 suppression 宣称安全。必须验证实际 linux-x64 NativeAOT 产物可启动并能重复运行动画和窗口生命周期场景。
静态接口实现不代表其所有反射调用已通过 AOT。

## 第二阶段：按证据决定动画采样合并

第一阶段不包含替换 `MediaContext._clock`。仅减少 UI pulse 已在因果实验中失败，不能先用采样优化替代提交修复。
如果第一阶段呈现节奏改善，但仍有明显多余布局采样，再单独验证按可用准备槽合并 AtomUI 显式动画采样。

可行性探测已确认：CoreCLR 中可反射构造 Avalonia 内部 `ClockBase`，绑定 `Pulse(TimeSpan)`，
订阅后输入两次相隔 16.6667ms 的时间得到 `0, 16.6667ms`；静态 NWayland listener relay 也已编译通过。
这只证明接入形态，不证明实际动画、线程、NativeAOT 或性能正确。

候选边界是 Core 提供 internal playback/clock-provider 契约，Desktop 提供每窗口调度，Native 仅通知准备槽变化。
MotionPlayback 和 NavMenu 根宽度动画通过同一播放入口使用显式 clock。
内部 `Animation.RunAsync(Animatable, IClock, CancellationToken)` 需要受控调用，因为 IClock 不能在外部直接实现。
不覆盖全局继承 Clock，不假定 ClockBase.Subscribe 会自动唤醒 MediaContext。

启动/取消/最终值、动画 delay、没有属性变化的采样、隐藏窗口、多个窗口和 callback 暂停都必须有不依赖持续呈现的进度语义。
需先明确 UI↔worker 身份映射和 one-shot RequestAnimationFrame 的唤醒契约，再形成独立实现设计。
这部分当前是后续候选，不能算第一阶段已承诺或已验证的修复，也不能把原型的固定 8ms 限制发布为全平台策略。

## 原型顺序与验收

1. 做未修改依赖的基线，然后只启用第一阶段适配器，再关闭适配器恢复基线。
   初轮通过临时宿主/诊断入口启用；用户于 2026-10-29 要求改为兼容 Wayland 默认启用，保留显式 false 作为基线对照。
   不得将临时 DLL 原型成绩当作本适配器成绩；默认策略改变不等于性能验收通过。
2. 先通过接入门槛：真实 callback relay、context 恢复、prepared 槽和销毁顺序；
   再覆盖 resize/configure、缩放、隐藏/恢复、重复 Popup、多个窗口、context/连接重建和失败回退。
3. 相同 Debug、主题、窗口尺寸、缩放、数据量下，侧栏两个方向及三层子菜单最深层两个方向各至少 20 次。
   三层必须实际有三个可展开层级。保留祖先快速反向切换、侧栏折叠恢复的正确性场景。
4. 使用 `wp_presentation` 的 presented 时间和 discarded 统计，报告均值/P95、单刷新间隔占比及最大长帧。
   callback 数、layout 数、绘制完成数都不能替代上屏 FPS。动作间空闲期不进入帧间隔计算。
5. 60Hz 的性能目标提议为各场景有效 FPS ≥59、P95 不跨两个刷新周期、≥99% 间隔在单刷新周期内，
   容差按实际 presentation timestamp 精度给出。适配器不能产生因重复覆盖导致的 discarded。
   达不到则继续定位剩余耗时，不表述为“稳定 60FPS”；120Hz 须在真实 120Hz 输出另测。
6. 脚本录制与人工复核背景/文字闪烁、裁剪、鼠标命中、滚动、祖先状态、最终宽高，以及 Modal mask/overlay Popup 回归。
   UI 行为或视觉质量退化即失败，不能用 FPS 达标抵消。当前文档没有声称已完成这些视觉检查。
7. 完成实际 NativeAOT 发布运行；按修改模块执行 focused verification，不自行运行全量回归。
   临时用例/宿主清理后再做最终模块验证，保留指标与必要截图记录，不保留可运行的临时诊断工程。

技术细节（文件拆分、缓存容器、日志格式）由实现者按现有风格决定；启用范围、真实布局语义、有界队列、
原协议处理顺序、AOT 和上述验收边界不得在实现中静默放宽。

## 本轮交付状态

第一阶段 Native/反射适配器已实现并完成独立源码审查；实际呈现、失败恢复与聚焦 NativeAOT 的结果见
[调查记录](../progress/2026-10-28-nav-menu-layout-motion-investigation.md)。性能尚未满足全部验收条件，
mask 渐变、120Hz 和真实视觉/输入验收仍未完成，第二阶段尚未实施。

用户于 2026-10-29 明确要求兼容 Wayland 默认启用，保留显式 false、ABI 校验和运行时失败回退；
启动策略通过临时真实 Wayland 宿主检查，不据此宣称性能已达标。当前改动未创建 commit 或 push。
XWayland 对照不用于替代原生 Wayland 的修复验收。
