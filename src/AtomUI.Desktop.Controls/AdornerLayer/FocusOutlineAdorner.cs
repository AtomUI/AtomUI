using AtomUI.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Keyboard focus outline matching Ant Design <c>genFocusOutline</c>: a stroke of
/// <see cref="OutlineThickness"/> drawn <see cref="OutlineOffset"/> outside the adorned element and
/// following its corner radius.
/// </summary>
internal sealed class FocusOutlineAdorner : Control
{
    /// <summary>
    /// Set on the adorned element to draw the outline just inside its bounds instead of outside
    /// (Ant Design <c>genFocusOutline(token, -lineWidthFocus)</c>, used by card tabs).
    /// </summary>
    public static readonly AttachedProperty<bool> IsInsetProperty =
        AvaloniaProperty.RegisterAttached<FocusOutlineAdorner, Visual, bool>("IsInset");

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, IBrush?>(nameof(OutlineBrush));

    public static readonly StyledProperty<double> OutlineThicknessProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, double>(nameof(OutlineThickness));

    public static readonly StyledProperty<double> OutlineOffsetProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, double>(nameof(OutlineOffset));

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public double OutlineThickness
    {
        get => GetValue(OutlineThicknessProperty);
        set => SetValue(OutlineThicknessProperty, value);
    }

    public double OutlineOffset
    {
        get => GetValue(OutlineOffsetProperty);
        set => SetValue(OutlineOffsetProperty, value);
    }

    public static bool GetIsInset(Visual element) => element.GetValue(IsInsetProperty);

    public static void SetIsInset(Visual element, bool value) => element.SetValue(IsInsetProperty, value);

    private Pen? _pen;
    private CornerRadius _renderedCornerRadius;
    // The adorned element and, when it borrows its shape from its owner, the owner: shape changes that keep
    // the size (an expander header squaring its bottom corners) re-arrange nothing, so they are watched here.
    private AvaloniaObject? _shapeSource;
    private AvaloniaObject? _shapeOwner;

    private const double ClipEdgeTolerance = 0.5;

    static FocusOutlineAdorner()
    {
        AffectsRender<FocusOutlineAdorner>(OutlineBrushProperty, OutlineThicknessProperty, OutlineOffsetProperty,
            AdornerLayer.AdornedElementProperty);
        IsHitTestVisibleProperty.OverrideDefaultValue<FocusOutlineAdorner>(false);
    }

