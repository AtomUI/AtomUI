using AtomUI.TypeMap.Linker.ConditionalBridge;
using Xunit;

namespace AtomUI.TypeMap.Linker.Tests;

public class FrameworkBindingPolicyTests
{
    [Theory]
    [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "10.0.8", true)]
    [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "10.0.15", true)]
    [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "11.0.0", true)]
    [InlineData("Microsoft.NETCore.App.Runtime.Mono.browser-wasm", "11.0.0-rc.2.26473.1", true)]
    // NuGet restores packs that the SDK does not bundle into the lowercase global-packages layout.
    [InlineData("microsoft.netcore.app.runtime.osx-arm64", "10.0.15", true)]
    [InlineData("microsoft.netcore.app.runtime.mono.browser-wasm", "11.0.0", true)]
    [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "9.0.23", false)]
    [InlineData("Some.Other.Pack", "10.0.8", false)]
    public void Runtime_pack_admission_uses_family_and_major(string name, string version, bool accepted)
        => Assert.Equal(accepted, FrameworkBindingPolicy.IsAdmittedRuntimePack(name, version));

    [Theory]
    [InlineData("10.0", "10.0.15", true)]
    [InlineData("11.0", "11.0.0", true)]
    [InlineData("10.0", "11.0.0", false)]
    [InlineData("9.0", "9.0.23", false)]
    public void Runtime_list_must_match_pack_framework(string listVersion, string packVersion, bool accepted)
        => Assert.Equal(accepted, FrameworkBindingPolicy.IsMatchingRuntimeList(listVersion, packVersion));

    [Theory]
    [InlineData("10.0", "10.0.0.0")]
    [InlineData("11.0", "11.0.0.0")]
    public void Net8_platform_references_bind_to_the_target_framework_assembly_version(string listVersion, string expected)
        => Assert.Equal(Version.Parse(expected), FrameworkBindingPolicy.FrameworkAssemblyVersion(listVersion));
}
