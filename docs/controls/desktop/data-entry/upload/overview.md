# Upload 桌面版架构设计

本文档定义 `Upload` 桌面版的设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见 [Upload Semantic Part 契约](semantic-part.md)，内部实现原理见 [Upload 桌面版实现原理](implementation.md)，拖动输入专项契约见 [Upload 拖动上传设计](drag-drop-design.md)，Upload Token 的专项设计见 [Upload Token 设计](token.md)，设计和契约变化记录见 [Upload Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Upload` |
| 控件状态 | Stable |

Upload 是 AtomUI 桌面控件体系中的上传协调控件，用于管理文件选择、目录选择、拖拽提交、任务状态、列表展示、Form 值投影和上传操作入口。

Upload 不负责具体网络传输协议、文件存储服务或业务附件模型。这些职责由业务层或 `IFileUploadTransport` 承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Upload`
- `src/AtomUI.Controls.Shared/Net`
- `controlgallery/AtomUIGallery/ShowCases/DataEntry/Upload`

## 2. 设计语言

Upload 的设计语言围绕“单一文件状态 owner + 可组合操作入口 + 独立列表视图”组织，而不是围绕某个固定模板节点组织。

| 维度 | 含义 | Upload 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | `Upload` 是上传状态协调器，统一管理文件、队列、选择入口、拖拽入口和列表视图。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Files` 保存真实文件项；`TriggerContent`、`UploadTrigger`、`UploadDropZone` 和 `UploadList` 负责组合展示。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | `UploadFileItem.Status`、`Progress`、`ErrorMessage`、`Result` 是任务状态来源；Form 错误走 `DataValidationErrors`。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Upload Token + ControlTheme；滚动边界在 `UploadList` 内部，并使用 AtomUI 自动隐藏滚动条。 |

## 3. API 与契约模型

Upload 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

### 3.1 单一状态 owner

Upload 以 `Files` 作为唯一上传文件状态 owner。触发器、拖拽区和列表只是组合视图或动作入口，不保存第二份任务状态。

- `Upload.Files` 表示所有可观察上传文件。
- `UploadTrigger` 通过 `SourceKind=Files|Directories` 选择文件或目录。
- `UploadDropZone` 独占拖动协商和 Drop 快照，并把输入批次提交给最近的 `Upload`。
- `UploadDefaultDropArea` 只提供默认视觉，不读取拖动数据或维护文件状态。
- `UploadList` 渲染 `Files`，并独立管理滚动区域。
- `PictureCard` / `PictureCircle` 的上传入口通过 append slot 呈现，不作为 `UploadFileItem`。

### 3.2 核心 public surface

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 文件状态 | `Files`、`UploadFileItem` | 唯一文件状态 owner，支持绑定、Form 投影和列表渲染。 |
| 文件选择 | `UploadTrigger`、`UploadSourceKind`、`SelectFilesAsync`、`SelectDirectoriesAsync` | 文件与目录选择是独立动作入口；`IsMultipleEnabled` 统一决定用户是否可提交多个顶层 StorageItem。 |
| 拖拽提交 | `UploadDropZone`、`UploadDirectoryDropMode`、`UploadDragState` | DropZone 负责平台协商和快照，`Upload` 负责统一准入与文件状态。 |
| 用户输入范围 | `IsMultipleEnabled` | 统一限制文件选择、目录选择和 Drop 的顶层 StorageItem 数量，不限制单个目录的文件展开结果或显式程序化批量输入。 |
| 文件准入 | `AllowedFileTypes`、`CountOverflowBehavior`、`AdmissionPolicy` | picker、directory、drop 和 programmatic 输入共享同一准入与数量语义。 |
| 输入结果 | `InputBatchCompleted`、`UploadInputBatchCompletedEventArgs` | 每个输入批次在 UI 线程统一报告接受项、拒绝项以及 Completed、Cancelled 或 Failed 终态。 |
| 文件内容 | `UploadFileInfo`、`IUploadFileSource` | Transport 通过可打开内容源读取文件，不假定本地路径可访问。 |
| 上传队列 | `UploadTransport`、`AutoUpload`、`MaxConcurrentTasks`、`UploadQueue` | 上传调度与视觉控件解耦，生命周期由 `Upload` 统一释放。 |
| 列表展示 | `UploadList`、`ListType`、`ListMaxHeight`、`ListScrollBarVisibility` | 列表内部滚动，触发区保持固定；应用通过 `list` / `item` Semantic Part 定制稳定区域。 |
| 触发入口 | `TriggerContent`、`UploadTrigger`、Picture append slot | 文件/目录触发器由用户布局组合，PictureCard/PictureCircle 通过显示源 append slot 呈现。 |
| 状态反馈 | `SuccessAutoRemoveDelay`、`PendingText`、`FileValueMode` | 成功自动移除、待上传文案和 Form 值投影可配置。 |
| 视觉与动效 | `IsMotionEnabled`、Upload Token | 只表达视觉状态，不保存业务任务状态。 |

输入与准入 API 的默认契约：

| 成员 | 默认值 | 语义 |
| --- | --- | --- |
| `Upload.AllowedFileTypes` | `null` | 不按文件名 pattern 或 MIME 拒绝文件；空集合含义相同。 |
| `Upload.CountOverflowBehavior` | `RejectExcess` | 按候选稳定顺序接受剩余容量内的文件，并拒绝超出项。 |
| `Upload.AdmissionPolicy` | `null` | 不执行额外业务准入；非空策略固定在非 UI 执行上下文调用。 |
| `Upload.MaxCount` | `int.MaxValue` | 输入管线可提交的最大 effective 文件数。 |
| `Upload.IsMultipleEnabled` | `false` | 文件选择器只选择一个文件、目录选择器只选择一个目录，Drop 只处理第一个顶层 StorageItem；单个目录仍可展开多个文件。 |
| `UploadDropZone.IsOpenFileDialogOnClick` | `true` | 主指针点击 DropZone 时按 `SourceKind` 打开选择器。 |
| `UploadDropZone.SourceKind` | `Files` | 点击时打开文件选择器。 |
| `UploadDropZone.IsFileDropEnabled` | `true` | DropZone 参与文件 DragDrop 协商。 |
| `UploadDropZone.DirectoryDropMode` | `Reject` | 外部拖入目录时拒绝目录。 |
| `UploadDropZone.MaxDirectoryDepth` | `32` | 递归目录根深度为 `0`，属性值不得小于 `0`。 |
| `UploadDropZone.MaxEnumeratedItems` | `10000` | 每个顶层目录树最多观察的文件和目录总数，属性值必须大于 `0`。 |
| `UploadDropZone.DragState` | `None` | 只读拖动协商状态。 |
| `UploadDropZone.IsDropProcessing` | `false` | 只读活动 Drop 批次状态。 |

`Upload.EnqueueFilesAsync`、文件选择器、目录选择器和拖动输入分别产生 `Programmatic`、`FilePicker`、`DirectoryPicker` 和 `DragDrop` 批次。每个实际启动的批次只触发一次 `InputBatchCompleted`；事件在输入处理进入终态、剩余批次资源完成释放且接受项已经提交后于 UI 线程触发。正常批次在事件后完成 Task，取消批次在事件后传播 `OperationCanceledException`，失败批次在事件后传播原始异常或清理聚合异常。

`IsMultipleEnabled` 只约束用户输入批次的顶层 StorageItem。值为 `false` 时，文件和目录 picker 都使用 `AllowMultiple=false`，Drop 按快照稳定顺序处理第一个顶层项目，并以 `MultipleSelectionNotAllowed` 拒绝和释放其余项目；第一个项目是目录时，目录内部仍按 `DirectoryDropMode` 展开多个文件。值为 `true` 时，picker 与 Drop 均允许多个顶层项目。`EnqueueFilesAsync(IEnumerable<UploadFileInfo>)` 是显式程序化批量 API，不受该属性限制，但仍受文件准入、`MaxCount` 和 `CountOverflowBehavior` 约束。

`UploadInputBatchStatus` 通过 `Completed`、`Cancelled` 和 `Failed` 区分互斥终态。只有 `Failed` 携带 `UploadInputFailureReason`；取消和批次级失败不再构造虚假的 rejected item。`UploadRejectedItem` 只公开稳定的 `UploadRejectionReason`、业务 rejection code 和业务 message，不公开平台 `Exception`。`UploadAdmissionDecision` 通过 `Accept()` 和 `Reject(...)` 创建，不能构造“接受但携带拒绝信息”的矛盾状态。

`UploadFileInfo` 是不可变输入描述。`Name` 和 `Source` 必须存在；`Path`、`Size`、`ContentType`、`DateCreated` 与 `DateModified` 都允许为空，因为跨进程或受限平台存储项不保证提供本地路径和完整元数据。`IUploadFileSource.OpenReadAsync` 是 transport 读取内容的唯一稳定入口。

### 3.3 公共状态类型

```csharp
public class UploadFileItem : AvaloniaObject
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Uri? Path { get; set; }
    public long Size { get; set; }
    public FileUploadStatus Status { get; set; }
    public double Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public FileUploadResult? Result { get; set; }
    public object? UserData { get; set; }
    public string? PendingText { get; set; }
    public bool IsImageFile { get; set; }
}

