namespace AtomUI;

public static class ControlPackageBuilderExtensions
{
    /// <summary>Uses the generated startup image when the Application cannot implement the generated host interface.</summary>
    public static IAtomUIBuilder UseStartupImage(this IAtomUIBuilder builder, Registration.StartupImage image)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(image);
        if (builder is not AtomUIBuilder root)
        {
            throw new ArgumentException("Startup images require the AtomUI application builder.", nameof(builder));
        }
        root.ControlRegistrationState.ThrowIfUnavailable();
        root.ThemeManagerBuilder.UseStartupImage(image);
        return builder;
    }
}
