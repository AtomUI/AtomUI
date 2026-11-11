using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

public sealed class Watermark : Control
{
    public static WatermarkGlyph? GetGlyph(Layoutable element)
    {
        return element.GetValue(GlyphProperty);
    }

    public static void SetGlyph(Layoutable element, WatermarkGlyph? value)
    {
        element.SetValue(GlyphProperty, value);
    }

    public static readonly AttachedProperty<WatermarkGlyph?> GlyphProperty = AvaloniaProperty
        .RegisterAttached<Watermark, Layoutable, WatermarkGlyph?>("Glyph");

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Glyph != null)
        {
            // 解析动态资源会同步发布 Glyph 属性变化，应用回调可能立即移除水印。
            // 先保存可释放的生命周期槽，退出发生在获取 token 期间时也能释放返回的 token。
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
