namespace AtomUI.Toolkits.GalleryBase.Controls;

public static class GalleryShowCaseRuntimeOptions
{
    public const string DisableDeferredLoadingEnvironmentVariable = "ATOMUI_GALLERY_DISABLE_SHOWCASE_DEFERRED";

    private static bool s_isDeferredLoadingDisabledOverride;
    private static bool s_isEnvironmentDeferredLoadingDisabled = IsEnvironmentDeferredLoadingDisabled();

    public static event EventHandler? DeferredLoadingDisabledChanged;

    public static bool IsDeferredLoadingDisabled
    {
        get => s_isDeferredLoadingDisabledOverride || s_isEnvironmentDeferredLoadingDisabled;
        set => SetDeferredLoadingDisabledOverride(value);
    }

    public static void ResetDeferredLoadingDisabledOverride()
    {
        SetDeferredLoadingDisabledOverride(false);
    }

    private static void SetDeferredLoadingDisabledOverride(bool value)
    {
        var oldValue = IsDeferredLoadingDisabled;
        s_isDeferredLoadingDisabledOverride = value;
        s_isEnvironmentDeferredLoadingDisabled = IsEnvironmentDeferredLoadingDisabled();
        var newValue = IsDeferredLoadingDisabled;
        if (oldValue != newValue)
        {
            DeferredLoadingDisabledChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    private static bool IsEnvironmentDeferredLoadingDisabled()
    {
        var value = Environment.GetEnvironmentVariable(DisableDeferredLoadingEnvironmentVariable);
        return value is not null &&
               (value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on", StringComparison.OrdinalIgnoreCase));
    }
}
