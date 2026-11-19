using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

// A Token-owner transparent part: a CollapseItem is an item of Collapse only and has no Own Tokens, so its theme reads the
// Collapse TokenResource and Controls[Collapse] reaches the item headers, bodies, separators and motion.
[TokenOwnerTransparent]
[PseudoClasses(StdPseudoClass.Pressed, StdPseudoClass.Selected)]
public class CollapseItem : AbstractCollapseItem, ISelectable
{
    #region 公共属性定义

    public new static readonly StyledProperty<bool> IsSelectedProperty =
        AbstractCollapseItem.IsSelectedProperty.AddOwner<CollapseItem>();

    public new static readonly StyledProperty<bool> IsShowExpandIconProperty =
        AbstractCollapseItem.IsShowExpandIconProperty.AddOwner<CollapseItem>();

    public new static readonly StyledProperty<PathIcon?> ExpandIconProperty =
        AbstractCollapseItem.ExpandIconProperty.AddOwner<CollapseItem>();

    public new static readonly StyledProperty<object?> AddOnContentProperty =
        AbstractCollapseItem.AddOnContentProperty.AddOwner<CollapseItem>();

    public new static readonly StyledProperty<IDataTemplate?> AddOnContentTemplateProperty =
        AbstractCollapseItem.AddOnContentTemplateProperty.AddOwner<CollapseItem>();
    
    public static readonly StyledProperty<Thickness> HeaderPaddingProperty =
        AvaloniaProperty.Register<CollapseItem, Thickness>(nameof(HeaderPadding));

    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<CollapseItem, Thickness>(nameof(ContentPadding));

    public new bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public new bool IsShowExpandIcon
    {
        get => GetValue(IsShowExpandIconProperty);
        set => SetValue(IsShowExpandIconProperty, value);
    }

    public new PathIcon? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    public new object? AddOnContent
    {
        get => GetValue(AddOnContentProperty);
        set => SetValue(AddOnContentProperty, value);
    }

    public new IDataTemplate? AddOnContentTemplate
    {
        get => GetValue(AddOnContentTemplateProperty);
        set => SetValue(AddOnContentTemplateProperty, value);
    }
    
    public Thickness HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<CollapseItem>();

    internal static readonly StyledProperty<bool> UseItemPaddingTokenResourcesProperty =
        AvaloniaProperty.Register<CollapseItem, bool>(nameof(UseItemPaddingTokenResources));

    internal static readonly DirectProperty<CollapseItem, Thickness?> OwnerHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, Thickness?>(nameof(OwnerHeaderPadding),
            o => o.OwnerHeaderPadding,
            (o, v) => o.OwnerHeaderPadding = v);

    internal static readonly DirectProperty<CollapseItem, Thickness?> OwnerContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, Thickness?>(nameof(OwnerContentPadding),
            o => o.OwnerContentPadding,
            (o, v) => o.OwnerContentPadding = v);

    internal static readonly DirectProperty<CollapseItem, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, Thickness>(nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding,
            (o, v) => o.EffectiveHeaderPadding = v);

    internal static readonly DirectProperty<CollapseItem, Thickness> EffectiveContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, Thickness>(nameof(EffectiveContentPadding),
            o => o.EffectiveContentPadding,
            (o, v) => o.EffectiveContentPadding = v);

    internal static readonly DirectProperty<CollapseItem, bool> IsGhostStyleProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, bool>(nameof(IsGhostStyle),
            o => o.IsGhostStyle,
            (o, v) => o.IsGhostStyle = v);

    internal static readonly DirectProperty<CollapseItem, bool> IsBorderlessProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, bool>(nameof(IsBorderless),
            o => o.IsBorderless,
            (o, v) => o.IsBorderless = v);

    internal static readonly DirectProperty<CollapseItem, CollapseTriggerType> TriggerTypeProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, CollapseTriggerType>(nameof(TriggerType),
            o => o.TriggerType,
            (o, v) => o.TriggerType = v);

    internal static readonly DirectProperty<CollapseItem, CollapseExpandIconPosition> ExpandIconPositionProperty =
        AvaloniaProperty.RegisterDirect<CollapseItem, CollapseExpandIconPosition>(nameof(ExpandIconPosition),
            o => o.ExpandIconPosition,
            (o, v) => o.ExpandIconPosition = v);
    
    internal static readonly StyledProperty<Thickness> ItemBorderThicknessProperty =
        AvaloniaProperty.Register<CollapseItem, Thickness>(nameof(ItemBorderThickness));

    internal static readonly StyledProperty<Thickness> ContentBorderThicknessProperty =
        AvaloniaProperty.Register<CollapseItem, Thickness>(nameof(ContentBorderThickness));

    internal static readonly StyledProperty<CornerRadius> HeaderCornerRadiusProperty =
        AvaloniaProperty.Register<CollapseItem, CornerRadius>(nameof(HeaderCornerRadius));

