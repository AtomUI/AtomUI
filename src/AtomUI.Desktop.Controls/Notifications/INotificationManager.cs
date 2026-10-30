namespace AtomUI.Desktop.Controls;

public interface INotificationManager
{
    public void Show(INotification notification, string[]? classes = null);
    public void DestroyAll();
}
