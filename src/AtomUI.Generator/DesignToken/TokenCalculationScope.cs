using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.DesignToken;

/// <summary>
/// Describes which token calculation method an explicit-override analyzer inspects and how a valid
/// <c>IsExplicit</c> guard looks there. Two scopes exist: control tokens (<c>CalculateTokenValues</c> on a
/// <c>[ControlDesignToken]</c> type, guarded with <c>&lt;Control&gt;TokenKind</c>) and global tokens
/// (<c>CalculateAliasTokenValues</c> on a <c>[GlobalDesignToken]</c> type, guarded with <c>SharedTokenKind</c>,
/// limited to <c>[DesignTokenKind(Alias)]</c> properties).
/// </summary>
internal sealed class TokenCalculationScope
{
    private const string ControlCalculateMethodName = "CalculateTokenValues";
    private const string GlobalCalculateMethodName = "CalculateAliasTokenValues";
    private const string TokenSuffix = "Token";
    private const string SharedTokenKindName = "SharedTokenKind";
    private const string SharedTokenKindNamespace = "AtomUI.Theme.Resources";
    private const string AliasKindName = "Alias";

    private readonly INamedTypeSymbol? _controlAttribute;
    private readonly INamedTypeSymbol? _controlBaseToken;
    private readonly INamedTypeSymbol? _globalAttribute;
    private readonly INamedTypeSymbol? _tokenKindAttribute;

    private TokenCalculationScope(
        INamedTypeSymbol? controlAttribute,
        INamedTypeSymbol? controlBaseToken,
        INamedTypeSymbol? globalAttribute,
        INamedTypeSymbol? tokenKindAttribute)
    {
        _controlAttribute   = controlAttribute;
        _controlBaseToken   = controlBaseToken;
        _globalAttribute    = globalAttribute;
        _tokenKindAttribute = tokenKindAttribute;
    }

    /// <summary>
    /// Resolves the marker symbols from the compilation; returns <c>null</c> when neither scope applies.
    /// </summary>
    public static TokenCalculationScope? Create(Compilation compilation, bool requireControlBaseToken)
    {
        var controlAttribute = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlDesignTokenAttribute);
        var controlBaseToken = compilation.GetTypeByMetadataName(TargetMarkConstants.AbstractControlDesignToken);
        if (controlAttribute is null || (requireControlBaseToken && controlBaseToken is null))
        {
            controlAttribute = null;
        }

        var globalAttribute    = compilation.GetTypeByMetadataName(TargetMarkConstants.GlobalDesignTokenAttribute);
        var tokenKindAttribute = compilation.GetTypeByMetadataName(TargetMarkConstants.DesignTokenKindAttribute);
        if (globalAttribute is null || tokenKindAttribute is null)
        {
            globalAttribute = null;
        }

        return controlAttribute is null && globalAttribute is null
            ? null
            : new TokenCalculationScope(controlAttribute, controlBaseToken, globalAttribute, tokenKindAttribute);
    }

    /// <summary>
    /// Returns the matched calculation kind for an instance method, or <c>null</c> when it is out of scope.
    /// </summary>
    public TokenCalculationKind? Match(IMethodSymbol method)
    {
        if (method.IsStatic || method.ContainingType is not { } tokenType)
        {
            return null;
        }

        if (_controlAttribute is not null &&
            string.Equals(method.Name, ControlCalculateMethodName, StringComparison.Ordinal) &&
            HasAttribute(tokenType, _controlAttribute))
        {
            return TokenCalculationKind.Control;
        }

        if (_globalAttribute is not null &&
            string.Equals(method.Name, GlobalCalculateMethodName, StringComparison.Ordinal) &&
            HasAttribute(tokenType, _globalAttribute))
        {
            return TokenCalculationKind.Global;
        }

        return null;
    }

    /// <summary>
    /// Whether the property is a token the given scope checks: declared on the token type or a base, and for
    /// the global scope additionally marked <c>[DesignTokenKind(Alias)]</c>.
    /// </summary>
    public bool IsCheckedToken(IPropertySymbol property, INamedTypeSymbol tokenType, TokenCalculationKind kind)
    {
        if (property.IsStatic || !IsDeclaredOnTypeOrBase(property.ContainingType, tokenType))
        {
            return false;
        }

        return kind == TokenCalculationKind.Control || IsAliasToken(property);
    }

    /// <summary>
    /// Whether the invoked method is the <c>IsExplicit</c> the scope accepts as a guard.
    /// </summary>
    public bool IsGuardMethod(IMethodSymbol called, INamedTypeSymbol tokenType, TokenCalculationKind kind)
    {
        if (!string.Equals(called.Name, "IsExplicit", StringComparison.Ordinal))
        {
            return false;
        }

        var declaringType = called.OriginalDefinition.ContainingType;
        return kind == TokenCalculationKind.Control
            ? SymbolEqualityComparer.Default.Equals(declaringType, _controlBaseToken)
            : IsDeclaredOnTypeOrBase(declaringType, tokenType);
    }

    /// <summary>
    /// Whether the enum is the kind enum whose members name the tokens of this scope.
    /// </summary>
    public static bool IsKindEnum(INamedTypeSymbol enumType, INamedTypeSymbol tokenType, TokenCalculationKind kind)
    {
        if (kind == TokenCalculationKind.Control)
        {
            return string.Equals(enumType.Name, GetKindEnumName(tokenType, kind), StringComparison.Ordinal);
        }

        return string.Equals(enumType.Name, SharedTokenKindName, StringComparison.Ordinal) &&
               string.Equals(enumType.ContainingNamespace?.ToDisplayString(), SharedTokenKindNamespace, StringComparison.Ordinal);
    }

    /// <summary>
    /// Name of the kind enum used in the guard, e.g. <c>ButtonTokenKind</c> or <c>SharedTokenKind</c>.
    /// </summary>
    public static string GetKindEnumName(INamedTypeSymbol tokenType, TokenCalculationKind kind)
    {
        if (kind == TokenCalculationKind.Global)
        {
            return SharedTokenKindName;
        }

        var controlName = tokenType.Name.EndsWith(TokenSuffix, StringComparison.Ordinal)
            ? tokenType.Name.Substring(0, tokenType.Name.Length - TokenSuffix.Length)
            : tokenType.Name;
        return controlName + "TokenKind";
    }

    public static string GetMethodName(TokenCalculationKind kind) =>
        kind == TokenCalculationKind.Control ? ControlCalculateMethodName : GlobalCalculateMethodName;

    private bool IsAliasToken(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _tokenKindAttribute) ||
                attribute.ConstructorArguments.Length != 1)
            {
                continue;
            }

            var argument = attribute.ConstructorArguments[0];
            if (argument.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } kindEnum)
            {
                continue;
            }

            foreach (var member in kindEnum.GetMembers(AliasKindName))
            {
                if (member is IFieldSymbol { HasConstantValue: true } aliasField &&
                    Equals(aliasField.ConstantValue, argument.Value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasAttribute(INamedTypeSymbol type, INamedTypeSymbol attributeSymbol)
    {
        foreach (var attribute in type.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDeclaredOnTypeOrBase(INamedTypeSymbol? declaringType, INamedTypeSymbol tokenType)
    {
        for (var current = tokenType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, declaringType))
            {
                return true;
            }
        }

        return false;
    }
}

internal enum TokenCalculationKind
{
    Control,
    Global
}
