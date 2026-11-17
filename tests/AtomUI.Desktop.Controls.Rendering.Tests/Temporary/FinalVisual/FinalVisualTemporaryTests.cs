using System.Collections.Immutable;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

// TEMPORARY visual check (never committed): renders one page per theme in an atom:Window host (headless Skia
// compositor frame) and writes PNGs to the scratchpad.
public class FinalVisualTemporaryTests
{
    private static readonly string Dir = Environment.GetEnvironmentVariable("FINAL_PNG_DIR") ??
        "/private/tmp/claude-501/-Users-chinboy--rec-claude-accounts-53646-config-spares-spare-UH6cZ4/e33a347e-bd63-4ba1-804b-614f77ac4801/scratchpad/final-png";

    public sealed record Item(string Name);

    private sealed class Source(IReadOnlyList<Item> items) : IDataGridSource
    {
        private readonly DataGridSnapshotId _snapshot = new($"final-{Guid.NewGuid():N}");

        public DataGridSourceSchema Schema { get; } = new(typeof(Item),
            ImmutableArray<DataGridFieldSchema>.Empty, preferredRangeSize: 32, maximumRangeSize: 128);

        public event EventHandler? Invalidated
        {
            add { }
            remove { }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(DataGridFetchRequest request, CancellationToken cancellationToken)
        {
            var start   = Math.Min(request.Range.StartIndex, items.Count);
            var count   = Math.Min(request.Range.Count, items.Count - start);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
            for (var index = start; index < start + count; index++)
            {
                entries.Add(DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(index + 1L), items[index], index, index));
            }
            return ValueTask.FromResult(new DataGridRangeResult(start, entries.MoveToImmutable(),
                items.Count, items.Count, items.Count, _snapshot));
        }
    }

