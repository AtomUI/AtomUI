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
using Avalonia.Styling;
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

    private static ContentPresenter FindDescription(AtomUIEmpty empty)
    {
        return empty.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(static presenter => presenter.Classes.Contains("semantic-description"));
    }

    // String descriptions render through an AtomUI TextBlock generated inside the description presenter.
    private static AtomUITextBlock FindText(AtomUIEmpty empty)
    {
        return FindDescription(empty).GetVisualDescendants().OfType<AtomUITextBlock>().Single();
    }

    [Fact]
    public void String_description_renders_as_text_inside_the_description_part()
    {
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Description = "Nothing here" };
        var window = Show(empty);
        try
        {
            var description = FindDescription(empty);
            var text = FindText(empty);
            text.Text.ShouldBe("Nothing here");
            description.IsVisible.ShouldBeTrue();
            // Text settings live on the part and inherit to the text, so part styles reach it.
            text.TextWrapping.ShouldBe(TextWrapping.Wrap);
            ((ISolidColorBrush)text.Foreground!).Color.ShouldBe(((ISolidColorBrush)description.Foreground!).Color);

            empty.IsDescriptionVisible = false;
            Dispatcher.UIThread.RunJobs();
            description.IsVisible.ShouldBeFalse();
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
    public void Null_description_keeps_an_empty_text_line()
    {
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Description = null };
        var window = Show(empty);
        try
        {
            FindText(empty).Text.ShouldBe(string.Empty);
            FindDescription(empty).IsVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Object_description_renders_inside_the_same_description_part()
    {
        var custom = new Border { Child = new Avalonia.Controls.TextBlock { Text = "custom" }, Tag = "custom" };
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Description = custom };
        var window = Show(empty);
        try
        {
            var description = FindDescription(empty);
            description.IsVisible.ShouldBeTrue();
            description.Child.ShouldBeSameAs(custom);

            empty.IsDescriptionVisible = false;
            Dispatcher.UIThread.RunJobs();
            description.IsVisible.ShouldBeFalse();
            empty.IsDescriptionVisible = true;

            empty.Description = "plain";
            Dispatcher.UIThread.RunJobs();
            FindDescription(empty).ShouldBeSameAs(description);
            FindText(empty).Text.ShouldBe("plain");
        }
        finally
        {
            window.Close();
        }
    }

    // Ant Design styles.description applies to the description node whatever it contains; text settings written
    // for the former TextBlock part (TextBlock / TextElement properties) keep working for strings.
    [Fact]
    public void Description_part_style_reaches_text_and_custom_content()
    {
        var customText = new Avalonia.Controls.TextBlock { Text = "custom" };
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Description = "plain" };
        empty.Classes.Add("styled");
        var blue = Color.Parse("#1890FF");
        var partStyle = new Style(selector => selector.OfType<AtomUIEmpty>().Class("styled").Template()
                                                      .Class("semantic-description"))
        {
            Setters =
            {
                new Setter(Avalonia.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(blue)),
                new Setter(Avalonia.Controls.TextBlock.FontWeightProperty, FontWeight.Bold),
                new Setter(Avalonia.Controls.TextBlock.LineHeightProperty, 30d),
                new Setter(Avalonia.Controls.TextBlock.TextAlignmentProperty, TextAlignment.Right),
                // TextBlock-only, non-inherited: no longer applicable to the part, but must not break styling.
                new Setter(Avalonia.Controls.TextBlock.TextDecorationsProperty, TextDecorations.Underline)
            }
        };
        empty.Styles.Add(partStyle);
        var window = Show(empty);
        try
        {
            var text = FindText(empty);
            ((ISolidColorBrush)text.Foreground!).Color.ShouldBe(blue);
            text.FontWeight.ShouldBe(FontWeight.Bold);
            text.LineHeight.ShouldBe(30d);
            text.TextAlignment.ShouldBe(TextAlignment.Right);

            empty.Description = new Border { Child = customText };
            Dispatcher.UIThread.RunJobs();
            ((ISolidColorBrush)customText.Foreground!).Color.ShouldBe(blue);
            customText.FontWeight.ShouldBe(FontWeight.Bold);
            customText.LineHeight.ShouldBe(30d);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Footer_keeps_its_own_margin()
    {
        var empty = new AtomUIEmpty { PresetImage = PresetEmptyImage.Simple, Footer = new Button { Content = "go" } };
        var window = Show(empty);
        try
        {
            var footer = empty.GetVisualDescendants().OfType<ContentPresenter>()
                              .Single(static presenter => presenter.Classes.Contains("semantic-footer"));
            footer.Margin.ShouldNotBe(default);
            footer.Margin.ShouldNotBe(FindDescription(empty).Margin);
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
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();
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
