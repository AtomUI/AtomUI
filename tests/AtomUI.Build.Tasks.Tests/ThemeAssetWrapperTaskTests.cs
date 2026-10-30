using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public sealed class ThemeAssetWrapperTaskTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        $"atomui-theme-asset-wrapper-tests-{Guid.NewGuid():N}");

    public ThemeAssetWrapperTaskTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Fact]
    public void Execute_Generates_Wrappers_For_Default_Typed_Control_Themes()
    {
        var themePath = Write(
            Path.Combine("Themes", "ButtonTheme.axaml"),
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                          xmlns:controls="using:Acme.Controls"
                          x:Class="Acme.Controls.ButtonTheme"
                          TargetType="controls:Button" />
            """);
        var outputDirectory = Path.Combine(_directory, "obj", "AtomUIThemeAssets");
        var generatedCodePath = Path.Combine(outputDirectory, "GeneratedThemeAssetResources.g.cs");
        var task = new GenerateThemeAssetWrappersTask
        {
            BuildEngine = new RecordingBuildEngine(),
            ThemeAssets =
            [
                new TestTaskItem(
                    themePath,
                    ("FullPath", themePath),
                    ("Link", "Themes/ButtonTheme.axaml"))
            ],
            OutputDirectory = outputDirectory,
            AssemblyName = "Acme.Controls",
            GeneratedCodePath = generatedCodePath
        };

        task.Execute().ShouldBeTrue();

        var className = GetThemeAssetResourceClassName("Themes/ButtonTheme.axaml");
        var generatedAssets = task.GeneratedAssets
                                  .OrderBy(item => item.GetMetadata("Link"), StringComparer.Ordinal)
                                  .ToArray();
        generatedAssets.Length.ShouldBe(2);
        generatedAssets.ShouldContain(item =>
            item.GetMetadata("Link") == $"AtomUI.Generated/{className}.axaml");
        generatedAssets.ShouldContain(item =>
            item.GetMetadata("Link") == $"AtomUI.Generated/{className}_Deferred.axaml");

        var wrapper = XDocument.Load(Path.Combine(outputDirectory, $"{className}.axaml"));
        wrapper.Root.ShouldNotBeNull()
               .Attribute(XName.Get("Class", "http://schemas.microsoft.com/winfx/2006/xaml"))
               .ShouldNotBeNull()
               .Value
               .ShouldBe($"AtomUI.Generated.AcmeControls.{className}");
        wrapper.Descendants()
               .Single(element => element.Name.LocalName == "ResourceInclude")
               .Attribute("Source")
               .ShouldNotBeNull()
               .Value
               .ShouldBe($"avares://Acme.Controls/AtomUI.Generated/{className}_Deferred.axaml");

        var deferred = XDocument.Load(Path.Combine(outputDirectory, $"{className}_Deferred.axaml"));
        var theme = deferred.Root.ShouldNotBeNull()
                            .Elements()
                            .Single(element => element.Name.LocalName == "ButtonTheme");
        theme.Attribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml"))
             .ShouldNotBeNull()
             .Value
             .ShouldBe("{x:Type controls:Button}");
        theme.Attribute("TargetType").ShouldNotBeNull().Value.ShouldBe("controls:Button");

        var generatedCode = File.ReadAllText(generatedCodePath);
        generatedCode.ShouldContain("namespace AtomUI.Generated.AcmeControls;");
        generatedCode.ShouldContain(
            $"internal sealed class {className} : global::Avalonia.Controls.ResourceDictionary");
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private string Write(string relativePath, string content)
    {
        var path = Path.Combine(_directory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    private static string GetThemeAssetResourceClassName(string path)
    {
        var hash = 14695981039346656037UL;
        foreach (var character in path.Replace('\\', '/'))
        {
            hash ^= character;
            hash *= 1099511628211UL;
        }
        return $"GeneratedThemeAssetResource_{hash:X16}";
    }
}
