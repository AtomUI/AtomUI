using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

// Format 1 uses canonical JSON values in string-only assembly records. These definitions are
// build inputs, never a runtime registry or a second source of control/asset dependency facts.
internal static class ConditionalRegistrationWriter
{
    internal const string CollectorTemplateId = "atomui.collector.v1";
    internal const string RequiredCapability = "atomui.conditional.v1";
    private const int MaximumIdentityLength = 65536;
    private const int MaximumPayloadLength = 1048576;
    private const int MaximumRecordCount = 100000;

    internal static string RegistrationMethodName(RegistrationControl control) =>
        "Register_" + RegistrationNames.Hash(control.Type.AssemblyQualifiedName);

    internal static void WriteCollector(StringBuilder source, RegistrationPackage package)
    {
        const string parameter = "global::AtomUI.Registration.ControlPackageRegistrationBuilder builder";
        source.Append("    private static void Collect(").Append(parameter).AppendLine(") => CollectFull(builder);");
        source.Append("    private static void CollectFull(").Append(parameter).AppendLine(")\n    {");
        // 本方法体受 net8 ILLink/ILC8 后端的精确 IL 模板绑定（仅允许逐片段调用）：
        // 禁止加入指纹赋值等任何非调用语句。包级指纹只属于 TypeMap 路径；条件路径保持
        // PackageSchemaFingerprint = null，运行时按全有或全无语义回退全量 Revision 计算。
        foreach (var control in package.Controls)
        {
            source.Append("        ").Append(RegistrationMethodName(control)).AppendLine("(builder);");
        }
        source.AppendLine("    }");
        source.Append("    private static void CollectSelected(").Append(parameter).AppendLine(")\n    {");
        source.AppendLine("        throw new global::System.InvalidOperationException(\"AtomUI conditional registration was not materialized by a supported publish backend.\");");
        source.AppendLine("    }");
        foreach (var control in package.Controls)
        {
            source.Append("    internal static void ").Append(RegistrationMethodName(control)).Append('(').Append(parameter).AppendLine(")\n    {");
            source.Append("        builder.AddFragment(new ").Append(RegistrationNames.Proxy(control)).AppendLine("());");
            source.AppendLine("    }");
        }
    }