public enum UploadSourceKind
{
    Files,
    Directories
}

public enum UploadFileValueMode
{
    AllFiles,
    SuccessfulFiles,
    Results
}
```

### 3.4 推荐用法

```xml
<atom:Upload Files="{Binding Attachments}"
             UploadTransport="{Binding UploadTransport}"
             ListMaxHeight="180"
             ListScrollBarVisibility="Auto"
             SuccessAutoRemoveDelay="0:0:3">
    <atom:Upload.TriggerContent>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <atom:UploadTrigger SourceKind="Files">
                <atom:Button Content="上传文件" />
            </atom:UploadTrigger>
            <atom:UploadTrigger SourceKind="Directories">
                <atom:Button Content="上传文件夹" />
            </atom:UploadTrigger>
        </StackPanel>
    </atom:Upload.TriggerContent>
</atom:Upload>
```

## 4. 行为与状态模型

Upload 的状态流只允许按以下路径收敛：

```text
Public API / UploadTrigger / UploadDropZone
  -> UploadInputPipeline
  -> top-level input limit / directory traversal / file admission
  -> UI count policy / accepted file commit
  -> Files collection
  -> UploadQueue / FileUploadScheduler
  -> UploadFileItem.Status / Progress / Result
  -> UploadList item containers
  -> Gallery 可观察行为
