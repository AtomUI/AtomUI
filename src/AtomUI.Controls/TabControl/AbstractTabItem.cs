using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls;

[TemplatePart("PART_HeaderPresenter", typeof(Control), IsRequired = false)]
[PseudoClasses(":selected")]
public abstract class AbstractTabItem : HeaderedContentControl, ISelectable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<AbstractTabItem>();

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

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Dock? TabStripPlacement
    {
        get => _tabStripPlacement;
        internal set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    internal Control? IndicatorHeader { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        IndicatorHeader = e.NameScope.Find<Control>("PART_HeaderPresenter") ?? e.NameScope.Find<Control>("ContentPresenter");
    }
}
