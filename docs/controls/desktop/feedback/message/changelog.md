# Message Changelog

本文档记录 Message 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-27

- Motion
  - 阴影 Frame 直接作为动效目标，移除内部 Actor 与额外 ContentPresenter。
  - 进入/退出改用共享 Motion 轨迹和框架时钟，关闭从当前透明度与矩阵连续替代入场。
  - 协调层保留业务版本与单次关闭提交，移除旧过渡订阅、两套取消源、额外帧屏障与等待旧执行清理的路径。
  - 保持 64 DIP 方向、完整时长曲线、堆叠布局与 Semantic Part route，补齐同步通知重入的覆盖释放。

## 2026-09-22

- Design / layering
  - Define and implement `WindowFeedbackLayer` as the single owner of cross-manager activation order: the manager that most recently
    commits a successful `Show` is the topmost atomic feedback group, while each manager retains its own card ordering,
    Stack, MaxItems, styling and DestroyAll boundaries.
  - Require direct child collection `Move` with an O(1) already-topmost fast path; prohibit remove/add reparenting,
    incrementing ZIndex counters, global registries, timers and Dispatcher-delayed ordering.
- Lifecycle / performance
  - Keep manager activation free of subscriptions, disposables, timers, caches and retained manager references,
    and to leave visual/logical attachment, scheduler pause state and card lifetime unchanged.
  - Add regression coverage for alternating Message/Notification manager activation, reentrant Show ordering,
    single Move publication, zero attach/detach and WeakReference collection after disposal.

## 2026-09-14

- Theme / Semantic Part
  - Preserve WindowMessageManager `Padding` customization with the shared feedback stack while keeping the default placement and queue hover bounds.
  - Align `listContent` with the public `ItemsControl` contract and update examples to use supported size and alignment setters; item spacing remains owned by the internal feedback stack.
- Lifecycle
  - Close cards after a persistent host detach while preserving cards shown before the first attachment and queues moved back into the visual tree during the same dispatch cycle.
- Token
  - Retain `MessageTopMargin` as the manager padding default and keep cards free of list-edge margins.

## 2026-09-13

- Lifecycle / correctness
  - Serialize entry and exit actor writes through the completion of the previous motion's asynchronous cleanup, with cancellation-aware handoff and six-placement regression coverage.
  - Release expired and already-closing cards from scheduler scratch lists at the end of each wakeup, and reschedule from the time after synchronous close callbacks.
  - Select stable close batches for MaxItems and DestroyAll so immediate removal and reentrant callbacks preserve ordering, new messages and disposal boundaries.
- Cleanup
  - Remove redundant motion guards, unused motion configuration, duplicated projection checks and empty Gallery overrides while preserving animation and template contracts.
- Motion
  - Preserve the latest card's backplate dimensions while expanded and fade/slide the two plates in during card collapse, avoiding a delayed width expansion after every hover exit.
  - Keep transparent backplates outside queue measurement and mirror their edge anchor for bottom placements; cover rapid hover reversal, content width changes, motion/Stack toggles and empty queues.
- Behavior
  - Clarify that Message stack remains opt-in and never changes or pauses a message lifetime unless the pointer is actually hovering the stack.
  - Keep finite messages on their original deadlines and require `Expiration=TimeSpan.Zero` when a caller wants a persistent stacked message.
- Gallery
  - Add an Ant Design-aligned Stack showcase with runtime Enabled and Threshold controls, alternating persistent messages, and a scoped Destroy all action.
  - Isolate the Stack showcase from the default Message examples through separate lazily created managers so stack configuration and destruction cannot leak across examples.
  - Center the Stack configuration labels and controls on one row and mark the showcase with the standard `v6.1.9` RibbonBadge.
- Verification / lifecycle
  - Cover default-stack isolation, live configuration updates, persistent content ordering, scoped destruction, detach cleanup, and finite-message scheduler registration while stack is enabled.
- Internal architecture
  - Name the per-card motion execution owner `FeedbackCardMotionCoordinator` to reflect its coordination of actor state, cancellation and close completion.

## 2026-09-12

- Design
  - Adopt the shared Feedback stack infrastructure with an opt-in Message stack, threshold `3`, whole-stack hover expansion and static AXAML depth plates.
  - Define `TopCenter`, 3-second expiration and unlimited `MaxItems` as the Message defaults.
  - Add the shared `IsStackEnabled`, `StackThreshold`, `IsPauseOnHover` and `DestroyAll()` manager contract.
