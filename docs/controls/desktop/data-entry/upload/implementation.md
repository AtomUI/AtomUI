# Upload 桌面版实现原理

本文档定义 Upload 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Upload 桌面版架构设计](overview.md)，公开主题区域见 [Upload Semantic Part 契约](semantic-part.md)，拖动输入的专项状态与平台边界见 [Upload 拖动上传设计](drag-drop-design.md)，变化记录见 [Upload Changelog](changelog.md)，控件视觉变量见 [Upload Token 设计](token.md)。

## 1. 实现定位

Upload 的实现定位是上传状态协调器，而不是固定上传按钮、拖拽框或列表模板的集合。根控件只拥有文件状态、上传队列、Form 值投影和生命周期释放；`UploadTrigger`、`UploadDropZone`、`UploadList` 只作为可组合动作入口或视图层协作对象。

本文档只记录维护者必须遵守的稳定实现结构和不变量。具体属性注册、AXAML selector、ControlTheme 节点和列表项视觉细节仍以源码为准。

## 2. 源码文件结构

源码结构按“状态 owner、输入入口、输入管线、队列协调、列表视图、主题模板”分层。

| 路径 | 职责 |
| --- | --- |
| `src/AtomUI.Desktop.Controls/Upload/Upload.cs` | 保留 public/protected API、Avalonia 属性注册、构造、display source 桥接、`IFormItemAware` 和顶层上传协调。 |
| `src/AtomUI.Desktop.Controls/Upload/Upload.SemanticParts.cs` | 声明 `list` / `item` 公共 Semantic Part，并驱动 descriptor、生成 Style、class 常量和静态注册。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadAppendContentItem.cs` | internal picture append visual slot，用于把 `TriggerContent` 放入 PictureCard/PictureCircle 的同一 wrap flow；不进入 `Files`。 |
| `src/AtomUI.Desktop.Controls/Upload/Upload.FileSelection.cs` | 封装文件选择和目录选择动作，并把同一个 `IsMultipleEnabled` 值投射到两种 picker 的 `AllowMultiple`。 |
| `src/AtomUI.Desktop.Controls/Upload/IUploadStorageProviderAdapter.cs` | 隔离 Avalonia storage picker 调用，向文件选择和目录选择提供可测试的 typed StorageItem 边界。 |
| `src/AtomUI.Desktop.Controls/Upload/Upload.InputPipeline.cs` | 连接 `Upload` 状态 owner 与输入管线，负责 UI 线程 option 快照、typed commit、批次事件和 ReplaceExisting source handoff。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadInputPipeline.cs` | 串行执行 picker、directory、drop 和 programmatic 批次，在一次 worker 边界内协调遍历与准入，再把数量策略和集合变更交给 UI commit。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadInputBatchOperation.cs` | 分别持有单个批次的 typed StorageItem 与 source lease ownership，验证 transfer，并在完成事件前释放全部未移交资源。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadStorageItemEnumerator.cs` | 在目录展开前应用顶层多选限制，再仅使用 `IStorageFolder.GetItemsAsync()` 按稳定顺序展开 storage items，并隔离目录分支错误。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadInputCandidate.cs` | 非 owning 地引用批次已拥有的 storage file，并提供基础元数据读取入口。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadFileAdmissionService.cs` | 在非 UI 执行上下文执行 `AllowedFileTypes` 和 `AdmissionPolicy`，生成明确的接受或拒绝结果。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadStorageFileSource.cs` | 以 `IStorageFile` lease 实现 `IUploadFileSource`，允许 transport 在 lease 有效期内打开读取流。 |
| `src/AtomUI.Desktop.Controls/Upload/Upload.AutoRemove.cs` | 管理成功自动移除的延迟任务、取消和生命周期释放。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadFileItem.cs` | public 文件状态模型，承载文件元数据、状态、进度、错误、结果和 pending 文案。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadSourceKind.cs` | public 触发来源枚举，区分文件选择和目录选择。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadFileValueMode.cs` | public Form 值投影枚举，控制提交全部文件、成功文件或上传结果。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadTrigger.cs` | public 可组合触发器，查找最近的 `Upload` 并触发文件或目录选择。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadDropZone.cs` | public 可组合拖拽区，拥有 DragDrop 协商、状态和 Drop 快照。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadDefaultDropArea.cs` | public 默认拖动视觉，只保存图标、标题、副标题和动效属性。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadQueue.cs` | internal 上传队列协调器，映射 `UploadFileItem` 与 `FileUploadTask`。 |
| `src/AtomUI.Desktop.Controls/Upload/UploadList.cs` | 文件列表视图，渲染 effective 文件视图并维护列表滚动。 |
| `src/AtomUI.Desktop.Controls/Upload/AbstractUploadListItem.cs` | 列表项基类，只绑定 `UploadFileItem` 并发出 item action，不拥有任务状态。 |
| `src/AtomUI.Controls.Shared/Net/FileUploadScheduler.cs` | 共享上传调度器，跟踪 pending/running 任务、并发和取消。 |
| `src/AtomUI.Controls.Shared/Net/FileUploadTask.cs` | 共享上传任务对象，承载传输回调和 cancellation 状态。 |
| `src/AtomUI.Controls.Shared/Net/IUploadFileSource.cs` | public 文件内容读取契约，不要求本地路径。 |
| `src/AtomUI.Controls.Shared/Net/IUploadFileSourceLease.cs` | internal AtomUI-owned source lease 标记，只由接受该 lease 的 owner 释放。 |

