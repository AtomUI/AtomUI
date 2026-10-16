using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AtomUI;
using AtomUI.Controls.Localization;
using AtomUI.Desktop.Controls.Localization;
using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUIGallery.Localization;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Localization;

public class GalleryCatalogCoverageTests
{
    private static readonly MethodInfo LocalizerGetMethod = typeof(ILocalizer)
        .GetMethods()
        .Single(method => method.Name == nameof(ILocalizer.Get));

    static GalleryCatalogCoverageTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Official_Catalogs_Preserve_Their_Enum_Xliff_And_Compiled_Translation_Contracts()
    {
        var assemblies = new[]
        {
            typeof(CommonLangResourceKind).Assembly,
            typeof(DatePickerLangResourceKind).Assembly,
            typeof(ColorPickerLangResourceKind).Assembly,
            typeof(DataGridLangResourceKind).Assembly,
            typeof(GalleryShowCaseHeaderLangResourceKind).Assembly,
            typeof(BaseGalleryApplication).Assembly
        }.Distinct().ToArray();
        var catalogTypes = assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type => type is { IsEnum: true } &&
                                  type.GetCustomAttribute<LanguageCatalogAttribute>() is not null)
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        catalogTypes.Length.ShouldBe(96);
        var memberOrderBaseline = LoadCatalogMemberOrderBaseline();
        memberOrderBaseline.Keys.ShouldBe(
            catalogTypes.Select(static type => type.FullName!),
            ignoreOrder: true);
        foreach (var catalogType in catalogTypes)
        {
            catalogType.GetCustomAttribute<LanguageCatalogAttribute>()
                       .ShouldNotBeNull();
            Enum.GetNames(catalogType).ShouldBe(memberOrderBaseline[catalogType.FullName!]);

            var extensionName = catalogType.Name[..^"Kind".Length] + "Extension";
            var extensionType = catalogType.Assembly
                                           .GetType($"{catalogType.Namespace}.{extensionName}")
                                           .ShouldNotBeNull();
            extensionType.IsSealed.ShouldBeTrue();
            extensionType.BaseType.ShouldBe(
                typeof(LanguageResourceExtension<>).MakeGenericType(catalogType));
        }

        assemblies.SelectMany(static assembly => assembly.GetTypes())
                  .ShouldNotContain(static type =>
                      type.Name == "en_US" || type.Name == "zh_CN" || type.Name == "zh_TW");

        var files = LoadLanguageFiles(catalogTypes);
        files.Length.ShouldBe(370);
        files.GroupBy(static file => file.CatalogType)
             .ShouldAllBe(static group => group.Count() == 3 || group.Count() == 4);
        AssertXliffContracts(catalogTypes, files);

