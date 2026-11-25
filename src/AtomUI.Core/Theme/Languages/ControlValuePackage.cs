using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>A generated, statically linked value registration for one control package and language token type.</summary>
public sealed class ControlValuePackage
{
    private readonly Action<IThemeManagerBuilder> _register;

    public ControlValuePackage(string id, string controlAssembly, Type languageTokenType, Action<IThemeManagerBuilder> register)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(controlAssembly);
        ArgumentNullException.ThrowIfNull(languageTokenType);
        ArgumentNullException.ThrowIfNull(register);
        if (!typeof(LanguageDesignToken).IsAssignableFrom(languageTokenType))
        {
            throw new ArgumentException("Expected a language token type.", nameof(languageTokenType));
        }
        Id = id;
        ControlAssembly = controlAssembly;
        LanguageTokenType = languageTokenType;
        _register = register;
    }

    public string Id { get; }
    public string ControlAssembly { get; }
    public Type LanguageTokenType { get; }
    internal void Register(IThemeManagerBuilder builder) => _register(builder);
}

/// <summary>Identifies a generated registration property for the application startup linker.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControlValuePackageRegistrationAttribute(Type registrationType, string member) : Attribute
{
    public Type RegistrationType { get; } = registrationType;
    public string Member { get; } = member;
}
