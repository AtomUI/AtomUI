# RadioButton Changelog

本文档记录 RadioButton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-23

- Theme
  - Migrate OptionButton's19 Own defaults to DependencyDefaultsV2 with explicit font, ratio and padding dependencies; consume base ButtonPadding and correct Small/Large inputs.
  - Consume explicit ordinary and Outline checked backgrounds while preserving transparent defaults, Solid/disabled colors and native property priority.
  - Resolve Item and Group Global inputs independently and update Item Transition durations on theme changes.
- Compatibility
  - Correct the Large padding resource from15,0 to15,7 while preserving default rendered pixels under existing size constraints; keep line-height units as ratios.
- Docs
  - Define the exact dependency, optional-background and compound-height contracts in the owning RadioButton module.

## 2026-10-23

- Theme
  - Transition 时长在实际 owner 上动态消费有效主题作用域；保留原生样式覆盖、默认时长与模板状态。共享框体使用其公开模板 owner，单选指示器保留 Mid/Fast 的独立输入。完整家族迁移与真实动画时序仍按专项任务验收。

## 2026-10-20

- Fixed
  - 内部 `RadioIndicator` 构造时持有共享初始化会话，挂载后计算的圆点尺寸直接提交，加载后才恢复过渡。初始 checked 不创建波纹任务，待执行波纹在分离和模板变更时取消。

## 2026-08-16

- Semantic Part
  - Add Semantic Part descriptors for `RadioButton`：`root`（owner）、`icon`（`RadioIndicator`，`.semantic-icon`，`Single`，`ContractType` 为 `TemplatedControl`）与 `label`（`ContentPresenter`，`.semantic-label`，`Single`）。
  - Add static `Classes.semantic-icon="True"` / `Classes.semantic-label="True"` markers to the RadioButton template; checked/unchecked share one indicator node, so marker identity and count do not change with check state.
  - Keep `RadioButtonGroup`、`RadioIndicator`、`OptionButton` and `OptionButtonGroup` free of independent descriptors.
- Gallery
  - Migrate the RadioButton ShowCase to `GalleryShowCaseHost` with a lazy Semantic Parts Preview and add a custom Semantic Part styling example (fixed and checked-conditional icon/label styling).
- Docs
  - Add `semantic-part.md` and rewrite the stale `radio-root/radio-group/indicator/option-group/option-item` LLMS semantic table to `root/icon/label`.

## 2026-07-29

- Design
  - Define `OptionButtonGroup.Orientation` as the single source of truth for layout direction, connected corners, separators and directional navigation, with `Horizontal` as the default.
  - Define Avalonia-native vertical width semantics: Stretch fills available width, non-Stretch alignment uses natural width, and explicit Width remains authoritative.
  - Define OnlyOne/First/Middle/Last corner mapping and group-local rendering rules for Horizontal and Vertical.
- Theme
  - Define direction-scoped Group and Item sizing, vertical leading content alignment and `EffectiveCornerRadius` consumption without introducing orientation-specific Token.
  - Align `CustomizableSizeType.Custom` with the global customization contract by retaining base values without a Custom preset selector.
- Docs
  - Add the OptionButtonGroup orientation design and synchronize RadioButton family architecture, implementation and Token ownership.

## 2026-07-06

- Behavior
  - Make `RadioButtonGroup.CheckedItem` default to TwoWay binding and enable Avalonia data validation.
  - Keep user selection, Form value and ViewModel state on the same `CheckedItem` source of truth.
- Gallery
  - Add a `v6.0.8` RadioButtonGroup binding example showing user selection and ViewModel updates staying synchronized.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `RadioButton`.
  - Align generated output paths with `controls/radio-button/index-cn.md` and `controls/radio-button/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete RadioButton desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish RadioButton desktop architecture documentation under `docs/controls/desktop/data-entry/radio-button/overview.md`.
  - Add RadioButton implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add RadioButton control-level changelog.
  - Add RadioButton Token documentation covering RadioButtonToken.
