using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

// A Token-owner transparent part: CardTabsContent is the tabs body of a Card and has no Own Tokens, so its theme reads the
// Card TokenResource; Controls[Card] reaches the tab bar edge padding and the tab content padding. The hosted TabControl
// keeps its own identity.
/// <summary>
/// Tabs body for the content of a <see cref="Card" />: a tab bar with the tabs declared in <see cref="Items" /> or
/// supplied by <see cref="TabItemsSource" />, and below it the content of the selected tab. The tabs are shown by
/// a <see cref="TabControl" /> inside the template.
/// </summary>
[TokenOwnerTransparent]
public class CardTabsContent : TemplatedControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="TabBarExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TabBarExtraContentProperty = 
        AvaloniaProperty.Register<CardTabsContent, object?>(nameof (TabBarExtraContent));
    
    /// <summary>
    /// Identifies the <see cref="TabBarExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TabBarExtraContentTemplateProperty = 
        AvaloniaProperty.Register<CardTabsContent, IDataTemplate?>(nameof (TabBarExtraContentTemplate));
    
    /// <summary>
    /// Identifies the <see cref="TabItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> TabItemsSourceProperty = 
        AvaloniaProperty.Register<CardTabsContent, IEnumerable?>(nameof (TabItemsSource));
    
    /// <summary>
    /// Identifies the <see cref="TabItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TabItemTemplateProperty = 
        AvaloniaProperty.Register<CardTabsContent, IDataTemplate?>(nameof (TabItemTemplate));
    
    /// <summary>
    /// Extra content shown at the end of the tab bar, for example a link to more options, or
    /// <see langword="null" /> to show none.
    /// </summary>
    public object? TabBarExtraContent
    {
        get => GetValue(TabBarExtraContentProperty);
        set => SetValue(TabBarExtraContentProperty, value);
    }
    
    /// <summary>
    /// Template that presents <see cref="TabBarExtraContent" /> when it is data and not a control.
    /// </summary>
    public IDataTemplate? TabBarExtraContentTemplate
    {
        get => GetValue(TabBarExtraContentTemplateProperty);
        set => SetValue(TabBarExtraContentTemplateProperty,  value);
    }
    
    /// <summary>
    /// Items the tabs are generated from, as an alternative to declaring the tabs in
    /// <see cref="Items" />. Each item is presented by <see cref="TabItemTemplate" />.
    /// </summary>
    public IEnumerable? TabItemsSource
    {
        get => GetValue(TabItemsSourceProperty);
        set => SetValue(TabItemsSourceProperty,  value);
    }
    
    /// <summary>
    /// Template that builds a tab from one item of <see cref="TabItemsSource" />. Its data type is taken from the type
    /// of the items, so its bindings can be compiled against that type.
    /// </summary>
    [InheritDataTypeFromItems(nameof(TabItemsSource))]
    public IDataTemplate? TabItemTemplate
    {
        get => GetValue(TabItemTemplateProperty);
        set => SetValue(TabItemTemplateProperty,  value);
    }
    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CardTabsContent>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardTabsContent>();
    
    /// <summary>
    /// Size step of the tabs body; selects the padding of the tab content and the padding at both ends of the tab bar.
    /// A <see cref="Card" /> that shows this content relays its own <see cref="Card.SizeType" /> here, so set it on
    /// the card. Unset, the value is <c>Middle</c>.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    private readonly ControlList _items = new ControlList();
    private TabControl? _tabControl;
    
    /// <summary>
    /// Tabs of the content, written as the XAML content of this element, normally one <see cref="TabItem" /> per tab.
    /// Every control in the list becomes an item of the <see cref="TabControl" /> in the template; add, remove,
    /// replace and move changes are mirrored once the template is applied.
    /// </summary>
    [Content]
    public ControlList Items => _items;

    public CardTabsContent()
    {
        Items.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleTabsChanged);
    }

    /// <summary>
    /// Called when the template is applied. It clears the items of the <see cref="TabControl" /> found by the previous
    /// template, finds <c>PART_TabControl</c> and copies every control in <see cref="Items" /> into it. An override
    /// must call the base member so that the tab control shows <see cref="Items" />.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_tabControl != null)
        {
            _tabControl.Items.Clear();
        }

        _tabControl = e.NameScope.Find<TabControl>("PART_TabControl");
        if (_tabControl != null)
        {
            foreach (var tabItem in Items)
            {
                _tabControl.Items.Add(tabItem);
            }
        }
    }
    
    private void HandleTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_tabControl != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newStartingIndex = e.NewStartingIndex;
                    for (var i = 0; i < e.NewItems!.Count; i++)
                    {
                        if (e.NewItems[i] is Control control)
                        {
                            _tabControl.Items.Insert(newStartingIndex++, control);
                        }
                    }
               
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems!.Count; i++)
                    {
                        _tabControl.Items.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int index1 = 0; index1 < e.OldItems!.Count; ++index1)
                    {
                        int     index2  = index1 + e.OldStartingIndex;
                        Control newItem = (Control) e.NewItems![index1]!;
                        _tabControl.Items[index2] = newItem;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    var moveItems = ControlCollectionChangedUtils.CollectControls(e.OldItems!);
                    if (moveItems is null)
                    {
                        break;
                    }

                    for (var i = 0; i < moveItems.Count; ++i)
                    {
                        _tabControl.Items.RemoveAt(e.OldStartingIndex);
                    }

                    var insertIndex = e.NewStartingIndex;
                    if (insertIndex > e.OldStartingIndex)
                    {
                        insertIndex -= moveItems.Count - 1;
                    }

                    foreach (var control in moveItems)
                    {
                        _tabControl.Items.Insert(insertIndex++, control);
                    }
                    
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }
}
