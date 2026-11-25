using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AtomUI.Controls;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISeparator = AtomUI.Desktop.Controls.Separator;

namespace AtomUI.Desktop.Controls.Tests.Separator;

public class SeparatorThemeTests
{
    static SeparatorThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(CustomizableSizeType.Small, 8, false)]
    [InlineData(CustomizableSizeType.Middle, 16, false)]
    [InlineData(CustomizableSizeType.Large, 24, false)]
    [InlineData(CustomizableSizeType.Small, 8, true)]
    [InlineData(CustomizableSizeType.Middle, 16, true)]
    // antd: an unsized (AtomUI Large) divider with text uses the with-text gutter margin (UniformlyMargin).
    [InlineData(CustomizableSizeType.Large, 16, true)]
    public void Horizontal_Separator_Applies_Size_Block_Margin(
        CustomizableSizeType sizeType,
        double expectedMargin,
        bool hasTitle)
    {
        var separator = new AtomUISeparator
        {
            SizeType = sizeType,
            Title    = hasTitle ? "Title" : null
        };
        ApplyTheme(separator);

        var host = new AvaloniaWindow
        {
            Width   = 400,
            Height  = 200,
            Content = separator
        };

        try
        {
            host.Show();
            separator.ApplyTemplate();
            host.UpdateLayout();

            separator.Margin.ShouldBe(new Thickness(0, expectedMargin));
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Custom_Size_Uses_Default_Margin_And_Allows_Instance_Override()
    {
        var defaultSeparator = new AtomUISeparator
        {
            SizeType = CustomizableSizeType.Custom
        };
        var customSeparator = new AtomUISeparator
        {
            SizeType = CustomizableSizeType.Custom,
            Margin   = new Thickness(0, 32)
        };
        ApplyTheme(defaultSeparator);
        ApplyTheme(customSeparator);

        var panel = new StackPanel
        {
            Children =
            {
                defaultSeparator,
                customSeparator
            }
        };
        var host = new AvaloniaWindow
        {
            Width   = 400,
            Height  = 300,
            Content = panel
        };

        try
        {
            host.Show();
            defaultSeparator.ApplyTemplate();
            customSeparator.ApplyTemplate();
            host.UpdateLayout();

            defaultSeparator.Margin.ShouldBe(new Thickness(0, 16));
            customSeparator.Margin.ShouldBe(new Thickness(0, 32));
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Separator_Uses_Customizable_SizeType_Contract()
    {
        typeof(ICustomizableSizeTypeAware).IsAssignableFrom(typeof(AtomUISeparator)).ShouldBeTrue();
        AtomUISeparator.SizeTypeProperty.PropertyType.ShouldBe(typeof(CustomizableSizeType));
    }

    [Fact]
    public void Vertical_Separator_Does_Not_Use_Horizontal_Block_Margin()
    {
        var separator = new AtomUISeparator
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            SizeType    = CustomizableSizeType.Large
        };
        ApplyTheme(separator);

        var host = new AvaloniaWindow
        {
            Width   = 200,
            Height  = 200,
            Content = separator
        };

        try
        {
            host.Show();
            separator.ApplyTemplate();
            host.UpdateLayout();

            separator.Margin.ShouldBe(default);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Drawer_Info_Separators_Override_Standard_Block_Margin()
    {
        var container = new DrawerInfoContainer
        {
            Title   = "Title",
            Content = new Border()
        };
        ApplyTheme(container);

        var host = new AvaloniaWindow
        {
            Width   = 400,
            Height  = 300,
            Content = container
        };

        try
        {
            host.Show();
            container.ApplyTemplate();
            host.UpdateLayout();

            var separators = container.GetVisualDescendants().OfType<AtomUISeparator>().ToList();
            separators.Count.ShouldBe(2);
            separators.ShouldAllBe(separator => separator.Margin == default);
        }
        finally
        {
            host.Close();
        }
    }

    private static void ApplyTheme(Control control)
    {
        Application.Current!.TryFindResource(control.GetType(), out var resource).ShouldBeTrue();
        control.Theme = resource.ShouldBeAssignableTo<ControlTheme>();
    }
}
