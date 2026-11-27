namespace AtomUI.Localization;

/// <summary>
/// The origin of a translation bundle. When a unit is resolved, the source kinds are read in the priority
/// application override, static language pack, module built-in, and every fallback language of a higher kind
/// is tried before a lower kind is read.
/// </summary>
public enum TranslationSourceKind : byte
{
    /// <summary>
    /// A translation compiled into the language module data of the assembly that owns the catalog.
    /// </summary>
    ModuleBuiltIn = 0,
    /// <summary>
    /// A translation that a language pack supplies in addition to the built-in ones.
    /// </summary>
    StaticLanguagePack = 1,
    /// <summary>
    /// A translation that the application registered explicitly. It wins over every other source kind.
    /// </summary>
    ApplicationOverride = 2
}
