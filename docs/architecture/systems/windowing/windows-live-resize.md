# Windows Live Resize 与窗口装饰架构

本文档定义 AtomUI 在 Windows 上的 live resize、合成模式和窗口装饰所有权。该契约跨越应用启动配置、
`AtomUI.Desktop.Controls.Window`、Window 主题和 `AtomUI.Native`，具体使用方式见
[跨平台 Window 定制指南](../../../guides/windowing/cross-platform-window.md)。

## 平台策略

| 系统 | 渲染模式 | 合成模式 | 窗口装饰 |
|---|---|---|---|
| Windows 10 | `AngleEgl`，失败时 `Software` | `RedirectionSurface` → `WinUIComposition` → `DirectComposition` → `LowLatencyDxgiSwapChain` | Avalonia CSD |
| Windows 11+ | `AngleEgl`，失败时 `Software` | `RedirectionSurface` → `WinUIComposition` → `DirectComposition` → `LowLatencyDxgiSwapChain` | Avalonia CSD |

AtomUI 使用 Avalonia 公开的 Windows 合成回退顺序；Window Control 不读取或切换合成后端；后端只在
`WithAtomUIDefaultOptions()` 的平台启动配置中确定。

## 单一所有权

AtomUI 在 Windows 上保持两个单一所有者：

- Avalonia Win32 拥有非客户区、resize hit-test、CSD、窗口状态和窗口几何。
- Avalonia 启动选项拥有渲染与合成后端选择。

AtomUI Window 负责 Control 状态、主题、标题栏内容和最小布局约束，不重新实现 Win32 chrome。AtomUI 不处理
`WM_NCCALCSIZE`，不返回边缘 resize hit-test，不扩展 DWM frame，也不通过 frame refresh 强制重算非客户区。

这条边界不禁止窄范围 Native 能力。`AtomUI.Native` 可以封装 Windows 公共 API 无法表达的 sizing boundary、
整窗鼠标穿透或明确的原生生命周期信号，但必须由上层显式启用和释放，且不能接管 caption、CSD 或 Snap Layout。

## 配置链路

```text
AppBuilder
  -> WithAtomUIDefaultOptions()
  -> Win32PlatformOptions
  -> CompositionMode = RedirectionSurface, WinUIComposition, DirectComposition, LowLatencyDxgiSwapChain fallback
  -> Avalonia Win32 platform initialization

AtomUI Window
  -> IsCsdEnabled = true
  -> ExtendClientAreaToDecorationsHint = true
  -> WindowDecorations = Full
  -> WindowDrawnDecorations / DWM owned by Avalonia Win32
```

启动配置直接使用 Avalonia 的强类型公开 API，不通过 `Type.GetType`、`Enum.Parse`、反射泛型调用或字符串枚举名
发现 Win32 options。API 变化必须在编译期暴露，NativeAOT 不依赖动态保留规则。

## Window 与 CSD

Windows Window 主题保持：

```xml
<Setter Property="ExtendClientAreaToDecorationsHint" Value="True" />
<Setter Property="WindowDecorations" Value="Full" />
```

`WindowDrawnDecorations`、resize grips、非客户区和窗口状态切换由 Avalonia 管理。AtomUI 不创建第二套边框、阴影、
caption hit-test 或 resize grip。

Windows CSD Window 的最小高度必须为装饰层和内容层保留稳定的非零空间。标题栏手动移动状态必须在以下路径清理：

- 普通 pointer release。
- pointer capture lost。
- 调用原生 `BeginMoveDrag` 之前。
- Window detach、关闭或模板重建。

同一次 pointer 序列不能从 resize 状态错误切换到 move 状态。

## 标题栏按钮

标题栏按钮使用 Avalonia `WindowDecorationProperties.ElementRole` 声明角色：

- `MinimizeButton`
- `MaximizeButton`
- `CloseButton`
- `FullScreenButton`
- `DecorationsElement`

AtomUI 不注册自定义 WndProc 返回 `HTMAXBUTTON`，不通过反射修改 pointer 状态，也不复制 Snap Layout 的命中逻辑。

