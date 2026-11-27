namespace AtomUI.Controls;

/// <summary>
/// Sets the horizontal alignment of the label text of a form item.
/// </summary>
public enum FormLabelAlign
{
    /// <summary>
    /// Left-aligns the label text.
    /// </summary>
    Left,
    /// <summary>
    /// Right-aligns the label text. This is the default. The vertical item layout left-aligns the label
    /// whatever this value is.
    /// </summary>
    Right
}

/// <summary>
/// Sets the layout of the items of a form. The value is set on the form and copied to every item.
/// </summary>
public enum FormLayout
{
    /// <summary>
    /// Places the label beside the content. This is the default.
    /// </summary>
    Horizontal,
    /// <summary>
    /// Places the label above the content.
    /// </summary>
    Vertical,
    /// <summary>
    /// Places the label beside the content, as <see cref="Horizontal" /> does. The spacing between items
    /// is the inline item spacing, and the content of an item gets no maximum width.
    /// </summary>
    Inline
}

/// <summary>
/// Sets the mark shown with the label of a form item to tell whether its value is required.
/// </summary>
public enum FormRequiredMark
{
    /// <summary>
    /// Shows an asterisk on the items that are required. This is the default.
    /// </summary>
    Default,
    /// <summary>
    /// Shows no mark.
    /// </summary>
    Hidden,
    /// <summary>
    /// Shows the localized "optional" text on the items that are not required.
    /// </summary>
    Optional,
    /// <summary>
    /// Shows the custom required mark of an item that is required and the custom optional mark of an item
    /// that is not. An item without the matching custom mark shows none.
    /// </summary>
    Customize,
    /// <summary>
    /// Shows the localized "required" text on the items that are required.
    /// </summary>
    RequiredText
}

/// <summary>
/// Sets when a form item validates its value on its own, without being asked to.
/// </summary>
public enum FormValidateTrigger
{
    /// <summary>
    /// Validates after the user changes the value of the item, waiting <c>ValidateDebounce</c> first.
    /// This is the default.
    /// </summary>
    OnChanged,
    /// <summary>
    /// Validates when keyboard focus leaves the item.
    /// </summary>
    OnBlur,
    /// <summary>
    /// Starts no validation itself. The item is validated only when the form validates or submits,
    /// or when <c>ValidateValueAsync</c> is called on it.
    /// </summary>
    OnSubmit
}

/// <summary>
/// Sets how a form item runs its validators when its value is validated.
/// </summary>
public enum FormValidateStrategy
{
    /// <summary>
    /// Runs the validators one after another and stops at the first one that returns
    /// <see cref="FormValidateResult.Error" />. This is the default.
    /// </summary>
    StopWhenFirstFailed,
    /// <summary>
    /// Runs every validator one after another, whatever the results, and reports the messages of all of them.
    /// </summary>
    Sequential,
    /// <summary>
    /// Starts all validators at once and waits for all of them. The messages are reported in the order
    /// of the validators.
    /// </summary>
    Parallel
}

/// <summary>
/// Names the validation state of a form item. It selects the feedback shown for the item and decides
/// whether the item counts as valid for the form.
/// </summary>
public enum FormValidateStatus
{
    /// <summary>
    /// The item has not been validated yet, or its validation state has been reset.
    /// </summary>
    Default,
    /// <summary>
    /// The last validation found no error and no warning.
    /// </summary>
    Success,
    /// <summary>
    /// The last validation found warnings but no error. The item still counts as valid for the form.
    /// </summary>
    Warning,
    /// <summary>
    /// The item has error messages. The item does not count as valid for the form.
    /// </summary>
    Error,
    /// <summary>
    /// A validation of the item is running. It lasts until that validation has finished.
    /// </summary>
    Validating
}