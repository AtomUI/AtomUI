using AtomUI.DesignLanguages.AntDesign;
using System.Collections.Immutable;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

/// <summary>
/// The DataGrid selected row under the PrimeNG presets (the Gallery registers both packages): its text is PrimeUIX
/// <c>highlight.color</c>, written once to the global <c>ControlItemColorActive</c> and taken by the DataGrid's
/// <c>RowSelectedColor</c>, on the solid highlight of noir and Nora.
/// </summary>
public class PrimeNGDataGridHighlightTests
{
    public sealed record Item(string Name);

    private sealed class Source(IReadOnlyList<Item> items) : IDataGridSource
    {
        private readonly DataGridSnapshotId _snapshot = new($"highlight-{Guid.NewGuid():N}");

        public DataGridSourceSchema Schema { get; } = new(typeof(Item),
            ImmutableArray<DataGridFieldSchema>.Empty, preferredRangeSize: 32, maximumRangeSize: 128);

        public event EventHandler? Invalidated
        {
            add { }
            remove { }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(DataGridFetchRequest request, CancellationToken cancellationToken)
        {
            var start = Math.Min(request.Range.StartIndex, items.Count);
            var count = Math.Min(request.Range.Count, items.Count - start);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
            for (var index = start; index < start + count; index++)
            {
                entries.Add(DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(index + 1L), items[index], index, index));
            }
            return ValueTask.FromResult(new DataGridRangeResult(start, entries.MoveToImmutable(),
                items.Count, items.Count, items.Count, _snapshot));
        }
    }

    // The v1 profiles PrimeNGAura, PrimeNGAura.Dark, PrimeNGNora and PrimeNGNora.Dark are the PrimeNG language's
    // Aura and Nora styles in the light and dark appearances.
    [Theory]
    [InlineData("Aura", false, null, "#020617", "#FFFFFF")]
    [InlineData("Aura", true, null, "#FAFAFA", "#09090B")]
    [InlineData("Nora", false, "#10B981", "#059669", "#FFFFFF")]
    [InlineData("Nora", true, "#10B981", "#10B981", "#09090B")]
    [InlineData("Nora", false, "#6366F1", "#4F46E5", "#FFFFFF")]
    [InlineData("Nora", true, "#6366F1", "#6366F1", "#09090B")]
    public async Task Selected_Row_Text_Is_The_Highlight_Color(string style, bool dark, string? seed, string background, string text)
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();
        // Motion off: the selected row's Foreground transitions with its background and would be read mid-way.
        var selection = ThemeSelection.Of("PrimeNG", new DesignStyle(style))
                                      .WithAppearance(dark ? ThemeAppearance.Dark : ThemeAppearance.Light)
                                      .WithSeed("EnableMotion", "false");
        if (seed is not null)
        {
            selection = selection.WithSeed("ColorPrimary", seed);
        }
        (await manager.ApplyAsync(selection, TestContext.Current.CancellationToken)).Status
            .ShouldBe(ThemeTransitionStatus.Committed);
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = new Source([new Item("one"), new Item("two")]),
            Width = 360,
            Height = 200
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Item.Name)) });
        var window = new AvaloniaWindow { Width = 420, Height = 260, Content = grid };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var row = grid.GetVisualDescendants().OfType<DataGridRow>().First();
            row.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var tokens = ControlTokenAccessor.Capture(grid);
            tokens.GetEffectiveGlobal<Color>(SharedTokenKind.ControlItemBgActive).ShouldBe(Color.Parse(background));
            tokens.GetEffectiveGlobal<Color>(SharedTokenKind.ControlItemColorActive).ShouldBe(Color.Parse(text));
            tokens.GetOwn<Color>(DataGridTokenKind.RowSelectedColor).ShouldBe(Color.Parse(text));
            ((ISolidColorBrush)row.Foreground!).Color.ShouldBe(Color.Parse(text));
        }
        finally
        {
            window.Close();
            await manager.ApplyThemeAsync(new ThemeRequest(AntDesignThemes.DaybreakBlue, null,
                ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
        }
    }
}
