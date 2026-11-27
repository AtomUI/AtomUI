using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Shared base of the list rows: prefix, title, description and extra regions next to the main content. A region
/// that has a value sets its pseudo-class (<c>:has-prefix</c>, <c>:has-title</c>, <c>:has-description</c>,
/// <c>:has-extra</c>) so that themes can collapse the empty ones; a region value that is a logical object is kept
/// as a logical child of the item.
/// </summary>
public abstract class AbstractListItem : ContentControl
{
    /// <summary>
    /// Pseudo-class that is set while <see cref="Prefix" /> is not <see langword="null" />.
    /// </summary>
    public const string HasPrefixPseudoClass = ":has-prefix";
    /// <summary>
    /// Pseudo-class that is set while <see cref="Title" /> is not <see langword="null" />.
    /// </summary>
    public const string HasTitlePseudoClass = ":has-title";
    /// <summary>
    /// Pseudo-class that is set while <see cref="Description" /> is not <see langword="null" />.
    /// </summary>
    public const string HasDescriptionPseudoClass = ":has-description";
    /// <summary>
    /// Pseudo-class that is set while <see cref="Extra" /> is not <see langword="null" />.
    /// </summary>
    public const string HasExtraPseudoClass = ":has-extra";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Prefix" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> PrefixProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Prefix));

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Description));

    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Extra));

    /// <summary>
    /// Content of the prefix region of the item. The default is <see langword="null" />, which leaves the region
    /// empty and <see cref="HasPrefixPseudoClass" /> unset. Any other value sets the pseudo-class, and when it
    /// implements <see cref="ILogical" /> it replaces the previous value among the logical children of the item.
    /// </summary>
    public object? Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /// <summary>
    /// Content of the title region of the item. The default is <see langword="null" />, which leaves the region
    /// empty and <see cref="HasTitlePseudoClass" /> unset. Any other value sets the pseudo-class, and when it
    /// implements <see cref="ILogical" /> it replaces the previous value among the logical children of the item.
    /// </summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Content of the description region of the item. The default is <see langword="null" />, which leaves the
    /// region empty and <see cref="HasDescriptionPseudoClass" /> unset. Any other value sets the pseudo-class, and
    /// when it implements <see cref="ILogical" /> it replaces the previous value among the logical children of the
    /// item.
    /// </summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Content of the extra region of the item. The default is <see langword="null" />, which leaves the region
    /// empty and <see cref="HasExtraPseudoClass" /> unset. Any other value sets the pseudo-class, and when it
    /// implements <see cref="ILogical" /> it replaces the previous value among the logical children of the item.
    /// </summary>
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    #endregion

    protected AbstractListItem()
    {
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Handles a change of <see cref="Prefix" />, <see cref="Title" />, <see cref="Description" /> or
    /// <see cref="Extra" />: the previous value leaves the logical children and the new value joins them when
    /// they implement <see cref="ILogical" />, and the region pseudo-classes follow the new values. An override
    /// must call the base method to keep this.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PrefixProperty ||
            change.Property == TitleProperty ||
            change.Property == DescriptionProperty ||
            change.Property == ExtraProperty)
        {
            if (change.OldValue is ILogical oldChild)
            {
                LogicalChildren.Remove(oldChild);
            }
            if (change.NewValue is ILogical newChild)
            {
                LogicalChildren.Add(newChild);
            }
            UpdatePseudoClasses();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(HasPrefixPseudoClass, Prefix is not null);
        PseudoClasses.Set(HasTitlePseudoClass, Title is not null);
        PseudoClasses.Set(HasDescriptionPseudoClass, Description is not null);
        PseudoClasses.Set(HasExtraPseudoClass, Extra is not null);
    }
}
