using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Splitter;

public partial class SplitterShowCase : GalleryReactiveUserControl<SplitterViewModel>
{
    public const string LanguageId = nameof(SplitterShowCase);

    public SplitterShowCase()
    {
        InitializeComponent();
    }

    private void HandleShowCollapsibleIconChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIRadioButton { IsChecked: true } radioButton)
        {
            return;
        }

        var mode = ParseShowMode(radioButton.Tag);
        if (mode == null)
        {
            return;
        }

        UpdateShowCollapsibleIconMode(mode.Value);
    }

    private SplitterCollapsibleIconDisplayMode? ParseShowMode(object? tag)
    {
        if (tag is SplitterCollapsibleIconDisplayMode mode)
        {
            return mode;
        }

        if (tag is string text &&
            Enum.TryParse<SplitterCollapsibleIconDisplayMode>(text, true, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private void UpdateShowCollapsibleIconMode(SplitterCollapsibleIconDisplayMode mode)
    {
        foreach (var panel in ExamplesContent.GetVisualDescendants().OfType<Control>())
        {
            if (panel.Name is "ShowCollapsiblePanelFirst" or "ShowCollapsiblePanelSecond" or "ShowCollapsiblePanelThird")
            {
                ApplyShowMode(panel, mode);
            }
        }
    }

    private static void ApplyShowMode(Control panel, SplitterCollapsibleIconDisplayMode mode)
    {
        AtomUISplitter.SetCollapsible(panel, new SplitterPanelCollapsible()
        {
            IsEnabled           = true,
            ShowCollapsibleIcon = mode
        });
    }

    private void HandleDraggerDoubleClicked(object? sender, SplitterDraggerDoubleClickedEventArgs e)
    {
        if (sender is not AtomUISplitter splitter)
        {
            return;
        }

        foreach (var panel in splitter.Children)
        {
            var defaultSize = AtomUISplitter.GetDefaultSize(panel);
            if (defaultSize.HasValue)
            {
                AtomUISplitter.SetSize(panel, defaultSize);
            }
        }
    }
}