主题结构：

| 路径 | 职责 |
| --- | --- |
| `src/AtomUI.Desktop.Controls/Upload/Themes/UploadTheme.axaml` | 根模板，连接 `TriggerContent`、list 和 picture display source，保持触发区与列表的稳定间距，并在两套模板变体上声明 `semantic-list`。 |
| `src/AtomUI.Desktop.Controls/Upload/Themes/UploadTriggerTheme.axaml` | 触发器 shell，只承载用户内容和点击表面。 |
| `src/AtomUI.Desktop.Controls/Upload/Themes/UploadDropZoneTheme.axaml` | 拖拽行为 shell，只承载用户内容和内容对齐。 |
| `src/AtomUI.Desktop.Controls/Upload/Themes/UploadDefaultDropAreaTheme.axaml` | 默认拖动视觉，保持 Frame、内容 presenter、Token 和动效结构。 |
| `src/AtomUI.Desktop.Controls/Upload/Themes/UploadListTheme.axaml` | 列表 shell，内部拥有自动隐藏的 `atom:ScrollViewer` 和滚动边界。 |
| `src/AtomUI.Desktop.Controls/Upload/Themes/PictureList/*` | Picture 列表项视觉，pending 内容优先读取 `UploadFileItem.PendingText`。 |
| `src/AtomUI.Desktop.Controls/Upload/Themes/PictureShapeList/*` | PictureCard/PictureCircle 列表布局和 item 视觉；append slot 由 display source 承载并进入同一 wrap flow。 |

## 3. 核心类职责

