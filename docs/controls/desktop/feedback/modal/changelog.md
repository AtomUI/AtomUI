# Modal Changelog

本文档记录 Modal 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-29

- Performance
  - Hold the Window content cache lease while the modal mask fades in or out, so mask frames composite cached window content instead of re-rendering the page; render-thread median drops from about 9.5 ms to 2.4 ms per mask frame on the measured Wayland Gallery.
  - Surface zoom now writes the compositor transform channels instead of `RenderTransform`; closing during opening hands the channels over without a reset.

## 2026-10-28

- Motion / architecture
  - Overlay 统一使用 Motion 原生时钟与真实完成任务，移除旧 Actor、合成提交后的计时完成和重复内容淡出。
  - 普通 Zoom 使用 .2↔1，入场 Slow / OutCirc、离场 Mid / InOutCirc；无显式锚点使用实际面板中心，原点在本次播放冻结。
  - 遮罩作为独立透明度目标，保留 modeless 条件物化；私有表面隔离 wrapper 自定义变换，专用 Style route 不变。
  - 关闭先连续替代入场，生命周期任务先登记后执行，保留 Footer、按钮、焦点和实际宿主释放边界。
- Validation
  - 受控时钟覆盖首帧、原点、反向、实际结束和覆盖释放；原生呈现预算独立验收，不以 Headless 推断 FPS。

## 2026-10-17

- Design
  - 明确 Overlay 正常态的按轴自然/定尺寸模型：宿主容量、约束或内容变化只影响自然轴的当前布局结果，不将自动 clamp 固化为定尺寸意图。
  - 明确单轴显式请求、交互 resize 和 maximize/restore 的接管边界；保留 runtime `NaN` 不强制重置用户尺寸，以及原生 Window 显示后的实际尺寸策略。
- Fix
  - 修复 #499：Overlay Dialog 的自然轴在 owner 容量或约束收紧后继续保留自然测量输入，容量恢复时重新展开，不再将自动 clamp 的 actual size 写成固定尺寸。
  - 显式尺寸请求与交互 resize 只接管对应轴；maximize/restore 保存并恢复 normal 按轴输入，保留另一自然轴及位置偏移意图。
- Docs
  - 同步宿主尺寸设计、公共总览与内部状态所有权约束，扩展自然高度往返、跨轴更新、restore、焦点和滚动布局的验收矩阵。
  - 本次完成 macOS 原生窗口中的真实 owner resize、滚轮、东侧 resize 和 maximize/restore 视觉验收；其他平台未在本次执行。

## 2026-10-16

- Fix
  - Overlay Dialog 在 owner 缩小时只临时钳制 Surface 位置，不把自动边界修正写成持久 `OffsetX/Y`；容量恢复后默认居中和用户拖动偏移均按原有意图重新计算。
- Validation
  - 用同一 Dialog 的 1240×820 → 940×640 往返缩放复现并验证水平、纵向居中，同时验证草稿和焦点保持；另验证已有用户偏移在缩放后仍保留。

## 2026-10-07

- Behavior
  - Overlay modal mask 改为直接显示，关闭期间保持固定透明度，随 presenter 移除；compositor、fallback 和 mask 主题均不再执行遮罩淡入淡出。保留 Surface 的锚点位移、缩放及透明度动画。
- Fix
  - Overlay opening/closing 的完成计时改为从动画批次到达渲染线程后开始，避免 UI 提交延迟截断 Surface 位移/缩放。
  - Scale 动画同步初始基值，确保完成或取消时最终比例实际提交，避免重置缩放中心后出现残留缩放与 X/Y 跳变。
- Validation
  - 在 Linux Wayland 的当前 Gallery 场景中，启用 BasicDialog 的 modal mask，使用偏离弹窗中心的真实按钮覆盖 X/Y 位移，并以延迟提交对照验证动画终态；不据此声明跨平台帧率提升。
  - 此前带 mask 动画版本的诊断中，ModalShowCase 的 21 个 Overlay 点击入口各完成三次打开/关闭，另检查两个常开语义预览；逐例绘制节奏仍显示部分带 mask 场景存在长帧，不能将终态修复视为所有场景的流畅度达标。

## 2026-09-26

