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
    // 承载弹层部件的语义预览：弹层内容跨视觉根，需要显式注册根。
    private readonly List<SemanticPartPreview> _popupHostPreviews = [];

    public SplitButtonShowCase()
    {
        InitializeComponent();
    }

    // SplitButton 的弹层是 Flyout 代码创建、跨视觉根的 Popup，SemanticPartHighlightSession
    // 只自动发现 owner 模板内 Popup，因此把已打开弹层的根注册进 SemanticPartPreview.AdditionalRoots，
    // 让 popup.root / itemTitle / item / itemContent / itemIcon 能被语义高亮会话解析到。
    //
    // 注册采用同 MenuShowCase（bf00b29b9）的弹性模式：弹层由 SplitButton 的钉住属性异步打开，
    // 打开时机晚于 Loaded，且预览滚动会让 Popup 按放置目标有效性规则做生命周期关闭、滚回后重开。
    // 因此跟随 LayoutUpdated 持续补注册，并订阅 Opened/Closed 在弹层状态变化后重扫；
    // 回调只做幂等注册、不写控件状态，不会与打开/关闭形成循环。
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

    // 预览滚出可视区时 Popup 会按放置目标有效性规则做生命周期关闭，滚回后重新打开；
    // 关闭后延迟一拍重扫，重开的弹层根（含嵌套子菜单 Popup）才能重新接进高亮会话。
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

            // 主弹层 Popup 由 Flyout 代码创建、不在 owner 视觉树内，只能经 Flyout 拿到；
            // 嵌套子菜单 Popup 位于 MenuFlyoutPresenter 的 MenuItem 模板内，从弹层根向下扫描。
            // 子菜单可能在主弹层打开后的下一帧才打开，因此 Track 不要求已打开（订阅 Opened
            // 等待晚开的 Popup），注册只对已打开者幂等追加。
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
