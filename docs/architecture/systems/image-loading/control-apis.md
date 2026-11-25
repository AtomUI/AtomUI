# 统一图片加载控件 API

> 状态：当前架构。本文定义 AsyncImage、Avatar 与 ImagePreviewer public surface。底层 Source、Options、Loader、Result 与 Error 由
> [图片加载公共契约](public-contracts.md)拥有。

`AsyncImage` 与 Avatar 位于 `AtomUI.Controls` namespace；Previewer 类型位于 `AtomUI.Desktop.Controls`。
本文件是跨控件图片加载 API 的事实来源；Avatar 与 ImagePreviewer Control 文档负责描述各自当前的完整控件契约。

## 单图片控件统一面

`AsyncImage` 和 Avatar 共同实现：

```csharp
public interface IImageLoadControl
{
    ImageSource? Source { get; set; }
    ImageSource? FallbackSource { get; set; }
    ImageRequestOptions? RequestOptions { get; set; }

    ImageLoadState LoadState { get; }
    ImageLoadError? LoadError { get; }
    ImageLoadProgress? LoadProgress { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }
    bool IsFailed { get; }

    event EventHandler<ImageOpenedEventArgs>? ImageOpened;
    event EventHandler<ImageFailedEventArgs>? ImageFailed;

    void Reload();
}
```

`Source == null` 时状态为 `Idle`。主来源失败且存在 `FallbackSource` 时，控件保持 `Loading` 并尝试 fallback；
fallback 成功后状态为 `Loaded`，`ImageOpenedEventArgs.IsFallback == true`。只有最终没有可显示图片时才进入
`Failed` 并触发 `ImageFailed`。Source 变化会清空上一轮 `LoadError` 和进度。`LoadProgress` 只在 `Loading` 时有值，进入
Idle/Loaded/Failed 后清空；主来源错误在 fallback 尝试期间不发布为最终 `LoadError`。

`FallbackSource` 与主 Source 规范化身份相同时不重复请求，直接使用主失败作为最终失败。`Reload()` 在 Source 为空时保持
Idle 并 no-op；加载中调用会创建新 generation。状态映射到统一 `:loading`、`:loaded`、`:failed` pseudo-class，三者互斥；
成功显示 fallback 时额外设置 `:fallback`。Idle 不设置上述伪类。

来源未变化的 Reload/尺寸桶替换可以在 Loading 期间继续显示旧租约，避免短暂空白；新结果成功后原子替换，最终失败时释放
旧租约并进入 Failed，保证 `LoadState`、可见内容和 error template 不出现“显示成功图但状态失败”的双重语义。

## AsyncImage

`AsyncImage` 位于 `AtomUI.Controls`，是通用异步图片显示控件：

```csharp
public class AsyncImage : TemplatedControl, IImageLoadControl
{
    public ImageSource? Source { get; set; }
    public ImageSource? FallbackSource { get; set; }
    public ImageRequestOptions? RequestOptions { get; set; }

    public Stretch Stretch { get; set; } = Stretch.Uniform;
    public StretchDirection StretchDirection { get; set; } = StretchDirection.Both;
    public ImageDecodeMode DecodeMode { get; set; } = ImageDecodeMode.Auto;
    public int DecodePixelWidth { get; set; }
    public int DecodePixelHeight { get; set; }
    public ImageRequestPriority Priority { get; set; } = ImageRequestPriority.Normal;
    public bool IsLazy { get; set; }

    public object? LoadingContent { get; set; }
    public IDataTemplate? LoadingContentTemplate { get; set; }
    public object? ErrorContent { get; set; }
    public IDataTemplate? ErrorContentTemplate { get; set; }

    public ImageLoadState LoadState { get; }
    public ImageLoadError? LoadError { get; }
    public ImageLoadProgress? LoadProgress { get; }
    public bool IsLoading { get; }
    public bool IsLoaded { get; }
    public bool IsFailed { get; }

    public void Reload();
}
```

