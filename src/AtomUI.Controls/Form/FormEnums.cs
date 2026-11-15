namespace AtomUI.Controls;

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
    Customize,
    RequiredText
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