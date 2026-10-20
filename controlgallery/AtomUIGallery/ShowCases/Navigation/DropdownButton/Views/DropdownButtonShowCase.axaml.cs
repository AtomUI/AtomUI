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

    // DropdownButton 的弹层是 Flyout 代码创建、跨视觉根的 Popup，SemanticPartHighlightSession
    // 只自动发现 owner 模板内 Popup，因此在这里把弹层根（MenuFlyoutPresenter）在打开时注册进
    // SemanticPartPreview.AdditionalRoots，让 popup.root / itemTitle / item / itemContent /
    // itemIcon 能被语义高亮会话解析到。
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

    // 子菜单弹层与外层弹层不在同一视觉子树，但子菜单 Popup 控件本身还在已注册根的
    // 模板内；从每个已跟踪弹层的根向下发现并跟踪/注册，item 级部件才能覆盖子菜单里的
    // 菜单项。声明式打开的子菜单在下一调度帧才 Open，因此发现时未打开也要先跟踪，
    // 其 Opened 事件触发时再注册根。
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
