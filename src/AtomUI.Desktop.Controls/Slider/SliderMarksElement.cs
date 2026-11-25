using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 承载 mark 点与 mark 标签绘制的内部子元素。它只负责渲染，几何与文本数据由 <see cref="SliderTrack"/>
/// 在布局与状态变化时推入，使 rail → tracks → track → mark → thumb 的视觉层级在元素化后保持不变。
/// </summary>
internal class SliderMarksElement : Control
{
    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> MarkBorderBrushProperty =
        AvaloniaProperty.Register<SliderMarksElement, IBrush?>(nameof(MarkBorderBrush));

    internal static readonly StyledProperty<IBrush?> MarkBorderActiveBrushProperty =
        AvaloniaProperty.Register<SliderMarksElement, IBrush?>(nameof(MarkBorderActiveBrush));

    internal IBrush? MarkBorderBrush
    {
        get => GetValue(MarkBorderBrushProperty);
        set => SetValue(MarkBorderBrushProperty, value);
    }

    internal IBrush? MarkBorderActiveBrush
    {
        get => GetValue(MarkBorderActiveBrushProperty);
        set => SetValue(MarkBorderActiveBrushProperty, value);
    }

    #endregion

    internal List<(Rect, int, bool)>? MarkRects { get; set; }

    internal List<(Rect, int, bool, FormattedText)>? MarkTextRects { get; set; }

    private IPen? _markBorderPen;
    private IPen? _markBorderActivePen;
    private SliderMarkPaint _staticPaint;

    static SliderMarksElement()
    {
        AffectsRender<SliderMarksElement>(MarkBorderBrushProperty, MarkBorderActiveBrushProperty);
    }

    internal void UpdateStaticPaint(SliderMarkPaint paint)
    {
        if (_staticPaint != paint)
        {
            _staticPaint = paint;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        if (MarkRects is not null)
        {
            foreach (var markRectEntry in MarkRects)
            {
                var centerPos = markRectEntry.Item1.Center;
                var radius   = _staticPaint.Size / 2;
                if (markRectEntry.Item3)
                {
                    PenUtils.TryModifyOrCreate(ref _markBorderActivePen,
                        MarkBorderActiveBrush,
                        _staticPaint.BorderThickness.Left);
                    context.DrawEllipse(_staticPaint.Background, _markBorderActivePen, centerPos, radius, radius);
                }
                else
                {
                    PenUtils.TryModifyOrCreate(ref _markBorderPen,
                        MarkBorderBrush,
                        _staticPaint.BorderThickness.Left);
                    context.DrawEllipse(_staticPaint.Background, _markBorderPen, centerPos, radius, radius);
                }
            }
        }

        if (MarkTextRects is not null)
        {
            foreach (var markTextRectEntry in MarkTextRects)
            {
                context.DrawText(markTextRectEntry.Item4, markTextRectEntry.Item1.Position);
            }
        }
    }
}

internal readonly record struct SliderMarkPaint(IBrush? Background, Thickness BorderThickness, double Size);
