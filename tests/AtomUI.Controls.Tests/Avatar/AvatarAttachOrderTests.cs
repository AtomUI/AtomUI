using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AtomUI.Controls.Tests.ImageLoading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Avatar;

// A real window attaches its content before the first layout pass, unlike the test host that measures first.
public class AvatarAttachOrderTests
{
    static AvatarAttachOrderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(Stretch.Uniform)]
    [InlineData(Stretch.UniformToFill)]
    public void Picture_loads_when_attached_before_first_layout(Stretch stretch)
    {
        var source = new BytesImageSource(ImageControlTestHost.CreatePng(160, 160), $"attach-order-{stretch}");
        var avatar = new global::AtomUI.Controls.Avatar
        {
            Width = 44,
            Height = 44,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = stretch,
            Source = source
        };
        var window = new Window
        {
            Width = 200,
            Height = 120
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.Content = avatar;
            ImageControlTestHost.WaitUntil(
                () => avatar.LoadState is ImageLoadState.Loaded or ImageLoadState.Failed,
                "avatar picture terminal state");

            avatar.LoadError.ShouldBeNull();
            avatar.LoadState.ShouldBe(ImageLoadState.Loaded);
            avatar.LoadedImage.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }
}
