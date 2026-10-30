using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed partial class SemanticPartPreviewInfoControl : UserControl
{
    public SemanticPartPreviewInfoControl()
    {
        InitializeComponent();
    }

    internal void Show(SemanticPartPreviewItem item)
    {
        DataContext = item;
    }

    internal void Clear()
    {
        DataContext = null;
    }
}
