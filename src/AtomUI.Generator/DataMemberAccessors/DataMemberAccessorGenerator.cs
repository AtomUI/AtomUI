using System.Text;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.DataMemberAccessors;

/// <summary>
/// Generates a <c>DataMemberAccessorRegistration_…</c> class for every non-generic class, struct or interface
/// marked with <c>[GenerateDataMemberAccessors]</c>. The class describes each accessible instance property,
/// inherited ones included, with its name, type, writability, display name, order, read-only and editable state,
/// and states whether an instance of the type can be created.
/// </summary>
[Generator]
public sealed class DataMemberAccessorGenerator : IIncrementalGenerator
{
    private const string DisplayAttributeMetadataName = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string EditableAttributeMetadataName = "System.ComponentModel.DataAnnotations.EditableAttribute";
    private const string ReadOnlyAttributeMetadataName = "System.ComponentModel.ReadOnlyAttribute";

    private static readonly SymbolDisplayFormat s_fullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    /// Registers the implementation source output that writes one registration class per type marked with
    /// <c>[GenerateDataMemberAccessors]</c>; types the generator does not handle produce none.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var dataTypes = initContext.SyntaxProvider.ForAttributeWithMetadataName(
                TargetMarkConstants.GenerateDataMemberAccessorsAttribute,
                static (node, token) => true,
                static (context, token) => CreateDataTypeInfo(context))
            .Where(static info => info is not null)
            .Collect()
            .Select(static (items, token) => items.Where(static item => item is not null)
                                                  .Select(static item => item!)
                                                  .ToArray());

        initContext.RegisterImplementationSourceOutput(dataTypes, static (context, items) =>
        {
            foreach (var item in items)
            {
                var writer = new DataMemberAccessorSourceWriter(context, item);
                writer.Write();
            }
        });
    }

    private static DataMemberAccessorTypeInfo? CreateDataTypeInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        if ((typeSymbol.TypeKind != TypeKind.Class &&
             typeSymbol.TypeKind != TypeKind.Struct &&
             typeSymbol.TypeKind != TypeKind.Interface) ||
            typeSymbol.IsGenericType)
        {
            return null;
        }

        var properties = GetAccessibleInstanceProperties(typeSymbol)
            .Select(CreatePropertyInfo)
            .ToArray();

        return new DataMemberAccessorTypeInfo(
            typeSymbol.ToDisplayString(s_fullyQualifiedNullableFormat),
            GetGeneratedClassName(typeSymbol),
            CanCreateFactory(typeSymbol),
            IsReadOnly(typeSymbol),
            properties);
    }

    private static DataMemberAccessorPropertyInfo CreatePropertyInfo(IPropertySymbol property)
    {
        var displayAttribute = FindAttribute(property, DisplayAttributeMetadataName);
        return new DataMemberAccessorPropertyInfo(
            property.Name,
            property.Type.ToDisplayString(s_fullyQualifiedNullableFormat),
            property.SetMethod is not null &&
            !property.SetMethod.IsInitOnly &&
            IsAccessibleFromGeneratedCode(property.SetMethod.DeclaredAccessibility),
            GetDisplayName(displayAttribute),
            GetNamedBoolValue(displayAttribute, "AutoGenerateField"),
            GetNamedIntValue(displayAttribute, "Order"),
            IsReadOnly(property),
            IsEditable(property));
    }

    private static IEnumerable<IPropertySymbol> GetAccessibleInstanceProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<IPropertySymbol>();
        var names      = new HashSet<string>();

        void AddProperties(INamedTypeSymbol symbol)
        {
            foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic ||
                    property.GetMethod is null ||
                    !IsAccessibleFromGeneratedCode(property.GetMethod.DeclaredAccessibility) ||
                    !names.Add(property.Name))
                {
                    continue;
                }

                properties.Add(property);
            }
        }

        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            AddProperties(typeSymbol);
            foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
            {
                AddProperties(interfaceSymbol);
            }
            return properties;
        }

        for (INamedTypeSymbol? current = typeSymbol; current != null; current = current.BaseType)
        {
            AddProperties(current);
        }

        return properties;
    }

    private static string? GetDisplayName(AttributeData? displayAttribute)
    {
        return GetNamedStringValue(displayAttribute, "ShortName") ??
               GetNamedStringValue(displayAttribute, "Name");
    }

    private static bool IsReadOnly(ISymbol symbol)
    {
        var readOnlyAttribute = FindAttribute(symbol, ReadOnlyAttributeMetadataName);
        return GetConstructorBoolValue(readOnlyAttribute, 0) == true;
    }

    private static bool IsEditable(ISymbol symbol)
    {
        var editableAttribute = FindAttribute(symbol, EditableAttributeMetadataName);
        return GetConstructorBoolValue(editableAttribute, 0) != false;
    }

    private static AttributeData? FindAttribute(ISymbol symbol, string metadataName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == metadataName)
            {
                return attribute;
            }
        }

        return null;
    }

    private static bool? GetConstructorBoolValue(AttributeData? attribute, int index)
    {
        if (attribute == null || attribute.ConstructorArguments.Length <= index)
        {
            return null;
        }

        var value = attribute.ConstructorArguments[index].Value;
        return value is bool typedValue ? typedValue : null;
    }

    private static string? GetNamedStringValue(AttributeData? attribute, string name)
    {
        return GetNamedValue(attribute, name) as string;
    }

    private static bool? GetNamedBoolValue(AttributeData? attribute, string name)
    {
        return GetNamedValue(attribute, name) is bool value ? value : null;
    }

    private static int? GetNamedIntValue(AttributeData? attribute, string name)
    {
        return GetNamedValue(attribute, name) is int value ? value : null;
    }

    private static object? GetNamedValue(AttributeData? attribute, string name)
    {
        return attribute?.NamedArguments.FirstOrDefault(argument => argument.Key == name).Value.Value;
    }

    private static bool CanCreateFactory(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        if (typeSymbol.IsAbstract || typeSymbol.TypeKind == TypeKind.Interface)
        {
            return false;
        }

        return typeSymbol.InstanceConstructors.Any(static ctor => ctor.Parameters.Length == 0 &&
                                                                  IsAccessibleFromGeneratedCode(ctor.DeclaredAccessibility));
    }

    private static bool IsAccessibleFromGeneratedCode(Accessibility accessibility)
    {
        return accessibility is Accessibility.Public or Accessibility.Internal;
    }

    private static string GetGeneratedClassName(INamedTypeSymbol typeSymbol)
    {
        var metadataName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var builder      = new StringBuilder("DataMemberAccessorRegistration_");
        foreach (var ch in metadataName)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        return builder.ToString();
    }
}

