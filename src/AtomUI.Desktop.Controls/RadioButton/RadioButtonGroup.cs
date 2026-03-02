using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public class RadioButtonGroup : ItemsControl,
                                IMotionAwareControl,
                                IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> CheckedItemProperty = 
        AvaloniaProperty.Register<RadioButtonGroup, object?>(nameof (CheckedItem));
    
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<RadioButtonGroup, double>(nameof (ItemSpacing));
    
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<RadioButtonGroup>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<RadioButtonGroup>();
    
    public object? CheckedItem
    {
        get => GetValue(CheckedItemProperty);
        set => SetValue(CheckedItemProperty, value);
    }
    
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public event EventHandler<RadioButtonGroupCheckedChangedEventArgs>? CheckedChanged;

    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => RadioButtonToken.ID;
    
    #endregion

    private readonly Dictionary<RadioButton, CompositeDisposable> _itemsBindingDisposables = new();
    private bool _ignoreSyncChecked;

    static RadioButtonGroup()
    {
        OrientationProperty.OverrideDefaultValue<RadioButtonGroup>(Orientation.Horizontal);
        RadioButton.IsCheckedChangedEvent.AddClassHandler<RadioButtonGroup>((group, args) => group.HandleRadioButtonCheckedChanged(args));
        CheckedItemProperty.Changed.AddClassHandler<RadioButtonGroup>((group, args) => group.HandleCheckedItemChanged(args));
    }
    
    public RadioButtonGroup()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is RadioButton radioButton)
                    {
                        if (_itemsBindingDisposables.TryGetValue(radioButton, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(radioButton);
                        }
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new RadioButton();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<RadioButton>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is RadioButton radioButton)
        {
            var disposables = new CompositeDisposable(4);
            
            if (item != null && item is not Visual)
            {
                {
                    if (ItemTemplate != null)
                    {
                        radioButton.SetCurrentValue(RadioButton.ContentProperty, item);
                    }
                    else
                    {
                        if (item is IRadioButtonOption radioButtonOption)
                        {
                            radioButton.SetCurrentValue(RadioButton.ContentProperty, radioButtonOption.Content);
                        }
                    }
                }

                {
                    if (item is IRadioButtonOption radioButtonOption)
                    {
                        radioButton.SetCurrentValue(RadioButton.IsEnabledProperty, radioButtonOption.IsEnabled);
                        radioButton.SetCurrentValue(RadioButton.IsCheckedProperty, radioButtonOption.IsChecked);
                    }
                }
                if (ItemTemplate != null)
                {
                    disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, radioButton, RadioButton.ContentTemplateProperty));
                }
            }
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, radioButton, RadioButton.IsMotionEnabledProperty));
            
            PrepareRadioButton(radioButton, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(radioButton, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(radioButton);
            }
            _itemsBindingDisposables.Add(radioButton, disposables);
        }  
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type RadioButton.");
        }
    }
    
    protected virtual void PrepareRadioButton(RadioButton radioButton, object? item, int index, CompositeDisposable disposables)
    {
    }
    
    private void HandleRadioButtonCheckedChanged(RoutedEventArgs args)
    {
        if (args.Source is RadioButton radioButton && radioButton.IsChecked == true)
        {
            _ignoreSyncChecked = true;
            if (ItemsSource != null)
            {
                SetCurrentValue(CheckedItemProperty, radioButton.DataContext);
            }
            else
            {
                SetCurrentValue(CheckedItemProperty, radioButton);
            }
            args.Handled = true;
        }
    }

    private void SyncCheckedState()
    {
        var isSourceMode = ItemsSource != null;
        foreach (var radioButton in LogicalChildren.OfType<RadioButton>())
        {
            if (isSourceMode)
            {
                if (radioButton.DataContext == CheckedItem)
                {
                    radioButton.SetCurrentValue(RadioButton.IsCheckedProperty, true);
                }
            }
            else
            {
                if (radioButton == CheckedItem)
                {
                    radioButton.SetCurrentValue(RadioButton.IsCheckedProperty, true);
                }
            }
        }
    }

    private void HandleCheckedItemChanged(AvaloniaPropertyChangedEventArgs args)
    {
        CheckedChanged?.Invoke(this, new RadioButtonGroupCheckedChangedEventArgs(args.OldValue, args.NewValue));
        if (_ignoreSyncChecked)
        {
            _ignoreSyncChecked = false;
            return;
        }
        SyncCheckedState();
    }
    
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is RadioButton radioButton)
        {
            if (item == CheckedItem)
            {
                radioButton.SetCurrentValue(RadioButton.IsCheckedProperty, true);
            }
        }
    }
}