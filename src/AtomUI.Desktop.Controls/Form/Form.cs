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
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
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

public enum FormValidateStrategy
{
    StopWhenFirstFailed,
    Sequential,
    Parallel
}

public enum FormValidateStatus
{
    Default,
    Success,
    Warning,
    Error,
    Validating
}

public class Form : ItemsControl,
                    ISizeTypeAware,
                    IMotionAwareControl,
                    IControlSharedTokenResourcesHost,
                    IForm,
                    IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Form, bool>(
            nameof(IsShowColon), true);
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AvaloniaProperty.Register<Form, FormRequiredMark>(nameof(RequiredMark), FormRequiredMark.Default);
    
    public static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AvaloniaProperty.Register<Form, bool>(
            nameof(IsValidateFeedbackEnabled), false);
    
    public static readonly StyledProperty<object?> SuccessFeedbackProperty =
        FormValidateFeedback.SuccessFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> SuccessFeedbackTemplateProperty =
        FormValidateFeedback.SuccessFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> WarningFeedbackProperty =
        FormValidateFeedback.WarningFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> WarningFeedbackTemplateProperty =
        FormValidateFeedback.WarningFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> ErrorFeedbackProperty =
        FormValidateFeedback.ErrorFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> ErrorFeedbackTemplateProperty =
        FormValidateFeedback.ErrorFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> ValidatingFeedbackProperty =
        FormValidateFeedback.ValidatingFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> ValidatingFeedbackTemplateProperty =
        FormValidateFeedback.ValidatingFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> FeedbackTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(FeedbackTemplate));
    
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        AvaloniaProperty.Register<Form, FormLabelAlign>(nameof(LabelAlign), FormLabelAlign.Right);
    
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        AvaloniaProperty.Register<Form, TextWrapping>(nameof(LabelWrapping), TextWrapping.NoWrap);
    
    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<Form, FormLayout>(nameof(FormLayout));
    
    public static readonly StyledProperty<object?> CustomRequireMarkProperty =
        AvaloniaProperty.Register<Form, object?>(nameof(CustomRequireMark));
    
    public static readonly StyledProperty<IDataTemplate?> CustomRequireMarkTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(CustomRequireMarkTemplate));
    
    public static readonly StyledProperty<object?> CustomOptionalMarkProperty =
        AvaloniaProperty.Register<Form, object?>(nameof(CustomOptionalMark));
    
    public static readonly StyledProperty<IDataTemplate?> CustomOptionalMarkTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(CustomOptionalMarkTemplate));
    
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
    
    public static readonly DirectProperty<Form, bool?> IsFormValidProperty =
        AvaloniaProperty.RegisterDirect<Form, bool?>(
            nameof(IsFormValid),
            o => o.IsFormValid);
    
    public static readonly StyledProperty<IBrush?> ErrorMessageForegroundProperty =
        AvaloniaProperty.Register<Form, IBrush?>(nameof(ErrorMessageForeground));
    
    public static readonly StyledProperty<IBrush?> WarningMessageForegroundProperty =
        AvaloniaProperty.Register<Form, IBrush?>(nameof(WarningMessageForeground));
    
    public static readonly StyledProperty<int> MinItemCountProperty =
        AvaloniaProperty.Register<Form, int>(nameof(MinItemCount), 1);
    
    public static readonly StyledProperty<IIconTemplate?> ItemDeleteButtonIconProperty =
        AvaloniaProperty.Register<Form, IIconTemplate?>(nameof(ItemDeleteButtonIcon));
    
    public static readonly DirectProperty<Form, bool> IsShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsShowItemDeleteButton),
            o => o.IsShowItemDeleteButton,
            (o, v) => o.IsShowItemDeleteButton = v);
    
    public static readonly DirectProperty<Form, bool> IsHideItemLabelProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsHideItemLabel),
            o => o.IsHideItemLabel,
            (o, v) => o.IsHideItemLabel = v);
    
    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }
    
    public InputControlStyleVariant StyleVariant
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
    
    public bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }
    
    [DependsOn(nameof(SuccessFeedbackTemplate))]
    public object? SuccessFeedback
    {
        get => GetValue(SuccessFeedbackProperty);
        set => SetValue(SuccessFeedbackProperty, value);
    }
    
    public IDataTemplate? SuccessFeedbackTemplate
    {
        get => GetValue(SuccessFeedbackTemplateProperty);
        set => SetValue(SuccessFeedbackTemplateProperty, value);
    }
    
    [DependsOn(nameof(WarningFeedbackTemplate))]
    public object? WarningFeedback
    {
        get => GetValue(WarningFeedbackProperty);
        set => SetValue(WarningFeedbackProperty, value);
    }
    
    public IDataTemplate? WarningFeedbackTemplate
    {
        get => GetValue(WarningFeedbackTemplateProperty);
        set => SetValue(WarningFeedbackTemplateProperty, value);
    }
    
    [DependsOn(nameof(ErrorFeedbackTemplate))]
    public object? ErrorFeedback
    {
        get => GetValue(ErrorFeedbackProperty);
        set => SetValue(ErrorFeedbackProperty, value);
    }

    public IDataTemplate? ErrorFeedbackTemplate
    {
        get => GetValue(ErrorFeedbackTemplateProperty);
        set => SetValue(ErrorFeedbackTemplateProperty, value);
    }
    
    [DependsOn(nameof(ValidatingFeedbackTemplate))]
    public object? ValidatingFeedback
    {
        get => GetValue(ValidatingFeedbackProperty);
        set => SetValue(ValidatingFeedbackProperty, value);
    }

    public IDataTemplate? ValidatingFeedbackTemplate
    {
        get => GetValue(ValidatingFeedbackTemplateProperty);
        set => SetValue(ValidatingFeedbackTemplateProperty, value);
    }
    
    public IDataTemplate? FeedbackTemplate
    {
        get => GetValue(FeedbackTemplateProperty);
        set => SetValue(FeedbackTemplateProperty, value);
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
    
    [DependsOn(nameof(CustomRequireMarkTemplate))]
    public object? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    public IDataTemplate? CustomRequireMarkTemplate
    {
        get => GetValue(CustomRequireMarkTemplateProperty);
        set => SetValue(CustomRequireMarkTemplateProperty, value);
    }
    
    [DependsOn(nameof(CustomOptionalMarkTemplate))]
    public object? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    public IDataTemplate? CustomOptionalMarkTemplate
    {
        get => GetValue(CustomOptionalMarkTemplateProperty);
        set => SetValue(CustomOptionalMarkTemplateProperty, value);
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
    
    private bool? _isFormValid;

    public bool? IsFormValid
    {
        get => _isFormValid;
        private set => SetAndRaise(IsFormValidProperty, ref _isFormValid, value);
    }
    
    public IBrush? ErrorMessageForeground
    {
        get => GetValue(ErrorMessageForegroundProperty);
        set => SetValue(ErrorMessageForegroundProperty, value);
    }
    
    public IBrush? WarningMessageForeground
    {
        get => GetValue(WarningMessageForegroundProperty);
        set => SetValue(WarningMessageForegroundProperty, value);
    }
    
    /// <summary>
    /// 表单中最少的数据 Item 的数量，用于控制是否显示删除 Item 的按钮
    /// </summary>
    public int MinItemCount
    {
        get => GetValue(MinItemCountProperty);
        set => SetValue(MinItemCountProperty, value);
    }
    
    public IIconTemplate? ItemDeleteButtonIcon
    {
        get => GetValue(ItemDeleteButtonIconProperty);
        set => SetValue(ItemDeleteButtonIconProperty, value);
    }
    
    private bool _isShowItemDeleteButton;

    public bool IsShowItemDeleteButton
    {
        get => _isShowItemDeleteButton;
        set => SetAndRaise(IsShowItemDeleteButtonProperty, ref _isShowItemDeleteButton, value);
    }
    
    private bool _isHideItemLabel;

    public bool IsHideItemLabel
    {
        get => _isHideItemLabel;
        set => SetAndRaise(IsHideItemLabelProperty, ref _isHideItemLabel, value);
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
    
    internal static readonly DirectProperty<Form, bool> IsEffectiveShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsEffectiveShowItemDeleteButton),
            o => o.IsEffectiveShowItemDeleteButton,
            (o, v) => o.IsEffectiveShowItemDeleteButton = v);
    
    internal static readonly StyledProperty<double> FormLayoutSpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(FormLayoutSpacing));
    
    private Orientation _formLayoutOrientation;

    internal Orientation FormLayoutOrientation
    {
        get => _formLayoutOrientation;
        set => SetAndRaise(FormLayoutOrientationProperty, ref _formLayoutOrientation, value);
    }
    
    private bool _isEffectiveShowItemDeleteButton;

    internal bool IsEffectiveShowItemDeleteButton
    {
        get => _isEffectiveShowItemDeleteButton;
        set => SetAndRaise(IsEffectiveShowItemDeleteButtonProperty, ref _isEffectiveShowItemDeleteButton, value);
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
    private CancellationTokenSource? _validationTokenSource;
    
    static Form()
    {
        AffectsMeasure<Form>(SizeTypeProperty);
        AffectsMeasure<Form>(FormLayoutOrientationProperty);
        SubmitButton.SubmitEvent.AddClassHandler<Form>((form, args) => form.HandleSubmitButtonClick(args));
        ResetButton.ResetEvent.AddClassHandler<Form>((form, args) => form.HandleResetButtonClick(args));
        FormItem.ValueChangedEvent.AddClassHandler<Form>((form, args) => form.HandleFormItemValueChanged(args));
        FormItem.ValidateChangedEvent.AddClassHandler<Form>((form, args) => form.HandleFormItemValidateChanged(args));
        FormItem.DeleteRequestEvent.AddClassHandler<Form>((form, args) => form.HandleFormItemDelete(args));
    }
    
    public Form()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        Items.CollectionChanged += (sender, args) =>
        {
            InvalidateMeasure();
        };
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

        ConfigureShowItemDeleteButton();
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new FormItem
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
            var disposables = new CompositeDisposable(16);
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, formItem, FormItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, formItem, FormItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, ErrorMessageForegroundProperty, formItem, FormItem.ErrorMessageForegroundProperty));
            disposables.Add(BindUtils.RelayBind(this, WarningMessageForegroundProperty, formItem, FormItem.WarningMessageForegroundProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowColonProperty, formItem, FormItem.IsShowColonProperty));
            disposables.Add(BindUtils.RelayBind(this, RequiredMarkProperty, formItem, FormItem.RequiredMarkProperty));
            disposables.Add(BindUtils.RelayBind(this, LabelWrappingProperty, formItem, FormItem.LabelWrappingProperty));
            disposables.Add(BindUtils.RelayBind(this, StyleVariantProperty, formItem, FormItem.StyleVariantProperty));
            disposables.Add(BindUtils.RelayBind(this, CustomRequireMarkProperty, formItem, FormItem.CustomRequireMarkProperty));
            disposables.Add(BindUtils.RelayBind(this, CustomRequireMarkTemplateProperty, formItem, FormItem.CustomRequireMarkTemplateProperty));
            disposables.Add(BindUtils.RelayBind(this, CustomOptionalMarkProperty, formItem, FormItem.CustomOptionalMarkProperty));
            disposables.Add(BindUtils.RelayBind(this, CustomOptionalMarkTemplateProperty, formItem, FormItem.CustomOptionalMarkTemplateProperty));
            disposables.Add(BindUtils.RelayBind(this, LabelColInfoProperty, formItem, FormItem.LabelColInfoProperty));
            disposables.Add(BindUtils.RelayBind(this, WrapperColInfoProperty, formItem, FormItem.WrapperColInfoProperty));
            disposables.Add(BindUtils.RelayBind(this, FeedbackTemplateProperty, formItem, FormItem.FeedbackTemplateProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowItemDeleteButtonProperty, formItem, FormItem.IsShowItemDeleteButtonProperty));
            disposables.Add(BindUtils.RelayBind(this, IsEffectiveShowItemDeleteButtonProperty, formItem, FormItem.IsEffectiveShowItemDeleteButtonProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemDeleteButtonIconProperty, formItem, FormItem.ItemDeleteButtonIconTemplateProperty));
            disposables.Add(BindUtils.RelayBind(this, FormLayoutProperty, formItem, FormItem.FormLayoutProperty));
            disposables.Add(BindUtils.RelayBind(this, IsHideItemLabelProperty, formItem, FormItem.IsHideItemLabelProperty));
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
        _validationTokenSource?.Cancel();
        _validationTokenSource = new CancellationTokenSource();
        var cancellationToken = _validationTokenSource.Token;
        Dispatcher.UIThread.InvokeAsync(() => ValidateAsync(cancellationToken));
    }

    public async Task<FormValidateResult> ValidateAsync(CancellationToken cancellationToken)
    {
        AboutToValidate?.Invoke(this, EventArgs.Empty);
        var tasks = new List<Task>();
        foreach (var item in Items)
        {
            if (item is FormItem formItem && !formItem.IsSkipValidate())
            {
                tasks.Add(formItem.ValidateValueAsync(cancellationToken));
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
                if (formItem.ValidateResult != FormValidateResult.Success)
                {
                    if (formItem.ValidateErrorMessages != null)
                    {
                        foreach (var errorMessage in formItem.ValidateErrorMessages)
                        {
                            validateMessages.Add(new FormValidateMessage(formItem.FieldName ?? string.Empty, errorMessage, FormValidateResult.Error));
                        }
                    }
                    if (formItem.ValidateWarningMessages != null)
                    {
                        foreach (var warningMessage in formItem.ValidateWarningMessages)
                        {
                            validateMessages.Add(new FormValidateMessage(formItem.FieldName ?? string.Empty, warningMessage, FormValidateResult.Warning));
                        }
                    }
                }
            }
        }

        var validateResult = FormValidateResult.Success;
        if (results.Any(item => item == FormValidateResult.Error))
        {
            validateResult = FormValidateResult.Error;
        }
        else
        {
            validateResult = FormValidateResult.Success;
        }

        if (validateResult == FormValidateResult.Error)
        {
            Validated?.Invoke(this, new FormValidatedEventArgs(validateResult, null, validateMessages));
            return FormValidateResult.Error;
        }
        return FormValidateResult.Success;
    }

    public void SetFormValues(FormValues formValues)
    {
        foreach (var item in Items)
        {
            if (item is FormItem formItem && formItem.IsValueItem)
            {
                var fieldName = formItem.FieldName;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    if (formValues.TryGetValue(fieldName, out var value))
                    {
                        formItem.SetItemValue(value);
                    }
                }
            }
        }
    }

    public void Submit()
    {
        _validationTokenSource?.Cancel();
        _validationTokenSource = new CancellationTokenSource();
        var cancellationToken = _validationTokenSource.Token;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var result = await ValidateAsync(cancellationToken);
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
            IsFormValid = false;
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
            change.Property == FormLayoutProperty ||
            change.Property == ValidateTriggerProperty)
        {
            SyncConfigToItems();
        }

        if (change.Property == FormLayoutProperty)
        {
            ConfigureFormLayoutOrientation();
        }
        else if (change.Property == IsShowItemDeleteButtonProperty)
        {
            ConfigureShowItemDeleteButton();
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
        formItem.SetValue(FormItem.LabelAlignProperty, LabelAlign, BindingPriority.Style);
        formItem.SetValue(FormItem.IsValidateFeedbackEnabledProperty, IsValidateFeedbackEnabled, BindingPriority.Style);
        formItem.SetValue(FormItem.ValidateTriggerProperty, ValidateTrigger, BindingPriority.Style);
        
        if (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Inline)
        {
            formItem.SetValue(FormItem.LayoutProperty, FormItemLayout.Horizontal, BindingPriority.Style);
        }
        else
        {
            formItem.SetValue(FormItem.LayoutProperty, FormItemLayout.Vertical, BindingPriority.Style);
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
    
    private void HandleFormItemValidateChanged(FormItemValidateChangedEventArgs args)
    {
        var formItem = args.Source as IFormItem;
        Debug.Assert(formItem != null);
        if (args.Status == FormValidateStatus.Error)
        {
            IsFormValid = false;
            return;
        }

        var isValid = true;
        foreach (var item in Items)
        {
            if (item is FormItem childFormItem && childFormItem.IsValueItem)
            {
                if (childFormItem.ValidateStatus == FormValidateStatus.Error ||
                    childFormItem.ValidateStatus == FormValidateStatus.Validating ||
                    childFormItem.ValidateStatus == FormValidateStatus.Default)
                {
                    isValid = false;
                    break;
                }
            }
        }
        IsFormValid = isValid;
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

    public void DeleteFormItem(FormItem formItem)
    {
        Items.Remove(formItem);
    }

    private void HandleFormItemDelete(RoutedEventArgs args)
    {
        if (args.Source is FormItem formItem)
        {
            DeleteFormItem(formItem);
        }
    }

    private void ConfigureShowItemDeleteButton()
    {
        var count = 0;
        foreach (var item in LogicalChildren)
        {
            if (item is FormItem formItem && formItem.IsValueItem)
            {
                count++;
            }
        }
        IsEffectiveShowItemDeleteButton = IsShowItemDeleteButton && count > MinItemCount;
    }
}