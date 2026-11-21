using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[TokenOwnerTransparent]
internal sealed class CardGridPanel : Grid
{
    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<CardGridPanel, double>(nameof(LineWidth));

    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    // The separators depend only on the arranged cells, so an arrange that leaves them unchanged reuses the geometry.
    private readonly List<Rect> _arrangedCells = [];
    private readonly List<Rect> _cellScratch = [];
    private Size _arrangedSize;
    private double _arrangedLineWidth = double.NaN;
    private Geometry? _separators;

    static CardGridPanel()
    {
        AffectsArrange<CardGridPanel>(LineWidthProperty);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var arrangedSize = base.ArrangeOverride(finalSize);
        var bounds = new Rect(arrangedSize);
        var lineWidth = BorderUtils.BuildRenderScaleAwareThickness(this, LineWidth);

        _cellScratch.Clear();
        foreach (var child in Children)
        {
            if (child is not CardGridItem { IsVisible: true } item)
            {
                continue;
            }

            var cell = item.Bounds;
            // A recycled container has its clip cleared, so the clip is refreshed on every arrange.
            item.SetGridHoverClip(new Rect(-cell.X, -cell.Y, bounds.Width, bounds.Height));
            _cellScratch.Add(cell);
        }

        if (arrangedSize != _arrangedSize ||
            !lineWidth.Equals(_arrangedLineWidth) ||
            !_cellScratch.SequenceEqual(_arrangedCells))
        {
            _arrangedSize      = arrangedSize;
            _arrangedLineWidth = lineWidth;
            _arrangedCells.Clear();
            _arrangedCells.AddRange(_cellScratch);
            _separators = BuildSeparators(bounds, lineWidth);
        }

        // ItemsPresenter.Panel isn't observable, and the items panel has its own template namescope.
        // Publish the finished geometry to the CardGridContent template overlay.
        if (TemplatedParent is CardGridContent owner)
        {
            owner.SeparatorGeometry = _separators;
        }

        return arrangedSize;
    }

    private Geometry? BuildSeparators(Rect bounds, double lineWidth)
    {
        var separators = new GeometryGroup { FillRule = FillRule.NonZero };

        foreach (var arrangedCell in _arrangedCells)
        {
            var cell = arrangedCell.Intersect(bounds);
            if (cell.Width <= 0 || cell.Height <= 0 || lineWidth <= 0)
            {
                continue;
            }

            AddVerticalSeparator(cell.Left);
            AddVerticalSeparator(cell.Right);
            AddHorizontalSeparator(cell.Top);
            AddHorizontalSeparator(cell.Bottom);

            void AddVerticalSeparator(double x)
            {
                if (x > 0 && x < bounds.Right)
                {
                    separators.Children.Add(new RectangleGeometry(new Rect(
                        x, cell.Top, Math.Min(lineWidth, bounds.Right - x), cell.Height)));
                }
            }

            void AddHorizontalSeparator(double y)
            {
                if (y > 0 && y < bounds.Bottom)
                {
                    separators.Children.Add(new RectangleGeometry(new Rect(
                        cell.Left, y, cell.Width, Math.Min(lineWidth, bounds.Bottom - y))));
                }
            }
        }

        return separators.Children.Count > 0 ? separators : null;
    }
}