- Performance
  - Overlay Dialog 的 opening/closing 从 UI-thread `AbstractMotion` transition 主路径切换为 Avalonia Composition visual 的服务端 `Opacity`/`Scale` animation；UI Dispatcher 不再逐帧写 `MotionTransform` 或 `Opacity`。
  - 新挂载 opening visual 在首次 compositor commit 后启动 animation；同一次 choreography 的 Surface、mask 与内容层任一必需 Composition visual 不可用时整体回退到既有 transition/animation 路径。
- Lifecycle
  - 使用一次性 Dispatcher timer 定义 server animation 的 duration 加安全余量完成边界，并在正常完成或 opening 取消时释放；opening 取消先停止 server animation、恢复 actor 基值，再进入 close motion。
- Tests
  - 新增 `OverlayDialogPresenterCompositorMotionTests`，覆盖 anchored zoom、无锚点 fade 的 opening/closing UI-thread 帧隔离，以及 opening 中途关闭的状态归一化。
- Docs
  - 同步 Overlay motion 的 Composition 主路径、fallback 边界、资源释放和验证不变量；详细取证与未覆盖平台记录在带日期的性能调查文档中。

## 2026-09-13

- Fix
  - Window 宿主 owner 实例级 Semantic Style 此前在真机上零生效（container 白底、正文/按钮样式不命中）。
    根因一：Avalonia `TopLevel` 把样式宿主父级固定为 Application（`IStyleHost.StylingParent => _globalStyles`），
    且窗口模板应用时 `ContentPresenter` 无条件改写 surface 继承父，窗口模板之后才挂载的内容文字与
    footer 按钮永远走不到 owner 样式链——此前控件级回归只验证了模板期节点的 Tag 命中，掩盖了时序差异。
    `DialogWindow` 现按 `PopupRoot` 既有范式覆写 `IStyleHost.StylingParent => Parent`（owner 未生根时退回
    `Application`，保证脱离页面树直接构造 presenter 的场景 ControlTheme 仍可达）。
  - 根因二：`DialogSurfaceTheme` 把 `container`（`Border#Frame`）的 `Background`/`CornerRadius` 写成模板
    局部值，局部值优先级压过任何 Style setter，`DialogContainerStyle` 永远无法覆盖；现迁入 ControlTheme
    嵌套样式，`CornerRadius` 经 `TemplatedParent` 绑定保持对 Surface 运行时变更（Overlay 最大化归零）的跟随。
  - Gallery「自定义语义结构的样式」的样式化窗口 Dialog 漏配 `StandardButtons`（默认 `NoButton`，footer
    无按钮），按钮级语义样式无目标可命中；现补 `Cancel,Ok`。
- Tests
  - 新增 `Window_Host_Owner_Instance_Styles_Restyle_Container_Content_And_Footer_Buttons`：以真实属性值
    （container 背景色、正文前景/字重、footer 按钮背景）锁定 Window 宿主三条修复，红→绿。
  - Gallery 页测试锁定样式化窗口 Dialog 必须显式声明 `StandardButtons="Cancel,Ok"`。
  - Gallery 文案修正：语义样式卡内引导句误用上游静态 API 文案（"Content of the modal/模态框内容"），
    现改为专用的 `SemanticStylesHint`；三个触发按钮改为语义化专用文案（打开样式化 Dialog /
    打开样式化 MessageBox / 打开样式化窗口 Dialog），不再复用泛化的 `P2ContentOpenModal`；
    清理孤儿键 `P2ContentOpenMessageBox`。语义预览对话框标题从机制名（"Semantic Part preview/
    语义部件预览"）换绑为上游语义 demo 的 `P2TitleBasicModal`（Basic Modal）。

## 2026-09-11

- Semantic Parts
  - Publish the Modal family Semantic Part contract: `Dialog` and `MessageBox` each expose implicit `root` plus `mask`, `wrapper`, `container`, `header`, `title`, `body`, `footer` and `close`, mapped to the upstream Ant Design `Modal` semantic DOM (`components/modal/demo/_semantic.tsx`, 9 slots).
  - Declare every non-root Part as `RuntimeCreated`/`CrossVisualRoot`/`CrossNestedOwners` (`Dialog` has no `ControlTemplate`; nodes live in the `DialogSurface`/`OverlayDialogPresenter`/`OverlayDialogHeader`/`OverlayDialogMask` templates) and mark them with static `Classes.semantic-*="True"`.
  - Route every Part through Dialog-owned `.semantic-scope-*` anchors instead of a broad `>>`: the body permanently hosts a `Skeleton` and user content commonly contains `Card`/`Tooltip`/`Spin`, which publish same-named `semantic-header`/`semantic-title`/`semantic-body`/`semantic-footer`/`semantic-container` classes that a broad descendant would also hit.
  - Cardinality correction (evidence-based): `mask`/`wrapper` were already `Optional`; diagnostics on the Window host proved `WindowDialogPresenter` unconditionally hides the surface header (`IsHeaderVisible=false`, native caption owns the title bar), so the hidden header template is never applied and `header`/`title`/`close` marker nodes do not exist there — all three are now `Optional` per the system design rule for host variants. `footer` stays `Single` (materialized in both hosts).
