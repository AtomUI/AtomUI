using System.Diagnostics;
using System.Reactive.Disposables;
using System.Text;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

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

public enum FormItemLayout
{
    Horizontal,
    Vertical
}

public class FormItem : TemplatedControl,
                        IControlSharedTokenResourcesHost,
                        IFormItem
{
    #region 公共属性定义
    
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<FormItem, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<FormItem, IDataTemplate?>(nameof(ExtraTemplate));
    
    public static readonly StyledProperty<string?> HelpProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Help));
    
    public static readonly StyledProperty<bool> HasFeedbackProperty =
        AvaloniaProperty.Register<FormItem, bool>(nameof(HasFeedback));
    
    public static readonly StyledProperty<IconTemplate?> ErrorFeedbackIconProperty =
        Form.ErrorFeedbackIconProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<IconTemplate?> WarningFeedbackIconProperty =
        Form.WarningFeedbackIconProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<object?> InitialValueProperty =
        AvaloniaProperty.Register<FormItem, object?>(nameof(InitialValue));
    
    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(LabelText));
    
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        Form.LabelAlignProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        Form.LabelWrappingProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        Form.LabelColInfoProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        Form.WrapperColInfoProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<string?> FieldNameProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(FieldName));
    
    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<FormItem, bool>(nameof(IsRequired));
    
    public static readonly StyledProperty<IList<IFormValidator>?> ValidatorsProperty =
        AvaloniaProperty.Register<FormItem, IList<IFormValidator>?>(nameof(Validators));
    
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Tooltip));
    
    public static readonly StyledProperty<PathIcon?> TooltipIconProperty =
        AvaloniaProperty.Register<FormItem, PathIcon?>(nameof(TooltipIcon));
    
    public static readonly StyledProperty<TimeSpan?> ValidateDebounceProperty =
        AvaloniaProperty.Register<FormItem, TimeSpan?>(nameof(ValidateDebounce));
    
    public static readonly StyledProperty<FormValidateStrategy> ValidateStrategyProperty =
        AvaloniaProperty.Register<FormItem, FormValidateStrategy>(nameof(ValidateStrategy), FormValidateStrategy.Sequential);
    
    public static readonly DirectProperty<FormItem, FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.RegisterDirect<FormItem, FormValidateStatus>(
            nameof(ValidateStatus),
            o => o.ValidateStatus);
    
    public static readonly DirectProperty<FormItem, FormValidateResult> ValidateResultProperty =
        AvaloniaProperty.RegisterDirect<FormItem, FormValidateResult>(
            nameof(ValidateResult),
            o => o.ValidateResult);
    
    public static readonly StyledProperty<FormItemLayout> LayoutProperty =
        AvaloniaProperty.Register<FormItem, FormItemLayout>(nameof(Layout), FormItemLayout.Horizontal);
    
    public static readonly StyledProperty<double> ChildrenSpacingProperty =
        AvaloniaProperty.Register<FormItem, double>(nameof(ChildrenSpacing));
    
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<FormItem, Control?>(nameof(Content));
    
    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
    
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }
    
    public string? Help
    {
        get => GetValue(HelpProperty);
        set => SetValue(HelpProperty, value);
    }
    
    public bool HasFeedback
    {
        get => GetValue(HasFeedbackProperty);
        set => SetValue(HasFeedbackProperty, value);
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
    
    public object? InitialValue
    {
        get => GetValue(InitialValueProperty);
        set => SetValue(InitialValueProperty, value);
    }
    
    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
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

    public string? FieldName
    {
        get => GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }
    
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }
    
    public IList<IFormValidator>? Validators
    {
        get => GetValue(ValidatorsProperty);
        set => SetValue(ValidatorsProperty, value);
    }
    
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    public PathIcon? TooltipIcon
    {
        get => GetValue(TooltipIconProperty);
        set => SetValue(TooltipIconProperty, value);
    }
    
    public TimeSpan? ValidateDebounce
    {
        get => GetValue(ValidateDebounceProperty);
        set => SetValue(ValidateDebounceProperty, value);
    }
    
    public FormValidateStrategy ValidateStrategy
    {
        get => GetValue(ValidateStrategyProperty);
        set => SetValue(ValidateStrategyProperty, value);
    }
    
    private FormValidateStatus _validateStatus = FormValidateStatus.Default;

    public FormValidateStatus ValidateStatus
    {
        get => _validateStatus;
        set => SetAndRaise(ValidateStatusProperty, ref _validateStatus, value);
    }
    
    private FormValidateResult _validateResult = FormValidateResult.Success;

    public FormValidateResult ValidateResult
    {
        get => _validateResult;
        private set => SetAndRaise(ValidateResultProperty, ref _validateResult, value);
    }
    
    public FormItemLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }
    
    public double ChildrenSpacing
    {
        get => GetValue(ChildrenSpacingProperty);
        set => SetValue(ChildrenSpacingProperty, value);
    }
    
    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<bool> IsShowColonProperty =
        Form.IsShowColonProperty.AddOwner<FormItem>();

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> IsColonVisibleProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsColonVisible),
            o => o.IsColonVisible,
            (o, v) => o.IsColonVisible = v);
    
    internal static readonly DirectProperty<FormItem, bool> IsValueItemProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsValueItem),
            o => o.IsValueItem);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        Form.RequiredMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        Form.ValidateTriggerProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, string?> ValidateMsgProperty =
        AvaloniaProperty.RegisterDirect<FormItem, string?>(
            nameof(ValidateMsg),
            o => o.ValidateMsg,
            (o, v) => o.ValidateMsg = v);
    
    internal static readonly StyledProperty<object?> CustomRequireMarkProperty =
        Form.CustomRequireMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> CustomRequireMarkTemplateProperty =
        Form.CustomRequireMarkTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<object?> CustomOptionalMarkProperty =
        Form.CustomOptionalMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> CustomOptionalMarkTemplateProperty =
        Form.CustomOptionalMarkTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> HasCustomRequireMarkProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasCustomRequireMark),
            o => o.HasCustomRequireMark,
            (o, v) => o.HasCustomRequireMark = v);
    
    internal static readonly DirectProperty<FormItem, bool> HasCustomOptionalMarkProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasCustomOptionalMark),
            o => o.HasCustomOptionalMark,
            (o, v) => o.HasCustomOptionalMark = v);
    
    internal bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }
    
    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isColonVisible;

    internal bool IsColonVisible
    {
        get => _isColonVisible;
        set => SetAndRaise(IsColonVisibleProperty, ref _isColonVisible, value);
    }
    
    private bool _isValueItem;
    
    // 如果这个 FormItem 设置了 Label 和 FieldName 那么就是一个 ValueItem
    internal bool IsValueItem
    {
        get => _isValueItem;
        set => SetAndRaise(IsValueItemProperty, ref _isValueItem, value);
    }
    
    internal FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    internal FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    private string? _validateMsg;

    internal string? ValidateMsg
    {
        get => _validateMsg;
        set => SetAndRaise(ValidateMsgProperty, ref _validateMsg, value);
    }
    
    [DependsOn(nameof(CustomRequireMarkTemplate))]
    internal object? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    internal IDataTemplate? CustomRequireMarkTemplate
    {
        get => GetValue(CustomRequireMarkTemplateProperty);
        set => SetValue(CustomRequireMarkTemplateProperty, value);
    }
    
    [DependsOn(nameof(CustomOptionalMarkTemplate))]
    internal object? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    internal IDataTemplate? CustomOptionalMarkTemplate
    {
        get => GetValue(CustomOptionalMarkTemplateProperty);
        set => SetValue(CustomOptionalMarkTemplateProperty, value);
    }
    
    private bool _hasCustomRequireMark;
    
    internal bool HasCustomRequireMark
    {
        get => _hasCustomRequireMark;
        set => SetAndRaise(HasCustomRequireMarkProperty, ref _hasCustomRequireMark, value);
    }
    
    private bool _hasCustomOptionalMark;
    
    internal bool HasCustomOptionalMark
    {
        get => _hasCustomOptionalMark;
        set => SetAndRaise(HasCustomOptionalMarkProperty, ref _hasCustomOptionalMark, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    #endregion

    #region 内部事件定义

    internal static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<FormItem, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);
    
    internal event EventHandler<RoutedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    #endregion
    
    protected override Type StyleKeyOverride => typeof(FormItem);

    private Grid? _rootLayout;
    private Panel? _childrenLayout;
    private Panel? _labelLayout;
    private MediaBreakPoint? _breakPoint;
    private CompositeDisposable? _disposables;

    internal Form? OwnerForm;
    
    static FormItem()
    {
        AffectsMeasure<FormItem>(SizeTypeProperty);
    }
    
    public FormItem()
    {
        this.RegisterResources();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TooltipIcon == null)
        {
            SetCurrentValue(TooltipIconProperty, new QuestionCircleOutlined());
        }
    }

    protected virtual void NotifyContentAdded(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.OldValue is IFormItemAware oldFormItemAware)
        {
            oldFormItemAware.ValueChanged -= HandleContentValueChanged;
        }
        
        if (change.NewValue is IFormItemAware newFormItemAware)
        {
            newFormItemAware.ValueChanged += HandleContentValueChanged;
        }
        else
        {
            throw new Exception($"Form item content: {change.NewValue?.GetType().FullName} not implement IFormItemAware interface");
        }
        
        _disposables?.Dispose();
        _disposables = new CompositeDisposable(2);
        if (Content is ISizeTypeAware)
        {
            _disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, Content, SizeTypeProperty));
        }

        if (Content is IInputControlStyleVariantAware)
        {
            _disposables.Add(BindUtils.RelayBind(this, StyleVariantProperty, Content, StyleVariantProperty));
        }
    }

    private void HandleContentValueChanged(object? sender, EventArgs e)
    {
        Debug.Assert(OwnerForm != null);
        if (!OwnerForm.IsResetting && ValidateTrigger == FormValidateTrigger.OnChanged)
        {
            Dispatcher.UIThread.InvokeAsync(async () => ValidateValueAsync());
        }
        RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
    }

    public async Task ValidateValueAsync()
    {
        if (Content == null || Validators == null || Validators.Count == 0)
        {
            return;
        }
        ValidateStatus = FormValidateStatus.Validating;
        var formItemAware     = Content as IFormItemAware;
        Debug.Assert(formItemAware != null);
        var value             = formItemAware.GetFormValue();
        var warningMsgBuilder = new StringBuilder();
        var hasWarning        = false;
        
        if (ValidateStrategy == FormValidateStrategy.Parallel || ValidateStrategy == FormValidateStrategy.Sequential)
        {
            var hasError        = false;
            var tasks           = new Dictionary<Task<FormValidateResult>, IFormValidator>();
            var errorMsgBuilder = new StringBuilder();
            if (ValidateStrategy == FormValidateStrategy.Parallel)
            {
                foreach (var validator in Validators)
                {
                    var task = validator.ValidateAsync(FieldName ?? string.Empty, value);
                    tasks.Add(task, validator);
                }
                await Task.WhenAll(tasks.Keys);
                foreach (var task in tasks.Keys)
                {
                    var result    = await task;
                    var validator = tasks[task];
                    if (result == FormValidateResult.Error)
                    {
                        hasError = true;
                        errorMsgBuilder.Append(validator.Message);
                    }
                    else if (result == FormValidateResult.Warning)
                    {
                        hasWarning = true;
                        warningMsgBuilder.Append(validator.Message);
                    }
                }
            }
            else
            {
                foreach (var validator in Validators)
                {
                    var result = await validator.ValidateAsync(FieldName ?? string.Empty, value);
                    if (result == FormValidateResult.Error)
                    {
                        hasError = true;
                        errorMsgBuilder.Append(validator.Message);
                    }
                    else if (result == FormValidateResult.Warning)
                    {
                        hasWarning = true;
                        warningMsgBuilder.Append(validator.Message);
                    }
                }
            }
            
            if (hasError)
            {
                ValidateMsg = errorMsgBuilder.ToString();
                ValidateStatus = FormValidateStatus.Error;
            }
            else if (hasWarning)
            {
                ValidateMsg = warningMsgBuilder.ToString();
                ValidateStatus = FormValidateStatus.Warning;
            }
            else
            {
                ValidateMsg    = null;
                ValidateStatus = FormValidateStatus.Success;
            }
        }
        else
        {
            foreach (var validator in Validators)
            {
                var result = await validator.ValidateAsync(FieldName ?? string.Empty, value);
                if (result == FormValidateResult.Error)
                {
                    ValidateStatus = FormValidateStatus.Error;
                    ValidateMsg    = validator.Message;
                    return;
                }
                if (result == FormValidateResult.Warning)
                {
                    hasWarning = true;
                    warningMsgBuilder.Append(validator.Message);
                }
            }
            
            if  (hasWarning)
            {
                ValidateMsg = warningMsgBuilder.ToString();
                ValidateStatus = FormValidateStatus.Warning;
            }
            else
            {
                ValidateMsg    = null;
                ValidateStatus = FormValidateStatus.Success;
            }
        }

        if (ValidateStatus == FormValidateStatus.Error)
        {
            ValidateResult = FormValidateResult.Error;
        }
        else if (ValidateStatus == FormValidateStatus.Warning)
        {
            ValidateResult = FormValidateResult.Warning;
        }
        else
        {
            ValidateResult = FormValidateResult.Success;
        }

        formItemAware.NotifyValidateStatus(ValidateStatus);
    }

    public object? GetItemValue()
    {
        if (Content is IFormItemAware formItemAware)
        {
            return formItemAware.GetFormValue();
        }
        return null;
    }

    public void SetItemValue(object? value)
    {
        if (Content is IFormItemAware formItemAware)
        {
            formItemAware.SetFormValue(value);
        }
    }

    public void ResetItemValue()
    {
        if (Content is IFormItemAware formItemAware)
        { 
            formItemAware.ClearFormValue();
            formItemAware.NotifyValidateStatus(FormValidateStatus.Default);
            ValidateStatus = FormValidateStatus.Default;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowColonProperty ||
            change.Property == LabelTextProperty ||
            change.Property == LayoutProperty)
        {
            ConfigureLabelColonVisible();
        }
        
        if (change.Property == LayoutProperty)
        {
            ConfigureLayout();
        }

        if (change.Property == ContentProperty)
        {
            NotifyContentAdded(change);
        }
        else if (change.Property == FieldNameProperty ||
                 change.Property == LabelTextProperty)
        {
            IsValueItem = !string.IsNullOrWhiteSpace(FieldName) && !string.IsNullOrWhiteSpace(LabelText);
        }

        if (change.Property == CustomRequireMarkProperty)
        {
            HasCustomRequireMark = CustomRequireMark != null;
        }
        else if (change.Property == CustomOptionalMarkProperty)
        {
            HasCustomOptionalMark = CustomOptionalMark != null;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootLayout        = e.NameScope.Find<Grid>(FormItemThemeConstants.RootLayoutPart);
        _labelLayout       = e.NameScope.Find<Panel>(FormItemThemeConstants.LabelLayoutPart);
        _childrenLayout    = e.NameScope.Find<Panel>(FormItemThemeConstants.ChildrenLayoutPart);
        ConfigureLayout();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.MediaBreakPointChanged -= HandleMediaBreakChanged;
        }
    }
    
    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint = args.MediaBreakPoint;
        if (_breakPoint != null)
        {
            if (Layout == FormItemLayout.Horizontal)
            {
                ConfigureLayout();
            }
        }
    }

    private void ConfigureLabelColonVisible()
    {
        IsColonVisible = !string.IsNullOrWhiteSpace(LabelText) && IsShowColon && Layout == FormItemLayout.Horizontal;
    }

    private void ConfigureLayout()
    {
        if (_rootLayout == null || _labelLayout == null || _childrenLayout == null)
        {
            return;
        }
        if (Layout == FormItemLayout.Horizontal)
        {
            _rootLayout.RowDefinitions.Clear();
            _rootLayout.ColumnDefinitions.Clear();
            _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large, 
                LabelColInfo ?? new MediaBreakGridLength(new GridLength(1, GridUnitType.Star)))));
            _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large,
                WrapperColInfo ?? new MediaBreakGridLength(new GridLength(3, GridUnitType.Star)))));
            _rootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetRow(_labelLayout, 0);
            Grid.SetRow(_childrenLayout, 0);
            Grid.SetColumn(_labelLayout, 0);
            Grid.SetColumn(_childrenLayout, 1);
        }
        else if (Layout == FormItemLayout.Vertical)
        {
            _rootLayout.RowDefinitions.Clear();
            _rootLayout.ColumnDefinitions.Clear();
            _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _rootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            _rootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Grid.SetRow(_labelLayout, 0);
            Grid.SetRow(_childrenLayout, 1);
            Grid.SetColumn(_labelLayout, 0);
            Grid.SetColumn(_childrenLayout, 0);
        }
    }
    
    private GridLength GetGridLengthForMediaBreak(MediaBreakPoint breakPoint, MediaBreakGridLength info)
    {
        var gridLength = GridLength.Auto;
        if (breakPoint == MediaBreakPoint.ExtraSmall)
        {
            gridLength = info.ExtraSmall;
        }
        else if (breakPoint == MediaBreakPoint.Small)
        {
            gridLength = info.Small;
        }
        else if (breakPoint == MediaBreakPoint.Medium)
        {
            gridLength = info.Medium;
        }
        else if (breakPoint == MediaBreakPoint.Large)
        {
            gridLength = info.Large;
        }
        else if (breakPoint == MediaBreakPoint.ExtraLarge)
        {
            gridLength = info.ExtraLarge;
        }
        else if (breakPoint == MediaBreakPoint.ExtraExtraLarge)
        {
            gridLength = info.ExtraExtraLarge;
        }

        return gridLength;
    }
    
    internal virtual bool IsSkipValidate()
    {
        return !IsVisible;
    }

    protected internal virtual void NotifyFormItemChanged(IFormItem formItem)
    {
    }
}