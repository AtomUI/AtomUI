namespace AtomUI.Localization;

/// <summary>
/// Build-time language catalog and translation data consumed by the application linker.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class LanguageModuleManifestAttribute : Attribute
{
    /// <summary>
    /// Creates a manifest entry for the language data of one module.
    /// </summary>
    /// <param name="version">
    /// The version of the format that <paramref name="data" /> uses. The generator writes 1.
    /// </param>
    /// <param name="moduleId">
    /// The identifier of the language module that <paramref name="data" /> holds.
    /// </param>
    /// <param name="data">
    /// The encoded catalogs and translation bundles of the module, as a localization builder accepts them
    /// through its <c>AddModuleData</c> method.
    /// </param>
    public LanguageModuleManifestAttribute(int version, string moduleId, string data)
    {
        Version = version;
        ModuleId = moduleId;
        Data = data;
    }

    /// <summary>
    /// The version of the format that <see cref="Data" /> uses. The generator writes 1.
    /// </summary>
    public int Version { get; }
    /// <summary>
    /// The identifier of the language module whose data <see cref="Data" /> carries.
    /// </summary>
    public string ModuleId { get; }
    /// <summary>
    /// The encoded catalogs and translation bundles of the module, in the form that a localization builder
    /// accepts through its <c>AddModuleData</c> method.
    /// </summary>
    public string Data { get; }
}
