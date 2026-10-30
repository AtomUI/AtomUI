using System.Collections.ObjectModel;
using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using SkiaSharp;

namespace AtomUIGallery.ShowCases.ImagePreviewer;

public class ImagePreviewerViewModel : ReactiveObject, IRoutableViewModel
{
    private const int RapidSwitchIntervalMs      = 200;
    private const int RapidSwitchLoadDelayMs     = 300;
    private const int RapidSwitchMaxItems        = 40;
    private const string ReplacementFirstAsset   = "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png";
    private const string ReplacementSecondAsset  = "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/Fallback.png";

    private static readonly string[] RapidSwitchAssets =
    [
        "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png",
        "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/4.webp",
        "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/5.webp",
        "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/6.webp"
    ];

    private static readonly object RapidAssetCacheGate = new();
    private static readonly byte[]?[] RapidAssetCache  = new byte[4][];

    public static EntityKey ID = "ImagePreviewer";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private IList<ImagePreviewItem>? _remoteImages;

    public IList<ImagePreviewItem>? RemoteImages
    {
        get => _remoteImages;
        set => this.RaiseAndSetIfChanged(ref _remoteImages, value);
    }

    private IList<ImagePreviewItem>? _defaultImages;

    public IList<ImagePreviewItem>? DefaultImages
    {
        get => _defaultImages;
        set => this.RaiseAndSetIfChanged(ref _defaultImages, value);
    }

    private IList<ImagePreviewItem>? _grayscaleImages;

    public IList<ImagePreviewItem>? GrayscaleImages
    {
        get => _grayscaleImages;
        set => this.RaiseAndSetIfChanged(ref _grayscaleImages, value);
    }

    private IList<ImagePreviewItem>? _twoImages;

    public IList<ImagePreviewItem>? TwoImages
    {
        get => _twoImages;
        set => this.RaiseAndSetIfChanged(ref _twoImages, value);
    }

    private IList<ImagePreviewItem>? _threeImages;

    public IList<ImagePreviewItem>? ThreeImages
    {
        get => _threeImages;
        set => this.RaiseAndSetIfChanged(ref _threeImages, value);
    }

    private IList<ImagePreviewItem>? _twentyRemoteImages;

    public IList<ImagePreviewItem>? TwentyRemoteImages
    {
        get => _twentyRemoteImages;
        set => this.RaiseAndSetIfChanged(ref _twentyRemoteImages, value);
    }

    private IList<ImagePreviewItem>? _fallbackImages;

    public IList<ImagePreviewItem>? FallbackImages
    {
        get => _fallbackImages;
        set => this.RaiseAndSetIfChanged(ref _fallbackImages, value);
    }

    private ObservableCollection<ImagePreviewItem>? _rapidImages;

    public ObservableCollection<ImagePreviewItem>? RapidImages
    {
        get => _rapidImages;
        set => this.RaiseAndSetIfChanged(ref _rapidImages, value);
    }

    private int _rapidCurrentIndex;

    public int RapidCurrentIndex
    {
        get => _rapidCurrentIndex;
        set => this.RaiseAndSetIfChanged(ref _rapidCurrentIndex, value);
    }

    private ImageSwitchMode _rapidSwitchMode = ImageSwitchMode.Immediate;

    public ImageSwitchMode RapidSwitchMode
    {
        get => _rapidSwitchMode;
        set => this.RaiseAndSetIfChanged(ref _rapidSwitchMode, value);
    }

    private bool _rapidWaitForLoaded;

    public bool RapidWaitForLoaded
    {
        get => _rapidWaitForLoaded;
        set
        {
            this.RaiseAndSetIfChanged(ref _rapidWaitForLoaded, value);
            RapidSwitchMode = value ? ImageSwitchMode.WaitForLoaded : ImageSwitchMode.Immediate;
        }
    }

    private bool _isRapidSwitchRunning;

    public bool IsRapidSwitchRunning
    {
        get => _isRapidSwitchRunning;
        private set => this.RaiseAndSetIfChanged(ref _isRapidSwitchRunning, value);
    }

    public ReactiveCommand<Unit, Unit> StartRapidSwitchCommand { get; }
    public ReactiveCommand<Unit, Unit> StopRapidSwitchCommand { get; }
    public ReactiveCommand<Unit, Unit> ReplaceSamePathCommand { get; }

    public bool IsFileReplacementAvailable => !OperatingSystem.IsBrowser();

    private ObservableCollection<ImagePreviewItem>? _samePathImages;

    public ObservableCollection<ImagePreviewItem>? SamePathImages
    {
        get => _samePathImages;
        set => this.RaiseAndSetIfChanged(ref _samePathImages, value);
    }

