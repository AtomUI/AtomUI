using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AtomUI.Build.Tasks.Registration;

internal sealed record RegistrationTypeIdentity(string Assembly, string MetadataName)
{
    internal string QualifiedName => MetadataName + ", " + Assembly;
}

internal sealed record RegistrationMethodIdentity(RegistrationTypeIdentity DeclaringType, string Name,
    RegistrationTypeIdentity ReturnType, IReadOnlyList<RegistrationTypeIdentity> Parameters)
{
    internal string QualifiedName => DeclaringType.QualifiedName + "::" + Name + "(" +
        string.Join(";", Parameters.Select(p => p.QualifiedName)) + ")->" + ReturnType.QualifiedName;
}

internal sealed record ConditionalRegistrationPackage(string Id, RegistrationTypeIdentity Group,
    RegistrationMethodIdentity Collect, RegistrationMethodIdentity CollectFull,
    RegistrationMethodIdentity CollectSelected, string RecordsHash);

internal sealed record ConditionalRegistrationFragment(string Identity, string FragmentId,
    RegistrationTypeIdentity Proxy, RegistrationMethodIdentity RegistrationMethod, int Order);

internal sealed record ConditionalRegistrationCondition(string Identity, string Fragment,
    RegistrationTypeIdentity Trigger, string Kind);

