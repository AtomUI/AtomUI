# Upload Changelog

本文档记录 Upload 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-30

- 接通 Upload、UploadTrigger、UploadDropZone、UploadDefaultDropArea 的有效 Global 字体，保留默认父继承与语义字号；正文前景进入实际文字。
- 内部上传状态资产显式读取 Upload Global；圆形默认几何改用主题声明，修复配置清除后变方和形态往返遗留。

- 公开表面Global接入各自注册identity，保留既有Own组合语义、模板、状态与原生样式优先级。

- 十三个Own默认采用DependencyDefaultsV2 rev1，保留名称/类型和资源ID/slot、精确公式及常量；不改变上传任务或视觉状态。

## Unreleased

- Design
  - Define the cross-platform drag-and-drop contract in `drag-drop-design.md`.
  - Establish `UploadDropZone` as the sole DragDrop owner and `UploadDefaultDropArea` as a visual-only control.
  - Lock the existing DropZone/DropArea ControlTheme, layout, Token mapping and rendered result as compatibility invariants.
  - Define typed StorageItem/source ownership, validated transfers and full cleanup aggregation for every input batch.
- API
  - Publish the `root`, `list` and runtime-created `item` Semantic Part contract for `Upload`, including generated `UploadListStyle` and `UploadItemStyle` selectors.
  - Remove `Upload.IsOpenFileDialogOnClick`; click-to-select ownership moves to `UploadDropZone.IsOpenFileDialogOnClick` and `UploadDropZone.SourceKind`.
  - Remove `Upload.Accepts`; replace it with `Upload.AllowedFileTypes` based on `FilePickerFileType`.
  - Add `Upload.CountOverflowBehavior`, `Upload.AdmissionPolicy` and `Upload.InputBatchCompleted` for deterministic batch admission and diagnostics.
  - Add `UploadRejectionReason.MultipleSelectionNotAllowed` for excess top-level user input items.
  - Replace `IsCancelled` and synthetic batch rejection values with `UploadInputBatchStatus` and `UploadInputFailureReason`.
  - Remove raw exceptions from `UploadRejectedItem`, consolidate storage-read failures and distinguish policy rejection from policy execution failure.
  - Replace directly constructed `UploadAdmissionDecision` records with `Accept()` and `Reject(...)` factories.
  - Remove `UploadDefaultDropArea.FilesDropped`, `FilesDroppedEvent` and `UploadFilesDroppedEventArgs`; `UploadDropZone` is the only drag data entry.
  - Replace URI-only `UploadFileInfo` with immutable nullable metadata plus required `IUploadFileSource` stream access.
  - Add `UploadDropZone` directory policies, traversal limits and read-only drag/drop processing state.
  - Redesign Upload around `Files` as the single state owner.
  - Replace directory mode with composable `UploadTrigger.SourceKind`.
- Theme
  - Add static Semantic Part markers to both Upload list templates and runtime item markers to every `UploadList` container while excluding the picture append trigger.
  - Move list scrolling into `UploadList`.
  - Replace picture trigger fake task with append trigger content.
- Behavior
  - Require explicit Copy/None negotiation, Drop-only data materialization and one shared picker/drop/programmatic input pipeline.
  - Run enumeration, metadata and admission inside one non-UI worker boundary per batch, then evaluate count capacity at the UI commit boundary.
  - Replace generic disposable ownership, boolean lifecycle switches and transfer callbacks with typed batch operations and named replacement paths.
  - Commit `ReplaceExisting` before asynchronously cancelling removed queue entries while retaining their sources until execution exits.
  - Define directory traversal, per-item rejection, cancellation and StorageItem lease ownership.
  - Serialize input batches so type admission, business policy and count overflow commits do not overlap.
  - Delay accepted source lease release until the corresponding upload execution exits for remove/reset, external collection changes, `Files` replacement, Form Set/Clear and detach paths.
  - Observe picker, DropZone and Upload lifecycle tasks through named cancellation/error handlers instead of `async void` or nested dispatcher async delegates.
  - Make scheduler cancellation wait for the transport execution task to finish before reporting completion.
  - Serialize scheduler maintenance operations and preserve pending/running task ownership when cancellation is requested during cleanup.
  - Continue cancellation and source lease release when collection, Form, batch-completion or task callbacks throw, then propagate the cleanup error.
  - Add configurable success auto-remove and pending text.
  - Unify file picker, directory picker and Drop top-level item counts under `Upload.IsMultipleEnabled`; keep directory expansion and explicit programmatic batches independent.
- Gallery
  - Add the Upload Semantic Part preview, generated Semantic Style example and root/list/item highlight coverage.
- Docs
  - Add `semantic-part.md` and synchronize the Upload overview and implementation documentation with the public Semantic Part contract.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Upload`.
  - Align generated output paths with `controls/upload/index-cn.md` and `controls/upload/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Upload desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Upload desktop architecture documentation under `docs/controls/desktop/data-entry/upload/overview.md`.
  - Add Upload implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Upload control-level changelog.
  - Add Upload Token documentation covering UploadToken.
