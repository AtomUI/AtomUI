using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using BoxShadowsTransition = AtomUI.Animations.BoxShadowsTransition;

namespace AtomUI.Desktop.Controls;

public enum CardStyleVariant
{
    Outline,
    Borderless
}

internal enum CardContentType
{
    Default,
    Meta,
    Grid,
    Tabs
}

public class Card : HeaderedContentControl,
                    IControlSharedTokenResourcesHost,
                    ISizeTypeAware,
                    IMotionAwareControl
{
    #region 公共属性定义
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = 
        Border.BoxShadowProperty.AddOwner<Card>();
    
    public static readonly StyledProperty<object?> ExtraProperty = 
        AvaloniaProperty.Register<Card, object?>(nameof (Extra));
    
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty = 
        AvaloniaProperty.Register<Card, IDataTemplate?>(nameof (ExtraTemplate));
    
    public static readonly StyledProperty<CardStyleVariant> StyleVariantProperty = 
        AvaloniaProperty.Register<Card, CardStyleVariant>(nameof (StyleVariant));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Card>();
    
    public static readonly StyledProperty<bool> IsLoadingProperty = 
        AvaloniaProperty.Register<Card, bool>(nameof (IsLoading));
    
    public static readonly StyledProperty<bool> IsInnerModeProperty = 
        AvaloniaProperty.Register<Card, bool>(nameof (IsInnerMode));
    
    public static readonly StyledProperty<bool> IsHoverableProperty = 
        AvaloniaProperty.Register<Card, bool>(nameof (IsHoverable));
    
    public static readonly StyledProperty<object?> CoverProperty = 
        AvaloniaProperty.Register<Card, object?>(nameof (Cover));
    
    public static readonly StyledProperty<IDataTemplate?> CoverTemplateProperty = 
        AvaloniaProperty.Register<Card, IDataTemplate?>(nameof (CoverTemplate));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Card>();
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
    
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }
    
    public CardStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    public bool IsInnerMode
    {
        get => GetValue(IsInnerModeProperty);
        set => SetValue(IsInnerModeProperty, value);
    }
    
    public bool IsHoverable
    {
        get => GetValue(IsHoverableProperty);
        set => SetValue(IsHoverableProperty, value);
    }
    
    public object? Cover
    {
        get => GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }
    
    public IDataTemplate? CoverTemplate
    {
        get => GetValue(CoverTemplateProperty);
        set => SetValue(CoverTemplateProperty, value);
    }

    public Avalonia.Controls.Controls Actions { get; } = new ();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CardToken.ID;
    
    internal static readonly DirectProperty<Card, Thickness> HeaderBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Card, Thickness>(
            nameof(HeaderBorderThickness),
            o => o.HeaderBorderThickness,
            (o, v) => o.HeaderBorderThickness = v);
    
    internal static readonly DirectProperty<Card, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Card, Thickness>(
            nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    internal static readonly DirectProperty<Card, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<Card, CornerRadius>(
            nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);
    
    internal static readonly DirectProperty<Card, bool> ActionsPanelVisibleProperty =
        AvaloniaProperty.RegisterDirect<Card, bool>(
            nameof(ActionsPanelVisible),
            o => o.ActionsPanelVisible,
            (o, v) => o.ActionsPanelVisible = v);
    
    internal static readonly DirectProperty<Card, CardContentType> ContentTypeProperty =
        AvaloniaProperty.RegisterDirect<Card, CardContentType>(
            nameof(ContentType),
            o => o.ContentType,
            (o, v) => o.ContentType = v);
    
    private Thickness _headerBorderThickness;

    internal Thickness HeaderBorderThickness
    {
        get => _headerBorderThickness;
        set => SetAndRaise(HeaderBorderThicknessProperty, ref _headerBorderThickness, value);
    }
    
    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }
    
    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }
    
    private bool _actionsPanelVisible;

    internal bool ActionsPanelVisible
    {
        get => _actionsPanelVisible;
        set => SetAndRaise(ActionsPanelVisibleProperty, ref _actionsPanelVisible, value);
    }
    
    private CardContentType _contentType;

    internal CardContentType ContentType
    {
        get => _contentType;
        set => SetAndRaise(ContentTypeProperty, ref _contentType, value);
    }
    #endregion

    private CardActionPanel? _cardActionPanel;
    private CompositeDisposable? _contentBindingDisposables;
    
    static Card()
    {
        AffectsRender<Card>(IsHoverableProperty, IsInnerModeProperty, BoxShadowProperty);
        AffectsMeasure<Card>(SizeTypeProperty, StyleVariantProperty);
    }
    
    public Card()
    {
        this.RegisterResources();
        Actions.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleActionsChanged);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty)
        {
            ConfigureHeaderBorderThickness();
            ConfigureContentBorderThickness();
        }
        else if (change.Property == CornerRadiusProperty)
        {
            ConfigureContentCornerRadius();
        }
        else if (change.Property == HeaderProperty ||
                 change.Property == HeaderTemplateProperty ||
                 change.Property == ExtraProperty ||
                 change.Property == ExtraTemplateProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == ContentProperty)
        {
            ConfigureContentType();
            ConfigureContentCornerRadius();
            ConfigureHeaderBorderThickness();
        }
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    private void ConfigureContentType()
    {
        _contentBindingDisposables?.Dispose();
        // 暂时只能探测 Content 直接指定的情况
        if (Content is CardMetaContent)
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Meta);
        }
        else if (Content is CardTabsContent cardTabsContent)
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Tabs);
            _contentBindingDisposables = new CompositeDisposable();
            _contentBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, cardTabsContent, CardTabsContent.IsMotionEnabledProperty));
            _contentBindingDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, cardTabsContent, CardTabsContent.SizeTypeProperty));
        }
        else if (Content is CardGridContent cardGridContent)
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Grid);
            _contentBindingDisposables = new CompositeDisposable();
            _contentBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, cardGridContent, CardGridContent.IsMotionEnabledProperty));
            _contentBindingDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, cardGridContent, CardGridContent.SizeTypeProperty));
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureContentBorderThickness();
    }
    
    private void ConfigureContentBorderThickness()
    {
        if (StyleVariant == CardStyleVariant.Outline)
        {
            SetCurrentValue(EffectiveBorderThicknessProperty, BorderThickness);
        }
        else if (StyleVariant == CardStyleVariant.Borderless)
        {
            SetCurrentValue(EffectiveBorderThicknessProperty, new Thickness(0));
        }
    }

    private void ConfigureHeaderBorderThickness()
    {
        if (ContentType == CardContentType.Grid)
        {
            SetCurrentValue(HeaderBorderThicknessProperty, new Thickness(0));
        }
        else
        {
            SetCurrentValue(HeaderBorderThicknessProperty, new Thickness(0, 0, 0, BorderThickness.Bottom));
        }
    }

    private void ConfigureContentCornerRadius()
    {
        if (ContentType == CardContentType.Grid)
        {
            SetCurrentValue(EffectiveCornerRadiusProperty, new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0));
        }
        else
        {
            SetCurrentValue(EffectiveCornerRadiusProperty, CornerRadius);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        _cardActionPanel = e.NameScope.Find<CardActionPanel>(CardThemeConstants.ActionPanelPart);
        if (_cardActionPanel != null)
        {
            foreach (var action in Actions)
            {
                _cardActionPanel.Actions.Add(action);
            }
        }
        ConfigureContentType();
        ConfigureContentCornerRadius();
        ConfigureHeaderBorderThickness();
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = new Transitions
                {
                    TransitionUtils.CreateTransition<BoxShadowsTransition>(BoxShadowProperty, SharedTokenKey.MotionDurationFast)
                };
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(CardPseudoClass.Headerless, Header == null && HeaderTemplate == null && Extra == null && ExtraTemplate == null);
    }
    
    private void HandleActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_cardActionPanel != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _cardActionPanel.Actions.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _cardActionPanel.Actions.RemoveAll(e.OldItems!.OfType<Control>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int index1 = 0; index1 < e.OldItems!.Count; ++index1)
                    {
                        int     index2  = index1 + e.OldStartingIndex;
                        Control newItem = (Control) e.NewItems![index1]!;
                        _cardActionPanel.Actions[index2] = newItem;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    _cardActionPanel.Actions.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
        SetCurrentValue(ActionsPanelVisibleProperty, Actions.Count > 0);
    }
}