- API
  - Add `Dialog.OverlayScope` (default `null`, Overlay host only): when set, the overlay host is injected into that element's scope so the mask, surface sizing and centering are bounded by the scope instead of the owning `TopLevel`, matching upstream Ant Design's inline/`setContainer` modal semantics. The default keeps the existing TopLevel host resolution unchanged.
  - Add `Dialog.IsPinnedOpen` (default `false`, Overlay host only). While true, user-initiated close requests (header close button, mask outside-press, Escape, footer buttons) are ignored; external `IsOpen=false` still closes. This replaces the earlier plan of an example-side `BeforeCloseAsync` veto, which AXAML cannot express.
- Behavior
  - `OverlayDialogPresenter` sets the `Dialog` owner as its logical parent so owner-scoped generated Semantic Styles can reach the presenter subtree; the parent is only set while the owner is attached to a logical tree, and the visual parent remains `DialogOverlayLayer`.
  - `Dialog` implements `ISemanticPartCrossRootProvider` and reports the live Overlay presenter as its cross root. The Window host now reports too: `GetCrossRoots()` returns the native `DialogWindow` once visible, and `WindowDialogPresenter` raises `CrossRootsChanged` on show, `Opened` and teardown, so the Gallery semantic preview can resolve and highlight parts inside that window (adorners land in the host window's own adorner layer).
  - Evidence-based correction: owner-instance Semantic Styles DO reach the Window host through the logical-parent chain (`DialogWindow` is parented to `Dialog`), proven by the `Window_Host_Owner_Scoped_Semantic_Styles_Cascade_Via_Logical_Parent` regression; the earlier "Overlay-only style cascade" claim was overly conservative. Static resources still flow through `DialogResourceBridge`.
- Docs
  - Add `semantic-part.md`; replace the previous non-contract `root`/`host`/`surface`/`content`/`motion` overview summary with the real Part table; document the `.semantic-scope-*` route anchors, host boundaries and `IsPinnedOpen` semantics.
- Gallery
  - Add a MessageBox Semantic Part styling demo inside the "Custom Semantic Part styling" example (generated `MessageBox<Part>Style` classes with an explicit owner selector, triggered by an `Open MessageBox` button next to the Dialog one), and switch every Modal example trigger from `ToggleSwitch` to `Button` (MessageBox host selection is now two buttons).
  - Stack a second `SemanticPartPreview` for MessageBox under the Dialog preview in the Semantic Parts tab; both previews use the stage-scoped inline modal pattern (`OverlayScope`), and the stage owners are explicitly sized to the stage so the `root` part can highlight (the Dialog theme defaults to a zero-size owner and the resolver skips zero-size targets).
  - The styled Window-host demo now demonstrates region-internal element styling through one-level nested styles on the owner style: content text (magenta bold `atom:TextBlock`) and footer buttons (green capsule `atom|Button` — `DialogButton`'s StyleKey is AtomUI `Button`, so `atom|DialogButton` never matches); nested styles must sit directly under the outer owner style (nesting inside a generated part style does not activate).
  - Add `ModalSemanticPartHighlightTests`: hovering each of the 9 part cards in both stage previews yields exactly one highlight adorner; a third preview demonstrates the native Window host (modeless so the Gallery stays interactive, opened on demand via buttons — not auto-opened), where `root` highlights the stage owner, `container`/`body`/`footer` highlight inside the native window via the reported cross root, and the five non-materialized parts produce no highlight; a mask press does not close the pinned previews; switching back to Examples clears all highlights. A second button opens a styled Window-host Dialog (container/body/footer customizations); it lives outside `PreviewContent` because the preview resolves all same-type owner instances inside its content.
- Validation
  - Add `DialogSemanticPartTests`: descriptor contract, static marker lists, runtime markers, logical-parent invariant, cross-root reporting, exact single-node style hits, pin gating, modeless/hidden-node `Optional` semantics and MessageBox parity. Desktop Controls 3732/3732; LLMS generate/verify 79 controls / 161 files.
  - Gallery Semantic Parts tab and NativeAOT publish are still pending.

## 2026-08-21

- API
  - Change the `Dialog.HorizontalStartupLocation` and `Dialog.VerticalStartupLocation` metadata defaults from `Custom` to `Center`, matching `DialogOptions` and the static Dialog APIs.
- Behavior
  - Directly instantiated Dialogs now open centered by default; explicit anchors, `Custom` placement and offsets keep their existing semantics.
  - Keep Overlay structural-minimum updates initialization-aware so template layout cannot clamp the unresolved `(0, 0)` Surface position and convert it into startup offsets before centered placement.
- Docs
  - Document the shared centered startup default and the explicit `Custom` offset behavior.
  - Extend the Dialog popup family matrix with shared surface ownership: Direct Popup and specialized Popup-bearing controls inherit the primitive's null surface default, while explicit non-null brushes opt into a host-owned surface.
  - Preserve the Windows/macOS tested and Linux X11/Wayland untested evidence boundary for Popup surface behavior.
- Implementation
  - Remove the obsolete standalone manual regression application and its source-contract assertions; retain the Popup Theme default assertion and automated Dialog Popup coverage.
- Validation
  - Record the Dialog content Popup real-window manual regression as passed on Ubuntu 26.04 GNOME Wayland; Linux X11 and Drawer on Linux remain untested.

## 2026-08-20

- Design
  - Keep Overlay Dialog presentation in the owning Window `TopLevel`: `DialogOverlayLayer` uses Avalonia `OverlayLayer`, while content popups use the higher `PopupOverlayLayer` with the normal `LightDismissOverlayLayer` between them.
  - Reserve `WindowDrawnDecorations` overlay for chrome visuals and manage modal chrome coverage through a Window-owned reference-counted suppression lease shared safely by overlapping Dialog and Drawer presentations.
- API
  - Add `Dialog.IsMaskClosable` (default `true`) controlling whether pressing the Overlay modal mask requests a close; `MessageBox` inherits it and `MessageBoxOptions` exposes the same passthrough.
  - Define the two close-entry switches as orthogonal: `IsClosable` gates the header close button, `IsMaskClosable` gates the mask outside-press entry.
- Behavior
  - With `IsMaskClosable=false`, a mask press is swallowed by the topmost Overlay presenter without producing any close request, and never enters the `Closing`/`BeforeCloseAsync` pipeline.
  - Dialog content popups retain their Window `TopLevel`, open without crashes, remain clickable and preserve normal outside-click light-dismiss behavior under managed/drawn window chrome.
- Docs
  - Add the Modal content popup layering design document and link it from the public design, implementation and compatibility sections.
  - Document the mask close-entry contract in the Dialog contract groups, behavior model, compatibility invariants and maintenance invariants.
  - Expand popup verification from the ComboBox trigger to the complete Popup/Flyout/ToolTip/ContextMenu inventory and control-family matrix.
  - Define the canonical Popup family matrix, with Windows and macOS tested and Linux X11/Wayland explicitly untested.

## 2026-07-22

- Design
  - Define one Surface-body sizing contract for Overlay and native Window hosts, including requested size, structural minimum, host capacity, effective constraints and actual size.
  - Make structural minimum preserve the title, Footer actions and a non-zero content viewport whenever host capacity permits.
  - Define normal resize, capacity degradation, maximize/restore and Window chrome translation as one stable host-sizing model.
- API
  - Keep all existing `Host*` registrations and defaults while defining `HostMin*` as a request that can raise, but cannot lower, the structural minimum.
  - Keep `HostWidth/Height=NaN` as an initial natural-size request without converting natural size into a permanent minimum or writing user resize back to the public properties.
- Token
  - Define `DialogToken.MinWidth/MinHeight` as the content viewport baseline used by structural minimum resolution.
- Docs
  - Add the Modal host sizing and resize design and link it from the public design, implementation and Token documents.
- Gallery
  - Enable resize in the basic Overlay and Window examples, keep the Overlay header on its default close-only capability, demonstrate finite HostMin/Max ranges, and expose the Dialog content viewport minimum Token baselines.
- Behavior
  - Capture Overlay resize pointers and terminate resize state on both release and capture loss so a later drag cannot reuse a stale origin.
  - Preserve a natural axis as actual geometry after constraints clamp it, and resolve final natural sizing and startup placement before native Window show so the first visible frame cannot show a preliminary size or platform-default position.
  - Re-resolve structural minimum after template-subtree measure changes and translate Window chrome according to the active CSD or managed-title-bar template metrics.
  - Disable native Window maximize whenever either HostMax axis is finite, and automatically restore the capability when both axes return to PositiveInfinity.

## 2026-07-20

- Architecture
  - Unify Overlay Dialog host selection across Windows, Linux and macOS by using the drawn decorations Dialog host whenever the current platform exposes it, with TopLevel popup and scoped overlay fallback.
  - Separate complete mask bounds, Window visible-frame Dialog bounds and Dialog BoxShadow extents as independent geometry responsibilities.
  - Replace OS-specific Dialog body bounds with one Window visible-frame calculation shared with the Window frame clip.
  - Derive Dialog body owner bounds from the Window visible frame and the current DPI-rounded drawn-decoration frame thickness, while keeping host dimensions as body dimensions.
- Behavior
  - Define the modal mask as covering and blocking the complete Avalonia drawable window, including managed/drawn title bars, while applying Dialog body owner bounds derived from the Window visible frame to placement, drag, resize and maximize; native system chrome outside the client visual tree remains platform-managed.
  - Let Overlay Dialog placement, drag, resize and maximize use managed/drawn title-bar space on every platform while continuing to exclude transparent frame-shadow buffers.
  - Keep the Dialog body inside the effective Window frame on every platform while allowing its BoxShadow to be clipped naturally at the window edge.
- Performance
  - Move Overlay Dialog drag positioning from layout-affecting Margin writes to one reusable render-only Matrix translation while retaining `OffsetX` / `OffsetY` as persistence state.
- Docs
  - Document capability-driven host selection, visual-layer clipping ownership, lifecycle cleanup, AOT reflection boundary and cross-platform regression requirements.

## 2026-07-18

- Architecture
  - Replace the multi-owner host lifecycle with one `DialogSession`, two async presenters and one shared `DialogSurface` implementation.
  - Make `MessageBox` a direct Dialog specialization without a hidden Dialog or parallel host lifecycle.
- Behavior
  - Make `OpenAsync` represent the complete opening, active and teardown lifetime; reject concurrent instance opens.
  - Unify normal and forced close sources, deterministic post-commit teardown, opening/closing motion, nested focus restoration and owner close behavior.
  - Preserve natural Overlay/Window sizing, runtime size updates, placement, standard/custom buttons, loading and semantic MessageBox styles.
  - Fix real modal mask pointer routing while allowing modeless background input.
  - Construct generic static API views on the UI Dispatcher and remove Gallery dispatcher-yield workarounds from direct click handlers.
- Lifecycle
  - Pair presenter bindings, events, logical/resource parents, content references and button subscriptions with deterministic release paths.
  - Disconnect Surface composition children before host removal so retained Content or CustomButton controls cannot retain a closed Presenter/Surface.
  - Add Overlay and Window WeakReference coverage for Session, Presenter, Surface, user Content and retained custom controls.
- API
  - Remove synchronous/callback display APIs and obsolete host/action-result types.

## 2026-07-06

- API
  - Make `Dialog.IsOpen` default to `BindingMode.TwoWay` so controlled dialog open state updates the bound ViewModel without explicit binding mode.
  - Add `DialogOptions.BeforeCloseAsync`, `DialogClosingContext` and `DialogCloseReason` for static Dialog close-before validation.
- Behavior
  - Route Dialog button, keyboard, host, owner, placement-target and programmatic close requests through one async close pipeline.
- Docs
  - Document `Dialog.IsOpen` as controlled open state and clarify that it is not a Form validation value.
  - Update the close-before validation design from planned behavior to implemented API and pipeline invariants.

## 2026-07-03

- Docs
  - Add the approved design for `DialogOptions.BeforeCloseAsync` as a future close-before-validation API for static Modal dialogs.
  - Document the intended close request pipeline, event order, async validation behavior and compatibility boundaries.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Modal`.
  - Align generated output paths with `controls/modal/index-cn.md` and `controls/modal/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Modal desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Modal desktop architecture documentation under `docs/controls/desktop/feedback/modal/overview.md`.
  - Add Modal implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Modal control-level changelog.
  - Add Modal Token documentation covering DialogToken.
