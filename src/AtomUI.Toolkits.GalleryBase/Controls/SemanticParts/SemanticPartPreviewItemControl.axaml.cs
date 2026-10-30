using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed partial class SemanticPartPreviewItemControl : UserControl
{
    public static readonly StyledProperty<SemanticPartPreviewItem?> ItemProperty =
        AvaloniaProperty.Register<SemanticPartPreviewItemControl, SemanticPartPreviewItem?>(nameof(Item));

    public SemanticPartPreviewItemControl()
    {
        InitializeComponent();
        AddHandler(Button.ClickEvent, HandleButtonClick);
    }

    public SemanticPartPreviewItem? Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemProperty)
        {
            DataContext = Item;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (Item is { } item)
        {
            this.FindAncestorOfType<SemanticPartPreview>()?.SetHoveredPart(item, true);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        if (Item is { } item)
        {
            this.FindAncestorOfType<SemanticPartPreview>()?.SetHoveredPart(item, false);
        }
        base.OnPointerExited(e);
    }

    private void HandleButtonClick(object? sender, RoutedEventArgs e)
    {
        if (Item is not { } item ||
            e.Source is not Control source ||
            this.FindAncestorOfType<SemanticPartPreview>() is not { } preview)
        {
            return;
        }

        if (source.Name == "PART_PinButton")
        {
            preview.TogglePinnedPart(item);
            e.Handled = true;
        }
        else if (source.Name == "PART_InfoButton")
        {
            preview.ShowPartInfo(item);
            e.Handled = true;
        }
    }
}
