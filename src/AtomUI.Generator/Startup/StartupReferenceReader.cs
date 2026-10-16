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
        if (reference is not PortableExecutableReference portable || portable.GetMetadata() is not AssemblyMetadata metadata)
        {
            return null;
        }
        try
        {
            var reader = metadata.GetModules()[0].GetMetadataReader();
            var definition = reader.GetAssemblyDefinition();
            var assemblyName = reader.GetString(definition.Name);
            string? packageId = null;
            string? packageData = null;
            var version = 0;
            var languages = new List<StartupLanguageReference>();
            var themes = new List<StartupThemeReference>();
            var hasPackageMarker = false;
            foreach (var handle in definition.GetCustomAttributes())
            {
                token.ThrowIfCancellationRequested();
                var attribute = reader.GetCustomAttribute(handle);
                var name = AttributeName(reader, attribute.Constructor);
                if (name == "AtomUI.Registration.ControlPackageMarkerAttribute")
                {
                    hasPackageMarker = true;
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
                if (name == "AtomUI.Registration.ControlPackageManifestAttribute")
                {
                    if (packageId is not null)
                    {
                        throw new InvalidDataException("Assembly contains multiple Control package manifests.");
                    }
                    packageId = id;
                    packageData = data;
                    version = format;
                }
                else if (name == "AtomUI.Theme.Definitions.Compiled.CompiledThemeManifestAttribute")
                {
                    themes.Add(new StartupThemeReference(format, id, data));
                }
                else
                {
                    languages.Add(new StartupLanguageReference(format, id, data));
                }
            }
            if (hasPackageMarker && packageId is null)
            {
                return new StartupReferenceData(assemblyName, 0, null, null, default,
                    "Control package has no startup manifest; rebuild it with the current AtomUI generator.");
            }
            return packageId is null && languages.Count == 0 && themes.Count == 0 ? null :
                new StartupReferenceData(assemblyName, version, packageId, packageData, new(languages), null, new(themes));
        }
        catch (Exception error) when (error is BadImageFormatException || error is InvalidDataException || error is ArgumentException || error is EndOfStreamException)
        {
            return new StartupReferenceData(reference.Display ?? "reference", 0, null, null, default, error.Message);
        }
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
