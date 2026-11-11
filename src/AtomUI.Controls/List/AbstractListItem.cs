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
    public const string HasPrefixPseudoClass = ":has-prefix";
    public const string HasTitlePseudoClass = ":has-title";
    public const string HasDescriptionPseudoClass = ":has-description";
    public const string HasExtraPseudoClass = ":has-extra";

    #region 公共属性定义

    public static readonly StyledProperty<object?> PrefixProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Prefix));

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Title));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Description));

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractListItem, object?>(nameof(Extra));

    public object? Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

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
