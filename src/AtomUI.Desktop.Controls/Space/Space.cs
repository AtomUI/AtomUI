using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Arranges its children with the layout of <see cref="AbstractSpace" />, takes the gaps between items and
/// lines from the size preset, optionally places a separator between adjacent items and paints a background
/// and border behind them. It has no template: <see cref="Render" /> paints the background and border.
/// </summary>
public partial class Space : AbstractSpace,
                             ICustomizableSizeTypeAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Space>();

    /// <summary>
    /// Identifies the <see cref="SplitTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<ITemplate<Control>?> SplitTemplateProperty =
        AvaloniaProperty.Register<Space, ITemplate<Control>?>(
            nameof(SplitTemplate));

    /// <summary>
    /// Identifies the <see cref="Background" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<Space, IBrush?>(nameof(Background));

    /// <summary>
    /// Identifies the <see cref="BorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<Space, IBrush?>(nameof(BorderBrush));

    /// <summary>
    /// Identifies the <see cref="BorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Space, Thickness>(nameof(BorderThickness));

    /// <summary>
    /// Identifies the <see cref="CornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Space, CornerRadius>(nameof(CornerRadius));

    /// <summary>
    /// Identifies the <see cref="Padding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<Space, Thickness>(nameof(Padding));

    /// <summary>
    /// Identifies the <see cref="BorderDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<Space, IReadOnlyList<double>?>(nameof(BorderDashArray));

    /// <summary>
    /// Identifies the <see cref="BorderDashOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<Space, double>(nameof(BorderDashOffset));

    /// <summary>
    /// Size preset that selects the gap between items and between lines. <c>Small</c>, <c>Middle</c> and
    /// <c>Large</c> bind <see cref="AbstractSpace.ItemSpacing" /> and <see cref="AbstractSpace.LineSpacing" />
    /// to the matching gap token; <c>Custom</c> applies no preset but keeps them bound to the small gap token,
    /// so set both properties directly to get other gaps. The bindings have template priority: a value set
    /// directly on the control wins, a style setter does not. The default is <c>Small</c>.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Template for the separators between adjacent children, or <see langword="null" /> to omit them.
    /// One control is built from it for each gap between two adjacent children and <c>SpaceSeparatorStyle</c>
    /// applies to every separator. The default is <see langword="null" />.
    /// </summary>
    public ITemplate<Control>? SplitTemplate
    {
        get => GetValue(SplitTemplateProperty);
        set => SetValue(SplitTemplateProperty, value);
    }

    /// <summary>
    /// Fill painted inside the border behind the items, or <see langword="null" /> to paint no fill.
    /// The default is <see langword="null" />.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Stroke of the border drawn along the edge of the control, or <see langword="null" /> to draw no border.
    /// The default is <see langword="null" />.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Width of the border on each edge, in device-independent pixels. The items are laid out inside the border:
    /// the measured size grows by it on every edge. The default is 0.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Radius of each corner of the border and of the background inside it, in device-independent pixels.
    /// The default is 0.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Insets the layout area of the items from the inner edge of the border; the measured size grows by it on
    /// every edge. Use <see cref="AbstractSpace.ItemSpacing" /> and <see cref="AbstractSpace.LineSpacing" />
    /// for the gaps between items and between lines. The default is 0.
    /// </summary>
    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Dash pattern of the border stroke, as alternating dash and gap lengths. <see langword="null" /> or an
    /// empty list draws a solid border. While a pattern is set, the border is drawn as one stroke whose width is
    /// the top value of <see cref="BorderThickness" />. The default is <see langword="null" />.
    /// </summary>
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    /// <summary>
    /// Offset into the dash pattern of <see cref="BorderDashArray" /> at which the border stroke starts.
    /// It has no effect while the border is solid. The default is 0.
    /// </summary>
    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    #endregion

    private CompositeDisposable? _spacingBindings;
    private OwnerTokenPropertyBindings? _spacingTokens;
    private readonly HashSet<Control> _semanticItemMarkerOwners = new();
    private BorderRenderHelper? _borderRenderHelper;

    static Space()
    {
        AffectsMeasure<Space>(SizeTypeProperty,
            BorderThicknessProperty,
            PaddingProperty);
        AffectsRender<Space>(BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            BorderDashArrayProperty,
            BorderDashOffsetProperty);
        CustomizableSizeTypeControlProperty.SizeTypeProperty.OverrideDefaultValue<Space>(CustomizableSizeType.Small);
    }

    /// <summary>
    /// Called when the control is attached to a visual tree. Calls the base implementation first, then binds
    /// <see cref="AbstractSpace.ItemSpacing" /> and <see cref="AbstractSpace.LineSpacing" /> to the token of the
    /// current <see cref="SizeType" /> and, when <see cref="SplitTemplate" /> is set, rebuilds the logical and
    /// visual children with separators. An override must call the base implementation to keep both steps.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateSpacingTokenBindings();

        if (SplitTemplate != null)
        {
            HandleSplitTemplateChanged();
        }
    }

    /// <summary>
    /// Called when the control is detached from a visual tree. Calls the base implementation first, then disposes
    /// the gap token bindings and their token resources; the next attach creates them again. An override must
    /// call the base implementation to release them.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _spacingBindings?.Dispose();
        _spacingBindings = null;
        _spacingTokens?.Dispose();
        _spacingTokens = null;
    }

    /// <summary>
    /// Calls the base implementation first, then rebinds the gaps to the token of the new <see cref="SizeType" />
    /// when that property changes, and rebuilds the logical and visual children when
    /// <see cref="SplitTemplate" /> changes while the control is attached to a visual tree.
    /// An override must call the base implementation to keep both reactions.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeTypeProperty)
        {
            UpdateSpacingTokenBindings();
        }
        else if (this.IsAttachedToVisualTree())
        {
            if (change.Property == SplitTemplateProperty)
            {
                HandleSplitTemplateChanged();
            }
        }
    }


    /// <summary>
    /// Keeps the item marker class <see cref="SpaceSemanticParts.ItemClass" /> in step with the change of
    /// <see cref="AbstractSpace.Children" />. While <see cref="SplitTemplate" /> is <see langword="null" /> it
    /// then calls the base implementation. Otherwise it rebuilds the logical and visual children with separators
    /// instead of calling the base implementation, which also lets a
    /// <see cref="NotifyCollectionChangedAction.Reset" /> change rebuild the children instead of throwing.
    /// </summary>
    /// <param name="sender">
    /// The <see cref="AbstractSpace.Children" /> list that changed.
    /// </param>
    /// <param name="e">
    /// The collection change.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Thrown by the base implementation when <paramref name="e" /> describes a
    /// <see cref="NotifyCollectionChangedAction.Reset" /> change and <see cref="SplitTemplate" /> is
    /// <see langword="null" />.
    /// </exception>
    protected override void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureSemanticItemMarkers(e);

        if (SplitTemplate == null)
        {
            base.HandleChildrenChanged(sender, e);
        }
        else
        {
            HandleSplitTemplateChanged();
        }
    }

    /// <summary>
    /// Returns the layout inset of the control, which is its padding plus its border thickness. The base class
    /// deducts it from the available size before it breaks the items into lines and adds it to the measured size,
    /// so the items are laid out inside the padding and the border.
    /// </summary>
    /// <returns>
    /// The sum of <c>Padding</c> and <c>BorderThickness</c>.
    /// </returns>
    private protected override Thickness GetLayoutInset()
    {
        return Padding + BorderThickness;
    }


    /// <summary>
    /// Paints the background and the border of the control. No template part draws them: they come only from the
    /// values set on the control.
    /// </summary>
    public sealed override void Render(DrawingContext context)
    {
        var borderRenderHelper = _borderRenderHelper ??= new BorderRenderHelper();
        borderRenderHelper.Render(
            context,
            Bounds.Size,
            BorderThickness,
            CornerRadius,
            BackgroundSizing.InnerBorderEdge,
            Background,
            BorderBrush,
            BorderDashArray,
            BorderDashOffset);
    }


    private void UpdateSpacingTokenBindings()
    {
        _spacingBindings?.Dispose();
        _spacingBindings = null;
        if (!this.IsAttachedToVisualTree())
        {
            return;
        }

        var tokenKind = SizeType switch
        {
            CustomizableSizeType.Small  => SpaceTokenKind.GapSmallSize,
            CustomizableSizeType.Middle => SpaceTokenKind.GapMiddleSize,
            CustomizableSizeType.Large  => SpaceTokenKind.GapLargeSize,
            _                           => SpaceTokenKind.GapSmallSize
        };
        // The active key follows SizeType while both public spacing properties keep their own Template binding.
        var tokens = _spacingTokens ??= new OwnerTokenPropertyBindings(this, 1, BindingPriority.Template);
        var bindings = new CompositeDisposable(2);
        try
        {
            var spacing = tokens.Observe(0, ControlTokenResourceKey.Own(GetType(), tokenKind));
            bindings.Add(Bind(ItemSpacingProperty, spacing, BindingPriority.Template));
            bindings.Add(Bind(LineSpacingProperty, spacing, BindingPriority.Template));
            _spacingBindings = bindings;
        }
        catch
        {
            bindings.Dispose();
            tokens.Dispose();
            _spacingTokens = null;
            throw;
        }
    }

    private void HandleSplitTemplateChanged()
    {
        LogicalChildren.Clear();
        VisualChildren.Clear();
        for (var i = 0; i < Children.Count; ++i)
        {
            var child = Children[i];
            EnsureSemanticItemMarker(child);
            if (!IsItemsHost)
            {
                LogicalChildren.Add(child);
            }
            VisualChildren.Add(child);
            if (SplitTemplate != null)
            {
                if (i != Children.Count - 1)
                {
                    var split = CreateSemanticSeparator();
                    if (!IsItemsHost)
                    {
                        LogicalChildren.Add(split);
                    }
                    VisualChildren.Add(split);
                }
            }
        }
    }

    private void ConfigureSemanticItemMarkers(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Control child in e.NewItems!)
                {
                    EnsureSemanticItemMarker(child);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (Control child in e.OldItems!)
                {
                    ReleaseSemanticItemMarker(child);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (Control child in e.OldItems!)
                {
                    ReleaseSemanticItemMarker(child);
                }

                foreach (Control child in e.NewItems!)
                {
                    EnsureSemanticItemMarker(child);
                }
                break;

            case NotifyCollectionChangedAction.Move:
                break;

            case NotifyCollectionChangedAction.Reset:
                ReleaseStaleSemanticItemMarkers();
                break;
        }
    }

    private void EnsureSemanticItemMarker(Control child)
    {
        if (child.Classes.Contains(SpaceSemanticParts.ItemClass))
        {
            return;
        }

        child.Classes.Add(SpaceSemanticParts.ItemClass);
        _semanticItemMarkerOwners.Add(child);
    }

    private void ReleaseSemanticItemMarker(Control child)
    {
        if (_semanticItemMarkerOwners.Remove(child))
        {
            child.Classes.Remove(SpaceSemanticParts.ItemClass);
        }
    }

    private void ReleaseStaleSemanticItemMarkers()
    {
        foreach (var child in _semanticItemMarkerOwners.ToArray())
        {
            if (!Children.Contains(child))
            {
                ReleaseSemanticItemMarker(child);
            }
        }
    }

    private Control CreateSemanticSeparator()
    {
        Debug.Assert(SplitTemplate != null);
        var separator = SplitTemplate.Build();
        if (!separator.Classes.Contains(SpaceSemanticParts.SeparatorClass))
        {
            separator.Classes.Add(SpaceSemanticParts.SeparatorClass);
        }

        return separator;
    }
}