## Live Resize 诊断边界

Live resize 问题分为两类，修复路径不能混用。

### 窗口外边缘错帧

拖动左边缘或上边缘时，对向边缘出现位置回跳，属于窗口 surface 与几何提交的同步问题。AtomUI 通过统一的
当前 Windows 合成策略处理（`RedirectionSurface` 优先，见下文"2026-10 自动化视觉验证"），不在 Window 模板、
布局或 Win32 消息层增加同步补丁。

### 内容区域抖动

窗口外框稳定但内容帧抖动时，先记录 GPU、驱动、Windows build、render mode 和 Avalonia 日志，并使用受支持的
驱动环境复测。该现象不能转化为 GPU 厂商判断、驱动版本分支、UI 线程渲染或软件渲染默认值。

## 内容帧滞后平台缺陷记录与上游跟踪

2026-08 对"拖动窗口左边缘时内容帧滞后/撕裂"做了完整的实机诊断，结论是 **Windows 平台层缺陷，微软官方已
承认并承诺修复**，AtomUI 与 Avalonia 应用层均无合规的根治路径。本节记录诊断过程、已排除的修复方向和上游
跟踪入口，后续遇到同类报告直接引用本节，不要重复排查。

### 诊断环境与现象

- 环境：Windows 11 专业版 build 26200，Intel Arc Pro Graphics 驱动 32.0.101.8517，2560x1600 @150% DPI。
- 现象：拖动窗口左边缘实时调整大小时，窗口外框已移动但内容帧滞后一拍以上，内容左部被裁切、整体右偏；
  Gallery 的右上角最小化/最大化/关闭按钮区域闪烁最明显。与机器负载无关。
- 像素扫描量化：固定扫描行测量"窗口左边缘 → 标题栏图标"的距离（正常恒定约 94px），拖动期间出现
  3 ~ 197px 的尖峰，即内容帧相对窗口框错位。

### 根因机制（基于 Avalonia 12.1.1 源码）

- `WindowImpl.AppWndProc.cs` 的 `WM_SIZE` 处理只触发 `Resized` 布局事件，渲染由合成器按显示节奏异步进行。
  模态 resize 循环中 DWM 立即移动窗口外框，内容帧至少滞后一拍。
- DXGI swapchain 路径（`DxgiRenderTarget.BeginDrawCore`）尺寸变化要到下一次 `BeginDraw` 才
  `ResizeBuffers` + `Present`，新帧出来之前 DWM 只能把旧尺寸内容拉伸/裁剪贴到新窗口框上。
- 拖左边缘比右边缘观感差的原因：左边缘拖动时窗口原点同步移动，旧内容按旧宽度渲染，贴到新窗口位置时
  整体偏移。
- 自绘标题栏（CSD）属于内容区，因此标题栏按钮随内容一起滞后；原生标题栏由 DWM 直接合成，不走应用内容
  管线，天然与窗口外框同步，所以原生应用表现为"内容闪但标题栏不闪"。

### 2026-08 实机 A/B 结果（已被"2026-10 自动化视觉验证"取代）

在 `AppBuilderExtensions.WithAtomUIDefaultOptions()` 中逐一切换 `Win32PlatformOptions.CompositionMode`
并实机拖动验证：

| CompositionMode | 结果 |
|---|---|
| `RedirectionSurface`（旧默认） | Icon showcase 整页白屏，且存在错帧 |
| `LowLatencyDxgiSwapChain`（显式 A/B） | 整页白屏消失，但右侧边缘仍可能闪烁 |
| `WinUIComposition`（当前优先路径） | 整页白屏消失，但右侧新暴露区域可能短暂透明 |
| `DirectComposition` | 需按目标机实测，不能假定优于其他路径 |

其他对照实验：

- 纯 Avalonia 12.1.1 对照应用（无 AtomUI、系统原生标题栏）：内容闪、标题栏不闪，与文件管理器一致。
- Windows 文件管理器（同机同操作）：内容区同样滞后。
- Avalonia 12.1.0 → 12.1.1 Win32 平台目录零改动，排除升级回归。
- resize 期间通常不发送 `WM_ERASEBKGND`；尝试通过 `WindowsBackgroundHook` 在 `WM_SIZE` 填充 native backdrop 也无法覆盖
  `WinUIComposition` 的透明 composition root，因此该路径不作为最终兜底方案。

