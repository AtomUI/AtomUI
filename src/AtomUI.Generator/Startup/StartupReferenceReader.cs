using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed record StartupLanguageReference(int Version, string ModuleId, string Data);
internal sealed record StartupThemeReference(int Version, string PackageId, string Data);
internal sealed record StartupReferenceData(string AssemblyName, int Version, string? PackageId, string? PackageData,
    ValueArray<StartupLanguageReference> Languages, string? Error, ValueArray<StartupThemeReference> Themes = default);

/// <summary>Reads raw string contracts per immutable metadata reference, cached across source edits.</summary>
internal static class StartupReferenceReader
{
    internal static IncrementalValueProvider<ImmutableArray<StartupReferenceData>> Create(
        IncrementalGeneratorInitializationContext context) =>
        context.MetadataReferencesProvider.Select(static (reference, token) => Read(reference, token))
            .Where(static data => data is not null).Select(static (data, _) => data!).Collect()
            .Select(static (data, _) => data.Distinct().ToImmutableArray())
            .WithTrackingName("StartupReferenceManifests");

    private static StartupReferenceData? Read(MetadataReference reference, CancellationToken token)
    {
        try
        {
            // IDE project references can expose a live compilation instead of a PE image.
            // Read its assembly attributes directly; emitting it would add build work to editing.
            if (reference is CompilationReference compilationReference)
            {
                return ReadCompilation(compilationReference.Compilation.Assembly, token);
            }
            if (reference is not PortableExecutableReference portable || portable.GetMetadata() is not AssemblyMetadata metadata)
            {
                return null;
            }
            var reader = metadata.GetModules()[0].GetMetadataReader();
            var definition = reader.GetAssemblyDefinition();
            var assemblyName = reader.GetString(definition.Name);
            ManifestCollector? manifests = null;
            foreach (var handle in definition.GetCustomAttributes())
            {
                token.ThrowIfCancellationRequested();
                var attribute = reader.GetCustomAttribute(handle);
                var name = AttributeName(reader, attribute.Constructor);
                if (name == "AtomUI.Registration.ControlPackageMarkerAttribute")
                {
                    (manifests ??= new ManifestCollector(assemblyName)).HasPackageMarker = true;
                    continue;
                }
                if (name is not ("AtomUI.Registration.ControlPackageManifestAttribute" or
                    "AtomUI.Localization.LanguageModuleManifestAttribute" or
                    "AtomUI.Theme.Definitions.Compiled.CompiledThemeManifestAttribute"))
                {
                    continue;
                }
                var blob = reader.GetBlobReader(attribute.Value);
                if (blob.ReadUInt16() != 1)
                {
                    throw new InvalidDataException("Invalid startup manifest attribute encoding.");
                }
                var format = blob.ReadInt32();
                var id = blob.ReadSerializedString() ?? throw new InvalidDataException("Missing startup manifest identity.");
                var data = blob.ReadSerializedString() ?? throw new InvalidDataException("Missing startup manifest data.");
                if (blob.ReadUInt16() != 0 || blob.RemainingBytes != 0)
                {
                    throw new InvalidDataException("Startup manifest contains unsupported named fields.");
                }
                (manifests ??= new ManifestCollector(assemblyName)).Add(name, format, id, data);
            }
            return manifests?.Complete();
        }
        catch (Exception error) when (error is BadImageFormatException || error is InvalidDataException || error is ArgumentException || error is EndOfStreamException)
        {
            return new StartupReferenceData(reference.Display ?? "reference", 0, null, null, default, error.Message);
        }
    }