`Auto` 根据有效 arranged width、有限高度约束与 TopLevel render scaling 计算物理像素目标，并使用稳定尺寸桶抑制微小布局变化；无界内容布局只使用宽度桶，避免加载结果的固有高度反馈到父布局；
`Stretch` 为 `Uniform`/`None` 时目标框是等比缩放上限，解码结果落在框内；为 `Fill`/`UniformToFill` 时图片加载后会撑满有限的
高度边界，目标高度在首个请求就取这条边界（依次为 `Height`、已含 `MaxHeight` 与父级槽位的测量可用高度、`MaxHeight`），不取加载前
占位内容的排列高度，因此内容自适应高度的布局只解码一次；没有任何高度边界时高度随图片宽高比变化，只按宽度解码。codec 按覆盖语义
取宽高缩放比中的较大者，解码结果覆盖整个框且不放大原图，整宽限高的图片不会先按高度缩小再被横向拉伸。覆盖且宽高均有边界时，若 Skia 截断推导出的另一轴会低于框的该边（如 257×127 覆盖 16×16 按宽度解码得 32×15），改按另一轴解码。运行时切换 `Stretch` 按新目标重新请求。尺寸桶上限是 `int` 范围内
最大的 16 像素整倍数，极大的有限 `Height`/`MaxHeight` 按上限处理，不会在 attach 或 Arrange 时溢出。Desktop 内置控件中
ImagePreviewer 使用显式解码尺寸，不受覆盖语义影响；直接使用 `AsyncImage` 并设置 `Fill`/`UniformToFill` 的界面，解码像素增大到
覆盖显示框，内存占用随之按显示尺寸计算；
`Original` 请求原始尺寸；`Explicit` 使用 `DecodePixelWidth`/`DecodePixelHeight` 形成保持宽高比的最大边界，至少一边
必须大于零；明确配置 0 x 0 时不启动 loader，并以 typed `InvalidSource` 进入 Failed。`Reload()` 对当前有效来源执行一次
`CacheRead=ImageCacheReadPolicy.RefreshSource`，不永久修改 `RequestOptions`。

`IsLazy` 默认 `false`，加载路径与此前一致。设为 `true` 后，控件在 attach 时先关闭请求门，只有收到有效视口且与视口相交、
有效可见时才开门并启动请求；显式零 `Width`/`Height`、折叠父级或视口之外的图片不发起请求。自动尺寸的轴测得 0 时（例如垂直
`StackPanel` 中只设宽度且没有占位内容）按其位置判断是否落在视口内；尺寸变化时用最近的有效视口重新判断，避免从零尺寸增长时
视口矩形不变而一直不开门。首次相交后取消视口订阅，已滚出视口的图片保留已有结果。门关闭期间 `LoadState` 为 `Idle`，默认
主题在 `IsLazy` 且 `Idle` 时显示 `LoadingContent`，使占位内容能撑开自适应布局；消费者仍应把“`Idle` 且 `Source` 非空”视为等待中。
运行时切换 `IsLazy` 时，空闲图片按新值重新设门，正在加载的请求不受影响，关闭 `IsLazy` 时开门。
AtomUI 内部提供 `ReleaseLoad()`（`internal`，无 Desktop 调用方）：保持挂树状态取消请求并释放租约，随后重新设门，下一次视口
相交或 `Source` 变化时再加载；未挂树时为空操作。

`LoadingContent` 只在当前没有可显示租约时替代图片 presenter；同来源替换仍显示旧图并保持 `:loading`。最终失败释放图片并
显示 `ErrorContent`。template reapply 不重启身份、尺寸和 options 均未变化的请求，也不泄漏旧 part 订阅。

`Source` 对 raster 和受限静态 SVG 使用同一契约。SVG 保持矢量结果，`Auto`/`Original`/`Explicit` 的尺寸仍用于控件请求身份和
raster 解码，但不会让 SVG codec 预栅格化或为每个尺寸生成不同 decoded entry。

