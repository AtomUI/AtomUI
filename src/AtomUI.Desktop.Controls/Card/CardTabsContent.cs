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
[TokenOwnerTransparent]
public class CardTabsContent : TemplatedControl
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> TabBarExtraContentProperty = 
        AvaloniaProperty.Register<CardTabsContent, object?>(nameof (TabBarExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> TabBarExtraContentTemplateProperty = 
        AvaloniaProperty.Register<CardTabsContent, IDataTemplate?>(nameof (TabBarExtraContentTemplate));
    
    public static readonly StyledProperty<IEnumerable?> TabItemsSourceProperty = 
        AvaloniaProperty.Register<CardTabsContent, IEnumerable?>(nameof (TabItemsSource));
    
    public static readonly StyledProperty<IDataTemplate?> TabItemTemplateProperty = 
        AvaloniaProperty.Register<CardTabsContent, IDataTemplate?>(nameof (TabItemTemplate));
    
    public object? TabBarExtraContent
    {
        get => GetValue(TabBarExtraContentProperty);
        set => SetValue(TabBarExtraContentProperty, value);
    }
    
    public IDataTemplate? TabBarExtraContentTemplate
    {
        get => GetValue(TabBarExtraContentTemplateProperty);
        set => SetValue(TabBarExtraContentTemplateProperty,  value);
    }
    
    public IEnumerable? TabItemsSource
    {
        get => GetValue(TabItemsSourceProperty);
        set => SetValue(TabItemsSourceProperty,  value);
    }
    
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
    
    [Content]
    public ControlList Items => _items;

    public CardTabsContent()
    {
        Items.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleTabsChanged);
    }

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
