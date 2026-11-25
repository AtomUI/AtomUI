# Alert Changelog

本文档记录 Alert 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-20

- Token
  - `AlertToken.SuccessMessageColor` / `InfoMessageColor` / `WarningMessageColor` / `ErrorMessageColor`（默认 `ColorText`）按类型设置 Alert 前景色；`TextFontWeight`（默认 `Normal`）由 Alert 自身的主题 Setter 读取，本地值与 section 语义样式仍优先。默认值与原外观一致；PrimeNG 采用 message 严重级文字色与 500 / 700 字重。

## 2026-11-14

- API
  - `AbstractMarqueeLabel` 提供 internal `UseCompositedMarquee` 扩展点。`AbstractMarqueeLabel`、`MarqueeLabel` 与 `Alert` 的 public 和 protected API 快照与 `f11459de0745fe02c46e134d94f39b9e35817706` 基线完全一致，共 1,351 行。
- Theme
  - Desktop Alert 沿用原 ControlTheme、告警图标和文本视口。
- Fix
  - 启用合成路径的标签由合成器移动两份保留的文本绘制内容。门控关闭或控件离树时释放动画视觉，重新开启后遵循 `StartDelay` 并从文本起点播放。
- Desktop 影响
  - Alert 继续使用既有帧动画，默认零启动延迟并在悬停时冻结当前位移，指针移出后从该位置继续。
- Tests
  - Desktop `Alert` 与 `RecurringVisualWorkTests` 共 16 个用例通过。在 Avalonia 12.1.6 上用同一 Skia 驱动回放基线与 adapter 工作区，浅深色裁剪、起点、位移、悬停、循环接缝及实际 Gallery 页面共 29 对帧，逐像素差异均为 0。macOS 原生 Gallery 另行打开 Alert 的 Loop Banner 并检查渲染。

## 2026-11-11

- Fix
  - `MarqueeLabel` 的启动统一检查溢出、挂树、运动门控与悬停暂停：`IsPausedOnPointerOver=false` 时指针停在文本上也能在运动门控重新开启后恢复滚动；`IsPausedOnPointerOver=true` 且悬停时，重新布局与延迟计时器都不会启动滚动。

## 2026-11-08

- Theme / Token
  - Alert theme reads every global Token through `AlertTokenResource`, so `Controls[Alert]` reaches status colours, frame, typography and icon spacing.
  - `CloseIconSize` now sizes the close icon (default `FontSizeIcon`, the same 12px as before); `MessageWithDescriptionMargin` is removed (use `DescriptionLabelMargin`).

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