internal sealed record ConditionalRegistrationManifest(ConditionalRegistrationMetadata Metadata,
    ConditionalRegistrationPackage Package, IReadOnlyList<ConditionalRegistrationFragment> Fragments,
    IReadOnlyList<ConditionalRegistrationCondition> Conditions)
{
    internal static ConditionalRegistrationManifest Parse(ConditionalRegistrationMetadata metadata)
    {
        var documents = new List<(ConditionalRegistrationRecord Record, JsonDocument Document)>();
        try
        {
            foreach (var record in metadata.Records)
            {
                var document = JsonDocument.Parse(record.Payload, new JsonDocumentOptions { MaxDepth = 16 });
                documents.Add((record, document));
                RegistrationContractJson.ValidateShape(document.RootElement);
            }
            if (documents.Count(d => d.Record.Kind == "package") != 1)
            {
                throw new InvalidDataException("Conditional registration format 1 requires exactly one package record per assembly.");
            }
            var packageRecord = documents.Single(d => d.Record.Kind == "package");
            var package = ReadPackage(packageRecord.Record, packageRecord.Document.RootElement, metadata);
            var fragments = new List<ConditionalRegistrationFragment>();
            var conditions = new List<ConditionalRegistrationCondition>();
            foreach (var (record, document) in documents)
            {
                if (record.Kind == "fragment")
                {
                    fragments.Add(ReadFragment(record, document.RootElement, metadata, package));
                }
                else if (record.Kind == "condition")
                {
                    conditions.Add(ReadCondition(record, document.RootElement, package));
                }
                else if (record.Kind != "package" || record.Format != 1)
                {
                    throw new InvalidDataException("Unknown registration record format or kind.");
                }
            }
            ValidateRelations(package, fragments, conditions);
            var canonical = "[" + string.Join(",", documents.OrderBy(d => d.Record.Kind, StringComparer.Ordinal)
                .ThenBy(d => d.Record.Identity, StringComparer.Ordinal).Select(d =>
                    "{\"format\":1,\"identity\":" + RegistrationContractJson.Quote(d.Record.Identity) +
                    ",\"kind\":" + RegistrationContractJson.Quote(d.Record.Kind) + ",\"payload\":" +
                    RegistrationContractJson.Canonical(d.Document.RootElement, d.Record.Kind == "package" ? "recordsHash" : null) + "}")) + "]";
            var actualHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
            if (actualHash != package.RecordsHash)
            {
                throw new InvalidDataException($"Conditional registration records hash mismatch for '{metadata.AssemblyIdentity}'.");
            }
            return new(metadata, package, fragments.OrderBy(f => f.Order).ToArray(),
                conditions.OrderBy(c => c.Identity, StringComparer.Ordinal).ToArray());
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid conditional registration JSON.", error);
        }
        finally
        {
            foreach (var (_, document) in documents)
            {
                document.Dispose();
            }
        }
    }

    private static ConditionalRegistrationPackage ReadPackage(ConditionalRegistrationRecord record, JsonElement payload,
        ConditionalRegistrationMetadata metadata)
    {
        RegistrationContractJson.Fields(payload, "packageId", "assembly", "group", "collect", "collectFull", "collectSelected",
            "collectorTemplateId", "requiredCapability", "recordsHash");
        if (record.Format != 1 || RegistrationContractJson.String(payload, "assembly") != metadata.AssemblyIdentity ||
            RegistrationContractJson.String(payload, "collectorTemplateId") != "atomui.collector.v1" ||
            RegistrationContractJson.String(payload, "requiredCapability") != "atomui.conditional.v1")
        {
            throw new InvalidDataException("Unknown registration collector contract or defining assembly.");
        }
        var group = RegistrationContractJson.Type(payload.GetProperty("group"));
        if (group.Assembly != metadata.AssemblyIdentity || group.QualifiedName != record.Identity)
        {
            throw new InvalidDataException("Registration package identity does not match its defining Group.");
        }
        var package = new ConditionalRegistrationPackage(RegistrationContractJson.String(payload, "packageId"), group,
            Method(payload.GetProperty("collect"), metadata), Method(payload.GetProperty("collectFull"), metadata),
            Method(payload.GetProperty("collectSelected"), metadata), RegistrationContractJson.String(payload, "recordsHash"));
        if (package.Collect.Name != "Collect" || package.CollectFull.Name != "CollectFull" || package.CollectSelected.Name != "CollectSelected" ||
            package.Collect.DeclaringType != package.CollectFull.DeclaringType || package.Collect.DeclaringType != package.CollectSelected.DeclaringType ||
            package.Collect.ReturnType != package.CollectFull.ReturnType || package.Collect.ReturnType != package.CollectSelected.ReturnType ||
            package.RecordsHash.Length != 64 || package.RecordsHash.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
        {
            throw new InvalidDataException("Registration collector methods or records hash violate format 1.");
        }
        return package;
    }

    private static ConditionalRegistrationFragment ReadFragment(ConditionalRegistrationRecord record, JsonElement payload,
        ConditionalRegistrationMetadata metadata, ConditionalRegistrationPackage package)
    {
        RegistrationContractJson.Fields(payload, "group", "fragmentId", "proxy", "registrationMethod", "order");
        var fragmentId = RegistrationContractJson.String(payload, "fragmentId");
        var proxy = RegistrationContractJson.Type(payload.GetProperty("proxy"));
        if (record.Format != 1 || RegistrationContractJson.String(payload, "group") != package.Group.QualifiedName ||
            record.Identity != package.Group.QualifiedName + "\n" + fragmentId ||
            proxy.Assembly != metadata.AssemblyIdentity || !fragmentId.StartsWith(package.Id + ":", StringComparison.Ordinal) ||
            payload.GetProperty("order").ValueKind != JsonValueKind.Number ||
            !payload.GetProperty("order").TryGetInt32(out var order) || order < 0)
        {
            throw new InvalidDataException("Invalid registration fragment ownership, identity or collection order.");
        }
        return new(record.Identity, fragmentId, proxy, Method(payload.GetProperty("registrationMethod"), metadata), order);
    }

    private static ConditionalRegistrationCondition ReadCondition(ConditionalRegistrationRecord record, JsonElement payload,
        ConditionalRegistrationPackage package)
    {
        RegistrationContractJson.Fields(payload, "group", "fragment", "trigger", "kind");
        var fragment = RegistrationContractJson.String(payload, "fragment");
        var trigger = RegistrationContractJson.Type(payload.GetProperty("trigger"));
        var kind = RegistrationContractJson.String(payload, "kind");
        if (record.Format != 1 || RegistrationContractJson.String(payload, "group") != package.Group.QualifiedName ||
            kind is not ("control" or "token" or "style") || record.Identity != fragment + "\n" + trigger.QualifiedName + "\n" + kind)
        {
            throw new InvalidDataException("Invalid registration condition identity or Group.");
        }
        return new(record.Identity, fragment, trigger, kind);
    }

    private static RegistrationMethodIdentity Method(JsonElement payload, ConditionalRegistrationMetadata metadata)
    {
        RegistrationContractJson.Fields(payload, "declaringType", "name", "genericArity", "callingConvention", "returnType", "parameters");
        var declaringType = RegistrationContractJson.Type(payload.GetProperty("declaringType"));
        var returnType = RegistrationContractJson.Type(payload.GetProperty("returnType"));
        var name = RegistrationContractJson.String(payload, "name");
        var parameters = payload.GetProperty("parameters");
        if (declaringType.Assembly != metadata.AssemblyIdentity ||
            payload.GetProperty("genericArity").ValueKind != JsonValueKind.Number ||
            !payload.GetProperty("genericArity").TryGetInt32(out var arity) || arity != 0 ||
            RegistrationContractJson.String(payload, "callingConvention") != "static" || returnType.MetadataName != "System.Void" ||
            parameters.ValueKind != JsonValueKind.Array || parameters.GetArrayLength() != 1 ||
            name.Any(c => char.IsWhiteSpace(c) || c is '(' or ')' or ':' or '`'))
        {
            throw new InvalidDataException("Registration method must be an owned, non-generic static void method with a builder parameter.");
        }
        var builder = RegistrationContractJson.Type(parameters[0]);
        if (builder.MetadataName != "AtomUI.Registration.ControlPackageRegistrationBuilder" || builder.Assembly != metadata.ContractAssemblyIdentity)
        {
            throw new InvalidDataException("Registration method builder has the wrong contract identity.");
        }
        return new(declaringType, name, returnType, new[] { builder });
    }

    private static void ValidateRelations(ConditionalRegistrationPackage package,
        IReadOnlyList<ConditionalRegistrationFragment> fragments, IReadOnlyList<ConditionalRegistrationCondition> conditions)
    {
        var ids = fragments.Select(f => f.Identity).ToHashSet(StringComparer.Ordinal);
        var referencedFragments = conditions.Select(c => c.Fragment).ToHashSet(StringComparer.Ordinal);
        if (ids.Count != fragments.Count || fragments.Select(f => f.Proxy).Distinct().Count() != fragments.Count ||
            fragments.Select(f => f.RegistrationMethod.QualifiedName).Distinct(StringComparer.Ordinal).Count() != fragments.Count ||
            !fragments.Select(f => f.Order).Order().SequenceEqual(Enumerable.Range(0, fragments.Count)) ||
            conditions.Select(c => c.Identity).Distinct(StringComparer.Ordinal).Count() != conditions.Count ||
            !ids.SetEquals(referencedFragments))
        {
            throw new InvalidDataException("Conflicting or incomplete conditional registration fragment/condition graph.");
        }
        var collectorNames = new[] { package.Collect.QualifiedName, package.CollectFull.QualifiedName, package.CollectSelected.QualifiedName };
        if (fragments.Any(f => collectorNames.Contains(f.RegistrationMethod.QualifiedName, StringComparer.Ordinal)))
        {
            throw new InvalidDataException("Registration thunk cannot alias a collector.");
        }
    }
}
