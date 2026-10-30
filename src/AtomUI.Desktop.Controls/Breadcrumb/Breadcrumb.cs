using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public partial class Breadcrumb : ItemsControl, IMotionAwareControl
{
    #region 公共属性定义

    public const string DefaultSeparator = "/";

    public static readonly StyledProperty<object?> SeparatorProperty =
        AvaloniaProperty.Register<Breadcrumb, object?>(
            nameof(Separator),
            defaultValue: DefaultSeparator
        );
    
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        AvaloniaProperty.Register<Breadcrumb, IDataTemplate?>(nameof(SeparatorTemplate));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Breadcrumb>();

    [DependsOn(nameof(SeparatorTemplate))]
    public object? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }
    
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<BreadcrumbNavigateEventArgs>? NavigateRequest;

    #endregion

    #region 内部协作 API

    internal void NotifyNavigateRequest(BreadcrumbItem breadcrumbItem)
    {
        NavigateRequest?.Invoke(this, new BreadcrumbNavigateEventArgs(breadcrumbItem));
    }

    internal void AttachSeparatorLogicalChild(Control presenter)
    {
        LogicalChildren.Add(presenter);
    }

    internal void DetachSeparatorLogicalChild(Control presenter)
    {
        LogicalChildren.Remove(presenter);
    }

    #endregion

    private readonly BreadcrumbSeparatorManager _separatorManager;
    private BorderRenderHelper? _borderRenderHelper;

    static Breadcrumb()
    {
        AffectsMeasure<Breadcrumb>(BorderThicknessProperty, PaddingProperty);
        AffectsRender<Breadcrumb>(BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty);
    }

    public Breadcrumb()
    {
        _separatorManager = new BreadcrumbSeparatorManager(this);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var breadcrumbItem = new BreadcrumbItem();
        breadcrumbItem.Classes.Add(BreadcrumbSemanticParts.ItemClass);
        return breadcrumbItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<BreadcrumbItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is BreadcrumbItem breadcrumbItem)
        {
            breadcrumbItem.Classes.Add(BreadcrumbSemanticParts.ItemClass);

            if (item != null && item is not Visual)
            {
                if (item is IBreadcrumbItemData breadcrumbItemData)
                {
                    ApplyItemDataContent(breadcrumbItem, breadcrumbItemData);
                    ApplyItemData(breadcrumbItem, breadcrumbItemData);
                }
                else if (!breadcrumbItem.IsSet(BreadcrumbItem.ContentProperty))
                {
                    breadcrumbItem.SetCurrentValue(BreadcrumbItem.ContentProperty, item);
                }
            }
            
            if (ItemTemplate != null)
            {
                breadcrumbItem[!BreadcrumbItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            breadcrumbItem[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            PrepareBreadcrumbItem(breadcrumbItem, item, index);
            if (!Equals(Separator, DefaultSeparator) ||
                SeparatorTemplate is not null)
            {
                ConfigureItemSeparator(breadcrumbItem);
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type BreadcrumbItem.");
        }
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        UpdateItemStates();
    }
    
    protected virtual void PrepareBreadcrumbItem(BreadcrumbItem breadcrumbItem, object? item, int index)
    {
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        UpdateItemStates();
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is BreadcrumbItem breadcrumbItem)
        {
            breadcrumbItem.IsLast = false;
            if (!ReferenceEquals(ItemFromContainer(breadcrumbItem), breadcrumbItem))
            {
                ClearGeneratedItemValues(breadcrumbItem);
            }
        }

        base.ClearContainerForItemOverride(container);
        UpdateItemStates();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SeparatorProperty ||
            change.Property == SeparatorTemplateProperty)
        {
            foreach (Control container in GetRealizedContainers())
            {
                if (container is BreadcrumbItem breadcrumbItem)
                {
                    ConfigureItemSeparator(breadcrumbItem);
                }
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _separatorManager.OnTemplateApplied();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _separatorManager.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _separatorManager.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void ConfigureItemSeparator(BreadcrumbItem breadcrumbItem)
    {
        breadcrumbItem.SetValue(BreadcrumbItem.SeparatorProperty, Separator, BindingPriority.Style);
        breadcrumbItem.SetValue(BreadcrumbItem.SeparatorTemplateProperty, SeparatorTemplate, BindingPriority.Style);
    }

    private void ApplyItemDataContent(BreadcrumbItem breadcrumbItem, IBreadcrumbItemData breadcrumbItemData)
    {
        if (ItemTemplate is null)
        {
            breadcrumbItem.SetCurrentValue(BreadcrumbItem.ContentProperty, breadcrumbItemData.Content);
        }
    }

    private static void ApplyItemData(BreadcrumbItem breadcrumbItem, IBreadcrumbItemData breadcrumbItemData)
    {
        if (!breadcrumbItem.IsSet(BreadcrumbItem.IconProperty))
        {
            breadcrumbItem.SetCurrentValue(BreadcrumbItem.IconProperty, breadcrumbItemData.Icon);
        }

        if (breadcrumbItemData.Separator != null)
        {
            breadcrumbItem.SetValue(BreadcrumbItem.SeparatorProperty, breadcrumbItemData.Separator);
        }
        else
        {
            breadcrumbItem.ClearValue(BreadcrumbItem.SeparatorProperty);
        }

        if (breadcrumbItemData.SeparatorTemplate != null)
        {
            breadcrumbItem.SetValue(BreadcrumbItem.SeparatorTemplateProperty, breadcrumbItemData.SeparatorTemplate);
        }
        else
        {
            breadcrumbItem.ClearValue(BreadcrumbItem.SeparatorTemplateProperty);
        }

        if (!breadcrumbItem.IsSet(BreadcrumbItem.NavigateContextProperty) && breadcrumbItemData.NavigateContext != null)
        {
            breadcrumbItem.SetCurrentValue(BreadcrumbItem.NavigateContextProperty, breadcrumbItemData.NavigateContext);
        }

        if (!breadcrumbItem.IsSet(BreadcrumbItem.NavigateUriProperty) && breadcrumbItemData.NavigateUri != null)
        {
            breadcrumbItem.SetCurrentValue(BreadcrumbItem.NavigateUriProperty, breadcrumbItemData.NavigateUri);
        }
    }

    private void UpdateItemStates()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is BreadcrumbItem breadcrumbItem)
            {
                breadcrumbItem.IsLast = i == ItemCount - 1;
            }
        }

        _separatorManager.QueueUpdate();
    }

    private static void ClearGeneratedItemValues(BreadcrumbItem breadcrumbItem)
    {
        breadcrumbItem.ClearValue(BreadcrumbItem.ContentProperty);
        breadcrumbItem.ClearValue(BreadcrumbItem.ContentTemplateProperty);
        breadcrumbItem.ClearValue(BreadcrumbItem.IconProperty);
        breadcrumbItem.ClearValue(BreadcrumbItem.NavigateContextProperty);
        breadcrumbItem.ClearValue(BreadcrumbItem.NavigateUriProperty);
        breadcrumbItem.ClearValue(BreadcrumbItem.SeparatorProperty);
        breadcrumbItem.ClearValue(BreadcrumbItem.SeparatorTemplateProperty);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var frameInset       = Padding + BorderThickness;
        var layoutConstraint = new Size(
            Math.Max(0, constraint.Width - frameInset.Left - frameInset.Right),
            Math.Max(0, constraint.Height - frameInset.Top - frameInset.Bottom));

        var childSize = new Size();
        foreach (var visual in VisualChildren)
        {
            if (visual is not Control child)
            {
                continue;
            }

            child.Measure(layoutConstraint);
            childSize = new Size(
                Math.Max(childSize.Width, child.DesiredSize.Width),
                Math.Max(childSize.Height, child.DesiredSize.Height));
        }

        return childSize.Inflate(frameInset);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var frameInset = Padding + BorderThickness;
        var layoutSize = finalSize.Deflate(frameInset);

        foreach (var visual in VisualChildren)
        {
            if (visual is Control child)
            {
                child.Arrange(new Rect(frameInset.Left, frameInset.Top, layoutSize.Width, layoutSize.Height));
            }
        }

        return finalSize;
    }

    public sealed override void Render(DrawingContext context)
    {
        var borderRenderHelper = _borderRenderHelper ??= new BorderRenderHelper();
        borderRenderHelper.Render(
            context,
            Bounds.Size,
            BorderThickness,
            CornerRadius,
            BackgroundSizing.InnerBorderEdge,
            Background,
            BorderBrush,
            null,
            0);
    }
}
