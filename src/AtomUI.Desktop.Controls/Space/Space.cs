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

public partial class Space : AbstractSpace,
                             ICustomizableSizeTypeAware
{
    #region 公共属性定义
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Space>();

    public static readonly StyledProperty<ITemplate<Control>?> SplitTemplateProperty =
        AvaloniaProperty.Register<Space, ITemplate<Control>?>(
            nameof(SplitTemplate));

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<Space, IBrush?>(nameof(Background));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<Space, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Space, Thickness>(nameof(BorderThickness));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Space, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<Space, Thickness>(nameof(Padding));

    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<Space, IReadOnlyList<double>?>(nameof(BorderDashArray));

    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<Space, double>(nameof(BorderDashOffset));

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public ITemplate<Control>? SplitTemplate
    {
        get => GetValue(SplitTemplateProperty);
        set => SetValue(SplitTemplateProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateSpacingTokenBindings();

        if (SplitTemplate != null)
        {
            HandleSplitTemplateChanged();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _spacingBindings?.Dispose();
        _spacingBindings = null;
        _spacingTokens?.Dispose();
        _spacingTokens = null;
    }

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

    private protected override Thickness GetLayoutInset()
    {
        return Padding + BorderThickness;
    }


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
