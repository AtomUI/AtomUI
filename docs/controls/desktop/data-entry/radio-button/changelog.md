# RadioButton Changelog

本文档记录 RadioButton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Theme
  - 键盘焦点改用共享的 `FocusOutlineAdorner`（Ant Design `genFocusOutline`）：3px `ColorPrimaryBorder` 描边，向外偏移 1px，跟随圆角，且不被宿主裁剪；原全局方角 `DefaultFocusAdorner` 已移除。
  - `RadioIndicator#Indicator` 标记 `TemplatedControl.IsTemplateFocusTarget`，并实现 `IFocusOutlineShapeProvider`（未显式设置 CornerRadius 时按自身尺寸给出正圆），描边为包围圆点的圆形。

## 2026-11-08

- Changed
  - `OptionButtonTheme`、`OptionButtonGroupTheme` 的全局 Token 改读 `OptionButtonTokenResource` / `OptionButtonGroupTokenResource`，
    控件级覆盖生效。
  - `ButtonPadding`（antd `buttonPaddingInline`）接入默认与大号 `ContentPadding*` 的左右分量；`ButtonBackground`、
    `ButtonCheckedBackground`（antd `buttonBg` / `buttonCheckedBg`）标为 Reserved，原因是分组在选项之下绘制分隔线与选中边框。
- Fixed
  - `OptionButtonToken` 的 `ContentPaddingLG` 改用大号高度、字号与行高计算，`ContentPaddingSM` 改用小号的同组值（此前分别借用
    小号值与无意义的运算）；字号、行高去掉无效的 `-1` 哨兵，基础字号变化后重新派生。

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
