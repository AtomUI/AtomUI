# Alert Changelog

本文档记录 Alert 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Fix
  - `MarqueeLabel` 的启动统一检查溢出、挂树、运动门控与悬停暂停：`IsPausedOnPointerOver=false` 时指针停在文本上也能在运动门控重新开启后恢复滚动；`IsPausedOnPointerOver=true` 且悬停时，重新布局与延迟计时器都不会启动滚动。

## 2026-11-07

- API
  - `MarqueeLabel`（`AbstractMarqueeLabel`）新增 `StartDelay`（`TimeSpan`，默认 `TimeSpan.Zero`）：滚动动画每次从静止状态（重新）启动前先等待该时长，等待期间文字保持静止、内部 `IsMarqueeAnimating` 为 `false`；门控关闭或脱离视觉树时取消尚未触发的延迟。
  - `MarqueeLabel` 新增 `IsPausedOnPointerOver`（`bool`，默认 `true`）：为 `false` 时忽略指针悬停变化，滚动不因悬停暂停。
- Desktop 影响
  - 默认不变：`StartDelay` 为零时启动路径与此前一致，`IsPausedOnPointerOver` 默认保持悬停暂停；Desktop 唯一使用者 Alert 不设置这两个属性。
- Tests
  - 新增 `MarqueeStartTests`（共享层）：默认值、零延迟立即启动、延迟推迟首个周期、门控关闭与脱离视觉树取消待定延迟、悬停默认暂停、关闭悬停暂停后忽略悬停、悬停离开后的恢复同样经过延迟。

## 2026-08-19

- Gallery
  - Align the semantic style example copy with the Gallery semantic copy convention: the title becomes "Custom Semantic Part styling" and the description describes the owner-scoped semantic part styles. Remove the dom/classNames wording (AtomUI has no DOM concept) and add `ShouldNotContain("semantic dom"/"classNames")` guards to the showcase page test.

## 2026-08-14

- Feature
  - Publish `root`, `icon`, `section`, `title`, `description`, `actions`, and `close` Semantic Parts for Alert.
  - Generate owner-scoped `Alert*Style` types and add static template markers without changing Alert behavior or layout ownership.
  - Add `StrokeDashArray` root surface projection so Semantic Styles can express the Ant Design dashed-border example.
- Fix
  - Recompute `:has-extra-action` when `ExtraAction` changes after template application so pseudo-class and presenter state stay synchronized.
- Gallery
  - Add a deferred Semantic Part preview and reproduce the Ant Design `Object styles` / `Function styles` example with owner-scoped styles.
- Docs
  - Add the complete Alert Semantic Part contract and synchronize overview, implementation, LLMS sources, and verification boundaries.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Alert`.
  - Align generated output paths with `controls/alert/index-cn.md` and `controls/alert/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Alert desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Alert desktop architecture documentation under `docs/controls/desktop/feedback/alert/overview.md`.
  - Add Alert implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Alert control-level changelog.
  - Add Alert Token documentation covering AlertToken.
