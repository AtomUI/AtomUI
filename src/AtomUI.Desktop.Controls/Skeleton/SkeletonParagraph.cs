using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public class SkeletonParagraph : AbstractSkeleton
{
    #region 公共属性定义

    public static readonly StyledProperty<Dimension> LastLineWidthProperty =
        AvaloniaProperty.Register<SkeletonParagraph, Dimension>(nameof(LastLineWidth), new Dimension(1.0, DimensionUnitType.Percentage));

    public static readonly StyledProperty<List<Dimension>?> LineWidthsProperty =
        AvaloniaProperty.Register<SkeletonParagraph, List<Dimension>?>(nameof(LineWidths));

    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<SkeletonParagraph, int>(nameof(Rows), 2, validate: i => i >= 1);

    public static readonly StyledProperty<bool> IsRoundProperty =
        SkeletonLine.IsRoundProperty.AddOwner<SkeletonParagraph>();

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

    private StackPanel? _linesLayout;

    public SkeletonParagraph()
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
            ConfigureLastLineWidth();
        }
        else if (change.Property == LineWidthsProperty)
        {
            ConfigureLastLineWidths();
        }
        else if (change.Property == IsActiveProperty)
        {
            ConfigureLinesActiveState();
        }
        else if (change.Property == IsRoundProperty)
        {
            ConfigureLinesRoundness();
        }
        else if (change.Property == BackgroundProperty)
        {
            ConfigureLinesBackground();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _linesLayout = e.NameScope.Find<StackPanel>("PART_LineLayout");
        BuildLines();
    }

    private void BuildLines()
    {
        if (_linesLayout != null)
        {
            _linesLayout.Children.Clear();
            for (var i = 0; i < Rows; i++)
            {
                var line = new SkeletonLine
                {
                    IsActive = IsActive,
                    IsRound = IsRound,
                    Background = Background
                };
                ConfigureLineWidth(line, i);
                _linesLayout.Children.Add(line);
            }
        }
    }

    private void ConfigureLastLineWidth()
    {
        if (_linesLayout != null && _linesLayout.Children.Count > 0)
        {
            if (_linesLayout.Children[_linesLayout.Children.Count - 1] is SkeletonLine lastLine)
            {
                ConfigureLineWidth(lastLine, _linesLayout.Children.Count - 1);
            }
        }
    }

    private void ConfigureLastLineWidths()
    {
        if (_linesLayout != null)
        {
            for (var i = 0; i < Math.Min(Rows, _linesLayout.Children.Count); i++)
            {
                if (_linesLayout.Children[i] is SkeletonLine line)
                {
                    ConfigureLineWidth(line, i);
                }
            }
        }
    }

    private void ConfigureLineWidth(SkeletonLine line, int index)
    {
        if (LineWidths != null && index < LineWidths.Count)
        {
            line.LineWidth = LineWidths[index];
        }
        else if (index == Rows - 1)
        {
            line.LineWidth = LastLineWidth;
        }
        else
        {
            line.ClearValue(SkeletonLine.LineWidthProperty);
        }
    }

    private void ConfigureLinesActiveState()
    {
        if (_linesLayout != null)
        {
            foreach (var child in _linesLayout.Children)
            {
                if (child is SkeletonLine line)
                {
                    line.IsActive = IsActive;
                }
            }
        }
    }

    private void ConfigureLinesRoundness()
    {
        if (_linesLayout != null)
        {
            foreach (var child in _linesLayout.Children)
            {
                if (child is SkeletonLine line)
                {
                    line.IsRound = IsRound;
                }
            }
        }
    }

    private void ConfigureLinesBackground()
    {
        if (_linesLayout != null)
        {
            foreach (var child in _linesLayout.Children)
            {
                if (child is SkeletonLine line)
                {
                    line.Background = Background;
                }
            }
        }
    }
}
