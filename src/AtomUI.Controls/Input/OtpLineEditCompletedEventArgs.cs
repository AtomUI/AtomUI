namespace AtomUI.Desktop.Controls;

public class OtpLineEditCompletedEventArgs : EventArgs
{
    public OtpLineEditCompletedEventArgs(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
