# ImagePreviewer 桌面版架构设计

本文档定义 ImagePreviewer 控件家族的正式架构契约，包括来源模型、内容身份、缓存策略、集合与加载状态、宿主行为和主题边界。
内部实现与维护职责见 [ImagePreviewer 桌面版实现原理](implementation.md)，Token 语义见
[ImagePreviewer Token 设计](token.md)，变化记录见 [ImagePreviewer Changelog](changelog.md)。应用级加载、缓存、安全和 codec
能力由 [AtomUI 图片加载系统](../../../../architecture/systems/image-loading/overview.md) 统一提供。预览图片的基础适配、变换状态、输入和边界
见 [ImagePreviewer 预览变换设计](preview-transform-design.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/ImagePreviewer` |
| 控件状态 | Stable |

`ImagePreviewer` 提供单封面入口与完整图片预览；`ImageGroupPreviewer` 提供多封面入口。两者共享同一套 immutable item、当前项、
预加载、标题、窗口或 overlay、加载状态和结果租约模型。

Previewer 是应用级图片加载系统的消费者，不拥有网络栈、文件读取器、codec、缓存或全局并发调度器。所有来源解析、内容验证、
请求合并、缓存读写和解码都进入当前 `Application` 的 `IImageLoader`；Previewer 只拥有控件 entry、waiter、显示状态和结果 lease。

## 2. 设计语言

ImagePreviewer 以“页面封面 -> 沉浸预览”的两阶段体验表达图片查看。关闭态封面提供稳定尺寸和加载反馈；打开态聚焦当前完整图，
相邻项只作为低优先级预加载。单封面与多封面只改变入口布局，不改变当前项、标题、缩放、导航、来源验证和加载语义。

Loading、Failed 和 Loaded 都是明确状态。封面 Skeleton、viewer Spin 和错误内容只呈现状态，不拥有加载行为。Desktop native dialog
与 Browser overlay 共享同一 item/entry 模型，平台差异只位于宿主能力和标题栏组合。

加载架构遵守以下原则：

- 来源地址不是内容身份；路径或 URI 相同不等于内容未变化。
- 普通读取先验证来源，再决定能否复用内容和解码结果；不能根据旧来源键直接返回 decoded image。
- `ImageSourceKey -> ImageSourceVersion -> ImageContentId -> ImageDecodeKey` 是唯一身份链。
- 缓存读取策略与缓存写入位置正交，Reload 只改变一次请求的读取策略。
- Previewer 的集合、宿主和 entry 生命周期不等于 Application cache 生命周期。
- Full 与 Thumbnail 通道的 generation、取消、状态、尺寸桶和结果 lease 完全隔离。
- borrowed image 的所有权始终属于调用方，loader、cache 和 Previewer 都不能销毁它。

## 3. API 与契约模型

### 3.1 封闭图片来源模型

所有加载型入口只接收 `ImageSource`。`ImageSource` 是 AtomUI 定义的封闭来源层次；基类不提供外部继承入口，每个具体来源均为
不可变 sealed 类型：

```csharp
[TypeConverter(typeof(ImageSourceConverter))]
public abstract class ImageSource
{
    public string? DisplayName { get; }

    public static ImageSource Parse(string source);
    public static bool TryParse(string? source, out ImageSource? result);
}

public sealed class HttpImageSource : ImageSource
{
    public HttpImageSource(Uri uri);
    public Uri Uri { get; }
}

public sealed class FileImageSource : ImageSource
{
    public FileImageSource(
        string path,
        ImageFileValidationMode validation = ImageFileValidationMode.Metadata);

    public string Path { get; }
    public ImageFileValidationMode Validation { get; }
}

public sealed class AssetImageSource : ImageSource
{
    public AssetImageSource(Uri uri);
    public Uri Uri { get; }
}

public sealed class StorageFileImageSource : ImageSource
{
    public StorageFileImageSource(IStorageFile file, string? revision = null);
    public IStorageFile File { get; }
    public string? Revision { get; }
}

public sealed class BytesImageSource : ImageSource
{
    public BytesImageSource(ReadOnlyMemory<byte> bytes, string? displayName = null);
    public ReadOnlyMemory<byte> Bytes { get; }
}

public sealed class StreamImageSource : ImageSource
{
    public StreamImageSource(
        Func<CancellationToken, ValueTask<Stream>> openStream,
        string? identity = null,
        string? revision = null,
        string? displayName = null);
    public Func<CancellationToken, ValueTask<Stream>> OpenStream { get; }
    public string? Identity { get; }
    public string? Revision { get; }
}

public sealed class BorrowedImageSource : ImageSource
{
    public BorrowedImageSource(IImage image, string? displayName = null);
    public IImage Image { get; }
}
```

`BytesImageSource` 防御性复制输入；`StreamImageSource` 的工厂每次必须返回新的可读流；`BorrowedImageSource` 只借用调用方的
`IImage`。字符串转换只接受 `http`、`https`、`avares`、`file` 和平台绝对路径，未知 scheme 不回退为文件路径。

文件来源提供两种验证强度：

```csharp
public enum ImageFileValidationMode
{
    Metadata,
    ContentHash
}
```

- `Metadata` 是默认值，使用已打开句柄可观察的文件 identity、长度和修改/变化令牌判断内容是否变化。
- `ContentHash` 每次请求都读取正文并计算内容摘要，用于必须识别元数据保持不变的内容替换场景。

### 3.2 图片项模型

Public 数据输入只有 `ItemsSource: IEnumerable<ImagePreviewItem>?`。`ImagePreviewItem` 是不可变配置 record：

```csharp
public sealed record ImagePreviewItem
{
    public ImagePreviewItem(ImageSource source);

    public ImageSource Source { get; init; }
    public ImageSource? ThumbnailSource { get; init; }
    public ImageSource? FallbackSource { get; init; }
    public ImageRequestOptions? RequestOptions { get; init; }
    public string? Title { get; init; }
    public object? Tag { get; init; }
}
```

`Source` 是完整图来源。`ThumbnailSource` 是可选封面来源；为空时封面使用 `Source` 按封面尺寸解码。完整图加载顺序为
`Source -> FallbackSource`，缩略图加载顺序为 `ThumbnailSource -> Source -> FallbackSource`，同一通道内相同来源 identity
不重复请求。

`ImagePreviewItem` 不保存加载状态，不持有 cancellation token、图片或 Visual。相同 item 可以出现在多个控件中，各控件的
entry 状态互不影响；底层加载器仍可按内容身份共享请求和缓存。`Tag` 只供业务关联，加载器和标题解析器不通过反射读取它。

### 3.3 请求与缓存策略

缓存读取语义与成功结果的允许存储位置是两个独立维度：

```csharp
public sealed record ImageRequestOptions
{
    public ImageCacheReadPolicy CacheRead { get; init; } =
        ImageCacheReadPolicy.ValidateSource;

    public ImageCacheStoragePolicy CacheStorage { get; init; } =
        ImageCacheStoragePolicy.MemoryAndDisk;

    public string? CachePartition { get; init; }
    public string? Variant { get; init; }
    public TimeSpan? Timeout { get; init; }
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
}

public enum ImageCacheReadPolicy
{
    ValidateSource,
    RefreshSource,
    PreferCache,
    CacheOnly
}

public enum ImageCacheStoragePolicy
{
    None,
    Memory,
    MemoryAndDisk
}
```

| `ImageCacheReadPolicy` | 已有来源映射 | 来源访问 | cache miss | 结果验证语义 |
| --- | --- | --- | --- | --- |
| `ValidateSource` | 仅在来源规则确认有效后复用 | 按 File、HTTP、Storage 等规则探测或重验证 | 访问来源 | `Current`、`Revalidated` 或 `NotRequired` |
| `RefreshSource` | 不把现有映射视为完成结果 | 强制访问来源；HTTP 可以使用 validator 发条件请求 | 访问来源 | `Current`、`Revalidated` 或 `NotRequired` |
| `PreferCache` | 映射和内容都存在时直接复用 | 仅 miss 时访问来源 | 访问来源 | 未验证命中为 `Unverified` |
| `CacheOnly` | 只使用已有映射和内容 | 禁止 | `CacheMiss` | 未验证命中为 `Unverified` |

`PreferCache` 不隐含后台刷新。一次 `LoadAsync` 不会在返回后静默替换结果；需要新内容时由调用方显式发起
`RefreshSource` 请求。`CacheStorage` 只决定本次成功结果允许写入 memory 或 disk；HTTP cache directive、安全策略和平台能力
只能进一步收紧，不能被 per-request 设置放宽。persistent cache 未启用时，`MemoryAndDisk` 的有效目标是 memory。

`ImageSourceValidation.Current` 表示内容满足该来源选择的验证契约，不表示所有来源都完成了逐字节证明。File 的
`Metadata` 模式以文件 identity 和 metadata token 为契约；需要逐字节证明时使用 `ContentHash`。

所有调用方提供、可能影响 HTTP 表示的 header 都以名称和不可逆值摘要进入来源 identity；header 原值不进入可打印 key、
持久文件名或默认 diagnostics。存在认证 header 而未提供 `CachePartition` 时，请求禁止共享、禁止读取共享缓存并强制不存储。

### 3.4 AbstractImagePreviewer 公共契约

| 契约组 | 成员与默认值 | 语义 |
| --- | --- | --- |
| 集合 | `ItemsSource=null` | 唯一图片项输入 |
| 当前项 | `CurrentIndex=0`、`CurrentItem` | `CurrentIndex` 默认 TwoWay；显示时 clamp，但不静默改写外部值 |
| 封面 | `CoverIndex=0`、`CoverWidth=NaN`、`CoverHeight=NaN` | CoverIndex 非负并在显示时 clamp，与 CurrentIndex 独立 |
| 预加载 | `PreloadCount=1` | 当前项前后各预加载的完整图片数量 |
| 切换显示 | `ImageSwitchMode=Immediate` | `Immediate` 在目标无图时同周期清空旧图并报告 loading；`WaitForLoaded` 在有效保留帧存在时持续显示旧图直到目标完成或失败 |
| 当前加载状态 | `CurrentLoadState/Error/Progress`、`IsCurrentLoading/Loaded/Failed` | 当前完整图 entry 的只读投影 |
| 占位内容 | `LoadingContent/Template`、`ErrorContent/Template` | 只替换主题 presenter 内容，不改变状态机 |
| 打开状态 | `IsOpen=false` | 默认 TwoWay，统一驱动 native dialog 或 Browser overlay |
| 预览交互 | `IsImageMovable=true`、`ImageScaleStep=0.5`、`ImageMinScale=1`、`ImageMaxScale=50`、`IsWheelZoomEnabled=true` | 基础适配、拖拽、缩放、旋转、翻转、reset 和边界 |
| 窗口 | `IsDialogModal=false`、`IsDialogTopmost=false` | Desktop native window 行为；Browser 使用 overlay |
| 标题 | `PreviewTitle`、`PreviewTitleIcon`、`PreviewTitleResolver` | 当前项标题和可选 PathIcon |
| 动效 | `IsMotionEnabled` | 控制主题动效，不改变加载与集合语义 |

命令和事件：

- `OpenDialog()` 打开当前宿主；无有效 item 时不打开。
- `ReloadCurrent()` 只刷新当前完整图通道。
- `ReloadItem(index)` 只刷新指定 entry 的完整图通道；越界抛出 `ArgumentOutOfRangeException`。
- `DialogOpened`、可取消的 `DialogClosing` 和 `DialogClosed` 描述宿主生命周期。
- `ImageOpened`、`ImageFailed` 只投影当前完整图状态，事件参数包含 immutable item 与索引。

### 3.5 ImagePreviewer 与 ImageGroupPreviewer

单封面 `ImagePreviewer` 增加：

- `CoverIndicatorContent`、`CoverIndicatorContentTemplate`；
- `IsShowCoverMask=true`；
- `CoverLoadState/Error/Progress`；
- `IsCoverLoading`、`IsCoverLoaded`、`IsCoverFailed`；
- `ReloadCover()`，只刷新当前封面的 Thumbnail 通道。

`ImageGroupPreviewer` 只增加 `ItemsPanel`。关闭态下它为每个 effective item 请求缩略图；点击某个封面时先把
`CurrentIndex` 设置为该项索引，再打开预览。单封面控件的 CoverIndex 只决定关闭态封面；点击单封面只打开预览，不隐式把
CurrentIndex 同步为 CoverIndex。

稳定 template part 契约：

| Template Part | AtomUI 节点 | 职责 | 稳定性 |
| --- | --- | --- | --- |
| `PART_CoverItemsControl` | `ImageGroupPreviewerTheme` | group 封面集合 | template-stable |
| `PART_ImageViewerScene` | `ImageViewerTheme` | 预览图片坐标空间 | template-stable |
| `PART_ImageRenderer` | `ImageViewerTheme` | 只渲染 entry 已持有的 `IImage` | template-stable |
| `PART_LoadingPresenter` | `ImagePreviewerCoverTheme` / `ImageViewerTheme` | 封面 Skeleton / viewer Spin 状态占位 | template-stable |
| `PART_ErrorPresenter` | `ImagePreviewerCoverTheme` / `ImageViewerTheme` | 失败内容占位 | template-stable |
| `PART_PreviousButton` / `PART_NextButton` | `ImageViewerTheme` | 上一张 / 下一张导航 | template-stable |
| `PART_TitleLayout` / `PART_IconPresenter` | `ImagePreviewerTitleBarTheme` | dialog 标题与图标 | template-stable |
| `PART_CloseButton` | `ImagePreviewerOverlayHostTheme` | overlay 关闭入口 | template-stable |

| Part | AtomUI 节点 | 职责 | 相关 API | 相关 Token | 稳定性 |
| --- | --- | --- | --- | --- | --- |
| `root` | `AbstractImagePreviewer` | 归一 items、current、open state、宿主和加载策略 | `ItemsSource`、`CurrentIndex`、`IsOpen`、`PreloadCount` | SharedToken、ImagePreviewerToken | public |
| `cover` | `ImagePreviewer` / `PART_CoverItemsControl` | 展示单封面或 group 缩略图及 loading/error 状态 | Cover size/state、`ItemsPanel` | Cover size、mask、radius Token | public/template-stable |
| `host` | `ImagePreviewerDialog` / `ImagePreviewerOverlayHost` | 承载 Desktop window 或 Browser overlay | dialog、modal、topmost、title、motion API | Window、overlay、motion Token | internal-observable |
| `viewer` | `ImageViewer` / `PART_ImageViewerScene` | 当前项导航、fit、拖拽、缩放和旋转 | interaction、scale、CurrentIndex | Viewer background、toolbar Token | internal-observable |
| `renderer` | `PART_ImageRenderer` | 只渲染 entry 已持有的 `IImage` | current load state | 无独立加载 Token | template-stable |
| `loading` | `PART_LoadingPresenter` | 呈现 Skeleton 或 Spin，不拥有请求 | LoadingContent/Template | Loading、Skeleton、Spin Token | template-stable |
| `error` | `PART_ErrorPresenter` | 呈现最终失败内容，不改变状态机 | ErrorContent/Template | Error semantic Token | template-stable |

Semantic Part 公开契约（`root`、`image`、`cover` 与 `popup.*` 分组）见 [Semantic Parts](#semantic-parts) 章节。`PART_*`
名称继续只服务控件代码查找，与 Semantic Part 承担不同职责。

## 4. 行为与状态模型

### 4.1 内容身份与来源验证

内部身份链固定为：

```text
ImageSourceKey --validate--> ImageSourceVersion --maps-to--> ImageContentId
ImageContentId + ImageDecodeSpec ---------------------------> ImageDecodeKey
```

| 术语 | 定义 | 是否包含解码尺寸 |
| --- | --- | --- |
| `ImageSourceKey` | 规范来源地址或 identity、representation header 摘要、Variant、partition 摘要和 reader contract | 否 |
| `ImageSourceVersion` | 某次来源验证观察到的版本令牌 | 否 |
| `ImageContentId` | 对读取到的精确编码字节计算的 SHA-256 内容身份 | 否 |
| `ImageDecodeSpec` | codec、安全策略、目标物理像素、颜色和方向等影响输出的参数 | 按 codec 决定 |
| `ImageDecodeKey` | partition、ContentId 与 DecodeSpec 组成的解码身份 | 按 codec 决定 |
| `ImageSourceSnapshot` | SourceKey、SourceVersion、ContentId、freshness/validator 和提交 generation 的不可变映射 | 否 |

`Timeout`、Priority、progress callback、CacheRead 和 CacheStorage 不进入内容身份。它们只控制单个 waiter 的执行方式。

来源验证策略：

| Source | SourceVersion 或验证契约 | 跨请求来源映射 |
| --- | --- | --- |
| File | 已打开句柄的 file identity、长度和 modify/change stamp；严格模式读取哈希 | Desktop 可持久化 |
| HTTP | FreshUntil、ETag、Last-Modified 和 Vary | 服从响应缓存指令 |
| Asset | 当前 Application build/resource identity 内不可变 | 可持久化 |
| StorageFile | 显式 revision，或平台可观察 basic properties | 仅稳定 identity/revision 可持久化 |
| Bytes | 防御性副本直接计算 ContentId | 不建立来源持久映射 |
| Stream | 显式 identity + revision；否则每次打开并计算 ContentId | 仅稳定 identity/revision 可持久化 |
| Borrowed image | 对象 identity；不进入 encoded/decoded cache | 不持久化 |

File 的 metadata probe 和正文读取基于同一个已打开句柄，避免路径探测与正文读取之间的替换竞态。HTTP fresh snapshot 可直接
复用；过期或 `no-cache` snapshot 必须重验证，优先使用 ETag，其次 Last-Modified；`304` 延续原 ContentId，`200` 对新正文
重新计算 ContentId；`no-store` 不保留 source、encoded 或 decoded 条目。

调用方为 Stream 或 Storage 提供 revision，即承诺 revision 不变时内容不变；无法作出该承诺时必须省略 revision，让 loader
重新读取并以 ContentId 判断内容复用。

### 4.2 集合与双通道状态

`ItemsSource` 物化为控件内部的 `ImagePreviewEntry` 集合。每个 entry 拥有两个互相隔离的通道：

| 通道 | 内容 | 状态与资源 |
| --- | --- | --- |
| Full | 预览完整图 | FullState/Error/Progress、generation、waiter CTS、物理像素 bucket、`ImageLoadResult` lease |
| Thumbnail | 页面封面 | ThumbnailState/Error/Progress、generation、waiter CTS、独立 bucket 与 lease |

集合支持 enumerable replacement，以及 `INotifyCollectionChanged` 的 Add、Remove、Move、Replace 和 Reset。未变的 immutable item
可以复用 entry 与现有 lease；被移除或替换的 entry 立即取消并 dispose。detach 期间解除集合订阅，reattach 时重新物化当前集合，
补齐离线变更。

集合动作只影响 Previewer 自己的 entry、waiter 和 lease，绝不调用全局或分区 cache clear：

| 集合或宿主动作 | Previewer 行为 | Application cache |
| --- | --- | --- |
| Add | 创建 entry，按 open/cover/preload 状态请求 | 正常共享 |
| Remove / Replace | 取消并 dispose 旧 entry | 不清理 |
| Move | 移动 entry，保留其通道与 lease | 不清理 |
| Reset / Clear | 复用仍存在的 item，其余 entry 全部 dispose | 不清理 |
| ItemsSource replacement | 重新物化，释放不再使用的 entry | 不清理 |
| host close | 释放 Full waiter/lease，恢复关闭态 Thumbnail 策略 | 不清理 |
| detach | 释放 Full/Thumbnail waiter/lease 并退订集合 | 不清理 |

缓存正确性不依赖集合 Clear。相同路径的文件被替换后，`ValidateSource` 产生新的 SourceVersion；新字节产生新的 ContentId，进而
产生新的 DecodeKey。旧 decoded entry 不能作为新内容命中，只能保留为受预算约束的 LRU 条目直至驱逐。

### 4.3 加载优先级、尺寸与预加载

| 场景 | Source 顺序 | `ImageRequestPriority` | 默认 CacheRead |
| --- | --- | --- | --- |
| 当前完整图 | `Source -> FallbackSource` | `Critical` | item options；缺省为 `ValidateSource` |
| 单封面或 group 缩略图 | `ThumbnailSource -> Source -> FallbackSource` | `High` | item options |
| 相邻完整图预加载 | `Source -> FallbackSource` | `Preload` | item options |

打开态加载当前完整图和 `PreloadCount` 邻近窗口；离开窗口的未完成 Full waiter 被取消。邻项按与 CurrentIndex 的距离从近到远
提交。关闭宿主时释放所有 Full lease，再恢复关闭态封面请求；Thumbnail lease 保留供页面封面继续显示。

完整图解码目标来自 TopLevel ClientSize，封面目标来自 CoverWidth/CoverHeight 或实际 Bounds，均乘 render scaling 并向上量化
到 16 px bucket。Full 与 Thumbnail 分别按 bucket 保持请求幂等；布局、窗口尺寸或 DPI 变化产生更合适的 bucket 时保留当前图片并
异步升级，成功后原子替换。明确得到 0 x 0 时不发起无意义的封面请求。SVG 等尺寸无关 codec 不把显示尺寸写入 DecodeSpec。

### 4.4 Fallback、Reload 与结果事件

Fallback 是每个 item、每个通道的局部策略。一个 item 失败不能删除其他 item、替换 ItemsSource 或改变 CurrentIndex。取消不触发
fallback，也不提交 Failed。

`ReloadCurrent()`、`ReloadItem(index)` 和 `ReloadCover()` 使用 item 的 RequestOptions 副本，只把目标通道本次请求的
`CacheRead` 覆盖为 `RefreshSource`。调用方 options 不被修改；另一个通道和其他 item 的 generation、waiter、state 与 lease
不受影响。

只有当前 Full entry 从非 Loaded 进入 Loaded 时触发 `ImageOpened`，从非 Failed 进入 Failed 时触发 `ImageFailed`。预加载项和
封面状态不冒充当前项事件。

加载结果把“从哪里取得”和“来源是否已验证”分开表达：

```csharp
public enum ImageLoadOrigin
{
    Borrowed,
    DecodedMemory,
    EncodedMemory,
    Persistent,
    Network,
    Local
}

public enum ImageSourceValidation
{
    NotRequired,
    Current,
    Revalidated,
    Unverified
}
```

成功结果至少包含 `Image`、`ContentId`、`Origin`、`SourceValidation`、原始/解码尺寸、media type 和阶段耗时。borrowed image 的
ContentId 为空，Origin 为 `Borrowed`，SourceValidation 为 `NotRequired`。结果 lease 独立于 cache membership；条目被驱逐时，
仍被控件持有的 image 在最后一个 lease 释放前保持有效。

### 4.5 切换显示、标题与宿主

`ImageSwitchMode` 决定目标图片切换时是否保留上一张图片。`Immediate` 在目标没有可显示图片时于同一 UI 更新周期清空旧图，并把
Idle/Loading 的待完成目标投影为 loading；`WaitForLoaded` 在有效保留帧存在时持续显示旧图，目标加载状态仍保持为 true，但 loading
presenter 由 `:loading:not(:has-image)` 门控而不覆盖旧图。失败呈现、事件时序、请求与租约语义不因模式变化；运行时切换模式立即重算
预览窗口与单封面状态。
完整显示矩阵和保留帧生命周期见 [ImagePreviewer 切换显示设计](switch-display-design.md)。

标题优先级固定为：

```text
宿主显式 Window.Title / PreviewTitle
  > ImagePreviewItem.Title
  > IImagePreviewTitleResolver
  > 空标题
```

默认 resolver 返回 `Item.Source.DisplayName`。resolver 上下文只包含 immutable item、显示用 CurrentIndex 和 Count，不发起 I/O。
`PreviewTitleIcon` 只显示在预览标题栏，不投射到普通 Window icon。

Desktop 支持 native window 时使用 `ImagePreviewerDialog`；Browser 等无 native window 平台使用
`ImagePreviewerOverlayHost`。两种宿主共享 ItemsSource、CurrentIndex TwoWay、交互、占位、动效、加载和关闭语义。

预览交互先按视口宽度和视口高度的 70% 计算完整可见的基础图片尺寸，再以 `Scale=1` 作为用户变换起点。`ImageMinScale`、
`ImageMaxScale` 和 `ImageScaleStep` 只约束用户变换比例；Dialog 与 OverlayHost 不复制缩放或拖拽算法，统一由 ImageViewer 维护
`X/Y`、`Scale`、`Rotate`、`FlipX` 和 `FlipY`。完整公式、输入语义、解码升级和验证边界见
[ImagePreviewer 预览变换设计](preview-transform-design.md)。

`ImageMinScale="0.1"` 允许缩小到初始适配尺寸的 10%；它不影响打开时的完整适配。`IsWheelZoomEnabled="False"` 将滚轮交回宿主，工具栏、双击和触摸缩放保持可用。切图和 reset 清空平移、比例、旋转与翻转；关闭后重新打开使用同一初始状态。


## 5. 视觉与主题模型

稳定主题节点包括：

- `PART_CoverItemsControl`：group 封面集合；
- `PART_ImageViewerScene`、`PART_ImageRenderer`：预览坐标空间与图片 renderer；
- `PART_LoadingPresenter`、`PART_ErrorPresenter`：封面和 viewer 的状态占位；
- `PART_PreviousButton`、`PART_NextButton` 与 toolbar 操作按钮；
- `PART_TitleLayout`、`PART_IconPresenter`：dialog 标题与图标；
- `PART_CloseButton`：overlay 关闭入口。

Renderer 只消费 entry 已提交的 `IImage`，不得自行打开 Source 或访问 loader/cache。Loading 时封面使用稳定尺寸 Skeleton，viewer
使用居中 Spin；Failed 时使用本地化默认错误内容或用户模板。viewer 的加载指示器由 `:loading:not(:has-image)` 门控，只在无可
显示图片时呈现。Loading/error 自定义模板只替换内容，不能拥有请求、entry 或结果 lease。

预览变换不改变 `popup.body`、`popup.footer`、`popup.actions` 和 `PART_ImageRenderer` 的主题语义；基础适配与用户变换共享
`PART_ImageViewerScene` 坐标空间。Token 的来源、计算和消费位置见 [ImagePreviewer Token 设计](token.md)。

## Semantic Parts

ImagePreviewer 家族公开 8 个 Semantic Part：`root`、`image`、`cover`、`popup.root`、`popup.mask`、`popup.body`、
`popup.footer`、`popup.actions`，由 `ImagePreviewer`（单封面入口）与 `ImageGroupPreviewer`（多封面入口）
两个 public owner 共同声明。`root` 为控件根；`image` 为封面图片元素；`cover` 为封面悬浮提示层；`popup.root` 为预览宿主
容器根；`popup.mask` 为 Overlay 宿主的半透明遮罩层（仅 Overlay 宿主）；`popup.body` 为居中图片区；`popup.footer` 为底部
操作区；`popup.actions` 为 footer 内操作按钮组。`popup.*` 属于独立宿主
部件：预览宿主由 `OpenDialog()` 运行时创建并经 logical parent 挂入 owner，随宿主打开存在、关闭销毁。

Part 命名与 SelectorRoute 与上游 Image 控件的 Semantic DOM 一一对齐。完整 Part 表、逐 Part 说明、Selector 用法、数量语义、
定制边界与兼容性见 [ImagePreviewer Semantic Part 契约](semantic-part.md)；marker 与宿主/模板节点的映射见
[ImagePreviewer 桌面版实现原理](implementation.md)；系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 6. 控件家族或集成关系

`AbstractImagePreviewer` 定义共享 item、current、loading、interaction 和 host contract。`ImagePreviewer` 增加单封面投影，
`ImageGroupPreviewer` 增加多封面 ItemsPanel。`ImagePreviewerDialog` 与 `ImagePreviewerOverlayHost` 是同一 open state 的两种宿主，
`ImageViewer` 统一维护预览变换，`ImagePreviewRenderer` 复用相同 entry 并只负责绘制。

应用级图片加载系统按以下稳定边界服务 Previewer：

```text
ImagePreviewer / ImageGroupPreviewer
  -> ImagePreviewEntry waiter + result lease
     -> IImageLoader
        -> ImageSourceSnapshotIndex
        -> ImageEncodedContentStore
        -> ImageDecodedContentStore
        -> source/decode single-flight + bounded scheduler
```

`ImageSourceSnapshotIndex` 把来源映射到当前 ContentId；encoded store 只保存经过验证的精确字节；decoded store 按 ContentId 和
DecodeSpec 保存解码结果。多个来源只有在同一 CachePartition 中内容摘要相同且 DecodeSpec 相同时才能共享 decoded entry。

持久缓存使用稳定目录 `image-cache/`。格式版本只写入 manifest 的内部 `formatRevision`，不进入目录、类型或公共 API 名称；
格式不匹配时重建缓存，不维护迁移分支。

## 7. 兼容性不变量

- `ImageSource` 来源层次、`ImageCacheReadPolicy` 和 `ImageCacheStoragePolicy` 是唯一公共模型，不保留旧名称或兼容 shim。
- 普通加载默认 `CacheRead=ValidateSource`、`CacheStorage=MemoryAndDisk`；Reload 只对单次目标通道使用 `RefreshSource`。
- 来源变化不依赖集合 Clear、控件重建或手工 cache clear 才能被识别。
- Previewer 的 Add/Remove/Replace/Move/Reset/Clear、host close 和 detach 不能清除 Application cache。
- 关闭 dialog/overlay 释放宿主 binding、订阅、logical parent 和全部 Full lease；detach 释放 Full/Thumbnail waiter 与 lease。
- reattach：重新物化集合并按 IsOpen 选择当前加载策略。
- TopLevel resize 或 render scaling 变化重新计算物理像素 bucket；相同 bucket 不重复读取，不同 bucket 异步升级。
- source replacement、collection remove/reset、旧 generation 和已关闭 host 都不能回写当前 entry。
- Full 与 Thumbnail 通道保持取消、状态、错误、进度、尺寸和 lease 隔离。
- 当前项和封面按显示索引 clamp，但不静默改写外部 TwoWay CurrentIndex。
- 基础图片尺寸同时受视口宽度和视口高度 70% 约束；`Scale=1` 表示基础适配尺寸，用户比例由统一变换状态管理。
- 缩放、滚轮、拖拽、旋转、翻转、reset、切图和 resize 共用同一边界修正算法；图片小于视口时自动回到中心。
- Dialog 与 OverlayHost 不各自维护变换算法，两个宿主对同一输入产生相同变换状态。
- Template Part、伪类、loading/error 门控、标题、导航和 Token 视觉语义保持稳定。
- 不允许同步 I/O、固定延迟、反射发现 reader/codec/serializer，或在 Previewer 内创建 `HttpClient`、cache 或 scheduler。
- SourceSnapshot、encoded content 和 decoded content 的共享范围都受 CachePartition 限制；安全分区之间不共享命中或诊断。
- borrowed `IImage` 始终由调用方拥有，任何 loader/cache/control 生命周期都不能 dispose 它。
- Semantic Part：删除或重命名 Part、修改 SelectorRoute 命中范围、收窄 `ContractType` 都属于破坏性变更；`popup.*` 部件仅宿主
  打开期间存在，关闭后不残留任何 marker 节点；内置主题不使用 `.semantic-*` selector 实现默认视觉；`.semantic-scope-*`
  路由锚点不是公开 Part，不进入兼容承诺；native dialog 的窗口 chrome（标题栏、caption 按钮、窗口边框）不属于任何 Part；
  宿主必须保持挂入 owner 的 logical parent 链，popup 部件的生成 Selector 依赖该链命中 overlay 宿主（与 owner 同 TopLevel）；
  native dialog 是独立 TopLevel，owner 作用域样式不跨窗口级联，预览视觉经 host 契约定制。
- 宿主分层与上游 DOM 对齐：overlay 宿主模板根 Panel 只承担 `popup.root` 容器职责、不带背景，遮罩背景必须由独立的
  `popup.mask` 子元素承担；`popup.mask` 仅 Overlay 宿主存在（`Optional`），native dialog 不物化该部件。
- 遮罩点击关闭（上游 `maskClosable=true` 默认）当前未在 overlay 宿主实现，关闭经 overlay 宿主内嵌关闭按钮（非语义部件）；Part 契约只承诺样式命中，
  不承诺该行为，属行为对齐的既有差异。

## 8. 专项模型

### 8.1 三层缓存与所有权

| 组件 | Key | Value | 所有权 |
| --- | --- | --- | --- |
| `ImageSourceSnapshotIndex` | `ImageSourceKey` | `ImageSourceVersion -> ImageContentId`、freshness/validator、commit generation | Application |
| `ImageEncodedContentStore` | partition hash + `ImageContentId` | validated immutable bytes + content metadata | Application |
| `ImageDecodedContentStore` | partition hash + `ImageContentId` + `ImageDecodeSpec` | decoded entry | Application |

SourceSnapshot 的 freshness、ETag、路径版本等来源 metadata 不进入 encoded store。encoded metadata 只描述字节自身；decoded store
只以实际内容和解码输出参数寻址。cache membership、在途 operation reference 和 Previewer result lease 是三个独立持有关系。
LRU 驱逐先移除 membership，活动 operation 或 lease 归零后再释放底层资源。

### 8.2 请求顺序与并发

每个请求固定按以下顺序执行：

1. 快照 Source、headers、partition、Variant、timeout、像素目标和策略。
2. 构造不含 decode 参数的 SourceKey。
3. 按 CacheRead 取得、验证或绕过 SourceSnapshot。
4. 取得 ContentId；内容缺失且允许访问来源时读取一次并流式计算摘要。
5. 完成字节上限、格式和安全验证后提交 immutable encoded content 与最新 source snapshot。
6. 选择唯一 codec，构造 DecodeKey，然后才查询 decoded cache。
7. decoded miss 加入有界 decode scheduler；成功后按 CacheStorage 与 cache epoch 提交。
8. 为每个 waiter 创建独立结果 lease；Previewer 回到 UI dispatcher 后再次检查 entry generation。

source resolution 以 SourceKey 和兼容读取策略做 single-flight，decode 以 DecodeKey 做 single-flight。每个 waiter 拥有独立取消、timeout、
progress 和 completion；取消一个 waiter 不影响其他 waiter，最后一个 waiter 离开才取消共享操作。每个 SourceKey 的单调 commit
generation 防止较早请求晚完成后回滚来源映射。全局或分区 cache clear 递增 epoch，阻止清理前启动的操作回填已清理 scope。

所有 cancellation、dispose、progress 和 completion callback 都在 coordinator/cache lock 外执行。取消不是图片失败，不触发
fallback 或 `ImageFailed`。

### 8.3 结构性能目标

| 场景 | 来源成本 | hash/验证 | decode |
| --- | --- | --- | --- |
| 未变化 File，Metadata | 1 次句柄 metadata probe，0 次正文读取 | 0 次内容 hash | 0 次 |
| File 版本变化 | 1 次正文读取 | 流中 1 次 hash + 1 次内容验证 | 每个 DecodeSpec 至多 1 次 |
| File，ContentHash | 每次 1 次正文读取 | 每次 1 次 hash | ContentId 相同时 0 次重复 decode |
| fresh HTTP snapshot | 0 次网络 | 0 次正文 hash | 0 次 |
| HTTP 304 | 1 次条件请求，0 次正文下载 | 复用 ContentId | 0 次 |
| 相同 Source 的 N 个并发 waiter | 1 次来源 operation | 1 次 | 每个 DecodeKey 1 次 |
| Previewer Clear/Reset | 0 次 cache scan | 0 次 | 只释放控件 lease |

性能结论分别测量 cold first load、warm validated load、content-addressed decoded hit、File replace、HTTP 304、多 Previewer 并发和
大集合 Clear；结构目标不替代 timing 基准。

### 8.4 平台、安全与 AOT

- Desktop 支持 File 的 Metadata/ContentHash 和可选 persistent cache。
- Browser 不模拟 File 或 disk cache；缺失能力返回确定错误或启动 diagnostics，HTTP、Asset、Storage、Bytes 和 Stream 仍使用
  相同 ContentId/DecodeKey 模型。
- reader、validator、codec 和 metadata serializer 使用静态已知类型或生成式注册；不扫描程序集，不使用 runtime reflection
  serializer。
- SourceKey、partition 和持久文件名只使用不可逆摘要；默认 diagnostics 不记录完整 URI、路径、header 或 partition 原值。
- encoded content 只有通过当前安全策略验证后才能进入 memory/disk store；codec 只接收已验证内容。
- SVG 的 DecodeSpec 包含 codec 与安全策略 revision，显示尺寸不进入尺寸无关 DecodeKey。
- hash、验证和 codec 工作不占用 UI dispatcher；Avalonia image 创建与释放遵守合法线程边界。
- 内置 source、reader、store、validator、codec 和 Application-owned service 由 Package Core 的真实静态调用保留，控件资源遵循 [TypeMap 注册契约](../../../../architecture/foundations/aot-and-trimming.md)。

## 9. 文档导航与验证策略

- [ImagePreviewer 桌面版实现原理](implementation.md)
- [ImagePreviewer Semantic Part 契约](semantic-part.md)
- [ImagePreviewer Token 设计](token.md)
- [ImagePreviewer 切换显示设计](switch-display-design.md)
- [ImagePreviewer Changelog](changelog.md)
- [ImagePreviewer 预览变换设计](preview-transform-design.md)
- [统一图片加载系统](../../../../architecture/systems/image-loading/overview.md)
- [AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)

验证分层：

- Public contract：Source 类型封闭性、默认 CacheRead/CacheStorage、File validation、result origin/validation 和错误语义。
- 来源与缓存：File 替换、HTTP fresh/304/200/no-store/Vary、Stream/Storage revision、Bytes、borrowed image 和分区隔离。
- 并发与租约：两级 single-flight、commit generation、waiter-local cancellation、cache epoch、LRU 与 lease 延迟释放。
- 控件行为：collection replacement/Add/Remove/Move/Replace/Reset/Clear、Full/Thumbnail 隔离、fallback、Reload 三个入口、尺寸升级、
  host close、detach/reattach 和旧结果拒绝。
- 显示与主题：两种 ImageSwitchMode 的显示矩阵、基础适配尺寸、统一变换状态、鼠标中心缩放、拖拽 rebound、稳定 Template Part、伪类、loading/error 门控、标题、Light/Dark 和宿主一致性。
- 发布：Gallery public API 示例、Shared/Desktop targeted tests、Browser managed publish、Desktop NativeAOT publish 与启动 smoke。
- Semantic Part：两个 owner 的 descriptor 断言（名称、Selector、SelectorRoute、ContractType、Cardinality 与 cross-root/runtime
  标志）、全部内置主题的 marker 完整性、生成 Semantic Style 在 overlay 宿主的命中（native dialog 内 marker 完整但 owner
  作用域样式不跨 TopLevel 级联）、open-close-reopen 生命周期、多 owner 实例隔离、Gallery 语义预览（`root`/`image`/`cover`
  高亮，`popup.*` 仅列出描述）和 NativeAOT publish。
- Gallery API 与 ShowCase 只能使用 `ItemsSource` 和 `ImagePreviewItem`。
