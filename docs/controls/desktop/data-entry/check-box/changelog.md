# CheckBox Changelog

本文档记录 CheckBox 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