| 类型 | 稳定职责 | 禁止职责 |
| --- | --- | --- |
| `Upload` | `Files` 状态 owner、上传协调、Form 适配、生命周期释放、trigger/drop/list 组合入口。 | 不创建第二份任务列表，不把 trigger 伪造成文件项。 |
| `UploadFileItem` | public 文件状态模型，保存文件元数据、上传状态、进度、错误、结果和自定义数据。 | 不持有视觉控件、队列、transport 或 owner 控件。 |
| `UploadQueue` | 管理 `UploadFileItem.Id` 与 `FileUploadTask` 的映射，转发 scheduler 状态到文件项。 | 不操作模板 part、列表容器或 Gallery 视图。 |
| `UploadTrigger` | 根据 `SourceKind` 调用最近 `Upload` 的文件或目录选择方法。 | 不保存文件集合，不直接打开业务上传 transport。 |
| `UploadDropZone` | 处理 DragDrop 路由、Copy/None 协商、Drop 快照和输入批次创建。 | 不读取文件元数据、枚举目录或维护任务状态。 |
| `UploadDefaultDropArea` | 通过既有 ControlTheme 渲染默认拖动界面。 | 不设置 DropTarget、不读取 DataTransfer、不查找 Upload owner。 |
| `UploadInputPipeline` | 统一处理 picker、directory、drop 和 programmatic 输入；只对 StorageItem 用户输入应用顶层多选限制。 | 不渲染控件，不执行网络传输。 |
| `UploadList` | 渲染 effective 文件视图，维护 `ListMaxHeight` 和 `ListScrollBarVisibility`；Picture shape 派生列表可追加 display-only visual slot。 | 不创建、删除、隐藏真实上传文件状态。 |
| `AbstractUploadListItem` | 从 `UploadFileItem` 投射 item 视觉状态并发出 remove/preview 等动作请求。 | 不反向持有 scheduler task 或复制 item 状态。 |
| `FileUploadScheduler` | 共享并发调度、pending/running 跟踪、取消和 transport 替换。 | 不引用 `Upload`、`UploadList` 或其他视觉控件。 |

## 4. 状态与数据流

Upload state flows through one path:

```text
Public API / Trigger / DropZone
  -> UploadInputPipeline
  -> StorageItem enumeration / metadata / admission
  -> Upload UI count policy / typed commit
  -> Files collection
  -> UploadQueue/FileUploadScheduler
  -> UploadFileItem.Status/Progress/Result
  -> UploadList item containers
```

实现中只能有一个文件状态 owner：

- `Upload.Files` 是唯一 public 文件列表入口。
- `Files` 为 `null` 时，`Upload` 可以使用内部 fallback collection，但该 collection 仍是唯一 effective 文件集合。
- 绑定场景下，`ResetAsync` 和 Form 写入必须清空或更新集合内容，不替换外部绑定的集合实例。
- 禁止保留 `_allTaskList`、`TaskInfoList`、`CurrentTaskList` 或同类复制集合。
- 禁止从 `UploadList`、item container 或 picture trigger 反向创建业务任务状态。

队列状态只在 `UploadQueue` 和 `FileUploadScheduler` 内部存在：

- `UploadQueue` 通过 `UploadFileItem.Id` 查找对应 `FileUploadTask`。
- scheduler 回调只更新对应 `UploadFileItem.Status`、`Progress`、`ErrorMessage` 和 `Result`。
- `RemoveFileAsync`、`ResetAsync` 和 detach 在 AtomUI 可控制的路径中等待相关队列工作退出，再释放 source lease；`ReplaceExisting` 先在 UI commit 中替换集合，再让被观察的清理任务取消实际移除项，旧 source 在执行退出前保持有效。
- 外部集合 remove/reset、`Files` 属性替换和同步 Form Set/Clear 已经先改变了集合；`Upload` 必须立即从 accepted-source map 取走被移除项，再由被观察的清理任务等待对应 queue cancellation，最后释放 lease。
- `MaxConcurrentTasks`、`UploadTransport` 和 cancel-all 维护操作必须经由同一个 scheduler maintenance gate 串行执行；取消令牌不得让已出队的 Pending task 留在无 owner 状态。

Form 与验证状态：

- `FileValueMode` 控制 `IFormItemAware.GetFormValue` 的投影结果。
- `IFormItemAware.SetFormValue` 接收 `IEnumerable<UploadFileItem>` 并更新 `Files` 内容。
- `IFormItemAware.SetFormValue` 和 `ClearFormValue` 保持同步集合语义；被替换或清除项的 queue cancellation 与 source lease 释放由具名、被观察的后台清理任务完成。
- error 状态以 Avalonia `DataValidationErrors` 为准，不新增 Upload 专用 error owner。

## 5. 组合结构模型

### 控件角色图

