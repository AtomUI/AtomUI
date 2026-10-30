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

    // PopupConfirm 的弹层由 PopupConfirmFlyout 代码创建、跨视觉根，SemanticPartHighlightSession 只自动
    // 发现模板内 Popup，因此在这里把弹层根（FlyoutPresenter）在打开时注册进
    // SemanticPartPreview.AdditionalRoots，让 popup.* 部件能被语义高亮会话解析到。
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
