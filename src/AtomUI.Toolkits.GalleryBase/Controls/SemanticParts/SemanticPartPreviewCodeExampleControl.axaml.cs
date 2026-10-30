using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed partial class SemanticPartPreviewCodeExampleControl : UserControl
{
    private readonly ContentControl _codeHost;

    public SemanticPartPreviewCodeExampleControl()
    {
        InitializeComponent();
        _codeHost = this.FindControl<ContentControl>("PART_CodeHost")!;
    }

    internal void Show(GalleryCodeViewer codeViewer)
    {
        _codeHost.Content = codeViewer;
    }

    internal void Clear()
    {
        _codeHost.Content = null;
    }
}
