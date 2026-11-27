namespace AtomUI.Localization;

/// <summary>
/// Marks an enum whose values name the units of one language catalog. The source generator compiles the
/// catalog with its translations into the language module data of the assembly and generates a XAML markup
/// extension class derived from <see cref="LanguageResourceExtension{TResourceKind}" /> for its values.
/// </summary>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class LanguageCatalogAttribute : Attribute
{
}
