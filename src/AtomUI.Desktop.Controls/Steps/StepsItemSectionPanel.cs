using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class StepsItemSectionPanel : Panel
{
    // The arrange width is the layout-rounded value of the measured heading width,
    // while the check below recomputes header+subHeader as a fresh double sum. On a
    // heading that exactly fills the section the two can differ by a rounding ulp,
    // which must not flip the same-line decision between measure and arrange or the
    // wrapped subheader is stacked onto the content line (text overlap).
    private const double SameLineTolerance = 0.01;

    public static readonly StyledProperty<StepsType> TypeProperty =
        AvaloniaProperty.Register<StepsItemSectionPanel, StepsType>(nameof(Type), StepsType.Default);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<StepsItemSectionPanel, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<Orientation> TitlePlacementProperty =
        AvaloniaProperty.Register<StepsItemSectionPanel, Orientation>(nameof(TitlePlacement), Orientation.Horizontal);

    public StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Orientation TitlePlacement
    {
        get => GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    internal Orientation EffectiveTitlePlacement =>
        StepsItemLayoutPanel.ResolveTitlePlacement(Type, Orientation, TitlePlacement);

    /// <summary>Header line height within the section, excluding the indicator.</summary>
    internal double HeadingHeight { get; private set; }

    /// <summary>Indicator-inclusive heading height supplied by the parent for horizontal arrange.</summary>
    internal double ArrangedHeadingHeight { get; set; } = double.NaN;

    /// <summary>Whether the section holds at least one visible body part.</summary>
    internal bool HasVisibleBody { get; private set; }

    /// <summary>Right edge of the header line in section coordinates, valid after arrange.</summary>
    internal double HeadingLineRight { get; private set; }

    static StepsItemSectionPanel()
    {
        AffectsMeasure<StepsItemSectionPanel>(TypeProperty, OrientationProperty, TitlePlacementProperty);
        AffectsArrange<StepsItemSectionPanel>(TypeProperty, OrientationProperty, TitlePlacementProperty);
        // The body children keep the layout panel's Role attached property, so a role
        // change must invalidate this panel (their layout parent) the same way it
        // invalidates the layout panel when the child lives directly in it.
        AffectsParentMeasure<StepsItemSectionPanel>(StepsItemLayoutPanel.RoleProperty);
        AffectsParentArrange<StepsItemSectionPanel>(StepsItemLayoutPanel.RoleProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        var headerChild    = FindBodyChild(StepsItemLayoutRole.Header);
        var subHeaderChild = FindBodyChild(StepsItemLayoutRole.SubHeader);
        var contentChild   = FindBodyChild(StepsItemLayoutRole.Content);
        HasVisibleBody = headerChild is not null || subHeaderChild is not null || contentChild is not null;

        var header    = headerChild?.DesiredSize ?? default;
        var subHeader = subHeaderChild?.DesiredSize ?? default;
        var content   = contentChild?.DesiredSize ?? default;

        if (EffectiveTitlePlacement == Orientation.Horizontal)
        {
            var sameLine = header.Width + subHeader.Width <= availableSize.Width + SameLineTolerance;
            HeadingHeight = sameLine
                ? Math.Max(header.Height, subHeader.Height)
                : header.Height + subHeader.Height;
            var bodyWidth = sameLine
                ? Math.Max(header.Width + subHeader.Width, content.Width)
                : Math.Max(Math.Max(header.Width, subHeader.Width), content.Width);
            return new Size(bodyWidth, HeadingHeight + content.Height);
        }

        var verticalBodyWidth = Math.Max(header.Width, Math.Max(subHeader.Width, content.Width));
        var verticalBodyHeight = header.Height + subHeader.Height + content.Height;
        return new Size(verticalBodyWidth, verticalBodyHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var header    = FindBodyChild(StepsItemLayoutRole.Header);
        var subHeader = FindBodyChild(StepsItemLayoutRole.SubHeader);
        var content   = FindBodyChild(StepsItemLayoutRole.Content);

        if (EffectiveTitlePlacement == Orientation.Horizontal)
        {
            ArrangeHorizontalBody(header, subHeader, content, finalSize);
        }
        else
        {
            ArrangeVerticalCenteredBody(header, subHeader, content, finalSize);
        }

        return finalSize;
    }

    private void ArrangeHorizontalBody(
        Control? header,
        Control? subHeader,
        Control? content,
        Size finalSize)
    {
        var bodyWidth = finalSize.Width;
        var headerSize = header?.DesiredSize ?? default;
        var subHeaderSize = subHeader?.DesiredSize ?? default;
        var contentSize = content?.DesiredSize ?? default;
        var sameLine = headerSize.Width + subHeaderSize.Width <= bodyWidth + SameLineTolerance;
        // The parent arranges the section at the indicator-inclusive heading height;
        // when it has not supplied that value the content offset can be derived
        // from the arranged section height.
        var headingHeight = double.IsNaN(ArrangedHeadingHeight)
            ? finalSize.Height - contentSize.Height
            : ArrangedHeadingHeight;

        var headerWidth = Math.Min(headerSize.Width, bodyWidth);
        double headerY;
        double subHeaderX;
        double subHeaderY;
        double subHeaderWidth;
        if (sameLine)
        {
            headerY = Math.Max(0, (headingHeight - headerSize.Height) / 2);
            subHeaderX = headerWidth;
            subHeaderY = Math.Max(0, (headingHeight - subHeaderSize.Height) / 2);
            // The decision above established the heading fits; give the subheader its
            // full desired width so the tolerance slack cannot shave the slot and
            // re-wrap the text inside it.
            subHeaderWidth = subHeaderSize.Width;
        }
        else
        {
            // The subheader does not fit beside the header, so it wraps onto its own
            // line below it and the heading grows to hold both lines.
            var headingBlockHeight = headerSize.Height + subHeaderSize.Height;
            var headingBlockY = Math.Max(0, (headingHeight - headingBlockHeight) / 2);
            headerY = headingBlockY;
            subHeaderX = 0;
            subHeaderY = headingBlockY + headerSize.Height;
            subHeaderWidth = Math.Min(subHeaderSize.Width, bodyWidth);
        }

        Arrange(header, new Rect(0, headerY, headerWidth, headerSize.Height));
        Arrange(subHeader, new Rect(subHeaderX, subHeaderY, subHeaderWidth, subHeaderSize.Height));

        var contentWidth = Math.Min(contentSize.Width, bodyWidth);
        Arrange(content, new Rect(
            0,
            headingHeight,
            contentWidth,
            Math.Min(contentSize.Height, Math.Max(0, finalSize.Height - headingHeight))));

        HeadingLineRight = Math.Max(header?.Bounds.Right ?? 0, subHeader?.Bounds.Right ?? 0);
    }

    private static void ArrangeVerticalCenteredBody(
        Control? header,
        Control? subHeader,
        Control? content,
        Size finalSize)
    {
        var y = 0d;
        ArrangeBodyPartCentered(header, finalSize.Width, ref y);
        ArrangeBodyPartCentered(subHeader, finalSize.Width, ref y);
        ArrangeBodyPartCentered(content, finalSize.Width, ref y);
    }

    private static void ArrangeBodyPartCentered(Control? child, double bodyWidth, ref double y)
    {
        if (child is null)
        {
            return;
        }

        var width  = Math.Min(child.DesiredSize.Width, bodyWidth);
        var height = child.DesiredSize.Height;
        var x      = Math.Clamp((bodyWidth - width) / 2, 0, bodyWidth - width);
        child.Arrange(new Rect(x, y, width, height));
        y += height;
    }

    private Control? FindBodyChild(StepsItemLayoutRole role)
    {
        return Children.FirstOrDefault(child =>
            StepsItemLayoutPanel.GetRole(child) == role && child.IsVisible);
    }

    private static void Arrange(Control? control, Rect bounds)
    {
        control?.Arrange(bounds);
    }
}
