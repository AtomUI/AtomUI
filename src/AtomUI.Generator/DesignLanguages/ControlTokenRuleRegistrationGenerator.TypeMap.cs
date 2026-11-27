using System.Text;

namespace AtomUI.Generator.DesignLanguages;

public sealed partial class ControlTokenRuleRegistrationGenerator
{
    private const string PackageType = "global::AtomUI.Theme.Languages.ControlValuePackage";
    private const string Runtime = "global::AtomUI.Registration.ControlRegistrationRuntime";
    private const string TypeMapping = "global::System.Runtime.InteropServices.TypeMapping";
    private const string Fragment = "global::AtomUI.Registration.ControlValueFragmentAttribute";

    private static void WriteConditionalRegistration(StringBuilder source, string targetNamespace, IReadOnlyList<RuleEntry> entries,
        IReadOnlyList<(string Owner, string Language)> packages, string assemblyName, bool deferExplicitRegistration,
        bool hasBehaviors = false, bool hasThemePackage = false)
    {
        var groups = packages.Select(pair => entries.Where(entry => entry.Owner == pair.Owner && entry.Language == pair.Language).ToArray()).ToList();
        var local = packages.Count == 0 ? entries.ToArray() : entries.Where(entry => entry.Owner == assemblyName).ToArray();
        if (local.Length != 0)
        {
            groups.Add(local);
        }

        var owner = "global::" + targetNamespace + "." + RegistrationClassName + ".";
        source.AppendLine("#pragma warning disable IL2026, IL3050, SYSLIB5004 // Official NativeAOT TypeMap metadata.");
        if (packages.Count != 0)
        {
            WriteMapMarker(source, owner, "PackageMap");
        }
        for (var index = 0; index < packages.Count; index++)
        {
            WriteMapping(source, owner, "PackageMap", PackageMember(index, packages.Count), "PackageFragment" + index, packages[index].Language);
        }
        for (var group = 0; group < groups.Count; group++)
        {
            WriteMapMarker(source, owner, "RuleMap" + group);
            for (var index = 0; index < groups[group].Length; index++)
            {
                WriteMapping(source, owner, "RuleMap" + group, RuleKey(index), $"RuleFragment{group}_{index}", groups[group][index].Token);
            }
        }
        source.AppendLine("#pragma warning restore IL2026, IL3050, SYSLIB5004");
        source.Append("namespace ").Append(targetNamespace).AppendLine(";");
        source.Append("public static class ").AppendLine(RegistrationClassName).AppendLine("{");
        WriteExplicitRegistrationStart(source, deferExplicitRegistration);
        for (var group = 0; group < groups.Count; group++)
        {
            source.Append("        Register").Append(group).AppendLine("(builder);");
        }
        if (hasBehaviors)
        {
            source.AppendLine("        " + ControlBehaviorRegistrationGenerator.Registration + ".Register(builder);");
        }
        source.AppendLine("    }");

        source.AppendLine("    #pragma warning disable IL2026, IL3050, SYSLIB5004 // Closed NativeAOT TypeMap accessors.");
        if (packages.Count != 0)
        {
            WriteMapType(source, "PackageMap");
            source.Append("    private static class LinkedPackages { internal static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::System.Type> Map = ")
                .Append(TypeMapping).AppendLine(".GetOrCreateExternalTypeMapping<PackageMap>(); }");
            for (var index = 0; index < packages.Count; index++)
            {
                var (controlAssembly, language) = packages[index];
                var member = PackageMember(index, packages.Count);
                source.Append("    public static ").Append(PackageType).Append(' ').Append(member).Append(" => PackageHolder").Append(index).AppendLine(".Value;");
                source.Append("    public static ").Append(PackageType).Append("? Resolve").Append(member).Append("() => ")
                    .Append(Runtime).Append(".UsesNativeValueRules ? ").Append(Fragment).Append('<').Append(PackageType)
                    .Append(">.Resolve(LinkedPackages.Map, ").Append(ThemeBuildContracts.Literal(member)).Append(") : ").Append(member).AppendLine(";");
                source.Append("    private static class PackageHolder").Append(index).AppendLine();
                source.AppendLine("    {");
                source.Append("        internal static readonly ").Append(PackageType).Append(" Value = new(")
                    .Append(ThemeBuildContracts.Literal(assemblyName)).Append(", ").Append(ThemeBuildContracts.Literal(controlAssembly))
                    .Append(", typeof(").Append(language).Append("), ")
                    .Append(hasBehaviors ? "RegisterPackage" + index : "Register" + index)
                    .AppendLine(hasThemePackage ? ", PackageMetadata.Definition);" : ");");
                source.AppendLine("    }");
                if (hasBehaviors)
                {
                    source.Append("    private static void RegisterPackage").Append(index)
                        .AppendLine("(global::AtomUI.Theme.IThemeManagerBuilder builder)");
                    source.AppendLine("    {");
                    source.Append("        Register").Append(index).AppendLine("(builder);");
                    source.AppendLine("        " + ControlBehaviorRegistrationGenerator.Registration + ".Register(builder);");
                    source.AppendLine("    }");
                }
                source.Append("    [PackageFragment").Append(index).Append("] internal sealed class PackageFragment").Append(index)
                    .Append(" : ").Append(Fragment).Append('<').Append(PackageType).AppendLine(">");
                source.AppendLine("    {");
                source.Append("        public override ").Append(PackageType).Append(" Value => ").Append(member).AppendLine(";");
                source.AppendLine("    }");
            }
        }
        for (var group = 0; group < groups.Count; group++)
        {
            WriteConditionalGroup(source, group, groups[group]);
        }
        source.AppendLine("    #pragma warning restore IL2026, IL3050, SYSLIB5004");
        source.AppendLine("}");
    }

