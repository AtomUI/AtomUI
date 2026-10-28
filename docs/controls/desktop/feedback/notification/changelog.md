# Notification Changelog

本文档记录 Notification 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-27

- Motion
  - 阴影 Frame 直接作为动效目标，移除内部 Actor 与额外 ContentPresenter。
  - 进入/退出改用共享 Motion 轨迹和框架时钟，关闭从当前透明度与矩阵连续替代入场。
  - 协调层保留业务版本与单次关闭提交，移除旧过渡订阅、两套取消源、额外帧屏障与等待旧执行清理的路径。
  - 保持阴影、64 DIP 方向、堆叠与内容快照、进度条及 Semantic Part route。

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
  - Add regression coverage for alternating Notification/Message manager activation, reentrant Show ordering,
    single Move publication, zero attach/detach and WeakReference collection after disposal.

## 2026-09-14

- Theme / Semantic Part
  - Preserve WindowNotificationManager `Padding` customization with the shared feedback stack while keeping the default placement and queue hover bounds.
  - Align `listContent` with the public `ItemsControl` contract and update examples to use supported size and alignment setters; item spacing remains owned by the internal feedback stack.
- Lifecycle
  - Close cards after a persistent host detach while preserving cards shown before the first attachment and queues moved back into the visual tree during the same dispatch cycle.
- Correctness
  - Initialize standalone notification motion state and align the card template, progress layout and render-only actor with feedback stack execution.
- Token
  - Retain `NotificationTopMargin` and `NotificationBottomMargin` as placement-dependent manager padding defaults.

## 2026-09-13

- Lifecycle / correctness
  - Serialize entry and exit actor writes through the completion of the previous motion's asynchronous cleanup, with cancellation-aware handoff and six-placement regression coverage.
  - Release expired and already-closing cards from scheduler scratch lists at the end of each wakeup, and reschedule from the time after synchronous close callbacks.
  - Select stable close batches for MaxItems and DestroyAll so immediate removal and reentrant callbacks preserve ordering, new messages and disposal boundaries.
- Cleanup
  - Remove redundant motion guards, unused motion configuration, duplicated projection checks and empty Gallery overrides while preserving animation and template contracts.
- Design / correctness
  - Define measured variable-height stack projection, `Min(3, StackThreshold)` visible layers, placement-aware back-layer clipping and immediate sibling reflow while a closing card retains its last projection.
  - Make list hover an independent state so count and threshold changes cannot desynchronize expansion or whole-stack lifetime pause.
  - Revalidate the last real pointer screen position after window resize or placement layout movement, so a moved stack cannot retain stale hover expansion or lifetime pause state.
- Motion / performance
  - Require Notification enter/exit to use a render-only actor so translate/fade does not invalidate layout on every animation frame.
  - Separate the transparent translated prepare state from the active transition with one real TopLevel animation frame, including the first card, and reuse immutable directional transforms.
  - Project threshold-hidden cards to the capped deepest scale and placement-aware half clip in the same collapse commit as opacity, preventing their full content and shadow from flashing during hover reset without adding timers or per-frame callbacks.
  - Freeze at most three visible card-content surfaces into one-shot `RenderTargetBitmap` snapshots during expanded-to-collapsed transitions; restore live content and deterministically dispose snapshots on completion, interruption, close, retemplate, detach, motion disable and manager disposal.
- API behavior
  - Disable Notification Stack by default; explicit `IsStackEnabled=true` retains threshold collapse and whole-stack hover behavior.
- Gallery
  - Add a dedicated `v6.1.9` Notification Stack showcase with isolated configuration, variable-height permanent notices and scoped destroy/dispose behavior.
- Internal architecture
  - Name the per-card motion execution owner `FeedbackCardMotionCoordinator` to reflect its coordination of actor state, cancellation and close completion.

## 2026-09-12

- Design
  - Adopt the shared Feedback stack infrastructure with the Notification stack enabled by default, threshold `3`, whole-stack hover expansion and three visible collapsed cards.
  - Define scale `1` / `0.94` / `0.88`, 8 DIP collapsed offsets, position-aware expansion and unlimited `MaxItems` as the Notification contract.
  - Add the shared `IsStackEnabled`, `StackThreshold` and `DestroyAll()` manager contract.
- Performance / lifecycle
  - Replace fixed expiration and cleanup polling with one lazy nearest-deadline scheduler that refreshes progress only for visible active cards.
  - Require stable ItemsSource-backed cards and deterministic cleanup for retemplate, detach, rehost, destroy, callback failure and dispose.
