

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A panel that arranges child controls using CSS FlexBox principles.
/// It organizes child items in one or more lines along a main-axis (either row or column)
/// and provides advanced control over their sizing and layout.
/// </summary>
/// <remarks>
/// See CSS FlexBox specification: https://www.w3.org/TR/css-flexbox-1
/// </remarks>
public sealed class FlexPanel : Panel
{
    private static readonly Func<Layoutable, int> s_getOrder = x => x is { } y ? Flex.GetOrder(y) : 0;
    private static readonly Func<Layoutable, bool> s_isVisible = x => x.IsVisible;

    /// <summary>
    /// Defines the <see cref="Direction"/> property.
    /// </summary>
    public static readonly StyledProperty<FlexDirection> DirectionProperty =
        AvaloniaProperty.Register<FlexPanel, FlexDirection>(nameof(Direction));

    /// <summary>
    /// Defines the <see cref="JustifyContent"/> property.
    /// </summary>
    public static readonly StyledProperty<JustifyContent> JustifyContentProperty =
        AvaloniaProperty.Register<FlexPanel, JustifyContent>(nameof(JustifyContent));

    /// <summary>
    /// Defines the <see cref="AlignItems"/> property.
    /// </summary>
    public static readonly StyledProperty<AlignItems> AlignItemsProperty =
        AvaloniaProperty.Register<FlexPanel, AlignItems>(nameof(AlignItems), AlignItems.Stretch);

    /// <summary>
    /// Defines the <see cref="AlignContent"/> property.
    /// </summary>
    public static readonly StyledProperty<AlignContent> AlignContentProperty =
        AvaloniaProperty.Register<FlexPanel, AlignContent>(nameof(AlignContent), AlignContent.Stretch);

    /// <summary>
    /// Defines the <see cref="Wrap"/> property.
    /// </summary>
    public static readonly StyledProperty<FlexWrap> WrapProperty =
        AvaloniaProperty.Register<FlexPanel, FlexWrap>(nameof(Wrap));

    /// <summary>
    /// Defines the <see cref="ColumnSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<FlexPanel, double>(nameof(ColumnSpacing));

    /// <summary>
    /// Defines the <see cref="RowSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<FlexPanel, double>(nameof(RowSpacing));

    private FlexLayoutState? _state;

    static FlexPanel()
    {
        AffectsMeasure<FlexPanel>(
            DirectionProperty,
            JustifyContentProperty,
            WrapProperty,
            ColumnSpacingProperty,
            RowSpacingProperty);

        AffectsArrange<FlexPanel>(
            AlignItemsProperty,
            AlignContentProperty);

        AffectsParentMeasure<FlexPanel>(
            HorizontalAlignmentProperty,
            VerticalAlignmentProperty,
            Flex.OrderProperty,
            Flex.BasisProperty,
            Flex.ShrinkProperty,
            Flex.GrowProperty);

        AffectsParentArrange<FlexPanel>(
            Flex.AlignSelfProperty);
    }

