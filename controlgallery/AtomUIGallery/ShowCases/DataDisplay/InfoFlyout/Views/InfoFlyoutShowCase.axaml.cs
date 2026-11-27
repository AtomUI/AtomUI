using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

namespace AtomUIGallery.ShowCases.InfoFlyout;

public partial class InfoFlyoutShowCase : GalleryReactiveUserControl<InfoFlyoutViewModel>
{
    public const string LanguageId = nameof(InfoFlyoutShowCase);

    private SemanticPartPreview? _semanticPreview;
    private AvaloniaPopup? _semanticPreviewPopup;

    public InfoFlyoutShowCase()
    {
        InitializeComponent();
    }

    private void HandleArrowSegmentedSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (DataContext is InfoFlyoutViewModel viewModel)
        {
            viewModel.HandleSelectionChanged(sender, args);
        }
    }

    // The InfoFlyout popup is created in code and lives outside the owner's visual root.
    // SemanticPartHighlightSession only discovers popups inside the template, so the popup
    // root (FlyoutPresenter) is registered in SemanticPartPreview.AdditionalRoots when it opens.
    // That lets the highlight session resolve popup.container, popup.content and popup.arrow.
    private void HandleSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        if (sender is not SemanticPartPreview preview ||
            preview.PreviewContent is not FlyoutHost host ||
            host.Flyout is not { } flyout)
        {
            return;
        }

        _semanticPreview      = preview;
        _semanticPreviewPopup = flyout.Popup;
        _semanticPreviewPopup.Opened += HandleSemanticPreviewPopupOpened;
        _semanticPreviewPopup.Closed += HandleSemanticPreviewPopupClosed;
        RegisterSemanticPreviewRoot();
    }

    private void HandleSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        if (_semanticPreviewPopup is { } popup)
        {
            popup.Opened -= HandleSemanticPreviewPopupOpened;
            popup.Closed -= HandleSemanticPreviewPopupClosed;
        }

        _semanticPreview?.AdditionalRoots.Clear();
        _semanticPreviewPopup = null;
        _semanticPreview      = null;
    }

    private void HandleSemanticPreviewPopupOpened(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewRoot();
    }

    private void HandleSemanticPreviewPopupClosed(object? sender, EventArgs args)
    {
        // Nothing is cleared on close: a pinned-open preview releases its root references only on Unloaded.
    }

    private void RegisterSemanticPreviewRoot()
    {
        if (_semanticPreview is not { } preview ||
            _semanticPreviewPopup?.Child is not { } child)
        {
            return;
        }

        if (!preview.AdditionalRoots.Contains(child))
        {
            preview.AdditionalRoots.Add(child);
        }
    }
}
