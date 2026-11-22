# Window Changelog

## 2026-11-22

- Changed
  - 全屏标题栏位于新的 `WindowChromeLayer`（T6）内的表面宿主中：位于内容、Badge、焦点描边、Watermark、吸顶 Tab、FloatButton、作用域 Drawer / Dialog 之上；位于 Window Drawer、Tour、Dialog、弹层和 Message / Notification 之下。修复全屏标题栏被 Gallery 吸顶 Tab 盖住的问题（此前宿主位于 `PART_VisualLayerManager` 内容根内，低于所有注入层）。
  - `PART_FullScreenTitleBarHost` 仍在三个 Window 模板中声明，保留 `TemplateBinding` 契约；Window 在 `OnApplyTemplate` 中把它迁入 chrome 层，模板解除与窗口关闭时释放（层被清空后随旧模板的 `VisualLayerManager` 丢弃，Avalonia 没有移除层的 API，不新增反射）。`WindowChromeLayerTheme.axaml` 把 `VisualLayerManager.Padding`（`OffScreenMargin`）投影为层的 `Margin`，表面保持迁移前的窗口位置。完整替换 Window ControlTheme 的应用仍需保留该 part。
  - 标题栏按钮的键盘焦点描边与标题栏内的 Badge 画在 chrome 层自己的装饰层中。
  - Window 取得 `PART_VisualLayerManager` 时确保共享 `AdornerLayer` 处于 T2 号码（`int.MaxValue - 1000`）。
  - Window Drawer（`OpenOn is Window`）位于全屏标题栏之上；drawn chrome 抑制租约不变。
- Fixed
  - macOS 带刘海显示器：指针到屏幕顶边、系统菜单栏与原生窗口按钮呈现时，全屏标题栏不出现，红黄绿直接浮在内容上；`Pinned` 下原生按钮盖住标题栏前导内容。原因是原生 chrome 跟踪只按原生标题条窗口的位置判断是否呈现，而刘海屏上该窗口不移动、只渐变透明度。现在平台 chrome manager 直接报告 native chrome presented（原生标题条窗口 `alphaValue` 大于零且进入客户区），并对 `alphaValue` 做键值观察；融合不成立时未呈现的原生标题条不再计入 native chrome extent。
- API
  - Window 的 public / protected API 不变。
- Docs
  - 更新 [Window 全屏标题栏设计](full-screen-title-bar-design.md) 的 6.1、6.4 与第 10 节，层归属见 [视觉层规范](../../../../architecture/systems/rendering/visual-layers.md)。
- Validation
  - Windows 10 实机（单显示器，150% 缩放，Gallery）：原始场景（全屏 → Space 页滚动 → 鼠标滑到顶边）中 AutoHide 标题栏在全宽呈现并盖住吸顶 Tab；Pinned 下标题栏常驻于吸顶 Tab 之上。
  - 常驻守护 `Primitives/LayerLadderHostOrderTests` 覆盖 chrome 层号码、标题栏宿主位于 chrome 层的表面宿主中，以及模板重套用后只有一个承载宿主的 chrome 层。
  - macOS 实机（内建带刘海显示器，Gallery，脚本驱动真实鼠标事件与截屏）：与分支前基线 A/B，基线中吸顶 Tab 盖住全屏标题栏右半，本分支标题栏完整盖住吸顶 Tab；`AutoHide` 与 `Pinned` 下全屏标题栏随系统 chrome 呈现与收起，原生窗口按钮融合在表面内。
- Known limits
  - Windows 未实测：Docked 与自定义全屏内容、标题栏内按钮的焦点描边、多显示器、Dark 主题。macOS 未实测：不带刘海的显示器上 native chrome presented 改动后的复测、Docked 与自定义全屏内容、Dark 主题；原生 chrome 跟踪依赖 AppKit，没有 Headless 测试。Linux 未实测，只有 Headless 证据。

## 2026-11-21

- Docs
  - Add [Window 全屏标题栏设计](full-screen-title-bar-design.md) and link it from the overview, implementation and Token documents.
