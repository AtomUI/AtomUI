using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls.Commons;

public abstract class AbstractSkeletonParagraph : AbstractSkeleton
{
    internal const string LineLayoutPart = "PART_LineLayout";

    #region 公共属性定义

    public static readonly StyledProperty<Dimension> LastLineWidthProperty =
        AvaloniaProperty.Register<AbstractSkeletonParagraph, Dimension>(nameof(LastLineWidth), new Dimension(100.0, DimensionUnitType.Percentage));

    public static readonly StyledProperty<List<Dimension>?> LineWidthsProperty =
        AvaloniaProperty.Register<AbstractSkeletonParagraph, List<Dimension>?>(nameof(LineWidths));

    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<AbstractSkeletonParagraph, int>(nameof(Rows), 2, validate: i => i >= 1);

    public static readonly StyledProperty<bool> IsRoundProperty =
        AbstractSkeletonLine.IsRoundProperty.AddOwner<AbstractSkeletonParagraph>();

    public Dimension LastLineWidth
    {
        get => GetValue(LastLineWidthProperty);
        set => SetValue(LastLineWidthProperty, value);
    }

    public List<Dimension>? LineWidths
    {
        get => GetValue(LineWidthsProperty);
        set => SetValue(LineWidthsProperty, value);
    }

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public bool IsRound
    {
        get => GetValue(IsRoundProperty);
        set => SetValue(IsRoundProperty, value);
    }

    #endregion

    private Panel? _linesLayout;

    protected abstract Control CreateLine();

    /// <summary>
    /// 把宽度写入行元素，width 为 null 表示该行使用默认宽度。
    /// </summary>
    protected abstract void ApplyLineWidth(Control line, Dimension? width);

    /// <summary>
    /// 把段落的状态（激活、圆角、背景等）同步到行元素。
    /// </summary>
    protected virtual void ApplyLineState(Control line)
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_linesLayout is null)
        {
            return;
        }

        if (change.Property == RowsProperty)
        {
            BuildLines();
        }
        else if (change.Property == LastLineWidthProperty)
        {
            UpdateLastLineWidth();
        }
        else if (change.Property == LineWidthsProperty)
        {
            UpdateLineWidths();
        }
        else if (change.Property == IsActiveProperty ||
                 change.Property == IsRoundProperty ||
                 change.Property == BackgroundProperty)
        {
            UpdateLineStates();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _linesLayout = e.NameScope.Find<Panel>(LineLayoutPart);
        BuildLines();
    }

    private void BuildLines()
    {
        if (_linesLayout is null)
        {
            return;
        }

        _linesLayout.Children.Clear();
        for (var i = 0; i < Rows; i++)
        {
            var line = CreateLine();
            ApplyLineState(line);
            ApplyLineWidth(line, ResolveLineWidth(i));
            _linesLayout.Children.Add(line);
        }
    }

    private void UpdateLastLineWidth()
    {
        if (_linesLayout is null || _linesLayout.Children.Count == 0)
        {
            return;
        }

        var lastIndex = _linesLayout.Children.Count - 1;
        ApplyLineWidth(_linesLayout.Children[lastIndex], ResolveLineWidth(lastIndex));
    }

    private void UpdateLineWidths()
    {
        if (_linesLayout is null)
        {
            return;
        }

        var count = Math.Min(Rows, _linesLayout.Children.Count);
        for (var i = 0; i < count; i++)
        {
            ApplyLineWidth(_linesLayout.Children[i], ResolveLineWidth(i));
        }
    }

    private void UpdateLineStates()
    {
        if (_linesLayout is null)
        {
            return;
        }

        foreach (var line in _linesLayout.Children)
        {
            ApplyLineState(line);
        }
    }

    private Dimension? ResolveLineWidth(int index)
    {
        var lineWidths = LineWidths;
        if (lineWidths != null && index < lineWidths.Count)
        {
            return lineWidths[index];
        }

        if (index == Rows - 1)
        {
            return LastLineWidth;
        }

        return null;
    }
}