- Performance / lifecycle
  - Require a stable ItemsSource-backed collection and one lazy nearest-deadline scheduler per manager, with no per-item timers or hot-layout allocations.
  - Define deterministic cleanup for retemplate, detach, rehost, destroy, callback failure and dispose, backed by WeakReference regression coverage.
- Motion
  - Align Message entry and exit with the shared Feedback 64 DIP translate/fade motion, full-duration Ant ease-in-out curve and scale-preserving geometry.
  - Animate existing cards to their new queue positions during add, remove, collapse and expand instead of jumping their layout bounds.

## 2026-09-11

- Semantic Part
  - Make the implicit `root` surface actually customizable: `MessageCardTheme.axaml` now projects `Background` / `BorderBrush` / `BorderThickness` / `CornerRadius` / `BoxShadow` / `Padding` from the owner onto `Border#PART_Frame` via `TemplateBinding`, and those token values moved to owner-level ControlTheme setters so the default look is unchanged. Previously every surface setter was silently ignored (measured: `Background=Red` left the frame `White`, `CornerRadius=16` left it `8,8,8,8`), so `semantic-part.md` §2.1's claim that the card could be customized this way was false.
  - Add `MessageCard.BoxShadowProperty` (`Border.BoxShadowProperty.AddOwner<MessageCard>()`, same pattern as `Card` / `InputControlFrame`), which the upstream `root` style needs for its hard shadow.
  - Coverage: `Root_Surface_Properties_Project_Onto_The_Frame_And_Keep_Token_Defaults` (control layer) and `Message_Semantic_Style_Buttons_Produce_Styled_Cards` (end-to-end), the latter failing when the theme projection is reverted.
  - Gallery: rewrite the `Custom Semantic Part styling` example to strictly mirror `components/message/demo/style-class.tsx` — an object-style success message (`#F6FFED` / `#95DE64` / radius 16 / `4 4 0 #D9F7BE`, green icon and title) and a function-style error message whose `:error` branch turns the whole card red (`#FFF2F0` / `#FFCCC7` / `#CF1322` / `4 4 0 #FFCCC7`). Both are emitted by the real buttons through `Show(IMessage, string[]? classes)`, the AtomUI equivalent of the upstream per-message `styles` prop, instead of the previous static two-card comparison whose values did not come from the demo.
  - Fix the example `BadgeText` to `{x:Static gallery:GalleryVersionInfo.DisplayVersion}` (`v6.1.8`, from `build/Versions.props`). It was hardcoded `v6.0`, violating `docs/superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md` §8.4.
  - Emit the styled messages where upstream does — at the viewport top via the window feedback layer, not in a page-local container. `WindowMessageManager` is now constructed with the `TopLevel` host for this example (the same pattern as the Notification page), so cards land in `WindowFeedbackLayer`; measured placement 1280×900 is `x=347`, `y=0` (exact horizontal centering), matching the upstream top-of-viewport collapse. Covered by `Message_Semantic_Style_Buttons_Pop_At_The_Window_Feedback_Layer`, which asserts the cards are absent from the page subtree and that all object/error style values reach the frame.
  - Merge the two Semantic Part previews into one, matching the upstream `message#semantic-dom` layout: a single stage hosts both owners and one panel lists all six parts in owner order, with a per-row owner label disambiguating the duplicated `root`. `SemanticPartPreview` gained `SemanticOwners` (multi-owner declaration), `SemanticPartDescription.OwnerType` (disambiguation, required for multi-owner) and per-owner highlight resolution; the single-owner path is unchanged. Covered by `Multi_Owner_Preview_Merges_Part_Lists_In_Declaration_Order` and `Multi_Owner_Description_Without_OwnerType_Is_Rejected`.
  - Fix the `root` / `listContent` geometry to match the upstream layering. `MessageCardToken.MessageTopMargin` (`(m, m, m, 0)` on `Border#PART_Frame`) made the `root` highlight box asymmetric — extra inset on left/top/right and none at the bottom — and the `list` and `listContent` boxes nearly coincided. Spacing now lives where upstream puts it: the list container carries `Padding` (upstream `.ant-message-list` `padding: marginLG`, consumed by a template `Border`), `listContent` carries `Spacing` (upstream `gap: margin`), and the card has no margin at all, so `root` equals the visible card exactly and `listContent` insets symmetrically inside `list`. Measured: card insets 0/0/0/0, list-content insets 24/24/24/24, `Spacing` 16. Not related to `BoxShadow` — shadows do not participate in layout. Covered by `Root_Frame_Has_No_Own_Margin_And_List_Insets_ListContent_Symmetrically`; contract documented in `semantic-part.md` §5.1.
  - Document the merged-preview contract in `semantic-part.md` §5.2.
  - Document the resulting style-scope constraint in `semantic-part.md` §5.3: the feedback layer sits outside the page visual tree, so page-scoped styles cannot reach it (measured; manager / `Window` / `Application` scopes do). The generated style classes therefore stay declaratively written in the page's `UserControl.Resources` and are attached to the manager's `Styles` by code — attachment only, no part-property rewriting.
