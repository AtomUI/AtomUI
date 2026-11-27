using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Frame treatment of a <see cref="Card" />, selected with <see cref="Card.StyleVariant" />.
/// </summary>
public enum CardStyleVariant
{
    /// <summary>
    /// The default: the card has a border and the shadow of an outlined card.
    /// </summary>
    Outlined,
    /// <summary>
    /// The card has no border and rests on the tertiary shadow instead. A border thickness set directly on the
    /// card still draws.
    /// </summary>
    Borderless
}

internal enum CardContentType
{
    Default,
    Meta,
    Grid,
    Tabs
}

/// <summary>
/// Desktop card with a header area, an optional cover, a body and an actions row, styled after Ant Design.
/// The body shows plain content or one of the dedicated content kinds (<c>CardMetaContent</c>,
/// <c>CardTabsContent</c>, <see cref="CardGridContent" />). The card tells them apart by the type of the value
/// assigned to <see cref="ContentControl.Content" />; grid content makes the frame keep square bottom corners.
/// </summary>
public partial class Card : AbstractCard,
                            ISizeTypeAware,
                            IMotionAwareControl,
                            IBorderBeamAwareControl
{
    #region IBorderBeamAwareControl 实现

    /// <summary>
    /// The beam geometry of the card's own frame. A hosting <see cref="BorderBeam" /> draws its light on this ring,
    /// so the beam follows the card's border thickness and corner radius instead of the beam's theme radius, which
    /// can diverge from the card under large-radius themes (e.g. shadcn Luma BorderRadiusLG 26 vs a radius-8 card).
    /// </summary>
    event EventHandler? IBorderBeamAwareControl.BorderBeamGeometryChanged
    {
        add => _borderBeamGeometryChanged += value;
        remove => _borderBeamGeometryChanged -= value;
    }

    private event EventHandler? _borderBeamGeometryChanged;

    /// <summary>
    /// Returns the ring a hosting <see cref="BorderBeam" /> draws on, built from the card's current
    /// <see cref="TemplatedControl.BorderThickness" /> and <see cref="TemplatedControl.CornerRadius" />.
    /// </summary>
    public BorderBeamGeometry GetBorderBeamGeometry()
    {
        return new BorderBeamGeometry(BorderThickness, CornerRadius);
    }

    private void NotifyBorderBeamGeometryChanged()
    {
        _borderBeamGeometryChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = 
        Border.BoxShadowProperty.AddOwner<Card>();
    
    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<CardStyleVariant> StyleVariantProperty = 
        AvaloniaProperty.Register<Card, CardStyleVariant>(nameof (StyleVariant));
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Card>();
    
    /// <summary>
    /// Identifies the <see cref="IsInnerMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInnerModeProperty = 
        AvaloniaProperty.Register<Card, bool>(nameof (IsInnerMode));
    
    /// <summary>
    /// Identifies the <see cref="IsHoverable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHoverableProperty = 
        AvaloniaProperty.Register<Card, bool>(nameof (IsHoverable));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Card>();
    
    /// <summary>
    /// Shadow painted behind the card frame. The theme sets the shadow of an outlined card, the tertiary shadow for
    /// <see cref="CardStyleVariant.Borderless" /> and the hover shadow while the pointer is over a hoverable card;
    /// <see cref="IsMotionEnabled" /> animates the change between them. A value set here replaces these theme
    /// shadows in every state; clearing it returns to them.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    /// <summary>
    /// Frame treatment of the card; <see cref="CardStyleVariant.Outlined" /> by default. Borderless removes the
    /// frame border and the header divider line and rests the card on the tertiary shadow. A border thickness set
    /// directly on the card still draws.
    /// </summary>
    public CardStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    /// <summary>
    /// Size step of the card. It sets the height, padding and font size of the header area and the padding of the
    /// body for plain and <c>CardMetaContent</c> content; <c>CardTabsContent</c> and <see cref="CardGridContent" />
    /// content receive the value and size themselves. Unset, the value is <c>Middle</c>.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Whether the card is an inner card. While <see langword="true" /> the header area gets a gray fill
    /// (ColorFillAlter) instead of the container background; the frame, cover and body do not change.
    /// </summary>
    public bool IsInnerMode
    {
        get => GetValue(IsInnerModeProperty);
        set => SetValue(IsInnerModeProperty, value);
    }
    
    /// <summary>
    /// Whether the card reacts to the pointer: the shadow changes to the hover shadow, the border takes the card's
    /// background color and the cursor becomes a hand. The change animates only while <see cref="IsMotionEnabled" />
    /// is on. Off by default.
    /// </summary>
    public bool IsHoverable
    {
        get => GetValue(IsHoverableProperty);
        set => SetValue(IsHoverableProperty, value);
    }
    
    /// <summary>
    /// Whether changes of the card animate: the shadow changes of the frame fade. A <c>CardTabsContent</c> or
    /// <see cref="CardGridContent" /> used as content, and the actions row, take the value from the card.
    /// The theme sets it from the EnableMotion token; clearing the value returns to that token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion
    
    #region 内部属性定义
    
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
    
    private CardContentType _contentType;

    internal CardContentType ContentType
    {
        get => _contentType;
        set => SetAndRaise(ContentTypeProperty, ref _contentType, value);
    }
    #endregion

    private CardActionPanel? _cardActionPanel;
    private CompositeDisposable? _contentStateBindings;
    
    static Card()
    {
        AffectsRender<Card>(IsHoverableProperty, IsInnerModeProperty, BoxShadowProperty);
        AffectsMeasure<Card>(SizeTypeProperty, StyleVariantProperty);
    }
    
    /// <summary>
    /// Updates the card after a property value changed. A change of <see cref="TemplatedControl.BorderThickness" />
    /// re-derives the header border thickness and the effective frame thickness; a change of
    /// <see cref="TemplatedControl.CornerRadius" /> re-derives the effective corner radius; both notify a hosting
    /// border beam. A change of <see cref="ContentControl.Content" /> re-derives the content kind and, with it, the
    /// effective corner radius. Calls the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty)
        {
            ConfigureHeaderBorderThickness();
            ConfigureContentBorderThickness();
            NotifyBorderBeamGeometryChanged();
        }
        else if (change.Property == CornerRadiusProperty)
        {
            ConfigureContentCornerRadius();
            NotifyBorderBeamGeometryChanged();
        }
        else if (change.Property == ContentProperty)
        {
            ConfigureContentType();
            ConfigureContentCornerRadius();
        }
    }

    private void ConfigureContentType()
    {
        DisposeContentStateBindings();
        // The content kind is read from the type of the Content value itself. Content that only becomes a
        // CardMetaContent, CardTabsContent or CardGridContent through a data template is treated as plain content.
        if (Content is CardMetaContent)
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Meta);
        }
        else if (Content is CardTabsContent cardTabsContent)
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Tabs);
            BindContentState(cardTabsContent);
        }
        else if (Content is CardGridContent cardGridContent)
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Grid);
            BindContentState(cardGridContent);
        }
        else
        {
            SetCurrentValue(ContentTypeProperty, CardContentType.Default);
        }
    }

    private void BindContentState(CardTabsContent cardTabsContent)
    {
        _contentStateBindings = new CompositeDisposable(2)
        {
            BindUtils.RelayBind(this, IsMotionEnabledProperty, cardTabsContent, CardTabsContent.IsMotionEnabledProperty),
            BindUtils.RelayBind(this, SizeTypeProperty, cardTabsContent, CardTabsContent.SizeTypeProperty)
        };
    }

    private void BindContentState(CardGridContent cardGridContent)
    {
        _contentStateBindings = new CompositeDisposable(2)
        {
            BindUtils.RelayBind(this, IsMotionEnabledProperty, cardGridContent, CardGridContent.IsMotionEnabledProperty),
            BindUtils.RelayBind(this, SizeTypeProperty, cardGridContent, CardGridContent.SizeTypeProperty)
        };
    }

    private void DisposeContentStateBindings()
    {
        _contentStateBindings?.Dispose();
        _contentStateBindings = null;
    }
    
    /// <summary>
    /// Calls the base member, then re-applies the current <see cref="TemplatedControl.BorderThickness" /> to the
    /// effective border thickness of the frame.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureContentBorderThickness();
    }
    
    private void ConfigureContentBorderThickness()
    {
        SetCurrentValue(EffectiveBorderThicknessProperty, BorderThickness);
    }

    private void ConfigureHeaderBorderThickness()
    {
        SetCurrentValue(HeaderBorderThicknessProperty, new Thickness(0, 0, 0, BorderThickness.Bottom));
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

    /// <summary>
    /// Binds the applied template: empties the action row of the previous template, stores the panel named
    /// <c>PART_ActionPanel</c> and fills it with the items of <see cref="AbstractCard.Actions" />, then re-derives
    /// the content kind, the effective corner radius and the header border thickness. Overrides must call the base
    /// member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_cardActionPanel != null)
        {
            _cardActionPanel.Actions.Clear();
        }
        _cardActionPanel = e.NameScope.Find<CardActionPanel>("PART_ActionPanel");
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

    /// <summary>
    /// Applies a change of <see cref="AbstractCard.Actions" /> to the action row once the template has been applied:
    /// items are inserted, removed, replaced or moved one by one. Calls the base member first. A
    /// <see cref="NotifyCollectionChangedAction.Reset" /> throws <see cref="NotSupportedException" />, so remove the
    /// items instead of clearing the collection.
    /// </summary>
    protected override void OnActionsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnActionsChanged(e);
        if (_cardActionPanel != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ControlCollectionChangedUtils.InsertControls(_cardActionPanel.Actions, e.NewStartingIndex, e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _cardActionPanel.Actions.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
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
    }

    /// <summary>
    /// Creates the loading indicator: a <see cref="Skeleton" /> with four paragraph rows and no title that fills
    /// the body and is not clipped by it.
    /// </summary>
    protected override Control CreateLoadingIndicator()
    {
        return new Skeleton
        {
            IsLoading           = true,
            IsActive            = true,
            ParagraphRows       = 4,
            IsShowTitle         = false,
            ClipToBounds        = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch
        };
    }
}