## Avatar

Avatar 不再区分 URL、Bitmap 和本地 SVG 属性：

```csharp
public abstract class AbstractAvatar : TemplatedControl, IImageLoadControl
{
    public ImageSource? Source { get; set; }
    public ImageSource? FallbackSource { get; set; }
    public ImageRequestOptions? RequestOptions { get; set; }

    public string? Text { get; set; }
    public PathIcon? Icon { get; set; }

    public ImageLoadState LoadState { get; }
    public ImageLoadError? LoadError { get; }
    public ImageLoadProgress? LoadProgress { get; }
    public bool IsLoading { get; }
    public bool IsLoaded { get; }
    public bool IsFailed { get; }

    public void Reload();
}
```

Avatar 内容优先级固定为：成功的 `Source`、成功的 `FallbackSource`、`Text`、`Icon`。加载中仍显示 Text/Icon
fallback，不展示空白图片 presenter。Avatar 始终按有效头像尺寸乘 render scaling 请求目标尺寸，不公开解码尺寸属性；
`Stretch` 为 `Fill`/`UniformToFill` 时同样按覆盖语义解码，非正方形图片铺满头像时不欠采样。
HTTP/HTTPS 与本地 SVG 直接进入同一 `Source`；loader 只接受通过统一静态 SVG 安全子集的内容。首次有效 Arrange 的头像尺寸
必须直接参与当轮请求，不能依赖旧 `Bounds`、额外 layout pass 或固定延迟才能启动网络加载。

## ImagePreviewer

Previewer 使用不可变配置项，而不是把加载状态写进调用方集合：

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

`Source` 是打开预览时的完整图片；`ThumbnailSource` 是可选封面来源，为空时使用 Source 的目标尺寸解码；
`FallbackSource` 只替代当前 item，不替代整个集合。内部 `ImagePreviewEntry` 持有状态、generation 和租约，不公开。
缩略图/cover 加载顺序为 `ThumbnailSource`（存在时）、同一 item 的 `Source` 缩略尺寸、`FallbackSource`；完整预览顺序为
`Source`、`FallbackSource`。因此失效的专用缩略图不会阻断仍可加载的完整来源。

`Source`、`ThumbnailSource` 与 `FallbackSource` 均可使用受限静态 SVG。cover、current 和 preload 复用同一矢量 decoded entry；
fit、zoom、rotate 和 viewport 尺寸变化不把 SVG 预栅格化成低分辨率 Bitmap，也不建立 Previewer 私有 SVG loader。

```csharp
public abstract class AbstractImagePreviewer : TemplatedControl
{
    public IEnumerable<ImagePreviewItem>? ItemsSource { get; set; }
    public int CurrentIndex { get; set; }
    public int CoverIndex { get; set; }
    public int PreloadCount { get; set; } = 1;

    public string? PreviewTitle { get; set; }
    public PathIcon? PreviewTitleIcon { get; set; }
    public IImagePreviewTitleResolver? PreviewTitleResolver { get; set; }

    public object? LoadingContent { get; set; }
    public IDataTemplate? LoadingContentTemplate { get; set; }
    public object? ErrorContent { get; set; }
    public IDataTemplate? ErrorContentTemplate { get; set; }

    public bool IsOpen { get; set; }
    public bool IsMotionEnabled { get; set; }
    public double CoverWidth { get; set; } = double.NaN;
    public double CoverHeight { get; set; } = double.NaN;
    public bool IsImageMovable { get; set; } = true;
    public double ImageScaleStep { get; set; } = 0.5;
    public double ImageMinScale { get; set; } = 1.0;
    public double ImageMaxScale { get; set; } = 50.0;
    public bool IsDialogModal { get; set; }
    public bool IsDialogTopmost { get; set; }

    public ImagePreviewItem? CurrentItem { get; }
    public ImageLoadState CurrentLoadState { get; }
    public ImageLoadError? CurrentLoadError { get; }
    public ImageLoadProgress? CurrentLoadProgress { get; }
    public bool IsCurrentLoading { get; }
    public bool IsCurrentLoaded { get; }
    public bool IsCurrentFailed { get; }

    public event EventHandler<ImagePreviewOpenedEventArgs>? ImageOpened;
    public event EventHandler<ImagePreviewFailedEventArgs>? ImageFailed;
    public event EventHandler? DialogOpened;
    public event EventHandler<CancelEventArgs>? DialogClosing;
    public event EventHandler? DialogClosed;

    public void ReloadCurrent();
    public void ReloadItem(int index);
    public void OpenDialog();
}
```

