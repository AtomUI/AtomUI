using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the radio button group. It shows one radio button per item and keeps at most one of them
/// checked by matching the radio buttons with <see cref="CheckedItem" />.
/// </summary>
/// <remarks>
/// A derived class supplies the radio button containers through <c>CreateContainerForItemOverride</c>.
/// <see cref="PrepareContainerForItemOverride" /> throws an <see cref="ArgumentOutOfRangeException" /> for any
/// other container type.
/// </remarks>
public abstract class AbstractRadioButtonGroup : ItemsControl,
                                                 IMotionAwareControl,
                                                 IFormItemAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="CheckedItem" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CheckedItemProperty = 
        AvaloniaProperty.Register<AbstractRadioButtonGroup, object?>(
            nameof(CheckedItem),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<AbstractRadioButtonGroup, double>(nameof (ItemSpacing));
    
    /// <summary>
    /// Identifies the <see cref="LineSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineSpacingProperty = 
        AvaloniaProperty.Register<AbstractRadioButtonGroup, double>(nameof (LineSpacing));
    
    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<AbstractRadioButtonGroup>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractRadioButtonGroup>();
    
    /// <summary>
    /// Gets or sets the checked item: the data item when <see cref="ItemsControl.ItemsSource" /> is set, otherwise the
    /// radio button itself. The default is <see langword="null" />, which means that no radio button is checked.
    /// </summary>
    /// <remarks>
    /// The property binds two-way by default and is kept in step with the radio buttons: a radio button that becomes
    /// checked is written here with <c>SetCurrentValue</c>, so a binding on the property stays in place, and a change
    /// here checks the matching radio button.
    /// </remarks>
    public object? CheckedItem
    {
        get => GetValue(CheckedItemProperty);
        set => SetValue(CheckedItemProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the gap between radio buttons on the same line, in device-independent pixels.
    /// </summary>
    /// <remarks>
    /// The theme reads this value in its wrap panel, so it takes effect only while that <c>ItemsPanel</c> is used.
    /// The theme sets it from the group's <c>SpacingXS</c> token; <c>0</c> places the radio buttons edge to edge.
    /// </remarks>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the gap between lines of radio buttons, in device-independent pixels, when the items wrap.
    /// </summary>
    /// <remarks>
    /// The theme reads this value in its wrap panel, so it takes effect only while that <c>ItemsPanel</c> is used.
    /// The theme sets it from the group's <c>SpacingXS</c> token; <c>0</c> places the lines edge to edge.
    /// </remarks>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the direction in which the radio buttons are laid out; the default is horizontal.
    /// The items wrap when the group runs out of space in that direction.
    /// </summary>
    /// <remarks>
    /// The theme reads this value in its wrap panel, so it takes effect only while that <c>ItemsPanel</c> is used.
    /// </remarks>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets whether motion is enabled. The value is forwarded to every radio button container the group
    /// prepares; the theme sets it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised after <see cref="CheckedItem" /> has changed and before the radio buttons are updated to match the new
    /// value.
    /// </summary>
    public event EventHandler<RadioButtonGroupCheckedChangedEventArgs>? CheckedChanged;

    #endregion
    
    static AbstractRadioButtonGroup()
    {
        OrientationProperty.OverrideDefaultValue<AbstractRadioButtonGroup>(Orientation.Horizontal);
        AbstractRadioButton.IsCheckedChangedEvent.AddClassHandler<AbstractRadioButtonGroup>((group, args) => group.HandleRadioButtonCheckedChanged(args));
        CheckedItemProperty.Changed.AddClassHandler<AbstractRadioButtonGroup>((group, args) => group.HandleCheckedItemChanged(args));
    }
    
    /// <summary>
    /// Decides whether an item needs a radio button container. An item that is an
    /// <see cref="AbstractRadioButton" /> is used as its own container; every other item gets one.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractRadioButton>(item, out recycleKey);
    }
    
    /// <summary>
    /// Prepares the radio button container for an item and forwards <see cref="IsMotionEnabled" /> to it.
    /// </summary>
    /// <param name="container">
    /// The container created for the item.
    /// </param>
    /// <param name="item">
    /// The item to prepare the container for.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not an <see cref="AbstractRadioButton" />.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///     Only data items are prepared; an item that is itself a <c>Visual</c> is left untouched.
    ///     </para>
    ///     <para>
    ///     With an <see cref="ItemsControl.ItemTemplate" />, the item becomes the content of the radio button and the
    ///     group's template is bound to its content template. Without one, an <see cref="IRadioButtonOption" />
    ///     supplies the content.
    ///     </para>
    ///     <para>
    ///     For an <see cref="IRadioButtonOption" />, <see cref="IRadioButtonOption.IsEnabled" /> and
    ///     <see cref="IRadioButtonOption.IsChecked" /> are copied to the radio button in both cases. The values are
    ///     set with <c>SetCurrentValue</c>, so bindings on the radio button are kept.
    ///     </para>
    /// </remarks>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractRadioButton radioButton)
        {
            if (item != null && item is not Visual)
            {
                {
                    if (ItemTemplate != null)
                    {
                        radioButton.SetCurrentValue(AbstractRadioButton.ContentProperty, item);
                    }
                    else
                    {
                        if (item is IRadioButtonOption radioButtonOption)
                        {
                            radioButton.SetCurrentValue(AbstractRadioButton.ContentProperty, radioButtonOption.Content);
                        }
                    }
                }

                {
                    if (item is IRadioButtonOption radioButtonOption)
                    {
                        radioButton.SetCurrentValue(AbstractRadioButton.IsEnabledProperty, radioButtonOption.IsEnabled);
                        radioButton.SetCurrentValue(AbstractRadioButton.IsCheckedProperty, radioButtonOption.IsChecked);
                    }
                }
                if (ItemTemplate != null)
                {
                    radioButton[!AbstractRadioButton.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
            }
            radioButton[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            
            PrepareRadioButton(radioButton, item, index);
        }  
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type RadioButton.");
        }
    }
    
    /// <summary>
    /// Called at the end of <see cref="PrepareContainerForItemOverride" /> for every radio button container.
    /// The default implementation does nothing; override it to apply per-item state to the radio button.
    /// </summary>
    /// <param name="radioButton">
    /// The container that is being prepared.
    /// </param>
    /// <param name="item">
    /// The item that the container presents.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    protected virtual void PrepareRadioButton(AbstractRadioButton radioButton, object? item, int index)
    {
    }
    
    private void HandleRadioButtonCheckedChanged(RoutedEventArgs args)
    {
        if (args.Source is AbstractRadioButton radioButton && radioButton.IsChecked == true)
        {
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
        for (var i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is not AbstractRadioButton radioButton)
            {
                continue;
            }

            var shouldBeChecked = isSourceMode
                ? radioButton.DataContext == CheckedItem
                : radioButton == CheckedItem;
            if (radioButton.IsChecked != shouldBeChecked)
            {
                radioButton.SetCurrentValue(AbstractRadioButton.IsCheckedProperty, shouldBeChecked);
            }
        }
    }

    private void HandleCheckedItemChanged(AvaloniaPropertyChangedEventArgs args)
    {
        CheckedChanged?.Invoke(this, new RadioButtonGroupCheckedChangedEventArgs(args.OldValue, args.NewValue));
        _formValueChanged?.Invoke(this, EventArgs.Empty);
        SyncCheckedState();
    }
    
    /// <summary>
    /// Checks the container of the item that is the current <see cref="CheckedItem" /> once the container is
    /// prepared, so that items added later show the checked state.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is AbstractRadioButton radioButton)
        {
            if (item == CheckedItem)
            {
                radioButton.SetCurrentValue(AbstractRadioButton.IsCheckedProperty, true);
            }
        }
    }
    
    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the group sets or resets its value.
    /// The default implementation assigns <paramref name="value" /> to <see cref="CheckedItem" /> while keeping an
    /// existing binding on it.
    /// </summary>
    /// <param name="value">
    /// The data item or radio button to check.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
        SetCurrentValue(CheckedItemProperty, value);
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the group reads its value.
    /// </summary>
    /// <returns>
    /// The value of <see cref="CheckedItem" />.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return CheckedItem;
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the group resets and has no initial value.
    /// The default implementation sets <see cref="CheckedItem" /> to <see langword="null" /> while keeping an
    /// existing binding on it.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(CheckedItemProperty, null);
    }
    
    /// <summary>
    /// Called when the validation status of the <c>FormItem</c> that hosts the group changes, for example after a
    /// validation run. The default implementation does nothing.
    /// </summary>
    /// <param name="status">
    /// The status that the form item now shows.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
