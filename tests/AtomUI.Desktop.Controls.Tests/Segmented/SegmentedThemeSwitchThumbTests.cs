using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Segmented;

/// <summary>
/// Switching the theme re-templates the items and changes the track geometry; the selected thumb must land
/// exactly on the selected item afterwards (same offset, same size), not keep a stretched pre-switch shape.
/// </summary>
public class SegmentedThemeSwitchThumbTests
{
    static SegmentedThemeSwitchThumbTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData("DaybreakBlue", "ShadcnMaia")]
    [InlineData("ShadcnNova", "ShadcnMaia")]
    [InlineData("ShadcnMaia", "ShadcnNova")]
    public async Task Thumb_Lands_On_The_Selected_Item_After_The_Theme_Switch(string from, string to)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var noMotion = new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build();
        await manager.ApplyThemeAsync(new ThemeRequest(from, noMotion, ThemeTransitionReason.UserRequest));
        var segmented = new AtomUI.Desktop.Controls.Segmented();
        for (var i = 1; i <= 4; i++)
        {
            segmented.Items.Add(new AtomUI.Desktop.Controls.SegmentedItem { Content = $"Item {i}" });
        }
        segmented.SelectedIndex = 1;
        var window = new AtomUIWindow { Width = 420, Height = 200, Content = new Avalonia.Controls.Panel { Children = { segmented } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await manager.ApplyThemeAsync(new ThemeRequest(to, noMotion, ThemeTransitionReason.UserRequest));
            // The theme transition itself settles on the dispatcher; let it finish before judging.
            await Task.Delay(600);
            Dispatcher.UIThread.RunJobs();

            var selectedItem = segmented.SelectedItem.ShouldNotBeNull();
            var item = segmented.ContainerFromItem(selectedItem).ShouldNotBeNull("selected container");
            var offset = item.TranslatePoint(default, segmented).ShouldNotBeNull("translate");
            segmented.SelectedThumbPos.X.ShouldBe(offset.X, 1.5, $"pos.X after {from}->{to}");
            segmented.SelectedThumbPos.Y.ShouldBe(offset.Y, 1.5, $"pos.Y after {from}->{to}");
            segmented.SelectedThumbSize.Width.ShouldBe(item.Bounds.Width, 1.5, $"size.W after {from}->{to}");
            segmented.SelectedThumbSize.Height.ShouldBe(item.Bounds.Height, 1.5, $"size.H after {from}->{to}");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest));
        }
    }

    [Theory]
    [InlineData("ShadcnNova", "ShadcnMaia")]
    public async Task Thumb_Never_Stretches_To_The_Track_Width_During_The_Theme_Switch(string from, string to)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var noMotion = new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build();
        await manager.ApplyThemeAsync(new ThemeRequest(from, noMotion, ThemeTransitionReason.UserRequest));
        var segmented = new AtomUI.Desktop.Controls.Segmented();
        for (var i = 1; i <= 4; i++)
        {
            segmented.Items.Add(new AtomUI.Desktop.Controls.SegmentedItem { Content = $"Item {i}" });
        }
        segmented.SelectedIndex = 1;
        var window = new AtomUIWindow { Width = 420, Height = 200, Content = new Avalonia.Controls.Panel { Children = { segmented } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var trackWidth = segmented.Bounds.Width;
            var log = new List<string>();
            for (var tick = 0; tick < 12; tick++)
            {
                await Task.Delay(50);
                Dispatcher.UIThread.RunJobs();
                log.Add($"t={tick * 50}ms thumb={segmented.SelectedThumbPos.X:0.#},{segmented.SelectedThumbSize.Width:0.#}x{segmented.SelectedThumbSize.Height:0.#}");
            }
            // begin the switch and sample through the transition
            await manager.ApplyThemeAsync(new ThemeRequest(to, noMotion, ThemeTransitionReason.UserRequest));
            var worst = 0d;
            for (var tick = 0; tick < 12; tick++)
            {
                await Task.Delay(50);
                Dispatcher.UIThread.RunJobs();
                worst = Math.Max(worst, segmented.SelectedThumbSize.Width);
            }
            var item = segmented.ContainerFromItem(segmented.SelectedItem.ShouldNotBeNull()).ShouldNotBeNull();
            // The thumb may never claim (nearly) the whole track, during or after the switch.
            worst.ShouldBeLessThan(trackWidth * 0.6,
                $"worst={worst:0.#} track={trackWidth:0.#} item={item.Bounds.Width:0.#} settled={segmented.SelectedThumbSize.Width:0.#}");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest));
        }
    }

    [Fact]
    public async Task Thumb_Recalibrates_When_The_Items_Relayout_Without_Resizing_The_Segmented()
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var noMotion = new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build();
        await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, noMotion, ThemeTransitionReason.UserRequest));
        var segmented = new AtomUI.Desktop.Controls.Segmented { Width = 320, IsExpanding = true };
        var items = new List<AtomUI.Desktop.Controls.SegmentedItem>();
        for (var i = 1; i <= 4; i++)
        {
            var item = new AtomUI.Desktop.Controls.SegmentedItem { Content = $"Item {i}" };
            items.Add(item);
            segmented.Items.Add(item);
        }
        segmented.SelectedIndex = 0;
        var window = new AtomUIWindow { Width = 420, Height = 200, Content = new Avalonia.Controls.Panel { Children = { segmented } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var before = segmented.SelectedThumbSize.Width;

            // Hiding an item re-divides the expanding panel (each item widens), but the Segmented itself keeps
            // its explicit Width — its own ArrangeOverride does not run again, only the items panel relayouts.
            items[3].IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            var item0 = segmented.ContainerFromItem(segmented.SelectedItem.ShouldNotBeNull()).ShouldNotBeNull();
            segmented.SelectedThumbSize.Width.ShouldBe(item0.Bounds.Width, 1.5,
                $"thumb {segmented.SelectedThumbSize.Width:0.#} must follow the re-divided item {item0.Bounds.Width:0.#} (was {before:0.#})");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest));
        }
    }
}
