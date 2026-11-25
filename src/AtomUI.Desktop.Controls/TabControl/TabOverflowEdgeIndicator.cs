using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

namespace AtomUI.Desktop.Controls;

internal sealed class TabOverflowEdgeIndicator : Control
{
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<TabOverflowEdgeIndicator>();

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    static TabOverflowEdgeIndicator()
    {
        AffectsRender<TabOverflowEdgeIndicator>(BoxShadowProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (BoxShadow.Count > 0)
        {
            var caster = new Rect(Bounds.Size);
            if (TemplatedParent is TabScrollViewer viewer)
            {
                // Negative spread shrinks both axes. Extend the caster across the clipped axis so
                // the visible shadow runs to the viewport corners like the inset shadow in Ant Design.
                var horizontal = viewer.TabStripPlacement is Dock.Top or Dock.Bottom;
                var extension = 0d;
                for (var i = 0; i < BoxShadow.Count; i++)
                {
                    var shadow = BoxShadow[i];
                    var crossOffset = horizontal ? shadow.OffsetY : shadow.OffsetX;
                    extension = Math.Max(extension, Math.Abs(crossOffset) + shadow.Blur + Math.Max(0, -shadow.Spread));
                }
                caster = caster.Inflate(horizontal ? new Thickness(0, extension) : new Thickness(extension, 0));
            }
            context.Custom(new ShadowDrawOperation(caster, BoxShadow));
        }
    }

    private sealed class ShadowDrawOperation(Rect caster, BoxShadows shadows) : ICustomDrawOperation
    {
        // The caster is outside the viewport; its shadow is the visible content.
        public Rect Bounds { get; } = shadows.TransformBounds(caster);

        public bool HitTest(Point point) => false;

        public bool Equals(ICustomDrawOperation? other) =>
            other is ShadowDrawOperation operation &&
            operation.Caster == caster && operation.Shadows == shadows;

        private Rect Caster => caster;
        private BoxShadows Shadows => shadows;

        public void Render(ImmediateDrawingContext context)
        {
            var effectiveShadows = shadows;
            if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is not null)
            {
                // Avalonia's current Skia backend appends shadow translation AFTER
                // the drawing transform, unlike blur/spread. Convert only the offset
                // vector to that coordinate space; exclude the matrix's translation.
                // Use the actual render transform, not a captured TopLevel DPI, so
                // changing displays, ancestor transforms and bitmap DPI remain valid.
                // TODO(Avalonia upgrade): remove this compensation when Skia applies
                // shadow offsets before the transform. Keep the DPI pixel regressions.
                var transform = context.PlatformImpl.Transform;
                var first = TransformOffset(shadows[0], transform);
                if (shadows.Count == 1)
                {
                    effectiveShadows = new BoxShadows(first);
                }
                else
                {
                    var rest = new BoxShadow[shadows.Count - 1];
                    for (var i = 1; i < shadows.Count; i++)
                    {
                        rest[i - 1] = TransformOffset(shadows[i], transform);
                    }
                    effectiveShadows = new BoxShadows(first, rest);
                }
            }
            context.DrawRectangle(Brushes.Transparent, null, caster, boxShadows: effectiveShadows);
        }

        private static BoxShadow TransformOffset(BoxShadow shadow, Matrix transform)
        {
            var x = shadow.OffsetX;
            var y = shadow.OffsetY;
            shadow.OffsetX = x * transform.M11 + y * transform.M21;
            shadow.OffsetY = x * transform.M12 + y * transform.M22;
            return shadow;
        }

        public void Dispose()
        {
        }
    }
}
