# Result Changelog

本文档记录 Result 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-07

- Breaking API（二进制不兼容）
  - `AbstractResult.Icon` 与 `IconProperty` 由 `PathIcon?` 放宽为 `object?`；内部的 `StatusIcon` 与 `StatusIconProperty` 同步放宽。赋值 `PathIcon` 的源码仍可编译；把读取到的 `Icon` 当作 `PathIcon` 使用的源码需要显式转换，引用旧签名的已编译程序集需要重新编译。迁移见 [6.2.14 迁移指南](../../../../releases/6.2.14-api-changes.zh-CN.md)。
- API
  - `ResultStatus` 末尾新增 `Waiting`（值 7；既有 0 至 6 不变）。
  - 新增 `private protected virtual object? CreateDefaultStatusIcon(ResultStatus status)`，供 AtomUI.Mobile 子类替换默认状态图标。基类映射不变：Info 为 `ExclamationCircleFilled`，Success 为 `CheckCircleFilled`，Warning 为 `WarningFilled`，Error 为 `CloseCircleFilled`，Waiting 为 `ClockCircleFilled`，403/404/500 返回 `null`；每次调用返回新实例。
  - 普通状态（Info、Success、Warning、Error、Waiting）的图标取 `Icon ?? CreateDefaultStatusIcon(Status)`；403/404/500 的图片分支不变。
- Theme
  - Desktop `ResultTheme.axaml` 为 `Waiting` 显示图标 presenter，图标画刷使用 `ResultSuccessIconColor`（`ColorSuccess`）。
- Desktop 影响
  - 二进制不兼容：`Icon` 的类型变化影响引用旧 `PathIcon?` 签名的已编译程序集；`StatusIcon` 为内部成员。
  - 既有七种状态的渲染不变（无头渲染对照中 Info、Success、Warning、Error、404 在浅色与深色下逐像素一致）；`Waiting` 为新增状态，显示绿色时钟图标。
- Tests
  - 新增 `ResultExtensionTests`（共享层）：`Waiting` 追加且不重排既有数值、默认图标工厂可重写、对象图标替换默认图标且清除后恢复、Desktop 既有图标映射不变。
  - 新增 `ResultIconContentTests`（Desktop）：五种状态的默认图标与尺寸、`Waiting` 使用成功色、对象图标替换与恢复。

## 2026-08-15

- Semantic Part
  - Publish `root`, `icon`, `title`, `subTitle`, `extra` and `body` for `Result` with five generated owner-scoped Style types.
  - Mark the two icon alternatives and four content presenters statically while keeping the default theme independent from `.semantic-*` selectors.
- API and Theme
  - Add `StrokeDashArray` to `AbstractResult` and project the standard root surface through `DashedBorder`.
  - Preserve all seven statuses, custom icons, optional content visibility and icon-presenter re-template subscription ownership.
- Fix
  - Recompute title and subtitle line heights when `HeaderFontSize` or `SubHeaderFontSize` changes after template application.
  - Stretch the `extra` Semantic Part across the Result content area while keeping its content centered, so area styles do not collapse around the action control.
- Gallery
  - Add the deferred Result Semantic Part preview with the six Ant Design 6.1.3 descriptions and the AtomUI `v6.1.3` Tag.
  - Reproduce the Ant Design `classNames Object` and `classNames Function` demo content and style values with owner-scoped Semantic Part styles.
- Tests
  - Cover descriptor metadata, six static marker targets, generated Style hits, root surface projection, status/content stability, re-template cleanup and lazy Gallery materialization.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Result`.
  - Align generated output paths with `controls/result/index-cn.md` and `controls/result/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Result desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Result desktop architecture documentation under `docs/controls/desktop/feedback/result/overview.md`.
  - Add Result implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Result control-level changelog.
  - Add Result Token documentation covering ResultToken.
