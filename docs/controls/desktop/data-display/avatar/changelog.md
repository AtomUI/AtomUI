# Avatar Changelog

本文档记录 Avatar 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Fix
  - 自定义 `Size` 派生的宽高、图标尺寸、`TextPadding` 与圆形圆角各自持有一个可替换的 Template 优先级值：反复改尺寸不再为同一属性累积值帧，`Size` 清除或形状不再是圆形时释放对应值；首次设置 `Size` 不再因 `SizeType` 同步回调重复配置。

## 2026-11-07

- API
  - `AbstractAvatar` 新增 `Stretch`（`StyledProperty<Stretch>`，默认 `Uniform`），模板中的 `ImagePresenter` 通过 `TemplateBinding` 消费。
- Fix
  - Circle 形状的圆角在 `Width`、`Size`、`SizeType` 变化时重新计算为 `Width / 2`；`Width` 非有限时跳过。此前圆角只在模板应用和 `Shape` 变化时写入，运行时改尺寸会留下过期圆角。
  - `Size` 在首次赋值之后再次变化时同步更新 `Width`、`Height` 和图标尺寸。此前 SizeType 已切到 Custom，后续 `Size` 变化不再触发尺寸配置，停留在首次取值。
  - `Size` 为 `NaN` 时尺寸配置直接返回，取代原先的 `Debug.Assert`。
  - `Stretch` 为 `Fill`/`UniformToFill` 时图片解码覆盖头像框，非正方形图片铺满时不再欠采样；运行时切换 `Stretch` 按新目标重新解码。
- Desktop 影响
  - `Stretch` 默认 `Uniform` 与此前 `Image` 的隐式默认值一致，渲染不变。
  - 仅在初始化时配置尺寸的用法不受影响；运行时改 `Size`、`SizeType` 的用法得到正确的宽高与圆角。
  - 默认 `Uniform` 的解码尺寸不变；设为 `Fill`/`UniformToFill` 的头像遇到非正方形图片时解码像素增大到覆盖头像框。
  - 已知边界：圆角和宽高由模板优先级写入，`Shape` 从 Circle 改回 Square、`Size` 恢复 `NaN` 时，模板优先级的值仍可能压过样式优先级的值，本次未改变。
- Tests
  - 新增 `AvatarStretchTests`：`Stretch` 默认值与模板绑定、Circle 圆角随 `Width` 与 `Size` 变化、随 `SizeType` 变化。
  - `AsyncImageDecodeSizeTests.Avatar_uniform_to_fill_decodes_to_cover_box`：4:1 图片在 48 px `UniformToFill` 头像中按 192 x 48 覆盖解码，切回 `Uniform` 后按 48 px 宽解码。

## 2026-09-10

- Behavior
  - Make the template layout the single owner of text measurement, downscaling and centering for short, boundary-width and long labels.
- Theme
  - Replace the manual TextBlock resize subscription and scale/translate transform with a Gap-constrained Viewbox while preserving `PART_TextPresenter`.
- Docs
  - Document that Avatar inherits the Application-scoped SVG conformance mode instead of exposing a per-control or per-request override.
  - Define the default `Compatible` behavior for safe local SVG resources with duplicate ids and the `Strict` `InvalidImageData` behavior.
  - Keep SVG security checks, resource budgets, reference-graph validation and renderer handoff owned by the shared image-loading pipeline.

## 2026-09-07

- Breaking API
  - Replace `ImageLoadSource` with the closed `ImageSource` hierarchy and concrete source constructors; no compatibility shim is retained.
  - Replace `ImageCacheMode` request semantics with orthogonal `ImageCacheReadPolicy` and `ImageCacheStoragePolicy` values.
- Loading
  - Adopt source-first validation and content-addressed decode reuse through the shared application loader, so a same-path replacement cannot be hidden by an old decoded-cache entry.
  - Keep Avatar reload as a one-request `RefreshSource` override without changing the caller-owned request options or application cache lifetime.
- Docs
  - Synchronize Avatar source, cache-policy, ownership and lifecycle documentation with the shared ImagePreviewer/image-loading architecture.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AvatarGroup as the semantic owner, with FlyoutHost used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-24

- Breaking API
  - Replace the old image source properties with `Source`, `FallbackSource` and `RequestOptions` based on `ImageLoadSource`.
  - Add `LoadState`, `LoadError`, `LoadProgress`, `IsLoading`, `IsLoaded`, `IsFailed`, `ImageOpened`, `ImageFailed` and `Reload()`.
  - Do not provide obsolete aliases, type forwarding or a per-control loader override.
- Ownership
  - Move `Avatar`, `AbstractAvatar`, `AvatarShape`, `AvatarToken` and `AvatarTheme` to `AtomUI.Controls`.
  - Keep `AvatarGroup` and `AvatarGroupTheme` in `AtomUI.Desktop.Controls` while consuming the shared Avatar Token.
- Behavior
  - Use the application-scoped `IImageLoader`, generation checks, 16 px physical decode buckets, fallback and result leases.
  - Keep the stable content priority `Image > Text > Icon`; loading and failure preserve available Text/Icon fallback content.
- Theme
  - Add `:loading`, `:loaded`, `:failed` and `:fallback` states.
  - Use Avalonia `TextBlock` for `PART_TextPresenter` so the common Avatar theme has no Desktop Controls dependency.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Avatar`.
  - Align generated output paths with `controls/avatar/index-cn.md` and `controls/avatar/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Avatar desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Avatar desktop architecture documentation under `docs/controls/desktop/data-display/avatar/overview.md`.
  - Add Avatar implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Avatar control-level changelog.
  - Add Avatar Token documentation covering AvatarToken.