    private DispatcherTimer? _rapidSwitchTimer;
    private int _rapidSwitchCounter;
    private string? _replacementDirectory;
    private string? _replacementFilePath;
    private int _replacementAssetIndex;

    public ImagePreviewerViewModel(IScreen screen)
    {
        HostScreen            = screen;
        StartRapidSwitchCommand = ReactiveCommand.Create(HandleStartRapidSwitch);
        StopRapidSwitchCommand  = ReactiveCommand.Create(HandleStopRapidSwitch);
        ReplaceSamePathCommand  = ReactiveCommand.CreateFromTask(HandleReplaceSamePathAsync);
    }

    public void EnsurePreviewAssets()
    {
        RemoteImages =
        [
            CreateItem("https://zos.alipayobjects.com/rmsportal/jkjgkEfvpUPVyRjUImniVslZfWPnJuuZ.png")
        ];
        DefaultImages =
        [
            CreateItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
        ];
        GrayscaleImages =
        [
            CreateGrayscaleItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
        ];
        ThreeImages =
        [
            CreateItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/4.webp"),
            CreateItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/5.webp"),
            CreateItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/6.webp")
        ];
        TwoImages =
        [
            CreateItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/2.svg"),
            CreateItem("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/3.svg"),
        ];
        TwentyRemoteImages =
        [
            CreateItem("https://picsum.photos/id/20/600/400"),
            CreateItem("https://picsum.photos/id/21/600/400"),
            CreateItem("https://picsum.photos/id/22/600/400"),
            CreateItem("https://picsum.photos/id/23/600/400"),
            CreateItem("https://picsum.photos/id/24/600/400"),
            CreateItem("https://picsum.photos/id/25/600/400"),
            CreateItem("https://picsum.photos/id/26/600/400"),
            CreateItem("https://picsum.photos/id/27/600/400"),
            CreateItem("https://picsum.photos/id/28/600/400"),
            CreateItem("https://picsum.photos/id/29/600/400"),
            CreateItem("https://picsum.photos/id/30/600/400"),
            CreateItem("https://picsum.photos/id/31/600/400"),
            CreateItem("https://picsum.photos/id/32/600/400"),
            CreateItem("https://picsum.photos/id/33/600/400"),
            CreateItem("https://picsum.photos/id/34/600/400"),
            CreateItem("https://picsum.photos/id/35/600/400"),
            CreateItem("https://picsum.photos/id/36/600/400"),
            CreateItem("https://picsum.photos/id/37/600/400"),
            CreateItem("https://picsum.photos/id/38/600/400"),
            CreateItem("https://picsum.photos/id/39/600/400")
        ];
        FallbackImages =
        [
            new ImagePreviewItem(ImageSource.Parse("https://example.invalid/missing-image.png"))
            {
                FallbackSource = ImageSource.Parse(
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/Fallback.png")
            }
        ];
        RapidImages      = [CreateRapidItem(_rapidSwitchCounter++)];
        RapidCurrentIndex = 0;
        EnsureSamePathImage();
    }

    public void ClearPreviewAssets()
    {
        HandleStopRapidSwitch();
        RemoteImages       = null;
        DefaultImages      = null;
        GrayscaleImages    = null;
        ThreeImages        = null;
        TwoImages          = null;
        TwentyRemoteImages = null;
        FallbackImages     = null;
        RapidImages        = null;
        SamePathImages     = null;
        RapidCurrentIndex  = 0;
        CleanupReplacementDirectory();
    }

    private void HandleStartRapidSwitch()
    {
        if (IsRapidSwitchRunning)
        {
            return;
        }
        IsRapidSwitchRunning = true;
        _rapidSwitchTimer     = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(RapidSwitchIntervalMs) };
        _rapidSwitchTimer.Tick += (_, _) => AppendRapidItem();
        _rapidSwitchTimer.Start();
    }

    private void HandleStopRapidSwitch()
    {
        _rapidSwitchTimer?.Stop();
        _rapidSwitchTimer = null;
        IsRapidSwitchRunning = false;
    }

    private void AppendRapidItem()
    {
        var items = RapidImages;
        if (items is null)
        {
            return;
        }
        items.Add(CreateRapidItem(_rapidSwitchCounter++));
        if (items.Count > RapidSwitchMaxItems)
        {
            items.RemoveAt(0);
        }
        RapidCurrentIndex = items.Count - 1;
    }

