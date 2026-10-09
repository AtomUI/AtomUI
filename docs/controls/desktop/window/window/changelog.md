# Window Changelog

本文档记录 Window 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