    internal static readonly StyledProperty<CornerRadius> ContentCornerRadiusProperty =
        AvaloniaProperty.Register<CollapseItem, CornerRadius>(nameof(ContentCornerRadius));

    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<CollapseItem>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CollapseItem>();
    
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal bool UseItemPaddingTokenResources
    {
        get => GetValue(UseItemPaddingTokenResourcesProperty);
        set => SetValue(UseItemPaddingTokenResourcesProperty, value);
    }

    private static readonly object[] s_paddingKeys =
    [
        CollapseTokenKind.CollapseHeaderPaddingSM,
        CollapseTokenKind.CollapseContentPaddingSM,
        CollapseTokenKind.HeaderPadding,
        CollapseTokenKind.ContentPadding,
        CollapseTokenKind.CollapseHeaderPaddingLG,
        CollapseTokenKind.CollapseContentPaddingLG
    ];

    private OwnerTokenSet? _paddingTokens;
    private (Thickness Header, Thickness Content) _defaultPaddings;

    private Thickness? _ownerHeaderPadding;

    internal Thickness? OwnerHeaderPadding
    {
        get => _ownerHeaderPadding;
        set => SetAndRaise(OwnerHeaderPaddingProperty, ref _ownerHeaderPadding, value);
    }

    private Thickness? _ownerContentPadding;

    internal Thickness? OwnerContentPadding
    {
        get => _ownerContentPadding;
        set => SetAndRaise(OwnerContentPaddingProperty, ref _ownerContentPadding, value);
    }

    private Thickness _effectiveHeaderPadding;

    internal Thickness EffectiveHeaderPadding
    {
        get => _effectiveHeaderPadding;
        set => SetAndRaise(EffectiveHeaderPaddingProperty, ref _effectiveHeaderPadding, value);
    }

    private Thickness _effectiveContentPadding;

    internal Thickness EffectiveContentPadding
    {
        get => _effectiveContentPadding;
        set => SetAndRaise(EffectiveContentPaddingProperty, ref _effectiveContentPadding, value);
    }

    private bool _isGhostStyle;

    internal bool IsGhostStyle
    {
        get => _isGhostStyle;
        set => SetAndRaise(IsGhostStyleProperty, ref _isGhostStyle, value);
    }

    private bool _isBorderless;

    internal bool IsBorderless
    {
        get => _isBorderless;
        set => SetAndRaise(IsBorderlessProperty, ref _isBorderless, value);
    }

    private CollapseTriggerType _triggerType = CollapseTriggerType.Header;

    internal CollapseTriggerType TriggerType
    {
        get => _triggerType;
        set => SetAndRaise(TriggerTypeProperty, ref _triggerType, value);
    }

    private CollapseExpandIconPosition _expandIconPosition = CollapseExpandIconPosition.Start;

    internal CollapseExpandIconPosition ExpandIconPosition
    {
        get => _expandIconPosition;
        set => SetAndRaise(ExpandIconPositionProperty, ref _expandIconPosition, value);
    }
    
    internal Thickness ItemBorderThickness
    {
        get => GetValue(ItemBorderThicknessProperty);
        set => SetValue(ItemBorderThicknessProperty, value);
    }

    internal Thickness ContentBorderThickness
    {
        get => GetValue(ContentBorderThicknessProperty);
        set => SetValue(ContentBorderThicknessProperty, value);
    }

    internal CornerRadius HeaderCornerRadius
    {
        get => GetValue(HeaderCornerRadiusProperty);
        set => SetValue(HeaderCornerRadiusProperty, value);
    }

    internal CornerRadius ContentCornerRadius
    {
        get => GetValue(ContentCornerRadiusProperty);
        set => SetValue(ContentCornerRadiusProperty, value);
    }

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    private Control? _headerDecorator;
    private IconButton? _expandButton;
    // Avalonia focuses the nearest focusable ancestor of a pressed element, so a press on plain content gives
    // the item focus; Ant Design only toggles from the header, so toggle keys stay off until focus comes
    // from the keyboard or a header press.
    private bool _isToggleKeySuppressed;

    static CollapseItem()
    {
        PressedMixin.Attach<CollapseItem>();
        DataContextProperty.Changed.AddClassHandler<CollapseItem>((x, e) => x.UpdateHeader(e));
    }

