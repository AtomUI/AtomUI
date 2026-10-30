using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class TabOverflowMenuItem : ContentControl, IMotionAwareControl
{
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TabOverflowMenuItem>();

    internal static readonly DirectProperty<TabOverflowMenuItem, TabOverflowItem?> OverflowItemProperty =
        AvaloniaProperty.RegisterDirect<TabOverflowMenuItem, TabOverflowItem?>(
            nameof(OverflowItem),
            control => control.OverflowItem);

    internal static readonly DirectProperty<TabOverflowMenuItem, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<TabOverflowMenuItem, object?>(
            nameof(Header),
            control => control.Header);

    internal static readonly DirectProperty<TabOverflowMenuItem, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<TabOverflowMenuItem, IDataTemplate?>(
            nameof(HeaderTemplate),
            control => control.HeaderTemplate);

    internal static readonly DirectProperty<TabOverflowMenuItem, bool> IsClosableProperty =
        AvaloniaProperty.RegisterDirect<TabOverflowMenuItem, bool>(
            nameof(IsClosable),
            control => control.IsClosable);

    internal static readonly DirectProperty<TabOverflowMenuItem, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<TabOverflowMenuItem, bool>(
            nameof(IsSelected),
            control => control.IsSelected);

    private TabOverflowItem? _overflowItem;
    private object? _header;
    private IDataTemplate? _headerTemplate;
    private bool _isClosable;
    private bool _isSelected;
    private ContentPresenter? _headerPresenter;

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    static TabOverflowMenuItem()
    {
        FocusableProperty.OverrideDefaultValue<TabOverflowMenuItem>(true);
        PointerReleasedEvent.AddClassHandler<TabOverflowMenuItem>(
            (item, args) => item.HandlePointerReleased(args));
        KeyDownEvent.AddClassHandler<TabOverflowMenuItem>(
            (item, args) => item.HandleKeyDown(args));
        IconButton.ClickEvent.AddClassHandler<TabOverflowMenuItem>(
            (item, args) => item.HandleCloseButtonClicked(args));
    }

    internal TabOverflowItem? OverflowItem
    {
        get => _overflowItem;
        private set => SetAndRaise(OverflowItemProperty, ref _overflowItem, value);
    }

    internal object? Header
    {
        get => _header;
        private set => SetAndRaise(HeaderProperty, ref _header, value);
    }

    internal IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        private set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }

    internal bool IsClosable
    {
        get => _isClosable;
        private set => SetAndRaise(IsClosableProperty, ref _isClosable, value);
    }

    internal bool IsSelected
    {
        get => _isSelected;
        private set
        {
            if (SetAndRaise(IsSelectedProperty, ref _isSelected, value))
            {
                PseudoClasses.Set(StdPseudoClass.Selected, value);
            }
        }
    }

    internal void SetOverflowItem(TabOverflowItem item)
    {
        OverflowItem = item;
        Header = item.Header;
        HeaderTemplate = item.HeaderTemplate;
        IsClosable = item.IsClosable;
        IsSelected = item.IsSelected;
        SetCurrentValue(IsEnabledProperty, item.IsEnabled);
    }

    internal void ClearOverflowItem()
    {
        OverflowItem = null;
        Header = null;
        HeaderTemplate = null;
        IsClosable = false;
        IsSelected = false;
        SetCurrentValue(IsEnabledProperty, true);
        DataContext = null;
        // Clearing detached content does not update the presenter's DataContext.
        // Release the old header and its template root before caching this item.
        _headerPresenter?.UpdateChild();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerPresenter = e.NameScope.Find<ContentPresenter>("ItemTextPresenter");
    }

    private void HandlePointerReleased(PointerReleasedEventArgs args)
    {
        if (args.Handled ||
            args.InitialPressMouseButton != MouseButton.Left ||
            IsCloseButtonSource(args.Source as Visual))
        {
            return;
        }

        TryActivate();
        args.Handled = true;
    }

    private void HandleKeyDown(KeyEventArgs args)
    {
        if (!args.Handled && args.Key is Key.Enter or Key.Space)
        {
            TryActivate();
            args.Handled = true;
        }
    }

    private void HandleCloseButtonClicked(RoutedEventArgs args)
    {
        if (args.Source is not IconButton { Name: "PART_ItemCloseButton" } ||
            OverflowItem is not { IsClosable: true } item ||
            FindMenu()?.Context is not { } context)
        {
            return;
        }

        if (context.TryClose(item))
        {
            context.Dismiss();
        }
        args.Handled = true;
    }

    private void TryActivate()
    {
        if (OverflowItem is not { IsEnabled: true } item || FindMenu()?.Context is not { } context)
        {
            return;
        }

        if (context.TryActivate(item))
        {
            context.Dismiss();
        }
    }

    private TabOverflowMenu? FindMenu()
    {
        return this.FindLogicalAncestorOfType<TabOverflowMenu>() ?? Parent as TabOverflowMenu;
    }

    private static bool IsCloseButtonSource(Visual? source)
    {
        return source is IconButton { Name: "PART_ItemCloseButton" } ||
               source?.FindAncestorOfType<IconButton>(includeSelf: true) is { Name: "PART_ItemCloseButton" };
    }
}
