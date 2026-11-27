using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls.Commons;

using AvaloniaButton = Avalonia.Controls.Button;

/// <summary>
/// Base class of links rendered as a button. It mirrors the visited, icon-only, loading and danger states into
/// pseudo-classes and opens <see cref="NavigateUri" /> after a click.
/// </summary>
[PseudoClasses(ButtonPseudoClass.Visited,
    ButtonPseudoClass.IconOnly,
    ButtonPseudoClass.Loading,
    ButtonPseudoClass.IsDanger)]
public abstract class AbstractHyperLinkButton : AvaloniaButton,
                                                ICustomizableSizeTypeAware,
                                                IMotionAwareControl
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDangerProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, bool>(nameof(IsDanger));
    
    /// <summary>
    /// Identifies the <see cref="IsGhost" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGhostProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, bool>(nameof(IsGhost));
    
    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, bool>(nameof(IsLoading));
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractHyperLinkButton>();

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty = 
        AvaloniaProperty.Register<AbstractHyperLinkButton, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IconWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, double>(nameof(IconWidth));

    /// <summary>
    /// Identifies the <see cref="IconHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, double>(nameof(IconHeight));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractHyperLinkButton>();

    /// <summary>
    /// Identifies the <see cref="IsVisited" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsVisitedProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, bool>(
            nameof(IsVisited),
            defaultValue: false);

    /// <summary>
    /// Identifies the <see cref="NavigateUri" /> property.
    /// </summary>
    public static readonly StyledProperty<Uri?> NavigateUriProperty =
        AvaloniaProperty.Register<AbstractHyperLinkButton, Uri?>(
            nameof(NavigateUri),
            defaultValue: null);
    
    /// <summary>
    /// Colors the link as an error link. While it is set the link raises the <c>danger</c> pseudo-class, and the
    /// default theme uses the error colors instead of the link colors in the normal, pointer-over and pressed states.
    /// </summary>
    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }
    
    /// <summary>
    /// Requests a link without a filled background. The default theme paints the background transparent whether or not
    /// it is set, so the property has no visible effect there.
    /// </summary>
    public bool IsGhost
    {
        get => GetValue(IsGhostProperty);
        set => SetValue(IsGhostProperty, value);
    }
    
    /// <summary>
    /// Switches the link to its loading look. It raises the <c>loading</c> pseudo-class, with which the default theme
    /// replaces the icon with a spinning loading icon and lowers the opacity. The text stays and the link stays
    /// enabled.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    /// <summary>
    /// Size step of the link. The default theme takes the height, padding, font size and icon size from the Tokens of
    /// that step; <see cref="CustomizableSizeType.Custom" /> uses the middle values.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Icon shown before the content, or <see langword="null" /> for no icon. The theme sizes it with
    /// <see cref="IconWidth" /> and <see cref="IconHeight" /> and paints it with the foreground. An icon without
    /// content raises the <c>icononly</c> pseudo-class, with which the theme applies the icon-only padding and size.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Width of the icon and of the loading icon. The default theme sets it from the icon size Token of the
    /// <see cref="SizeType" /> step; an explicit value overrides that size.
    /// </summary>
    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// Height of the icon and of the loading icon. The default theme sets it from the icon size Token of the
    /// <see cref="SizeType" /> step; an explicit value overrides that size.
    /// </summary>
    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }
    
    /// <summary>
    /// Enables the background and foreground transitions of the default theme, which initializes the value from the
    /// <c>EnableMotion</c> Token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Marks the link as visited. It raises the <c>visited</c> pseudo-class, which the default theme does not style,
    /// and is set to <see langword="true" /> after <see cref="NavigateUri" /> was opened successfully.
    /// </summary>
    public bool IsVisited
    {
        get => GetValue(IsVisitedProperty);
        set => SetValue(IsVisitedProperty, value);
    }

    /// <summary>
    /// Address opened with the system launcher after the link is clicked, or <see langword="null" /> to only raise the
    /// click. <see cref="IsVisited" /> becomes <see langword="true" /> when the launch succeeds; nothing happens when
    /// the link has no top level or the launch fails.
    /// </summary>
    public Uri? NavigateUri
    {
        get => GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }

    #endregion

    static AbstractHyperLinkButton()
    {
        AffectsMeasure<AbstractHyperLinkButton>(SizeTypeProperty,
            IconProperty,
            IconWidthProperty,
            IconHeightProperty);
        AffectsRender<AbstractHyperLinkButton>(IsDangerProperty,
            IsGhostProperty);
    }

    /// <summary>
    /// Updates the pseudo-classes when <see cref="IsVisited" />, <see cref="ContentControl.Content" />, <see
    /// cref="Icon" />, <see cref="IsLoading" /> or <see cref="IsDanger" /> changes. Derived overrides must call the
    /// base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisitedProperty ||
            change.Property == ContentProperty ||
            change.Property == IconProperty ||
            change.Property == IsLoadingProperty ||
            change.Property == IsDangerProperty)
        {
            UpdatePseudoClasses();
        }
    }

    /// <summary>
    /// Applies the pseudo-classes from the current property values once the template is applied. Derived overrides
    /// must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    // The theme maps icon-only, loading and danger visuals through these pseudo-classes.
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ButtonPseudoClass.Visited, IsVisited);
        PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        PseudoClasses.Set(ButtonPseudoClass.Loading, IsLoading);
        PseudoClasses.Set(ButtonPseudoClass.IsDanger, IsDanger);
    }

    /// <summary>
    /// Raises the click and then opens <see cref="NavigateUri" /> when it is set. The launch is queued on the
    /// dispatcher, so this member returns before the address is open. Derived overrides must call the base member to
    /// keep the navigation.
    /// </summary>
    protected override void OnClick()
    {
        base.OnClick();

        Uri? uri = NavigateUri;
        if (uri is not null)
        {
            this.Dispatcher.InvokeAsync(async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is null)
                {
                    return;
                }

                bool success = await topLevel.Launcher.LaunchUriAsync(uri);
                if (success)
                {
                    SetCurrentValue(IsVisitedProperty, true);
                }
            });
        }
    }

}
