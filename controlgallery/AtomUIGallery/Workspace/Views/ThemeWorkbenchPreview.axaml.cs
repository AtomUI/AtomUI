using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUIGallery.Workspace.Views;

public sealed record ThemeWorkbenchRow(string Name, int Value);

public static class ThemeWorkbenchFields
{
    public static DataGridFieldId Name { get; } = new("Name");
    public static DataGridFieldId Value { get; } = new("Value");
}

public partial class ThemeWorkbenchPreview : UserControl
{
    private DataGridLocalSource<ThemeWorkbenchRow>? _source;

    public ThemeWorkbenchPreview()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var descriptor = DataGridLocalSourceDescriptor.For<ThemeWorkbenchRow>(static row => DataGridRowKey.FromString(row.Name))
            .Field("Name", static row => row.Name).Field("Value", static row => row.Value);
        _source = DataGridLocalSource.Create(new[]
        {
            new ThemeWorkbenchRow("Alpha", 10), new ThemeWorkbenchRow("Beta", 20), new ThemeWorkbenchRow("Gamma", 30)
        }, descriptor);
        PreviewGrid.ItemsSource = _source;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        PreviewGrid.ItemsSource = null;
        _source?.Dispose();
        _source = null;
        base.OnDetachedFromVisualTree(e);
    }
}
