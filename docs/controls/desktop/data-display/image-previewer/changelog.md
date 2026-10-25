# ImagePreviewer Changelog

本文档记录 ImagePreviewer 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-24

- Fix：Full／Thumbnail的进度与结果提交保留所属UI dispatcher，避免迟到后台回调在应用隔离空窗重新创建全局UI dispatcher；保留原generation、取消及结果lease规则。

## 2026-10-21

- 修复快速连续点击旋转时图片角度倒转、来回摆动的问题：`Scale=1` 时不再向 `RenderTransform` 写入无意义的 Scale 操作，避免 Avalonia 的 TransformOperations 插值在动画重定向后退化为矩阵分解并归一化旋转角。

## 2026-10-17

- 新增 `IsWheelZoomEnabled=true`，只控制滚轮；统一滚轮刻度、指针锚点、拖拽平移、双击与捏合缩放，运行时缩放范围变化立即钳制比例和刷新按钮。
- 两种宿主使用相同的切图/reset 行为，工具栏适配按钮改为完整重置；viewer scene 明确裁剪图片，原始尺寸元数据稳定基础布局，实际预览窗口的视口与 DPI 驱动当前项解码升级。
- Gallery 基础示例提供 `ImageMinScale=0.1`、`ImageMaxScale=8` 与滚轮开关，覆盖四种页面语言。

- 建立预览变换专项设计：基础图片尺寸同时受视口宽度和高度 70% 约束，`Scale=1` 表示基础适配尺寸；`ImageViewer` 统一拥有缩放、滚轮、拖拽、旋转、翻转、reset 和边界修正，Dialog 与 OverlayHost 只负责宿主和操作转发；解码目标按基础尺寸、用户比例和 render scaling 升级，并继续受应用级图片预算约束。

## 2026-09-28

- 为 internal `ImagePreviewerTitleBar` 补齐与 native `WindowTitleBar` 一致的 Browser 平台排除声明，使生成的主题资产与所需 WindowTitleBar Token schema 具有一致可用域；Browser 继续使用既有 overlay 宿主。

## 2026-09-09

- 移除 `popup.close` 语义部件：关闭职能在 native dialog 由 OS 标题栏按钮、在 overlay 宿主由内嵌关闭按钮（`PART_CloseButton`，非语义部件）承担，发布部件 9 → 8；同步移除生成 `ImagePreviewerPopupCloseStyle` / `ImageGroupPreviewerPopupCloseStyle`、overlay 模板 marker 与示例描述。
- 语义预览支持独立预览窗口：`AbstractImagePreviewer` 实现 `AtomUI.Theme.ISemanticPartCrossRootProvider`，
  把存活的 native 预览对话框窗口 / overlay 宿主上报为跨根宿主，并在 `DialogOpened`、`RootTemplateApplied`（对话框表面
  模板就绪）与 `DialogClosed` 位点触发 `CrossRootsChanged`；Gallery 语义部件页签在预览窗口打开时即可对窗口内的
  `popup.body` / `popup.footer` 等部件定位描边与显现，关闭窗口后自动释放。该契约为通用机制，窗口宿主家族控件
  （如 Dialog/WindowDialogPresenter）可直接复用。

## 2026-09-08

- Semantic Part style alignment（严格对齐上游 Image 语义 DOM 样式）
  - 预览操作图标颜色：`PreviewOperationColor` 0.85 → 0.65、`PreviewOperationHoverColor` 1.0 → 0.85、`PreviewOperationColorDisabled` 由 `ColorTextDisabled` → `ColorTextLightSolid`@0.25（与上游 `previewOperationColor` / `previewOperationHoverColor` / `previewOperationColorDisabled` 一致）。
  - `popup.close` 与切换按钮图标改为基础 `ColorTextLightSolid`，背景 0.1（`NavButtonBgColor`）悬浮至 0.2（`NavButtonBgHoverColor`），移除图标悬浮变色；关闭按钮外边距 `MarginLG` → `MarginSM`（对齐上游 `top` / `inset-inline-end: marginSM`）。
  - `popup.actions` 胶囊背景 0.2 → 0.1，水平内间距 `FloatToolbarPadding` 由 `paddingLG/2` → `paddingLG`；`popup.footer` 底部偏移 `MarginLG` → `MarginXL`，页码指示与操作组间距 `SpacingXS` → `Spacing`（对齐上游 `bottom: marginXL` / `gap: margin`）。
  - `popup.mask` 由硬编码 `#E6000000` 改为共享 `ColorBgMask`（对齐上游 `colorBgMask`）。
