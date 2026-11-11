using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIEmpty = AtomUI.Desktop.Controls.Empty;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using AvaloniaSvg = Avalonia.Svg.Svg;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataDisplay;

public class EmptyObjectContentTests
{
    static EmptyObjectContentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow { Width = 360, Height = 300, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static AtomUITextBlock FindText(AtomUIEmpty empty)
    {
        return empty.GetVisualDescendants().OfType<AtomUITextBlock>()
                    .Single(static block => block.Classes.Contains("semantic-description"));
    }

    private static ContentPresenter FindContent(AtomUIEmpty empty)
    {
        return empty.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(static presenter => presenter.Name == "DescriptionContent");
    }

    [Fact]
    public void String_description_renders_through_the_text_part()
    {
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Description = "Nothing here" };
        var window = Show(empty);
        try
        {
            var text = FindText(empty);
            text.Text.ShouldBe("Nothing here");
            text.IsVisible.ShouldBeTrue();
            text.TextWrapping.ShouldBe(TextWrapping.Wrap);
            FindContent(empty).IsVisible.ShouldBeFalse();

            empty.IsDescriptionVisible = false;
            Dispatcher.UIThread.RunJobs();
            text.IsVisible.ShouldBeFalse();
            FindContent(empty).IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Default_description_is_the_localized_no_data_text()
    {
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple };
        var window = Show(empty);
        try
        {
            FindText(empty).Text.ShouldBe("No data");
            empty.Description.ShouldBe("No data");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Object_description_renders_through_the_content_presenter()
    {
        var custom = new Border { Child = new Avalonia.Controls.TextBlock { Text = "custom" }, Tag = "custom" };
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Description = custom };
        var window = Show(empty);
        try
        {
            var text = FindText(empty);
            var content = FindContent(empty);
            text.IsVisible.ShouldBeFalse();
            content.IsVisible.ShouldBeTrue();
            content.Child.ShouldBeSameAs(custom);

            // The presenter shares the description spacing and text colour with the text part.
            content.Margin.ShouldBe(text.Margin);
            ((ISolidColorBrush)content.Foreground!).Color.ShouldBe(((ISolidColorBrush)text.Foreground!).Color);
            content.HorizontalAlignment.ShouldBe(text.HorizontalAlignment);

            empty.IsDescriptionVisible = false;
            Dispatcher.UIThread.RunJobs();
            content.IsVisible.ShouldBeFalse();
            empty.IsDescriptionVisible = true;

            // Switching back to a string restores the text part.
            empty.Description = "plain";
            Dispatcher.UIThread.RunJobs();
            text.IsVisible.ShouldBeTrue();
            text.Text.ShouldBe("plain");
            content.IsVisible.ShouldBeFalse();
            content.Child.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Footer_keeps_its_own_margin_after_the_description_presenter_was_added()
    {
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Footer = new Button { Content = "go" } };
        var window = Show(empty);
        try
        {
            var footer = empty.GetVisualDescendants().OfType<ContentPresenter>()
                              .Single(static presenter => presenter.Classes.Contains("semantic-footer"));
            footer.Margin.ShouldNotBe(default);
            footer.Margin.ShouldNotBe(FindText(empty).Margin);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Preset_image_follows_theme_when_reattached_after_a_switch()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Default };
        var window = Show(empty);
        try
        {
            var svg = empty.GetVisualDescendants().OfType<AvaloniaSvg>().Single();
            var lightSource = svg.Source.ShouldNotBeNull();
            var lightBorder = ((ISolidColorBrush)empty.BorderColor!).Color;
            lightSource.ShouldBe(ExpectedDefault(empty));

            // Switch the theme while the empty is detached (its resource subscriptions are gone).
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            await ApplyAppearanceAsync(ThemeAppearance.Dark);

            window.Content = empty;
            Dispatcher.UIThread.RunJobs();
            empty.UpdateLayout();

            ((ISolidColorBrush)empty.BorderColor!).Color.ShouldNotBe(lightBorder);
            var darkSource = empty.GetVisualDescendants().OfType<AvaloniaSvg>().Single().Source.ShouldNotBeNull();
            darkSource.ShouldNotBe(lightSource);
            darkSource.ShouldBe(ExpectedDefault(empty));
        }
        finally
        {
            window.Close();
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    // Builds the default preset straight from the control's current colours, bypassing the cache.
    private static string ExpectedDefault(AtomUIEmpty empty)
    {
        var bg = ((ISolidColorBrush)empty.BgColor!).Color;
        Color Resolve(IBrush? brush) => ColorUtils.OnBackground(((ISolidColorBrush)brush!).Color, bg);
        return BuiltInImageBuilder.BuildDefaultImage(
            Resolve(empty.ShadowColor), Resolve(empty.BorderColor), Resolve(empty.BorderColorSecondary));
    }

    private static async Task ApplyAppearanceAsync(ThemeAppearance appearance)
    {
        var manager = ((IThemeManager?)AvaloniaLocator.Current.GetService(typeof(ThemeManager))).ShouldNotBeNull();
        var algorithms = appearance == ThemeAppearance.Dark
            ? new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Dark }
            : new[] { ThemeAlgorithm.Default };
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID,
                new ThemeConfigBuilder().WithAlgorithms(algorithms).Build(),
                ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        Dispatcher.UIThread.RunJobs();
    }
}
