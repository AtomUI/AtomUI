using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls;

/// <summary>
/// Base class of a tab item: a headered content control that reports whether it is selected and carries the
/// placement of the tab strip it belongs to.
/// </summary>
/// <remarks>
/// The template can declare <c>PART_HeaderPresenter</c>, the element the selection indicator spans when it
/// follows the header of the selected tab.
/// </remarks>
[TemplatePart("PART_HeaderPresenter", typeof(Control), IsRequired = false)]
[PseudoClasses(":selected")]
public abstract class AbstractTabItem : HeaderedContentControl, ISelectable
{
    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<AbstractTabItem>();

    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractTabItem, Dock?> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<AbstractTabItem, Dock?>(nameof(TabStripPlacement), owner => owner.TabStripPlacement);

    private Dock? _tabStripPlacement;

    static AbstractTabItem()
    {
        SelectableMixin.Attach<AbstractTabItem>(IsSelectedProperty);
        FocusableProperty.OverrideDefaultValue<AbstractTabItem>(true);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<AbstractTabItem>(AutomationControlType.TabItem);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<AbstractTabItem>(IsOffscreenBehavior.FromClip);
    }

    /// <summary>
    /// Whether the item is the selected tab. The tab control or tab strip that hosts the item sets the value
    /// together with its own selection; <see langword="true" /> also sets the <c>:selected</c> pseudo-class.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Edge of the tab strip the item belongs to, used by the item themes to lay the item out for a horizontal
    /// or vertical strip. The hosting control copies its own placement here when it prepares the item and
    /// when that placement changes. It is <see langword="null" /> until then; the setter is internal.
    /// </summary>
    public Dock? TabStripPlacement
    {
        get => _tabStripPlacement;
        internal set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    /// <summary>
    /// The header element resolved from the template by <see cref="OnApplyTemplate" />, or <see langword="null" />
    /// when the template has neither <c>PART_HeaderPresenter</c> nor <c>ContentPresenter</c>. The selection
    /// indicator measures it for <see cref="TabIndicatorMode.Content" /> and <see cref="TabIndicatorMode.Fixed" />.
    /// </summary>
    internal Control? IndicatorHeader { get; private set; }

    /// <summary>
    /// Resolves <see cref="IndicatorHeader" /> from the applied template. Overrides call the base member before
    /// they read that element.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        IndicatorHeader = e.NameScope.Find<Control>("PART_HeaderPresenter") ?? e.NameScope.Find<Control>("ContentPresenter");
    }
}
