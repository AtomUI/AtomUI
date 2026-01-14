namespace AtomUI.Desktop.Controls;

public interface IMentionOptionFilter
{
    bool Filter(IMentionOption option, object? filterValue);
}