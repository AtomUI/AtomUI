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

/// <summary>
/// Displays a location path as a row of <see cref="BreadcrumbItem" /> containers, with a separator
/// between each adjacent pair; the last item has no separator.
/// An item that is not a <see cref="BreadcrumbItem" /> gets one generated. An item that implements
/// <see cref="IBreadcrumbItemData" /> can supply its own content, icon, separator and navigation data.
/// </summary>
public partial class Breadcrumb : ItemsControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// The default value of the <see cref="Separator" /> property.
    /// </summary>
    public const string DefaultSeparator = "/";

    /// <summary>
    /// Identifies the <see cref="Separator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SeparatorProperty =
        AvaloniaProperty.Register<Breadcrumb, object?>(
            nameof(Separator),
            defaultValue: DefaultSeparator
        );
    
    /// <summary>
    /// Identifies the <see cref="SeparatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        AvaloniaProperty.Register<Breadcrumb, IDataTemplate?>(nameof(SeparatorTemplate));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Breadcrumb>();

    /// <summary>
    /// Content drawn as the separator between an item and the next one.
    /// </summary>
    /// <remarks>
    /// The value is applied to every item container and affects the separator that follows the item, so the
    /// last item is not affected. A separator set directly on an item, through
    /// <see cref="BreadcrumbItem.Separator" /> or <see cref="IBreadcrumbItemData.Separator" />, overrides this
    /// value. <see langword="null" /> leaves the separator without content.
    /// </remarks>
    [DependsOn(nameof(SeparatorTemplate))]
    public object? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }
    
    /// <summary>
    /// Template that builds the separator between an item and the next one.
    /// </summary>
    /// <remarks>
    /// Like <see cref="Separator" />, it is applied to every item container and affects the separator that
    /// follows the item. A template set directly on an item, through
    /// <see cref="BreadcrumbItem.SeparatorTemplate" /> or <see cref="IBreadcrumbItemData.SeparatorTemplate" />,
    /// overrides it. When it is <see langword="null" />, the separator content is displayed without a
    /// template.
    /// </remarks>
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }
    
    /// <summary>
    /// When <see langword="true" />, changes of an item's foreground and background colors are animated, for
    /// example when the pointer moves over an item.
    /// </summary>
    /// <remarks>
    /// The value is bound to every item container. When it is not set, the theme supplies it from the
    /// <c>EnableMotion</c> design token.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Occurs when an item that has a <see cref="BreadcrumbItem.NavigateUri" /> or a
    /// <see cref="BreadcrumbItem.NavigateContext" /> is clicked.
    /// </summary>
    /// <remarks>
    /// The event argument carries the clicked item. The event is raised before a set
    /// <see cref="BreadcrumbItem.NavigateUri" /> is handed to the system launcher, and it cannot be canceled.
    /// </remarks>
    public event EventHandler<BreadcrumbNavigateEventArgs>? NavigateRequest;

    #endregion

    #region 内部协作 API

    /// <summary>
    /// Raises <see cref="NavigateRequest" /> for an item that was clicked and has a navigation target.
    /// </summary>
    internal void NotifyNavigateRequest(BreadcrumbItem breadcrumbItem)
    {
        NavigateRequest?.Invoke(this, new BreadcrumbNavigateEventArgs(breadcrumbItem));
    }

    /// <summary>
    /// Adds a separator presenter to the logical children of the breadcrumb.
    /// </summary>
    internal void AttachSeparatorLogicalChild(Control presenter)
    {
        LogicalChildren.Add(presenter);
    }

    /// <summary>
    /// Removes a separator presenter from the logical children of the breadcrumb.
    /// </summary>
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

    /// <summary>
    /// Creates a new <see cref="BreadcrumbItem" /> as the container for an item and adds the item part class
    /// to it.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var breadcrumbItem = new BreadcrumbItem();
        breadcrumbItem.Classes.Add(BreadcrumbSemanticParts.ItemClass);
        return breadcrumbItem;
    }

    /// <summary>
    /// Returns <see langword="true" /> unless the item is a <see cref="BreadcrumbItem" />, which then serves
    /// as its own container.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<BreadcrumbItem>(item, out recycleKey);
    }

    /// <summary>
    /// Requires the container to be a <see cref="BreadcrumbItem" />, fills it from an
    /// <see cref="IBreadcrumbItemData" /> item, binds it to this control and then calls
    /// <see cref="PrepareBreadcrumbItem" />.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not a <see cref="BreadcrumbItem" />.
    /// </exception>
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

    /// <summary>
    /// After the base call, updates which item is the last one and the set of separators.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        UpdateItemStates();
    }
    
    /// <summary>
    /// Adjusts a container for a specific item. It is called from
    /// <see cref="PrepareContainerForItemOverride" /> after the container is filled and bound to this control.
    /// The default implementation does nothing, so an override does not need to call the base member.
    /// </summary>
    /// <param name="breadcrumbItem">
    /// The container that presents <paramref name="item" />.
    /// </param>
    /// <param name="item">
    /// The item being prepared, or <see langword="null" />.
    /// </param>
    /// <param name="index">
    /// The position of the item in the items source.
    /// </param>
    protected virtual void PrepareBreadcrumbItem(BreadcrumbItem breadcrumbItem, object? item, int index)
    {
    }

    /// <summary>
    /// After the base call, updates which item is the last one and the set of separators.
    /// </summary>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        UpdateItemStates();
    }

    /// <summary>
    /// Clears the content, icon, navigation and separator values of a generated container before the base
    /// call, so that a recycled container does not show its previous item. A <see cref="BreadcrumbItem" />
    /// that was added to the items directly keeps its values. After the base call, the last item state and
    /// the separators are updated.
    /// </summary>
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

    /// <summary>
    /// After the base call, forwards a changed <see cref="Separator" /> or <see cref="SeparatorTemplate" />
    /// to every realized item container, without replacing values set on an item itself.
    /// </summary>
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

    /// <summary>
    /// After the base call, rebuilds the separators so that they attach to the panel of the new template.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _separatorManager.OnTemplateApplied();
    }

    /// <summary>
    /// After the base call, starts tracking the items and creates a separator between each adjacent pair of
    /// items.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _separatorManager.Attach();
    }

    /// <summary>
    /// Before the base call, removes all separators and stops tracking the items. The separators are created
    /// again when the control is attached again.
    /// </summary>
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

    /// <summary>
    /// Measures each child against the available size reduced by the padding and border thickness, and
    /// returns the largest desired size plus that inset.
    /// </summary>
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

    /// <summary>
    /// Arranges every child in the same rectangle, inset by the padding and border thickness, and returns
    /// the size it was given.
    /// </summary>
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

    /// <summary>
    /// Paints the background and border of the control itself; the background ends at the inner edge of the
    /// border.
    /// </summary>
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
