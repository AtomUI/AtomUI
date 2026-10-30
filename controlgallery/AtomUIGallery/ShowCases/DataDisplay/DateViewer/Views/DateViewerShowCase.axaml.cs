using AtomUI.Controls;
using Avalonia.Interactivity;

namespace AtomUIGallery.ShowCases.DateViewer;

public partial class DateViewerShowCase : GalleryReactiveUserControl<DateViewerViewModel>
{
    public const string LanguageId = nameof(DateViewerShowCase);

    public DateViewerShowCase() => InitializeComponent();

    private void HandleSelectionUnitChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is DateViewerViewModel viewModel && args.Index is >= 0 and <= 4)
        {
            viewModel.SelectionUnit = (AtomUI.Desktop.Controls.DateViewerSelectionUnit)args.Index;
        }
    }

    private void ClearSelection(object? sender, RoutedEventArgs args)
    {
        if (DataContext is DateViewerViewModel viewModel)
        {
            viewModel.Value = null;
            viewModel.RangeValue = null;
        }
    }

    private void ClearBrowsingSelection(object? sender, RoutedEventArgs args)
    {
        if (DataContext is DateViewerViewModel viewModel)
        {
            viewModel.EmptyValue = null;
        }
    }
}