    /// <summary>
    /// Gets or sets the direction of the <see cref="FlexPanel"/>'s main-axis,
    /// determining the orientation in which child controls are laid out.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to <see cref="FlexDirection.Row"/>.
    /// Equivalent to CSS flex-direction property
    /// </remarks>
    public FlexDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the main-axis alignment of child items inside a line of the <see cref="FlexPanel"/>.
    /// Typically used to distribute extra free space leftover after flexible lengths and margins have been resolved.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to <see cref="JustifyContent.FlexStart"/>.
    /// Equivalent to CSS justify-content property.
    /// </remarks>
    public JustifyContent JustifyContent
    {
        get => GetValue(JustifyContentProperty);
        set => SetValue(JustifyContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the cross-axis alignment of all child items inside a line of the <see cref="FlexPanel"/>.
    /// Similar to <see cref="JustifyContent"/>, but in the perpendicular direction.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to <see cref="AlignItems.Stretch"/>.
    /// Equivalent to CSS align-items property.
    /// </remarks>
    public AlignItems AlignItems
    {
        get => GetValue(AlignItemsProperty);
        set => SetValue(AlignItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the cross-axis alignment of lines in the <see cref="FlexPanel"/> when there is extra space.
    /// Similar to <see cref="AlignItems"/>, but for entire lines.
    /// <see cref="FlexPanel.Wrap"/> property set to <see cref="FlexWrap.Wrap"/> mode
    /// allows controls to be arranged on multiple lines.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to <see cref="AlignContent.Stretch"/>.
    /// Equivalent to CSS align-content property.
    /// </remarks>
    public AlignContent AlignContent
    {
        get => GetValue(AlignContentProperty);
        set => SetValue(AlignContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the wrap mode, controlling whether the <see cref="FlexPanel"/> is single-line or multi-line.
    /// Additionally, it determines the cross-axis stacking direction for new lines.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to <see cref="FlexWrap.NoWrap"/>.
    /// Equivalent to CSS flex-wrap property.
    /// </remarks>
    public FlexWrap Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum horizontal spacing between child items or lines,
    /// depending on main-axis direction of the <see cref="FlexPanel"/>.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to 0.
    /// Similar to CSS column-gap property.
    /// </remarks>
    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum vertical spacing between child items or lines,
    /// depending on main-axis direction of the <see cref="FlexPanel"/>.
    /// </summary>
    /// <remarks>
    /// When omitted, it is set to 0.
    /// Similar to CSS row-gap property.
    /// </remarks>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var children = (IReadOnlyList<Layoutable>)Children;
        children = children.Where(s_isVisible).OrderBy(s_getOrder).ToArray();

        var isColumn = Direction is FlexDirection.Column or FlexDirection.ColumnReverse;

        var max = Uv.FromSize(availableSize, isColumn);
        var spacing = Uv.FromSize(ColumnSpacing, RowSpacing, isColumn);

        LineData lineData = default;
        var (childIndex, firstChildIndex, itemIndex) = (0, 0, 0);

        var flexLines = new List<FlexLine>();
        var useFinalSizeForRelativeBasis = false;

        foreach (var element in children)
        {
            if (!useFinalSizeForRelativeBasis && double.IsInfinity(max.U) &&
                Flex.GetBasis(element).Kind == FlexBasisKind.Relative)
            {
                useFinalSizeForRelativeBasis = true;
            }

            var size = MeasureChild(element, max, isColumn);

            if (Wrap != FlexWrap.NoWrap && lineData.U + size.U + itemIndex * spacing.U > max.U)
            {
                flexLines.Add(new FlexLine(firstChildIndex, childIndex - 1, lineData));
                lineData = default;
                firstChildIndex = childIndex;
                itemIndex = 0;
            }

            lineData.U += size.U;
            lineData.V = Math.Max(lineData.V, size.V);
            lineData.Shrink += Flex.GetShrink(element);
            lineData.Grow += Flex.GetGrow(element);
            lineData.AutoMargins += GetItemAutoMargins(element, isColumn);
            itemIndex++;
            childIndex++;
        }

        if (itemIndex != 0)
        {
            flexLines.Add(new FlexLine(firstChildIndex, firstChildIndex + itemIndex - 1, lineData));
        }

        var state = new FlexLayoutState(children, flexLines, useFinalSizeForRelativeBasis);

        var totalSpacingV = (flexLines.Count - 1) * spacing.V;
        var panelSizeU = flexLines.Count > 0
            ? flexLines.Max(flexLine => flexLine.U + (flexLine.Count - 1) * spacing.U)
            : 0.0;

        // Resizing along main axis using grow and shrink factors can affect cross axis, so remeasure affected items and lines.
        foreach (var flexLine in flexLines)
        {
            var (_, _, _, freeU) = GetLineMeasureU(flexLine, max.U, spacing.U);
            var (_, _, flexFreeU) = GetLineMultInfo(flexLine, freeU);

            if (flexFreeU != 0.0)
            {
                var lineItems = state.GetLineItems(flexLine).ToArray();
                ResolveFlexibleLengths(lineItems, flexFreeU);

                foreach (var element in lineItems)
                {
                    var length = Flex.GetCurrentLength(element);
                    element.Measure(Uv.ToSize(max.WithU(length), isColumn));
                }

                flexLine.V = lineItems.Max(i => Uv.FromSize(i.DesiredSize, isColumn).V);
            }
        }

        _state = state;
        var totalLineV = flexLines.Sum(l => l.V);
        var panelSize = flexLines.Count == 0 ? default : new Uv(panelSizeU, totalLineV + totalSpacingV);

        var stretchU = isColumn
            ? VerticalAlignment == VerticalAlignment.Stretch
            : HorizontalAlignment == HorizontalAlignment.Stretch;
        var stretchV = isColumn
            ? HorizontalAlignment == HorizontalAlignment.Stretch
            : VerticalAlignment == VerticalAlignment.Stretch;

        if (stretchU && !double.IsInfinity(max.U))
        {
            panelSize = panelSize.WithU(max.U);
        }

        if (stretchV && !double.IsInfinity(max.V))
        {
            panelSize = panelSize.WithV(max.V);
        }
        return Uv.ToSize(panelSize, isColumn);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var state = _state ?? throw new InvalidOperationException();

        var isColumn = Direction is FlexDirection.Column or FlexDirection.ColumnReverse;
        var isReverse = Direction is FlexDirection.RowReverse or FlexDirection.ColumnReverse;
        var isWrapReverse = Wrap == FlexWrap.WrapReverse;

        var panelSize = Uv.FromSize(finalSize, isColumn);
        var spacing = Uv.FromSize(ColumnSpacing, RowSpacing, isColumn);

        var linesCount = state.Lines.Count;
        var totalLineV = state.Lines.Sum(s => s.V);
        var totalSpacingV = (linesCount - 1) * spacing.V;
        var totalV = totalLineV + totalSpacingV;
        var freeV = panelSize.V - totalV;
        var singleLineNoWrap = Wrap == FlexWrap.NoWrap && linesCount == 1;

        if (singleLineNoWrap)
        {
            totalLineV = panelSize.V;
            totalSpacingV = 0.0;
            totalV = totalLineV;
            freeV = 0.0;
        }

        var alignContent = DetermineAlignContent(AlignContent, freeV, linesCount);

        var (v, spacingV) = GetCrossAxisPosAndSpacing(alignContent, spacing, freeV, linesCount);

        var scaleV = alignContent == AlignContent.Stretch && totalLineV != 0
            ? (panelSize.V - totalSpacingV) / totalLineV
            : 1.0;

        var crossOffset = v;
        foreach (var line in state.Lines)
        {
            var lineItems = state.GetLineItems(line).ToArray();
            if (state.UseFinalSizeForRelativeBasis)
            {
                ApplyRelativeBasisUsingPanelSize(lineItems, panelSize.U);
            }

            var lineV = singleLineNoWrap ? panelSize.V : scaleV * line.V;
            var itemsCount = lineItems.Length;
            var totalSpacingU = (itemsCount - 1) * spacing.U;
            var lineU = state.UseFinalSizeForRelativeBasis
                ? lineItems.Sum(item => Flex.GetCurrentLength(item))
                : line.U;
            var totalU = lineU + totalSpacingU;
            var freeU = panelSize.U - totalU;
            var (_, lineAutoMargins, flexFreeU) = GetLineMultInfo(line, freeU);

            var remainingFreeU = freeU - flexFreeU;
            remainingFreeU += ResolveFlexibleLengths(lineItems, flexFreeU);

            var lineStart = isWrapReverse ? panelSize.V - crossOffset - lineV : crossOffset;
            var currentFreeU = remainingFreeU;
            if (lineAutoMargins != 0 && remainingFreeU != 0.0)
            {
                foreach (var element in lineItems)
                {
                    var baseLength = Flex.GetCurrentLength(element);
                    var autoMargins = GetItemAutoMargins(element, isColumn);
                    if (autoMargins != 0)
                    {
                        var length = Math.Max(0.0, baseLength + remainingFreeU * autoMargins / lineAutoMargins);
                        Flex.SetCurrentLength(element, length);
                        currentFreeU -= length - baseLength;
                    }
                }
            }
            remainingFreeU = currentFreeU;

            var (u, spacingU) = GetMainAxisPosAndSpacing(JustifyContent, spacing, remainingFreeU, itemsCount);

            foreach (var element in lineItems)
            {
                var size = Uv.FromSize(element.DesiredSize, isColumn).WithU(Flex.GetCurrentLength(element));
                var align = Flex.GetAlignSelf(element) ?? AlignItems;
                var hasExplicitCrossSize = isColumn
                    ? element.IsSet(Layoutable.WidthProperty)
                    : element.IsSet(Layoutable.HeightProperty);
                if (align == AlignItems.Stretch && hasExplicitCrossSize)
                {
                    align = AlignItems.FlexStart;
                }
                if (isWrapReverse)
                {
                    align = align switch
                    {
                        AlignItems.FlexStart => AlignItems.FlexEnd,
                        AlignItems.FlexEnd => AlignItems.FlexStart,
                        _ => align
                    };
                }

                var positionV = align switch
                {
                    AlignItems.FlexStart => lineStart,
                    AlignItems.FlexEnd => lineStart + lineV - size.V,
                    AlignItems.Center => lineStart + (lineV - size.V) / 2,
                    AlignItems.Stretch => lineStart,
                    _ => throw new InvalidOperationException()
                };

                size = size.WithV(align == AlignItems.Stretch ? lineV : size.V);
                var position = new Uv(isReverse ? panelSize.U - size.U - u : u, positionV);
                element.Arrange(new Rect(Uv.ToPoint(position, isColumn), Uv.ToSize(size, isColumn)));

                u += size.U + spacingU;
            }

            crossOffset += lineV + spacingV;
        }

        return finalSize;
    }

    private static Uv MeasureChild(Layoutable element, Uv max, bool isColumn)
    {
        var basis = Flex.GetBasis(element);
        var basisKind = basis.Kind;
        if (basisKind == FlexBasisKind.Relative && double.IsInfinity(max.U))
        {
            basisKind = FlexBasisKind.Auto;
        }
        var flexConstraint = basisKind switch
        {
            FlexBasisKind.Auto => max.U,
            FlexBasisKind.Absolute => basis.Value,
            FlexBasisKind.Relative => max.U * basis.Value,
            _ => throw new InvalidOperationException($"Unsupported FlexBasisKind value: {basis.Kind}")
        };
        var hasExplicitMin = isColumn
            ? element.IsSet(Layoutable.MinHeightProperty)
            : element.IsSet(Layoutable.MinWidthProperty);
        var useAutoMeasuredMin = !hasExplicitMin && basisKind != FlexBasisKind.Auto && flexConstraint != max.U;
        var autoMinLength = 0.0;
        if (useAutoMeasuredMin)
        {
            element.Measure(Uv.ToSize(max.WithU(max.U), isColumn));
            autoMinLength = Uv.FromSize(element.DesiredSize, isColumn).U;
        }

        element.Measure(Uv.ToSize(max.WithU(flexConstraint), isColumn));

        var size = Uv.FromSize(element.DesiredSize, isColumn);
        var minLength = hasExplicitMin
            ? Math.Max(0.0, isColumn ? element.MinHeight : element.MinWidth)
            : Math.Max(0.0, useAutoMeasuredMin ? autoMinLength : size.U);
        Flex.SetMinLength(element, minLength);

        var flexLength = basisKind switch
        {
            FlexBasisKind.Auto => Math.Max(size.U, minLength),
            FlexBasisKind.Absolute or FlexBasisKind.Relative => Math.Max(flexConstraint, minLength),
            _ => throw new InvalidOperationException()
        };
        size = size.WithU(flexLength);

        Flex.SetBaseLength(element, flexLength);
        Flex.SetCurrentLength(element, flexLength);
        return size;
    }

    private static AlignContent DetermineAlignContent(AlignContent currentAlignContent, double freeV, int linesCount)
    {
        // Determine AlignContent based on available space and line count
        return currentAlignContent switch
        {
            AlignContent.Stretch when freeV > 0.0 => AlignContent.Stretch,
            AlignContent.SpaceBetween when freeV > 0.0 && linesCount > 1 => AlignContent.SpaceBetween,
            AlignContent.SpaceAround when freeV > 0.0 && linesCount > 0 => AlignContent.SpaceAround,
            AlignContent.SpaceEvenly when freeV > 0.0 && linesCount > 0 => AlignContent.SpaceEvenly,
            AlignContent.Stretch => AlignContent.FlexStart,
            AlignContent.SpaceBetween => AlignContent.FlexStart,
            AlignContent.SpaceAround => AlignContent.Center,
            AlignContent.SpaceEvenly => AlignContent.Center,
            AlignContent.FlexStart or AlignContent.Center or AlignContent.FlexEnd => currentAlignContent,
            _ => throw new InvalidOperationException($"Unsupported AlignContent value: {currentAlignContent}")
        };
    }

    private static (double v, double spacingV) GetCrossAxisPosAndSpacing(AlignContent alignContent, Uv spacing,
        double freeV, int linesCount)
    {
        return alignContent switch
        {
            AlignContent.FlexStart => (0.0, spacing.V),
            AlignContent.FlexEnd => (freeV, spacing.V),
            AlignContent.Center => (freeV / 2, spacing.V),
            AlignContent.Stretch => (0.0, spacing.V),
            AlignContent.SpaceBetween when linesCount > 1 => (0.0, spacing.V + freeV / (linesCount - 1)),
            AlignContent.SpaceBetween => (0.0, spacing.V),
            AlignContent.SpaceAround when linesCount > 0 => (freeV / linesCount / 2,
                spacing.V + freeV / linesCount),
            AlignContent.SpaceAround => (freeV / 2, spacing.V),
            AlignContent.SpaceEvenly => (freeV / (linesCount + 1), spacing.V + freeV / (linesCount + 1)),
            _ => throw new InvalidOperationException($"Unsupported AlignContent value: {alignContent}")
        };
    }

    private static (double u, double spacingU) GetMainAxisPosAndSpacing(JustifyContent justifyContent, Uv spacing,
        double remainingFreeU, int itemsCount)
    {
        if (remainingFreeU <= 0)
        {
            return justifyContent switch
            {
                JustifyContent.FlexEnd => (remainingFreeU, spacing.U),
                JustifyContent.Center => (remainingFreeU / 2, spacing.U),
                _ => (0.0, spacing.U),
            };
        }

        return justifyContent switch
        {
            JustifyContent.FlexStart => (0.0, spacing.U),
            JustifyContent.FlexEnd => (remainingFreeU, spacing.U),
            JustifyContent.Center => (remainingFreeU / 2, spacing.U),
            JustifyContent.SpaceBetween when itemsCount > 1 =>
                (0.0, spacing.U + remainingFreeU / (itemsCount - 1)),
            JustifyContent.SpaceBetween => (0.0, spacing.U),
            JustifyContent.SpaceAround when itemsCount > 0 =>
                (remainingFreeU / itemsCount / 2, spacing.U + remainingFreeU / itemsCount),
            JustifyContent.SpaceAround => (remainingFreeU / 2, spacing.U),
            JustifyContent.SpaceEvenly when itemsCount > 0 =>
                (remainingFreeU / (itemsCount + 1), spacing.U + remainingFreeU / (itemsCount + 1)),
            JustifyContent.SpaceEvenly => (remainingFreeU / 2, spacing.U),
            _ => throw new InvalidOperationException($"Unsupported JustifyContent value: {justifyContent}")
        };
    }

    private static (int ItemsCount, double TotalSpacingU, double TotalU, double FreeU) GetLineMeasureU(
        FlexLine line, double panelSizeU, double spacingU)
    {
        var itemsCount = line.Count;
        var totalSpacingU = (itemsCount - 1) * spacingU;
        var totalU = line.U + totalSpacingU;
        var freeU = panelSizeU - totalU;
        return (itemsCount, totalSpacingU, totalU, freeU);
    }

    private static (double LineMult, double LineAutoMargins, double RemainingFreeU) GetLineMultInfo(FlexLine line,
        double freeU)
    {
        var lineMult = freeU switch
        {
            < 0 => line.Shrink,
            > 0 => line.Grow,
            _ => 0.0,
        };
        if (lineMult == 0.0)
        {
            return (0.0, line.AutoMargins, 0.0);
        }

        // https://www.w3.org/TR/css-flexbox-1/#remaining-free-space
        // Sum of flex factors less than 1 reduces remaining free space to be distributed.
        return lineMult is > 0 and < 1
            ? (lineMult, line.AutoMargins, freeU * lineMult)
            : (lineMult, line.AutoMargins, freeU);
    }

    // Distribute free space across flex items, clamping shrink to measured minimum size.
    private static double ResolveFlexibleLengths(IReadOnlyList<Layoutable> items, double flexFreeU)
    {
        foreach (var element in items)
        {
            Flex.SetCurrentLength(element, Flex.GetBaseLength(element));
        }

        if (flexFreeU == 0.0)
        {
            return 0.0;
        }

        var isShrink = flexFreeU < 0;
        var activeItems = new List<(Layoutable Item, double Mult)>(items.Count);

        foreach (var element in items)
        {
            var mult = isShrink ? Flex.GetShrink(element) : Flex.GetGrow(element);
            if (mult > 0.0 && isShrink)
            {
                mult *= Flex.GetBaseLength(element);
            }

            if (mult > 0.0)
            {
                activeItems.Add((element, mult));
            }
        }

        if (activeItems.Count == 0)
        {
            return flexFreeU;
        }

        var remainingFreeU = flexFreeU;

        while (activeItems.Count > 0)
        {
            var totalMult = activeItems.Sum(item => item.Mult);
            if (totalMult == 0.0)
            {
                break;
            }

            if (isShrink)
            {
                var clamped = false;
                for (var i = activeItems.Count - 1; i >= 0; i--)
                {
                    var (element, mult) = activeItems[i];
                    var baseLength = Flex.GetBaseLength(element);
                    var target = baseLength + remainingFreeU * mult / totalMult;
                    var minLength = GetItemMinLength(element);
                    if (target < minLength)
                    {
                        Flex.SetCurrentLength(element, minLength);
                        remainingFreeU -= minLength - baseLength;
                        activeItems.RemoveAt(i);
                        clamped = true;
                    }
                }

                if (clamped)
                {
                    continue;
                }
            }

            var consumed = 0.0;
            foreach (var (element, mult) in activeItems)
            {
                var baseLength = Flex.GetBaseLength(element);
                var target = baseLength + remainingFreeU * mult / totalMult;
                var length = Math.Max(0.0, target);
                Flex.SetCurrentLength(element, length);
                consumed += length - baseLength;
            }

            remainingFreeU -= consumed;
            break;
        }

        return remainingFreeU;
    }

    private static void ApplyRelativeBasisUsingPanelSize(IReadOnlyList<Layoutable> items, double panelSizeU)
    {
        if (double.IsInfinity(panelSizeU) || double.IsNaN(panelSizeU))
        {
            return;
        }

        foreach (var element in items)
        {
            var basis = Flex.GetBasis(element);
            if (basis.Kind != FlexBasisKind.Relative)
            {
                continue;
            }

            var flexConstraint = panelSizeU * basis.Value;
            var minLength = Flex.GetMinLength(element);
            var flexLength = Math.Max(flexConstraint, minLength);
            Flex.SetBaseLength(element, flexLength);
            Flex.SetCurrentLength(element, flexLength);
        }
    }

    private static double GetItemMinLength(Layoutable element)
    {
        var minLength = Flex.GetMinLength(element);
        return Math.Max(0.0, minLength);
    }

    private static int GetItemAutoMargins(Layoutable element, bool isColumn)
    {
        // Avoid consuming free space via alignment-based "auto margins" because many controls
        // in AtomUI default to Left/Top alignment, which would neutralize justify spacing.
        return 0;
    }

    private readonly struct FlexLayoutState
    {
        private readonly IReadOnlyList<Layoutable> _children;

        public IReadOnlyList<FlexLine> Lines { get; }

        public bool UseFinalSizeForRelativeBasis { get; }

        public FlexLayoutState(IReadOnlyList<Layoutable> children, List<FlexLine> lines,
            bool useFinalSizeForRelativeBasis)
        {
            _children = children;
            Lines = lines;
            UseFinalSizeForRelativeBasis = useFinalSizeForRelativeBasis;
        }

        public IEnumerable<Layoutable> GetLineItems(FlexLine line)
        {
            for (var i = line.First; i <= line.Last; i++)
            {
                yield return _children[i];
            }
        }
    }

    private struct LineData
    {
        public double U { get; set; }

        public double V { get; set; }

        public double Shrink { get; set; }

        public double Grow { get; set; }

        public int AutoMargins { get; set; }
    }

    private class FlexLine
    {
        public FlexLine(int first, int last, LineData line)
        {
            First = first;
            Last = last;
            U = line.U;
            V = line.V;
            Shrink = line.Shrink;
            Grow = line.Grow;
            AutoMargins = line.AutoMargins;
        }

        /// <summary>First item index.</summary>
        public int First { get; }

        /// <summary>Last item index.</summary>
        public int Last { get; }

        /// <summary>Sum of main sizes of items.</summary>
        public double U { get; }

        /// <summary>Max of cross sizes of items.</summary>
        public double V { get; set; }

        /// <summary>Sum of shrink factors of flexible items.</summary>
        public double Shrink { get; }

        /// <summary>Sum of grow factors of flexible items.</summary>
        public double Grow { get; }

        /// <summary>Number of "auto margins" along main axis.</summary>
        public int AutoMargins { get; }

        /// <summary>Number of items.</summary>
        public int Count => Last - First + 1;
    }
}