- Motion
  - Replace placement-specific oversized and scale-distorting entry motions with the shared Feedback 64 DIP translate/fade motion and full-duration Ant ease-in-out curve.
  - Animate existing cards to their new queue positions during add, remove, collapse and expand instead of jumping their layout bounds.

## 2026-09-11

- Semantic Part
  - Publish two owner descriptors aligned with the upstream Notification semantic keys: `NotificationCard` exposes
    `wrapper` / `icon` / `section` / `title` / `description` / `actions` / `close` / `progress` (plus implicit `root`),
    and `WindowNotificationManager` exposes `listContent` (plus implicit `root`, which maps the upstream list).
    `Since` is `6.2.0` for both.
  - Restructure `NotificationCardTheme.axaml` to the upstream notice DOM
    (`root > [wrapper > (icon, section > (title, description)), actions, close, progress]`): add a real `section`
    (`StackPanel`) that owns the title/description gap, add an `actions` region, and express `close` / `progress` as
    overlays on `Panel#PART_Layout` (`Panel` replaced `Grid#PART_Layout`) instead of in-flow children. This is an
    authorized rendered-result and theme-contract change; the structural diff is documented in `semantic-part.md` §7.1.
  - Add static `Classes.semantic-*` markers in `NotificationCardTheme.axaml` and `WindowNotificationManagerTheme.axaml`;
    `progress` is RuntimeCreated and gets its marker injected by `ConfigureProgressBar` /
    `NotificationCardSemanticParts.ProgressClass`. The built-in themes do not consume `.semantic-*` for default visuals.
  - Add generated semantic style types `NotificationCardWrapperStyle`, `NotificationCardIconStyle`,
    `NotificationCardSectionStyle`, `NotificationCardTitleStyle`, `NotificationCardDescriptionStyle`,
    `NotificationCardActionsStyle`, `NotificationCardCloseStyle`, `NotificationCardProgressStyle` and
    `WindowNotificationManagerListContentStyle`.
  - Add `docs/controls/desktop/feedback/notification/semantic-part.md` as the authoritative Part contract, and document
    the descriptor/marker mapping in `implementation.md` and `overview.md`.
  - Add `tests/AtomUI.Desktop.Controls.Tests/Notifications/NotificationSemanticPartTests.cs` covering descriptor fields,
    static markers, notice structure parenting, generated style hits, actions visibility, runtime progress marker
    lifecycle, queue/close removal and host detach cleanup.
- API
  - Add `NotificationCard.Actions` / `NotificationCard.ActionsTemplate` (upstream notice `actions`), plus the matching
    `INotification.Actions` / `INotification.ActionsTemplate` default interface members, `Notification.Actions` /
    `Notification.ActionsTemplate` and the manager's `Show` projection. `INotification` default members keep existing implementers source-compatible.
  - Add `NotificationCard.BoxShadowProperty` (`Border.BoxShadowProperty.AddOwner<NotificationCard>()`), so the `root`
    surface shadow can be customized from owner-scoped Semantic Styles.
  - Add a public parameterless `NotificationCard()` constructor alongside `NotificationCard(WindowNotificationManager)`,
    mirroring `MessageCard`; a standalone card has no host manager, so hover-pause feedback does not apply.
  - Add `WindowNotificationManager.OnDetachedFromVisualTree` to clear `PART_Items`, matching the `WindowMessageManager`
    memory-leak fix for the sibling host layer.
- Token
  - Remove the control-local `× 2/3` scaling that was applied to every notice spacing token
    (`NotificationPadding`, `NotificationSectionSpacing`, `NotificationActionsMargin`, `NotificationCloseButtonMargin`,
    `NotificationIconMargin`). Upstream derives these directly from the global tokens
    (`notificationPaddingVertical = paddingMD`, `notificationPaddingHorizontal = paddingLG`, `gap: marginSM` /
    `marginXS`, `margin-top: marginSM`), so the card rendered shorter than antd. Measured before/after against the
    antd reference: card height went from ~83 to ~90 logical units at width 384, matching antd's measured 89.
  - `NotificationPadding` is now symmetric (upstream `padding: paddingMD paddingLG`).
  - Add `NotificationSectionSpacing` (upstream `section` `gap: marginXS`), `NotificationTitlePadding` (upstream
    `.notice-closable` `padding-inline-end`), `NotificationActionsMargin` (upstream `margin-top: marginSM`),
    `NotificationCloseButtonMargin` and a `BorderRadiusLG`-inset `NotificationProgressMargin`.
  - Remove `NotificationContentMargin` / `HeaderMargin` (replaced by `NotificationSectionSpacing`) and the card edge
    margins `NotificationMarginBottom` / `NotificationTopMargin` / `NotificationBottomMargin`: list padding is now the
    manager's `Padding` (`MarginLG`) and item spacing is `listContent`'s `Spacing` (`UniformlyMargin`), so the `root`
    highlight box equals the visible card.
  - Render the progress track: upstream's notice progress paints the full-width track (`rgba(0, 0, 0, 0.04)`) and then
    the coloured value on top; `NotificationProgressBar.Render` drew only the coloured fill, so the remaining time was
    invisible against the card background. Added `NotificationProgressTrackBg` (`ColorFillQuaternary`) and
    `NotificationProgressBar.ProgressTrackBrush`, wired in `NotificationProgressBarTheme.axaml`.
  - Align the notice icon vertically: upstream's notice wrapper is `display: flex; align-items: flex-start`, so the icon
    top-aligns with the title line. Avalonia's `DockPanel` stretches children, and with an explicit `Height` the
    `IconPresenter` was vertically centred (measured icon centre 37.0 vs antd 34.8 logical), so the theme now sets
    `VerticalAlignment="Top"`. Covered by the icon/title top-offset assertion in `NotificationSemanticPartTests`.
