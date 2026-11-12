using AtomUI.Controls.Localization;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

internal class InputClearIconButton : IconButton
{
    private ILanguageManager? _subscribedLanguageManager;
    private IDisposable? _automationName;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachLanguageListener();
        UpdateAutomationName();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachLanguageListener();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            if (change.OldValue is PathIcon oldIcon)
            {
                oldIcon.Classes.Remove("skip-status");
            }

            if (change.NewValue is PathIcon newIcon)
            {
                newIcon.Classes.Add("skip-status");
            }
        }
    }

    // Ant Design names the clear button from locale.global.clear. Template priority lets a local value or
    // binding set by the application win; the previous value is released first so language changes replace
    // one value instead of stacking property frames.
    private void UpdateAutomationName()
    {
        var localizer = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)
            : null;
        var name = localizer?.Get(CommonLangResourceKind.Clear) ?? "Clear";
        _automationName?.Dispose();
        _automationName = SetValue(AutomationProperties.NameProperty, name, BindingPriority.Template);
    }

    private void AttachLanguageListener()
    {
        if (_subscribedLanguageManager is not null)
        {
            return;
        }

        var languageManager = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
            : null;
        if (languageManager is null)
        {
            return;
        }

        languageManager.LanguageChanged += HandleLanguageChanged;
        _subscribedLanguageManager = languageManager;
    }

    private void DetachLanguageListener()
    {
        if (_subscribedLanguageManager is null)
        {
            return;
        }

        _subscribedLanguageManager.LanguageChanged -= HandleLanguageChanged;
        _subscribedLanguageManager = null;
    }

    private void HandleLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        UpdateAutomationName();
    }
}
