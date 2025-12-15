using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public class ImageGroupPreviewer : AbstractImagePreviewer
{
    #region 公共属性定义
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel
        {
            Orientation = Orientation.Horizontal
        });

    public static readonly StyledProperty<ITemplate<Panel?>> ItemsPanelProperty =
        AvaloniaProperty.Register<ImageGroupPreviewer, ITemplate<Panel?>>(nameof(ItemsPanel), DefaultPanel);

    public ITemplate<Panel?> ItemsPanel
    {
        get => GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }
    #endregion
    
    private ItemsControl? _itemsControl;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<ItemsControl>(ImagePreviewerThemeConstants.CoverItemsControlPart);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_itemsControl != null)
        {
            var count        = _itemsControl.Items.Count;
            var currentIndex = 0;
            for (var i = 0; i < count; i++)
            {
                var cover = _itemsControl.ContainerFromIndex(i);
                if (cover != null)
                {
                    var offset = cover.TranslatePoint(new Point(0, 0), this) ?? default;
                    var bounds = new Rect(offset, cover.Bounds.Size);
                    if (bounds.Contains(e.GetPosition(this)))
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }
            SetCurrentValue(CurrentIndexProperty, currentIndex);
            OpenDialog();
        }
    }
}