结论：`WinUIComposition` 与 `LowLatencyDxgiSwapChain` 均可显著缓解旧 `RedirectionSurface` 路径下的整页空帧，但不能完全消除
`WM_SIZE`、swap chain resize、Present 与 DWM 合成之间的时序差；右侧边缘仍可能闪烁。Icon showcase 的负载
会放大这一问题，但不是白屏的根因。`WinUIComposition` 的透明 composition root 位于 HWND native backdrop 之上，AtomUI 的 Window.Background 或 GDI 背景填充不能覆盖尚未提交的 composition surface。

### 2026-10 自动化视觉验证与当前结论

上表的 2026-08 结论已被本节取代。环境：Windows 11 build 26300，Avalonia 12.1.4，2560x1600 @150% DPI，
Gallery Release 构建、不挂调试器。

方法（脚本化，可复现）：全屏置顶洋红背景（鼠标穿透、不可激活）；把 Gallery 窗口放到固定位置；
用 `WM_NCHITTEST` 探测 `HTLEFT`/`HTTOP` 起点后按正弦轨迹往返拖动左/上边缘 6 秒；`ffmpeg ddagrab`
（Desktop Duplication，即 DWM 合成结果）60fps 录制；逐帧统计固定对向边缘内侧露出的洋红像素宽度（缺口）
和整窗无内容的帧（白屏）。每组 510 帧。

| CompositionMode | 拖左边缘：右边缘缺口帧 / 最大缺口 | 拖上边缘：下边缘缺口帧 / 最大缺口 | 白屏帧 |
|---|---|---|---|
| `WinUIComposition` | 28%～33% / 78～121px | 31% / 56px | 0 |
| `DirectComposition` | 33% / 73～99px | 未测 | 0 |
| `LowLatencyDxgiSwapChain` | 无洋红缺口，但边缘出现黑/白闪烁条纹 | 未测 | 0 |
| `RedirectionSurface`（修复背景 hook 前） | 0 | — | 2～3 |
| `RedirectionSurface`（当前） | **0**（2 倍拖动速度同样为 0） | **0** | **0** |
| Windows 10 19045：旧顺序（`WS_EX_NOREDIRECTIONBITMAP`） | 36%～42% | 37%～40% | 0～2 |
| Windows 10 19045：`RedirectionSurface`（当前） | 0（每组另有 2～4 帧右边缘黑条） | 0.2%～1%（1～4 帧） | 0 |

结论：

- 只有 `RedirectionSurface` 的内容由 DWM 与窗口几何变化同步合成；WinUI/DComp/flip swapchain 的 Present
  与窗口外框是两条独立时间线，任何应用侧同步渲染都无法对齐。因此默认合成顺序改为 `RedirectionSurface` 优先。
- 旧记录中 `RedirectionSurface` 的"整页白屏"根因是 `WindowsBackgroundHook` 在 `WM_ERASEBKGND` 中用 GDI 填满
  客户区：redirection surface 下这层填充先于渲染帧被 DWM 合成。`WindowsBackgroundHook` 在
  `WM_ENTERSIZEMOVE` 与 `WM_EXITSIZEMOVE` 之间跳过擦除填充，交还 Avalonia 默认行为（窗口类无背景画刷），
  首次显示前的底色填充不受影响。
- 在 `WM_PAINT` 前同步 `UpdateLayout()` 无效：Avalonia 在 `WM_SIZE` 内已同步完成布局（实测布局耗时恒为 0），
  该方向不再尝试。
- 透明窗口回归验证：overlay Popup（标题栏菜单、Tooltip）与原生窗口 Popup（InfoFlyout
  `ShouldUseOverlayPopup=False` 的悬停/点击浮层）在两种合成模式下截图一致（PSNR ≥ 55dB 或完全相同），
  圆角、箭头和阴影透明合成正常，无黑框；Splash 透明圆角与阴影正常。
