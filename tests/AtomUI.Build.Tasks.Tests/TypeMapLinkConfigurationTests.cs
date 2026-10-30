using System.Text.Json;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class TypeMapLinkConfigurationTests
{
    [Fact]
    public void Only_ignored_compiler_warning_codes_and_set_order_are_normalized_for_NoWarn()
    {
        var first = TypeMapLinkConfiguration.Capture([new TestTaskItem("NoWarn", ("Value", "CS1591;IL2027;IL02026;1701;IL2026;8002"))], [], [], []);
        var same = TypeMapLinkConfiguration.Capture([new TestTaskItem("NoWarn", ("Value", "IL2026;IL2027"))], [], [], []);
        var changed = TypeMapLinkConfiguration.Capture([new TestTaskItem("NoWarn", ("Value", "IL2026;IL2028"))], [], [], []);
        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(same));
        JsonSerializer.Serialize(first).ShouldNotBe(JsonSerializer.Serialize(changed));
    }

    [Fact]
    public void Configuration_values_cannot_collide_through_delimiters()
    {
        var first = TypeMapLinkConfiguration.Capture([new TestTaskItem("ExtraArgs", ("Value", "a|b=c;d"))], [], [], []);
        var second = TypeMapLinkConfiguration.Capture([new TestTaskItem("ExtraArgs|a", ("Value", "b=c;d"))], [], [], []);
        JsonSerializer.Serialize(first).ShouldNotBe(JsonSerializer.Serialize(second));
    }

    [Fact]
    public void Ordered_roots_and_steps_keep_their_order_and_all_consumed_metadata()
    {
        var first = new TestTaskItem("A", ("Kind", "RootAssemblyNames"), ("RootMode", "All"));
        var second = new TestTaskItem("B", ("Kind", "RootAssemblyNames"), ("RootMode", "EntryPoint"));
        var step = new TestTaskItem("tool.dll", ("Kind", "CustomSteps"), ("Type", "Fixture.Step"), ("BeforeStep", "MarkStep"));
        var value = TypeMapLinkConfiguration.Capture([], [first, second, step], [], []);
        value.Items[0].Metadata["RootMode"].ShouldBe("All");
        value.Items[2].Metadata["Type"].ShouldBe("Fixture.Step");
        value.Items[2].Metadata["BeforeStep"].ShouldBe("MarkStep");
        JsonSerializer.Serialize(value).ShouldNotBe(JsonSerializer.Serialize(TypeMapLinkConfiguration.Capture([], [second, first, step], [], [])));
    }

    [Fact]
    public void Assembly_signature_includes_every_supported_per_assembly_parameter()
    {
        var settings = new[] { "TrimMode", "IsTrimmable", "BeforeFieldInit", "OverrideRemoval", "UnreachableBodies", "UnusedInterfaces", "IPConstProp", "Sealer", "TrimmerSingleWarn" };
        var item = new TestTaskItem("input.dll", settings.Select((name, index) => (name, index.ToString())).ToArray());
        var configuration = TypeMapLinkConfiguration.Capture([], [], [item], []);
        configuration.Items.ShouldHaveSingleItem().Metadata.Keys.ShouldBe(settings);
        configuration.Items[0].Metadata["Sealer"].ShouldBe("7");
    }

    [Theory]
    [InlineData("--substitutions \"/path with spaces/a.xml\"", "--substitutions", "/path with spaces/a.xml")]
    [InlineData("-d \"C:\\folder with spaces\\\\\"", "-d", "C:\\folder with spaces\\")]
    [InlineData("--custom-data \"name=a\"\"b\"", "--custom-data", "name=a\"b")]
    public void File_option_tokenization_matches_response_file_quoting(string text, string first, string second) =>
        TypeMapLinkConfiguration.SplitArguments(text).ShouldBe(new[] { first, second });

    [Fact]
    public void File_backed_options_tool_dependencies_and_input_symbols_are_bound()
    {
        var root = Path.Combine(Path.GetTempPath(), "atomui-link-files-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var input = Path.Combine(root, "input.dll");
            var dependency = Path.Combine(root, "dependency.dll");
            File.Copy(typeof(TypeMapLinkConfigurationTests).Assembly.Location, input);
            File.Copy(input, dependency);
            var pdb = Path.ChangeExtension(input, ".pdb"); File.WriteAllText(pdb, "input symbols");
            var xml = Path.Combine(root, "attributes with spaces.xml"); File.WriteAllText(xml, "<linker/>");
            var list = Path.Combine(root, "attributes.list"); File.WriteAllText(list, xml);
            var deps = Path.ChangeExtension(input, ".deps.json");
            File.WriteAllText(deps, "{\"targets\":{\"framework\":{\"tool\":{\"runtime\":{\"dependency.dll\":{}}}}}}");
            var configuration = TypeMapLinkConfiguration.Capture(
                [new TestTaskItem("ExtraArgs", ("Value", "--link-attributes \"@" + list + "\""))],
                [new TestTaskItem(input, ("Kind", "CustomSteps"), ("Type", "Fixture.Step"))], [new TestTaskItem(input)], []);
            var files = configuration.InputFiles().ToArray();
            files.ShouldContain(input); files.ShouldContain(pdb); files.ShouldContain(deps); files.ShouldContain(dependency);
            files.ShouldContain(list); files.ShouldContain(xml);
            var before = files.Select(TypeMapBuildContract.Hash).ToArray();
            File.AppendAllText(dependency, "changed while preserving identity");
            files.Select(TypeMapBuildContract.Hash).ShouldNotBe(before);
        }
        finally { Directory.Delete(root, true); }
    }
}