- Behavior
  - `cover` 遮罩几何对齐上游 `genImageCoverStyle` 的 `position:absolute; inset:0` cover：遮罩不再被 owner `Padding` 缩进到图片区，而是经负 Margin（`OwnerPadding` + `OwnerBorderThickness` 之和的负值，owner 模板经 `TemplateBinding` / `RelativeSource` 中继）铺满整个 owner root（含 padding 环与边框）。hover 时 30% 黑色遮罩压暗 padding 环（白色 padding 视觉上变成约 178 灰的一圈"粗框"）是上游固有视觉，单封面与组封面一致复现。
  - 修复遮罩 padding 环被裁剪不显示的问题：移除 `ImagePreviewerCoverTheme` 的 `ClipToBounds=True` Setter，并在 `ImagePreviewerCover` 静态构造 `ClipToBoundsProperty.OverrideDefaultValue(false)`（Avalonia `TemplatedControl` 类级默认值为 `true`，属合成层裁剪，会把负 Margin 遮罩裁回 cover 内区且不体现在布局 Bounds 上）；`#Mask` 与 cover 模板 border/loading/error presenter 的圆角改经 `OwnerCornerRadius` 中继直接跟随 owner `CornerRadius`——遮罩以负 Margin 越过 owner padding，无法被 owner 的圆角裁剪覆盖，圆角需直接涂在遮罩上以对齐上游 root `overflow:hidden + border-radius` 的视觉效果。
  - `image` 部件新增封面图片圆角能力：`ImagePreviewRenderer` 暴露 `CornerRadius`（AddOwner `Border.CornerRadiusProperty`）并把 `RoundRectGeometryBuilder` WinUI 关键点圆角几何（与 `DashedBorder.ClipContentToCornerRadius` 同算法）设到子 `Image` 的 `Clip` 属性——渲染管线遍历每个 Visual 时应用其 `Clip`，Image 只渲染一次且带裁剪；不得在 `Render` override 里 `PushGeometryClip` 包着 `image.Render` 手绘（子 Image 是 VisualChild，渲染器在父 `Render` 后还会独立遍历 VisualChildren 再画一遍无裁剪的 Image，覆盖手绘结果）。内置主题不设默认值（对齐上游默认 image 无圆角），经生成 `ImagePreviewerImageStyle` 由用户 Semantic Style 定制（`x:SetterTargetType="atom:ImagePreviewRenderer"`，Setter 属性名必须写限定名 `Property="Border.CornerRadius"`，否则经 internal 渲染器类型字段解析在运行时抛 `FieldAccessException`）。
- Gallery
  - 「自定义语义结构的样式」示例对齐上游 `style-class.tsx`：并排两个 160 宽 `ImagePreviewer`，`root` 经 owner 属性表达
    padding 4 + 圆角 8 + 裁剪，右侧再加常驻 2px `#A594F9` 边框（经上游三张截图逐像素核对，边框在常态与 hover 态均常驻，
    hover 变化的是 `cover` 遮罩 0.3 淡入）；`image` 的 `borderRadius: 4` 经生成 `ImagePreviewerImageStyle` 落到
    `ImagePreviewRenderer.CornerRadius`；右侧 `filter: grayscale(50%)` 因无等价属性，仍通过 ViewModel 以
    SkiaSharp 颜色矩阵去饱和图像源复现；`cover` /
    `popup.mask` 上游未覆盖，均保持默认视觉，移除先前 0.8 透明度、0.13 遮罩等偏离上游的覆盖。

## 2026-09-07

- Semantic Part
  - 在 `ImagePreviewer` 与 `ImageGroupPreviewer` 上发布 9 个 Semantic Part（`root`/`image`/`cover`/`popup.root`/`popup.mask`/`popup.body`/`popup.footer`/`popup.actions`/`popup.close`），与上游 Image 语义 DOM 对齐；生成 owner 作用域强类型 Semantic Style（`ImagePreviewer<Part>Style` / `ImageGroupPreviewer<Part>Style`）。
  - 在六个内置主题中补充静态 `.semantic-*` marker 与 `.semantic-scope-*` 路由锚点；默认视觉不使用 `.semantic-*` selector。
  - `popup.*` 统一声明 `CrossVisualRoot` + `RuntimeCreated`；`popup.mask` 与 `popup.close` 仅 overlay 宿主存在（`Optional`）。
