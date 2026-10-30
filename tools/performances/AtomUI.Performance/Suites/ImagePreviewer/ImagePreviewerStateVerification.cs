using System.Collections;
using System.Reflection;
using AtomUI.Controls;
using Avalonia.Threading;
using ImagePreviewItem = AtomUI.Desktop.Controls.ImagePreviewItem;
using ImageSource = AtomUI.Controls.ImageSource;
using AtomImagePreviewer = AtomUI.Desktop.Controls.ImagePreviewer;
using AtomImageGroupPreviewer = AtomUI.Desktop.Controls.ImageGroupPreviewer;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunImagePreviewerStateVerification()
    {
        var failures = new List<string>();
        VerifyImagePreviewerClosedStateMaterialization(failures);
        VerifyImagePreviewerDialogMaterialization(failures);
        VerifyImagePreviewerOpenItemsSourceReplacement(failures);
        VerifyImageGroupPreviewerMaterialization(failures);
        VerifyImagePreviewerCoverReplacement(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("ImagePreviewer state verification passed.");
            return true;
        }

        Console.Error.WriteLine("ImagePreviewer state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyImagePreviewerClosedStateMaterialization(ICollection<string> failures)
    {
        var previewer = CreateMultiSourceImagePreviewer();
        using var realized = RealizeControl(previewer);

        // 新管线中 EffectiveItems 在 attach 时即建立（条目为轻量包装），
        // 关闭状态的裁剪点改为：仅封面项触发缩略图加载请求。
        Expect(GetImagePreviewerEffectiveSourceCount(previewer) == ImagePreviewerThreeImages.Length,
            "Single ImagePreviewer should keep lightweight entries for all items.",
            failures);
        Expect(previewer.CoverLoadState != AtomUI.Controls.ImageLoadState.Idle,
            "Single ImagePreviewer closed state should request the visible cover thumbnail load.",
            failures);
        Expect(CountVisualByTypeName(previewer, "ImagePreviewerCover") == 1,
            "Single ImagePreviewer should keep one cover visual.",
            failures);

        previewer.ItemsSource = null;
        RefreshLayout(realized.Window);
        Expect(GetImagePreviewerEffectiveSourceCount(previewer) == 0,
            "Clearing ItemsSource should clear effective entries.",
            failures);
        Expect(GetImagePreviewerEffectiveCoverImage(previewer) == null,
            "Clearing ItemsSource should release the effective cover image when no fallback exists.",
            failures);
    }

    private static void VerifyImagePreviewerDialogMaterialization(ICollection<string> failures)
    {
        var previewer = CreateMultiSourceImagePreviewer();
        using var realized = RealizeControl(previewer);

        previewer.OpenDialog();
        Dispatcher.UIThread.RunJobs();

        Expect(previewer.IsOpen,
            "OpenDialog should set IsOpen=true.",
            failures);
        Expect(GetImagePreviewerEffectiveSourceCount(previewer) == ImagePreviewerThreeImages.Length,
            "Opening ImagePreviewer should keep dialog entries materialized.",
            failures);
        Expect(previewer.CurrentLoadState == AtomUI.Controls.ImageLoadState.Loading ||
               previewer.IsCurrentLoading,
            "Opening ImagePreviewer should request the current full image load.",
            failures);

        previewer.IsOpen = false;
        RefreshLayout(realized.Window);
        Expect(!previewer.IsOpen,
            "Closing ImagePreviewer should set IsOpen=false.",
            failures);
    }

    private static void VerifyImagePreviewerOpenItemsSourceReplacement(ICollection<string> failures)
    {
        var previewer = CreateMultiSourceImagePreviewer();
        using var realized = RealizeControl(previewer);

        previewer.OpenDialog();
        Dispatcher.UIThread.RunJobs();
        previewer.ItemsSource = ImagePreviewerTwoImages;
        RefreshLayout(realized.Window);

        Expect(GetImagePreviewerEffectiveSourceCount(previewer) == ImagePreviewerTwoImages.Length,
            "Replacing ItemsSource while ImagePreviewer is open should keep dialog sources materialized.",
            failures);

        previewer.IsOpen = false;
        RefreshLayout(realized.Window);
    }

    private static void VerifyImageGroupPreviewerMaterialization(ICollection<string> failures)
    {
        AtomImageGroupPreviewer groupPreviewer = CreateImageGroupPreviewer();
        using var realized = RealizeControl(groupPreviewer);

        Expect(GetImagePreviewerEffectiveSourceCount(groupPreviewer) == ImagePreviewerTwoImages.Length,
            "ImageGroupPreviewer closed state should materialize all visible cover images.",
            failures);
        Expect(CountVisualByTypeName(groupPreviewer, "ImagePreviewerCover") == ImagePreviewerTwoImages.Length,
            "ImageGroupPreviewer should create one cover visual per source.",
            failures);
    }

    private static void VerifyImagePreviewerCoverReplacement(ICollection<string> failures)
    {
        AtomImagePreviewer previewer = CreateCustomCoverImagePreviewer();
        using var realized = RealizeControl(previewer);
        var diagLoader = Avalonia.Application.Current?.GetImageLoader();
        var diagResult = diagLoader?.LoadAsync(new AtomUI.Controls.ImageLoadRequest(
            ImageSource.Parse(GetImagePreviewerAssetUri("1.png")))).AsTask().GetAwaiter().GetResult();
        Console.Error.WriteLine($"[diag] direct load success={diagResult?.IsSuccess} err={diagResult?.Error} snapshot={diagLoader?.Snapshot}");

        // 新管线封面走异步缩略图加载提交，等待加载完成后再断言（与测试项目 WaitUntil 用法一致）
        WaitForImagePreviewer(() => previewer.CoverLoadState == AtomUI.Controls.ImageLoadState.Loaded);
        var firstCover = GetImagePreviewerEffectiveCoverImage(previewer);
        Expect(firstCover != null,
            "Custom cover ImagePreviewer should materialize its custom cover image.",
            failures);
        Expect(previewer.CurrentLoadState == AtomUI.Controls.ImageLoadState.Idle,
            "Custom cover ImagePreviewer should not request full dialog image loads while closed.",
            failures);

        // CoverImageSrc 已移除：自定义封面由 ImagePreviewItem.ThumbnailSource 提供，
        // 替换自定义封面等价于替换带不同 ThumbnailSource 的 ItemsSource。
        previewer.ItemsSource = new[]
        {
            new ImagePreviewItem(ImageSource.Parse(GetImagePreviewerAssetUri("1.png")))
            {
                ThumbnailSource = ImageSource.Parse(ImagePreviewerFallbackImage)
            }
        };
        RefreshLayout(realized.Window);
        WaitForImagePreviewer(() => previewer.CoverLoadState == AtomUI.Controls.ImageLoadState.Loaded);
        var secondCover = GetImagePreviewerEffectiveCoverImage(previewer);
        Expect(secondCover != null && !ReferenceEquals(firstCover, secondCover),
            "Replacing the custom cover thumbnail source should replace the effective cover image.",
            failures);

        previewer.ItemsSource = ImagePreviewerDefaultImages;
        RefreshLayout(realized.Window);
        WaitForImagePreviewer(() => previewer.CoverLoadState == AtomUI.Controls.ImageLoadState.Loaded);
        Expect(GetImagePreviewerEffectiveCoverImage(previewer) != null,
            "Clearing the custom cover thumbnail should fall back to the item main source as the cover.",
            failures);
        Expect(previewer.CurrentLoadState == AtomUI.Controls.ImageLoadState.Idle,
            "Replacing the custom cover thumbnail should not request full dialog image loads while closed.",
            failures);
    }

    private static void WaitForImagePreviewer(Func<bool> condition)
    {
        var deadline = Environment.TickCount64 + 5000;
        while (true)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition() || Environment.TickCount64 >= deadline)
            {
                return;
            }
            Thread.Sleep(10);
        }
    }

    private static int GetImagePreviewerEffectiveSourceCount(object previewer)
    {
        return GetNonPublicProperty(previewer, "AtomUI.Desktop.Controls.AbstractImagePreviewer", "EffectiveItems") is ICollection sources
            ? sources.Count
            : 0;
    }

    private static object? GetImagePreviewerEffectiveCoverImage(AtomImagePreviewer previewer)
    {
        return GetNonPublicProperty(previewer, "AtomUI.Desktop.Controls.ImagePreviewer", "EffectiveCoverImage");
    }

    private static object? GetNonPublicProperty(object target, string declaringTypeName, string propertyName)
    {
        var type = target.GetType();
        while (type is not null)
        {
            if (type.FullName == declaringTypeName)
            {
                return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target);
            }

            type = type.BaseType;
        }

        return null;
    }
}
