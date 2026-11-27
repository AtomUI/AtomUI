using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AtomUI.Controls;

/// <summary>
/// One panel of a collapse control: a header with an expand icon, the header content and an optional add-on,
/// above the panel content that is shown while <see cref="IsSelected" /> is <see langword="true" />.
/// A collapse uses an item of this type as the panel of its data entry and creates one for every other entry.
/// </summary>
[PseudoClasses(":selected")]
public abstract class AbstractCollapseItem : HeaderedContentControl, ISelectable
{
    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<AbstractCollapseItem>();

    /// <summary>
    /// Identifies the <see cref="IsShowExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowExpandIconProperty =
        AvaloniaProperty.Register<AbstractCollapseItem, bool>(nameof(IsShowExpandIcon), true);

    /// <summary>
    /// Identifies the <see cref="ExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ExpandIconProperty =
        AvaloniaProperty.Register<AbstractCollapseItem, PathIcon?>(nameof(ExpandIcon));

    /// <summary>
    /// Identifies the <see cref="AddOnContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> AddOnContentProperty =
        AvaloniaProperty.Register<AbstractCollapseItem, object?>(nameof(AddOnContent));

    /// <summary>
    /// Identifies the <see cref="AddOnContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> AddOnContentTemplateProperty =
        AvaloniaProperty.Register<AbstractCollapseItem, IDataTemplate?>(nameof(AddOnContentTemplate));

    /// <summary>
    /// Gets or sets whether the panel is expanded. While <see langword="true" />, the theme shows the panel content
    /// and sets the <c>:selected</c> pseudo-class; a change shows or hides the content with a motion.
    /// A collapse sets the value on the panel from the <c>IsSelected</c> of its data entry.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the expand icon is shown at the start of the header.
    /// A collapse sets the value on the panel from the <c>IsShowExpandIcon</c> of its data entry.
    /// </summary>
    public bool IsShowExpandIcon
    {
        get => GetValue(IsShowExpandIconProperty);
        set => SetValue(IsShowExpandIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon shown at the start of the header. It is visible only while
    /// <see cref="IsShowExpandIcon" /> is <see langword="true" />, and the theme rotates it by 90 degrees while the
    /// panel is expanded.
    /// When <see langword="null" />, <c>CollapseItem</c> installs its default icon at template priority, which a
    /// value set by a style or on the item replaces.
    /// </summary>
    public PathIcon? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    /// <summary>
    /// Gets or sets extra content shown in the header after the header content. <see langword="null" /> shows nothing.
    /// </summary>
    public object? AddOnContent
    {
        get => GetValue(AddOnContentProperty);
        set => SetValue(AddOnContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display <see cref="AddOnContent" />. When <see langword="null" />, the add-on
    /// content is displayed with the data template found for its type.
    /// </summary>
    public IDataTemplate? AddOnContentTemplate
    {
        get => GetValue(AddOnContentTemplateProperty);
        set => SetValue(AddOnContentTemplateProperty, value);
    }

    static AbstractCollapseItem()
    {
        SelectableMixin.Attach<AbstractCollapseItem>(IsSelectedProperty);
        FocusableProperty.OverrideDefaultValue<AbstractCollapseItem>(true);
    }
}
