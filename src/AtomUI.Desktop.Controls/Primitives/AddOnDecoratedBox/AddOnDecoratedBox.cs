using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class AddOnDecoratedBox : InputControlFrame
{
    public const string AddOnDecoratedBoxPart = "PART_AddOnDecoratedBox";
    
    #region 公共属性定义

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, object?>(nameof(LeftAddOn));
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, IDataTemplate?>(nameof(LeftAddOnTemplate));

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, object?>(nameof(RightAddOn));
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, IDataTemplate?>(nameof(RightAddOnTemplate));
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, object?>(nameof(ContentLeftAddOn));
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, IDataTemplate?>(nameof(ContentLeftAddOnTemplate));

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, object?>(nameof(ContentRightAddOn));
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, IDataTemplate?>(nameof(ContentRightAddOnTemplate));

    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    [DependsOn(nameof(ContentLeftAddOnTemplate))]
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(ContentRightAddOnTemplate))]
    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    #endregion
    
    #region 内部属性定义
    internal static readonly StyledProperty<IBrush?> AddOnStatusForegroundProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, IBrush?>(nameof(AddOnStatusForeground));

    internal static readonly StyledProperty<IBrush?> AddOnStatusIconBrushProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, IBrush?>(nameof(AddOnStatusIconBrush));

    internal IBrush? AddOnStatusForeground
    {
        get => GetValue(AddOnStatusForegroundProperty);
        set => SetValue(AddOnStatusForegroundProperty, value);
    }

    internal IBrush? AddOnStatusIconBrush
    {
        get => GetValue(AddOnStatusIconBrushProperty);
        set => SetValue(AddOnStatusIconBrushProperty, value);
    }

    internal static readonly DirectProperty<AddOnDecoratedBox, Thickness> InnerBoxBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, Thickness>(nameof(InnerBoxBorderThickness),
            o => o.InnerBoxBorderThickness,
            (o, v) => o.InnerBoxBorderThickness = v);

    internal static readonly DirectProperty<AddOnDecoratedBox, CornerRadius> InnerBoxCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, CornerRadius>(nameof(InnerBoxCornerRadius),
            o => o.InnerBoxCornerRadius,
            (o, v) => o.InnerBoxCornerRadius = v);

    internal static readonly DirectProperty<AddOnDecoratedBox, CornerRadius> LeftAddOnCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, CornerRadius>(nameof(LeftAddOnCornerRadius),
            o => o.LeftAddOnCornerRadius,
            (o, v) => o.LeftAddOnCornerRadius = v);

    internal static readonly DirectProperty<AddOnDecoratedBox, CornerRadius> RightAddOnCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, CornerRadius>(nameof(RightAddOnCornerRadius),
            o => o.RightAddOnCornerRadius,
            (o, v) => o.RightAddOnCornerRadius = v);

    internal static readonly DirectProperty<AddOnDecoratedBox, Thickness> LeftAddOnBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, Thickness>(nameof(LeftAddOnBorderThickness),
            o => o.LeftAddOnBorderThickness,
            (o, v) => o.LeftAddOnBorderThickness = v);

    internal static readonly DirectProperty<AddOnDecoratedBox, Thickness> RightAddOnBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, Thickness>(nameof(RightAddOnBorderThickness),
            o => o.RightAddOnBorderThickness,
            (o, v) => o.RightAddOnBorderThickness = v);
    
    internal static readonly StyledProperty<double> CustomControlHeightProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, double>(nameof(CustomControlHeight), double.NaN);

    internal static readonly StyledProperty<Thickness> ContentFramePaddingProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, Thickness>(nameof(ContentFramePadding));

    internal static readonly StyledProperty<Thickness> CompactContentFramePaddingProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, Thickness>(nameof(CompactContentFramePadding));

    internal static readonly StyledProperty<double> ContentMinHeightProperty =
        AvaloniaProperty.Register<AddOnDecoratedBox, double>(nameof(ContentMinHeight), double.NaN);

    internal static readonly DirectProperty<AddOnDecoratedBox, Thickness> EffectiveContentFramePaddingProperty =
        AvaloniaProperty.RegisterDirect<AddOnDecoratedBox, Thickness>(
            nameof(EffectiveContentFramePadding),
            o => o.EffectiveContentFramePadding,
            (o, v) => o.EffectiveContentFramePadding = v);
    
    private Thickness _innerBoxBorderThickness;

    internal Thickness InnerBoxBorderThickness
    {
        get => _innerBoxBorderThickness;
        set => SetAndRaise(InnerBoxBorderThicknessProperty, ref _innerBoxBorderThickness, value);
    }

    private CornerRadius _innerBoxCornerRadius;

    internal CornerRadius InnerBoxCornerRadius
    {
        get => _innerBoxCornerRadius;
        set => SetAndRaise(InnerBoxCornerRadiusProperty, ref _innerBoxCornerRadius, value);
    }

    private CornerRadius _leftAddOnCornerRadius;

    internal CornerRadius LeftAddOnCornerRadius
    {
        get => _leftAddOnCornerRadius;
        set => SetAndRaise(LeftAddOnCornerRadiusProperty, ref _leftAddOnCornerRadius, value);
    }

    private CornerRadius _rightAddOnCornerRadius;

    internal CornerRadius RightAddOnCornerRadius
    {
        get => _rightAddOnCornerRadius;
        set => SetAndRaise(RightAddOnCornerRadiusProperty, ref _rightAddOnCornerRadius, value);
    }

    private Thickness _leftAddOnBorderThickness;

    internal Thickness LeftAddOnBorderThickness
    {
        get => _leftAddOnBorderThickness;
        set => SetAndRaise(LeftAddOnBorderThicknessProperty, ref _leftAddOnBorderThickness, value);
    }

    private Thickness _rightAddOnBorderThickness;

    internal Thickness RightAddOnBorderThickness
    {
        get => _rightAddOnBorderThickness;
        set => SetAndRaise(RightAddOnBorderThicknessProperty, ref _rightAddOnBorderThickness, value);
    }
    
    internal double CustomControlHeight
    {
        get => GetValue(CustomControlHeightProperty);
        set => SetValue(CustomControlHeightProperty, value);
    }

    internal Thickness ContentFramePadding
    {
        get => GetValue(ContentFramePaddingProperty);
        set => SetValue(ContentFramePaddingProperty, value);
    }

    internal Thickness CompactContentFramePadding
    {
        get => GetValue(CompactContentFramePaddingProperty);
        set => SetValue(CompactContentFramePaddingProperty, value);
    }

    internal double ContentMinHeight
    {
        get => GetValue(ContentMinHeightProperty);
        set => SetValue(ContentMinHeightProperty, value);
    }

    private Thickness _effectiveContentFramePadding;

    internal Thickness EffectiveContentFramePadding
    {
        get => _effectiveContentFramePadding;
        set => SetAndRaise(EffectiveContentFramePaddingProperty, ref _effectiveContentFramePadding, value);
    }
    
    #endregion
    
    private protected Control? _leftAddOn;
    private protected Control? _rightAddOn;
    private ContentPresenter? _leftAddOnPresenter;
    private ContentPresenter? _rightAddOnPresenter;
    private ContentPresenter? _contentLeftAddOn;
    private ContentPresenter? _contentRightAddOn;
    private CompositeDisposable? _statusColorValues;
    private bool _borderInfoDirty;
    private bool _cornerRadiusDirty;
    private bool _borderThicknessDirty;
    private bool _layoutUpdatePosted;

    internal AddOnDecoratedBoxContentFrame? ContentFrame;
    
    static AddOnDecoratedBox()
    {
        AffectsRender<AddOnDecoratedBox>(BorderBrushProperty, BackgroundProperty);
        AffectsMeasure<AddOnDecoratedBox>(LeftAddOnProperty,
            LeftAddOnTemplateProperty,
            RightAddOnProperty,
            RightAddOnTemplateProperty,
            ContentLeftAddOnProperty,
            ContentLeftAddOnTemplateProperty,
            ContentRightAddOnProperty,
            ContentRightAddOnTemplateProperty,
            EffectiveContentFramePaddingProperty);
    }

    public AddOnDecoratedBox()
    {
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StyleVariantProperty ||
            change.Property == BorderThicknessProperty)
        {
            _borderThicknessDirty = true;
        }

        if (change.Property == SizeTypeProperty ||
            change.Property == CustomControlHeightProperty ||
            change.Property == ContentFramePaddingProperty ||
            change.Property == CompactContentFramePaddingProperty ||
            change.Property == ContentMinHeightProperty ||
            change.Property == InnerBoxBorderThicknessProperty)
        {
            ConfigureEffectiveContentFramePadding();
        }

        if (change.Property == LeftAddOnProperty ||
            change.Property == RightAddOnProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == StyleVariantProperty ||
            change.Property == CompactSpaceItemPositionProperty ||
            change.Property == CompactSpaceOrientationProperty)
        {
            _cornerRadiusDirty = true;
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == BorderThicknessProperty ||
            change.Property == StyleVariantProperty ||
            change.Property == CompactSpaceItemPositionProperty ||
            change.Property == CompactSpaceOrientationProperty)
        {
            _borderInfoDirty = true;
        }

        if (change.Property == StatusProperty ||
            change.Property == EffectiveStatusProperty ||
            change.Property == IsEnabledProperty ||
            change.Property == ContentLeftAddOnProperty ||
            change.Property == ContentRightAddOnProperty ||
            change.Property == AddOnStatusForegroundProperty ||
            change.Property == AddOnStatusIconBrushProperty)
        {
            UpdateIconStatusColors();
        }

        ScheduleLayoutUpdate();
    }

    private void ConfigureEffectiveContentFramePadding()
    {
        var effectivePadding = ContentFramePadding;
        if (CustomizableSizeLayoutHelper.ShouldUseCompactVerticalPadding(
                SizeType,
                CustomControlHeight,
                ContentFramePadding,
                InnerBoxBorderThickness,
                ContentMinHeight))
        {
            effectivePadding = new Thickness(
                ContentFramePadding.Left,
                CompactContentFramePadding.Top,
                ContentFramePadding.Right,
                CompactContentFramePadding.Bottom);
        }

        if (EffectiveContentFramePadding != effectivePadding)
        {
            EffectiveContentFramePadding = effectivePadding;
        }
    }

    private void ScheduleLayoutUpdate()
    {
        if (_layoutUpdatePosted)
        {
            return;
        }
        if (!_borderInfoDirty && !_cornerRadiusDirty && !_borderThicknessDirty)
        {
            return;
        }
        _layoutUpdatePosted = true;
        Dispatcher.Post(ApplyDirtyLayoutUpdates, DispatcherPriority.Render);
    }

    private void ApplyDirtyLayoutUpdates()
    {
        _layoutUpdatePosted = false;
        if (_cornerRadiusDirty)
        {
            ConfigureInnerBoxCornerRadius();
            _cornerRadiusDirty = false;
        }
        if (_borderInfoDirty)
        {
            ConfigureAddOnBorderInfo();
            _borderInfoDirty = false;
        }
        if (_borderThicknessDirty)
        {
            ConfigureInnerBoxBorderThickness();
            _borderThicknessDirty = false;
        }
    }
    
    private void ConfigureAddOnBorderInfo()
    {
        var topLeftRadius     = CornerRadius.TopLeft;
        var topRightRadius    = CornerRadius.TopRight;
        var bottomLeftRadius  = CornerRadius.BottomLeft;
        var bottomRightRadius = CornerRadius.BottomRight;

        CornerRadius newLeftRadius;
        CornerRadius newRightRadius;

        if (IsUsedInCompactSpace &&
            CompactSpaceItemPosition is { } position &&
            CompactSpace.IsPartialPosition(position))
        {
            if (CompactSpace.HasPositionFlag(position, SpaceItemPosition.First))
            {
                if (CompactSpaceOrientation == Orientation.Horizontal)
                {
                    newLeftRadius = new CornerRadius(topLeft:topLeftRadius,
                        topRight:0,
                        bottomLeft: bottomLeftRadius,
                        bottomRight: 0);
                    newRightRadius = new CornerRadius(topLeft:0,
                        topRight: 0,
                        bottomLeft: 0,
                        bottomRight: 0);
                }
                else
                {
                    newLeftRadius = new CornerRadius(topLeft:topLeftRadius,
                        topRight:0,
                        bottomLeft: 0,
                        bottomRight: 0);
                    newRightRadius = new CornerRadius(topLeft:0,
                        topRight:topRightRadius,
                        bottomLeft: 0,
                        bottomRight: 0);
                }
            }
            else if (CompactSpace.HasPositionFlag(position, SpaceItemPosition.Middle))
            {
                newLeftRadius = new CornerRadius(topLeft:0,
                    topRight:0,
                    bottomLeft: 0,
                    bottomRight: 0);
                newRightRadius = new CornerRadius(topLeft:0,
                    topRight:0,
                    bottomLeft: 0,
                    bottomRight: 0);
            }
            else // Last
            {
                if (CompactSpaceOrientation == Orientation.Horizontal)
                {
                    newLeftRadius = new CornerRadius(topLeft:0,
                        topRight:0,
                        bottomLeft: 0,
                        bottomRight: 0);
                    newRightRadius = new CornerRadius(topLeft:0,
                        topRight:topRightRadius,
                        bottomLeft: 0,
                        bottomRight: bottomRightRadius);
                }
                else
                {
                    newLeftRadius = new CornerRadius(topLeft:0,
                        topRight:0,
                        bottomLeft: bottomLeftRadius,
                        bottomRight: 0);
                    newRightRadius = new CornerRadius(topLeft:0,
                        topRight:0,
                        bottomLeft: 0,
                        bottomRight: bottomRightRadius);
                }
            }
        }
        else
        {
            newLeftRadius = new CornerRadius(topLeft:topLeftRadius,
                topRight:0,
                bottomLeft: bottomLeftRadius,
                bottomRight: 0);
            newRightRadius = new CornerRadius(topLeft:0,
                topRight:topRightRadius,
                bottomLeft: 0,
                bottomRight: bottomRightRadius);
        }

        if (LeftAddOnCornerRadius != newLeftRadius)
        {
            LeftAddOnCornerRadius = newLeftRadius;
        }
        if (RightAddOnCornerRadius != newRightRadius)
        {
            RightAddOnCornerRadius = newRightRadius;
        }

        Thickness newLeftThickness;
        Thickness newRightThickness;

        if (StyleVariant == InputControlStyleVariant.Outlined ||
            StyleVariant == InputControlStyleVariant.Filled)
        {
            var topThickness    = BorderThickness.Top;
            var rightThickness  = BorderThickness.Right;
            var bottomThickness = BorderThickness.Bottom;
            var leftThickness   = BorderThickness.Left;

            newLeftThickness =
                new Thickness(top: topThickness, right: 0, bottom: bottomThickness, left: leftThickness);
            newRightThickness =
                new Thickness(top: topThickness, right: rightThickness, bottom: bottomThickness, left: 0);
        }
        else if (StyleVariant == InputControlStyleVariant.Underlined)
        {
            newLeftThickness  = new Thickness(0);
            newRightThickness = new Thickness(0);
        }
        else
        {
            newLeftThickness  = LeftAddOnBorderThickness;
            newRightThickness = RightAddOnBorderThickness;
        }

        if (LeftAddOnBorderThickness != newLeftThickness)
        {
            LeftAddOnBorderThickness = newLeftThickness;
        }
        if (RightAddOnBorderThickness != newRightThickness)
        {
            RightAddOnBorderThickness = newRightThickness;
        }

        NotifyAddOnBorderInfoCalculated();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 取消旧的 ContentPresenter 订阅
        if (_contentLeftAddOn != null)
        {
            _contentLeftAddOn.PropertyChanged -= HandleContentPresenterChildChanged;
        }

        if (_contentRightAddOn != null)
        {
            _contentRightAddOn.PropertyChanged -= HandleContentPresenterChildChanged;
        }

        if (_leftAddOnPresenter != null)
        {
            _leftAddOnPresenter.PropertyChanged -= HandleContentPresenterChildChanged;
        }

        if (_rightAddOnPresenter != null)
        {
            _rightAddOnPresenter.PropertyChanged -= HandleContentPresenterChildChanged;
        }

        _leftAddOn   = e.NameScope.Find<Control>("PART_LeftAddOn");
        _rightAddOn  = e.NameScope.Find<Control>("PART_RightAddOn");
        _leftAddOnPresenter = e.NameScope.Find<ContentPresenter>(
                                  AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart)
                              ?? _leftAddOn as ContentPresenter;
        _rightAddOnPresenter = e.NameScope.Find<ContentPresenter>(
                                   AddOnDecoratedBoxThemeConstants.RightAddOnPresenterPart)
                               ?? _rightAddOn as ContentPresenter;
        _contentLeftAddOn  = e.NameScope.Find<ContentPresenter>("PART_ContentLeftAddOn");
        _contentRightAddOn = e.NameScope.Find<ContentPresenter>("PART_ContentRightAddOn");

        // 订阅新的 ContentPresenter Child 变化
        if (_contentLeftAddOn != null)
        {
            _contentLeftAddOn.PropertyChanged += HandleContentPresenterChildChanged;
        }

        if (_contentRightAddOn != null)
        {
            _contentRightAddOn.PropertyChanged += HandleContentPresenterChildChanged;
        }

        if (_leftAddOnPresenter != null)
        {
            _leftAddOnPresenter.PropertyChanged += HandleContentPresenterChildChanged;
        }

        if (_rightAddOnPresenter != null)
        {
            _rightAddOnPresenter.PropertyChanged += HandleContentPresenterChildChanged;
        }
        
        ContentFrame = e.NameScope.Find<AddOnDecoratedBoxContentFrame>("PART_ContentFrame");

        ConfigureInnerBoxCornerRadius();
        ConfigureAddOnBorderInfo();
        ConfigureInnerBoxBorderThickness();
        UpdateIconStatusColors();
    }

    private void HandleContentPresenterChildChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            if (ReferenceEquals(sender, _leftAddOnPresenter) ||
                ReferenceEquals(sender, _rightAddOnPresenter))
            {
                _cornerRadiusDirty = true;
                ScheduleLayoutUpdate();
            }

            if (e.OldValue is Control oldChild)
            {
                oldChild.AttachedToVisualTree -= HandleAddOnChildAttachedToVisualTree;
            }

            if (e.NewValue is Control newChild)
            {
                if (newChild.IsAttachedToVisualTree())
                {
                    UpdateIconStatusColors();
                }
                else
                {
                    newChild.AttachedToVisualTree += HandleAddOnChildAttachedToVisualTree;
                }
            }
        }
    }

    private void HandleAddOnChildAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control child)
        {
            child.AttachedToVisualTree -= HandleAddOnChildAttachedToVisualTree;
        }
        UpdateIconStatusColors();
    }

    private void UpdateIconStatusColors()
    {
        // Status colours are removable Template-priority values: user local values and part styles on the add-on
        // presenters and icons keep winning and are never erased, and leaving the status disposes exactly the values
        // applied here (no ClearValue). Add-on icons are runtime content (not template parts), possibly logically
        // owned by the outer input control, so a ControlTheme selector cannot reach them.
        _statusColorValues?.Dispose();
        _statusColorValues = null;

        var foreground = AddOnStatusForeground;
        var iconBrush  = AddOnStatusIconBrush;
        if (foreground is null && iconBrush is null)
        {
            return;
        }

        var values = new CompositeDisposable();
        if (foreground is not null)
        {
            ApplyAddOnForeground(values, _contentLeftAddOn, foreground);
            ApplyAddOnForeground(values, _contentRightAddOn, foreground);
            ApplyAddOnForeground(values, _leftAddOnPresenter, foreground);
            ApplyAddOnForeground(values, _rightAddOnPresenter, foreground);
        }

        if (iconBrush is not null)
        {
            ApplyIconBrush(values, _contentLeftAddOn, iconBrush);
            ApplyIconBrush(values, _contentRightAddOn, iconBrush);
            ApplyIconBrush(values, _leftAddOn, iconBrush);
            ApplyIconBrush(values, _rightAddOn, iconBrush);
        }

        _statusColorValues = values;
    }

    private static void ApplyAddOnForeground(CompositeDisposable values, ContentPresenter? presenter, IBrush brush)
    {
        AddValue(values, presenter?.SetValue(ForegroundProperty, brush, BindingPriority.Template));
    }

    private static void ApplyIconBrush(CompositeDisposable values, Control? container, IBrush brush)
    {
        if (container == null)
        {
            return;
        }

        foreach (var descendant in container.GetVisualDescendants())
        {
            if (descendant is not Icon icon || icon.Classes.Contains("skip-status"))
            {
                continue;
            }

            AddValue(values, icon.SetValue(Icon.FillBrushProperty, brush, BindingPriority.Template));
            AddValue(values, icon.SetValue(Icon.StrokeBrushProperty, brush, BindingPriority.Template));
            AddValue(values, icon.SetValue(Icon.ForegroundProperty, brush, BindingPriority.Template));
        }
    }

    private static void AddValue(CompositeDisposable values, IDisposable? applied)
    {
        if (applied is not null)
        {
            values.Add(applied);
        }
    }
    
    protected virtual void NotifyAddOnBorderInfoCalculated()
    {
    }
    
    private void ConfigureInnerBoxCornerRadius()
    {
        if (StyleVariant != InputControlStyleVariant.Underlined)
        {
            var topLeftRadius     = CornerRadius.TopLeft;
            var topRightRadius    = CornerRadius.TopRight;
            var bottomLeftRadius  = CornerRadius.BottomLeft;
            var bottomRightRadius = CornerRadius.BottomRight;

            if (_leftAddOn is not null && _leftAddOn.IsVisible)
            {
                topLeftRadius    = 0;
                bottomLeftRadius = 0;
            }

            if (_rightAddOn is not null && _rightAddOn.IsVisible)
            {
                topRightRadius    = 0;
                bottomRightRadius = 0;
            }

            if (IsUsedInCompactSpace &&
                CompactSpaceItemPosition is { } position &&
                CompactSpace.IsPartialPosition(position))
            {
                if (CompactSpace.HasPositionFlag(position, SpaceItemPosition.First))
                {
                    if (CompactSpaceOrientation == Orientation.Horizontal)
                    {
                        topRightRadius    = 0;
                        bottomRightRadius = 0;
                    }
                    else
                    {
                        bottomLeftRadius  = 0;
                        bottomRightRadius = 0;
                    }
                }
                else if (CompactSpace.HasPositionFlag(position, SpaceItemPosition.Middle))
                {
                     topLeftRadius     = 0;
                     topRightRadius    = 0;
                     bottomLeftRadius  = 0;
                     bottomRightRadius = 0;
                }
                else if (CompactSpace.HasPositionFlag(position, SpaceItemPosition.Last))
                {
                    if (CompactSpaceOrientation == Orientation.Horizontal)
                    {
                        topLeftRadius    = 0;
                        bottomLeftRadius = 0;
                    }
                    else
                    {
                        topLeftRadius = 0;
                        topRightRadius = 0;
                    }
                }
            }
            
            var innerBoxCornerRadius = new CornerRadius(topLeftRadius,
                topRightRadius,
                bottomLeft: bottomLeftRadius,
                bottomRight: bottomRightRadius);
            if (InnerBoxCornerRadius != innerBoxCornerRadius)
            {
                SetCurrentValue(InnerBoxCornerRadiusProperty, innerBoxCornerRadius);
            }
        }
        else
        {
            var innerBoxCornerRadius = new CornerRadius(0);
            if (InnerBoxCornerRadius != innerBoxCornerRadius)
            {
                SetCurrentValue(InnerBoxCornerRadiusProperty, innerBoxCornerRadius);
            }
        }
    }

    private void ConfigureInnerBoxBorderThickness()
    {
        Thickness innerBoxBorderThickness;
        if (StyleVariant == InputControlStyleVariant.Borderless)
        {
            innerBoxBorderThickness = new Thickness(0);
        }
        else if (StyleVariant == InputControlStyleVariant.Underlined)
        {
            innerBoxBorderThickness = new Thickness(0, 0, 0, BorderThickness.Bottom);
        }
        else
        {
            innerBoxBorderThickness = BorderThickness;
        }
        if (InnerBoxBorderThickness != innerBoxBorderThickness)
        {
            SetCurrentValue(InnerBoxBorderThicknessProperty, innerBoxBorderThickness);
        }
    }
    

}

