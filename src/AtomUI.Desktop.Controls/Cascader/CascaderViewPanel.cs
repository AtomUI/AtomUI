using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewFrame : Decorator
{
    public static readonly StyledProperty<IBrush?> BorderBrushProperty = 
        Border.BorderBrushProperty.AddOwner<CascaderViewFrame>();
    
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }
    
    private StackPanel? _itemsPanel;

    static CascaderViewFrame()
    {
        AffectsMeasure<CascaderViewFrame>(BorderBrushProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ChildProperty)
        {
            _itemsPanel = Child as StackPanel;
        }
    }

    public override void Render(DrawingContext context)
    {
        if (_itemsPanel == null)
        {
            return;
        }
        using var state = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });
        var count  = _itemsPanel.Children.Count;
        var height = DesiredSize.Height;
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                var child  = _itemsPanel.Children[i];
                var offset = child.TranslatePoint(new Point(0, 0), this);
                if (offset != null)
                {
                    var pointStart = new Point(offset.Value.X, 0);
                    var pointEnd   = new Point(offset.Value.X, height);
                    context.DrawLine(new Pen(BorderBrush), pointStart, pointEnd);
                }
            }
        }
    }
}