    private void EnsureSamePathImage()
    {
        if (!IsFileReplacementAvailable)
        {
            SamePathImages = null;
            return;
        }
        CleanupReplacementDirectory();
        _replacementDirectory = Path.Combine(
            Path.GetTempPath(),
            $"atomui-image-previewer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_replacementDirectory);
        _replacementFilePath = Path.Combine(_replacementDirectory, "current-image");
        _replacementAssetIndex = 0;
        File.WriteAllBytes(_replacementFilePath, ReadAssetBytes(ReplacementFirstAsset));
        SamePathImages = [CreateSamePathItem()];
    }

    private async Task HandleReplaceSamePathAsync()
    {
        if (_replacementFilePath is null || SamePathImages is null)
        {
            return;
        }
        _replacementAssetIndex = (_replacementAssetIndex + 1) % 2;
        var asset = _replacementAssetIndex == 0 ? ReplacementFirstAsset : ReplacementSecondAsset;
        var nextPath = _replacementFilePath + ".next";
        await File.WriteAllBytesAsync(nextPath, ReadAssetBytes(asset));
        File.Move(nextPath, _replacementFilePath, overwrite: true);
        File.SetLastWriteTimeUtc(_replacementFilePath, DateTime.UtcNow.AddSeconds(_replacementAssetIndex + 1));

        SamePathImages.Clear();
        SamePathImages.Add(CreateSamePathItem());
    }

    private ImagePreviewItem CreateSamePathItem() =>
        new ImagePreviewItem(new FileImageSource(_replacementFilePath!));

    private void CleanupReplacementDirectory()
    {
        if (_replacementDirectory is not null && Directory.Exists(_replacementDirectory))
        {
            Directory.Delete(_replacementDirectory, recursive: true);
        }
        _replacementDirectory = null;
        _replacementFilePath = null;
    }

    private static ImagePreviewItem CreateRapidItem(int index)
    {
        var assetIndex = Math.Abs(index) % RapidSwitchAssets.Length;
        return new ImagePreviewItem(new StreamImageSource(
            async token =>
            {
                // 模拟磁盘读取/解码耗时，制造可观察的加载窗口用于对比两种切换模式
                await Task.Delay(RapidSwitchLoadDelayMs, token);
                var bytes = GetRapidAssetBytes(assetIndex);
                return new MemoryStream(bytes);
            },
            $"rapid-switch-{Guid.NewGuid():N}",
            "v1"));
    }

    private static byte[] GetRapidAssetBytes(int assetIndex)
    {
        lock (RapidAssetCacheGate)
        {
            var cached = RapidAssetCache[assetIndex];
            if (cached is not null)
            {
                return cached;
            }
        }
        using var stream = AssetLoader.Open(new Uri(RapidSwitchAssets[assetIndex]));
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        var bytes = memory.ToArray();
        lock (RapidAssetCacheGate)
        {
            RapidAssetCache[assetIndex] ??= bytes;
        }
        return bytes;
    }

    private static byte[] ReadAssetBytes(string source)
    {
        using var stream = AssetLoader.Open(new Uri(source));
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static ImagePreviewItem CreateItem(string source)
    {
        return new ImagePreviewItem(ImageSource.Parse(source));
    }

    private static ImagePreviewItem CreateGrayscaleItem(string source)
    {
        var grayscaleBytes = ApplyGrayscale(ReadAssetBytes(source), 0.5f);
        return new ImagePreviewItem(new StreamImageSource(
            _ => new ValueTask<Stream>(new MemoryStream(grayscaleBytes)),
            "grayscale-1",
            "v1"));
    }

    private static byte[] ApplyGrayscale(byte[] pngBytes, float amount)
    {
        using var source = SKBitmap.Decode(pngBytes);
        if (source is null)
        {
            return pngBytes;
        }

        // CSS filter: grayscale(amount) —— 在原始颜色与亮度灰度之间按 amount 插值。
        var inv = 1f - amount;
        var matrix = new[]
        {
            0.2126f + 0.7874f * inv, 0.7152f - 0.7152f * inv, 0.0722f - 0.0722f * inv, 0f, 0f,
            0.2126f - 0.2126f * inv, 0.7152f + 0.2848f * inv, 0.0722f - 0.0722f * inv, 0f, 0f,
            0.2126f - 0.2126f * inv, 0.7152f - 0.7152f * inv, 0.0722f + 0.9278f * inv, 0f, 0f,
            0f, 0f, 0f, 1f, 0f
        };
        using var paint  = new SKPaint { ColorFilter = SKColorFilter.CreateColorMatrix(matrix) };
        using var result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
        using (var canvas = new SKCanvas(result))
        {
            canvas.DrawBitmap(source, 0f, 0f, paint);
        }

        using var image = SKImage.FromBitmap(result);
        using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