```text
Upload (public coordinator)
  -> TriggerContent slot
     -> UploadTrigger (public action entry)
     -> UploadDropZone (public drop entry)
        -> UploadDefaultDropArea (public default visual)
  -> UploadList / UploadPictureShapeList (public/internal-observable list view)
     -> atom:ScrollViewer (template-stable auto-hide list scroll boundary)
        -> ItemsPresenter#PART_ItemsPresenter (template-stable)
           -> AbstractUploadListItem derived container (internal-observable)
           -> UploadAppendContentItem visual slot (picture display source only)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Upload` | 控件 | `Upload.cs` | 自身 | `Files`、`TriggerContent`、`UploadTransport`、`FileValueMode` | public | 可作为用户 API 和实现入口。 |
| `UploadTrigger` | 控件 | `UploadTrigger.cs` / `UploadTriggerTheme.axaml` | visual tree | `UploadSourceKind`、`SelectFilesAsync`、`SelectDirectoriesAsync` | public | 可公开示例，不能持有任务状态。 |
| `UploadDropZone` | 控件 | `UploadDropZone.cs` / `UploadDropZoneTheme.axaml` | visual tree | `DragState`、`DirectoryDropMode`、输入批次 | public | 唯一 DragDrop owner，drop 后必须交给统一输入管线。 |
| `UploadDefaultDropArea` | 控件 | `UploadDefaultDropArea.cs` / `UploadDefaultDropAreaTheme.axaml` | visual tree | `DropIcon`、`Header`、`SubHeader` | public | 只负责默认视觉，不处理 DataTransfer。 |
| `UploadInputPipeline` | internal service | Upload 输入实现 | `Upload` | picker/drop/programmatic input | private | 统一目录、准入、数量和 lease 生命周期。 |
| `UploadList` | 控件 | `UploadList.cs` / `UploadListTheme.axaml` | `Upload` 或用户布局 | `ListMaxHeight`、`ListScrollBarVisibility` | internal-observable | 用于理解布局和滚动，不作为第二状态 owner。 |
| `atom:ScrollViewer` | 模板节点 | `UploadListTheme.axaml` | `UploadList` template | 列表滚动 | template-stable | 使用 AtomUI lite/auto-hide 滚动条，只包裹列表，不包裹 trigger。 |
| `AbstractUploadListItem` | item container | `AbstractUploadListItem.cs` | items control container lifecycle | remove/preview/action 事件 | internal-observable | container recycle 必须释放 item 绑定和事件。 |
| `UploadAppendContentItem` | internal visual slot | `UploadAppendContentItem.cs` | `Upload` display source | `TriggerContent` | internal | 只能承载 picture append 入口，不能写入 `Files` 或持有任务状态。 |
| `UploadQueue` | internal service | `UploadQueue.cs` | `Upload` | 上传状态和取消 | private | 不暴露给用户，不引用视觉控件。 |

### Semantic Part 映射

| Part | 实现节点 | Marker ownership | 稳定路由 |
| --- | --- | --- | --- |
| `root` | `Upload` owner | 隐式，不添加 class | owner |
| `list` | `UploadList` / `UploadPictureShapeList` | `UploadTheme.axaml` 静态 marker | `/template/ .semantic-list` |
| `item` | `UploadTextListItem` / `UploadPictureListItem` / `UploadPictureShapeListItem` | `UploadList.CreateContainerForItemOverride` 使用生成常量添加 | `/template/ .semantic-list > .semantic-item` |

`UploadAppendContentItem` 是 picture display source 的 append trigger，不获得 `semantic-item`。文件状态只替换 item
内部 ControlTemplate，marker 保留在容器 owner 上。完整公共契约见 [Upload Semantic Part 契约](semantic-part.md)。

## 6. 生命周期与模板接入

Upload 的生命周期释放必须成对设计，不能依赖 GC 或视觉树自然释放。

