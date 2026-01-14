using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class MentionOptionList : List
{
    public static readonly RoutedEvent<MentionOptionItemClickedEventArgs> ItemClickedEvent =
        RoutedEvent.Register<MentionOptionList, MentionOptionItemClickedEventArgs>(
            nameof(ItemClicked),
            RoutingStrategies.Direct);
    
    public event EventHandler<MentionOptionItemClickedEventArgs>? ItemClicked
    {
        add => AddHandler(ItemClickedEvent, value);
        remove => RemoveHandler(ItemClickedEvent, value);
    }
    
    internal override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new MentionOptionListItem(this);
    }
    
    internal override void PrepareContainerForItemOverride(CompositeDisposable disposables, Control container, object? item, int index)
    {
        if (container is MentionOptionListItem listItem)
        {
            if (item != null && item is not Visual)
            {
                if (ItemTemplate != null)
                {
                    listItem.SetCurrentValue(MentionOptionListItem.ContentProperty, item);
                }
                else if (item is IMentionOption mentionOption)
                {
                    listItem.SetCurrentValue(MentionOptionListItem.ContentProperty, mentionOption.Header);
                }
                if (item is IMentionOption data)
                {
                    if (!listItem.IsSet(IsEnabledProperty))
                    {
                        listItem.SetCurrentValue(IsEnabledProperty, data.IsEnabled);
                    }
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, listItem, MentionOptionListItem.ContentTemplateProperty));
            }
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, listItem, MentionOptionListItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, listItem, MentionOptionListItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowSelectedIndicatorProperty, listItem, MentionOptionListItem.IsShowSelectedIndicatorProperty));
            disposables.Add(BindUtils.RelayBind(this, DisabledItemHoverEffectProperty, listItem,
                SelectOptionItem.DisabledItemHoverEffectProperty));
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SelectedIndex = 0;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        SelectedIndex = 0;
    }

    internal void NotifyItemClicked(MentionOptionListItem item)
    {
        if (ListDefaultView?.ItemFromContainer(item) is IMentionOption option)
        {
            RaiseEvent(new MentionOptionItemClickedEventArgs(option)
            {
                Source = this,
                RoutedEvent = ItemClickedEvent,
            });
        }
    }
    
    internal void MoveSelectedBy(int delta)
    {
        if (ListDefaultView == null || delta == 0)
        {
            return;
        }
        
        var itemCount = ItemCount;
        if (itemCount <= 0)
        {
            return;
        }

        var startIndex = SelectedIndex;

        if (startIndex < 0 || startIndex >= itemCount)
        {
            return;
        }
        
        var nextIndex = FindNextEnabledOptionIndex(startIndex, delta);
        if (nextIndex < 0)
        {
            return;
        }
        
        if (ListDefaultView.Items[nextIndex] is IMentionOption option)
        {
            SelectedItem = option;
        }
    }
    
    private int FindNextEnabledOptionIndex(int startIndex, int delta)
    {
        if (ListDefaultView == null)
        {
            return -1;
        }
        
        var index = startIndex;
        while (true)
        {
            index += delta;
            if (index < 0 || index >= ItemCount)
            {
                return -1;
            }
            
            if (ListDefaultView.Items[index] is IMentionOption option && option.IsEnabled)
            {
                return index;
            }
        }
    }
}