`ItemsSource` 实现 `INotifyCollectionChanged` 时必须增量同步；普通 enumerable 在属性替换时重新物化。`CurrentIndex`
保持 TwoWay binding 语义，宿主显示时可以 clamp，但不能把临时 clamp 值反写破坏调用方值。
Previewer 的 `ImageOpened`/`ImageFailed` 只对应当前完整图片的最终提交；cover 和 preload 通过各自状态观察，不产生伪装成
current item 的公共事件。

`ImagePreviewer` 的完整 cover loading surface 为：

```csharp
public class ImagePreviewer : AbstractImagePreviewer
{
    public object? CoverIndicatorContent { get; set; }
    public IDataTemplate? CoverIndicatorContentTemplate { get; set; }
    public bool IsShowCoverMask { get; set; } = true;

    public ImageLoadState CoverLoadState { get; }
    public ImageLoadError? CoverLoadError { get; }
    public ImageLoadProgress? CoverLoadProgress { get; }
    public bool IsCoverLoading { get; }
    public bool IsCoverLoaded { get; }
    public bool IsCoverFailed { get; }

    public void ReloadCover();
}
```

`ImageGroupPreviewer` 只增加 `ItemsPanel`。Previewer 不再公开 `MaxConcurrentLoads`，全局并发由应用 loader 控制。保留的
title、dialog、motion、cover size 和 image interaction API 不参与 source 兼容；它们消费新的 entry/state，但名称和语义不因
loader 统一而另造第二套。

标题解析上下文改为：

```csharp
public readonly record struct ImagePreviewTitleResolveContext(
    ImagePreviewItem Item,
    int CurrentIndex,
    int Count);
```

标题优先级固定为宿主显式 Window title、非空 `PreviewTitle`、当前 `ImagePreviewItem.Title`、
`IImagePreviewTitleResolver`、空态。resolver 只在 item title 为空时调用，不发起 I/O 或反射读取业务对象。

```csharp
public interface IImagePreviewTitleResolver
{
    string? ResolveTitle(in ImagePreviewTitleResolveContext context);
}

public class ImageGroupPreviewer : AbstractImagePreviewer
{
    public ITemplate<Panel?> ItemsPanel { get; set; }
}
```

## 无兼容公共面

统一图片加载不提供 obsolete shim、类型转发或双写属性。以下旧 public surface 不属于当前 API：

```text
Avatar.Src
Avatar.BitmapSrc
Avatar.BitmapUrl
AbstractImagePreviewer.Source
AbstractImagePreviewer.Sources
AbstractImagePreviewer.FallbackSource
AbstractImagePreviewer.MaxConcurrentLoads
IImagePreviewSource
IImagePreviewSourceIdentity
UriImagePreviewSource
StreamImagePreviewSource
ImageSourceUri
ImageSourceUriKind
IImageSourceLoader
DefaultImageSourceLoader
LoadedImageSource
LoadedImageSourceType
ImagePreviewItemState
```

`ImagePreviewerCover`、`ImagePreviewRenderer`、`ImagePreviewerDialog`、`ImagePreviewerOverlayHost` 和运行时 entry 类型
都是 internal template/runtime 组件，不进入新 public surface。
