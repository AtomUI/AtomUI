using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using AtomUI.Controls.Tests.ImageLoading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Avatar;

public class AvatarStretchTests
{
    static AvatarStretchTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Stretch_defaults_to_uniform()
    {
        var avatar = new global::AtomUI.Controls.Avatar();
        avatar.Stretch.ShouldBe(Stretch.Uniform);

        avatar.Size = 48;
        using var host = new ImageControlTestHost(avatar);
        var image = avatar.GetVisualDescendants()
                          .OfType<Image>()
                          .Single(control => control.Name == "ImagePresenter");
        image.Stretch.ShouldBe(Stretch.Uniform);

        avatar.Stretch = Stretch.UniformToFill;
        image.Stretch.ShouldBe(Stretch.UniformToFill);
    }

    [Fact]
    public void Circle_radius_follows_width_change()
    {
        var avatar = new global::AtomUI.Controls.Avatar { Size = 48 };
        using var host = new ImageControlTestHost(avatar);
        avatar.CornerRadius.TopLeft.ShouldBe(24);

        avatar.Size = 80;
        avatar.Width.ShouldBe(80);
        avatar.CornerRadius.TopLeft.ShouldBe(40);

        avatar.Width = 60;
        avatar.CornerRadius.TopLeft.ShouldBe(30);
    }

    [Fact]
    public void Circle_radius_follows_size_type_change()
    {
        var avatar = new global::AtomUI.Controls.Avatar { SizeType = CustomizableSizeType.Small };
        using var host = new ImageControlTestHost(avatar);
        var small = avatar.Width;
        small.ShouldBeGreaterThan(0);
        avatar.CornerRadius.TopLeft.ShouldBe(small / 2);

        avatar.SizeType = CustomizableSizeType.Large;
        avatar.Width.ShouldBeGreaterThan(small);
        avatar.CornerRadius.TopLeft.ShouldBe(avatar.Width / 2);
    }
}
