using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerMotionTests
{
    public ImagePreviewerMotionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ImageViewer_Transform_Transitions_Respect_SuppressTransformAnimation()
    {
        var viewerTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml");

        viewerTheme.ShouldContain("^[IsMotionEnabled=True][SuppressTransformAnimation=False]");
        viewerTheme.ShouldContain("TransformOperationsTransition Property=\"ImageRenderTransform\"");
        viewerTheme.ShouldContain("DoubleTransition Property=\"ImageTranslateX\"");
        viewerTheme.ShouldContain("DoubleTransition Property=\"ImageTranslateY\"");
    }

    [Fact]
    public void ImagePreviewerDialog_Suppresses_Transform_Animation_During_WindowState_Change()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = new ImagePreviewerDialog(
                new global::Avalonia.Controls.Window(),
                new global::AtomUI.Desktop.Controls.ImagePreviewer());

            dialog.SuppressTransformAnimation.ShouldBeFalse();

            dialog.WindowState = global::Avalonia.Controls.WindowState.Maximized;

            dialog.SuppressTransformAnimation.ShouldBeTrue();

            Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);

            dialog.SuppressTransformAnimation.ShouldBeFalse();
        });
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
