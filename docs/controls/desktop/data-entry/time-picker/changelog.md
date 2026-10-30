# TimePicker Changelog

本文档记录 TimePicker 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-07

- API / Behavior
  - 保留 `IsNeedConfirm` 属性名并默认启用确认；默认 24 小时制，新增默认关闭的 `IsChangeOnScroll`。
  - 分离提交值、面板候选与悬停预览；输入立即显示候选，显式 false 在关闭/失焦提交，OK、Enter 与 Now 统一显式提交。
  - 修正已有值确认、无效文本、外部值重置、范围端点切换及钉住弹层重复确认路径。
- Theme
  - 时、分、秒使用循环列表和独立物理滚动，选中项始终垂直居中，首尾由真实周期值填充；隐藏这三列的纵向滚动条，保留滚轮、触控板和键盘操作，保持既有 Semantic Part；列内焦点滚动请求不传播至页面滚动容器。
  - 格子内部文字按自然文本行高垂直居中，最小行高只由 TimeViewCell 承载，避免内部 presenter 重复约束高度导致文字顶端绘制。
  - Gallery 确认模式对照示例改为独立的提交值与结果显示，并使用等宽、可换行的分组布局。
  - footer 与行视觉由 ControlTheme 声明，单时间面板展示 8 行；共享 DatePicker 宿主保留自己的高度与提交策略。
- Gallery / Docs
  - 基础示例展示默认确认，增加两种确认模式及提交值反馈；同步示例快照、本地化与控件文档。
- Compatibility
  - 默认确认和默认时钟制变化需调用方复核；显式设置 `IsNeedConfirm=false` 或 `ClockIdentifier=HourClock12` 保留对应配置。

## 2026-10-03

- Implementation
  - Isolate TimeView pointer state by its live presentation source and normalize input coordinates to the selector TopLevel; clear transient state on exit and detach.

## 2026-09-07

- Fixed
  - Keep the TimePicker/RangeTimePicker input reserved width on the content baseline when an explicit `Width` or `HorizontalAlignment=Stretch` is set; the input zone no longer jumps between the placeholder and the selected text widths while the owner width stays governed by the external layout.
  - Align `RangeTimePickerTheme.axaml` with the shared code-driven popup open model: drop the template `IsOpen` two-way binding so the physical popup opens through `ApplyPopupOpenState` after the pinned relay, placement and light-dismiss suppression are prepared; add pinned light-dismiss regression tests for `TimePicker` and `RangeTimePicker`.
  - Add `TimePickerInputWidthStabilityTests` covering explicit-width, stretch and natural-width selection stability.
- Fixed
  - Keep the TimePicker/RangeTimePicker input reserved width on the content baseline when an explicit `Width` or `HorizontalAlignment=Stretch` is set; the input zone no longer jumps between the placeholder and the selected text widths while the owner width stays governed by the external layout.
  - Add `TimePickerInputWidthStabilityTests` covering explicit-width, stretch and natural-width selection stability.


## 2026-09-06

- Architecture
  - Publish the TimePicker family Semantic Part contract with eleven parts on `TimePicker` and twelve on `RangeTimePicker` (`root`, `prefix`, `input`, `secondaryInput` (range only), `suffix`, `clear`, `popup.root`, `popup.container`, `popup.content`, `popup.column`, `popup.item`, `popup.footer`) aligned with the Ant Design `TimePicker` / `TimePicker.RangePicker` semantic structure; see `TimePicker.SemanticParts.cs`, `RangeTimePicker.SemanticParts.cs` and `semantic-part.md`.
  - Reuse the shared `InfoPickerInputTheme.axaml` / `PickerClearUpButtonTheme.axaml` trigger-zone markers (also serving the DatePicker family) for the single-value owner, and add a marked template override in `RangeTimePickerTheme.axaml` while keeping the shared `RangeInfoPickerInputTheme.axaml` free of semantic markers.
  - Mark `popup.container` / `popup.footer` in `TimePickerPresenterTheme.axaml` and `popup.content` / `popup.column` in `TimeViewTheme.axaml`; inject `popup.item` markers on `TimeViewCell` creation in `DateTimePickerPanel` so scrolling reuse, loop shifting, increment changes and popup reopen keep the marker set stable.
  - Use `semantic-time-content` / `semantic-time-column` / `semantic-time-item` marker classes inside the shared `TimeView` subtree so DatePicker popups embedding a `TimeView` never match the DatePicker-family `popup.content` / `popup.body` / `popup.cell` contracts.
  - Add an AtomUI-specific `popup.column` part targeting the four column host panels (the per-column width owners); upstream Ant Design has no column-level slot.