- 代价：`RedirectionSurface` 下 `AcrylicBlur`/`Mica` 不可用（`CompositionEffectsSurface` 为空），
  `Transparent` 走 `DwmEnableBlurBehindWindow` 旧式透明。需要材质效果的应用可显式覆盖 `CompositionMode`。

### 上游与官方立场

- **微软官方承认并承诺平台层修复**：[microsoft-ui-xaml#10820](https://github.com/microsoft/microsoft-ui-xaml/issues/10820)
  的症状与本案例逐字吻合（"从左或上边缘 resize 时尤其明显"）。2026-05-29 微软 Partner Director of Design
  March Rogers 公开表示："We are working on platform improvements to solve the tearing... Will start
  rolling out over the summer"（先在系统自带应用验证，再推到 Windows App SDK；修复在 Windows/DWM 层，
  Avalonia 应用预计同样受益）。相关 issue：[#2506](https://github.com/microsoft/microsoft-ui-xaml/issues/2506)、
  [#5148](https://github.com/microsoft/microsoft-ui-xaml/issues/5148)。
- **Avalonia 官方定性为已知限制**：[官方文档](https://docs.avaloniaui.net/troubleshooting/platform-specific-issues/windows)
  写明 "Window resize flickering is a known limitation of the Win32 windowing model"，未承诺修复。最接近的
  issue 是 [#9103](https://github.com/AvaloniaUI/Avalonia/issues/9103)（仅覆盖 DirectComposition 模式，
  修复方向是 `WM_NCCALCSIZE` 内以新尺寸 present）。
- **全框架通病**：Qt [QTBUG-93084](https://bugreports.qt.io/browse/QTBUG-93084)、Flutter
  [#44136](https://github.com/flutter/flutter/issues/44136)、Electron
  [#40603](https://github.com/electron/electron/issues/40603)、winit
  [#786](https://github.com/rust-windowing/winit/issues/786)。
- **修复原理**（winit#786 中 Raph Levien 的分析）：必须在 `WM_SIZE` 返回前同步绘制并 present 一帧新尺寸
  内容；flip-model swapchain 与窗口尺寸之间没有内建同步。辅助手段：`Present` 后 `DwmFlush()` 对齐 vblank，
  Windows 11 21H2+ 可用 `DCompositionWaitForCompositorClock()`。
- AtomUI 无法采用上述方案：同步渲染需要修改 Avalonia 平台层（渲染在独立 render thread 异步执行），且
  `ShouldRenderOnUIThread` 属于本文档的禁止做法。

### 可用缓解与跟进项

- 微软平台修复 rollout 后（2026 夏起，跟踪 #10820 和 Windows App SDK release notes），按本文档验证矩阵
  在 Windows 11+ 实机重测，确认受益后更新本节状态。
- 跟踪 [Avalonia#9103](https://github.com/AvaloniaUI/Avalonia/issues/9103)；若上游实现
  `WM_NCCALCSIZE`/`WM_SIZE` 内同步 present，评估升级收益。
- 用户侧可交叉验证 Intel Arc 驱动版本（已知部分版本存在视觉故障）。
- 给用户/issue 的答复口径：默认 `RedirectionSurface` 下拖动左/上边缘不再错帧；若应用显式把
  `WinUIComposition`/`DirectComposition` 排在前面（例如为了 Mica/Acrylic），对向边缘错帧是这些合成路径的
  平台限制，按本节上游跟踪处理。

## 禁止做法

- 在 AtomUI 中处理 `WM_NCCALCSIZE`。
- 手动返回 `HTLEFT`、`HTTOP`、`HTRIGHT`、`HTBOTTOM` 等 resize hit-test。
- 调用 `DwmExtendFrameIntoClientArea` 修补 CSD 阴影。
- 使用 `SWP_FRAMECHANGED`、延时、重试或强制刷新掩盖时序问题。
- 默认开启 `ShouldRenderOnUIThread`、`Software` 或 `Wgl` 规避单机环境问题。
- 使用 WndProc hook 重复实现公开的 caption element roles。
- 把 Native sizing helper 扩展成第二套 Window chrome 或 caption/resize hit-test。
- 由单个 Window 实例改变全局渲染或合成后端。

## 兼容性不变量

- Windows 10 和 Windows 11+ 使用 `RedirectionSurface`、`WinUIComposition`、`DirectComposition`、`LowLatencyDxgiSwapChain` 顺序的合成回退。
- `WindowsBackgroundHook` 在 live resize（size-move）期间不执行 `WM_ERASEBKGND` 填充。
- Windows 默认 render modes 为 `AngleEgl` 和 `Software` fallback，`ShouldRenderOnUIThread=false`。
- Win32 非客户区、resize hit-test、CSD 和 Snap Layout 只有一个所有者。
- Window 主题始终通过 Avalonia CSD 表达 Windows 装饰。
- Caption button 只使用公开 element role。
- Native helper 必须显式 opt-in、可释放，并保持窄范围职责。
- 启动配置不依赖反射或运行时类型发现。

## 验证矩阵

### 自动验证

1. Windows 10 和 Windows 11+ options 按 `RedirectionSurface`、`WinUIComposition`、`DirectComposition`、`LowLatencyDxgiSwapChain` 顺序配置。
2. Render modes 保持 `AngleEgl`、`Software`，且 `ShouldRenderOnUIThread=false`。
3. Caption buttons 使用公开 `ElementRole`，不存在自定义 caption WndProc 注册。
4. Window 主题保持 Avalonia CSD，不存在平行 Windows chrome manager。
5. Desktop tests、Browser build 和 NativeAOT publish 不依赖反射发现 Win32 options。
6. Windows CSD 最小高度为装饰层和内容层保留非零布局空间。
7. 标题栏拖动状态不会跨 release、capture lost、detach 或 `BeginMoveDrag` 保留。

### Windows 10 实机验证

2026-10-29 已在 Windows 10 build 19045（2560x1600 @150%，默认 `RedirectionSurface`）上按同一脚本化方法执行，
记录见 [Windows 10 实机验证快速上手](../../../superpowers/progress/2026-10-29-windows10-resize-and-popup-shadow-quickstart.md)
文末“Windows 10 结果”。

- 与旧的合成顺序对比：拖左/上边缘的缺口帧从 35%～42% 降到 0～1%，没有白屏帧。
- 与 Win11 不同，还有少量单帧缺陷：拖左边缘时每组 2～4 帧在右边缘出现 20～60px 黑条；
  拖上边缘时每组 1～4 帧在下边缘出现洋红缺口。
- 下面第 4、5 项通过。暗色主题下的 Win10 边框颜色还没有检查。
- `RedirectionSurface` 下 Avalonia 把 DWM 框架向客户区扩展 1px，Win10 激活态的左、右、下边框因此被客户区覆盖。
  该问题由仅在 Win10 创建的 `WindowsLegacyFrameMarginHook` 修复，见
  [AtomUI.Native Windows](../../../modules/native/windows.md) 第 7 节。

1. 快速来回拖动左边缘，右边缘保持固定。
2. 快速来回拖动上边缘，下边缘保持固定。
3. Resize 期间没有黑框、透明条或原生标题栏按钮闪现。
4. 最大化、还原、全屏、焦点切换和退出全屏后装饰正常。
5. 缩到最小高度后继续 resize 不产生旧帧叠加或错误窗口移动。

### Windows 11+ 实机验证

1. 最大化按钮 Snap Layout 正常。
2. 左边缘和上边缘快速拖动时对向边缘稳定。
3. 透明 Popup 和独立窗口 Popup 没有黑边或闪烁回归。
4. 最大化、还原、全屏和退出全屏后装饰正常。

改变合成模式、render mode、CSD 所有权或 Native helper 边界时，必须重新完成自动验证和两个 Windows 实机矩阵。

## 实现入口

- `src/AtomUI.Core/AppBuilderExtensions.cs`
- `src/AtomUI.Desktop.Controls/Window/Window.cs`
- `src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml`
- `src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml`
- `src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml`
- `src/AtomUI.Native/Windows/`
