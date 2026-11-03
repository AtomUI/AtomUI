using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class DescriptionBorderedCell : ContentControl
{
    #region 公共属性定义

    public static readonly StyledProperty<double> RelativeLineHeightProperty =
        AvaloniaProperty.Register<DescriptionBorderedCell, double>(nameof(RelativeLineHeight));

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<DescriptionBorderedCell, double>(nameof(LineHeight));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DescriptionBorderedCell>();

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

    internal static readonly DirectProperty<DescriptionBorderedCell, bool> IsLastRowProperty =
        AvaloniaProperty.RegisterDirect<DescriptionBorderedCell, bool>(nameof(IsLastRow),
            o => o.IsLastRow,
            (o, v) => o.IsLastRow = v);

    internal static readonly DirectProperty<DescriptionBorderedCell, bool> IsLastColumnProperty =
        AvaloniaProperty.RegisterDirect<DescriptionBorderedCell, bool>(nameof(IsLastColumn),
            o => o.IsLastColumn,
            (o, v) => o.IsLastColumn = v);

    internal static readonly DirectProperty<DescriptionBorderedCell, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<DescriptionBorderedCell, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

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

    #region 公共与受保护 API

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

    #endregion

    #region 私有实现

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

    #endregion

}