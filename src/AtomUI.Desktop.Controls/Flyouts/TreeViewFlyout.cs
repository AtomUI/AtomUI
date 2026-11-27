using System.Collections;
using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A flyout that shows a tree of <c>MenuItem</c> nodes, built from <see cref="Items" /> or
/// <see cref="ItemsSource" />, in a <see cref="TreeViewFlyoutPresenter" />.
/// </summary>
public class TreeViewFlyout : Flyout
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<TreeViewFlyout, IEnumerable?>(
            nameof(ItemsSource));

    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<TreeViewFlyout, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// The theme of the <c>MenuItem</c> containers that the presenter generates for the items. Set it on the flyout;
    /// the value is relayed one-way to the item container theme of the presenter.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<TreeViewFlyout>();

    /// <summary>
    /// Identifies the <see cref="FlyoutPresenterTheme" /> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> FlyoutPresenterThemeProperty =
        AvaloniaProperty.Register<TreeViewFlyout, ControlTheme?>(nameof(FlyoutPresenterTheme));

    /// <summary>
    /// The source of the items shown in the tree. A change is applied to <see cref="Items" />, which then shows the
    /// items of the source and becomes read-only; <see langword="null" /> returns it to an empty collection that
    /// stores its own items. Setting a source while <see cref="Items" /> holds items added directly throws
    /// <see cref="InvalidOperationException" />.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The template that displays one item of the tree. It is relayed one-way to the presenter and used for the
    /// <c>MenuItem</c> the presenter generates for that item; <see langword="null" /> applies no template.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The theme applied to the <see cref="TreeViewFlyoutPresenter" /> when the flyout opens. With
    /// <see langword="null" />, the theme of the presenter is left untouched.
    /// </summary>
    public ControlTheme? FlyoutPresenterTheme
    {
        get => GetValue(FlyoutPresenterThemeProperty);
        set => SetValue(FlyoutPresenterThemeProperty, value);
    }

    /// <summary>
    /// The items shown in the tree. They can be added in XAML content or through the collection itself, and are
    /// passed to the presenter as its items source. While <see cref="ItemsSource" /> is set, the collection shows
    /// that source instead and is read-only.
    /// </summary>
    [Content]
    public ItemCollection Items { get; }

    #endregion

    private protected TreeViewFlyoutPresenter? Presenter;
    private CompositeDisposable? _presenterBindingDisposables;

    static TreeViewFlyout()
    {
        ShouldUseOverlayPopupProperty.OverrideDefaultValue<TreeViewFlyout>(true);
    }

    public TreeViewFlyout()
    {
        var itemCollectionType = typeof(ItemCollection);
        Items = (ItemCollection)Activator.CreateInstance(itemCollectionType, true)!;
        BindPointerPlacementOffsets(FlyoutHostTokenKind.HorizontalOffset, FlyoutHostTokenKind.VerticalOffset);
    }

    /// <summary>
    /// Creates the <see cref="TreeViewFlyoutPresenter" /> that the popup hosts, with <see cref="Items" /> as its
    /// items source, and relays the item template, the item container theme, the effective arrow visibility, the
    /// arrow position, the motion switch and the pinned-open state to it one-way. The relay bindings of a previous
    /// presenter are disposed first.
    /// </summary>
    protected override Control CreatePresenter()
    {
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(6);

        Presenter = new TreeViewFlyoutPresenter
        {
            TreeViewFlyout = this,
            ItemsSource = Items
        };

        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, Presenter, TreeViewFlyoutPresenter.ItemTemplateProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ItemContainerThemeProperty, Presenter, TreeViewFlyoutPresenter.ItemContainerThemeProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsArrowVisibleEffectiveProperty, Presenter, TreeViewFlyoutPresenter.IsArrowVisibleProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, Presenter, TreeViewFlyoutPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ArrowPositionProperty, Presenter, TreeViewFlyoutPresenter.ArrowPositionProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, Presenter, TreeView.IsPopupPinnedOpenProperty));
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();

        return Presenter;
    }

    /// <summary>
    /// Applies <see cref="FlyoutPresenterTheme" /> to the presenter hosted by the popup when the flyout is opening,
    /// then calls the base implementation.
    /// </summary>
    protected override void OnOpening(CancelEventArgs args)
    {
        if (Popup.Child is { } presenter)
        {
            if (FlyoutPresenterTheme is { } theme)
            {
                presenter.SetValue(StyledElement.ThemeProperty, theme);
            }
        }

        base.OnOpening(args);
    }

    /// <summary>
    /// Calls the base implementation, then forwards a changed <see cref="ItemsSource" /> to <see cref="Items" />.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            Items.SetItemsSource(change.GetNewValue<IEnumerable?>());
        }
    }
}
