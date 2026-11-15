using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public partial class FormItem
{
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
            item.Foreground = foreground;
            position++;
        }
    }
}
