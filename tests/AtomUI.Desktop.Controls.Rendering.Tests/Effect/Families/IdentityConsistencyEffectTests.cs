using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>Distinct public controls share calculations, never another control's explicit Own overrides.</summary>
public class IdentityConsistencyEffectTests
{
    [Theory]
    [InlineData("TitleFontSize")]
    [InlineData("ContentFontSize")]
    public void Statistic_And_Timer_Own_Overrides_Are_Independent(string token)
    {
        var statistic = new Statistic { Header = "Statistic", Value = 1234 };
        var timer = new TimerStatistic { Header = "Timer", Value = DateTime.Now.AddHours(1) };
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { statistic, timer } }, (provider, capture) =>
        {
            using var before = capture();
            var statisticBefore = Font(statistic, token);
            var timerBefore = Font(timer, token);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (StatisticTokens.Identity, token, "37"));
            using var sourceChanged = capture();
            Font(statistic, token).ShouldBe(37);
            Font(timer, token).ShouldBe(timerBefore);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (TimerStatisticTokens.Identity, token, "41"));
            using var targetChanged = capture();
            Font(statistic, token).ShouldBe(statisticBefore);
            Font(timer, token).ShouldBe(41);
        });
    }

    [Theory]
    [InlineData(CustomizableSizeType.Small, "FontSize", 23d)]
    [InlineData(CustomizableSizeType.Middle, "FontSize", 25d)]
    [InlineData(CustomizableSizeType.Large, "FontSizeLG", 29d)]
    public void Numeric_Embedded_Editor_Uses_Host_Size_Typography(CustomizableSizeType size, string token, double value)
    {
        var numeric = new NumericUpDown { Value = 1234, SizeType = size, Width = 220 };
        FeedbackFamilyEffectTests.WithIsolationWindow(numeric, (provider, capture) =>
        {
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (NumericUpDownTokens.Identity, token, value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            using var changed = capture();
            numeric.GetVisualDescendants().OfType<EmbeddedTextBox>().Single().FontSize.ShouldBe(value);
        });
    }

    [Fact]
    public void Dropdown_Own_And_Global_Colours_Are_Independent_Of_Button()
    {
        var button = new Button { Content = "Button" };
        var dropdown = new DropdownButton { Content = "Dropdown" };
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { button, dropdown } }, (provider, capture) =>
        {
            using var before = capture();
            var baseline = dropdown.Foreground;
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (ButtonTokens.Identity, "DefaultColor", "#ff00b96b"));
            using var sourceChanged = capture();
            dropdown.Foreground.ShouldBe(baseline);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (DropdownButtonTokens.Identity, "ColorText", "#ff00b96b"));
            using var targetChanged = capture();
            dropdown.Foreground.ShouldNotBe(baseline);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(root: ("ColorText", "#ff00b96b"));
            using var rootChanged = capture();
            button.Foreground.ShouldBe(dropdown.Foreground);
        });
    }

    [Theory]
    [InlineData("AutoCompleteSearchEdit")]
    [InlineData("AutoCompleteTextArea")]
    public void AutoComplete_Public_Terminals_Do_Not_Read_Source_Own(string terminal)
    {
        AbstractAutoComplete target = terminal == "AutoCompleteSearchEdit" ? new AutoCompleteSearchEdit() : new AutoCompleteTextArea();
        var identity = terminal == "AutoCompleteSearchEdit" ? AutoCompleteSearchEditTokens.Identity : AutoCompleteTextAreaTokens.Identity;
        var source = new AutoComplete();
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { source, target } }, (provider, capture) =>
        {
            using var before = capture();
            var baseline = target.MinPopupWidth;
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (AutoCompleteTokens.Identity, "MinPopupWidth", "333"));
            using var sourceChanged = capture();
            source.MinPopupWidth.ShouldBe(333);
            target.MinPopupWidth.ShouldBe(baseline);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (identity, "MinPopupWidth", "444"));
            using var targetChanged = capture();
            target.MinPopupWidth.ShouldBe(444);
            source.MinPopupWidth.ShouldBe(baseline);
        });
    }

    [Fact]
    public void Numeric_Handle_Uses_The_Numeric_Own_Background()
    {
        var numeric = new NumericUpDown { Width = 200, Value = 123 };
        FeedbackFamilyEffectTests.WithIsolationWindow(numeric, (provider, capture) =>
        {
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (NumericUpDownTokens.Identity, "HandleBg", "#ff00b96b"));
            using var changed = capture();
            var spinner = numeric.GetVisualDescendants().OfType<NumericUpDownSpinner>().Single();
            var handle = numeric.GetVisualDescendants().OfType<ButtonSpinnerHandle>().Single();
            spinner.HandleBg.ShouldBe(new Avalonia.Media.Immutable.ImmutableSolidColorBrush(Avalonia.Media.Color.Parse("#ff00b96b")));
            handle.Background.ShouldBe(spinner.HandleBg);
        });
    }

    [Theory]
    [InlineData("DropdownButton")]
    [InlineData("SubmitButton")]
    [InlineData("ResetButton")]
    [InlineData("TimerStatistic")]
    [InlineData("StatisticCountUp")]
    [InlineData("RangeDateViewer")]
    [InlineData("TransferListView")]
    [InlineData("TransferTreeView")]
    public void Public_Terminal_Pairs_Isolate_Own_And_Global_But_Share_Root(string targetName)
    {
        Control source;
        Control target;
        ControlTokenIdentity sourceIdentity;
        ControlTokenIdentity targetIdentity;
        string own;
        string global;
        string ownValue;
        string globalValue;
        Func<Control, object?> observe;
        switch (targetName)
        {
            case "DropdownButton" or "SubmitButton" or "ResetButton":
                source = new Button { Content = "Source" };
                target = targetName switch
                {
                    "DropdownButton" => new DropdownButton { Content = "Target" },
                    "SubmitButton" => new SubmitButton { Content = "Target", ButtonType = ButtonType.Default },
                    _ => new ResetButton { Content = "Target" }
                };
                sourceIdentity = ButtonTokens.Identity;
                targetIdentity = targetName switch
                {
                    "DropdownButton" => DropdownButtonTokens.Identity,
                    "SubmitButton" => SubmitButtonTokens.Identity,
                    _ => ResetButtonTokens.Identity
                };
                own = "DefaultColor";
                global = "ColorText";
                ownValue = globalValue = "#ff00b96b";
                observe = c => ((Button)c).Foreground;
                break;
            case "TimerStatistic" or "StatisticCountUp":
                source = new Statistic { Header = "Source", Value = 1234 };
                target = targetName == "TimerStatistic"
                    ? new TimerStatistic { Header = "Target", Value = DateTime.Now.AddDays(2), Formatter = static _ => "48:00:00" }
                    : new StatisticCountUp { EndValue = 1234 };
                sourceIdentity = StatisticTokens.Identity;
                targetIdentity = targetName == "TimerStatistic" ? TimerStatisticTokens.Identity : StatisticCountUpTokens.Identity;
                own = "ContentFontSize";
                global = "FontSizeHeading3";
                ownValue = "37";
                globalValue = "41";
                observe = c => c is AbstractStatistic statistic ? statistic.ContentFontSize : ((StatisticCountUp)c).FontSize;
                break;
            case "RangeDateViewer":
                source = DataFamilyEffectTests.CreateDateViewer();
                target = DataFamilyEffectTests.CreateRangeDateViewer();
                sourceIdentity = DateViewerTokens.Identity;
                targetIdentity = RangeDateViewerTokens.Identity;
                own = "PanelBg";
                global = "ColorBgContainer";
                ownValue = globalValue = "#ff00b96b";
                observe = c => ((Avalonia.Controls.Primitives.TemplatedControl)c).Background;
                break;
            case "TransferListView":
                source = DataFamilyEffectTests.CreateListView();
                target = DataFamilyEffectTests.CreateListView(transfer: true);
                sourceIdentity = ListViewTokens.Identity;
                targetIdentity = TransferListViewTokens.Identity;
                own = "ItemColor";
                global = "ColorTextSecondary";
                ownValue = globalValue = "#ff00b96b";
                observe = c => c.GetVisualDescendants().OfType<ListViewItem>().First().Foreground;
                break;
            default:
                source = DataFamilyEffectTests.CreateTreeView();
                target = DataFamilyEffectTests.CreateTreeView(transfer: true);
                sourceIdentity = TreeViewTokens.Identity;
                targetIdentity = TransferTreeViewTokens.Identity;
                own = "HeaderHeight";
                global = "ControlHeightSM";
                ownValue = "37";
                globalValue = "43";
                observe = c => c.GetVisualDescendants().OfType<TreeViewItemHeader>().First().MinHeight;
                break;
        }
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { source, target } }, (provider, capture) =>
        {
            using var initial = capture();
            var sourceBaseline = observe(source);
            var targetBaseline = observe(target);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (sourceIdentity, own, ownValue));
            using var sourceOwn = capture();
            observe(source).ShouldNotBe(sourceBaseline);
            observe(target).ShouldBe(targetBaseline, "Source Own leaked into " + targetName);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (targetIdentity, own, ownValue));
            using var targetOwn = capture();
            observe(source).ShouldBe(sourceBaseline);
            observe(target).ShouldNotBe(targetBaseline);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (sourceIdentity, global, globalValue));
            using var sourceGlobal = capture();
            observe(source).ShouldNotBe(sourceBaseline);
            observe(target).ShouldBe(targetBaseline, "Source global leaked into " + targetName);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (targetIdentity, global, globalValue));
            using var targetGlobal = capture();
            observe(source).ShouldBe(sourceBaseline);
            observe(target).ShouldNotBe(targetBaseline);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(root: (global, globalValue));
            using var root = capture();
            observe(source).ShouldNotBe(sourceBaseline);
            observe(target).ShouldNotBe(targetBaseline);
        });
    }

    [Fact]
    public void CountUp_Content_Uses_Explicit_Host_Font_And_Releases_On_Replacement()
    {
        var count = new StatisticCountUp { EndValue = 1234 };
        var host = new Statistic { Header = "Host", Content = count, ContentFontSize = 31 };
        FeedbackFamilyEffectTests.WithIsolationWindow(host, (provider, capture) =>
        {
            using var initial = capture();
            count.FontSize.ShouldBe(31);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (StatisticCountUpTokens.Identity, "ContentFontSize", "43"));
            using var own = capture();
            count.FontSize.ShouldBe(31, "The existing host typography contract must remain explicit.");
            count.FontSize = 47;
            host.ContentFontSize = 35;
            using var local = capture();
            count.FontSize.ShouldBe(47, "A caller's local child font must beat the host style binding.");
            host.Content = "replacement";
            count.ClearValue(Avalonia.Controls.Primitives.TemplatedControl.FontSizeProperty);
            var releasedValue = count.FontSize;
            host.ContentFontSize = 59;
            using var replaced = capture();
            count.FontSize.ShouldBe(releasedValue, "Replaced content retained the host typography binding.");
        });
    }

    [Theory]
    [InlineData("Button")]
    [InlineData("DropdownButton")]
    [InlineData("SubmitButton")]
    [InlineData("ResetButton")]
    public void Terminal_Explicit_Zero_LineHeight_Is_Used_By_Its_Padding(string name)
    {
        Button button = name switch
        {
            "DropdownButton" => new DropdownButton { Content = "Zero" },
            "SubmitButton" => new SubmitButton { Content = "Zero" },
            "ResetButton" => new ResetButton { Content = "Zero" },
            _ => new Button { Content = "Zero" }
        };
        var identity = name switch
        {
            "DropdownButton" => DropdownButtonTokens.Identity,
            "SubmitButton" => SubmitButtonTokens.Identity,
            "ResetButton" => ResetButtonTokens.Identity,
            _ => ButtonTokens.Identity
        };
        FeedbackFamilyEffectTests.WithIsolationWindow(button, (provider, capture) =>
        {
            provider.Config = new ThemeConfigBuilder().WithToken("EnableMotion", "false")
                .WithControl(identity, new ControlThemeConfigBuilder().WithToken("ContentLineHeight", "0").Build()).Build();
            using var changed = capture();
            var token = name switch
            {
                "DropdownButton" => ControlTokenAccessor.Capture<DropdownButton>(button),
                "SubmitButton" => ControlTokenAccessor.Capture<SubmitButton>(button),
                "ResetButton" => ControlTokenAccessor.Capture<ResetButton>(button),
                _ => ControlTokenAccessor.Capture<Button>(button)
            };
            var expected = token.GetEffectiveGlobal<double>(SharedTokenKind.ControlHeight) / 2 - token.GetEffectiveGlobal<double>(SharedTokenKind.LineWidth);
            button.Padding.Top.ShouldBe(expected);
        });
    }

    private static double Font(AbstractStatistic control, string token) => token == "ContentFontSize"
        ? control.ContentFontSize
        : control.GetVisualDescendants().OfType<ContentPresenter>().Single(part => part.Name == "HeaderPresenter").FontSize;
}
