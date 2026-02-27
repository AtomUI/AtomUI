using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public enum FormLabelAlign
{
    Left,
    Right
}

public enum FormLayout
{
    Horizontal,
    Vertical,
    Inline
}

public enum FormRequiredMark
{
    Default,
    Hidden,
    Optional,
    Customize
}

public enum FormValidateTrigger
{
    OnChanged,
    OnBlur,
    OnSubmit
}

public class Form : ItemsControl,
                    ISizeTypeAware,
                    IMotionAwareControl,
                    IControlSharedTokenResourcesHost,
                    IForm
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Form, bool>(
            nameof(IsShowColon), true);
    
    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AvaloniaProperty.Register<Form, FormRequiredMark>(nameof(RequiredMark), FormRequiredMark.Default);
    
    public static readonly StyledProperty<IconTemplate?> ErrorFeedbackIconProperty =
        AvaloniaProperty.Register<Form, IconTemplate?>(nameof(ErrorFeedbackIcon));
    
    public static readonly StyledProperty<IconTemplate?> WarningFeedbackIconProperty =
        AvaloniaProperty.Register<Form, IconTemplate?>(nameof(WarningFeedbackIcon));
    
    public static readonly StyledProperty<IFormValue?> InitialValuesProperty =
        AvaloniaProperty.Register<Form, IFormValue?>(nameof(InitialValues));
    
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        AvaloniaProperty.Register<Form, FormLabelAlign>(nameof(LabelAlign), FormLabelAlign.Right);
    
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        AvaloniaProperty.Register<Form, TextWrapping>(nameof(LabelWrapping), TextWrapping.NoWrap);
    
    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<Form, FormLayout>(nameof(FormLayout), FormLayout.Horizontal);
    
    public static readonly StyledProperty<IControlTemplate?> CustomRequireMarkProperty =
        AvaloniaProperty.Register<Form, IControlTemplate?>(nameof(CustomRequireMark));
    
    public static readonly StyledProperty<IControlTemplate?> CustomOptionalMarkProperty =
        AvaloniaProperty.Register<Form, IControlTemplate?>(nameof(CustomOptionalMark));
    
    public static readonly StyledProperty<bool> ScrollToFirstErrorProperty =
        AvaloniaProperty.Register<Form, bool>(nameof(ScrollToFirstError));
    
    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AvaloniaProperty.Register<Form, FormValidateTrigger>(nameof(ValidateTrigger), FormValidateTrigger.OnSubmit);
    
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(LabelColInfo));
    
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(WrapperColInfo));
    
    public static readonly StyledProperty<Type> ValueTypeProperty =
        AvaloniaProperty.Register<Form, Type>(nameof(ValueType), typeof(DefaultFormValue),
            validate: ValidateValueType);
    
    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }
    
    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    public IconTemplate? ErrorFeedbackIcon
    {
        get => GetValue(ErrorFeedbackIconProperty);
        set => SetValue(ErrorFeedbackIconProperty, value);
    }
    
    public IconTemplate? WarningFeedbackIcon
    {
        get => GetValue(WarningFeedbackIconProperty);
        set => SetValue(WarningFeedbackIconProperty, value);
    }
        
    public IFormValue? InitialValues
    {
        get => GetValue(InitialValuesProperty);
        set => SetValue(InitialValuesProperty, value);
    }
    
    public FormLabelAlign LabelAlign
    {
        get => GetValue(LabelAlignProperty);
        set => SetValue(LabelAlignProperty, value);
    }
    
    public TextWrapping LabelWrapping
    {
        get => GetValue(LabelWrappingProperty);
        set => SetValue(LabelWrappingProperty, value);
    }
    
    public FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }
    
    public IControlTemplate? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    public IControlTemplate? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    public bool ScrollToFirstError
    {
        get => GetValue(ScrollToFirstErrorProperty);
        set => SetValue(ScrollToFirstErrorProperty, value);
    }
    
    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    public MediaBreakGridLength? LabelColInfo
    {
        get => GetValue(LabelColInfoProperty);
        set => SetValue(LabelColInfoProperty, value);
    }
    
    public MediaBreakGridLength? WrapperColInfo
    {
        get => GetValue(WrapperColInfoProperty);
        set => SetValue(WrapperColInfoProperty, value);
    }
    
    public Type ValueType
    {
        get => GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }
    
    private new IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    private new IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public event EventHandler<FormAboutToValidateEventArgs>? AboutToValdiate;
    public event EventHandler<FormValidatedEventArgs>? Valdiated;
    public event EventHandler<FormSubmittedEventArgs>? Submitted;
    public event EventHandler? ResetCompleted;

    #endregion
    
    #region 内部属性定义
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    
    private readonly Dictionary<FormItem, CompositeDisposable> _itemsBindingDisposables = new();
    #endregion
    
    static Form()
    {
        AffectsMeasure<Form>(SizeTypeProperty);
    }
    
    public Form()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }
        
    private static bool ValidateValueType(Type value)
    {
        return typeof(IFormValue).IsAssignableFrom(value);
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is FormItem formItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(formItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(formItem);
                        }
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new FormItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<FormItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is FormItem formItem)
        {
            var disposables = new CompositeDisposable(2);
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, formItem, FormItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, formItem, FormItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowColonProperty, formItem, FormItem.IsShowColonProperty));
            disposables.Add(BindUtils.RelayBind(this, RequiredMarkProperty, formItem, FormItem.RequiredMarkProperty));
            
            PrepareFormItem(formItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(formItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(formItem);
            }
            _itemsBindingDisposables.Add(formItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type FormItem.");
        }
    }

    protected virtual void PrepareFormItem(FormItem formItem, object? item, int index, CompositeDisposable disposables)
    {
    }
    
    public void Validate()
    {
        
    }

    public void Submit()
    {
        
    }

    public void Reset()
    {
        
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SyncConfigToItems();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LabelAlignProperty ||
            change.Property == LabelWrappingProperty ||
            change.Property == FormLayoutProperty)
        {
            SyncConfigToItems();
        }
    }

    private void SyncConfigToItems()
    {
        for (int i = 0; i < ItemCount; i++)
        {
            var item = Items[i];
            if (item is FormItem formItem)
            {
                SyncConfigToItem(formItem);
            }
        }
    }

    private void SyncConfigToItem(FormItem formItem)
    {
        if (!formItem.IsSet(FormItem.LabelWrappingProperty))
        {
            formItem.SetCurrentValue(FormItem.LabelWrappingProperty, LabelWrapping);
        }

        if (!formItem.IsSet(FormItem.LabelAlignProperty))
        {
            formItem.SetCurrentValue(FormItem.LabelAlignProperty, LabelAlign);
        }
        
        if (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Inline)
        {
            formItem.SetCurrentValue(FormItem.LayoutProperty, FormItemLayout.Horizontal);
        }
        else
        {
            formItem.SetCurrentValue(FormItem.LayoutProperty, FormItemLayout.Vertical);
        }
    }
}