    public FocusOutlineAdorner()
    {
        // The outline sits outside the adorned element, so AdornerLayer clipping, which starts at the element's
        // own bounds (TemplatedControl clips by default), would remove it; Render applies the ancestor clips.
        AdornerLayer.SetIsClipEnabled(this, false);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeShapeSources();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        UnsubscribeShapeSources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AdornerLayer.AdornedElementProperty)
        {
            UnsubscribeShapeSources();
            if (this.IsAttachedToVisualTree())
            {
                SubscribeShapeSources();
            }
        }
        else if (change.Property == ParentProperty && change.NewValue is null &&
                 this.GetVisualParent() is AdornerLayer)
        {
            // Removed from AdornerLayer.Children (Control.OnLostFocus does this): Panel drops the logical child
            // before the visual one. AdornerLayer releases its subscriptions on the adorned element and every
            // ancestor only when the adorned element changes while the adorner is still its visual child.
            AdornerLayer.SetAdornedElement(this, null);
        }
    }

    public override void Render(DrawingContext context)
    {
        var thickness = OutlineThickness;
        var adorned   = AdornerLayer.GetAdornedElement(this);
        if (OutlineBrush is not { } brush || thickness <= 0 || adorned is null)
        {
            return;
        }

        if (_pen is null || !ReferenceEquals(_pen.Brush, brush) || !MathUtils.AreClose(_pen.Thickness, thickness))
        {
            _pen = new Pen(brush, thickness);
        }

        // The pen is centered on the path, so the path runs half a stroke further out than the offset.
        var offset       = GetIsInset(adorned) ? -thickness : OutlineOffset;
        var inflate      = offset + thickness / 2;
        var cornerRadius = ResolveCornerRadius(adorned);
        var elementRect  = new Rect(Bounds.Size);
        var rect         = elementRect.Inflate(inflate);
        _renderedCornerRadius = cornerRadius;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        var clip = CalculateAncestorClip(adorned, elementRect, rect.Inflate(thickness / 2));
        if (clip is { Width: <= 0 } or { Height: <= 0 })
        {
            return;
        }

        using var clipState = clip is { } clipRect ? context.PushClip(clipRect) : default(DrawingContext.PushedState?);
        var maxRadius = Math.Min(rect.Width, rect.Height) / 2;
        context.DrawRectangle(null, _pen, new RoundedRect(rect,
            Grow(cornerRadius.TopLeft, inflate, maxRadius),
            Grow(cornerRadius.TopRight, inflate, maxRadius),
            Grow(cornerRadius.BottomRight, inflate, maxRadius),
            Grow(cornerRadius.BottomLeft, inflate, maxRadius)));
    }

    // The clips of the adorned element's ancestors in its own coordinates (what AdornerLayer clipping applies,
    // minus the element's own clip), except that on a side where an ancestor does not cut the element the
    // outline may pass that ancestor's edge: wrappers that hug the element keep the outline, while a viewport
    // the element is scrolled out of hides it. Evaluated on each render, which AdornerLayer triggers whenever
    // the element or an ancestor moves or resizes (it re-arranges the adorner and replaces its transform).
    private static Rect? CalculateAncestorClip(Visual adorned, Rect element, Rect outline)
    {
        Rect? result = null;
        for (var ancestor = adorned.GetVisualParent(); ancestor is not null; ancestor = ancestor.GetVisualParent())
        {
            var geometry = ancestor.Clip;
            if (!ancestor.ClipToBounds && geometry is null)
            {
                continue;
            }

            var clip = ancestor.ClipToBounds ? new Rect(ancestor.Bounds.Size) : geometry!.Bounds;
            if (ancestor.ClipToBounds && geometry is not null)
            {
                clip = clip.Intersect(geometry.Bounds);
            }

            if (ancestor.TransformToVisual(adorned) is not { } transform)
            {
                continue;
            }

            clip = clip.TransformToAABB(transform);
            var left   = element.Left >= clip.Left - ClipEdgeTolerance ? Math.Min(clip.Left, outline.Left) : clip.Left;
            var top    = element.Top >= clip.Top - ClipEdgeTolerance ? Math.Min(clip.Top, outline.Top) : clip.Top;
            var right  = element.Right <= clip.Right + ClipEdgeTolerance ? Math.Max(clip.Right, outline.Right) : clip.Right;
            var bottom = element.Bottom <= clip.Bottom + ClipEdgeTolerance ? Math.Max(clip.Bottom, outline.Bottom) : clip.Bottom;
            if (right <= left || bottom <= top)
            {
                return default(Rect);
            }

            var sideClip = new Rect(left, top, right - left, bottom - top);
            var combined = result is { } current ? current.Intersect(sideClip) : sideClip;
            if (combined.Width <= 0 || combined.Height <= 0)
            {
                return default(Rect);
            }

            result = combined;
        }

        return result;
    }

    private void SubscribeShapeSources()
    {
        if (AdornerLayer.GetAdornedElement(this) is not { } adorned)
        {
            return;
        }

        _shapeSource                 =  adorned;
        _shapeSource.PropertyChanged += HandleShapeSourcePropertyChanged;
        if (adorned is not IFocusOutlineShapeProvider &&
            adorned is Control { TemplatedParent: IFocusOutlineShapeProvider and AvaloniaObject owner })
        {
            _shapeOwner                 =  owner;
            _shapeOwner.PropertyChanged += HandleShapeSourcePropertyChanged;
        }
    }

    private void UnsubscribeShapeSources()
    {
        if (_shapeSource is not null)
        {
            _shapeSource.PropertyChanged -= HandleShapeSourcePropertyChanged;
            _shapeSource                 =  null;
        }

        if (_shapeOwner is not null)
        {
            _shapeOwner.PropertyChanged -= HandleShapeSourcePropertyChanged;
            _shapeOwner                 =  null;
        }
    }

    private void HandleShapeSourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (AdornerLayer.GetAdornedElement(this) is { } adorned &&
            ResolveCornerRadius(adorned) != _renderedCornerRadius)
        {
            InvalidateVisual();
        }
    }

    // Shapes such as circle buttons report a radius larger than half their size; clamp so the outline
    // stays a circle instead of relying on the renderer. An inset outline shrinks the radius, never below 0.
    private static double Grow(double radius, double inflate, double maxRadius) =>
        radius > 0 ? Math.Clamp(radius + inflate, 0, maxRadius) : 0;

    private static CornerRadius ResolveCornerRadius(Visual? adorned)
    {
        return adorned switch
        {
            IFocusOutlineShapeProvider provider => provider.FocusOutlineCornerRadius,
            // A template focus target whose visible shape comes from its owner, such as a header clipped by
            // the owner's rounded frame.
            Control { TemplatedParent: IFocusOutlineShapeProvider provider } => provider.FocusOutlineCornerRadius,
            TemplatedControl templatedControl => templatedControl.CornerRadius,
            Border border => border.CornerRadius,
            DashedBorder dashedBorder => dashedBorder.CornerRadius,
            _ => default
        };
    }
}
