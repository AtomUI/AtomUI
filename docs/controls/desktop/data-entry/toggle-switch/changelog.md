# ToggleSwitch Changelog

本文档记录 ToggleSwitch 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-20

- Fixed
  - 根控件使用初始化会话，把手通过固定模板声明接入；完整基线及加载后恢复最新过渡集合，首次 checked、重挂载和重模板直接定位。
  - 初始状态不排队波纹，待执行反馈在 detach / 模板替换时取消。
  - 默认文本按实际字形轮廓垂直居中，处理字体回退和 shaping offset；保留显式及隐式自定义模板的布局。
- Docs
  - 更新 [共享状态动效初始化设计](../../../../architecture/systems/control-infrastructure/transition-initialization.md)
    与实际实现、原生验证和测试处置说明。

## 2026-10-03

- Fixed
  - ToggleSwitch 的轨道、命中和把手使用最终内容盒；内容画布拥有尺寸传播与偏移动画排列，各内容按自身高度居中。

## 2026-08-25

- Fixed
  - Pause SwitchKnob's loading animation when ToggleSwitch is effectively invisible.
- Docs
  - Document hidden-ancestor animation behavior and cancellation ownership.

## 2026-08-17

- Semantic Part
  - Add Semantic Part descriptors for `ToggleSwitch`：`root`（owner）、`content`（`ContentPresenter`，`.semantic-content`，`Multiple`，对应 on/off 两个内容节点）与 `indicator`（`.semantic-indicator`，`Single`，`ContractType` 为 `TemplatedControl`，映射到内部 `SwitchKnob` 手柄）。
  - Add static `Classes.semantic-content="True"` / `Classes.semantic-indicator="True"` markers to the ToggleSwitch template; the on/off content presenters are statically present in the template, so marker identity and count do not change with check or loading state.
  - Drive the `SwitchKnob` handle fill from the standard `Background` property (theme sets it from `HandleBg`), so `.semantic-indicator` can recolor the handle directly instead of only adjusting `Opacity`.
  - Expose owner-level geometry properties `TrackHeight` / `TrackMinWidth` / `TrackPadding` / `KnobSize` on `AbstractToggleSwitch` (mapping antd Switch `ComponentToken` `trackHeight` / `trackMinWidth` / `trackPadding` / `handleSize`); explicit `Width` now drives internal knob/content geometry, and empty-content switches collapse to `TrackMinWidth`.
  - Move `PART_SwitchKnob` out of the clipped content `Canvas` into the outer template `Panel` (antd-aligned structure), so custom geometry with negative `TrackPadding` or `KnobSize` larger than `TrackHeight` overhangs the track instead of being clipped.
  - Derive the handle shadow `Effect` from `KnobBoxShadow` at `BindingPriority.Style`, letting `.semantic-indicator` override the theme-derived shadow via an `Effect` setter.
  - Keep `AbstractToggleSwitch` and `SwitchKnob` free of independent descriptors.
- Gallery
  - Migrate the ToggleSwitch ShowCase to `GalleryShowCaseHost` with a lazy Semantic Parts Preview and add a custom Semantic Part styling example aligned with the antd Switch `style-class` demo (object/fixed root background, function/medium root background, and a MUI-style checked root + blue handle).

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ToggleSwitch`.
  - Align generated output paths with `controls/toggle-switch/index-cn.md` and `controls/toggle-switch/semantic-cn.md`.

## 2026-06-21

- Docs
  - Add ToggleSwitch control documentation set with architecture overview, implementation notes, Token design and changelog.
  - Document the split between desktop `ToggleSwitch` and shared `AbstractToggleSwitch` implementation.
  - Document checked, loading, content, SizeType, Form, WaveSpirit, SwitchKnob and Token compatibility boundaries.
  - Add ToggleSwitch to the Data Entry control documentation index.