- Gallery
  - Add the Semantic Parts tab (two-owner preview listing all 11 parts) via `GalleryShowCaseHost`, an `Actions` example
    mirroring `notification/demo/with-btn.tsx`, and a `Custom Semantic Part styling` example mirroring
    `notification/demo/style-class.tsx` (default green card and error-branch red card).
  - Add localization units for the new titles, descriptions and example content (en-US / zh-CN / zh-TW / pt-BR) and
    update the catalog member-order baseline and pt-BR unit total.
- Fix
  - `Custom Semantic Part styling` silently did nothing: `ApplySemanticStyleStyles` looked the `Styles` up through
    `Application.Current.TryGetResource`, but the resource is declared in the page's `UserControl.Resources`, so the
    lookup returned false, the attach was skipped, and the cards fell back to the default look. It now uses the page's
    own `Resources.TryGetResource`, matching the working Message page. Covered by
    `Notification_Semantic_Style_Buttons_Produce_Styled_Cards`, which clicks the real buttons and asserts the produced
    cards' frame background / border / radius / shadow and the icon / title / description brushes in both branches
    (it failed with `White` vs `#F6FFED` before the fix). The lookup-scope contract is documented in `semantic-part.md` §5.3.
  - Align the demo's `Default Notification` branch with upstream `notification/demo/style-class.tsx`, which opens
    `api.info`, so it now passes `NotificationType.Information` instead of `Success`.
  - Fix the card's right/bottom `box-shadow` band being clipped to a 1-logical-pixel sliver. The template previously
    nested `Panel#PART_Layout` between `LayoutAwareMotionActor` and `Border#Frame`, so the shadow — which `Frame`
    paints — was clipped by the intermediate panel's bounds (measured 2 device pixels on the right/bottom vs antd's 8).
    `Border#Frame` is now the motion actor's direct content (only `ContentControl`'s own `PART_ContentPresenter`
    remains between them), exactly like `Message`'s working `Border#PART_Frame`. `Frame` keeps `Padding=0` and a new
    inner `Border#ContentBox` consumes `NotificationPadding`, so `Panel#PART_Layout` stays identical to the CSS
    padding box and the absolutely-positioned `close`/`progress` overlays line up with upstream. Covered by
    `Frame_Sits_Directly_In_The_Motion_Actor_So_BoxShadow_Is_Not_Clipped`.

## 2026-08-24

- Motion
  - Reuse the shared internal `AtomUI.MotionScene.MotionExecutionState` for NotificationCard exit motion scheduling, playback and final `IsClosed` submission while preserving the public `IsClosing` / `IsClosed` contract.
  - Coalesce property-change and template-reapply close scheduling into one execution flow.

## 2026-07-20

- Fix
  - Add `NotificationType.Default` for plain notifications so the default notification path renders without a type icon.
  - Align the close button size, hover background and pressed background with shared text/icon state tokens.
  - Derive `NotificationProgressBg` from primary border hover color to primary color.
  - Align the default auto-close expiration with the documented 4.5 second duration.
  - Reduce the default internal card spacing tokens by one third while preserving the external stack spacing.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Notification`.
  - Align generated output paths with `controls/notification/index-cn.md` and `controls/notification/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Notification desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Notification desktop architecture documentation under `docs/controls/desktop/feedback/notification/overview.md`.
  - Add Notification implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Notification control-level changelog.
  - Add Notification Token documentation covering NotificationToken.
