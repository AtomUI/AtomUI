using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;
using SplitButtonControl = AtomUI.Desktop.Controls.SplitButton;

namespace AtomUIGallery.ShowCases.SplitButton;

public partial class SplitButtonShowCase : GalleryReactiveUserControl<SplitButtonViewModel>
{
    public const string LanguageId = nameof(SplitButtonShowCase);

    private readonly List<AvaloniaPopup> _trackedSemanticPreviewPopups = [];
    // Semantic previews that host popup parts: the popup content lives outside the visual root,
    // so its root must be registered explicitly.
    private readonly List<SemanticPartPreview> _popupHostPreviews = [];

    public SplitButtonShowCase()
    {
        InitializeComponent();
    }

    // The SplitButton popup is created in code by the flyout and lives outside the owner's visual root.
    // SemanticPartHighlightSession only discovers popups inside the owner template, so the root of each open
    // popup is registered in SemanticPartPreview.AdditionalRoots. That lets the highlight session resolve
    // popup.root, itemTitle, item, itemContent and itemIcon.
    //
    // Registration follows the flexible mode of MenuShowCase: the popup is opened asynchronously by the
    // SplitButton pin property, later than Loaded, and scrolling the preview makes the popup close for
    // placement-target lifetime reasons and reopen when it scrolls back. Registration therefore retraces on
    // LayoutUpdated, and Opened and Closed trigger a rescan after the popup state changes. The callbacks only
    // register idempotently and never write control state, so they cannot loop with opening and closing.
    private void HandleSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        if (sender is not SemanticPartPreview preview)
        {
            return;
        }

        if (!_popupHostPreviews.Contains(preview))
        {
            _popupHostPreviews.Add(preview);
        }

        preview.LayoutUpdated += HandleSemanticPreviewLayoutUpdated;
        RegisterSemanticPreviewPopupRoots();
    }

    private void HandleSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        if (sender is not SemanticPartPreview preview)
        {
            return;
        }

        preview.LayoutUpdated -= HandleSemanticPreviewLayoutUpdated;
        preview.AdditionalRoots.Clear();
        _popupHostPreviews.Remove(preview);

        foreach (var popup in _trackedSemanticPreviewPopups)
        {
            popup.Opened -= HandleSemanticPreviewPopupOpened;
            popup.Closed -= HandleSemanticPreviewPopupClosed;
        }

        _trackedSemanticPreviewPopups.Clear();
    }

    private void HandleSemanticPreviewLayoutUpdated(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewPopupRoots();
    }

    private void HandleSemanticPreviewPopupOpened(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewPopupRoots();
    }

    // When the preview scrolls out of the viewport the popup closes for placement-target lifetime reasons and
    // reopens when it scrolls back. The rescan waits one pass after a close so that the reopened popup root,
    // including nested submenu popups, rejoins the highlight session.
    private void HandleSemanticPreviewPopupClosed(object? sender, EventArgs args)
    {
        Dispatcher.UIThread.Post(RegisterSemanticPreviewPopupRoots, DispatcherPriority.Loaded);
    }

    private void RegisterSemanticPreviewPopupRoots()
    {
        foreach (var preview in _popupHostPreviews)
        {
            if (preview.PreviewContent is not SplitButtonControl owner ||
                owner.Flyout is not { } flyout)
            {
                continue;
            }

            // The main popup is created in code by the flyout and is not in the owner visual tree, so it is reachable
            // only through the flyout. Nested submenu popups live in the MenuItem template of MenuFlyoutPresenter and
            // are found by scanning down from the popup root. A submenu may open a frame after the main popup, so
            // Track does not require it to be open (it subscribes to Opened to wait for a late popup); registration
            // appends idempotently for popups that are already open.
            if (flyout.Popup is { } popup)
            {
                TrackSemanticPreviewPopup(popup);
                if (popup.IsOpen && popup.Child is { } popupRoot &&
                    !preview.AdditionalRoots.Contains(popupRoot))
                {
                    preview.AdditionalRoots.Add(popupRoot);
                }

                var scanRoot = popup.Child;
                if (scanRoot is null)
                {
                    continue;
                }

                foreach (var nested in scanRoot.GetVisualDescendants().OfType<AvaloniaPopup>())
                {
                    TrackSemanticPreviewPopup(nested);
                    if (nested.IsOpen && nested.Child is { } nestedChild &&
                        !preview.AdditionalRoots.Contains(nestedChild))
                    {
                        preview.AdditionalRoots.Add(nestedChild);
                    }
                }
            }
        }
    }

    private void TrackSemanticPreviewPopup(AvaloniaPopup popup)
    {
        if (_trackedSemanticPreviewPopups.Contains(popup))
        {
            return;
        }

        popup.Opened += HandleSemanticPreviewPopupOpened;
        popup.Closed += HandleSemanticPreviewPopupClosed;
        _trackedSemanticPreviewPopups.Add(popup);
    }
}