    internal static bool Write(GenerationOutput output, RegistrationPackage package, string groupName)
    {
        if (package.RegistrationBuilderType is null || package.VoidType is null)
        {
            Report(output, "Conditional registration requires bound builder and return types.");
            return false;
        }

        var groupNameWithNamespace = package.Namespace + "." + groupName;
        var groupIdentity = groupNameWithNamespace + ", " + package.AssemblyIdentity;
        var group = NamedType(package.AssemblyIdentity, groupNameWithNamespace);
        var collector = NamedType(package.AssemblyIdentity, package.Namespace + ".GeneratedControlPackageRegistration");
        var records = new List<Record>();
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var packagePayload = Fields(
            ("assembly", String(package.AssemblyIdentity)),
            ("collect", Method(collector, "Collect", package)),
            ("collectFull", Method(collector, "CollectFull", package)),
            ("collectSelected", Method(collector, "CollectSelected", package)),
            ("collectorTemplateId", String(CollectorTemplateId)),
            ("group", group),
            ("packageId", String(package.Id)),
            ("requiredCapability", String(RequiredCapability)));
        records.Add(new("package", groupIdentity, packagePayload));

        for (var order = 0; order < package.Controls.Count; order++)
        {
            var control = package.Controls[order];
            if (!ValidateTriggers(output, control))
            {
                return false;
            }

            var fragmentId = package.Id + ":" + control.Type.AssemblyQualifiedName;
            var fragmentIdentity = groupIdentity + "\n" + fragmentId;
            records.Add(new("fragment", fragmentIdentity, Fields(
                ("fragmentId", String(fragmentId)), ("group", String(groupIdentity)),
                ("order", order.ToString(CultureInfo.InvariantCulture)),
                ("proxy", NamedType(package.AssemblyIdentity, package.Namespace + "." + RegistrationNames.Proxy(control))),
                ("registrationMethod", Method(collector, RegistrationMethodName(control), package)))));
            foreach (var trigger in control.ConditionalTriggers)
            {
                var identity = fragmentIdentity + "\n" + trigger.Type.AssemblyQualifiedName + "\n" + trigger.Kind;
                records.Add(new("condition", identity, Fields(
                    ("fragment", String(fragmentIdentity)), ("group", String(groupIdentity)),
                    ("kind", String(trigger.Kind)), ("trigger", Type(trigger.Type)))));
            }
        }

        if (records.Count > MaximumRecordCount)
        {
            Report(output, "Conditional registration exceeds the format 1 record count limit.");
            return false;
        }
        foreach (var record in records)
        {
            if (!identities.Add(record.Kind + "\n" + record.Identity))
            {
                output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationIdentityConflict,
                    Location.None, "Duplicate conditional registration record: " + record.Identity));
                return false;
            }
            if (record.Identity.Length > MaximumIdentityLength || Object(record.Payload).Length > MaximumPayloadLength - 128)
            {
                Report(output, "Conditional registration exceeds the format 1 record size limit.");
                return false;
            }
        }

        var ordered = records.OrderBy(record => record.Kind, StringComparer.Ordinal)
            .ThenBy(record => record.Identity, StringComparer.Ordinal).ToArray();
        // The package record has no recordsHash yet. The final IL/assembly hash belongs to the
        // publish snapshot, not to this generator, which runs before compilation/XAML rewriting.
        var canonicalRecords = "[" + string.Join(",", ordered.Select(record => Object(Fields(
            ("format", "1"), ("identity", String(record.Identity)),
            ("kind", String(record.Kind)), ("payload", Object(record.Payload)))))) + "]";
        using (var sha = SHA256.Create())
        {
            packagePayload.Add("recordsHash", String(string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(canonicalRecords))
                .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)))));
        }

        var source = new StringBuilder("// <auto-generated />\n");
        foreach (var record in ordered)
        {
            source.Append("[assembly: global::AtomUI.Registration.ConditionalRegistrationRecord(1, ")
                .Append(RegistrationNames.Literal(record.Kind)).Append(", ")
                .Append(RegistrationNames.Literal(record.Identity)).Append(", ")
                .Append(RegistrationNames.Literal(Object(record.Payload))).AppendLine(")]");
        }
        output.AddSource("GeneratedConditionalRegistration.g.cs", GeneratedSourceText.From(source.ToString()));
        return true;
    }

    private static bool ValidateTriggers(GenerationOutput output, RegistrationControl control)
    {
        if (control.ConditionalTriggers.Count == 0 ||
            !control.Triggers.OrderBy(name => name, StringComparer.Ordinal).SequenceEqual(
                control.ConditionalTriggers.Select(trigger => trigger.Type.Name).OrderBy(name => name, StringComparer.Ordinal)))
        {
            Report(output, "Conditional registration has unknown or unbound trigger identities for " + control.Type.AssemblyQualifiedName);
            return false;
        }
        foreach (var trigger in control.ConditionalTriggers)
        {
            var type = trigger.Type;
            if (trigger.Kind is not ("control" or "token" or "style") ||
                string.IsNullOrWhiteSpace(type.MetadataName) || type.MetadataName.IndexOfAny(new[] { '`', '[', ']', '&', '*', ',' }) >= 0 ||
                !type.AssemblyQualifiedName.StartsWith(type.MetadataName + ", ", StringComparison.Ordinal) ||
                !AssemblyIdentity.TryParseDisplayName(type.AssemblyIdentity, out var assembly) ||
                assembly.GetDisplayName() != type.AssemblyIdentity)
            {
                Report(output, "Unsupported conditional registration trigger: " + type.AssemblyQualifiedName);
                return false;
            }
        }
        foreach (var conflict in control.ConditionalTriggers.GroupBy(trigger => trigger.Type.Name, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationIdentityConflict,
                Location.None, "Conflicting conditional registration trigger: " + conflict.Key));
            return false;
        }
        return true;
    }

    private static void Report(GenerationOutput output, string message) => output.ReportDiagnostic(
        Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, Location.None, message));

    private static string Method(string declaringType, string name, RegistrationPackage package) => Object(Fields(
        ("callingConvention", String("static")), ("declaringType", declaringType), ("genericArity", "0"),
        ("name", String(name)), ("parameters", "[" + Type(package.RegistrationBuilderType!) + "]"),
        ("returnType", Type(package.VoidType!))));

    private static string Type(RegistrationType type) => NamedType(type.AssemblyIdentity, type.MetadataName);
    private static string NamedType(string assembly, string metadataName) => Object(Fields(
        ("assembly", String(assembly)), ("metadataName", String(metadataName))));
    private static SortedDictionary<string, string> Fields(params (string Key, string Value)[] fields) =>
        new(fields.ToDictionary(field => field.Key, field => field.Value, StringComparer.Ordinal), StringComparer.Ordinal);
    private static string Object(SortedDictionary<string, string> fields) =>
        "{" + string.Join(",", fields.Select(field => String(field.Key) + ":" + field.Value)) + "}";

    private static string String(string value)
    {
        var result = new StringBuilder("\"");
        foreach (var ch in value)
        {
            if (ch == '"' || ch == '\\')
            {
                result.Append('\\').Append(ch);
            }
            else if (ch < 0x20)
            {
                result.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
            }
            else
            {
                result.Append(ch);
            }
        }
        return result.Append('"').ToString();
    }

    private sealed record Record(string Kind, string Identity, SortedDictionary<string, string> Payload);
}
