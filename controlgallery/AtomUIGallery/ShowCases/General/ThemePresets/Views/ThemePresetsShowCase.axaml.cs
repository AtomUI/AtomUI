using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUIGallery.ShowCases.ThemePresets;

public partial class ThemePresetsShowCase : GalleryReactiveUserControl<ThemePresetsViewModel>
{
    private ThemePresetsViewModel? _activeModel;
    private WindowNotificationManager? _notifications;

    public ThemePresetsShowCase() { InitializeComponent(); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ActivateModel();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _activeModel?.Deactivate();
        _activeModel = null;
        base.OnDataContextChanged(e);
        if (VisualRoot is not null) { ActivateModel(); }
    }

    private void ActivateModel()
    {
        if (DataContext is ThemePresetsViewModel model)
        {
            _activeModel = model;
            model.Activate();
            model.CopyText = CopyAsync;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _activeModel?.Deactivate();
        _activeModel = null;
        if (_notifications is { } notifications)
        {
            NotificationOverlay.Children.Remove(notifications);
            notifications.Dispose();
            _notifications = null;
        }
        base.OnDetachedFromVisualTree(e);
    }

    private void SelectPreset(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: ThemePresetItem preset } && DataContext is ThemePresetsViewModel model)
        {
            model.SelectedPresetId = preset.ThemeId;
        }
    }

    private void SelectPrimary(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: ThemePrimarySwatch swatch } && DataContext is ThemePresetsViewModel model)
        {
            model.SelectedPrimary = swatch.Color;
        }
    }

    private async Task<bool> CopyAsync(string xml)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) { return false; }
        var item = new DataTransferItem();
        item.SetText(xml);
        var transfer = new DataTransfer();
        transfer.Add(item);
        await clipboard.SetDataAsync(transfer);
        return true;
    }

    private void OpenDialog(object? sender, RoutedEventArgs e) => PreviewDialog.IsOpen = true;

    private void ShowNotification(object? sender, RoutedEventArgs e)
    {
        if (_notifications is null)
        {
            _notifications = new WindowNotificationManager { MaxItems = 1, Position = NotificationPosition.BottomRight };
            NotificationOverlay.Children.Add(_notifications);
        }
        _notifications.Show(new AtomUINotification(
            GalleryLocalization.Get(ThemePresetsShowCaseLangResourceKind.Preview, "Preview"),
            GalleryLocalization.Get(ThemePresetsShowCaseLangResourceKind.Alert, "Your changes are saved.")));
    }
}
