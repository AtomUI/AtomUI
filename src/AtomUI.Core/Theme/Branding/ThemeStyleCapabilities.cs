namespace AtomUI.Theme.Branding;

[Flags]
public enum ThemeStyleCapabilities
{
    QualifiedStyles = 1,
    CompiledTemplates = 2,
    RegisteredStaticAssets = 4,
    ScopedResources = 8
}