    public CollapseItem()
    {
        AddHandler(PointerPressedEvent, HandlePreviewPointerPressed, RoutingStrategies.Tunnel, true);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ListItemAutomationPeer(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_expandButton is not null)
        {
            _expandButton.Click -= HandleExpandButtonClick;
        }
        _headerDecorator       = e.NameScope.Find<Control>("PART_HeaderDecorator");
        _expandButton          = e.NameScope.Find<IconButton>("PART_ExpandButton");

        UpdateEffectivePaddings();
        if (_expandButton is not null)
        {
            _expandButton.Click += HandleExpandButtonClick;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupDefaultExpandIcon();
        UpdatePaddingTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _paddingTokens?.Dispose();
        _paddingTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this) && e.NavigationMethod != NavigationMethod.Pointer)
        {
            _isToggleKeySuppressed = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!_isToggleKeySuppressed &&
            TriggerType == CollapseTriggerType.Header &&
            SelectingItemsControl.ItemsControlFromItemContainer(this) is Collapse collapse)
        {
            e.Handled = collapse.UpdateSelectionFromEvent(this, e);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UseItemPaddingTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdatePaddingTokenSubscription();
        }
        else if (change.Property == SizeTypeProperty && _paddingTokens is not null)
        {
            RefreshDefaultPaddings();
        }
        else if (change.Property == HeaderPaddingProperty ||
            change.Property == ContentPaddingProperty ||
            change.Property == OwnerHeaderPaddingProperty ||
            change.Property == OwnerContentPaddingProperty)
        {
            UpdateEffectivePaddings();
        }
    }

    // Runs in the tunnel phase, before the focus manager focuses the item for a content press.
    private void HandlePreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isToggleKeySuppressed = _headerDecorator is null ||
                                     !new Rect(_headerDecorator.Bounds.Size).Contains(e.GetPosition(_headerDecorator));
        }
    }

    internal bool IsPointInHeaderBounds(Point position)
    {
        if (_headerDecorator is not null && TriggerType != CollapseTriggerType.Icon)
        {
            return _headerDecorator.Bounds.Contains(position);
        }

        return false;
    }

    private void UpdateHeader(AvaloniaPropertyChangedEventArgs obj)
    {
        if (Header == null)
        {
            if (obj.NewValue is IHeadered headered)
            {
                if (Header != headered.Header)
                {
                    SetCurrentValue(HeaderProperty, headered.Header);
                }
            }
            else if (obj.NewValue is IReadOnlyHeadered readOnlyHeadered)
            {
                if (Header != readOnlyHeadered.Header)
                {
                    SetCurrentValue(HeaderProperty, readOnlyHeadered.Header);
                }
            }
            else
            {
                if (!(obj.NewValue is Control))
                {
                    SetCurrentValue(HeaderProperty, obj.NewValue);
                }
            }
        }
        else
        {
            if (Header == obj.OldValue)
            {
                SetCurrentValue(HeaderProperty, obj.NewValue);
            }
        }
    }

    private void HandleExpandButtonClick(object? sender, RoutedEventArgs args)
    {
        if (SelectingItemsControl.ItemsControlFromItemContainer(this) is Collapse collapse)
        {
            args.Handled = collapse.UpdateSelectionFromEvent(this, args);
        }
        else
        {
            IsSelected = !IsSelected;
        }
    }

    private void SetupDefaultExpandIcon()
    {
        if (ExpandIcon is null)
        {
            ClearValue(ExpandIconProperty);
            SetValue(ExpandIconProperty, new RightOutlined(), BindingPriority.Template);
        }
        Debug.Assert(ExpandIcon != null);
    }

    private void UpdatePaddingTokenSubscription()
    {
        if (UseItemPaddingTokenResources && _paddingTokens is null)
        {
            _paddingTokens = new OwnerTokenSet(this, s_paddingKeys.Length, RefreshDefaultPaddings);
            for (var index = 0; index < s_paddingKeys.Length; ++index)
            {
                _paddingTokens.SetResourceKey(index, s_paddingKeys[index]);
            }
        }
        else if (!UseItemPaddingTokenResources && _paddingTokens is not null)
        {
            _paddingTokens.Dispose();
            _paddingTokens = null;
        }
        RefreshDefaultPaddings();
    }

    private void RefreshDefaultPaddings()
    {
        var index = SizeType switch
        {
            CustomizableSizeType.Small => 0,
            CustomizableSizeType.Middle or CustomizableSizeType.Custom => 2,
            CustomizableSizeType.Large => 4,
            _ => -1
        };
        _defaultPaddings = index < 0
            ? default
            : (ReadPadding(_paddingTokens?.GetValue(index), _defaultPaddings.Header),
               ReadPadding(_paddingTokens?.GetValue(index + 1), _defaultPaddings.Content));
        UpdateEffectivePaddings();
    }

    private static Thickness ReadPadding(object? value, Thickness previous)
    {
        if (value is Thickness padding)
        {
            return padding;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        if (value is string text)
        {
            try
            {
                return Thickness.Parse(text);
            }
            catch (FormatException)
            {
                return default;
            }
        }
        return default;
    }

    private void UpdateEffectivePaddings()
    {
        EffectiveHeaderPadding = IsSet(HeaderPaddingProperty)
            ? HeaderPadding
            : OwnerHeaderPadding ?? _defaultPaddings.Header;
        EffectiveContentPadding = IsSet(ContentPaddingProperty)
            ? ContentPadding
            : OwnerContentPadding ?? _defaultPaddings.Content;
    }

}
