using Avalonia.Controls.Templates;

namespace AtomUI.Controls;

/// <summary>
/// Shows the validation feedback of a form item. <see cref="ValidateStatus" /> selects which of the four pairs
/// of content and template is presented.
/// </summary>
public interface IFormValidateFeedback
{
    /// <summary>
    /// Gets or sets the validation status that selects the pair of content and template to present:
    /// <see cref="SuccessFeedback" /> with <see cref="SuccessFeedbackTemplate" /> for
    /// <see cref="FormValidateStatus.Success" />, and likewise for the warning, error and validating status.
    /// </summary>
    FormValidateStatus ValidateStatus { get; set; }
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Success" />, presented
    /// with <see cref="SuccessFeedbackTemplate" />.
    /// </summary>
    public object? SuccessFeedback { get; set; }
    
    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Success" />, with <see cref="SuccessFeedback" /> as its data.
    /// </summary>
    public IDataTemplate? SuccessFeedbackTemplate { get; set; }
  
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Warning" />, presented
    /// with <see cref="WarningFeedbackTemplate" />.
    /// </summary>
    public object? WarningFeedback { get; set; }
    
    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Warning" />, with <see cref="WarningFeedback" /> as its data.
    /// </summary>
    public IDataTemplate? WarningFeedbackTemplate { get; set; }
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Error" />, presented
    /// with <see cref="ErrorFeedbackTemplate" />.
    /// </summary>
    public object? ErrorFeedback { get; set; }
    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Error" />, with <see cref="ErrorFeedback" /> as its data.
    /// </summary>
    public IDataTemplate? ErrorFeedbackTemplate { get; set; }
    
    /// <summary>
    /// The content shown while <see cref="ValidateStatus" /> is <see cref="FormValidateStatus.Validating" />,
    /// presented with <see cref="ValidatingFeedbackTemplate" />.
    /// </summary>
    public object? ValidatingFeedback { get; set; }

    /// <summary>
    /// The template that builds the feedback while <see cref="ValidateStatus" /> is
    /// <see cref="FormValidateStatus.Validating" />, with <see cref="ValidatingFeedback" /> as its data.
    /// </summary>
    public IDataTemplate? ValidatingFeedbackTemplate { get; set; }
}