    private static StartupReferenceData? ReadCompilation(IAssemblySymbol assembly, CancellationToken token)
    {
        ManifestCollector? manifests = null;
        foreach (var attribute in assembly.GetAttributes())
        {
            token.ThrowIfCancellationRequested();
            var name = attribute.AttributeClass?.ToDisplayString();
            if (name == "AtomUI.Registration.ControlPackageMarkerAttribute")
            {
                (manifests ??= new ManifestCollector(assembly.Name)).HasPackageMarker = true;
                continue;
            }
            if (name is not ("AtomUI.Registration.ControlPackageManifestAttribute" or
                "AtomUI.Localization.LanguageModuleManifestAttribute" or
                "AtomUI.Theme.Definitions.Compiled.CompiledThemeManifestAttribute"))
            {
                continue;
            }
            if (attribute.NamedArguments.Length != 0)
            {
                throw new InvalidDataException("Startup manifest contains unsupported named fields.");
            }
            var arguments = attribute.ConstructorArguments;
            if (arguments.Length != 3 || arguments[0].Kind != TypedConstantKind.Primitive ||
                arguments[0].Value is not int format || arguments[1].Kind != TypedConstantKind.Primitive ||
                arguments[2].Kind != TypedConstantKind.Primitive)
            {
                throw new InvalidDataException("Invalid startup manifest attribute encoding.");
            }
            var id = arguments[1].Value as string ?? throw new InvalidDataException("Missing startup manifest identity.");
            var data = arguments[2].Value as string ?? throw new InvalidDataException("Missing startup manifest data.");
            (manifests ??= new ManifestCollector(assembly.Name)).Add(name, format, id, data);
        }
        return manifests?.Complete();
    }

    // Both reference forms share the same duplicate/marker checks and immutable output.
    // Allocate only for assemblies that actually contribute startup contracts.
    private sealed class ManifestCollector(string assemblyName)
    {
        private string? _packageId;
        private string? _packageData;
        private int _version;
        private readonly List<StartupLanguageReference> _languages = new();
        private readonly List<StartupThemeReference> _themes = new();

        internal bool HasPackageMarker { get; set; }

        internal void Add(string name, int format, string id, string data)
        {
            if (name == "AtomUI.Registration.ControlPackageManifestAttribute")
            {
                if (_packageId is not null)
                {
                    throw new InvalidDataException("Assembly contains multiple Control package manifests.");
                }
                _packageId = id;
                _packageData = data;
                _version = format;
            }
            else if (name == "AtomUI.Theme.Definitions.Compiled.CompiledThemeManifestAttribute")
            {
                _themes.Add(new StartupThemeReference(format, id, data));
            }
            else
            {
                _languages.Add(new StartupLanguageReference(format, id, data));
            }
        }

        internal StartupReferenceData Complete() => HasPackageMarker && _packageId is null
            ? new StartupReferenceData(assemblyName, 0, null, null, default,
                "Control package has no startup manifest; rebuild it with the current AtomUI generator.")
            : new StartupReferenceData(assemblyName, _version, _packageId, _packageData, new(_languages), null, new(_themes));
    }

    private static string? AttributeName(MetadataReader reader, EntityHandle constructor)
    {
        EntityHandle type;
        if (constructor.Kind == HandleKind.MemberReference)
        {
            type = reader.GetMemberReference((MemberReferenceHandle)constructor).Parent;
        }
        else if (constructor.Kind == HandleKind.MethodDefinition)
        {
            type = reader.GetMethodDefinition((MethodDefinitionHandle)constructor).GetDeclaringType();
        }
        else
        {
            return null;
        }
        return type.Kind switch
        {
            HandleKind.TypeReference => Name(reader, reader.GetTypeReference((TypeReferenceHandle)type)),
            HandleKind.TypeDefinition => Name(reader, reader.GetTypeDefinition((TypeDefinitionHandle)type)),
            _ => null
        };
    }

    private static string Name(MetadataReader reader, TypeReference type) =>
        reader.GetString(type.Namespace) + "." + reader.GetString(type.Name);
    private static string Name(MetadataReader reader, TypeDefinition type) =>
        reader.GetString(type.Namespace) + "." + reader.GetString(type.Name);
}
