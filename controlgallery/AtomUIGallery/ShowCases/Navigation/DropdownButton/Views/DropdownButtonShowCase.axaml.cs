using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;
using DropdownButtonControl = AtomUI.Desktop.Controls.DropdownButton;

namespace AtomUIGallery.ShowCases.DropdownButton;

public partial class DropdownButtonShowCase : GalleryReactiveUserControl<DropdownButtonViewModel>
{
    public const string LanguageId = nameof(DropdownButtonShowCase);

    private readonly List<AvaloniaPopup> _trackedSemanticPreviewPopups = [];
    private SemanticPartPreview? _semanticPreview;
    private AvaloniaPopup? _semanticPreviewPopup;

    public DropdownButtonShowCase()
    {
        InitializeComponent();
    }

    // The DropdownButton popup is created in code by the flyout and lives outside the owner's visual root.
    // SemanticPartHighlightSession only discovers popups inside the owner template, so the popup root
    // (MenuFlyoutPresenter) is registered in SemanticPartPreview.AdditionalRoots when it opens. That lets
    // the highlight session resolve popup.root, itemTitle, item, itemContent and itemIcon.
    private void HandleSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        if (sender is not SemanticPartPreview preview ||
            preview.PreviewContent is not DropdownButtonControl owner ||
            owner.DropdownFlyout is not { } flyout)
        {
            return;
        }

        _semanticPreview      = preview;
        _semanticPreviewPopup = flyout.Popup;
        TrackPopup(_semanticPreviewPopup);
        RegisterSemanticPreviewRoots();
        DiscoverNestedPopups();
    }

    private void HandleSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        foreach (var popup in _trackedSemanticPreviewPopups)
        {
            popup.Opened -= HandleSemanticPreviewPopupOpened;
        }

        _trackedSemanticPreviewPopups.Clear();
        _semanticPreview?.AdditionalRoots.Clear();
        _semanticPreviewPopup = null;
        _semanticPreview      = null;
    }

    private void HandleSemanticPreviewPopupOpened(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewRoots();
        DiscoverNestedPopups();
    }

    // A submenu popup is not in the same visual subtree as the outer popup, but the submenu Popup control is
    // still inside the template of a registered root. Discovery scans down from each tracked popup root and
    // tracks or registers what it finds, so that item-level parts also cover the menu items inside submenus.
    // A declaratively opened submenu opens on the next scheduler frame, so it is tracked even when it is not
    // open yet, and its root is registered when its Opened event fires.
    private void DiscoverNestedPopups()
    {
        foreach (var popup in _trackedSemanticPreviewPopups.ToArray())
        {
            if (popup.Child is not { } root)
            {
                continue;
            }

            foreach (var nested in root.GetVisualDescendants().OfType<AvaloniaPopup>())
            {
                TrackPopup(nested);
                RegisterPopupRoot(nested);
            }
        }
    }

    private void RegisterSemanticPreviewRoots()
    {
        if (_semanticPreviewPopup is { } popup)
        {
            RegisterPopupRoot(popup);
        }
    }

    private void RegisterPopupRoot(AvaloniaPopup popup)
    {
        if (_semanticPreview is not { } preview ||
            !popup.IsOpen ||
            popup.Child is not { } child)
        {
            return;
        }

        TrackPopup(popup);
        if (!preview.AdditionalRoots.Contains(child))
        {
            preview.AdditionalRoots.Add(child);
        }
    }

    private void TrackPopup(AvaloniaPopup popup)
    {
        if (_trackedSemanticPreviewPopups.Contains(popup))
        {
            return;
        }

        popup.Opened += HandleSemanticPreviewPopupOpened;
        _trackedSemanticPreviewPopups.Add(popup);
    }
}
