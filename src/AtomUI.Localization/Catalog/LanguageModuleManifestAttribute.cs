namespace AtomUI.Localization;

/// <summary>Build-time language catalog and translation data consumed by the application linker.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class LanguageModuleManifestAttribute : Attribute
{
    public LanguageModuleManifestAttribute(int version, string moduleId, string data)
    {
        Version = version;
        ModuleId = moduleId;
        Data = data;
    }

    public int Version { get; }
    public string ModuleId { get; }
    public string Data { get; }
}