- Gallery
  - Add localization units `P2ContentObjectStyle`, `P2ContentFunctionStyle`, `P2MessageObjectStyles`, `P2MessageFunctionStyles` (en-US / zh-CN / zh-TW / pt-BR).
  - Remove the shared `SemanticPartAdorner` white halo ring, observed first on this page. The primary marker no longer paints the upstream dumi `Marker` `box-shadow: 0 0 0 1px #fff`; on the light Gallery stage it read as a stray white line. The primary layout outset drops from `3px` to `2px` (half the `2px` pen width) since no outer ring needs space. Covered by the updated `Adorner_Draws_An_Outline_Without_Covering_The_Target` (asserts a single gold drawing and no near-white brush); contract updated in `docs/gallery/authoring/semantic-part-preview.md` §9.
  - Fix a crash when switching the Message page to the Semantic Parts tab: the semantic content template root was a `ScrollContentPresenter`, which does not realize its `Content` into visual children until it is attached, so `GalleryShowCaseHost` judged the template empty and threw. The root is now a plain `StackPanel` (scrolling is already provided by the host's own `ScrollViewer`), matching the other 52 Semantic Part pages. Covered by `Message_Semantic_Previews_Materialize_When_The_Semantic_Tab_Is_Selected`, which fails with the original exception on the previous revision. The template-root contract is tightened in `docs/gallery/authoring/semantic-part-preview.md` §5.
- API
  - Add a public parameterless `WindowMessageManager()` constructor alongside `WindowMessageManager(TopLevel? host)`; the host overload now delegates to it. A null or omitted host means the manager is not installed into a TopLevel layer and renders inline where the caller places it. This makes the control declaratively usable from XAML, mirrors `WindowNotificationManager`, and backs the Gallery semantic preview. Covered by `Parameterless_Manager_Renders_Inline_Without_Taking_Over_The_Host_Layer`.

## 2026-09-10

- Semantic Part
  - Publish two owner descriptors aligned with the upstream Message semantic keys: `MessageCard` exposes `wrapper` / `icon` / `title` (plus implicit `root`) for the notice card, and `WindowMessageManager` exposes `listContent` (plus implicit `root`, which maps the upstream list). `Since` is `6.2.0` for both.
  - Add generated semantic style types `MessageCardWrapperStyle`, `MessageCardIconStyle`, `MessageCardTitleStyle` and `WindowMessageManagerListContentStyle`.
  - Add static `Classes.semantic-*` markers in `MessageCardTheme.axaml` and `WindowMessageManagerTheme.axaml`; the built-in themes do not consume `.semantic-*` for default visuals.
  - Add `docs/controls/desktop/feedback/message/semantic-part.md` as the authoritative Part contract, and document the descriptor/marker mapping in `implementation.md`.
  - Add `tests/AtomUI.Desktop.Controls.Tests/Message/MessageSemanticPartTests.cs` covering descriptor fields, static markers, state-preserved marker identity, generated style hits, queue/close removal and host detach cleanup.
  - Gallery: add two `SemanticPartPreview` sections (one per owner) plus a `Custom Semantic Part styling` example using the generated style classes.
  - Record the pre-existing gap that `WindowMessageManager` never updates `Position` pseudo-classes, so the theme's `:topcenter` alignment branch is unreachable. It is not part of this Semantic Part change and is tracked in `semantic-part.md` §7.1.

## 2026-08-24

- Motion
  - Reuse the shared internal `AtomUI.MotionScene.MotionExecutionState` for MessageCard exit motion scheduling, playback and final `IsClosed` submission while preserving the public `IsClosing` / `IsClosed` contract.
  - Coalesce property-change and template-reapply close scheduling into one execution flow.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Message`.
  - Align generated output paths with `controls/message/index-cn.md` and `controls/message/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Message desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Message desktop architecture documentation under `docs/controls/desktop/feedback/message/overview.md`.
  - Add Message implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Message control-level changelog.
  - Add Message Token documentation covering MessageToken.
