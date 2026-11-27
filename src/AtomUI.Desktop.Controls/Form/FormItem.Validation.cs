using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public partial class FormItem
{
    /// <summary>
    /// Validates the current value of the item and updates the validation state with the outcome.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancels the validation.
    /// </param>
    /// <returns>
    /// A task that completes when the validators have finished.
    /// </returns>
    /// <remarks>
    /// The call is forwarded to <c>AbstractFormItem.ValidateValueAsync</c>, which owns the contract.
    /// <c>ValidateChanged</c> is raised before the task completes. A validation that is already running is canceled
    /// and its outcome is discarded. The outcome of this run is discarded as well when the token is canceled or a
    /// newer validation starts; the validation state then stays as it is. The method validates whatever
    /// <c>ValidateTrigger</c> is.
    /// </remarks>
    public new Task ValidateValueAsync(CancellationToken cancellationToken) => base.ValidateValueAsync(cancellationToken);

    private void BuildMessageItems()
    {
        if (_extraInfoLayout is not { } layout)
        {
            return;
        }

        var requiredCount = (ValidateErrorMessages?.Count ?? 0) + (ValidateWarningMessages?.Count ?? 0);
        for (var index = _messageItems.Count - 1; index >= requiredCount; index--)
        {
            layout.Children.Remove(_messageItems[index]);
            _messageItems.RemoveAt(index);
        }
        var position = 0;
        SynchronizeMessages(ValidateErrorMessages, ErrorMessageForeground, layout, ref position);
        SynchronizeMessages(ValidateWarningMessages, WarningMessageForeground, layout, ref position);
    }

    private void SynchronizeMessages(IList<string>? messages, IBrush? foreground, StackPanel layout, ref int position)
    {
        if (messages is null)
        {
            return;
        }
        foreach (var message in messages)
        {
            Avalonia.Controls.TextBlock item;
            if (position < _messageItems.Count && layout.Children.Contains(_messageItems[position]))
            {
                item = _messageItems[position];
            }
            else
            {
                item = new Avalonia.Controls.TextBlock();
                item.Classes.Add(FormItemSemanticParts.HelpItemClass);
                var insertIndex = _helpText is not null ? layout.Children.IndexOf(_helpText) : -1;
                layout.Children.Insert(insertIndex < 0 ? layout.Children.Count : insertIndex, item);
                if (position < _messageItems.Count)
                {
                    _messageItems[position] = item;
                }
                else
                {
                    _messageItems.Add(item);
                }
            }

            item.Text = message;
            // Template priority: Semantic Part styles on the help item and user values keep precedence.
            item.SetValue(Avalonia.Controls.TextBlock.ForegroundProperty, foreground, BindingPriority.Template);
            position++;
        }
    }
}