        var application = Application.Current.ShouldNotBeNull();
        var manager = application.GetLanguageManager().ShouldNotBeNull();
        var localizer = application.GetLocalizer().ShouldNotBeNull();
        try
        {
            foreach (var language in new[]
                     {
                         LanguageTags.EnUS,
                         LanguageTags.ZhCN,
                         LanguageTags.ZhTW,
                         LanguageTags.PtBR
                     })
            {
                manager.ChangeLanguage(language);
                foreach (var file in files.Where(file => file.Language == language))
                {
                    var get = LocalizerGetMethod.MakeGenericMethod(file.CatalogType);
                    foreach (var entry in file.Entries)
                    {
                        var kind = Enum.Parse(file.CatalogType, entry.Key);
                        get.Invoke(localizer, [kind]).ShouldBe(entry.Text);
                    }
                }
            }
        }
        finally
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    [Fact]
    public void Gallery_PtBr_Templates_Match_All_Application_Catalog_Sources()
    {
        var root = GetRepoRoot();
        var sourceFiles = Directory.GetFiles(
                                  Path.Combine(root, "controlgallery", "AtomUIGallery"),
                                  "en-US.xlf",
                                  SearchOption.AllDirectories)
                              .Concat(Directory.GetFiles(
                                  Path.Combine(root, "src", "AtomUI.Toolkits.GalleryBase"),
                                  "en-US.xlf",
                                  SearchOption.AllDirectories))
                              .OrderBy(static path => path, StringComparer.Ordinal)
                              .ToArray();

        sourceFiles.Length.ShouldBe(82);
        // 4748（feature/semantic 基线）+ 14（ComboBox Semantic 示例）+ 18（Splash Semantic 示例）
        // + 8（release/6.0 TabControl/TabStrip 溢出搜索示例）
        // + 3（Pagination 自定义页大小组件示例）+ 1（DataGrid 分页 Extra Content 示例）
        // + 38（DateViewer 独立页面）+ 1（DateViewer 导航）+ 1（DateViewer footer Part）+ 2（ColorPicker 确认示例）+ 2（TimePicker 确认示例）= 4836
        // - 51（启动重构：Splash 迁入 AtomUI.Controls，示例页改为页内预览，65 → 14）= 4785。
        sourceFiles.Sum(CountUnits).ShouldBe(4785);
        foreach (var sourcePath in sourceFiles)
        {
            var targetPath = Path.Combine(
                Path.GetDirectoryName(sourcePath)!,
                "pt-BR.xlf");
            File.Exists(targetPath).ShouldBeTrue();

            XNamespace xliff = "urn:oasis:names:tc:xliff:document:2.0";
            var source = XDocument.Load(sourcePath).Root.ShouldNotBeNull();
            var target = XDocument.Load(targetPath).Root.ShouldNotBeNull();
            ((string?)source.Attribute("srcLang")).ShouldBe("en-US");
            source.Attribute("trgLang").ShouldBeNull();
            ((string?)target.Attribute("srcLang")).ShouldBe("en-US");
            ((string?)target.Attribute("trgLang")).ShouldBe("pt-BR");
            ((string?)target.Element(xliff + "file")?.Attribute("id"))
                .ShouldBe((string?)source.Element(xliff + "file")?.Attribute("id"));

            var targetUnits = target.Descendants(xliff + "unit").ToArray();
            var sourceUnits = source.Descendants(xliff + "unit").ToArray();
            var sourceUnitsByKey = sourceUnits.ToDictionary(
                static unit => unit.Attribute("id").ShouldNotBeNull().Value,
                StringComparer.Ordinal);
            targetUnits.Select(static unit => (string?)unit.Attribute("id"))
                       .OrderBy(static id => id, StringComparer.Ordinal)
                       .ShouldBe(sourceUnits.Select(static unit => (string?)unit.Attribute("id"))
                                           .OrderBy(static id => id, StringComparer.Ordinal));
            foreach (var unit in targetUnits)
            {
                var key = unit.Attribute("id").ShouldNotBeNull().Value;
                var sourceSegment = sourceUnitsByKey[key]
                    .Element(xliff + "segment")
                    .ShouldNotBeNull();
                var segment = unit.Element(xliff + "segment").ShouldNotBeNull();
                var sourceText = sourceSegment.Element(xliff + "source").ShouldNotBeNull().Value;
                segment.Element(xliff + "source").ShouldNotBeNull().Value.ShouldBe(sourceText);
                var targetElement = segment.Element(xliff + "target").ShouldNotBeNull();
                targetElement.Value.ShouldNotBeEmpty();
                ((string?)targetElement.Attribute("state")).ShouldBe("translated");
                ExtractPlaceholders(targetElement.Value).ShouldBe(ExtractPlaceholders(sourceText));
            }
        }
    }