- Hosts and theme
  - 在 native dialog 内容根包裹 Panel 上注入 `popup.root` marker；overlay 宿主模板根改为纯 `popup.root` 容器，遮罩背景迁移为独立的 `popup.mask` 子元素，并新增 `popup.close` 关闭按钮。
  - native dialog 保持独立 `Window`/TopLevel：owner 作用域 Semantic Style 仅在 overlay 宿主（与 owner 同 TopLevel）命中，dialog 内预览视觉继续经 host 契约（owner 属性/Token 中继与 App 级 `ImageViewer` 主题）定制。
- Gallery
  - ImagePreviewer ShowCase 迁移到 `GalleryShowCaseHost`：Semantic Preview 列出全部 9 个 Part 并新增「自定义语义结构的样式」示例；`root`/`image`/`cover` 在 owner 模板内高亮，`popup.*` 因宿主 internal 且无公开访问器仅列出描述。
- Design
  - Define the formal ImagePreviewer loading architecture around `ImageSourceKey -> ImageSourceVersion -> ImageContentId -> ImageDecodeKey`, so source addresses no longer act as content identities.
  - Separate source snapshots, validated encoded content and decoded content into application-owned stores while keeping Previewer collection and cache lifecycles independent.
  - Define source-first validation, two-level single-flight, monotonic source commit generations, waiter-local cancellation, cache epochs and result leases as the shared loading invariants.
  - Define the closed `ImageSource` hierarchy and `ImageFileValidationMode` as the public source contract without compatibility shims.
  - Separate the public cache contract into `ImageCacheReadPolicy` and `ImageCacheStoragePolicy`; ordinary loads default to `ValidateSource`, while Reload uses a per-request `RefreshSource` override.
  - Separate `ImageLoadOrigin` from `ImageSourceValidation` so cache location and source validation are not conflated.
- Behavior
  - Restore strict `Immediate` switching semantics for both the preview surface and the single cover: a target without a displayable image clears the previous frame and reports loading in the same UI update cycle, including the Idle interval before its request starts. Remove the retained-frame grace period and delayed callback from this mode.
  - Keep `WaitForLoaded` as the only retained-frame mode and scope target markers to one tracker session, preventing late older requests, never-targeted preloads and stale markers from a closed host session from replacing the current visible frame.
  - Recompute preview and cover display state immediately when `ImageSwitchMode` changes at runtime.
  - Bind the Gallery rapid-switch cover and preview to the same rolling index so the demonstration compares the two display modes without targeting different items.
- Breaking API
  - Replace `ImageLoadSource` and its factories with the closed `ImageSource` hierarchy and concrete source constructors.
  - Replace `ImageCacheMode` with orthogonal `ImageCacheReadPolicy` and `ImageCacheStoragePolicy` properties.
  - Replace `ImageCacheSource` diagnostics with independent `ImageLoadOrigin`, `ImageSourceValidation` and `ContentId` result fields.
- Implementation
  - Make every ordinary load resolve or validate the source snapshot before constructing `ImageDecodeKey`; remove the SourceKey-based decoded fast path that could return stale images after same-path replacement.
  - Store validated encoded bytes and decoded entries by SHA-256 `ImageContentId`, allowing different sources with identical content to share work without conflating source freshness.
  - Add source commit generations, cache epochs and persistent-write serialization so late source/decode operations cannot roll back a newer snapshot or repopulate a cleared scope.
  - Replace the persistent layout with stable `image-cache/` content and source stores; keep `formatRevision` internal and rebuild incompatible layouts without migration shims.
  - Keep `CacheStorage=None` as a strict no-write policy even on persistent hits, and invalidate a source's prior memory/disk variant when HTTP changes to `no-store` without allowing late responses to erase newer generations.
  - Keep ImagePreviewer Full and Thumbnail generations, cancellation sources, progress, states and result leases independent; collection and host lifecycle never clear the application cache.
  - Propagate same-bucket Preload-to-Critical upgrades through a shared mutable waiter-priority state so queued source and decode work is reprioritized without canceling or restarting the request.
  - Detach Full and Thumbnail results, publish a consistent image/state transition, and only then release old leases during unload, failed replacement and entry disposal, preventing a host from observing an already released image or a failed state paired with the previous frame.
