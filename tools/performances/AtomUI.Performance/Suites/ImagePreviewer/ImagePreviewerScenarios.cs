using Avalonia.Controls;
using Avalonia.Layout;
using AtomImagePreviewer = AtomUI.Desktop.Controls.ImagePreviewer;
using AtomImageGroupPreviewer = AtomUI.Desktop.Controls.ImageGroupPreviewer;
using ImagePreviewItem = AtomUI.Desktop.Controls.ImagePreviewItem;
using ImageSource = AtomUI.Controls.ImageSource;

namespace AtomUI.Performance;

internal static partial class Program
{
    // ImagePreviewer API 演进：ItemsSource 从 string[] 变为 IEnumerable<ImagePreviewItem>，
    // 图片地址需通过 ImageSource.Parse 包装；FallbackImageSrc / CoverImageSrc 已移除，
    // 分别由 ImagePreviewItem.FallbackSource / ThumbnailSource 取代。
    private static readonly ImagePreviewItem[] ImagePreviewerDefaultImages =
        CreateImagePreviewerItems(GetImagePreviewerAssetUri("1.png"));

    private static readonly ImagePreviewItem[] ImagePreviewerThreeImages =
        CreateImagePreviewerItems(
            GetImagePreviewerAssetUri("4.webp"),
            GetImagePreviewerAssetUri("5.webp"),
            GetImagePreviewerAssetUri("6.webp"));

    private static readonly ImagePreviewItem[] ImagePreviewerTwoImages =
        CreateImagePreviewerItems(
            GetImagePreviewerAssetUri("2.svg"),
            GetImagePreviewerAssetUri("3.svg"));

    private static readonly string ImagePreviewerFallbackImage = GetImagePreviewerAssetUri("Fallback.png");
    private static readonly string ImagePreviewerBlurImage = GetImagePreviewerAssetUri("Blur.png");

    private static IReadOnlyList<PerfScenario> CreateImagePreviewerScenarios()
    {
        return
        [
            new PerfScenario("ImagePreviewer.Basic", _ => CreateBasicImagePreviewer()),
            new PerfScenario("ImagePreviewer.Fallback", _ => CreateFallbackImagePreviewer()),
            new PerfScenario("ImagePreviewer.MultiSource", _ => CreateMultiSourceImagePreviewer()),
            new PerfScenario("ImagePreviewer.CustomCover", _ => CreateCustomCoverImagePreviewer()),
            new PerfScenario("ImageGroupPreviewer.TwoSvg", _ => CreateImageGroupPreviewer()),
            new PerfScenario("ImagePreviewer.GalleryShape", _ => CreateImagePreviewerGalleryShape())
        ];
    }

    private static ImagePreviewItem[] CreateImagePreviewerItems(params string[] sources)
    {
        return sources
            .Select(source => new ImagePreviewItem(ImageSource.Parse(source)))
            .ToArray();
    }

    private static AtomImagePreviewer CreateBasicImagePreviewer()
    {
        return new AtomImagePreviewer
        {
            Width       = 200,
            ItemsSource = ImagePreviewerDefaultImages
        };
    }

    private static AtomImagePreviewer CreateFallbackImagePreviewer()
    {
        // FallbackImageSrc 已移除：回退图改为 ImagePreviewItem.FallbackSource
        return new AtomImagePreviewer
        {
            Width       = 200,
            ItemsSource = new[]
            {
                new ImagePreviewItem(ImageSource.Parse(GetImagePreviewerAssetUri("1.png")))
                {
                    FallbackSource = ImageSource.Parse(ImagePreviewerFallbackImage)
                }
            }
        };
    }

    private static AtomImagePreviewer CreateMultiSourceImagePreviewer()
    {
        return new AtomImagePreviewer
        {
            Width       = 200,
            ItemsSource = ImagePreviewerThreeImages
        };
    }

    private static AtomImagePreviewer CreateCustomCoverImagePreviewer()
    {
        // CoverImageSrc 已移除：自定义封面改为 ImagePreviewItem.ThumbnailSource
        return new AtomImagePreviewer
        {
            Width       = 200,
            ItemsSource = new[]
            {
                new ImagePreviewItem(ImageSource.Parse(GetImagePreviewerAssetUri("1.png")))
                {
                    ThumbnailSource = ImageSource.Parse(ImagePreviewerBlurImage)
                }
            }
        };
    }

    private static AtomImageGroupPreviewer CreateImageGroupPreviewer()
    {
        return new AtomImageGroupPreviewer
        {
            CoverWidth  = 200,
            CoverHeight = 200,
            ItemsSource = ImagePreviewerTwoImages
        };
    }

    private static Control CreateImagePreviewerGalleryShape()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 20,
            Children =
            {
                CreateBasicImagePreviewer(),
                CreateFallbackImagePreviewer(),
                CreateMultiSourceImagePreviewer(),
                CreateCustomCoverImagePreviewer(),
                CreateImageGroupPreviewer()
            }
        };
    }

    private static string GetImagePreviewerAssetUri(string fileName)
    {
        var path = Path.GetFullPath(Path.Combine(
            "controlgallery",
            "AtomUIGallery",
            "Assets",
            "ImagePreviewerShowCase",
            fileName));
        return new Uri(path).AbsoluteUri;
    }
}