- API
  - Make `IsPickerOpen` and `IsPopupPinnedOpen` public on the shared `InfoPickerInput` base (covering `TimePicker`, `RangeTimePicker`, and the DatePicker family), following the Mentions precedent so Semantic Parts previews and applications can drive and pin the picker popup programmatically, matching the upstream Ant Design `open` prop.
- Gallery
  - Add the TimePicker Semantic Parts tab with a pinned-open `RangeTimePicker` preview presenting 12/24-hour columns and localized part descriptions; migrate the showcase host from `GalleryStickyTabsHost` to `GalleryShowCaseHost` per the standard Semantic Part page model.
  - Add the "Custom Semantic Part styling" example with owner-scoped `TimePicker*Style` / `RangeTimePicker*Style` demos covering prefix, popup root, popup item, input and popup column.
- Tests
  - Add `TimePickerSemanticPartTests` covering descriptor shape (11/12 parts), trigger and popup marker inventory, shared-range-theme non-consumption, generated style hits, popup marker exposure with counts, `ClockIdentifier` toggle / increment change / reopen marker persistence, hidden footer marker survival and the SizeType height baseline regression.
  - Update `TimePickerShowCasePageTests` and `TimePickerShowCaseExamples.snapshot` for the migrated host, semantic preview template, style example and deferred materialization assertions.
- Docs
  - Add `semantic-part.md` as the single source of the public contract and sync `overview.md` / `implementation.md` semantic sections, the size-and-state baseline matrix and marker maintenance boundaries.

## 2026-09-04

- Behavior
  - Inherit the shared InfoPickerInput first-open pinned light-dismiss fix for TimePicker and RangeTimePicker.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record InfoPickerInput as the semantic owner for TimePicker.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Architecture
  - Align TimePicker and RangeTimePicker input surfaces with the shared `AbstractTextInput` / `InputControlFrame` model.
  - Keep time-panel state in TimePicker while projecting native validation, FormStatus and explicit status through the shared frame.

## 2026-08-03

- Implementation
  - Use the shared internal `InfoPickerTextBox` for TimePicker input visuals, keeping padding and text-presenter spacing in the child theme instead of crossing the child template from TimePicker themes.

## 2026-07-16

- Changed
  - Make TimePicker and RangeTimePicker preferred input width ignore user placeholder text and reserve the larger value between the widest formatted value text and the Ant Design TimePicker locale baseline (`Select time`, or `Start time` / `End time` for ranges).
  - Render long TextBox placeholder text with character ellipsis inside the reserved input area.
- Docs
  - Clarify that TimePicker and RangeTimePicker default input width follows Ant Design's `picker === 'time'` locale placeholder branch, not DatePicker's `Select quarter` baseline.
  - Document that long placeholder text is ellipsized inside the reserved input area and must not resize the control.

## 2026-07-05

- API
  - Make `TimePicker.SelectedTime` bind `TwoWay` by default while keeping Avalonia data validation enabled.
- Gallery
  - Add a `v6.0.8` `SelectedTime` binding example that shows ViewModel synchronization without explicit `Mode=TwoWay`.
- Docs
  - Document `SelectedTime` as the controlled Form value and native validation projection point.

## 2026-07-04

- API
  - Add `PickerDisplayTime` as the popup panel display anchor for `TimePicker`, separate from `SelectedTime` and `DefaultTime`.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `TimePicker`.
  - Align generated output paths with `controls/time-picker/index-cn.md` and `controls/time-picker/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete TimePicker desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish TimePicker desktop architecture documentation under `docs/controls/desktop/data-entry/time-picker/overview.md`.
  - Add TimePicker implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add TimePicker control-level changelog.
  - Add TimePicker Token documentation covering TimePickerToken.
