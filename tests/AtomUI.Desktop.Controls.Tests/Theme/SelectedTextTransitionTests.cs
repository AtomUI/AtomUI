using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Animations;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomTreeView = AtomUI.Desktop.Controls.TreeView;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;
using AtomComboBoxItem = AtomUI.Desktop.Controls.ComboBoxItem;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>
/// Where a selected item's background transitions, its selected text (ControlItemColorActive and the tokens defaulting
/// to it) transitions with it, so a contrast text colour never sits on the not-yet-changed background. The transitions
/// exist only while motion is enabled.
/// </summary>
public class SelectedTextTransitionTests
{
    static SelectedTextTransitionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task TreeView_Header_Text_Transitions_With_Its_Background_Only_With_Motion()
    {
        var manager = GetThemeManager();
        await Apply(manager, null);
        var tree = new AtomTreeView();
        tree.Items.Add(new AtomTreeViewItem { Header = "Node", IsSelected = true });
        await Show(tree, async () =>
        {
            HasForegroundTransition(HeaderPresenter(tree)).ShouldBeTrue();

            await Apply(manager, new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build());
            Dispatcher.UIThread.RunJobs();
            HasForegroundTransition(HeaderPresenter(tree)).ShouldBeFalse();
        });
        await Apply(manager, null);
    }

    [Fact]
    public async Task ComboBox_Item_Text_Transitions_With_Its_Background_Only_With_Motion()
    {
        var manager = GetThemeManager();
        await Apply(manager, null);
        // The item takes IsMotionEnabled from its ComboBox; standalone it is set directly.
        var item = new AtomComboBoxItem { Content = "Item", IsSelected = true };
        item.SetValue(AtomUI.Controls.MotionAwareControlProperty.IsMotionEnabledProperty, true);
        await Show(item, () =>
        {
            HasForegroundTransition(item).ShouldBeTrue();

            item.SetValue(AtomUI.Controls.MotionAwareControlProperty.IsMotionEnabledProperty, false);
            Dispatcher.UIThread.RunJobs();
            HasForegroundTransition(item).ShouldBeFalse();
            return Task.CompletedTask;
        });
        await Apply(manager, null);
    }

    [Fact]
    public async Task Compact_Date_Cell_Text_Transitions_With_Its_Value_Frame_Only_With_Motion()
    {
        // The compact (picker) cell fills its value frame with the primary on selection (MotionDurationMid) and turns its
        // text to the contrast colour: the text transitions over the same duration. The content presentation keeps
        // its own (MotionDurationSlow) transition.
        var manager = GetThemeManager();
        await Apply(manager, null);
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 11, 1), Value = new DateTime(2026, 11, 12) };
        await Show(viewer, async () =>
        {
            var value = CellValue(viewer);
            HasForegroundTransition(value).ShouldBeTrue();
            ForegroundTransitionDuration(value).ShouldBe(ValueFrameBackgroundDuration(viewer));

            await Apply(manager, new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build());
            Dispatcher.UIThread.RunJobs();
            HasForegroundTransition(CellValue(viewer)).ShouldBeFalse();
        });
        await Apply(manager, null);
    }

    private static AtomUI.Desktop.Controls.TextBlock CellValue(DateViewer viewer) =>
        viewer.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>().First()
              .GetVisualDescendants().OfType<AtomUI.Desktop.Controls.TextBlock>().First(text => text.Name == "PART_Value");

    private static TimeSpan ForegroundTransitionDuration(Animatable target) =>
        target.Transitions!.OfType<SolidColorBrushTransition>().Single(transition => transition.Property?.Name == "Foreground").Duration;

    private static TimeSpan ValueFrameBackgroundDuration(DateViewer viewer) =>
        viewer.GetVisualDescendants().OfType<Avalonia.Controls.Border>().First(border => border.Name == "PART_ValueFrame")
              .Transitions!.OfType<SolidColorBrushTransition>().Single(transition => transition.Property?.Name == "Background").Duration;

    private static ContentPresenter HeaderPresenter(AtomTreeView tree) =>
        tree.GetVisualDescendants().OfType<ContentPresenter>().First(presenter => presenter.Name == "HeaderPresenter");

    private static bool HasForegroundTransition(Animatable target) =>
        target.Transitions?.OfType<SolidColorBrushTransition>()
              .Any(transition => transition.Property?.Name == "Foreground") == true;

    private static async Task Show(Control content, Func<Task> assertion)
    {
        var window = new AvaloniaWindow { Width = 300, Height = 200, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static ThemeManager GetThemeManager() =>
        Application.Current!.GetThemeManager().ShouldBeOfType<ThemeManager>();

    private static async Task Apply(ThemeManager manager, ThemeConfig? config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(AntDesignThemes.DaybreakBlue, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