- API
  - Define `FullScreenTitleBarMode` (`Hidden`, `AutoHide`, `Pinned`, `Docked`; default `AutoHide`), `IsFullScreenTitleBarOpen`, `FullScreenTitleBar`, `FullScreenTitleBarTemplate`, `FullScreenTitleBarMouseEnterDelay`, `FullScreenTitleBarMouseLeaveDelay` and `ToggleFullScreen()` as the full-screen title bar contract of `Window`.
  - `IsTitleBarVisible=false` without custom full-screen content resolves the full-screen title bar to `Hidden`.
- Theme
  - Replace the two hand-written full-screen title templates with a single `PART_FullScreenTitleBarHost` present in all three Window templates. `WindowDrawnDecorationsTheme` no longer supplies drawn-decorations full-screen popover content, and `FullscreenPopoverLayerTheme.axaml` is superseded by `FullScreenTitleBarHostTheme.axaml`.
  - Remove the template parts `PART_FullscreenPopoverLayer`, `PART_PopoverBorder`, `PART_PopoverCloseButton`, `PART_PopoverFullScreenButton` and the decorations-level `PART_FullScreenButton` / `PART_CloseButton`; add `PART_FullScreenTitleBarHost` and `PART_Surface`.
- Token
  - `FullscreenPopoverShadows` becomes the surface shadow of the full-screen title bar host in overlay modes. `FullscreenHeaderFramePadding` has no consumer in this model and leaves the Window Token contract.
- Implementation structure
  - Window owns the full-screen title bar mode, content source, open state and dock inset. The default `WindowTitleBar` instance migrates between the Normal slot and the FullScreen slot instead of being re-templated; reveal behavior is an internal state machine (`FullScreenTitleBarRevealController`) separate from the surface (`FullScreenTitleBarHost`), which replaces `FullscreenPopoverLayer`.
  - Close and exit-full-screen in full-screen state go through the Window caption command only, sharing the close-request flow and the pre-full-screen state restore with the normal state on every platform.
  - The platform chrome managers gain `AttachFullScreenNativeChrome`; Windows and Linux return `null`, macOS returns a tracker in `AtomUI.Native` that follows AppKit's menu bar and title strip through the strip window's move notifications, hides the strip's background, decoration and shadow while fused, and centres the standard buttons by offsetting the strip title bar view's bounds origin (AppKit re-lays the button frames on every frame of the slide).
  - While full screen, `NativeChromeInsets` is written by the full-screen title bar and the normal-state macOS button layout returns early.
  - The platform chrome managers report `SupportsFullScreen`; macOS reports `false` for owned windows, which AppKit never takes into native full screen. `Window.IsFullScreenSupported` follows it and the owner, a full-screen request on such a window is rejected by restoring the previous `WindowState`, the full-screen caption command cannot execute, and the caption group hides the enter button (exit stays available).
- Validation
  - `AtomUI.Desktop.Controls.Tests` Window namespace and the temporary full-screen title bar tests: 242 passed. Dialog, Drawer, Popup, AOT, ImagePreviewer and overlay suites: 528 passed. Window and title bar token effect rendering tests: 50 passed.
  - macOS 26.6.2 scripted acceptance on a real full-screen window: AutoHide, Pinned, Docked, custom content and exit, including the fused traffic lights centred at 49.75pt in the 30–70pt bar. Windows and Linux have no real-machine evidence yet.

## 2026-11-10

- 窗口首次显示的背景从当前作用域的 Window Own Token 快照读取；后续发布继续由 WindowTheme 动态绑定更新。标题、字体与装饰部件按有效身份 / 所属控件读取。


本文档记录 Window 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-29

- Performance
  - Add the reference-counted `AcquireContentCache(overlay)` lease: while a full-window overlay animates, the CSD template content root (`PART_ContentCacheHost`) is composited from a BitmapCache instead of being re-rendered every frame. A hidden `PART_ContentBackdrop` repaints the live frame brush inset from the rounded edge so cached text keeps an opaque base. The lease is refused (direct rendering kept) for non-CSD templates, non-opaque frame brushes, frame-layer content, non-integral device-pixel bounds, or an overlay inside the cached content. See [overlay content cache](../../../../architecture/systems/control-infrastructure/overlay-content-cache.md).