    public static TheoryData<string, string?, string> Themes()
    {
        var data = new TheoryData<string, string?, string>();
        var filter = Environment.GetEnvironmentVariable("FINAL_PNG_ONLY");
        foreach (var (id, seed, name) in new (string, string?, string)[]
                 {
                     ("PrimeNGAura", null, "primeng-aura-noir-light"),
                     ("PrimeNGAura.Dark", null, "primeng-aura-noir-dark"),
                     ("PrimeNGNora", "#6366F1", "primeng-nora-indigo-light"),
                     ("PrimeNGAura", "#10B981", "primeng-aura-emerald-light"),
                     ("DaybreakBlue", null, "antdesign-daybreakblue-light"),
                     ("ShadcnNova", null, "shadcn-nova-light"),
                     ("ShadcnNova.Dark", null, "shadcn-nova-dark")
                 })
        {
            if (filter is null || name.Contains(filter, StringComparison.Ordinal))
            {
                data.Add(id, seed, name);
            }
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(Themes))]
    public async Task Render(string themeId, string? seed, string name)
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();
        var config  = seed is null ? null : new ThemeConfigBuilder().WithToken("ColorPrimary", seed).Build();
        var applied = await manager.ApplyThemeAsync(new ThemeRequest(themeId, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        applied.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        Directory.CreateDirectory(Dir);

        var page   = new Page();
        var window = new AtomWindow { Width = 1180, Height = 1000, Content = page.Root };
        window.Show();
        try
        {
            Settle(window);
            page.AfterShow();
            Settle(window);
            Save(window, name + "-page");

            page.Select.IsDropDownOpen = true;
            Settle(window);
            Save(window, name + "-select");
            page.Select.IsDropDownOpen = false;
            Settle(window);

            page.PopoverTop.IsPopupPinnedOpen    = true;
            page.PopoverBottom.IsPopupPinnedOpen = true;
            page.PopoverTop.ShowFlyout(true);
            page.PopoverBottom.ShowFlyout(true);
            Settle(window);
            Save(window, name + "-popover");
            page.PopoverTop.IsPopupPinnedOpen    = false;
            page.PopoverBottom.IsPopupPinnedOpen = false;
            page.PopoverTop.HideFlyout(true);
            page.PopoverBottom.HideFlyout(true);
            Settle(window);

            page.ContextMenu.Open(page.ContextAnchor);
            Settle(window);
            Save(window, name + "-menu");
            page.ContextMenu.Close();
            Settle(window);

            page.DatePicker.IsPopupPinnedOpen = true;
            page.DatePicker.IsPickerOpen      = true;
            Settle(window);
            Save(window, name + "-datepicker");
            page.DatePicker.IsPopupPinnedOpen = false;
            page.DatePicker.IsPickerOpen      = false;
            Settle(window);

            var lunarWindow = new AtomWindow { Width = 1180, Height = 1900 };
            lunarWindow.Content = new StackPanel
            {
                Margin = new Thickness(16, 60, 16, 16), Spacing = 10,
                Children =
                {
                    new LunarCalendar { Fullscreen = true, Value = new DateTime(2026, 11, 12), Width = 1100 },
                    new LunarCalendar { Fullscreen = true, Mode = CalendarMode.Year, Value = new DateTime(2026, 11, 12), Width = 1100 }
                }
            };
            lunarWindow.Show();
            Settle(lunarWindow);
            Save(lunarWindow, name + "-lunar");
            lunarWindow.Close();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, null,
                ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
        }
    }

    private static void Settle(AtomWindow window)
    {
        for (var i = 0; i < 4; i++)
        {
            FrameCapture.Refresh(window);
            Thread.Sleep(60);
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Save(AtomWindow window, string file)
    {
        using var bitmap = FrameCapture.Capture(window);
        using var data   = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(Path.Combine(Dir, file + ".png"), data.ToArray());
    }

    private sealed class Page
    {
        public Page()
        {
            Select = new Select { Width = 200, IsMotionEnabled = false };
            foreach (var key in new[] { "Ada", "Grace", "Linus" })
            {
                Select.Options.Add(new SelectOption { ItemKey = key, Header = key });
            }
            Select.SelectedOption = (ISelectOption)Select.Options[1]!;

            PopoverTop = new FlyoutHost
            {
                Placement = PlacementMode.Top, IsMotionEnabled = false,
                Content   = new Button { Content = "Popover top" },
                Flyout    = new Flyout { Content = new TextBlock { Text = "Popover with arrow (top)" } }
            };
            PopoverBottom = new FlyoutHost
            {
                Placement = PlacementMode.Bottom, IsMotionEnabled = false,
                Content   = new Button { Content = "Popover bottom" },
                Flyout    = new Flyout { Content = new TextBlock { Text = "Popover with arrow (bottom)" } }
            };

            ContextAnchor = new Button { Content = "Context menu anchor" };
            ContextMenu   = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem { Header = "Copy" });
            ContextMenu.Items.Add(new MenuItem { Header = "Paste" });
            ContextMenu.Items.Add(new MenuItem { Header = "Delete" });
            ContextAnchor.ContextMenu = ContextMenu;

            DatePicker = new DatePicker { Width = 200, SelectedDateTime = new DateTime(2026, 11, 12), IsMotionEnabled = false };

            Grid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource         = new Source([new Item("Row one"), new Item("Selected row"), new Item("Row three")]),
                Width               = 240, Height = 150, IsMotionEnabled = false
            };
            Grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Item.Name)) });

            var child = new NavMenuNode { Header = "Selected item", ItemKey = "child" };
            var parent = new NavMenuNode { Header = "Submenu", ItemKey = "parent" };
            parent.Children.Add(new NavMenuNode { Header = "Option 1", ItemKey = "o1" });
            parent.Children.Add(child);
            NavMenu = new NavMenu { Mode = NavMenuMode.Inline, IsMotionEnabled = false, Width = 220 };
            NavMenu.Items.Add(new NavMenuNode { Header = "Home", ItemKey = "home" });
            NavMenu.Items.Add(parent);
            NavMenu.Items.Add(new NavMenuNode { Header = "Settings", ItemKey = "settings" });
            NavMenu.DefaultOpenPaths = [new TreeNodePath("parent")];
            NavMenu.DefaultSelectedPath = new TreeNodePath(["parent", "child"]);

            var horizontal = new NavMenu { Mode = NavMenuMode.Horizontal, IsMotionEnabled = false, Width = 360 };
            horizontal.Items.Add(new NavMenuNode { Header = "Home", ItemKey = "home" });
            horizontal.Items.Add(new NavMenuNode { Header = "Selected", ItemKey = "sel" });
            horizontal.Items.Add(new NavMenuNode { Header = "About", ItemKey = "about" });
            horizontal.DefaultSelectedPath = new TreeNodePath("sel");

            var tabs = new TabControl { Width = 300, IsMotionEnabled = false };
            tabs.Items.Add(new TabItem { Header = "Tab one", Content = new TextBlock { Text = "Content one" } });
            tabs.Items.Add(new TabItem { Header = "Tab two", Content = new TextBlock { Text = "Content two" } });
            tabs.SelectedIndex = 0;

            var lunar = new LunarCalendar { Fullscreen = false, Value = new DateTime(2026, 11, 12), Width = 300 };
            var compact = new Calendar { Fullscreen = false, Value = new DateTime(2026, 11, 12), Width = 280 };

            Row Buttons() => new(
                new Button { ButtonType = ButtonType.Primary, Content = "Primary" },
                new Button { Content = "Default" },
                new Button { ButtonType = ButtonType.Text, Content = "Text" },
                new Button { ButtonType = ButtonType.Link, Content = "Link" });

            var left = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    Buttons(),
                    new Row(new CheckBox { Content = "CheckBox", IsChecked = true },
                        new RadioButton { Content = "Radio", IsChecked = true },
                        new ToggleSwitch { IsChecked = true }, new ToggleSwitch { IsChecked = false }),
                    tabs,
                    new Row(Select, DatePicker),
                    new Row(PopoverTop, PopoverBottom, ContextAnchor),
                    new Row(NavMenu, Grid),
                    horizontal
                }
            };
            var right = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new ProgressBar { Value = 60, Width = 300, PercentPosition = new PercentPosition { IsInner = true } , IndicatorThickness = 18},
                    new ProgressBar { Value = 0, Width = 300, PercentPosition = new PercentPosition { IsInner = true }, IndicatorThickness = 18 },
                    new ProgressBar { Value = 45, Width = 300 },
                    new Row(new CircleProgress { Value = 70 }, new StepsProgressBar { Value = 50, Steps = 5 }),
                    new Row(lunar),
                    new Row(compact)
                }
            };
            Root = new Border
            {
                Padding = new Thickness(16, 70, 16, 16),
                Child   = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 30, Children = { left, right } }
            };
        }

        public Control Root { get; }
        public Select Select { get; }
        public FlyoutHost PopoverTop { get; }
        public FlyoutHost PopoverBottom { get; }
        public Button ContextAnchor { get; }
        public ContextMenu ContextMenu { get; }
        public DatePicker DatePicker { get; }
        public DataGrid Grid { get; }
        public NavMenu NavMenu { get; }

        public void AfterShow()
        {
            Grid.GetVisualDescendants().OfType<DataGridRow>().ElementAt(1).IsSelected = true;
        }
    }

    private sealed class Row : StackPanel
    {
        public Row(params Control[] children)
        {
            Orientation = Orientation.Horizontal;
            Spacing     = 12;
            foreach (var child in children)
            {
                Children.Add(child);
            }
        }
    }
}
