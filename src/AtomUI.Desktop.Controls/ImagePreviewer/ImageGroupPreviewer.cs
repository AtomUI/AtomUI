using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows a group of images as covers laid out by <see cref="ItemsPanel" /> and opens the preview on
/// the cover that was clicked.
/// </summary>
public partial class ImageGroupPreviewer : AbstractImagePreviewer
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ItemsPanel" /> property.
    /// </summary>
    public static readonly StyledProperty<ITemplate<Panel?>> ItemsPanelProperty =
        AvaloniaProperty.Register<ImageGroupPreviewer, ITemplate<Panel?>>(
            nameof(ItemsPanel),
            new FuncTemplate<Panel?>(() => new StackPanel
            {
                Orientation = Orientation.Horizontal
            }));

    /// <summary>
    /// Template that creates the panel laying out the covers of the group. The default is a horizontal
    /// <c>StackPanel</c>.
    /// </summary>
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

    /// <summary>
    /// Requests the thumbnails of the items for the closed state when <c>EffectiveItems</c> changes and
    /// the preview is not open. The base implementation is called first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == EffectiveItemsProperty && !IsOpen)
        {
            RequestClosedStateLoads();
        }
    }

    /// <summary>
    /// Resolves the cover items control from the template and requests the thumbnails of the items for
    /// the closed state when the preview is not open. The base implementation is called first. Without
    /// the <c>PART_CoverItemsControl</c> part, clicking the group does not open the preview.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<ItemsControl>("PART_CoverItemsControl");
        if (!IsOpen)
        {
            RequestClosedStateLoads();
        }
    }

    /// <summary>
    /// Sets <c>CurrentIndex</c> to the cover under the pointer and opens the preview. When the pointer
    /// is released outside every cover, the index falls back to 0. It does nothing when the template has
    /// no cover items control, and it never calls the base implementation.
    /// </summary>
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

    /// <summary>
    /// Requests the thumbnail of every entry at high priority, decoded for the cover size. That size is
    /// <see cref="AbstractImagePreviewer.CoverWidth" /> and <see cref="AbstractImagePreviewer.CoverHeight" />
    /// scaled by the render scaling of the top level and rounded up to a multiple of 16 device pixels.
    /// It requests nothing when there is no entry or when both sizes are zero.
    /// </summary>
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