internal sealed class DataMemberAccessorTypeInfo
{
    public string TypeName { get; }

    public string GeneratedClassName { get; }

    public bool CanCreateFactory { get; }

    public bool IsDataTypeReadOnly { get; }

    public IReadOnlyList<DataMemberAccessorPropertyInfo> Properties { get; }

    public DataMemberAccessorTypeInfo(string typeName,
                                      string generatedClassName,
                                      bool canCreateFactory,
                                      bool isDataTypeReadOnly,
                                      IReadOnlyList<DataMemberAccessorPropertyInfo> properties)
    {
        TypeName           = typeName;
        GeneratedClassName = generatedClassName;
        CanCreateFactory   = canCreateFactory;
        IsDataTypeReadOnly = isDataTypeReadOnly;
        Properties         = properties;
    }
}

internal sealed class DataMemberAccessorPropertyInfo
{
    public string Name { get; }

    public string TypeName { get; }

    public bool CanWrite { get; }

    public string? DisplayName { get; }

    public bool? AutoGenerateField { get; }

    public int? Order { get; }

    public bool IsReadOnly { get; }

    public bool IsEditable { get; }

    public DataMemberAccessorPropertyInfo(string name,
                                          string typeName,
                                          bool canWrite,
                                          string? displayName,
                                          bool? autoGenerateField,
                                          int? order,
                                          bool isReadOnly,
                                          bool isEditable)
    {
        Name              = name;
        TypeName          = typeName;
        CanWrite          = canWrite;
        DisplayName       = displayName;
        AutoGenerateField = autoGenerateField;
        Order             = order;
        IsReadOnly        = isReadOnly;
        IsEditable        = isEditable;
    }
}
