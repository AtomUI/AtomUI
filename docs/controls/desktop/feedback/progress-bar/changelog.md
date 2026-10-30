# ProgressBar Changelog

本文档记录 ProgressBar 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 ProgressBar 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-08-15

- Fix
  - Replace transparent line/circle/dashboard Semantic proxies with the actual visible template renderers, retaining owner rendering only as a legacy custom-template fallback.
  - Replace line rail, track, and success Shapes with `Border` box visuals, and make `LineProgressPanel` their sole layout owner so Bounds are applied once without owner-side child layout or `Shape.Stretch` transforms.
  - Make `StepsProgressPanel` the sole layout owner for runtime tracks and indicator so deferred Examples render Start/Center/End positions correctly on first attachment without a Semantic Parts Tab round trip.
  - Restore visible Steps percentage text by inheriting the owner `Foreground` instead of binding the label to the line-only `PercentageLabelColor` state.
  - Align Semantic highlight Bounds with the Ant Design structure: rail covers the full groove, track covers the completed geometry, and indicator covers the percentage or status information region instead of the whole control.
  - Add `CircleProgressPanel` as the sole Circle/Dashboard Path layout owner so rail and track use the centered `body size - StrokeThickness` viewport that matches the Ant Design SVG radius contract.
- Gallery
  - Make explicit `PartDescriptions` order the Semantic Preview display order while retaining descriptor order for undescribed parts.
  - Align the ProgressBar Semantic Preview structure with the Ant Design `v6.1.3` demo: keep the selector toolbar inside the preview stage, preserve the middle token gap, and top-align the 200px line, steps, circle, and dashboard preview area.
  - Vertically center the type selector and Gradient switch on the Semantic Preview toolbar.
  - Synchronize the five Progress Semantic Part descriptions with the official English wording and all Gallery locales.

## 2026-08-14

- Feature
  - Publish owner-scoped `root`, `body`, `rail`, `track`, and `indicator` Semantic Parts for the ProgressBar family; `StepsProgressBar` follows the upstream steps structure and does not publish a rail.
  - Project line progress onto stable template `Border` targets, circle and dashboard geometry onto stable template Shapes, and expose each runtime steps Rectangle as a reusable `track` target.
  - Generate concrete owner Style types while keeping default themes independent from `.semantic-*` selectors.
- Fix
  - Preserve `ChunkWidth=NaN` as the automatic sizing state so runtime Large/Middle/Small changes continue to resolve to `14/6/2` instead of retaining the first computed width.
  - Release and rebuild runtime steps targets on detach and template reapplication without retaining old parents or subscriptions.
- Theme
  - Give `CircleProgress` and `DashboardProgress` concrete leaf templates so source generation validates every public owner instead of relying on an inherited abstract-only template.
- Gallery
  - Add a truly deferred four-type Semantic Preview aligned with the Ant Design `v6.1.3` `_semantic.tsx` example.
  - Reproduce the Ant Design `v6.1.3` `style-class.tsx` visual example with six percentages, root padding/radius, rail color/radius, hue gradients, and CSS-ease transitions; label it with the current AtomUI version and AtomUI Semantic Part terminology.
  - Match the upstream semantic style example's vertical rhythm without adding a second `SpacingLG` gap between naturally measured ProgressBar rows.
- Docs
  - Add the complete ProgressBar Semantic Part contract and synchronize overview, implementation, Gallery, LLMS sources, compatibility boundaries, and verification requirements.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ProgressBar`.
  - Align generated output paths with `controls/progress-bar/index-cn.md` and `controls/progress-bar/semantic-cn.md`.

## 2026-06-22

- Docs
  - Establish ProgressBar desktop architecture documentation under `docs/controls/desktop/feedback/progress-bar/overview.md`.
  - Add ProgressBar implementation documentation for shared RangeBase state, progress normalization, line / step / circle / dashboard rendering, lifecycle and maintenance invariants.
  - Add ProgressBar Token documentation for default colors, line spacing, line icon sizes, circle inner info limits and token compatibility boundaries.
  - Add ProgressBar documentation links to the Feedback category entry.
