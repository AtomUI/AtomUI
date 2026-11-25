using AtomUI.Theme;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerCoverOwnerInputTests
{
    static ImagePreviewerCoverOwnerInputTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Cover_Hosted_In_Its_Previewer_Keeps_The_Owner_Setter_Values()
    {
        // In the owner's styled tree the cover inherits ImagePreviewerTheme's inputs; the standalone mapping must not
        // mask them, so an explicit input on the owner reaches the cover and the cover carries no mapped value.
        Dispatcher.UIThread.Invoke(() =>
        {
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer { Width = 200, Height = 200 };
            ImageViewer.SetCoverImageSize(previewer, 300);
            var window = new AvaloniaWindow { Content = previewer, Width = 400, Height = 400 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                previewer.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var cover = previewer.GetVisualDescendants().OfType<ImagePreviewerCover>().Single();
                ThemeScope.GetTokenOwner(cover).ShouldBe(ThemeScope.GetTokenOwner(previewer));
                cover.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeFalse("the hosted cover inherits its owner");
                ImageViewer.GetCoverImageSize(cover).ShouldBe(300d, "the owner's explicit input must reach the hosted cover");
                cover.IsSet(ImageViewer.CoverImageSizeProperty).ShouldBeFalse("no mapped value may sit on the hosted cover");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Cover_Created_Alone_Takes_The_Previewer_Identity_And_Maps_Its_Inputs()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var cover = new ImagePreviewerCover { Width = 100, Height = 100, IsShowCoverMask = true, Content = "preview" };
            var window = new AvaloniaWindow { Content = cover, Width = 200, Height = 200 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                cover.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer();
                var registry = ThemeScope.ResolveContext(cover).ShouldNotBeNull().Snapshot.Registry;
                registry.TryGetControl(previewer.GetType(), out var descriptor).ShouldBeTrue();
                ThemeScope.GetTokenOwner(cover).ShouldBe(descriptor!.Identity);
                ImageViewer.GetMaskBackground(cover).ShouldNotBeNull();
                ImageViewer.GetCoverImageSize(cover).ShouldBeGreaterThan(0);
            }
            finally
            {
                window.Close();
            }
        });
    }
}
