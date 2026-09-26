# WindowTitleBar Changelog

本文档记录 WindowTitleBar 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-25

- Theme
  - Align Linux/macOS `WindowTitleBarButton` and `WindowTitleBarToggleButton` geometry with the adjacent managed caption band: the AddOn themes now take `CornerRadius` from `CaptionButtonCornerRadius` instead of the shared `BorderRadiusSM`, and the title bar trailing container separates the AddOn area from `PART_CaptionButtonGroup` with `CaptionGroupSpacing`, so the buttons read as one set with the system caption buttons instead of touching them. The spacing collapses when no AddOn content is present, and the Windows trailing layout keeps zero spacing.
  - Extract the managed caption geometry into an internal `CaptionButtonFrame` (inset rounded background layer plus layout-driving content layer) with its own ControlTheme. `CaptionButtonTheme` and both AddOn themes now compose that frame and only contribute their own presenters, so the two-layer structure is declared once instead of being copied per theme. Linux passes `CaptionButtonBackgroundInset` to the shared frame, shrinking the background to the same 26 logical pixel circle as the adjacent caption buttons while the 30 logical pixel hit surface is preserved. Windows square geometry is unchanged.
- Token
  - Add `CaptionButtonCornerRadius` to `WindowTitleBarToken`, derived from `CaptionButtonIconSize` and `CaptionButtonPadding`. It mirrors the size-derived circle of the internal `CaptionButton`; the control keeps its measured derivation so the fullscreen caption-button size override still resolves correctly.
  - Add `CaptionButtonBackgroundInset` to `WindowTitleBarToken`, derived from `SizeUnit`, and make the Linux caption group consume it instead of a literal, so the caption band and the AddOn buttons share one background-inset source.
- Docs
  - Record the Linux/macOS geometry rule in the AddOn platform matrix, ControlTheme section and verification list, and add the new Tokens to the token table.
- Verification
  - Cover Linux and macOS AddOn button and toggle button corner equality against the real `PART_PinButton`, the visible background size and inset equality, the operation-area gap, the no-AddOn no-phantom-gap case, and the preserved 30 logical pixel hit surface in `WindowTitleBarButtonTests`.
  - Cover the shared frame rendering itself in `WindowTitleBarCaptionFrameRenderingTests`: a real render pass paints the inset circular background for both AddOn buttons, while the hit surface corners stay unpainted.

## 2026-09-10

- Platform
  - Align Windows `WindowTitleBarButton` and `WindowTitleBarToggleButton` with managed caption-button geometry: a full-height square hit surface, square corners, transparent resting background and Arrow cursor, while preserving Linux/macOS managed visuals.
- Theme
  - Project `WindowTitleBar.OsType` through both AddOn presenters and select Windows-specific active, inactive, hover, pressed and disabled states without assigning native caption roles or window commands.
  - Keep both AddOn theme assets as top-level `ControlTheme` documents so `x:Class` remains valid for IDE tooling and compiled theme loading.
- Architecture
  - Share a stateless `WindowsCaptionButtonLayout` measurement helper between system Windows caption buttons and public AddOn buttons; keep application-owned container spacing and business icon sizing independent from the hit surface.
- Verification
  - Cover Windows button and toggle geometry, host platform projection and detach fallback, real pointer hover/pressed/exit feedback, and non-Windows size, corner-radius and cursor preservation in `WindowTitleBarButtonTests`.

## 2026-09-04

- Design
  - Split logo content resolution from explicit state: public `Logo`/`LogoTemplate` hold developer values only; rendering resolves `Window.EffectiveLogo`/`EffectiveLogoTemplate` (explicit → host `Icon` → main-window explicit logo → main-window `Icon`, with the built-in `WindowIcon` template).
- API
  - Remove the one-shot default-logo injection (`ApplyDefaultLogoIfNeeded`/`TryApplyWindowIconLogo`) that wrote fallback values into public properties; runtime `Logo = <Control>` now renders directly and `Logo = null` falls back to the default icon. `LogoVisibility = Never` remains the way to fully hide the logo.
- Theme
  - Bind the three platform templates' `PART_Logo` to `WindowTitleBar.EffectiveLogo` / `EffectiveLogoTemplate` through `TemplateBinding`, preserving standalone and custom-title-bar values; bind both fullscreen hosts to the Window effective title and logo state.
- Lifecycle
  - Move Window effective-logo notifications into every title bar's host projection lease; create child-window main-logo fallback subscriptions only while the child is open and release them on local takeover or close.
- Docs
  - Rewrite the Effective Logo section; document the fallback chain and the explicit-values-only contract.
- Verification
  - Add `WindowTitleBarEffectiveLogoTests` covering icon fallback, runtime control replacement without template residue, custom-title-bar precedence, runtime main-window updates, zero pre-open subscriptions and release on close; cover both fullscreen theme hosts and the caption-button-preserving title-hidden template state.

## 2026-09-03

- Design
  - Define the in-title-bar title text visibility model: `IsEffectiveTitleVisible = IsTitleVisible && Title is not null`, with `LogoVisibility=Auto` following the titleless branch when the title is hidden.
- API
  - Add `IsTitleVisible` (`bool`, default `true`) to `WindowTitleBar` and expose the `Window` AddOwner facade with `NotifyConfigureTitleBar` projection; the OS-level window title is unaffected.