    [Fact]
    public void Gallery_PtBr_Snapshot_Combines_Application_And_Official_Translations()
    {
        var application = Application.Current.ShouldNotBeNull();
        application.ShouldBeAssignableTo<IGeneratedApplicationLanguageBootstrap>();
        var manager = application.GetLanguageManager().ShouldNotBeNull();
        var localizer = application.GetLocalizer().ShouldNotBeNull();
        try
        {
            manager.ChangeLanguage(LanguageTags.PtBR);

            manager.Current.CurrentLanguage.ShouldBe(LanguageTags.PtBR);
            manager.Current.FormattingCulture.Name.ShouldBe("pt-BR");
            manager.Current.TextDirection.ShouldBe(LanguageTextDirection.LeftToRight);
            localizer.Get(CommonLangResourceKind.Cancel).ShouldBe("Cancelar");
            localizer.Get(GalleryShowCaseHeaderLangResourceKind.PackageLabel).ShouldBe("Pacote");
            localizer.Get(WorkspaceWindowLangResourceKind.MenuItemSettings).ShouldBe("Configurações");
        }
        finally
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    private static int CountUnits(string path)
    {
        XNamespace xliff = "urn:oasis:names:tc:xliff:document:2.0";
        return XDocument.Load(path).Descendants(xliff + "unit").Count();
    }

    private static string[] ExtractPlaceholders(string text)
    {
        return Regex.Matches(text, @"\{\d+\}")
                    .Select(static match => match.Value)
                    .OrderBy(static placeholder => placeholder, StringComparer.Ordinal)
                    .ToArray();
    }

    private static CatalogLanguageFile[] LoadLanguageFiles(IReadOnlyCollection<Type> catalogTypes)
    {
        var typesByName = catalogTypes.ToDictionary(
            static type => type.FullName!,
            StringComparer.Ordinal);
        var root = GetRepoRoot();
        XNamespace xliff = "urn:oasis:names:tc:xliff:document:2.0";

        return new[] { "src", "controlgallery" }
            .SelectMany(directory => Directory.GetFiles(
                Path.Combine(root, directory),
                "*.xlf",
                SearchOption.AllDirectories))
            .Where(path => !path.Contains(
                Path.Combine("src", "LanguagePacks") + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .Select(path =>
            {
                var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
                var rootElement = document.Root.ShouldNotBeNull();
                var file = rootElement.Element(xliff + "file").ShouldNotBeNull();
                var catalogName = file.Attribute("id")!.Value;
                var catalogType = typesByName[catalogName];
                var targetLanguage = rootElement.Attribute("trgLang")?.Value;
                var language = LanguageTag.Parse(
                    targetLanguage ?? rootElement.Attribute("srcLang")!.Value);
                var entries = file.Elements(xliff + "unit")
                    .Select(unit =>
                    {
                        var segment = unit.Element(xliff + "segment").ShouldNotBeNull();
                        var source = segment.Element(xliff + "source").ShouldNotBeNull().Value;
                        var target = segment.Element(xliff + "target");
                        var text = targetLanguage is null ? source : target.ShouldNotBeNull().Value;
                        unit.Attribute("name").ShouldBeNull();
                        return new CatalogEntry(
                            unit.Attribute("id").ShouldNotBeNull().Value,
                            source,
                            text,
                            target?.Attribute("state")?.Value);
                    })
                    .ToArray();
                return new CatalogLanguageFile(catalogName, catalogType, language, entries);
            })
            .ToArray();
    }

    private static void AssertXliffContracts(
        IReadOnlyCollection<Type> catalogTypes,
        IReadOnlyCollection<CatalogLanguageFile> files)
    {
        foreach (var catalogType in catalogTypes)
        {
            var catalogFiles = files.Where(file => file.CatalogType == catalogType).ToArray();
            var expectedLanguages = new[]
            {
                LanguageTags.EnUS,
                LanguageTags.ZhCN,
                LanguageTags.ZhTW
            };
            if (catalogFiles.Any(static file => file.Language == LanguageTags.PtBR))
            {
                expectedLanguages = [.. expectedLanguages, LanguageTags.PtBR];
            }

            catalogFiles.Select(static file => file.Language)
                        .ShouldBe(expectedLanguages, ignoreOrder: true);
            catalogFiles.ShouldAllBe(file => file.CatalogId == catalogType.FullName);

            var sourceEntries = catalogFiles.Single(file => file.Language == LanguageTags.EnUS).Entries;
            var enumKeys = Enum.GetNames(catalogType)
                               .OrderBy(static key => key, StringComparer.Ordinal)
                               .ToArray();
            sourceEntries.Select(static entry => entry.Key)
                         .OrderBy(static key => key, StringComparer.Ordinal)
                         .ShouldBe(enumKeys);

            foreach (var targetFile in catalogFiles.Where(file => file.Language != LanguageTags.EnUS))
            {
                targetFile.Entries.Select(static entry => (entry.Key, entry.Source))
                          .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
                          .ShouldBe(sourceEntries.Select(static entry => (entry.Key, entry.Source))
                                                 .OrderBy(static entry => entry.Key, StringComparer.Ordinal));
                targetFile.Entries.ShouldAllBe(static entry =>
                    entry.TargetState == "translated" && !string.IsNullOrEmpty(entry.Text));
            }
        }
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("AtomUI repository root was not found.");
    }

    private static IReadOnlyDictionary<string, string[]> LoadCatalogMemberOrderBaseline()
    {
        var path = Path.Combine(
            GetRepoRoot(),
            "tests",
            "AtomUIGallery.Tests",
            "Localization",
            "CatalogMemberOrder.baseline");
        return File.ReadLines(path)
                   .Where(static line => !string.IsNullOrWhiteSpace(line))
                   .Select(static line => line.Split('|', 2))
                   .ToDictionary(
                       static parts => parts[0],
                       static parts => parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries),
                       StringComparer.Ordinal);
    }

    private sealed record CatalogLanguageFile(
        string CatalogId,
        Type CatalogType,
        LanguageTag Language,
        CatalogEntry[] Entries);

    private sealed record CatalogEntry(
        string Key,
        string Source,
        string Text,
        string? TargetState);
}
