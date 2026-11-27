using Avalonia.Interactivity;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;
using PopupConfirmControl = AtomUI.Desktop.Controls.PopupConfirm;

namespace AtomUIGallery.ShowCases.PopupConfirm;

public partial class PopupConfirmShowCase : GalleryReactiveUserControl<PopupConfirmViewModel>
{
    public const string LanguageId = nameof(PopupConfirmShowCase);

    private SemanticPartPreview? _semanticPreview;
    private AvaloniaPopup? _semanticPreviewPopup;

    public PopupConfirmShowCase()
    {
        InitializeComponent();
    }

    // The PopupConfirm popup is created in code by PopupConfirmFlyout and lives outside the owner's visual
    // root. SemanticPartHighlightSession only discovers popups inside the template, so the popup root
    // (FlyoutPresenter) is registered in SemanticPartPreview.AdditionalRoots when it opens. That lets the
    // highlight session resolve the popup.* parts.
    private void HandleSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        if (sender is not SemanticPartPreview preview ||
            preview.PreviewContent is not PopupConfirmControl host ||
            host.Flyout?.Popup is not { } popup)
        {
            return;
        }

        _semanticPreview      = preview;
        _semanticPreviewPopup = popup;
        _semanticPreviewPopup.Opened += HandleSemanticPreviewPopupOpened;
        RegisterSemanticPreviewRoot();
    }

    private void HandleSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        if (_semanticPreviewPopup is { } popup)
        {
            popup.Opened -= HandleSemanticPreviewPopupOpened;
        }

        _semanticPreview?.AdditionalRoots.Clear();
        _semanticPreviewPopup = null;
        _semanticPreview      = null;
    }

    private void HandleSemanticPreviewPopupOpened(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewRoot();
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
