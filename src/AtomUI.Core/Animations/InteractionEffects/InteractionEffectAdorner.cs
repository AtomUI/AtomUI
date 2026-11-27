using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

/// <summary>
/// Clips a presentation in the adorned control's coordinates while the lease is active.
/// </summary>
internal sealed class InteractionEffectAdorner : Decorator
{
    private readonly Control _surface;
    private readonly InteractionEffectClip _clip;
    private readonly CornerRadius _radius;
    private readonly Decorator _clipHost;
    private readonly List<Visual> _trackedAncestors = [];
    private TopLevel? _trackedRoot;
    private bool _released;

    internal InteractionEffectAdorner(Control surface, Control child, InteractionEffectClip clip, CornerRadius radius)
    {
        _surface = surface;
        _clip = clip;
        _radius = radius;
        IsHitTestVisible = false;
        _clipHost = new Decorator { Child = child };
        Child = _clipHost;
    }

    internal void ReleaseVisual()
    {
        _released = true;
        UnsubscribeAncestorTracking();
        _clipHost.Child = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_released)
        {
            SubscribeAncestorTracking();
            RefreshClip();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeAncestorTracking();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _clipHost.Clip = CalculateClip(finalSize);
        _clipHost.Arrange(new Rect(finalSize));
        return finalSize;
    }

    private void SubscribeAncestorTracking()
    {
        UnsubscribeAncestorTracking();
        foreach (var ancestor in _surface.GetSelfAndVisualAncestors())
        {
            ancestor.PropertyChanged += OnAncestorPropertyChanged;
            _trackedAncestors.Add(ancestor);
        }

        _trackedRoot = TopLevel.GetTopLevel(_surface);
        if (_trackedRoot is not null)
        {
            _trackedRoot.LayoutUpdated += OnRootLayoutUpdated;
        }
    }

    private void UnsubscribeAncestorTracking()
    {
        foreach (var ancestor in _trackedAncestors)
        {
            ancestor.PropertyChanged -= OnAncestorPropertyChanged;
        }
        _trackedAncestors.Clear();

        if (_trackedRoot is not null)
        {
            _trackedRoot.LayoutUpdated -= OnRootLayoutUpdated;
            _trackedRoot = null;
        }
    }

    private void OnAncestorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty ||
            e.Property == RenderTransformProperty ||
            e.Property == ClipProperty ||
            e.Property == ClipToBoundsProperty ||
            e.Property == IsVisibleProperty)
        {
            RefreshClip();
        }
    }

    private void OnRootLayoutUpdated(object? sender, EventArgs e) => RefreshClip();

    private void RefreshClip()
    {
        if (!_released && _clipHost.Child is not null)
        {
            _clipHost.Clip = CalculateClip(_surface.Bounds.Size);
        }
    }

    private Geometry? CalculateClip(Size size)
    {
        Geometry? intersection = _clip == InteractionEffectClip.InsideShape
            ? CreateOutline(new Rect(size), _radius)
            : null;
        foreach (var ancestor in _surface.GetSelfAndVisualAncestors())
        {
            if (!ancestor.IsVisible || ancestor.TransformToVisual(_surface) is not { } transform)
            {
                return new RectangleGeometry(default);
            }
            if (!ReferenceEquals(ancestor, _surface) && (ancestor.ClipToBounds || ancestor is TopLevel))
            {
                Intersect(new RectangleGeometry(new Rect(ancestor.Bounds.Size)), transform);
            }
            if (ancestor.Clip is { } clip)
            {
                Intersect(clip.Clone(), transform);
            }
        }
        return intersection;

        void Intersect(Geometry geometry, Matrix transform)
        {
            geometry.Transform = new MatrixTransform((geometry.Transform?.Value ?? Matrix.Identity) * transform);
            intersection = intersection is null
                ? geometry
                : new CombinedGeometry(GeometryCombineMode.Intersect, intersection, geometry);
        }
    }

    internal static Geometry CreateOutline(Rect bounds, CornerRadius radius)
    {
        var limit = Math.Min(bounds.Width, bounds.Height) / 2;
        var topLeft = Math.Clamp(radius.TopLeft, 0, limit);
        var topRight = Math.Clamp(radius.TopRight, 0, limit);
        var bottomLeft = Math.Clamp(radius.BottomLeft, 0, limit);
        var bottomRight = Math.Clamp(radius.BottomRight, 0, limit);
        if (topLeft == topRight && topLeft == bottomLeft && topLeft == bottomRight)
        {
            return new RectangleGeometry(bounds, topLeft, topLeft);
        }
        var geometry = new StreamGeometry();
        using var path = geometry.Open();
        path.BeginFigure(new Point(bounds.Left + topLeft, bounds.Top), isFilled: true);
        path.LineTo(new Point(bounds.Right - topRight, bounds.Top));
        Corner(new Point(bounds.Right, bounds.Top + topRight), topRight);
        path.LineTo(new Point(bounds.Right, bounds.Bottom - bottomRight));
        Corner(new Point(bounds.Right - bottomRight, bounds.Bottom), bottomRight);
        path.LineTo(new Point(bounds.Left + bottomLeft, bounds.Bottom));
        Corner(new Point(bounds.Left, bounds.Bottom - bottomLeft), bottomLeft);
        path.LineTo(new Point(bounds.Left, bounds.Top + topLeft));
        Corner(new Point(bounds.Left + topLeft, bounds.Top), topLeft);
        path.EndFigure(isClosed: true);
        return geometry;

        void Corner(Point point, double corner)
        {
            if (corner != 0)
            {
                path.ArcTo(point, new Size(corner, corner), 0, false, SweepDirection.Clockwise);
            }
        }
    }
}