- Theme
  - Bind the three platform templates' `PART_ContentPresenter` and the fullscreen layer's `FullscreenTitleText` to internal effective visibility direct properties instead of `Title != null`.
- Docs
  - Add the title display model section; update the API table, Logo Auto matrix, template contract, compatibility invariants, test list and LLMS semantic areas.
- Verification
  - Cover defaults, effective computation, empty-string semantics, Logo Auto interplay, Window projection and fullscreen binding in `WindowTitleBarTitleVisibilityTests`.

## 2026-08-23

- Design
  - Define public `WindowTitleBarButton` and `WindowTitleBarToggleButton` controls for `LeftAddOn` and `RightAddOn` content.
  - Share managed caption visual tokens and active/inactive state with system caption buttons while keeping Window operations, native element roles and platform chrome internal.
- API
  - Add `WindowTitleBarButton : IconButton` and `WindowTitleBarToggleButton : ToggleIconButton` as additive public AXAML controls.
- Theme
  - Register independent AddOn themes that reuse `WindowTitleBarToken` dimensions, active/inactive colors, interaction states and motion policy without entering the system caption contract.
- Integration
  - Add Gallery Workspace coverage for appearance and Wave Spirit actions, including checked-state synchronization with the existing menu.
- Verification
  - Cover host-state projection, standalone fallback, icon-pair switching, theme asset registration and pointer-input isolation in `WindowTitleBarButtonTests`.

## 2026-08-19

- Architecture
  - Define a shared Window host projection for every `WindowTitleBar` in an AtomUI Window logical tree.
  - Make `Window` own projection definitions and lease creation while each `WindowTitleBar` owns and disposes its connection lease.
  - Include drag and double-click interaction subscriptions in each host projection lease while keeping default-title-bar content projection separate.
  - Keep title-bar height hints and CSD geometry owned only by the default title bar; content-area title bars receive caption, drag and double-click behavior without becoming geometry owners.
- Platform
  - Keep CSD windows on `WindowDecorations.Full` when the AtomUI title bar is hidden, and hide only the drawn title-bar visual so native window state transitions remain platform-owned.
- Verification
  - Extend the caption button lifecycle contract to cover content-area title bars, real pointer double-click toggling, multiple title bars per Window, host switching and lease release.

## 2026-08-18

- Design
  - Define a conditional gap between effective Windows/Linux Logo and LeftAddOn content while keeping macOS Leading and Logo/Title grouping unchanged.
- Token
  - Add the dedicated `LogoAndLeftAddOnSpacing` semantic with a default `SpacingXXS` value of 4 logical pixels.
- Architecture
  - Separate managed caption button requested visibility from Window capability and platform support.
  - Keep Window as the operation and state owner while CaptionButtonGroup derives presentation state and forwards declarative actions.
- Docs
  - Add the caption button configuration design covering Public API defaults, platform/state matrices, template flow, lifecycle and verification boundaries.
  - Synchronize the Leading composition, title safe-region formula, Token contract and conditional-spacing verification requirements.

## 2026-07-28

- Integration
  - Project the existing left and right add-on content and template properties from the default `Window` facade.
- Docs
  - Clarify default Window ownership and preserve `WindowTitleBar` layout and interaction responsibilities.

## 2026-07-23

- Theme
  - Place visible Windows and Linux `PART_Logo` content at the physical leading edge before `PART_LeftAddOn`, while keeping macOS Logo/Title grouping unchanged.
- Tests
  - Cover Windows/Linux template Logo ordering and verify explicit title alignments plus dynamic add-on changes against the shared safe-region formula.
- Docs
  - Synchronize the platform-specific Logo role model and Token spacing semantics.

## 2026-07-22

- API
  - Add `WindowTitleBarTitleAlignment`, `WindowTitleBar.TitleAlignment` and the `Window.TitleAlignment` owner projection.
- Theme
  - Use one full-frame `WindowTitleBarLayoutPanel` with Leading, Title and Trailing roles in the default, ImagePreviewer and fullscreen title hosts.
  - Treat native chrome insets, managed operations, add-on margins and conditional title spacing as single-source layout inputs.
  - Preserve `TitleBarPadding` as managed content spacing after the native chrome safe extent without shifting the full-frame title center.
- Implementation
  - Add stateless platform strategies and publish native chrome insets from the existing Window platform metric path.
  - Recalculate zero, hidden and dynamically replaced left/right add-on content without cached widths or ghost spacing.
- Docs
  - Synchronize the title alignment model, layout formulas, derived hosts and LLMS inputs.

## 2026-07-21

- Docs
  - Rewrite the control design, implementation and Token documents from the WindowTitleBar source, Themes, Window host and ImagePreviewer integration.
  - Define the cross-platform title alignment contract, platform Strategy ownership, CSD matrix, full-frame Template contract and shared safe-region algorithm.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `WindowTitleBar`.
  - Align generated output paths with `controls/window-title-bar/index-cn.md` and `controls/window-title-bar/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete WindowTitleBar desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish WindowTitleBar desktop architecture documentation under `docs/controls/desktop/window/window-title-bar/overview.md`.
  - Add WindowTitleBar implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add WindowTitleBar control-level changelog.
  - Add WindowTitleBar Token documentation covering WindowTitleBarToken.
