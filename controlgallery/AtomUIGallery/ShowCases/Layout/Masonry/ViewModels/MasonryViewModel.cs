using System.Collections.ObjectModel;
using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Masonry;

public class MasonryViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "MasonryShowCase";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private ObservableCollection<MasonryBasicItem>? _basicItems;
    private ObservableCollection<MasonryBasicItem>? _semanticItems;
    private ObservableCollection<MasonryBasicItem>? _responsiveItems;
    private ObservableCollection<MasonryImageItem>? _imageItems;
    private ObservableCollection<MasonryDynamicItem>? _dynamicItems;
    private readonly Random _dynamicItemHeightRandom = new();

    /// <summary>
    /// Sample items for the basic example, mirroring the Ant Design Masonry basic demo:
    /// 15 cards of varying heights, where the 5th item (index 4) is a special card with
    /// a cover image.
    /// </summary>
    public ObservableCollection<MasonryBasicItem>? BasicItems
    {
        get => _basicItems;
        private set => this.RaiseAndSetIfChanged(ref _basicItems, value);
    }

    public ObservableCollection<MasonryBasicItem>? SemanticItems
    {
        get => _semanticItems;
        private set => this.RaiseAndSetIfChanged(ref _semanticItems, value);
    }

    public ObservableCollection<MasonryBasicItem>? ResponsiveItems
    {
        get => _responsiveItems;
        private set => this.RaiseAndSetIfChanged(ref _responsiveItems, value);
    }

    public ObservableCollection<MasonryImageItem>? ImageItems
    {
        get => _imageItems;
        private set => this.RaiseAndSetIfChanged(ref _imageItems, value);
    }

    public ObservableCollection<MasonryDynamicItem>? DynamicItems
    {
        get => _dynamicItems;
        private set => this.RaiseAndSetIfChanged(ref _dynamicItems, value);
    }

    public ReactiveCommand<Unit, Unit> AddDynamicMasonryItemCommand { get; }

    public ReactiveCommand<int, Unit> RemoveDynamicMasonryItemCommand { get; }

    public MasonryViewModel(IScreen screen)
    {
        HostScreen = screen;
        EnsureBasicItems();
        EnsureSemanticItems();
        EnsureResponsiveItems();
        EnsureImageItems();
        EnsureDynamicItems();

        AddDynamicMasonryItemCommand    = ReactiveCommand.Create(AddDynamicMasonryItem);
        RemoveDynamicMasonryItemCommand = ReactiveCommand.Create<int>(RemoveDynamicMasonryItem);
    }

    private void EnsureBasicItems()
    {
        if (BasicItems is not null)
        {
            return;
        }

        // Heights mirror the Ant Design Masonry basic demo. The 5th item (index 4) provides
        // custom children, so its rendered height comes from the special card content.
        var heights = new double[] { 150, 50, 90, 70, 110, 150, 130, 80, 50, 90, 100, 150, 60, 50, 80 };
        var items = new ObservableCollection<MasonryBasicItem>();
        for (var i = 0; i < heights.Length; i++)
        {
            // The 5th item (index 4) is the special card with a cover image.
            if (i == 4)
            {
                items.Add(new MasonryBasicItem(
                    index: i + 1,
                    height: heights[i],
                    isSpecial: true,
                    coverSource: ImageSource.Parse(
                        "https://images.unsplash.com/photo-1491961865842-98f7befd1a60?w=523&auto=format"),
                    title: "I'm Special",
                    description: "Let's have a meal"));
            }
            else
            {
                items.Add(new MasonryBasicItem(index: i + 1, height: heights[i]));
            }
        }

        BasicItems = items;
    }

    private void EnsureSemanticItems()
    {
        if (SemanticItems is not null)
        {
            return;
        }

        // Mirrors the Ant Design Masonry semantic DOM demo: 7 cards with the same
        // shortest-column rhythm as the upstream object/function style examples.
        var heights = new double[] { 75, 50, 70, 60, 85, 75, 50 };
        var items = new ObservableCollection<MasonryBasicItem>();
        for (var i = 0; i < heights.Length; i++)
        {
            items.Add(new MasonryBasicItem(index: i + 1, height: heights[i]));
        }

        SemanticItems = items;
    }

    private void EnsureResponsiveItems()
    {
        if (ResponsiveItems is not null)
        {
            return;
        }

        var heights = new double[] { 120, 55, 85, 160, 95, 140, 75, 110, 65, 130, 90, 145, 55, 100, 80 };
        var items = new ObservableCollection<MasonryBasicItem>();
        for (var i = 0; i < heights.Length; i++)
        {
            items.Add(new MasonryBasicItem(index: i + 1, height: heights[i]));
        }

        ResponsiveItems = items;
    }

    private void EnsureImageItems()
    {
        if (ImageItems is not null)
        {
            return;
        }

        var imageSources = new[]
        {
            "https://images.unsplash.com/photo-1510001618818-4b4e3d86bf0f?w=523&auto=format",
            "https://images.unsplash.com/photo-1507513319174-e556268bb244?w=523&auto=format",
            "https://images.unsplash.com/photo-1474181487882-5abf3f0ba6c2?w=523&auto=format",
            "https://images.unsplash.com/photo-1492778297155-7be4c83960c7?w=523&auto=format",
            "https://images.unsplash.com/photo-1508062878650-88b52897f298?w=523&auto=format",
            "https://images.unsplash.com/photo-1506158278516-d720e72406fc?w=523&auto=format",
            "https://images.unsplash.com/photo-1552203274-e3c7bd771d26?w=523&auto=format",
            "https://images.unsplash.com/photo-1528163186890-de9b86b54b51?w=523&auto=format",
            "https://images.unsplash.com/photo-1727423304224-6d2fd99b864c?w=523&auto=format",
            "https://images.unsplash.com/photo-1675090391405-432434e23595?w=523&auto=format",
            "https://images.unsplash.com/photo-1554196967-97a8602084d9?w=523&auto=format",
            "https://images.unsplash.com/photo-1491961865842-98f7befd1a60?w=523&auto=format",
            "https://images.unsplash.com/photo-1721728613411-d56d2ddda959?w=523&auto=format",
            "https://images.unsplash.com/photo-1731901245099-20ac7f85dbaa?w=523&auto=format",
            "https://images.unsplash.com/photo-1617694455303-59af55af7e58?w=523&auto=format",
            "https://images.unsplash.com/photo-1709198165282-1dab551df890?w=523&auto=format"
        };

        var items = new ObservableCollection<MasonryImageItem>();
        for (var i = 0; i < imageSources.Length; i++)
        {
            items.Add(new MasonryImageItem(i + 1, ImageSource.Parse(imageSources[i])));
        }

        ImageItems = items;
    }

    private void EnsureDynamicItems()
    {
        if (DynamicItems is not null)
        {
            return;
        }

        var heights = new[] { 150, 50, 90, 70, 110, 150, 130, 80, 50, 90, 100, 150, 70, 50, 80 };
        var items = new ObservableCollection<MasonryDynamicItem>();
        for (var i = 0; i < heights.Length; i++)
        {
            items.Add(new MasonryDynamicItem(i, heights[i], i % 4));
        }

        DynamicItems = items;
    }

    public void UpdateDynamicMasonryColumns(IReadOnlyList<MasonryItemLayout> layouts)
    {
        if (DynamicItems is null)
        {
            return;
        }

        foreach (var layout in layouts)
        {
            if (layout.Index >= 0 && layout.Index < DynamicItems.Count)
            {
                DynamicItems[layout.Index].Column = layout.Column;
            }
        }
    }

    private void AddDynamicMasonryItem()
    {
        if (DynamicItems is null)
        {
            return;
        }

        var key = DynamicItems.Count > 0 ? DynamicItems[^1].Key + 1 : 0;
        DynamicItems.Add(new MasonryDynamicItem(key, _dynamicItemHeightRandom.Next(50, 150)));
    }

    public void RemoveDynamicMasonryItem(int key)
    {
        if (DynamicItems is null)
        {
            return;
        }

        for (var i = 0; i < DynamicItems.Count; i++)
        {
            if (DynamicItems[i].Key == key)
            {
                DynamicItems.RemoveAt(i);
                return;
            }
        }
    }

}

public sealed record MasonryImageItem(int Index, ImageSource ImageSource);

public sealed class MasonryDynamicItem : ReactiveObject
{
    private int? _column;

    public int Key { get; }

    public int Height { get; }

    public int? Column
    {
        get => _column;
        set => this.RaiseAndSetIfChanged(ref _column, value);
    }

    public string DisplayText => (Key + 1).ToString();

    public MasonryDynamicItem(int key, int height, int? column = null)
    {
        Key     = key;
        Height  = height;
        _column = column;
    }
}

/// <summary>
/// Data item for the basic Masonry example. Regular items render as a small Card with the
/// given height and a 1-based number. The special item renders a Card with a cover image.
/// </summary>
public sealed class MasonryBasicItem
{
    public int Index { get; }
    public double Height { get; }
    public bool IsSpecial { get; }
    public ImageSource? CoverSource { get; }
    public string? Title { get; }
    public string? Description { get; }

    public MasonryBasicItem(int index, double height, bool isSpecial = false,
        ImageSource? coverSource = null, string? title = null, string? description = null)
    {
        Index       = index;
        Height      = height;
        IsSpecial   = isSpecial;
        CoverSource = coverSource;
        Title       = title;
        Description = description;
    }
}
