using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class DescriptionDefaultItem : HeaderedContentControl
{
    #region 公共属性定义

    public static readonly StyledProperty<double> RelativeLineHeightProperty =
        AvaloniaProperty.Register<DescriptionDefaultItem, double>(nameof(RelativeLineHeight));
    
    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<DescriptionDefaultItem, double>(nameof(LineHeight));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DescriptionDefaultItem>();

    public double RelativeLineHeight
    {
        get => GetValue(RelativeLineHeightProperty);
        set => SetValue(RelativeLineHeightProperty, value);
    }
    
    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<DescriptionDefaultItem, bool> IsColonVisibleProperty =
        AvaloniaProperty.RegisterDirect<DescriptionDefaultItem, bool>(nameof(IsColonVisible),
            o => o.IsColonVisible,
            (o, v) => o.IsColonVisible = v);
    
    internal static readonly DirectProperty<DescriptionDefaultItem, bool> IsBorderedProperty =
        AvaloniaProperty.RegisterDirect<DescriptionDefaultItem, bool>(nameof(IsBordered),
            o => o.IsBordered,
            (o, v) => o.IsBordered = v);
    
    internal static readonly DirectProperty<DescriptionDefaultItem, Orientation> LayoutProperty =
        AvaloniaProperty.RegisterDirect<DescriptionDefaultItem, Orientation>(nameof(Layout),
            o => o.Layout,
            (o, v) => o.Layout = v);
    
    internal static readonly DirectProperty<DescriptionDefaultItem, bool> IsLastRowProperty =
        AvaloniaProperty.RegisterDirect<DescriptionDefaultItem, bool>(nameof(IsLastRow),
            o => o.IsLastRow,
            (o, v) => o.IsLastRow = v);
    
    internal static readonly DirectProperty<DescriptionDefaultItem, bool> IsLastColumnProperty =
        AvaloniaProperty.RegisterDirect<DescriptionDefaultItem, bool>(nameof(IsLastColumn),
            o => o.IsLastColumn,
            (o, v) => o.IsLastColumn = v);
    
    internal static readonly DirectProperty<DescriptionDefaultItem, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<DescriptionDefaultItem, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    private bool _isColonVisible;

    internal bool IsColonVisible
    {
        get => _isColonVisible;
        set => SetAndRaise(IsColonVisibleProperty, ref _isColonVisible, value);
    }
    
    private bool _isBordered;

    internal bool IsBordered
    {
        get => _isBordered;
        set => SetAndRaise(IsBorderedProperty, ref _isBordered, value);
    }
    
    private Orientation _layout;

    internal Orientation Layout
    {
        get => _layout;
        set => SetAndRaise(LayoutProperty, ref _layout, value);
    }
    
    private bool _isLastRow;

    internal bool IsLastRow
    {
        get => _isLastRow;
        set => SetAndRaise(IsLastRowProperty, ref _isLastRow, value);
    }
    
    private bool _isLastColumn;

    internal bool IsLastColumn
    {
        get => _isLastColumn;
        set => SetAndRaise(IsLastColumnProperty, ref _isLastColumn, value);
    }
    
    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }
    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find("Label") is ContentPresenter label)
        {
            label.Classes.Add(DescriptionsSemanticParts.LabelClass);
        }
        if (e.NameScope.Find("Content") is ContentPresenter content)
        {
            content.Classes.Add(DescriptionsSemanticParts.ContentClass);
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RelativeLineHeightProperty ||
            change.Property == FontSizeProperty)
        {
            SetCurrentValue(LineHeightProperty, RelativeLineHeight * FontSize);
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == IsLastRowProperty ||
                 change.Property == IsLastColumnProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
    }
    
    private void ConfigureEffectiveBorderThickness()
    {
        if (!IsLastRow)
        {
            if (!IsLastColumn)
            {
                SetCurrentValue(EffectiveBorderThicknessProperty, new Thickness(0, 0, BorderThickness.Right, BorderThickness.Bottom));
            }
            else
            {
                SetCurrentValue(EffectiveBorderThicknessProperty, new Thickness(0, 0, 0, BorderThickness.Bottom));
            }
        }
        else
        {
            if (!IsLastColumn)
            {
                SetCurrentValue(EffectiveBorderThicknessProperty, new Thickness(0, 0, BorderThickness.Right, 0));
            }
            else
            {
                SetCurrentValue(EffectiveBorderThicknessProperty, new Thickness(0, 0, 0, 0));
            }
        }
    }
}
