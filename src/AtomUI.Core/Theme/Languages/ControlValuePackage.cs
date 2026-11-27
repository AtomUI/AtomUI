using System.ComponentModel;
using AtomUI.Registration;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// A generated, statically linked value registration for one control package and language token type.
/// </summary>
public sealed class ControlValuePackage
{
    private readonly Action<IThemeManagerBuilder> _register;
    private readonly ControlPackageDefinition? _themePackage;

    /// <summary>
    /// Creates a value package. The constructor is meant for generated code; applications select generated
    /// packages with the language builder's <c>UseValues</c>.
    /// </summary>
    /// <param name="id">
    /// The identity of the package, the name of the assembly that generates it.
    /// </param>
    /// <param name="controlAssembly">
    /// The assembly of the control package whose token rules the package registers.
    /// </param>
    /// <param name="languageTokenType">
    /// The language token type the rules are written for; it must derive from <see cref="LanguageDesignToken" />.
    /// </param>
    /// <param name="register">
    /// Registers the package's control token rules on the theme manager builder.
    /// </param>
    public ControlValuePackage(string id, string controlAssembly, Type languageTokenType, Action<IThemeManagerBuilder> register)
        : this(id, controlAssembly, languageTokenType, register, null)
    {
    }

    /// <summary>
    /// Initializes a control value package together with an optional companion theme asset package definition.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ControlValuePackage(
        string id,
        string controlAssembly,
        Type languageTokenType,
        Action<IThemeManagerBuilder> register,
        ControlPackageDefinition? themePackage)
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
        _themePackage = themePackage;
    }

    /// <summary>
    /// The identity of the package: the name of the assembly that generates it. It names the package in
    /// diagnostics.
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// The assembly of the control package whose token rules this package registers. Together with
    /// <see cref="LanguageTokenType" /> it names the pair a value package covers.
    /// </summary>
    public string ControlAssembly { get; }
    /// <summary>
    /// The language token type the package's rules are written for. The package is linked when it is the
    /// token type of an enabled language or one of its base types.
    /// </summary>
    public Type LanguageTokenType { get; }

    internal void Register(IThemeManagerBuilder builder)
    {
        _register(builder);
        if (_themePackage is not null && builder is ThemeManagerBuilder themeBuilder)
        {
            _themePackage.Ensure(themeBuilder);
        }
    }
}

/// <summary>
/// Identifies a generated registration property for the application startup linker.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControlValuePackageRegistrationAttribute(Type registrationType, string member, string? resolver) : Attribute
{
    /// <summary>
    /// Initializes a registration attribute without a dedicated resolver method.
    /// </summary>
    public ControlValuePackageRegistrationAttribute(Type registrationType, string member) : this(registrationType, member, null)
    {
    }

    /// <summary>
    /// The generated class that declares the registration property named <see cref="Member" />.
    /// </summary>
    public Type RegistrationType { get; } = registrationType;
    /// <summary>
    /// The name of the public static property of <see cref="RegistrationType" /> that returns the package.
    /// </summary>
    public string Member { get; } = member;
    /// <summary>
    /// Optional static method name on <see cref="RegistrationType" /> that resolves the package via TypeMap.
    /// </summary>
    public string? Resolver { get; } = resolver;
}
