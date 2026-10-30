using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class HyperLinkButtonIconSizeTests
{
    static HyperLinkButtonIconSizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void HyperLinkButton_Applies_Explicit_Icon_Size_To_Template_Icons()
    {
        var iconWidthProperty = typeof(HyperLinkButton).GetProperty("IconWidth");
        var iconHeightProperty = typeof(HyperLinkButton).GetProperty("IconHeight");
        iconWidthProperty.ShouldNotBeNull();
        iconHeightProperty.ShouldNotBeNull();

        var button = new HyperLinkButton
        {
            Width     = 40,
            Height    = 40,
            IsLoading = true
        };
        iconWidthProperty.SetValue(button, 22d);
        iconHeightProperty.SetValue(button, 24d);

        var window = new Avalonia.Controls.Window
        {
            Width   = 160,
            Height  = 120,
            Content = button
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var buttonIcon = button.GetVisualDescendants()
                                   .OfType<IconPresenter>()
                                   .Single(x => x.Name == "PART_ButtonIcon");
            var loadingIcon = button.GetVisualDescendants()
                                    .OfType<Icon>()
                                    .Single(x => x.Name == "PART_LoadingIcon");

            buttonIcon.Width.ShouldBe(22d);
            buttonIcon.Height.ShouldBe(24d);
            loadingIcon.Width.ShouldBe(22d);
            loadingIcon.Height.ShouldBe(24d);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void HyperLinkButton_Template_Icon_Color_Follows_Foreground()
    {
        var firstBrush  = new SolidColorBrush(Colors.Red);
        var secondBrush = new SolidColorBrush(Colors.Blue);
        var button = new HyperLinkButton
        {
            Width      = 40,
            Height     = 40,
            IconWidth  = 16,
            IconHeight = 16,
            Icon            = new PathIcon { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") },
            IsLoading       = true,
            IsMotionEnabled = false,
            Foreground      = firstBrush
        };

        var window = new Avalonia.Controls.Window
        {
            Width   = 160,
            Height  = 120,
            Content = button
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var buttonIcon = button.GetVisualDescendants()
                                   .OfType<IconPresenter>()
                                   .Single(x => x.Name == "PART_ButtonIcon");
            var loadingIcon = button.GetVisualDescendants()
                                    .OfType<Icon>()
                                    .Single(x => x.Name == "PART_LoadingIcon");

            buttonIcon.IconBrush.ShouldBeSameAs(firstBrush);
            loadingIcon.FillBrush.ShouldBeSameAs(firstBrush);
            loadingIcon.StrokeBrush.ShouldBeSameAs(firstBrush);

            button.Foreground = secondBrush;
            Dispatcher.UIThread.RunJobs();

            buttonIcon.IconBrush.ShouldBeSameAs(secondBrush);
            loadingIcon.FillBrush.ShouldBeSameAs(secondBrush);
            loadingIcon.StrokeBrush.ShouldBeSameAs(secondBrush);
        }
        finally
        {
            window.Close();
        }
    }

}
