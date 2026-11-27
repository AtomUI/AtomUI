using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Controls;

/// <summary>
/// Shows the validation feedback of a form item. <see cref="ValidateStatus" /> selects what is shown: the
/// control is hidden while the status is <see cref="FormValidateStatus.Default" />, and otherwise presents the
/// content and template pair of that status.
/// The theme of this control defines no default content or template. The default Form theme provides a
/// template that creates the control bound to the form's properties of the same names, and sets an icon
/// template for each of the four statuses.
/// </summary>
public class FormValidateFeedback : TemplatedControl, IFormValidateFeedback
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SuccessFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SuccessFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(SuccessFeedback));
    
    /// <summary>
    /// Identifies the <see cref="SuccessFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SuccessFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(SuccessFeedbackTemplate));
    
    /// <summary>
    /// Identifies the <see cref="WarningFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> WarningFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(WarningFeedback));
    
    /// <summary>
    /// Identifies the <see cref="WarningFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> WarningFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(WarningFeedbackTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ErrorFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ErrorFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(ErrorFeedback));
    
    /// <summary>
    /// Identifies the <see cref="ErrorFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ErrorFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(ErrorFeedbackTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ValidatingFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ValidatingFeedbackProperty =
        AvaloniaProperty.Register<FormValidateFeedback, object?>(nameof(ValidatingFeedback));
    
    /// <summary>
    /// Identifies the <see cref="ValidatingFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ValidatingFeedbackTemplateProperty =
        AvaloniaProperty.Register<FormValidateFeedback, IDataTemplate?>(nameof(ValidatingFeedbackTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ValidateStatus" /> property.
    /// </summary>
    public static readonly DirectProperty<FormValidateFeedback, FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.RegisterDirect<FormValidateFeedback, FormValidateStatus>(
            nameof(ValidateStatus),
            o => o.ValidateStatus,
            (o, v) => o.ValidateStatus = v);
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Success" />. It is
    /// presented with <see cref="SuccessFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(SuccessFeedbackTemplate))]
    public object? SuccessFeedback
    {
        get => GetValue(SuccessFeedbackProperty);
        set => SetValue(SuccessFeedbackProperty, value);
    }
    
    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Success" />, with <see cref="SuccessFeedback" /> as its data.
    /// The default is <see langword="null" />.
    /// </summary>
    public IDataTemplate? SuccessFeedbackTemplate
    {
        get => GetValue(SuccessFeedbackTemplateProperty);
        set => SetValue(SuccessFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Warning" />. It is
    /// presented with <see cref="WarningFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(WarningFeedbackTemplate))]
    public object? WarningFeedback
    {
        get => GetValue(WarningFeedbackProperty);
        set => SetValue(WarningFeedbackProperty, value);
    }
    
    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Warning" />, with <see cref="WarningFeedback" /> as its data.
    /// The default is <see langword="null" />.
    /// </summary>
    public IDataTemplate? WarningFeedbackTemplate
    {
        get => GetValue(WarningFeedbackTemplateProperty);
        set => SetValue(WarningFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Error" />. It is
    /// presented with <see cref="ErrorFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(ErrorFeedbackTemplate))]
    public object? ErrorFeedback
    {
        get => GetValue(ErrorFeedbackProperty);
        set => SetValue(ErrorFeedbackProperty, value);
    }

    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Error" />, with <see cref="ErrorFeedback" /> as its data.
    /// The default is <see langword="null" />.
    /// </summary>
    public IDataTemplate? ErrorFeedbackTemplate
    {
        get => GetValue(ErrorFeedbackTemplateProperty);
        set => SetValue(ErrorFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Validating" />. It is
    /// presented with <see cref="ValidatingFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(ValidatingFeedbackTemplate))]
    public object? ValidatingFeedback
    {
        get => GetValue(ValidatingFeedbackProperty);
        set => SetValue(ValidatingFeedbackProperty, value);
    }

    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Validating" />, with <see cref="ValidatingFeedback" /> as its data.
    /// The default is <see langword="null" />.
    /// </summary>
    public IDataTemplate? ValidatingFeedbackTemplate
    {
        get => GetValue(ValidatingFeedbackTemplateProperty);
        set => SetValue(ValidatingFeedbackTemplateProperty, value);
    }
    
    private FormValidateStatus _validateStatus = FormValidateStatus.Default;

    /// <summary>
    /// The status that selects the feedback. <see cref="FormValidateStatus.Default" />, the initial value, hides
    /// the control; each other status shows its content and template pair. The item that uses the control relays
    /// its own status into this property.
    /// </summary>
    public FormValidateStatus ValidateStatus
    {
        get => _validateStatus;
        set => SetAndRaise(ValidateStatusProperty, ref _validateStatus, value);
    }
    #endregion
}