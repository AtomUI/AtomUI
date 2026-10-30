using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using AtomUI.Utils;
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

    #region 渲染缓存

    // 两种旋转矩阵（正向 / 镜像），仅在 glyph 属性或尺寸变化时重建
    private Matrix _normalRotationMatrix;
    private Matrix _mirrorRotationMatrix;
    private Size   _cachedGlyphSize;
    private Size   _cachedTargetSize;
    private bool   _matrixCacheValid;
    private IDisposable? _glyphResourceAttachment;

    private void RebuildMatrixCache(Size glyphSize, Size targetSize)
    {
        var angleRad           = Glyph!.Rotate * Math.PI / 180;
        _normalRotationMatrix  = MatrixUtils.CreateRotationRadians(angleRad, glyphSize.Width / 2, glyphSize.Height / 2);
        _mirrorRotationMatrix  = MatrixUtils.CreateRotationRadians(-angleRad, glyphSize.Width / 2, glyphSize.Height / 2);
        _cachedGlyphSize       = glyphSize;
        _cachedTargetSize      = targetSize;
        _matrixCacheValid      = true;
    }

    #endregion

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
        _matrixCacheValid = false;
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

        var size = Glyph.GetDesiredSize();
        var columnStep = size.Width + Glyph.HorizontalSpace;
        var rowStep = size.Height + Glyph.VerticalSpace;
        var horizontalOffset = Glyph.HorizontalOffset;
        var verticalOffset = Glyph.VerticalOffset;
        // 负间距允许叠印，但实际步长必须有限且严格前进；非有限 Glyph 尺寸
        // （例如尚未有有效尺寸的图片）也不能进入绘制或旋转矩阵计算。
        if (!double.IsFinite(size.Width) || size.Width <= 0 ||
            !double.IsFinite(size.Height) || size.Height <= 0 ||
            !double.IsFinite(columnStep) || columnStep <= 0 ||
            !double.IsFinite(rowStep) || rowStep <= 0 ||
            !double.IsFinite(horizontalOffset) || !double.IsFinite(verticalOffset) ||
            !double.IsFinite(Glyph.Rotate))
        {
            return;
        }

        var targetSize = Target.Bounds.Size;

        // Rebuild matrix cache when invalidated or sizes changed
        if (!_matrixCacheValid || _cachedGlyphSize != size || _cachedTargetSize != targetSize)
        {
            RebuildMatrixCache(size, targetSize);
        }

        // 只为具有已知绘制边界的内置实现跳过不可见 tile。自定义 Glyph/子类
        // 可能绘制到 DesiredSize 之外，保留其既有 Render 契约。
        Rect? drawingBounds = Glyph.GetType() == typeof(TextGlyph)
            ? ((TextGlyph)Glyph).GetDrawingBounds()
            : Glyph.GetType() == typeof(ImageGlyph) ? new Rect(size) : null;
        if (drawingBounds is null && (horizontalOffset + columnStep <= horizontalOffset ||
                                     verticalOffset + rowStep <= verticalOffset))
        {
            return;
        }

        using (context.PushClip(new Rect(targetSize)))
        using (context.PushOpacity(Glyph.Opacity))
        {
            var tileBounds = drawingBounds?.TransformToAABB(_normalRotationMatrix);
            if (drawingBounds is { } bounds && Glyph.IsMirrorUsed)
            {
                tileBounds = tileBounds!.Value.Union(bounds.TransformToAABB(_mirrorRotationMatrix));
            }
            var (t, isOddRow) = tileBounds is { } rowBounds
                ? FirstVisibleTile(verticalOffset, rowStep, -rowBounds.Bottom)
                : (verticalOffset, false);
            while (t < targetSize.Height)
            {
                var pushState = new DrawingContext.PushedState();
                if (isOddRow && Glyph.IsCrossUsed)
                {
                    pushState = context.PushTransform(
                        Matrix.CreateTranslation(columnStep / 2, 0));
                }

                using (pushState)
                {
                    var crossOffset = isOddRow && Glyph.IsCrossUsed ? columnStep / 2 : 0;
                    var (l, isOddColumn) = tileBounds is { } columnBounds
                        ? FirstVisibleTile(horizontalOffset, columnStep, -columnBounds.Right - crossOffset)
                        : (horizontalOffset, false);
                    while (l < targetSize.Width)
                    {
                        // Use pre-cached rotation matrix — no trig/allocation per tile
                        var m = isOddColumn && Glyph.IsMirrorUsed
                            ? _mirrorRotationMatrix
                            : _normalRotationMatrix;

                        using (context.PushTransform(Matrix.CreateTranslation(l, t)))
                        using (context.PushTransform(m))
                        {
                            Glyph.Render(context);
                        }

                        var nextColumn = l + columnStep;
                        if (nextColumn <= l)
                        {
                            break;
                        }
                        l = nextColumn;
                        isOddColumn = !isOddColumn;
                    }

                    var nextRow = t + rowStep;
                    if (nextRow <= t)
                    {
                        break;
                    }
                    t = nextRow;
                    isOddRow = !isOddRow;
                }
            }
        }
    }

    private static (double Position, bool IsOdd) FirstVisibleTile(double offset, double step, double boundary)
    {
        var period = step * 2;
        if (offset >= boundary || !double.IsFinite(period) || !double.IsFinite(boundary))
        {
            return (offset, false);
        }

        // 分别取余，避免巨大商乘回 step 与 offset 相消，并保持隔行/隔列相位。
        var phase = (offset % period - boundary % period) % period;
        if (phase < 0)
        {
            phase += period;
        }
        var isOdd = phase >= step;
        if (isOdd)
        {
            phase -= step;
        }
        return (boundary + phase, isOdd);
    }
}
