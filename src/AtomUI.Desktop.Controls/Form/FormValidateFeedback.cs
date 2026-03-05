using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class FormValidateFeedback : TemplatedControl,
                                    IControlSharedTokenResourcesHost,
                                    IFormValidateFeedback
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> SuccessFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(SuccessFeedback));
    
    public static readonly StyledProperty<IDataTemplate?> SuccessFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(SuccessFeedbackTemplate));
    
    public static readonly StyledProperty<object?> WarningFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(WarningFeedback));
    
    public static readonly StyledProperty<IDataTemplate?> WarningFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(WarningFeedbackTemplate));
    
    public static readonly StyledProperty<object?> ErrorFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(ErrorFeedback));
    
    public static readonly StyledProperty<IDataTemplate?> ErrorFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(ErrorFeedbackTemplate));
    
    public static readonly StyledProperty<object?> ValidatingFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(ValidatingFeedback));
    
    public static readonly StyledProperty<IDataTemplate?> ValidatingFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(ValidatingFeedbackTemplate));
    
    public static readonly DirectProperty<FormValidateFeedback, FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.RegisterDirect<FormValidateFeedback, FormValidateStatus>(
            nameof(ValidateStatus),
            o => o.ValidateStatus,
            (o, v) => o.ValidateStatus = v);
    
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
    
    private FormValidateStatus _validateStatus = FormValidateStatus.Default;

    public FormValidateStatus ValidateStatus
    {
        get => _validateStatus;
        set => SetAndRaise(ValidateStatusProperty, ref _validateStatus, value);
    }
    #endregion
    
    #region 内部属性定义
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    #endregion

    public FormValidateFeedback()
    {
        this.RegisterResources();
    }
}