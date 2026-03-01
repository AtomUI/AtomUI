using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

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
    
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        AvaloniaProperty.Register<Form, FormLabelAlign>(nameof(LabelAlign), FormLabelAlign.Right);
    
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        AvaloniaProperty.Register<Form, TextWrapping>(nameof(LabelWrapping), TextWrapping.NoWrap);
    
    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<Form, FormLayout>(nameof(FormLayout));
    
    public static readonly StyledProperty<IControlTemplate?> CustomRequireMarkProperty =
        AvaloniaProperty.Register<Form, IControlTemplate?>(nameof(CustomRequireMark));
    
    public static readonly StyledProperty<IControlTemplate?> CustomOptionalMarkProperty =
        AvaloniaProperty.Register<Form, IControlTemplate?>(nameof(CustomOptionalMark));
    
    public static readonly StyledProperty<bool> ScrollToFirstErrorProperty =
        AvaloniaProperty.Register<Form, bool>(nameof(ScrollToFirstError));
    
    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AvaloniaProperty.Register<Form, FormValidateTrigger>(nameof(ValidateTrigger), FormValidateTrigger.OnChanged);
    
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(LabelColInfo));
    
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(WrapperColInfo));
    
    public static readonly DirectProperty<Form, IFormValues?> ValuesProperty =
        AvaloniaProperty.RegisterDirect<Form, IFormValues?>(
            nameof(Values),
            o => o.Values);
    
    public static readonly DirectProperty<Form, IFormValues?> InitialValuesProperty =
        AvaloniaProperty.RegisterDirect<Form, IFormValues?>(
            nameof(InitialValues),
            o => o.InitialValues,
            (o, v) => o.InitialValues = v);
    
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
    
    private IFormValues? _values;

    public IFormValues? Values
    {
        get => _values;
        private set => SetAndRaise(ValuesProperty, ref _values, value);
    }
    
    private IFormValues? _initialValues;

    public IFormValues? InitialValues
    {
        get => _initialValues;
        set => SetAndRaise(InitialValuesProperty, ref _initialValues, value);
    }
    
    #endregion

    #region 公共事件定义

    public event EventHandler<EventArgs>? AboutToValidate;
    public event EventHandler<FormValidatedEventArgs>? Validated;
    public event EventHandler<FormSubmittedEventArgs>? Submitted;
    public event EventHandler<FormItemValueChangedEventArgs>? ItemValueChanged;
    public event EventHandler? ResetCompleted;
    
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<Form, Orientation> FormLayoutOrientationProperty =
        AvaloniaProperty.RegisterDirect<Form, Orientation>(
            nameof(FormLayoutOrientation),
            o => o.FormLayoutOrientation);
    
    internal static readonly StyledProperty<double> FormLayoutSpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(FormLayoutSpacing));
    
    private Orientation _formLayoutOrientation;

    internal Orientation FormLayoutOrientation
    {
        get => _formLayoutOrientation;
        set => SetAndRaise(FormLayoutOrientationProperty, ref _formLayoutOrientation, value);
    }
    
    internal double FormLayoutSpacing
    {
        get => GetValue(FormLayoutSpacingProperty);
        set => SetValue(FormLayoutSpacingProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    
    private readonly Dictionary<FormItem, CompositeDisposable> _itemsBindingDisposables = new();
    #endregion

    private bool _initValueApplied;
    internal bool IsResetting;
    
    static Form()
    {
        AffectsMeasure<Form>(SizeTypeProperty);
        SubmitButton.SubmitEvent.AddClassHandler<Form>((form, args) => form.HandleSubmitButtonClick(args));
        ResetButton.ResetEvent.AddClassHandler<Form>((form, args) => form.HandleResetButtonClick(args));
        FormItem.ValueChangedEvent.AddClassHandler<Form>((form, args) => form.HandleFormItemValueChanged(args));
    }
    
    public Form()
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
        return new FormItem()
        {
            OwnerForm = this
        };
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
            formItem.OwnerForm = this;
            var disposables = new CompositeDisposable(2);
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, formItem, FormItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, formItem, FormItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowColonProperty, formItem, FormItem.IsShowColonProperty));
            disposables.Add(BindUtils.RelayBind(this, RequiredMarkProperty, formItem, FormItem.RequiredMarkProperty));
            disposables.Add(BindUtils.RelayBind(this, ValidateTriggerProperty, formItem, FormItem.ValidateTriggerProperty));
            
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
        Dispatcher.UIThread.InvokeAsync(async () => ValidateAsync());
    }

    public async Task<FormValidateResult> ValidateAsync()
    {
        AboutToValidate?.Invoke(this, EventArgs.Empty);
        var tasks = new List<Task>();
        foreach (var item in Items)
        {
            if (item is FormItem formItem && !formItem.IsSkipValidate())
            {
                tasks.Add(formItem.ValidateValueAsync());
            }
        }
        await Task.WhenAll(tasks);
        var results          = new List<FormValidateResult>();
        var validateMessages = new List<FormValidateMessage>();
        foreach (var item in Items)
        {
            if (item is FormItem formItem && !formItem.IsSkipValidate())
            {
                results.Add(formItem.ValidateResult);
                if (formItem.ValidateResult == FormValidateResult.Error ||
                    formItem.ValidateResult == FormValidateResult.Warning)
                {
                    validateMessages.Add(new FormValidateMessage(formItem.FieldName ?? string.Empty, formItem.ValidateMsg ?? string.Empty));
                }
            }
        }

        var validateResult = FormValidateResult.Success;
        if (results.Any(item => item == FormValidateResult.Error))
        {
            validateResult = FormValidateResult.Error;
        }

        else if (results.Any(item => item == FormValidateResult.Warning))
        {
            validateResult = FormValidateResult.Warning;
        }
        else
        {
            validateResult = FormValidateResult.Success;
        }

        if (validateResult == FormValidateResult.Error || validateResult == FormValidateResult.Warning)
        {
            Validated?.Invoke(this, new FormValidatedEventArgs(validateResult, null, validateMessages));
        }
        return FormValidateResult.Success;
    }

    public void Submit()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var result = await ValidateAsync();
            if (result == FormValidateResult.Success)
            {
                var values = new FormValues();
                // 收集值
                CollectValues(values);
                Validated?.Invoke(this, new FormValidatedEventArgs(result, values, null));
                NotifySubmit(values);
                Submitted?.Invoke(this, new FormSubmittedEventArgs(values));
            }
        });
    }

    protected virtual void NotifySubmit(FormValues values)
    {
    }

    public void Reset()
    {
        try
        {
            IsResetting = true;
            foreach (var item in Items)
            {
                if (item is FormItem formItem)
                {
                    formItem.ResetItemValue();
                }
            }

            NotifyReset();
            ResetCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsResetting = false;
            });
        }
    }

    protected virtual void NotifyReset()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SyncConfigToItems();
        ConfigureFormLayoutOrientation();
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

        if (change.Property == FormLayoutProperty)
        {
            ConfigureFormLayoutOrientation();
        }
        
    }

    private void SyncConfigToItems()
    {
        foreach (var item in Items)
        {
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
            if (!formItem.IsSet(FormItem.LayoutProperty))
            {
                formItem.SetCurrentValue(FormItem.LayoutProperty, FormItemLayout.Horizontal);
            }
        }
        else
        {
            if (!formItem.IsSet(FormItem.LayoutProperty))
            {
                formItem.SetCurrentValue(FormItem.LayoutProperty, FormItemLayout.Vertical);
            }
        }
    }

    private void ConfigureFormLayoutOrientation()
    {
        FormLayoutOrientation = (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Vertical) ? Orientation.Vertical : Orientation.Horizontal;
    }

    private void HandleSubmitButtonClick(RoutedEventArgs args)
    {
        Submit();
        args.Handled = true;
    }

    private void HandleResetButtonClick(RoutedEventArgs args)
    {
        Reset();
        args.Handled = true;
    }

    private void HandleFormItemValueChanged(RoutedEventArgs args)
    {
        var formItem = args.Source as IFormItem;
        Debug.Assert(formItem != null);
        ItemValueChanged?.Invoke(this, new FormItemValueChangedEventArgs(formItem, formItem.GetItemValue()));
        args.Handled = true;
        foreach (var item in Items)
        {
            if (item is FormItem childFormItem && childFormItem != formItem)
            {
                childFormItem.NotifyFormItemChanged(formItem);
            }
        }
    }

    private void CollectValues(FormValues formValues)
    {
        foreach (var item in Items)
        {
            if (item is FormItem formItem)
            {
                var fieldName = formItem.FieldName;
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    continue;
                }
                formValues.Add(fieldName, formItem.GetItemValue());
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_initialValues == null)
        {
            return;
        }
        if (!_initValueApplied)
        {
            foreach (var item in Items)
            {
                if (item is FormItem formItem)
                {
                    var fieldName = formItem.FieldName;
                    if (string.IsNullOrWhiteSpace(fieldName))
                    {
                        continue;
                    }

                    if (_initialValues.ContainsKey(fieldName))
                    {
                        formItem.SetItemValue(_initialValues[fieldName]);
                    }
                }
            }
            _initValueApplied = true;
        }
    }
}