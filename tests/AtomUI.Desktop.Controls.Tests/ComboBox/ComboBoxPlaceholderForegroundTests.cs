using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ComboBox;

/// <summary>
/// Theme contract: both placeholder nodes (non-editable TextBlock and the embedded editable text box) consume
/// <c>PlaceholderForeground</c>, whose default is <c>ColorTextPlaceholder</c>. A missing default or a node that
/// ignores the brush renders the placeholder invisible while every inspector property still looks normal (#534).
/// </summary>
public class ComboBoxPlaceholderForegroundTests
{
    static ComboBoxPlaceholderForegroundTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Editable_Placeholder_Uses_ColorTextPlaceholder_By_Default()
    {
        var comboBox = CreateComboBox(isEditable: true);

        ShowInWindow(comboBox, () =>
        {
            var placeholder = FindTemplatePart<AvaloniaTextBlock>(comboBox, "Placeholder");

            placeholder.IsEffectivelyVisible.ShouldBeTrue();
            BrushShouldHaveSameColor(placeholder.Foreground, GetThemeResource<IBrush>(SharedTokenKind.ColorTextPlaceholder));
        });
    }

    [Fact]
    public void NonEditable_Placeholder_Uses_ColorTextPlaceholder_By_Default()
    {
        var comboBox = CreateComboBox(isEditable: false);

        ShowInWindow(comboBox, () =>
        {
            var placeholder = FindTemplatePart<AvaloniaTextBlock>(comboBox, "PlaceholderText");

            placeholder.IsEffectivelyVisible.ShouldBeTrue();
            placeholder.Opacity.ShouldBe(1d);
            BrushShouldHaveSameColor(placeholder.Foreground, GetThemeResource<IBrush>(SharedTokenKind.ColorTextPlaceholder));
        });
    }

    [Fact]
    public void Custom_PlaceholderForeground_Applies_To_Both_Placeholder_Nodes()
    {
        var comboBox = CreateComboBox(isEditable: false);
        comboBox.PlaceholderForeground = Brushes.Red;

        ShowInWindow(comboBox, () =>
        {
            BrushShouldHaveSameColor(FindTemplatePart<AvaloniaTextBlock>(comboBox, "PlaceholderText").Foreground, Brushes.Red);

            comboBox.IsEditable = true;
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(FindTemplatePart<AvaloniaTextBlock>(comboBox, "Placeholder").Foreground, Brushes.Red);
        });
    }

    private static AtomUIComboBox CreateComboBox(bool isEditable)
    {
        return new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = isEditable,
            IsMotionEnabled = false,
            PlaceholderText = "请输入",
            ItemsSource     = new[] { "Alpha", "Beta" }
        };
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : Control
    {
        var part = control.GetVisualDescendants()
                          .OfType<T>()
                          .SingleOrDefault(item => item.Name == name);
        part.ShouldNotBeNull();
        return part!;
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected)
    {
        actual.ShouldNotBeNull();
        actual.ShouldBeAssignableTo<ISolidColorBrush>();
        expected.ShouldBeAssignableTo<ISolidColorBrush>();
        ((ISolidColorBrush)actual!).Color.ShouldBe(((ISolidColorBrush)expected!).Color);
    }
}
