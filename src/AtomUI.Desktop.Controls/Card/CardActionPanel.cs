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
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardActionPanel>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public Avalonia.Controls.Controls Actions { get; } = new ();

    #endregion

    private UniformGrid? _uniformGrid;
    private IPen? _borderPen;
    private readonly List<Rect?> _renderedActionBounds = new();
    
    static CardActionPanel()
    {
        AffectsRender<CardActionPanel>(
            BorderBrushProperty,
            BorderThicknessProperty,
            UseLayoutRoundingProperty,
            PaddingProperty);
    }

    public CardActionPanel()
    {
        Actions.CollectionChanged += HandleActionsChanged;
        // Separators use caller-owned action positions, including child-only arrange changes.
        // ControlTheme cannot invalidate this custom drawing from those runtime positions.
        // Layoutable manages this self-owned event's layout-manager attach/detach subscription.
        LayoutUpdated += HandleLayoutUpdated;
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

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        var count = Math.Max(0, Actions.Count - 1);
        if (_renderedActionBounds.Count != count)
        {
            InvalidateVisual();
            return;
        }
        for (var i = 0; i < count; i++)
        {
            if (_renderedActionBounds[i] != GetActionBounds(i))
            {
                InvalidateVisual();
                return;
            }
        }
    }

    private Rect? GetActionBounds(int index)
    {
        var action = Actions[index];
        return action.TranslatePoint(default, this) is { } origin
            ? new Rect(origin, action.Bounds.Size)
            : null;
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
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        _renderedActionBounds.Clear();
        for (var i = 0; i < Actions.Count - 1; i++)
        {
            _renderedActionBounds.Add(GetActionBounds(i));
        }
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
        foreach (var bounds in _renderedActionBounds)
        {
            if (bounds is not { } actionBounds)
            {
                continue;
            }
            var offsetX = actionBounds.Right;
            var offsetY = actionBounds.Y + lineWidth / 2;
            var deltaY     = actionBounds.Height * 0.2;
            var startPoint = new Point(offsetX, offsetY + deltaY); 
            var endPoint   = new Point(offsetX, offsetY + actionBounds.Height - deltaY);
            
            context.DrawLine(_borderPen, startPoint, endPoint);
        }
    }

}