- Tests
  - Add regressions for same-path file replacement, identical content across different sources, metadata versus content-hash file validation, cache read/storage policies and persistent content/source separation.
  - Add an ImagePreviewer integration regression proving that clear-and-readd of the same file path renders the replacement without a manual cache clear.
  - Add regressions for Immediate loading before request start, continuous rapid switching, runtime mode changes, monotonic WaitForLoaded fallback selection, atomic failed-state publication, display cleanup before lease release, queued priority promotion and synchronized Gallery cover/current bindings.
- Docs
  - Rewrite the formal ImagePreviewer architecture document with the content-addressed cache model, persistent `image-cache/` naming, source validation matrix, Previewer collection ownership and verification contract.
  - Synchronize the switching design and implementation documents with strict Immediate loading semantics, WaitForLoaded-only retention and the no-timer resource boundary.
  - 新增 `semantic-part.md`，并在 overview/implementation 中固化为 overlay 命中、native dialog 跨 TopLevel 边界与 Gallery 解析边界。

## 2026-09-03

- API
  - Add `ImageSwitchMode` (`Immediate`, `WaitForLoaded`) and `AbstractImagePreviewer.ImageSwitchMode` (default `Immediate`) to select when the loading placeholder replaces the previous image while the target item switches.
- Behavior
  - Unify display continuity for both preview surface and cover: while the target has no image yet (including the Idle transient right after a switch) the previously loaded image stays visible; `Immediate` falls back to the loading placeholder only after a 300 ms grace period, `WaitForLoaded` keeps it indefinitely. High-frequency add-and-track-latest switching no longer produces blank-frame or spinner strobing in either mode.
  - Feed the retained frame from two levels: the current item when it completes, and the latest completed full load across entries when the current item is superseded before finishing (switching faster than loading).
  - Keep `ImageOpened` / `ImageFailed` timing bound to the target entry state machine; cancellations never produce failure events.
- Loading pipeline
  - Classify shared-operation internal cancellations (racing waiter teardown, `ClearCache(CancelInFlight)`) as cancellations instead of `InvalidSource` source failures in `ImageLoader`.
  - Resolve cancellations benignly in `ImagePreviewEntry`: keep a previously committed image or return to Idle, without a Failed commit; the non-fatal fallback no longer swallows `OperationCanceledException`.
  - Broadcast cached Full/Thumbnail reset notifications from `ImagePreviewEntry.Dispose()` before clearing subscribers so display holders drop references to released bitmaps.
  - Adopt in-flight requests on same-bucket priority upgrades (Preload→Critical) instead of restarting them, so requests can complete when switching outpaces loading.
- Hosts and theme
  - Introduce the internal `ImagePreviewDisplayTracker` as the single display state machine shared by `ImagePreviewerDialog` and `ImagePreviewerOverlayHost`, replacing their duplicated current-entry tracking.
  - Make both hosts observe effective-collection incremental changes so the display target follows entry replacement even when `CurrentIndex` keeps the same value (capped-list trimming no longer leaves a stale, eventually disposed entry displayed as a blank preview).
  - Gate the viewer loading presenter with the `:loading:not(:has-image)` selector and make the cover hover mask depend only on `IsShowCoverMask`, decoupling both from raw load-state flicker.
- Docs
  - Add the switching display design document covering the display matrix, retained-frame lifecycle with grace and seeding, host collection following, subscription pairing, pipeline cancellation delivery and resource bounds; link it from the overview and implementation docs.

## 2026-08-24

- Breaking API
  - Replace the single/multiple source properties and preview-specific source interfaces with `ItemsSource: IEnumerable<ImagePreviewItem>?`.
  - Make `ImagePreviewItem` an immutable configuration record using `ImageLoadSource`, optional `ThumbnailSource`, per-item `FallbackSource`, `RequestOptions`, `Title` and `Tag`.
  - Remove the control-private loader, loaded result types, scheduler and `MaxConcurrentLoads`; do not provide compatibility shims.
  - Add current and cover load state projections, `ImageOpened`/`ImageFailed`, `ReloadCurrent()`, `ReloadItem(index)` and `ReloadCover()`.
