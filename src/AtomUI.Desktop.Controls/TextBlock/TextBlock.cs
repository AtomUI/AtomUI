using AtomUI.Controls;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

/// <summary>
/// A text block that can be the value control of a form item: it reports <c>Text</c> as its form value
/// through <see cref="IFormItemAware" />. Its default font style is <c>Normal</c>, and its text is not
/// clipped to the bounds of the control.
/// </summary>
public class TextBlock : AvaloniaTextBlock, IFormItemAware
{
    static TextBlock()
    {
        FontStyleProperty.OverrideDefaultValue<TextBlock>(FontStyle.Normal);
        TextProperty.Changed.AddClassHandler<TextBlock>((block, args) => block.NotifyFormValueChanged(args.NewValue as string));
        ClipToBoundsProperty.OverrideDefaultValue<TextBlock>(false);
    }
    
    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as string);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    /// <summary>
    /// Called after <c>Text</c> changed, with the new text. The base implementation raises the
    /// <see cref="IFormItemAware.ValueChanged" /> event, which makes the hosting form item publish the new value.
    /// </summary>
    /// <param name="value">
    /// The new value of <c>Text</c>.
    /// </param>
    protected virtual void NotifyFormValueChanged(string? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Writes the value that the form item passes into the control to <c>Text</c>, as a current value, so that
    /// a binding on <c>Text</c> is kept. Called by <see cref="IFormItemAware.SetFormValue" />.
    /// </summary>
    /// <param name="value">
    /// The new text; a value that is not a string arrives here as <see langword="null" />.
    /// </param>
    protected virtual void NotifySetFormValue(string? value)
    {
        SetCurrentValue(TextProperty, value);
    }

    /// <summary>
    /// Returns the text that the form item reads as its form value.
    /// Called by <see cref="IFormItemAware.GetFormValue" />.
    /// </summary>
    /// <returns>
    /// The current value of <c>Text</c>.
    /// </returns>
    protected virtual string? NotifyGetFormValue()
    {
        return Text;
    }

    /// <summary>
    /// Sets <c>Text</c> to <see langword="null" /> when the form item clears its value through
    /// <see cref="IFormItemAware.ClearFormValue" />. As in <see cref="NotifySetFormValue" />, the value is
    /// written as a current value, so a binding on <c>Text</c> is kept.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(TextProperty, null);
    }

    /// <summary>
    /// Receives the validation status of the hosting form item, which calls it whenever it updates its validation
    /// state. The base implementation does nothing.
    /// </summary>
    /// <param name="status">
    /// The validation status that the form item reports.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}