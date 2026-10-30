using AtomUI.Controls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Styling;
using Shouldly;
using Xunit;
using AtomUIDesktopCascader = AtomUI.Desktop.Controls.Cascader;
using AtomUIDesktopSelect = AtomUI.Desktop.Controls.Select;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

public class SelectHandleIndicatorColorTests
{
    static SelectHandleIndicatorColorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Cascader_Arrow_Indicator_Defaults_To_The_Quaternary_Text_Token()
    {
        var cascader = new AtomUIDesktopCascader();
        var window = Show(cascader);
        try
        {
            var presenter = FindOpenIndicator(cascader);
            presenter.IconBrush.ShouldBe(QuaternaryBrush(window));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Select_Arrow_Indicator_Defaults_To_The_Quaternary_Text_Token()
    {
        var select = new AtomUIDesktopSelect();
        var window = Show(select);
        try
        {
            var presenter = FindOpenIndicator(select);
            presenter.IconBrush.ShouldBe(QuaternaryBrush(window));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Cascader_Suffix_Semantic_Style_Overrides_The_Arrow_Color()
    {
        var cascader = new AtomUIDesktopCascader();
        cascader.Classes.Add("suffix-color-demo");
        var blue = new SolidColorBrush(Color.Parse("#1890FF"));

        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIDesktopCascader), out var descriptor).ShouldBeTrue();
        var suffixPart = descriptor.ShouldNotBeNull()
                                   .Parts.Single(static part => part.Name == "suffix");
        var suffixStyle = (Style)Activator.CreateInstance(suffixPart.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        suffixStyle.Setters.Add(new Setter(Avalonia.Controls.Documents.TextElement.ForegroundProperty, blue));

        var ownerStyle = new Style(selector => selector.OfType<AtomUIDesktopCascader>().Class("suffix-color-demo"));
        ownerStyle.Children.Add(suffixStyle);
        cascader.Styles.Add(ownerStyle);

        var window = Show(cascader);
        try
        {
            var suffixPanel = cascader.GetVisualDescendants()
                                      .OfType<StackPanel>()
                                      .Single(panel => panel.Classes.Contains("semantic-suffix"));
            Avalonia.Controls.Documents.TextElement.GetForeground(suffixPanel).ShouldBe(blue);

            var presenter = FindOpenIndicator(cascader);
            presenter.IconBrush.ShouldBe(blue);
            var icon = presenter.Icon.ShouldNotBeNull();
            var atomIcon = icon.ShouldBeAssignableTo<AtomUI.Controls.Icon>().ShouldNotBeNull();
            atomIcon.StrokeBrush.ShouldBe(blue);
        }
        finally
        {
            window.Close();
        }
    }

    private static IconPresenter FindOpenIndicator(Control owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<IconPresenter>()
                    .Single(presenter => presenter.Name == "OpenIndicator");
    }

    private static IBrush QuaternaryBrush(Avalonia.Controls.Window window)
    {
        window.ActualThemeVariant.ShouldNotBeNull();
        Application.Current!.TryGetResource(
            SharedTokenKind.ColorTextQuaternary, window.ActualThemeVariant, out var value).ShouldBeTrue();
        return value.ShouldBeAssignableTo<IBrush>().ShouldNotBeNull();
    }

    private static Avalonia.Controls.Window Show(Control content)
    {
        var window = new Avalonia.Controls.Window
        {
            Width = 320,
            Height = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
