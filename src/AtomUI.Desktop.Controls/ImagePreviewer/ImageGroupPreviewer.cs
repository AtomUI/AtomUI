using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public partial class ImageGroupPreviewer : AbstractImagePreviewer
{
    #region 公共属性定义

    public static readonly StyledProperty<ITemplate<Panel?>> ItemsPanelProperty =
        AvaloniaProperty.Register<ImageGroupPreviewer, ITemplate<Panel?>>(
            nameof(ItemsPanel),
            new FuncTemplate<Panel?>(() => new StackPanel
            {
                Orientation = Orientation.Horizontal
            }));

    public ITemplate<Panel?> ItemsPanel
    {
        get => GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }

    #endregion

    private ItemsControl? _itemsControl;

    public ImageGroupPreviewer()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == EffectiveItemsProperty && !IsOpen)
        {
            RequestClosedStateLoads();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<ItemsControl>("PART_CoverItemsControl");
        if (!IsOpen)
        {
            RequestClosedStateLoads();
        }
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

    private protected override void RequestClosedStateLoads()
    {
        if (EffectiveItems is not { Count: > 0 } effectiveItems)
        {
            return;
        }

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var width = Quantize(CoverWidth * scaling);
        var height = Quantize(CoverHeight * scaling);
        if (width == 0 && height == 0)
        {
            return;
        }
        foreach (var item in effectiveItems)
        {
            RequestThumbnailLoad(item, width, height, AtomUI.Controls.ImageRequestPriority.High);
        }
    }

    private static int Quantize(double value)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 0;
        }
        return checked((int)(Math.Ceiling(value / 16) * 16));
    }
}
