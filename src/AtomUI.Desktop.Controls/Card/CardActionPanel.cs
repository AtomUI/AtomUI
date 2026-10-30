using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class CardActionPanel : TemplatedControl
{
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardActionPanel>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public Avalonia.Controls.Controls Actions { get; } = new ();

    private UniformGrid? _uniformGrid;
    private IPen? _borderPen;
    
    static CardActionPanel()
    {
        AffectsRender<CardActionPanel>(
            BorderBrushProperty,
            BorderThicknessProperty,
            UseLayoutRoundingProperty);
    }

    public CardActionPanel()
    {
        Actions.CollectionChanged += HandleActionsChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_uniformGrid != null)
        {
            _uniformGrid.Children.Clear();
        }

        _uniformGrid = e.NameScope.Find<UniformGrid>("PART_GridPanel");
        if (_uniformGrid != null)
        {
            _uniformGrid.SetCurrentValue(UniformGrid.ColumnsProperty, Actions.Count);
            foreach (var action in Actions)
            {
                _uniformGrid.Children.Add(action);
            }
        }
    }

    private void HandleActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_uniformGrid != null)
        {
            _uniformGrid.SetCurrentValue(UniformGrid.ColumnsProperty, Actions.Count);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ControlCollectionChangedUtils.InsertControls(_uniformGrid.Children, e.NewStartingIndex, e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _uniformGrid.Children.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int index1 = 0; index1 < e.OldItems!.Count; ++index1)
                    {
                        var oldItem = e.OldItems![index1];
                        int     index2  = index1 + e.OldStartingIndex;
                        Control newItem = (Control) e.NewItems![index1]!;
                        _uniformGrid.Children[index2] = newItem;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    _uniformGrid.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        var lineWidth = BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness.Left);
        PenUtils.TryModifyOrCreate(ref _borderPen, BorderBrush, lineWidth);
        if (_borderPen is null)
        {
            return;
        }
        {
            var startPoint = new Point(0, lineWidth / 2); 
            var endPoint   = new Point(Bounds.Width, lineWidth / 2);
            context.DrawLine(_borderPen, startPoint, endPoint);
        }
        for (var i = 0; i < Actions.Count - 1; i++)
        {
            var action     = Actions[i];
            var offsetX    = action.Bounds.TopRight.X;
            var offsetY    = lineWidth / 2;
            var deltaY     = action.Bounds.Height * 0.2;
            var startPoint = new Point(offsetX, offsetY + deltaY); 
            var endPoint   = new Point(offsetX, offsetY + action.Bounds.Height - deltaY);
            
            context.DrawLine(_borderPen, startPoint, endPoint);
        }
    }

}