- Loading
  - Route current, cover and neighbor requests through the application-scoped `IImageLoader` with Critical, High and Preload priorities.
  - Keep Full and Thumbnail generations, cancellation and result leases isolated in internal `ImagePreviewEntry` instances.
  - Apply fallback per item and per channel; one failed item never replaces the collection.
  - Make Full and Thumbnail requests size-aware: re-decode when the 16 px physical bucket grows after real layout, resize or DPI change, keep
    the previous lease during replacement, and deduplicate repeated requests for the same bucket.
- Collections and lifecycle
  - Support enumerable replacement and observable Add, Remove, Move, Replace and Reset while reusing unchanged entries.
  - Re-materialize on reattach, release Full leases when the preview host closes, and release Full/Thumbnail leases on detach.
- Titles
  - Resolve titles in the order explicit preview/window title, item title, resolver and empty state.

## 2026-07-23

- Behavior
  - Make preview dialog title alignment default to `WindowCenter` on Windows, Linux and macOS while preserving explicit `TitleAlignment` projection to `ImagePreviewerTitleBar`.
  - Paint the dialog image viewer with the dialog background so Windows CSD maximize/restore frames do not expose the generic window underlay.
  - Suppress preview image transform transitions during dialog window state/size transitions so maximize/restore does not combine outer window resizing with inner image translate animation.
- Docs
  - Document the preview dialog title alignment invariant and its boundary with the shared `WindowTitleBarLayoutPanel` algorithm.
  - Document the dialog-only viewer background and transform suppression boundary for Windows CSD state transitions.

## 2026-07-08

- API
  - Define `IImagePreviewSource` as the unified image source contract, with `UriImagePreviewSource` for URI inputs and `StreamImagePreviewSource` for lazy stream inputs.
  - Keep source identity out of the main `IImagePreviewSource` contract; add optional `IImagePreviewSourceIdentity` for advanced source reuse.
  - Replace the old URI property family with `Source`, `Sources` and `FallbackSource`; URI inputs now use `UriImagePreviewSource` explicitly.
  - Replace the independent `CoverSourceUri` cover model with `CoverIndex`, keeping cover selection inside the same `Source` / `Sources` source set.
  - Add `MaxConcurrentLoads` and `PreloadCount` as the public loading scheduler contract.
- Behavior
  - Load only the closed-state cover for single `ImagePreviewer`, load default `ImageGroupPreviewer` cover items through the shared scheduler, and load only `CurrentIndex`, `CoverIndex` and the `PreloadCount` neighbor window while preview is open.
  - Preserve loaded cover items when opening preview from an unchanged `Sources` collection, and keep the cover item in the open-state scheduler window so non-modal preview navigation does not blank the page cover.
  - Keep `CoverIndex` display-only: changing cover, loading cover, failing cover or opening preview no longer writes `CurrentIndex`.
  - Preserve `CurrentIndex` when dialog or overlay `ItemsSource` changes; hosts clamp only for display and do not write the public/bound value back to `0`.
  - Treat fallback as a source batch result: single item failure stays local, mixed batches keep loaded items and skip failed items, and `FallbackSource` is used only after all source items in the batch have failed.
- Implementation
  - Keep URI and stream inputs on one loading path: source materialization creates lightweight items, scheduled loads call `OpenReadAsync`, and all results flow through `ImagePreviewLoadScheduler`.
  - Resolve source identity internally: URI sources use `ImageSourceUri.CacheKey`, stream sources use optional identity, and sources without identity use object reference identity.
  - Require stream sources to return a fresh readable stream for each load; ImagePreviewer owns returned stream disposal.
  - Route cover, current image, neighbor preload and fallback loads through `ImagePreviewLoadScheduler`, including priority, generation checks, bounded concurrency and cancellation.
  - Keep canceled running loads counted until the loader actually settles, so `MaxConcurrentLoads` is not exceeded during rapid current-window changes.
