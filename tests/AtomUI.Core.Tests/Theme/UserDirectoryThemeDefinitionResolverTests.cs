using System.ComponentModel;
using System.Runtime.InteropServices;
using AtomUI.Theme;
using AtomUI.Theme.Definitions;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class UserDirectoryThemeDefinitionResolverTests
{
    [Fact]
    public void Default_Directory_Uses_Application_Data_Application_Id_And_Themes()
    {
        using var directory = new TemporaryDirectory();
        var resolver = new UserDirectoryThemeDefinitionResolver();

        var result = resolver.Resolve(new ThemeDefinitionResolveContext(
            "AtomUIGallery",
            directory.Path,
            false,
            0));

        result.Success.ShouldBeTrue();
        result.Sources.ShouldBeEmpty();
        Directory.Exists(System.IO.Path.Combine(
            directory.Path,
            "AtomUIGallery",
            "Themes")).ShouldBeTrue();
    }

    [Fact]
    public void Resolver_Uses_Only_Top_Level_Theme_Files_In_Ordinal_Path_Order()
    {
        using var directory = new TemporaryDirectory();
        Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, "nested"));
        File.WriteAllText(System.IO.Path.Combine(directory.Path, "b.theme.xml"), ThemeXml("B"));
        File.WriteAllText(System.IO.Path.Combine(directory.Path, "a.theme.xml"), ThemeXml("A"));
        File.WriteAllText(System.IO.Path.Combine(directory.Path, "ignored.xml"), ThemeXml("Ignored"));
        File.WriteAllText(
            System.IO.Path.Combine(directory.Path, "nested", "nested.theme.xml"),
            ThemeXml("Nested"));
        var resolver = new UserDirectoryThemeDefinitionResolver(directory.Path);

        var result = resolver.Resolve(Context());

        result.Success.ShouldBeTrue();
        result.Sources.Select(static source => System.IO.Path.GetFileName(source.SourceIdentity))
              .ShouldBe(["a.theme.xml", "b.theme.xml"]);
    }

    [Fact]
    public void Resolver_Rejects_The_Whole_Directory_When_File_Or_Byte_Limits_Are_Exceeded()
    {
        using var directory = new TemporaryDirectory();
        File.WriteAllText(System.IO.Path.Combine(directory.Path, "a.theme.xml"), ThemeXml("A"));
        File.WriteAllText(System.IO.Path.Combine(directory.Path, "b.theme.xml"), ThemeXml("B"));

        var tooMany = new UserDirectoryThemeDefinitionResolver(
            directory.Path,
            maxFiles: 1,
            maxTotalBytes: 1024 * 1024).Resolve(Context());
        var tooLarge = new UserDirectoryThemeDefinitionResolver(
            directory.Path,
            maxFiles: 10,
            maxTotalBytes: 8).Resolve(Context());

        tooMany.Success.ShouldBeFalse();
        tooMany.Sources.ShouldBeEmpty();
        tooLarge.Success.ShouldBeFalse();
        tooLarge.Sources.ShouldBeEmpty();
    }

    [Fact]
    public void Resolver_Rejects_Symbolic_Link_Files()
    {
        using var directory = new TemporaryDirectory();
        var target = System.IO.Path.Combine(directory.Path, "target.xml");
        var link = System.IO.Path.Combine(directory.Path, "linked.theme.xml");
        File.WriteAllText(target, ThemeXml("Linked"));
        try
        {
            File.CreateSymbolicLink(link, target);
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException or UnauthorizedAccessException)
        {
            return;
        }

        var result = new UserDirectoryThemeDefinitionResolver(directory.Path).Resolve(Context());

        result.Success.ShouldBeFalse();
        result.Sources.ShouldBeEmpty();
        result.Diagnostics.ShouldContain(static diagnostic => diagnostic.Code == "ATMTHM4103");
    }

    [Fact]
    public void User_Theme_Stream_Open_Does_Not_Follow_A_Final_Symbolic_Link()
    {
        using var directory = new TemporaryDirectory();
        var target = System.IO.Path.Combine(directory.Path, "target.theme.xml");
        var link = System.IO.Path.Combine(directory.Path, "linked.theme.xml");
        File.WriteAllText(target, ThemeXml("Linked"));
        try
        {
            File.CreateSymbolicLink(link, target);
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException or UnauthorizedAccessException)
        {
            return;
        }

        Should.Throw<IOException>(() => ThemeDefinitionFileStream.OpenReadNoFollow(link));
    }

    [Fact]
    public void User_Theme_Stream_Open_Rejects_A_Named_Pipe()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            return;
        }

        using var directory = new TemporaryDirectory();
        var pipe = System.IO.Path.Combine(directory.Path, "pipe.theme.xml");
        if (MkFifo(pipe, Convert.ToUInt32("600", 8)) != 0)
        {
            var error = Marshal.GetLastPInvokeError();
            throw new IOException(
                $"Could not create named pipe for theme source test: {new Win32Exception(error).Message}",
                new Win32Exception(error));
        }

        Should.Throw<IOException>(() => ThemeDefinitionFileStream.OpenReadNoFollow(pipe));
    }

    [Fact]
    public void Resolver_Freezes_A_Canonical_Root_When_An_Ancestor_Is_A_Symbolic_Link()
    {
        using var directory = new TemporaryDirectory();
        var realParent = System.IO.Path.Combine(directory.Path, "real");
        var realThemes = System.IO.Path.Combine(realParent, "themes");
        var linkedParent = System.IO.Path.Combine(directory.Path, "linked-parent");
        Directory.CreateDirectory(realThemes);
        File.WriteAllText(System.IO.Path.Combine(realThemes, "theme.theme.xml"), ThemeXml("Theme"));
        try
        {
            Directory.CreateSymbolicLink(linkedParent, realParent);
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException or UnauthorizedAccessException)
        {
            return;
        }

        var result = new UserDirectoryThemeDefinitionResolver(
            System.IO.Path.Combine(linkedParent, "themes")).Resolve(Context());

        result.Success.ShouldBeTrue();
        result.Sources.ShouldHaveSingleItem().SourceIdentity
              .ShouldStartWith(ThemeDefinitionFileStream.GetCanonicalDirectoryPath(realThemes));
        result.Sources[0].SourceIdentity.ShouldNotContain("linked-parent");
    }

    [Fact]
    public void Invalid_User_Theme_Falls_Back_To_Static_Catalog_And_Exposes_Diagnostics()
    {
        HeadlessTestApp.Run(() =>
        {
            using var directory = new TemporaryDirectory();
            File.WriteAllText(System.IO.Path.Combine(directory.Path, "invalid.theme.xml"), "<Theme>");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes.Select(static theme => theme.Id).ShouldBe([
                IThemeManager.DEFAULT_THEME_ID
            ]);
            manager.ThemeCatalogDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Severity == ThemeDiagnosticSeverity.Error);
        });
    }

    [Fact]
    public void Builder_Appends_Valid_User_Themes_After_Static_Themes()
    {
        HeadlessTestApp.Run(() =>
        {
            using var directory = new TemporaryDirectory();
            File.WriteAllText(System.IO.Path.Combine(directory.Path, "green.theme.xml"),
                ThemeXml("PolarGreen"));
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes.Select(static theme => theme.Id).ShouldBe([
                IThemeManager.DEFAULT_THEME_ID,
                "PolarGreen"
            ]);
        });
    }

    private static ThemeDefinitionResolveContext Context()
    {
        return new ThemeDefinitionResolveContext("TestApp", string.Empty, false, 0);
    }

    private static string ThemeXml(string id)
    {
        return $$"""
                 <Theme xmlns="https://atomui.net/schemas/theme/v1"
                        Id="{{id}}"
                        Name="{{id}}"
                        Appearance="Light">
                   <Algorithms><Algorithm Id="Default" /></Algorithms>
                 </Theme>
                 """;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        internal TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"atomui-theme-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }

    [DllImport("libc", EntryPoint = "mkfifo", SetLastError = true)]
    private static extern int MkFifo(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        uint mode);
}
