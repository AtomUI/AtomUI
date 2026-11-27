using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base of a skeleton paragraph: a stack of <see cref="Rows" /> line controls, created by the derived class,
/// whose widths and corner style follow this control. The template supplies the panel the lines are added to.
/// </summary>
public abstract class AbstractSkeletonParagraph : AbstractSkeleton
{
    internal const string LineLayoutPart = "PART_LineLayout";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="LastLineWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension> LastLineWidthProperty =
        AvaloniaProperty.Register<AbstractSkeletonParagraph, Dimension>(nameof(LastLineWidth), new Dimension(100.0, DimensionUnitType.Percentage));

    /// <summary>
    /// Identifies the <see cref="LineWidths" /> property.
    /// </summary>
    public static readonly StyledProperty<List<Dimension>?> LineWidthsProperty =
        AvaloniaProperty.Register<AbstractSkeletonParagraph, List<Dimension>?>(nameof(LineWidths));

    /// <summary>
    /// Identifies the <see cref="Rows" /> property.
    /// </summary>
    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<AbstractSkeletonParagraph, int>(nameof(Rows), 2, validate: i => i >= 1);

    /// <summary>
    /// Identifies the <see cref="IsRound" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRoundProperty =
        AbstractSkeletonLine.IsRoundProperty.AddOwner<AbstractSkeletonParagraph>();

    /// <summary>
    /// Width of the last line, as a percentage of the width offered to the line or as an exact width in DIPs; the
    /// line never grows beyond the offered width. It is used only while <see cref="LineWidths" /> has no entry for
    /// the last row.
    /// </summary>
    public Dimension LastLineWidth
    {
        get => GetValue(LastLineWidthProperty);
        set => SetValue(LastLineWidthProperty, value);
    }

    /// <summary>
    /// Widths of the single lines in top-down order; the line of row <c>i</c> takes entry <c>i</c> when the list
    /// has one. Any other line falls back to <see cref="LastLineWidth" /> when it is the last row and to its own
    /// default width otherwise; <see langword="null" /> does so for every line. Each entry is a percentage of the
    /// width offered to the line or an exact width in DIPs.
    /// </summary>
    public List<Dimension>? LineWidths
    {
        get => GetValue(LineWidthsProperty);
        set => SetValue(LineWidthsProperty, value);
    }

    /// <summary>
    /// Number of lines of the paragraph; values below 1 are rejected. Changing it rebuilds all lines.
    /// </summary>
    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>
    /// Corner style passed to every line: true gives rounded ends, false gives the block corner radius.
    /// </summary>
    public bool IsRound
    {
        get => GetValue(IsRoundProperty);
        set => SetValue(IsRoundProperty, value);
    }

    #endregion

    private Panel? _linesLayout;

    /// <summary>
    /// Creates the control for one line of the paragraph; it is called for each row whenever the lines are built.
    /// </summary>
    /// <returns>
    /// A new control that becomes one line; its width and state are applied afterwards.
    /// </returns>
    protected abstract Control CreateLine();

    /// <summary>
    /// Gives a line the width resolved for its row; <see langword="null" /> means the line keeps its default width.
    /// </summary>
    /// <param name="line">
    /// A line created by <see cref="CreateLine" />.
    /// </param>
    /// <param name="width">
    /// The width of the line's row, or <see langword="null" /> for the default width.
    /// </param>
    protected abstract void ApplyLineWidth(Control line, Dimension? width);

    /// <summary>
    /// Passes the state of the paragraph to a line, such as <see cref="AbstractSkeleton.IsActive" />, <see
    /// cref="IsRound" /> and the background. It is called for every new line and again whenever one of these properties
    /// changes. The default implementation does nothing.
    /// </summary>
    /// <param name="line">
    /// A line created by <see cref="CreateLine" />.
    /// </param>
    protected virtual void ApplyLineState(Control line)
    {
    }

    /// <summary>
    /// Rebuilds the lines when <see cref="Rows" /> changes, reapplies the widths when <see cref="LineWidths" /> or
    /// <see cref="LastLineWidth" /> changes, and reapplies the state when <see cref="AbstractSkeleton.IsActive" />,
    /// <see cref="IsRound" /> or the background changes. Nothing happens before the line panel of the template is
    /// found. Overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Locates the line panel of the template and fills it with <see cref="Rows" /> lines. Overrides must call the
    /// base member.
    /// </summary>
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