- Docs
  - Document the unified source model and default source implementations, including `new UriImagePreviewSource("https://example.com/image.png")` for ordinary web images and `StreamImagePreviewSource` for authenticated, database, encrypted or memory-backed images.
  - Clarify that source objects do not carry cover, fallback, title metadata or business metadata; those concerns remain separate public contracts.
  - Define the lazy loading redesign for large image sets: closed preview loads only the `CoverIndex` cover, open preview loads `CurrentIndex`, `CoverIndex` and the `PreloadCount` neighbor window, and all load paths share `MaxConcurrentLoads`.
  - Document `CoverIndex` as a display-only cover selector that is decoupled from `CurrentIndex`; clicking the cover opens preview without synchronizing the current preview index.
  - Keep the public source model lightweight by using `IImagePreviewSource` only for lazy stream opening, with optional identity extension separated from cover, current item, fallback and scheduler semantics.
  - Add the internal `ImagePreviewLoadScheduler` responsibility for priority, bounded concurrency, generation checks, cancellation, stale result disposal and lazy result writeback.
  - Define source collection fallback as a batch-level decision: individual failed items must not replace the whole preview list with `FallbackSource`; fallback is used only when the current `Source` / `Sources` batch fully fails.
  - Document that mixed success/failure batches keep loaded items, skip failed items, and prevent stale batches from writing back after source replacement or cancellation.

## 2026-07-06

- API
  - Make `IsOpen` and `CurrentIndex` default to `BindingMode.TwoWay` so controlled preview open state and current image index update the bound ViewModel without explicit binding mode.
- Implementation
  - Keep dialog and overlay `CurrentIndex` relay bindings `TwoWay` and owned by host disposables.
- Docs
  - Document the controlled state contract and clarify that these properties are not Form validation values.

## 2026-07-01

- API
  - Add `PreviewTitleIcon` for configuring the optional `PathIcon` displayed before the preview dialog title.
- Theme
  - Add `PART_IconPresenter` to the preview title group so an explicitly configured icon appears before the preview title with the standard logo-title spacing.
  - Keep Windows and Linux caption buttons outside the centered title area, and rely on macOS title bar offset handling instead of inheriting a window logo.
- Docs
  - Document the `PathIcon` based preview title icon model and the `PART_TitleLayout` / `PART_IconPresenter` title group contract.
  - Align ImagePreviewer docs with the control documentation structure by adding the composition model and making semantic parts and template parts source-specific.
  - Define the remote image loading UX contract: inline cover loading uses an image Skeleton, loading/error placeholders keep a stable cover size, default failure text is localized, and custom `LoadingContent` / `ErrorContent` only replaces presenter content.
  - Document that single `ImagePreviewer` and `ImageGroupPreviewer` must consume `CoverWidth` / `CoverHeight` consistently so failed remote images do not collapse to text-height strips.

## 2026-06-30

- Behavior
  - Make inline cover selection follow `CurrentIndex` when `CoverSourceUri` is not set, keeping dialog and overlay preview on the same current item contract.
  - Resolve preview dialog title from explicit title first and current image file name second.
- API
  - Add `PreviewTitle`, `PreviewTitleResolver`, `IImagePreviewTitleResolver`, `ImagePreviewTitleResolveContext` and `DefaultImagePreviewTitleResolver`.
- Docs
  - Document the redesigned `ImageSourceUri` image source contract for local, asset and remote image loading.
  - Add the `ImagePreviewItem` loading state model, default Skeleton/Spin loading behavior, fallback handling and renderer layout invalidation requirements.
  - Define `IImageSourceLoader` responsibilities for asynchronous loading, cancellation, source identity and stale result prevention.
  - Define `CurrentIndex` as the shared current item contract for inline cover, dialog and overlay preview, with `CoverSourceUri` kept as the explicit cover override.
  - Define the preview title resolution model: non-empty `Window.Title` or `PreviewTitle` wins; otherwise the default resolver derives a title from the current image source file name.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ImagePreviewer`.
  - Align generated output paths with `controls/image-previewer/index-cn.md` and `controls/image-previewer/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete ImagePreviewer desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish ImagePreviewer desktop architecture documentation under `docs/controls/desktop/data-display/image-previewer/overview.md`.
  - Add ImagePreviewer implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add ImagePreviewer control-level changelog.
  - Add ImagePreviewer Token documentation covering ImagePreviewerToken.