| Acquire | Release |
| --- | --- |
| upload task cancellation token | task completion, cancel, remove, reset, detach |
| success auto-remove delay | remove, reset, detach, status changes away from Success |
| trigger parent lookup | detached visual tree |
| drop-zone drag session | leave、drop、disable 或 detached visual tree |
| input batch typed StorageItem/source sets | reject、cancel、failed cleanup、accepted item transfer、reset 或 detach；所有 transfer 验证当前 owner |
| accepted `UploadFileInfo` source lease | remove、external collection change、`Files` replacement、Form Set/Clear、reset 或 detach；释放前等待对应 upload execution 退出 |
| generated list container bindings | container recycle |

模板和集合接入规则：

- `OnApplyTemplate` 重新应用时先释放旧 template part 订阅，再接入新 part。
- `Files` 集合替换时解绑旧集合 change 订阅，接入新集合，保留新旧集合共有 id 的 source ownership，并把仅存在于旧集合的 source 移交给取消清理任务。
- item container 准备时只绑定当前 `UploadFileItem`；container recycle 时必须释放旧 item 的订阅和 action handler。
- item container 创建时必须保留生成的 `UploadSemanticParts.ItemClass`；prepare、clear、状态变化和 recycle 不得把 marker 转移到内容节点或 append trigger。
- `UploadTrigger` 和 `UploadDropZone` 查找 owner 时只依赖当前 visual ancestor；脱离 visual tree 后不缓存旧 owner。
- detach 时取消全部输入批次、上传任务、pending auto-remove delay、drag/drop 会话和 C# binding。
- collection/Form 通知、批次完成事件或上传取消事件抛出异常时，清理路径必须继续完成 task cancellation、accepted-source map 移交和 lease 释放，再向等待方传播单个异常或聚合异常。

## 7. 交互与事件处理

交互入口必须统一收敛到 `Upload` 的 public 方法：

- 文件按钮点击：`UploadTrigger.SourceKind=Files` 调用 `Upload.SelectFilesAsync`。
- 目录按钮点击：`UploadTrigger.SourceKind=Directories` 调用 `Upload.SelectDirectoriesAsync`。
- 拖拽提交：`UploadDropZone` 同步取得一次 StorageItem 快照，再交给 `UploadInputPipeline`。
- 文件 picker、目录 picker 与 Drop 共享 `Upload.IsMultipleEnabled` 契约；picker 打开时把该值写入 `AllowMultiple`，StorageItem 管线在处理批次时快照该值。程序化 `EnqueueFilesAsync` 不应用顶层输入限制。
- 手动移除：列表项发出 remove 请求，`Upload.RemoveFileAsync` 负责取消任务、取消 auto-remove 并从 `Files` 移除。
- 表单写入与清空：同步更新当前 effective 集合，并把被移除 source 移交给被观察的取消清理任务；不得直接释放仍可能被 transport 使用的 lease。

事件处理不得通过 suppression flag、延时刷新或强制重算来修正状态顺序。若出现状态不同步，优先修正 owner、事件顺序或释放路径。

## 8. 内部算法与关键流程

### 8.1 入队流程

1. 文件选择、目录选择、拖动或程序化入口创建统一输入批次。
2. `UploadInputPipeline` 在 UI 线程取得包含 `IsMultipleEnabled` 的不可变 option snapshot；StorageItem 批次先按稳定顺序限制顶层项目，再在单一 worker 边界中展开目录并取得受控 `IUploadFileSource`。程序化文件批次直接进入文件准入。
3. 管线在非 UI 执行上下文应用 `AllowedFileTypes` 和 `AdmissionPolicy`。
4. UI commit 根据当前 effective file count 和批次 option snapshot 计算数量策略，再把接受文件转换为 `UploadFileItem`；typed batch operation 在集合变更前验证 ownership，并只为实际保留项执行 source-to-Upload transfer。
5. 数量拒绝或尚未提交的文件仍由批次释放 lease；`AutoUpload=true` 时已提交 item 与 file info 进入 `UploadQueue`。
6. queue/scheduler 回调只更新对应 `UploadFileItem`，列表通过绑定观察变化。
7. 批次逐项释放全部未移交资源并聚合清理异常，再在 UI 线程触发一次 `InputBatchCompleted`；事件处理返回后，正常 Task 完成，取消或失败 Task 向等待方传播对应异常。

