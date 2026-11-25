using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class Net8EngineToolchainTests
{
    [Theory]
    [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "8.0.27+a6bde67c", true)]
    [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "8.0.34+0123abcd", true)]
    [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "8.0.35-servicing.26500.1+ab", true)]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.8", false)]
    [InlineData("illink, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "7.0.20", false)]
    [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef", "8.0.34", false)]
    [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.8", false)]
    public void Linker_identity_requires_official_engine_major_8(string identity, string version, bool accepted)
        => Net8EngineToolchain.IsSupportedLinker(identity, version).ShouldBe(accepted);

    [Fact]
    public void Tasks_and_engine_must_be_one_servicing_release()
    {
        Net8EngineToolchain.IsSameEngine("8.0.34+aaa", "8.0.34+bbb").ShouldBeTrue();
        Net8EngineToolchain.IsSameEngine("8.0.34+aaa", "8.0.33+aaa").ShouldBeFalse();
    }

    [Theory]
    [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/8.0.27", true)]
    [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/8.0.34", true)]
    [InlineData("/dotnet/packs/runtime.osx-arm64.Microsoft.DotNet.ILCompiler/8.0.34", true)]
    [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/10.0.15", false)]
    [InlineData("/p/runtime.osx-x64.microsoft.dotnet.ilcompiler/8.0.34", false)]
    [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/not-a-version", false)]
    public void Native_compiler_package_requires_official_ilc_major_8(string directory, bool accepted)
        => Net8EngineToolchain.IsSupportedNativeCompilerPackage(directory).ShouldBe(accepted);

    [Fact]
    public void Native_compiler_launcher_rolls_forward_by_major()
    {
        var launcher = PrepareNative8RegistrationTask.CreateCompilerLauncher("/usr/local/share/dotnet/dotnet", ["/owned", "id"]);
        launcher.ShouldContain("'/usr/local/share/dotnet/dotnet' exec --roll-forward Major \"$base/host/ilc.dll\" \"$@\"");
        launcher.ShouldContain("'/usr/local/share/dotnet/dotnet' exec --roll-forward Major \"$base/host/ilc.dll\" --atomui-cleanup-invocation '/owned' 'id'");
    }

    [Fact]
    public void Prepared_host_rolls_forward_by_major()
    {
        using var config = JsonDocument.Parse(PrepareNet8LinkHostTask.HostConfig);
        config.RootElement.GetProperty("runtimeOptions").GetProperty("rollForward").GetString().ShouldBe("Major");
    }
}