```

状态维护规则：

- `Files` 是唯一文件状态 owner；实现中按文件 id 保存的 accepted `UploadFileInfo` 只承担内容源 lease，不形成第二份可观察任务状态。
- `UploadQueue` 只负责把 `UploadFileItem` 映射到 `FileUploadTask` 并转发调度结果，不直接操作视觉容器。
- `UploadList` 只渲染 `Files`，不得创建、删除或隐藏真实任务状态。
- 文件选择和目录选择由 `UploadTrigger.SourceKind` 决定，可以在同一 `Upload` 下并存。
- `IsMultipleEnabled` 是 picker 与 Drop 共享的顶层输入策略，不得复用为目录展开数量或最终文件容量限制。
- `SuccessAutoRemoveDelay` 的延迟任务必须在 remove、reset、detach 和状态离开 success 时取消。
- Form 值由 `FileValueMode` 投影，错误状态以 Avalonia `DataValidationErrors` 为准。

## 5. 视觉与主题模型

Upload 的视觉模型由控件模板、ControlTheme、SharedToken 和控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `UploadTheme.axaml` | 根模板，连接 `TriggerContent`、list 和 picture display source。 |
| `UploadTriggerTheme.axaml` | 触发器 shell，只承载用户内容和点击动作，不硬编码 Button。 |
| `UploadDropZoneTheme.axaml` | 拖拽行为 shell，只承载用户内容和内容对齐。 |
| `UploadDefaultDropAreaTheme.axaml` | 默认拖动视觉，保留边框、图标、标题、副标题、状态 selector 和动效，不处理拖动数据。 |
| `UploadListTheme.axaml` | 上传列表 shell，内部拥有自动隐藏的 `atom:ScrollViewer` 和滚动边界。 |
| `UploadTextListItemTheme.axaml` | Text 列表项状态视觉。 |
| `UploadTextListItemHeaderTheme.axaml` | Text 列表项头部状态视觉。 |
| `UploadPictureListItemTheme.axaml` | Picture 列表项状态视觉。 |
| `UploadPicturePendingContentTheme.axaml` | Picture pending 内容，优先显示 `UploadFileItem.PendingText`。 |
| `UploadPicturePreviewContentTheme.axaml` | Picture preview 内容。 |
| `UploadPictureUploadingContentTheme.axaml` | Picture uploading 内容。 |
| `UploadPictureDefaultContentTheme.axaml` | Picture fallback 内容。 |
| `UploadPictureShapeListTheme.axaml` | PictureCard/PictureCircle 列表布局。 |
| `UploadPictureShapeListItemTheme.axaml` | PictureCard/PictureCircle 列表项状态视觉。 |
| `UploadPictureShapePendingContentTheme.axaml` | Shape pending 内容，优先显示 `UploadFileItem.PendingText`。 |
| `UploadPictureShapePreviewContentTheme.axaml` | Shape preview 内容。 |
| `UploadPictureShapeUploadingContentTheme.axaml` | Shape uploading 内容。 |
| `UploadPictureShapeDefaultContentTheme.axaml` | Shape fallback 内容。 |

主题维护规则：

- Trigger 不作为文件项渲染，PictureCard/PictureCircle 使用不进入 `Files` 的 display append slot 保持同一 wrap flow。
- Text/Picture 根模板必须在 trigger 与 list 之间保留 Shared spacing，避免触发按钮和第一条文件项贴在一起。
- 滚动区域只包裹列表，不包裹 trigger。
- 可由 AXAML 表达的模板状态必须优先留在 AXAML。
- Token 只表达视觉变量，不承载上传状态、队列状态或 Form 错误。

公开 Semantic Part 为 `root`、`list` 与 `item`。`list` 在四种 `ListType` 下都表示唯一活动文件列表，`item`
只表示真实 `UploadFileItem` 容器，不包含 PictureCard/PictureCircle 的 append trigger。完整 Selector、ContractType、
状态矩阵和定制边界见 [Upload Semantic Part 契约](semantic-part.md)。

## 6. 控件家族或集成关系

主要协作类型：

- `Upload`：上传协调器，持有 public API、`Files`、上传队列、Form 适配和生命周期释放。
- `UploadFileItem`：文件状态模型，承载文件元数据、上传状态、进度、错误、结果和自定义 pending 文案。
- `UploadQueue`：内部上传队列协调器，映射 `UploadFileItem` 与 `FileUploadTask`。
- `UploadTrigger`：独立触发控件，按 `UploadSourceKind` 调用最近 `Upload` 的文件或目录选择方法。
- `UploadDropZone`：独立拖拽控件，拥有 DragDrop 状态机、Copy/None 协商、Drop 快照和输入批次入口。
- `UploadDefaultDropArea`：默认拖动视觉控件，只渲染 DropZone 内容，不拥有 DataTransfer 或上传状态。
- `UploadList`：列表视图控件，渲染 `Files` 并维护滚动边界。
- `AbstractUploadListItem`：列表项基类，只绑定 `UploadFileItem` 属性并发出 item action。
- `AbstractUploadPictureContent`：图片类内容基类，投射 `UploadFileItem` 的视觉状态。
- `FileUploadScheduler`：共享上传调度器，必须正确跟踪 pending/running/completed/cancelled 状态。
- `IFileUploadTransport`：业务上传传输边界。

集成关系：

- Gallery 展示新的组合用法，不再示例 `DefaultTaskList`、`IsUploadDirectoryEnabled` 或 `IsShowUploadTrigger`。
- Form 读取 `FileValueMode` 投影结果；Form 写入应更新 `Files` 内容而不是替换集合实例。
- `DataValidationErrors` 是 error 状态来源，Upload 不独立维护另一套 error 机制。

## 7. 兼容性不变量

维护 Upload 时必须保持以下兼容性不变量：

- 不引入与 `Files` 平行的任务集合或把触发入口伪装成文件项。
- 不让视觉容器反向持有业务任务状态。
- 不用延时、强制刷新或 suppression flag 掩盖状态不同步。
- Template reapply、集合替换、remove、reset、detach 都必须释放旧订阅、取消运行任务和取消 pending auto-remove。
- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- `UploadDropZone` 和 `UploadDefaultDropArea` 的 ControlTheme、模板视觉树、Token 映射、布局和默认渲染结果保持稳定。
- `UploadDropZone` 的新增拖动状态伪类只提供自定义主题入口；AtomUI 默认主题不得据此改变 pointerover、disabled、motion、Light/Dark 或缩放后的视觉结果。
- `root`、`list`、`item` 的 selector class、route、ContractType 和 cardinality 属于公共主题 API；所有内置模板变体必须实现相同契约。
- 文档只描述稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 文件选择模型

`UploadTrigger.SourceKind` 决定点击后的选择动作：

- `Files`：打开文件选择器，支持 `AllowedFileTypes` 和 `MaxCount`。
- `Directories`：打开目录选择器，并以 `TopLevelFiles` 目录策略提交统一输入管线。

文件和目录触发器可以同时存在于一个 `Upload.TriggerContent` 中。两种 picker 都从所属 `Upload.IsMultipleEnabled` 取得同一个 `AllowMultiple` 值，避免点击 DropZone、独立 `UploadTrigger` 和拖动输入形成不同的多选语义。

### 8.2 拖动输入模型

`UploadDropZone` 只在 `Drop` 阶段取得一次 StorageItem 快照；`DragEnter` 和 `DragOver` 仅检查 typed DataTransfer 格式并明确返回 `Copy` 或 `None`。顶层多选限制在目录展开前执行，随后目录展开、文件类型、业务准入、最终数量限制和资源释放全部收敛到 `Upload` 的统一输入管线。批次分别持有 typed StorageItem 和 source lease ownership，storage-to-source 与 source-to-Upload transfer 都验证当前 owner；单个释放异常不能截断其他资源清理。

`UploadDefaultDropArea` 保留当前默认渲染，但不再设置 `DragDrop.AllowDrop`、注册 Drop handler 或暴露第二条文件事件。完整状态、平台、Template 和生命周期契约见 [Upload 拖动上传设计](drag-drop-design.md)。

默认契约为 `IsOpenFileDialogOnClick=true`、`SourceKind=Files`、`IsFileDropEnabled=true`、`DirectoryDropMode=Reject`、`MaxDirectoryDepth=32` 和 `MaxEnumeratedItems=10000`。`DragState=None` 与 `IsDropProcessing=false` 只表达运行状态，默认主题不使用它们改变渲染。

### 8.3 列表滚动模型

`UploadList` 内部拥有滚动边界：

- `ListMaxHeight` 控制列表最大高度。
- `ListScrollBarVisibility` 控制垂直滚动条。
- 内部滚动容器应使用 AtomUI `ScrollViewer` 的 lite/auto-hide 模式。
- trigger 位于滚动区域外，用户滚动列表时上传按钮不移动。

### 8.4 自动移除模型

`SuccessAutoRemoveDelay` 不为空时，文件进入 success 后开始延迟移除。延迟任务由 `Upload` 统一管理，并在以下场景取消：

- 用户手动移除文件。
- 文件状态离开 success。
- `ResetAsync`。
- 控件 detach。

### 8.5 Form 与验证模型

`FileValueMode` 决定 Form 值：

- `AllFiles`：返回全部 `UploadFileItem`。
- `SuccessfulFiles`：返回成功文件。
- `Results`：返回成功文件的 `FileUploadResult`。

错误状态必须投射到 Avalonia `DataValidationErrors`，不得另建 Upload 专属 error 机制。

## 9. 文档导航与验证策略

关联文档：

- [Upload 桌面版实现原理](implementation.md)
- [Upload Semantic Part 契约](semantic-part.md)
- [Upload 拖动上传设计](drag-drop-design.md)
- [Upload Token 设计](token.md)
- [Upload Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和绑定语义。 |
| 状态模型 | 覆盖 add、remove、reset、detach、collection replacement、cancel 和 auto-remove。 |
| AXAML/Theme | 检查 trigger 固定、list 内部滚动、display append slot、template part、资源 key 和 Light/Dark 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查 fixed trigger、scrollable list、file/directory dual trigger 和 auto-remove 示例。 |
| Semantic Part | 验证 descriptor、四种 `ListType` marker、生成 Style route、Gallery 高亮和 append trigger 排除。 |
