using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Adorner that repeats a <see cref="WatermarkGlyph" /> over the bounds of an element. It is created and
/// installed by <see cref="SetGlyph" />, repaints whenever a property of the glyph changes, and lets pointer
/// input pass through to the element.
/// </summary>
public sealed class Watermark : Control
{
    /// <summary>
    /// Gets the glyph that <paramref name="element" /> shows as a watermark, or <see langword="null" /> when it
    /// shows none.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    public static WatermarkGlyph? GetGlyph(Layoutable element)
    {
        return element.GetValue(GlyphProperty);
    }

    /// <summary>
    /// Tiles <paramref name="value" /> as a watermark over the bounds of <paramref name="element" />, once the
    /// element has been arranged. The value applies to that element alone, not to its children. An element
    /// that already carries a different adorner shows no watermark, and neither does an element with no
    /// scope-aware adorner layer in its scope; that case is written to the trace log.
    /// </summary>
    /// <param name="element">
    /// The element to show the watermark on.
    /// </param>
    /// <param name="value">
    /// The glyph to tile, or <see langword="null" /> to remove the watermark.
    /// </param>
    public static void SetGlyph(Layoutable element, WatermarkGlyph? value)
    {
        element.SetValue(GlyphProperty, value);
    }

    /// <summary>
    /// Identifies the <c>Glyph</c> attached property that <see cref="SetGlyph" /> describes.
    /// </summary>
    public static readonly AttachedProperty<WatermarkGlyph?> GlyphProperty = AvaloniaProperty
        .RegisterAttached<Watermark, Layoutable, WatermarkGlyph?>("Glyph");

    /// <summary>
    /// The element that carries this adorner. Its bounds are the area the watermark is tiled over.
    /// </summary>
    public Layoutable Target { get; }

    private WatermarkGlyph? Glyph { get; }

    static Watermark()
    {
        IsHitTestVisibleProperty.OverrideMetadata<Watermark>(new StyledPropertyMetadata<bool>(false));
        GlyphProperty.Changed.AddClassHandler<Layoutable>(OnGlyphChanged);
    }

    private Watermark(Layoutable target, WatermarkGlyph? glyph)
    {
        Target = target;
        Glyph  = glyph;
    }

    private readonly WatermarkTileRenderer _renderer = new();
    private IDisposable? _glyphResourceAttachment;

    /// <summary>
    /// Calls the base member first, then attaches the glyph to the resource scope of <see cref="Target" /> and
    /// makes the watermark repaint on every change of a glyph property.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Glyph != null)
        {
            // Resolving a dynamic resource publishes glyph property changes synchronously, and a callback that runs
            // during that may remove the watermark at once. The disposable lifetime slot is stored first, so a detach
            // while the token is being acquired still releases the token the call returns.
            var attachment = new SingleAssignmentDisposable();
            _glyphResourceAttachment = attachment;
            Glyph.PropertyChanged += HandleGlyphPropertyChanged;
            attachment.Disposable = Glyph.AttachToResourceOwner(Target);
        }
    }

    private void HandleGlyphPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        InvalidateVisual();
    }

    private static void OnGlyphChanged(Layoutable target, AvaloniaPropertyChangedEventArgs arg)
    {
        target.LayoutUpdated -= HandleTargetLayoutUpdated;
        if (GetGlyph(target) is null)
        {
            UnInstallWatermark(target);
            return;
        }

        if (target.IsArrangeValid)
        {
            InstallWatermark(target);
            return;
        }

        target.LayoutUpdated += HandleTargetLayoutUpdated;
    }

    private static void HandleTargetLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Layoutable target)
        {
            return;
        }
        target.LayoutUpdated -= HandleTargetLayoutUpdated;
        if (GetGlyph(target) is null)
        {
            UnInstallWatermark(target);
            return;
        }

        InstallWatermark(target);
    }

    /// <summary>
    /// Calls the base member first, then stops listening to the glyph and releases its resource attachment, so
    /// the glyph no longer resolves resources in the scope of <see cref="Target" />.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (Glyph != null)
        {
            Glyph.PropertyChanged -= HandleGlyphPropertyChanged;
        }
        _glyphResourceAttachment?.Dispose();
        _glyphResourceAttachment = null;
    }

    private static void InstallWatermark(Layoutable target)
    {
        var glyph = GetGlyph(target);
        if (glyph is null)
        {
            UnInstallWatermark(target);
            return;
        }

        if (CheckLayer(target, out _) == false)
        {
            return;
        }

        var currentAdorner = ScopeAwareAdornerLayer.GetAdorner(target);
        if (currentAdorner is Watermark watermark && ReferenceEquals(watermark.Glyph, glyph))
        {
            return;
        }

        if (currentAdorner is not null and not Watermark)
        {
            return;
        }

        ScopeAwareAdornerLayer.SetAdorner(target, new Watermark(target, glyph));
    }

    private static void UnInstallWatermark(Layoutable target)
    {
        target.LayoutUpdated -= HandleTargetLayoutUpdated;
        if (ScopeAwareAdornerLayer.GetAdorner(target) is not Watermark)
        {
            return;
        }

        ScopeAwareAdornerLayer.SetAdorner(target, null);
    }

    private static bool CheckLayer(Visual target, [NotNullWhen(true)] out ScopeAwareAdornerLayer? layer)
    {
        layer = ScopeAwareAdornerLayer.GetLayer(target);
        if (layer == null)
        {
            Trace.WriteLine($"Can not get ScopeAwareAdornerLayer for {target} to show a watermark.");
        }

        return layer != null;
    }

    /// <summary>
    /// Calls the base member, then tiles the glyph over an area the size of the bounds of <see cref="Target" />,
    /// clipped to that area.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Glyph == null)
        {
            return;
        }

        // The cell is the glyph's own size here, so the glyph fills it exactly.
        _renderer.Render(context, Target.Bounds.Size, Glyph.GetDesiredSize(), Glyph);
    }
}
