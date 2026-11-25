using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Models;

public record PaletteColorInfo
{
    public IBrush Brush { get; }
    public IBrush TextBrush { get; }
    public string DisplayName { get; }
    public string Hex { get; }

    public PaletteColorInfo(string displayName, IBrush brush, bool light, int index)
    {
        DisplayName = displayName;
        Brush       = brush;
        Hex         = brush.ToString()?.ToUpperInvariant() ?? "Invalid Color";
        if ((light && index < 5) || (!light && index >= 5))
        {
            // Reference palette swatches use absolute black/white contrast data, independent of UI theme colors.
#pragma warning disable ATOMUIGEN044
            TextBrush = Brushes.Black;
#pragma warning restore ATOMUIGEN044
        }
        else
        {
            // Reference palette swatches use absolute black/white contrast data, independent of UI theme colors.
#pragma warning disable ATOMUIGEN044
            TextBrush = Brushes.White;
#pragma warning restore ATOMUIGEN044
        }
    }
}
