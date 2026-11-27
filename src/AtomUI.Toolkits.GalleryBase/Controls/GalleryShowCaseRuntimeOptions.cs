namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Process-wide switch that turns off the deferred loading of showcase content in every showcase panel,
/// overriding the setting of the panel.
/// </summary>
public static class GalleryShowCaseRuntimeOptions
{
    /// <summary>
    /// The name of the environment variable that disables deferred loading. It takes effect when its value is
    /// <c>1</c>, <c>true</c>, <c>yes</c> or <c>on</c>, ignoring case.
    /// </summary>
    public const string DisableDeferredLoadingEnvironmentVariable = "ATOMUI_GALLERY_DISABLE_SHOWCASE_DEFERRED";

    private static bool s_isDeferredLoadingDisabledOverride;
    private static bool s_isEnvironmentDeferredLoadingDisabled = IsEnvironmentDeferredLoadingDisabled();

    /// <summary>
    /// Raised after the effective value of <see cref="IsDeferredLoadingDisabled" /> changes; the sender is
    /// <see langword="null" />.
    /// </summary>
    public static event EventHandler? DeferredLoadingDisabledChanged;

    /// <summary>
    /// <see langword="true" /> when deferred loading of showcase content is turned off in every showcase panel.
    /// Setting it stores a process-wide override; setting <see langword="false" /> clears the override. The option
    /// stays <see langword="true" /> while the environment variable is set.
    /// </summary>
    public static bool IsDeferredLoadingDisabled
    {
        get => s_isDeferredLoadingDisabledOverride || s_isEnvironmentDeferredLoadingDisabled;
        set => SetDeferredLoadingDisabledOverride(value);
    }

    /// <summary>
    /// Clears the process-wide override set through <see cref="IsDeferredLoadingDisabled" />. The option stays
    /// <see langword="true" /> when the environment variable is set.
    /// </summary>
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
