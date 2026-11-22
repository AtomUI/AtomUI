# CheckBox Changelog

本文档记录 CheckBox 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-20

- Token
  - `CheckBoxToken.IndicatorHoverBorderColor`（默认 `ColorPrimaryHover`）：未选中与半选指示器悬停边框；PrimeNG 采用 `form.field.hover.border.color`。

## 2026-11-17

- Theme
  - Shadcn 各 Style 的勾选框圆角改为按附录 C 的专用值（Vega/Nova/Mira 4、Maia 6、Luma/Rhea 5、Lyra/Sera 0），不再复用按钮圆角阶梯：Luma/Maia 的 26 在 16px 指示器上会被 CSS 语义钳到 8，视觉上整个勾选框变成圆形。

## 2026-11-11

- Theme
  - 键盘焦点改用共享的 `FocusOutlineAdorner`（Ant Design `genFocusOutline`）：3px `ColorPrimaryBorder` 描边，向外偏移 1px，跟随圆角，且不被宿主裁剪；原全局方角 `DefaultFocusAdorner` 已移除。
  - `CheckBoxIndicator#Indicator` 标记 `TemplatedControl.IsTemplateFocusTarget`，描边只包围勾选框，不包含文字。

## 2026-08-16

- Semantic Part
  - Add Semantic Part descriptors for `CheckBox`：`root`（owner）、`icon`（`CheckBoxIndicator`，`.semantic-icon`，`Single`，`ContractType` 为 `TemplatedControl`）与 `label`（`ContentPresenter`，`.semantic-label`，`Single`）。
  - Add static `Classes.semantic-icon="True"` / `Classes.semantic-label="True"` markers to the CheckBox template; the checked/unchecked/indeterminate states share one indicator node, so marker identity and count do not change with check state.
  - Keep `CheckBoxGroup`、`CheckBoxItemsControl` and `CheckBoxIndicator` free of independent descriptors.
- Gallery
  - Migrate the CheckBox ShowCase to `GalleryShowCaseHost` with a lazy Semantic Parts Preview and add a custom Semantic Part styling example (fixed and checked-conditional icon/label styling).
- Docs
  - Add `semantic-part.md` and rewrite the stale `root/input/trigger/popup/validation` LLMS semantic table to `root/icon/label`.

## 2026-07-06

- Behavior
  - Make `CheckBoxGroup.CheckedItems` default to TwoWay binding and enable Avalonia data validation.
  - Project bound `CheckedItems` collections into the internal items control through snapshots, preventing duplicate selection writes.
  - Support `INotifyCollectionChanged` mutations on bound `CheckedItems` collections and refresh checked visuals, Form value notifications and `CheckedChanged` event data.
- Gallery
  - Add a `v6.0.8` CheckBoxGroup binding example showing user selection and bound collection mutation staying synchronized.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `CheckBox`.
  - Align generated output paths with `controls/check-box/index-cn.md` and `controls/check-box/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete CheckBox desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish CheckBox desktop architecture documentation under `docs/controls/desktop/data-entry/check-box/overview.md`.
  - Add CheckBox implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add CheckBox control-level changelog.
  - Add CheckBox Token documentation covering CheckBoxToken.
