using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class LanguagePackageTasksTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        $"atomui-language-package-tests-{Guid.NewGuid():N}");

    public LanguagePackageTasksTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Fact]
    public void Prepare_Writes_A_Deferred_Manifest_When_No_Source_Contract_Is_Available()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var manifestPath = Path.Combine(_directory, "AtomUI.LanguagePack.xml");
        var languageItem = DeferredLanguageItem(languagePath);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [languageItem],
            PackageFiles = [languageItem],
            OutputManifestPath = manifestPath
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();
        var warning = engine.Warnings.ShouldHaveSingleItem();
        warning.Code.ShouldBe("ATOMUILOC010");
        warning.Message.ShouldNotBeNull().ShouldContain("PrivateAssets=all");
        File.Exists(manifestPath).ShouldBeTrue();

        var manifest = XDocument.Load(manifestPath);
        var root = manifest.Root.ShouldNotBeNull();
        ((string?)root.Attribute("packageId")).ShouldBe("AtomUI.Desktop.Controls.I18n.JaJP");
        ((string?)root.Attribute("language")).ShouldBe("ja-JP");
        var catalog = root.Elements("catalog").ShouldHaveSingleItem();
        ((string?)catalog.Attribute("moduleId")).ShouldBe("AtomUI.Desktop.Controls");
        ((string?)catalog.Attribute("catalogId"))
            .ShouldBe("AtomUI.Desktop.Controls.DatePickerLang.DatePickerLangResourceKind");
        catalog.Attribute("contractValidation").ShouldBeNull();
        catalog.Attribute("contractVersion").ShouldBeNull();
        ((string?)catalog.Attribute("path")).ShouldBe("Localization/DatePicker/ja-JP.xlf");
        ((string?)catalog.Attribute("sourceFingerprint"))
            .ShouldNotBeNull()
            .ShouldMatch("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Prepare_Enriches_Deferred_Items_With_Validation_And_Fingerprint()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var languageItem = DeferredLanguageItem(languagePath);
        languageItem.SetMetadata(
            "AtomUILanguagePackagePath",
            "Localization\\DatePicker\\ja-JP.xlf");
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [languageItem],
            PackageFiles = [languageItem],
            OutputManifestPath = Path.Combine(_directory, "project-assets.xml")
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();
        engine.Warnings.ShouldHaveSingleItem().Code.ShouldBe("ATOMUILOC010");

        var prepared = task.PreparedLanguageFiles.ShouldHaveSingleItem();
        prepared.GetMetadata("AtomUILanguageSourceKind").ShouldBe("StaticLanguagePack");
        prepared.GetMetadata("AtomUILanguageSourceIdentity")
                .ShouldBe("AtomUI.Desktop.Controls.I18n.JaJP");
        prepared.GetMetadata("AtomUILanguageModuleId").ShouldBe("AtomUI.Desktop.Controls");
        prepared.GetMetadata("AtomUILanguageContractValidation").ShouldBe("Deferred");
        prepared.GetMetadata("AtomUILanguagePackagePath")
                .ShouldBe("Localization/DatePicker/ja-JP.xlf");
        prepared.GetMetadata("AtomUILanguageSourceFingerprint")
                .ShouldMatch("^[0-9a-f]{64}$");
    }

    [Fact]
    public void PrepareProjectReferenceAssets_Enriches_Deferred_Items_Without_Writing_A_Manifest()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var languageItem = DeferredLanguageItem(languagePath);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageAssetsTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            ExpectedLanguage = "ja-JP",
            LanguageFiles = [languageItem]
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();
        engine.Warnings.ShouldHaveSingleItem().Code.ShouldBe("ATOMUILOC010");

        var prepared = task.PreparedLanguageFiles.ShouldHaveSingleItem();
        prepared.GetMetadata("AtomUILanguageContractValidation").ShouldBe("Deferred");
        Directory.EnumerateFiles(_directory, "AtomUI.LanguagePack*.xml").ShouldBeEmpty();
    }

    [Fact]
    public void Prepare_Verifies_A_Target_Against_The_Authoritative_Source()
    {
        var sourcePath = Write("en-US.xlf", ModuleEnglishXliff);
        var targetPath = Write("ja-JP.xlf", JapaneseXliff);
        var manifestPath = Path.Combine(_directory, "verified.xml");
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            SourceLanguageFiles = [SourceLanguageItem(sourcePath)],
            LanguageFiles = [DeferredLanguageItem(targetPath)],
            PackageFiles = [new TestTaskItem(targetPath)],
            OutputManifestPath = manifestPath
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();
        engine.Warnings.ShouldBeEmpty();

        var prepared = task.PreparedLanguageFiles.ShouldHaveSingleItem();
        prepared.GetMetadata("AtomUILanguageContractValidation").ShouldBe("Verified");
        prepared.GetMetadata("AtomUILanguageSourceFingerprint")
                .ShouldMatch("^[0-9a-f]{64}$");

        var catalog = XDocument.Load(manifestPath).Descendants("catalog").ShouldHaveSingleItem();
        catalog.Attribute("contractValidation").ShouldBeNull();
        catalog.Attribute("contractVersion").ShouldBeNull();
        ((string?)catalog.Attribute("sourceFingerprint"))
            .ShouldBe(prepared.GetMetadata("AtomUILanguageSourceFingerprint"));
    }

    [Fact]
    public void Prepare_Rejects_Deferred_Output_When_Verified_Contract_Is_Required()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [DeferredLanguageItem(languagePath)],
            PackageFiles = [new TestTaskItem(languagePath)],
            OutputManifestPath = Path.Combine(_directory, "strict.xml"),
            RequireVerifiedContract = true
        };

        task.Execute().ShouldBeFalse();
        engine.Warnings.ShouldBeEmpty();
        var error = engine.Errors.ShouldHaveSingleItem();
        error.Code.ShouldBe("ATOMUILOC009");
        error.Message.ShouldNotBeNull().ShouldContain("verified language contract");
    }

    [Fact]
    public void Prepare_Rejects_A_Partially_Covered_Authoritative_Module_Contract()
    {
        var datePickerSource = Write("DatePicker.en-US.xlf", ModuleEnglishXliff);
        var dialogSource = Write(
            "Dialog.en-US.xlf",
            ModuleEnglishXliff.Replace(
                "AtomUI.Desktop.Controls.DatePickerLang.DatePickerLangResourceKind",
                "AtomUI.Desktop.Controls.DialogLang.DialogLangResourceKind",
                StringComparison.Ordinal));
        var targetPath = Write("ja-JP.xlf", JapaneseXliff);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            SourceLanguageFiles =
            [
                SourceLanguageItem(datePickerSource),
                SourceLanguageItem(
                    dialogSource,
                    packagePath: "Localization/Dialog/en-US.xlf")
            ],
            LanguageFiles = [DeferredLanguageItem(targetPath)],
            PackageFiles = [new TestTaskItem(targetPath)],
            OutputManifestPath = Path.Combine(_directory, "partial.xml")
        };

        task.Execute().ShouldBeFalse();
        engine.Warnings.ShouldBeEmpty();
        var error = engine.Errors.ShouldHaveSingleItem();
        error.Code.ShouldBe("ATOMUILOC006");
        error.Message.ShouldNotBeNull().ShouldContain("does not contain a target XLIFF");
        error.Message.ShouldNotBeNull().ShouldContain("DialogLangResourceKind");
    }

    [Fact]
    public void Prepare_Rejects_An_Intermediate_Target_State_By_Default()
    {
        var languagePath = Write("ja-JP.xlf", ReviewedJapaneseXliff);
        var languageItem = DeferredLanguageItem(languagePath);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [languageItem],
            PackageFiles = [languageItem],
            OutputManifestPath = Path.Combine(_directory, "invalid-state.xml")
        };

        task.Execute().ShouldBeFalse();
        var error = engine.Errors.ShouldHaveSingleItem();
        error.Code.ShouldBe("ATOMUILOC007");
        error.Message.ShouldNotBeNull().ShouldContain("reviewed");
        error.Message.ShouldNotBeNull().ShouldContain("final");
        File.Exists(task.OutputManifestPath).ShouldBeFalse();
    }

    [Theory]
    [InlineData("RuntimeInitializer.dll", "runtime assembly")]
    [InlineData("Initializer.cs", "runtime source")]
    [InlineData("download.atomlang", ".atomlang")]
    [InlineData("build/Install.targets", "build logic")]
    [InlineData("build/Install.props", "build logic")]
    [InlineData("install.ps1", "script")]
    [InlineData("install.sh", "script")]
    [InlineData("install.cmd", "script")]
    [InlineData("install.bat", "script")]
    public void Prepare_Rejects_Executable_Content_In_A_Static_Language_Package(
        string forbiddenFileName,
        string messageFragment)
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var forbiddenPath = Write(forbiddenFileName, "forbidden content");
        var languageItem = DeferredLanguageItem(languagePath);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [languageItem],
            PackageFiles = [languageItem, new TestTaskItem(forbiddenPath)],
            OutputManifestPath = Path.Combine(_directory, "invalid.xml")
        };

        task.Execute().ShouldBeFalse();
        var error = engine.Errors.ShouldHaveSingleItem();
        error.Code.ShouldBe("ATOMUILOC009");
        error.Message.ShouldNotBeNull().ShouldContain(messageFragment);
        File.Exists(task.OutputManifestPath).ShouldBeFalse();
    }

    [Fact]
    public void Prepare_Rejects_Build_Logic_Placed_By_PackagePath()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var forbiddenPath = Write("Install.targets", "forbidden content");
        var languageItem = DeferredLanguageItem(languagePath);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [languageItem],
            PackageFiles =
            [
                languageItem,
                new TestTaskItem(
                    forbiddenPath,
                    ("Pack", "true"),
                    ("PackagePath", "buildTransitive/Install.targets"))
            ],
            OutputManifestPath = Path.Combine(_directory, "invalid.xml")
        };

        task.Execute().ShouldBeFalse();
        var error = engine.Errors.ShouldHaveSingleItem();
        error.Code.ShouldBe("ATOMUILOC009");
        error.Message.ShouldNotBeNull().ShouldContain("build logic");
        File.Exists(task.OutputManifestPath).ShouldBeFalse();
    }

    [Fact]
    public void Prepare_Validates_The_Final_PackagePath_Instead_Of_The_Source_Extension()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var sourcePath = Write("payload.dll", "documentation payload");
        var languageItem = DeferredLanguageItem(languagePath);
        var engine = new RecordingBuildEngine();
        var task = new PrepareLanguagePackageTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [languageItem],
            PackageFiles =
            [
                languageItem,
                new TestTaskItem(
                    sourcePath,
                    ("Pack", "true"),
                    ("PackagePath", "contentFiles/any/any/payload.txt"))
            ],
            OutputManifestPath = Path.Combine(_directory, "valid.xml")
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();
        File.Exists(task.OutputManifestPath).ShouldBeTrue();
    }

    [Fact]
    public void GenerateProps_Emits_Only_Declarative_Static_Language_Items()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var outputPath = Path.Combine(_directory, "AtomUI.Desktop.Controls.I18n.JaJP.props");
        var engine = new RecordingBuildEngine();
        var task = new GenerateLanguagePackagePropsTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [VerifiedLanguageItem(languagePath)],
            OutputPath = outputPath
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();

        var props = XDocument.Load(outputPath);
        props.Descendants("None")
             .Select(static element => element.Attribute("Remove")!.Value)
             .ShouldBe(
             [
                 "$(MSBuildThisFileDirectory)../contentFiles/any/any/**/*.xlf",
                 "$(MSBuildThisFileDirectory)../contentFiles/any/any/AtomUI.LanguagePack.xml"
             ],
             ignoreOrder: true);
        var item = props.Descendants("AtomUILanguage").ShouldHaveSingleItem();
        ((string?)item.Attribute("Include"))
            .ShouldBe("$(MSBuildThisFileDirectory)../contentFiles/any/any/Localization/DatePicker/ja-JP.xlf");
        ((string?)item.Attribute("AtomUILanguageSourceKind")).ShouldBe("StaticLanguagePack");
        ((string?)item.Attribute("AtomUILanguageSourceIdentity"))
            .ShouldBe("AtomUI.Desktop.Controls.I18n.JaJP");
        ((string?)item.Attribute("AtomUILanguageModuleId")).ShouldBe("AtomUI.Desktop.Controls");
        ((string?)item.Attribute("AtomUILanguageContractValidation")).ShouldBe("Verified");
        ((string?)item.Attribute("AtomUILanguagePackagePath"))
            .ShouldBe("Localization/DatePicker/ja-JP.xlf");
        ((string?)item.Attribute("AtomUILanguageSourceFingerprint"))
            .ShouldNotBeNull()
            .ShouldMatch("^[0-9a-f]{64}$");
        props.Descendants("UsingTask").ShouldBeEmpty();
        props.Descendants("Target").ShouldBeEmpty();
    }

    [Fact]
    public void GenerateProps_Emits_Deferred_Items_With_Their_Validation_Mode()
    {
        var languagePath = Write("ja-JP.xlf", JapaneseXliff);
        var outputPath = Path.Combine(_directory, "Deferred.props");
        var engine = new RecordingBuildEngine();
        var task = new GenerateLanguagePackagePropsTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls.I18n.JaJP",
            LanguageFiles = [DeferredLanguageItem(languagePath, prepared: true)],
            OutputPath = outputPath
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();

        var item = XDocument.Load(outputPath).Descendants("AtomUILanguage").ShouldHaveSingleItem();
        ((string?)item.Attribute("AtomUILanguageContractValidation")).ShouldBe("Deferred");
    }

    [Fact]
    public void GenerateProps_Emits_Module_BuiltIn_Source_Items()
    {
        var languagePath = Write("en-US.xlf", ModuleEnglishXliff);
        var outputPath = Path.Combine(_directory, "AtomUI.Desktop.Controls.Localization.props");
        var engine = new RecordingBuildEngine();
        var task = new GenerateLanguagePackagePropsTask
        {
            BuildEngine = engine,
            PackageId = "AtomUI.Desktop.Controls",
            SourceKind = "ModuleBuiltIn",
            RequireTargetLanguage = false,
            LanguageFiles = [SourceLanguageItem(languagePath)],
            OutputPath = outputPath
        };

        task.Execute().ShouldBeTrue();
        engine.Errors.ShouldBeEmpty();

        var props = XDocument.Load(outputPath);
        props.Descendants("None")
             .Select(static element => element.Attribute("Remove")!.Value)
             .ShouldBe(
             [
                 "$(MSBuildThisFileDirectory)../contentFiles/any/any/**/*.xlf",
                 "$(MSBuildThisFileDirectory)../contentFiles/any/any/AtomUI.LanguagePack.xml"
             ],
             ignoreOrder: true);
        var item = props.Descendants("AtomUILanguage").ShouldHaveSingleItem();
        ((string?)item.Attribute("AtomUILanguageSourceKind")).ShouldBe("ModuleBuiltIn");
    }

    public void Dispose()
    {
        Directory.Delete(_directory, recursive: true);
    }

    private TestTaskItem VerifiedLanguageItem(string path)
    {
        return new TestTaskItem(
            path,
            ("AtomUILanguageModuleId", "AtomUI.Desktop.Controls"),
            ("AtomUILanguageContractValidation", "Verified"),
            ("AtomUILanguagePackagePath", "Localization/DatePicker/ja-JP.xlf"));
    }

    private TestTaskItem DeferredLanguageItem(string path, bool prepared = false)
    {
        var item = new TestTaskItem(
            path,
            ("AtomUILanguageModuleId", "AtomUI.Desktop.Controls"),
            ("AtomUILanguagePackagePath", "Localization/DatePicker/ja-JP.xlf"));
        if (prepared)
        {
            item.SetMetadata("AtomUILanguageContractValidation", "Deferred");
            item.SetMetadata(
                "AtomUILanguageSourceFingerprint",
                AtomUI.Build.Tasks.LocalizationBuild.LanguageSourceFingerprint.Compute(
                    AtomUI.Build.Tasks.LocalizationBuild.Xliff21Parser.Parse(JapaneseXliff).Document!));
        }
        return item;
    }

    private TestTaskItem SourceLanguageItem(
        string path,
        string packagePath = "Localization/DatePicker/en-US.xlf")
    {
        return new TestTaskItem(
            path,
            ("AtomUILanguageSourceKind", "ModuleBuiltIn"),
            ("AtomUILanguageSourceIdentity", "AtomUI.Desktop.Controls"),
            ("AtomUILanguageModuleId", "AtomUI.Desktop.Controls"),
            ("AtomUILanguageContractValidation", "Verified"),
            ("AtomUILanguagePackagePath", packagePath));
    }

    private string Write(string fileName, string content)
    {
        var path = Path.Combine(_directory, fileName);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, content);
        return path;
    }

    private const string JapaneseXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US" trgLang="ja-JP">
          <file id="AtomUI.Desktop.Controls.DatePickerLang.DatePickerLangResourceKind">
            <unit id="Today">
              <segment><source>Today</source><target state="final">今日</target></segment>
            </unit>
          </file>
        </xliff>
        """;

    private static readonly string ReviewedJapaneseXliff = JapaneseXliff.Replace(
        "state=\"final\"",
        "state=\"reviewed\"",
        StringComparison.Ordinal);

    private const string ModuleEnglishXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="AtomUI.Desktop.Controls.DatePickerLang.DatePickerLangResourceKind">
            <unit id="Today">
              <segment><source>Today</source></segment>
            </unit>
          </file>
        </xliff>
        """;
}
