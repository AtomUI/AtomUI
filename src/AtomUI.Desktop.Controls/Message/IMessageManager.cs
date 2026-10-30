namespace AtomUI.Desktop.Controls;

public interface IMessageManager
{
    public void Show(IMessage message, string[]? classes = null);
    public void DestroyAll();
}
