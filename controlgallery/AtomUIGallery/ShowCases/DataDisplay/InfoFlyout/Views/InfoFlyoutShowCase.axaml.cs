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

    // InfoFlyout 的弹层是代码创建、跨视觉根的 Popup，SemanticPartHighlightSession 只自动
    // 发现模板内 Popup，因此在这里把弹层根（FlyoutPresenter）在打开时注册进
    // SemanticPartPreview.AdditionalRoots，让 popup.container/popup.content/popup.arrow
    // 能被语义高亮会话解析到。
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
        // 关闭时不立即清空：钉住打开的预览只在 Unloaded 时释放根引用。
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