拖动协商、Drop 快照、目录遍历、平台矩阵和 StorageItem 生命周期的完整算法见 [Upload 拖动上传设计](drag-drop-design.md)。

### 8.2 移除与 reset 流程

- `RemoveFileAsync(id)` 查找对应 `UploadFileItem`，取消上传任务和 auto-remove delay，再从 effective `Files` 移除。
- `ResetAsync()` 取消全部运行和 pending 工作，取消全部 auto-remove delay，清空 effective `Files`，不替换绑定集合实例。
- 外部集合 remove/reset、`Files` 替换和 Form Set/Clear 无法延迟调用方已经执行的集合变化；这些路径先同步状态，再逐项等待 `UploadQueue.CancelAsync`，并只在该调用返回后释放对应 source lease。
- detach 使用 reset/cancel 语义释放资源，但不应触发用户不可预期的业务上传重试。
- `ResetAsync` 和 detach 将各清理阶段视为独立 acquire/release pair；非取消异常不得阻止后续队列清理或 source 释放。

### 8.3 成功自动移除流程

- 当 item 状态进入 `Success` 且 `SuccessAutoRemoveDelay` 不为空时，按 item id 注册延迟取消源。
- 延迟到期后通过 `RemoveFileAsync` 移除文件，确保与手动移除共享同一释放路径。
- item 状态离开 `Success`、手动移除、reset、detach 时取消对应延迟。

## 9. 资源、性能与 AOT 边界

资源边界：

- 异步上传任务、取消源、delay、drag/drop 事件、collection change 和 item container 绑定必须有确定释放点。
- DragOver 不读取文件值或元数据；Drop 只物化一次顶层 StorageItem 数据。
- 输入批次通过 arrival gate 串行执行；每个批次只建立一次显式 worker dispatch，目录展开、元数据读取和准入按候选稳定顺序处理，不创建无界并发任务；数量决策与集合变更保持在同一个 UI commit 边界。
- `UploadInputBatchOperation` 不保存泛化 `IDisposable`；StorageItem 和 source lease 使用独立的引用相等 owner set，释放遍历不得因单个异常提前中止。
- `UploadFileItem` 不持有视觉控件，避免文件项生命周期反向保留控件树。
- `UploadQueue` 不捕获 `Upload` 外的视觉对象；回调只写 item 状态。
- scheduler 的取消完成语义必须等待 transport execution task 真正退出，source lease 才能被上层释放。
- scheduler maintenance 必须先把全部 Pending task 转入 Cancelled、向全部 Running task 发出取消，再观察调用方取消令牌；并发 transport/并发度变更按调用顺序生效。
- C# binding 仅用于 AXAML 无法表达的动态关系，并必须挂到明确 owner 上释放。

性能边界：

- 列表滚动由 `UploadList` 内部 `atom:ScrollViewer` 管理，并启用 lite/auto-hide；避免用户外层 `ScrollViewer` 把 trigger、drop-zone 和 list 一起滚动。
- `UploadList` 依赖 ItemsControl container 生命周期，不为每次进度变化重建列表项。
- 大量文件状态更新时只更新对应 `UploadFileItem` 属性，不替换 `Files` 集合。

AOT 边界：

- 不通过运行时反射扫描 public API、Token、API 契约摘要或上传模型。
- 拖动输入使用 typed DataTransfer、StorageItem 和显式策略，不依赖平台私有反射或动态发现。
- 新增 public 类型应显式引用并由源码、Gallery 和测试覆盖。
- Semantic descriptor、`UploadListStyle`、`UploadItemStyle` 与 class 常量由生成器静态产生，不使用运行时类型发现。
- Source generator 生成文件不手工编辑。

## 10. 维护不变量

维护 Upload 时不得破坏以下不变量：

