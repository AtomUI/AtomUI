using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum GroupBoxTitlePosition
{
    Left,
    Right,
    Center
}

public class GroupBox : ContentControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> HeaderTitleProperty =
        AvaloniaProperty.Register<GroupBox, string?>(nameof(HeaderTitle));

    public static readonly StyledProperty<IBrush?> HeaderTitleColorProperty =
        AvaloniaProperty.Register<GroupBox, IBrush?>(nameof(HeaderTitleColor));
    
    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty =
        AvaloniaProperty.Register<GroupBox, IBrush?>(nameof(HeaderBackground));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderTitleTemplateProperty =
        AvaloniaProperty.Register<GroupBox, IDataTemplate?>(nameof(HeaderTitleTemplate));

    public static readonly StyledProperty<PathIcon?> HeaderIconProperty =
        AvaloniaProperty.Register<GroupBox, PathIcon?>(nameof(HeaderIcon));

    public static readonly StyledProperty<GroupBoxTitlePosition> HeaderTitlePositionProperty =
        AvaloniaProperty.Register<GroupBox, GroupBoxTitlePosition>(nameof(HeaderTitlePosition));

    public static readonly StyledProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.Register<GroupBox, double>(nameof(HeaderFontSize));

    public static readonly StyledProperty<FontStyle> HeaderFontStyleProperty =
        AvaloniaProperty.Register<GroupBox, FontStyle>(nameof(HeaderFontStyle));

    public static readonly StyledProperty<FontWeight> HeaderFontWeightProperty =
        AvaloniaProperty.Register<GroupBox, FontWeight>(nameof(HeaderFontWeight), FontWeight.Normal);

    public string? HeaderTitle
    {
        get => GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    public IBrush? HeaderTitleColor
    {
        get => GetValue(HeaderTitleColorProperty);
        set => SetValue(HeaderTitleColorProperty, value);
    }
    
    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }
    
    public IDataTemplate? HeaderTitleTemplate
    {
        get => GetValue(HeaderTitleTemplateProperty);
        set => SetValue(HeaderTitleTemplateProperty, value);
    }

    public PathIcon? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public GroupBoxTitlePosition HeaderTitlePosition
    {
        get => GetValue(HeaderTitlePositionProperty);
        set => SetValue(HeaderTitlePositionProperty, value);
    }

    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    public FontStyle HeaderFontStyle
    {
        get => GetValue(HeaderFontStyleProperty);
        set => SetValue(HeaderFontStyleProperty, value);
    }

    public FontWeight HeaderFontWeight
    {
        get => GetValue(HeaderFontWeightProperty);
        set => SetValue(HeaderFontWeightProperty, value);
    }

    #endregion

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => GroupBoxToken.ID;
    
    #endregion
    
    private readonly BorderRenderHelper _borderRenderHelper;
    private Border? _headerDecorator;
    private Border? _frame;
    private Rect _borderBounds;
    private Rect _headerBounds;
    
    static GroupBox()
    {
        AffectsMeasure<GroupBox>(HeaderIconProperty, HeaderTitleProperty, HeaderTitleTemplateProperty);
        AffectsRender<GroupBox>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty, CornerRadiusProperty,
            HeaderBackgroundProperty);
    }

    public GroupBox()
    {
        this.RegisterResources();
        _borderRenderHelper = new BorderRenderHelper();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_headerDecorator is not null)
        {
            _headerDecorator.PropertyChanged -= HandleHeaderDecoratorPropertyChanged;
        }
        
        base.OnApplyTemplate(e);
        _headerDecorator = e.NameScope.Find<Border>(GroupBoxThemeConstants.HeaderDecoratorPart);
        _frame           = e.NameScope.Find<Border>(GroupBoxThemeConstants.FramePart);
        if (_headerDecorator is not null)
        {
            _headerDecorator.PropertyChanged += HandleHeaderDecoratorPropertyChanged;
        }
    }

    // protected override Size MeasureOverride(Size availableSize)
    // {
    //     return LayoutHelper.MeasureChild(_frame, availableSize, default, BorderThickness);
    // }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = LayoutHelper.ArrangeChild(_frame, finalSize, default, BorderThickness);
        _borderBounds = new Rect(finalSize);
        _headerBounds = default;
        if (_headerDecorator is not null && _headerDecorator.Bounds.Width > 0 && _headerDecorator.Bounds.Height > 0)
        {
            var headerOffset = _headerDecorator.TranslatePoint(default, this) ?? default;
            _headerBounds = new Rect(headerOffset, _headerDecorator.Bounds.Size);
            var offsetY   = _headerBounds.Y + _headerBounds.Height / 2;
            _borderBounds = new Rect(new Point(0, offsetY), new Size(finalSize.Width, Math.Max(0, finalSize.Height - offsetY)));
        }

        return size;
    }

    public override void Render(DrawingContext context)
    {
        {
            using var state = context.PushTransform(Matrix.CreateTranslation(0, _borderBounds.Y));
            _borderRenderHelper.Render(context,
                _borderBounds.Size,
                BorderThickness,
                CornerRadius,
                BackgroundSizing.InnerBorderEdge,
                Background,
                BorderBrush);
        }

        var headerGapBrush = ResolveHeaderGapBrush();
        if (_headerBounds.Width <= 0 || _headerBounds.Height <= 0 || IsTransparentBrush(headerGapBrush))
        {
            return;
        }

        var horizontalGap = HeaderTitleTemplate is null ? Math.Max(BorderThickness.Left, BorderThickness.Right) + 1 : 0;
        var gapX          = Math.Max(0, _headerBounds.X - horizontalGap);
        var gapRight      = Math.Min(Bounds.Width, _headerBounds.Right + horizontalGap);
        var gapRect       = new Rect(gapX, _headerBounds.Y, Math.Max(0, gapRight - gapX), _headerBounds.Height);
        context.FillRectangle(headerGapBrush!, gapRect);
    }

    private void HandleHeaderDecoratorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Border.BackgroundProperty)
        {
            InvalidateVisual();
        }
    }

    private IBrush? ResolveHeaderGapBrush()
    {
        if (!IsTransparentBrush(_headerDecorator?.Background))
        {
            return _headerDecorator?.Background;
        }

        foreach (var parent in this.GetVisualAncestors())
        {
            var background = GetVisualBackground(parent);
            if (!IsTransparentBrush(background))
            {
                return background;
            }
        }

        return Background;
    }

    private static IBrush? GetVisualBackground(Visual visual)
    {
        return visual switch
        {
            Border border => border.Background,
            Panel panel => panel.Background,
            TemplatedControl templatedControl => templatedControl.Background,
            _ => null
        };
    }

    private static bool IsTransparentBrush(IBrush? brush)
    {
        return brush is null || brush.Opacity <= 0 || brush is ISolidColorBrush solidColorBrush && solidColorBrush.Color.A == 0;
    }
}