    private static void WriteConditionalGroup(StringBuilder source, int group, IReadOnlyList<RuleEntry> entries)
    {
        source.Append("    private static void Register").Append(group).Append("(global::AtomUI.Theme.IThemeManagerBuilder builder) => builder.AddControlTokenRules(")
            .Append(Runtime).Append(".UsesNativeValueRules ? LinkedRules").Append(group).Append(".Items : FullRules").Append(group).AppendLine(".Items);");
        WriteGroup(source, "FullRules" + group, entries);
        WriteMapType(source, "RuleMap" + group);
        source.Append("    private static class LinkedRules").Append(group).AppendLine();
        source.AppendLine("    {");
        source.Append("        internal static readonly global::System.Collections.Generic.IReadOnlyList<").Append(DescriptorType)
            .Append("> Items = global::AtomUI.Registration.ControlValueRegistration.ResolveRules(").Append(TypeMapping)
            .Append(".GetOrCreateExternalTypeMapping<RuleMap").Append(group).Append(">(), [")
            .Append(string.Join(", ", Enumerable.Range(0, entries.Count).Select(i => ThemeBuildContracts.Literal(RuleKey(i))))).AppendLine("]);");
        source.AppendLine("    }");
        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            var proxy = $"RuleFragment{group}_{index}";
            source.Append("    [").Append(proxy).Append("] internal sealed class ").Append(proxy).Append(" : ").Append(Fragment)
                .Append('<').Append(DescriptorType).AppendLine(">");
            source.AppendLine("    {");
            source.Append("        public override ").Append(DescriptorType).Append(" Value => ");
            WriteCreateRule(source, entry).AppendLine(";");
            source.AppendLine("    }");
        }
    }

    private static void WriteMapping(StringBuilder source, string owner, string group, string key, string proxy, string trigger)
    {
        source.Append("[assembly: global::System.Runtime.InteropServices.TypeMap<").Append(owner).Append(group).Append(">(")
            .Append(ThemeBuildContracts.Literal(key)).Append(", typeof(").Append(owner).Append(proxy)
            .Append("), typeof(").Append(trigger).AppendLine("))]");
    }

    private static void WriteMapMarker(StringBuilder source, string owner, string group) =>
        source.Append("[assembly: global::AtomUI.Registration.ControlValueMap(typeof(").Append(owner).Append(group).AppendLine("))]");

    private static void WriteMapType(StringBuilder source, string name)
    {
        source.AppendLine("    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        source.Append("    public sealed class ").Append(name).Append(" { private ").Append(name).AppendLine("() { } }");
    }

    private static string RuleKey(int index) => index.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