- `Files` 是唯一文件状态 owner。
- 不得引入与 `Files` 平行的任务集合，也不得把 picture trigger 伪装成文件项。
- trigger、drop-zone、list、item container 都不能保存第二份业务任务状态。
- `UploadDropZone` 是唯一 DragDrop 行为 owner；`UploadDefaultDropArea` 必须保持纯视觉职责。
- `IsMultipleEnabled` 只能限制 picker 与 Drop 的顶层 StorageItem 数量；目录内部展开、`EnqueueFilesAsync` 和最终 `MaxCount` 各自保持独立语义。
- `AdmissionPolicy` 不得在 UI 线程执行，也不得访问 Avalonia 控件；策略异常与策略显式拒绝必须使用不同 rejection reason。
- 取消和批次级失败通过 `UploadInputBatchStatus` 表达，不得创建空名称或虚假 `UploadRejectedItem`。
- ownership transfer 必须由 typed batch operation 验证；不得重新引入 `ownsFileSources`、`queueAlreadyCancelled` 或通用 transfer callback。
- 默认 DropZone/DropArea ControlTheme、模板视觉树、Token、布局和渲染结果不得因输入管线重构改变。
- PictureCard/PictureCircle 的上传入口只能通过 `EffectivePictureItems` 中的 display append slot 呈现，确保与图片项处于同一 wrap flow。
- `semantic-list` 必须存在于两套 Upload 根模板，`semantic-item` 只存在于真实文件容器；append slot 不属于 item cardinality。
- `RemoveFileAsync`、外部集合变更、`Files` 替换、Form Set/Clear、`ResetAsync` 和 detach 必须以各自时序释放上传任务、source lease、auto-remove delay、集合订阅和 container 绑定。
- `DataValidationErrors` 是 error 状态来源，Upload 不维护独立 error 机制。
- AXAML-first binding 是默认选择；C# binding 必须说明 AXAML 不能表达的原因和释放 owner。
- Gallery 示例、控件文档和测试必须使用同一套 public contract。

## 11. 测试与验证

重构任务按以下层级验证：

| 变更范围 | 验证 |
| --- | --- |
| public API | `UploadRedesignContractTests` 检查属性、默认值、batch status/failure、AdmissionDecision factory 和 raw Exception 移除。 |
| 单一状态 owner | `UploadFileStateTests` 覆盖 add、remove、reset、集合替换和外部绑定集合保持。 |
| 队列生命周期 | `UploadSchedulerTests` 覆盖 running 计数、完成释放、cancel all、维护操作序列化、取消中途一致性和 transport 替换。 |
| 触发器与拖拽 | 行为测试覆盖文件/目录触发、Copy/None 协商、Drop-only 数据读取、嵌套路由和 owner 委托。 |
| 输入管线与生命周期 | fake StorageItem 覆盖目录模式、worker-thread 准入、数量策略、typed adopt/transfer、取消、异常聚合、ReplaceExisting、detach 和 lease 恰好释放一次。 |
| 平台拖动 | Windows、macOS、X11 和 Wayland 跨进程验证文件、目录、效果反馈和异步读取。 |
| 渲染兼容 | Light/Dark、不同缩放和自定义 Content 的截图、Measure、Arrange 与 Bounds 保持基线。 |
| 成功自动移除 | `UploadAutoRemoveTests` 覆盖 delay 到期、remove、reset、detach 和状态变更取消。 |
| Form 与验证 | `UploadFormValueTests` 覆盖 `FileValueMode`、set/clear 和 `DataValidationErrors`。 |
| Gallery 示例 | `UploadShowCasePageTests` 和 snapshot 覆盖示例、本地化和源码片段。 |
| Semantic Part | `UploadSemanticPartTests` 覆盖 descriptor、四种 `ListType`、生成 Style route 和 append trigger 排除；`UploadSemanticPartHighlightTests` 覆盖 Gallery 高亮。 |
| 文档卫生 | `git diff --check` 和关键术语扫描。 |

文档验证命令：

```bash
rg -n "UploadFileItem|UploadTrigger|UploadDropZone|UploadDefaultDropArea|UploadInputPipeline|SuccessAutoRemoveDelay|UploadFileValueMode" docs/controls/desktop/data-entry/upload
git diff --check
```