- Fix
  - Restore the active-state left, right and bottom frame borders on Windows 10 under the default `RedirectionSurface`. Avalonia extends the DWM frame 1px into the client area when it uses a redirection bitmap, and Win10 DWM draws the active border beneath that glass, where the opaque client covered it; only the self-drawn top edge was left visible. A Windows 10-only native hook (`WindowsLegacyFrameMarginHook`) resets the margins to 0 after each Avalonia rewrite, so DWM draws the native translucent border again. Windows 11, Linux and macOS are unaffected. The self-drawn top edge is still required.
  - Eliminate opposite-edge frame lag on Windows live resize. Dragging the left or top edge no longer makes the right or bottom edge jump; previously 28%–33% of frames showed gaps of up to 121px under `WinUIComposition`.
  - Remove the whole-window background flashes that `RedirectionSurface` used to show during live resize.
  - Predict the Wayland window scale before the first layout. Avalonia starts Wayland toplevels at scale 1 and adopts the compositor's fractional scale only after mapping, so the window laid out and rendered once at scale 1 and re-laid out about 250 ms later (NavMenu text appeared to change). The constructor now applies the outputs' scale through the impl's `UpdateScaling`; the later compositor notification matches and changes nothing.
- Implementation
  - `WithAtomUIDefaultOptions()` now orders Windows composition modes as `RedirectionSurface` → `WinUIComposition` → `DirectComposition` → `LowLatencyDxgiSwapChain`. Only the redirection surface is composited by DWM in sync with window geometry changes; WinUI/DComp/flip swapchain presents run on a separate timeline from the window frame.
  - `WindowsBackgroundHook` skips its GDI `WM_ERASEBKGND` fill between `WM_ENTERSIZEMOVE` and `WM_EXITSIZEMOVE`. Under `RedirectionSurface` that fill reached DWM before the rendered frame and was the root cause of the previously recorded full-page white frames. The first-show fill is unchanged.
  - A synchronous `UpdateLayout()` before `WM_PAINT` was tried and removed: Avalonia already completes layout synchronously inside `WM_SIZE`.
- Breaking
  - `AcrylicBlur` and `Mica` are unavailable under `RedirectionSurface`; `Transparent` uses the legacy `DwmEnableBlurBehindWindow` path. Applications that need material effects can put `WinUIComposition` first in their own `Win32PlatformOptions` and accept the live resize lag.
- Docs
  - Record the scripted visual verification, data and conclusions in the Windows live resize architecture document; update the cross-platform window guide, startup registration, `AtomUI.Native` Windows documentation and this implementation guide.
- Validation
  - Windows 11 build 26300, Avalonia 12.1.4, 2560x1600 @150%, Gallery Release. Scripted test: topmost click-through magenta backdrop, left/top edge located via `WM_NCHITTEST`, sinusoidal drag for 6 seconds, `ffmpeg ddagrab` at 60fps, per-frame gap and blank-frame counting over 510 frames per run.
  - `RedirectionSurface`: 0 gap frames and 0 blank frames for both left and top edge drags, including at double drag speed. `WinUIComposition`: 28%–33% gap frames for both edges.
  - Overlay popups (title-bar menu, tooltip), native-window popups (InfoFlyout with `ShouldUseOverlayPopup=False`) and the transparent splash render identically under both composition modes; no black-box regressions.
  - `WindowResizeArtifactTests`: 62 passed, including the updated composition-order assertion and a new assertion that the background hook skips the erase fill during size-move.
  - Windows 10 active frame borders, Windows 10 19045 @150%: on white, gray and black backdrops, active and inactive borders now match a native Notepad window pixel for pixel, including after maximize/restore, fullscreen enter/exit, resizing and refocusing. Live resize A/B in one session shows the same gap and stripe frame counts before and after (13 and 4). `AtomUI.Desktop.Controls.Tests` Window tests: 238 passed.

## 2026-10-21

- Fix
  - Restore the visible top frame border on Windows 10 CSD windows. Avalonia 12's extended client area covers the top non-client strip, so DWM paints the left, right and bottom frame but not the top edge; the drawn decorations overlay now supplies a 1-physical-pixel top edge on that platform only.
  - Match the native frame instead of using a fixed solid color: the new `FrameBorderActive` / `FrameBorderInactive` Window tokens reproduce the DWM shadow texture's translucent compositing (`#707070` on white when active, `#AAAAAA` when inactive).