internal class AddOnDecoratedBoxContentFrame : PixelAlignedBorder
{
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (TemplatedParent is InputControlFrame frame)
        {
            SetInnerBoxHoverIfChanged(frame, true);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (TemplatedParent is InputControlFrame frame)
        {
            SetInnerBoxHoverIfChanged(frame, false);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (TemplatedParent is InputControlFrame frame)
        {
            SetInnerBoxHoverIfChanged(frame, true);
            SetInnerBoxPressedIfChanged(frame, true);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (TemplatedParent is InputControlFrame frame)
        {
            SetInnerBoxPressedIfChanged(frame, false);
            SetInnerBoxHoverIfChanged(frame, true);
        }
    }

    private static void SetInnerBoxHoverIfChanged(InputControlFrame frame, bool isHover)
    {
        if (frame.IsInnerBoxHover != isHover)
        {
            frame.IsInnerBoxHover = isHover;
        }
    }

    private static void SetInnerBoxPressedIfChanged(InputControlFrame frame, bool isPressed)
    {
        if (frame.IsInnerBoxPressed != isPressed)
        {
            frame.IsInnerBoxPressed = isPressed;
        }
    }
}

internal class AddOnContentPresenter : ContentPresenter
{
    // Templates wrap an inner presenter as the add-on content; an empty inner presenter hides itself,
    // and the wrapper must follow it so that its margin and the panel spacing are not reserved.
    private Control? _observedContent;

    public AddOnContentPresenter()
    {
        UpdateVisibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            ObserveContentVisibility(Content as Control);
        }

        if (change.Property == ContentProperty ||
            change.Property == ContentTemplateProperty)
        {
            UpdateVisibility();
        }
    }

    private void ObserveContentVisibility(Control? content)
    {
        if (ReferenceEquals(_observedContent, content))
        {
            return;
        }

        if (_observedContent is not null)
        {
            _observedContent.PropertyChanged -= HandleContentPropertyChanged;
        }

        _observedContent = content;
        if (_observedContent is not null)
        {
            _observedContent.PropertyChanged += HandleContentPropertyChanged;
        }
    }

    private void HandleContentPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty)
        {
            UpdateVisibility();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Child is null && (Content is not null || ContentTemplate is not null))
        {
            UpdateChild();
        }
    }

    private void UpdateVisibility()
    {
        var isVisible = Content is Control content
            ? content.IsVisible
            : Content is not null || ContentTemplate is not null;
        if (IsVisible != isVisible)
        {
            SetCurrentValue(IsVisibleProperty, isVisible);
        }
    }
}