- Implementation
  - Add `Window.IsLegacyWindowsFrameBorderVisible`, gated to Windows builds below 22000, CSD enabled, extended client area, `WindowDecorations.Full` and `WindowState.Normal`. Windows 11, Linux, macOS, maximized and fullscreen states keep the platform-owned frame unchanged.
  - Add `Window.LegacyWindowsFrameBorderThickness = 1 / RenderScaling` and refresh it on `ScalingChanged`, so the top edge stays one physical pixel at 125%/150%/200% scaling.
  - Render the border in the existing `WindowDrawnDecorations` overlay with `IsHitTestVisible=false`; no `WM_NCCALCSIZE`, WndProc hook, native style change or hit-test change is introduced.
- Docs
  - Document the Windows 10 compensation in the implementation and drawn-decorations guides, including why clearing `WS_CAPTION` is not used (Avalonia #21498 restores it to keep window animations).
- Validation
  - Windows 10 22H2 (19045), 150% scaling, Gallery: active top edge `#707070` and inactive top edge `#AAAAAA`, both 1 physical pixel and continuous with the DWM left/right borders; the pre-fix build showed no top edge.
  - Temporary pure-function tests covered the Windows version, CSD, decorations and window-state gates; deleted after validation. Window focused test suite: 237 passed.

## 2026-10-17

- Fix
  - Project `TitleBarFrameBackground` to the default `WindowTitleBar` background so the brush is visible on Windows CSD and non-CSD paths; `null` restores the themed default.
  - Keep the default title bar transparent on Linux CSD so the existing frame-clipped `WindowTitleBarShadowBackground` painter becomes visible instead of being covered.
- Implementation
  - Retain the Linux-only `WindowTitleBarShadowBackground` painter and remove the duplicate frame-layer background paint to avoid double compositing for translucent brushes.
- Docs
  - Document the title-bar background projection and the Underlay visibility limitation on non-transparent surfaces.
- Validation
  - Verified the Linux CSD path on Ubuntu 26.04 (X11 managed decorations, Xvfb): gradient, solid, translucent and default title-bar brushes render with frame clipping, and the translucent brush composites once.

## 2026-10-09

- Theme
  - Keep the opaque surface fallback background synchronized with runtime theme and explicit `TransparencyBackgroundFallback` changes, preserving Avalonia's transparency transitions.
- Lifecycle
  - Scope the fallback Border reference to the current template and release it when the Window closes.

## 2026-09-04

- Integration
  - Keep Window effective Logo resolution live for runtime `Icon` and main-window Logo/Icon changes, while preserving explicit child-window values.
- Theme
  - Make both fullscreen title hosts consume Window effective title visibility and effective Logo content.
- Lifecycle
  - Scope main-window fallback subscriptions to dependent child Windows only while they are open, and release them on local takeover or close so never-shown windows cannot be retained by the main window.
- Verification
  - Cover runtime Icon visibility, main-window fallback updates, zero pre-open subscriptions and release, both fullscreen hosts, and the Gallery `window-title-visibility` example.

## 2026-09-03

- API
  - Add the `IsTitleVisible` AddOwner facade (default `true`) for hiding the in-title-bar title text without affecting the OS-level window title.
- Integration
  - Project `IsTitleVisible` to the default title bar via `NotifyConfigureTitleBar`, and mirror the fullscreen effective title visibility (`IsEffectiveFullscreenTitleVisible`) consumed by `FullscreenPopoverLayer`.
- Docs
  - Update the public surface grouping and the title-bar projection description.

## 2026-08-21

- Implementation
  - Remove the obsolete standalone manual regression application; keep popup-in-Dialog coverage in the automated primitive, control-family and DataGrid test suites.
- Validation
  - Record the Dialog content Popup real-window manual regression as passed on Ubuntu 26.04 GNOME Wayland; Linux X11 and Drawer on Linux remain untested.

## 2026-08-20

- Architecture
  - Reserve the Avalonia drawn decorations overlay for title bar, caption buttons and shadow; keep Dialog and Drawer presentation in the owning Window `TopLevel`.
  - Add a Window-owned reference-counted chrome suppression lease so overlapping modal Dialog/Window Drawer owners cannot restore drawn chrome early.
- Validation
  - Use the canonical popup family matrix for popup-in-Dialog acceptance, with platform evidence recorded separately.
  - Record Windows and macOS as tested for the final layering scheme; Linux X11/Wayland remains untested.

## 2026-08-19

- Architecture
  - Define Window-defined title-bar host projection leases for all logical-tree `WindowTitleBar` instances, including content-area and multiple-title-bar scenarios.
  - Move pointer drag and double-click maximize subscriptions into each title-bar host lease while keeping size hints and CSD-height integration exclusive to the default title bar.
  - Project Avalonia's raw decoration margin into an effective content margin that removes only the hidden drawn title-bar reservation while preserving frame and shadow geometry.
- Theme
  - Preserve `WindowDecorations.Full` for hidden CSD title bars and hide only AtomUI drawn title-bar layers, retaining platform-owned minimize/restore and maximize/restore transitions.
  - Bind the CSD content frame to the effective content margin so hidden title bars no longer leave a title-bar-height blank band.
- Docs
  - Synchronize Window and WindowTitleBar lifecycle, ownership, host-switching and release invariants.

## 2026-08-18

- Architecture
  - Define independent requested visibility for Minimize, Maximize, Close, FullScreen and Pin managed caption buttons without changing Window operation capability.
  - Keep caption capability, WindowState, Topmost, fullscreen restoration and action execution owned by Window and projected one-way to WindowTitleBar.
- Docs
  - Link the Window public contract and implementation boundaries to the shared caption button configuration design.

## 2026-07-28

- API
  - Add `LeftAddOn`, `LeftAddOnTemplate`, `RightAddOn` and `RightAddOnTemplate` as `Window` owners of the default `WindowTitleBar` content contract.
- Docs
  - Define the default title-bar add-on entry points and keep `Window.TitleBar` as template-owned internal state.

## 2026-07-23

- Implementation
  - Rename the shared Linux chrome manager to `AbstractLinuxWindowChromeManager` and the fallback backend manager to `GenericLinuxWindowChromeManager`.
- Docs
  - Synchronize Window and WindowTitleBar implementation docs with the renamed Linux chrome manager structure.

## 2026-07-22

- Implementation
  - Move platform chrome managers under `Window/Chrome` and Window helper implementations under `Window/Utils`.
  - Move the Wayland input-region reflection bridge into `AtomUI.Native/Linux` while keeping X11 shadow input-region policy in `X11WindowChromeManager`.
- Docs
  - Clarify that Window visual helpers are internal observable implementation details, not public API.

## 2026-07-21

- Architecture
  - Define one Windows, macOS and Linux initial theme-surface contract owned by the shared Window show lifecycle.
  - Keep `WindowTheme` as the only long-lived background owner and limit pre-show initialization to a synchronous scoped Snapshot read with disposable Template-priority values.
- Maintenance
  - Reserve platform chrome managers for native geometry and capability projection; require platform-specific surface fallback only after managed first-frame state is proven correct on that backend.

## 2026-07-20

- Architecture
  - Define complete layer, visible frame and content bounds as separate Window geometry contracts.
  - Make `FrameShadowThickness` plus the actual drawn-host presence the platform capability projection consumed by Dialog and Drawer.
- Maintenance
  - Keep native backend differences inside Window chrome managers and remove speculative same-value OS branches from Window tokens.

## 2026-07-09

- Docs
  - Clarify `TitleBarFrameLayer` as the title-bar background/decorative layer, not the user interaction entry.
  - Document that title-bar buttons, menus and search boxes should be hosted by a custom `TitleBar`.
  - Clarify the responsibility split between `TitleBarFrameLayer`, `TitleBar`, and Avalonia `WindowDrawnDecorations` CSD roles.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Window`.
  - Align generated output paths with `controls/window/index-cn.md` and `controls/window/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Window desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Window desktop architecture documentation under `docs/controls/desktop/window/window/overview.md`.
  - Add Window implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Window control-level changelog.
  - Add Window Token documentation covering WindowToken.
