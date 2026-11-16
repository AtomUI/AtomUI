using System.Xml.Linq;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Schema;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using AtomUIGallery.ShowCases.ThemePresets;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using Button = AtomUI.Desktop.Controls.Button;
using Window = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests;

public class ThemePresetsShowCaseTests
{
    [Fact]
    public void Primary_Choice_Follows_Declared_Brand_Support()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        model.SelectedPresetId = "PrimeNGAura";
        model.CanChoosePrimary.ShouldBeTrue();
        foreach (var id in new[] { "Bootstrap", "Material" })
        {
            model.SelectedPresetId = id;
            model.CanChoosePrimary.ShouldBeFalse(id);
        }
    }

    [Fact]
    public void Gallery_Brand_Derived_Token_Set_Matches_The_Core_Rule()
    {
        ThemePresetCatalog.s_brandDerivedTokens.ShouldBe(
            AtomUI.Theme.Definitions.ThemeDocumentReader.s_brandDerivedTokens, ignoreOrder: true);
    }

    [Fact]
    public void Theme_Without_Design_Language_Or_Brand_Declaration_Cannot_Choose_Primary()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "GalleryPreviewFixture" };
        model.CanChoosePrimary.ShouldBeFalse();
    }

    [Fact]
    public void Selected_Swatch_Survives_Mode_Toggles_On_A_Seed_Family()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        var rose = Color.Parse("#F43F5E");
        model.SelectedPrimary = rose;
        foreach (var (dark, compact, suffix) in new[]
                 { (true, false, ".Dark"), (true, true, ".DarkCompact"), (false, true, ".Compact"), (false, false, "") })
        {
            model.IsDark = dark;
            model.IsCompact = compact;
            model.PreviewThemeId.ShouldBe("PrimeNGAura" + suffix);
            model.SelectedPrimary.ShouldBe(rose);
            Color.Parse(model.PreviewConfig!.Tokens["ColorPrimary"]).ShouldBe(rose);
        }
    }

    [Fact]
    public void Export_Writes_Design_Language_Metadata()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        model.IsDark = true;
        model.IsCompact = true;
        model.PreviewThemeId.ShouldBe("PrimeNGAura.DarkCompact");
        var xml = model.ExportThemeXml();
        var root = XDocument.Parse(xml).Root!;
        root.Attribute("DesignLanguage")!.Value.ShouldBe("PrimeNG.Aura");
        root.Attribute("BrandColor")!.Value.ShouldBe("Seed");
        root.Attribute("Density")!.Value.ShouldBe("Compact");
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var result = AtomUI.Theme.Definitions.ThemeDocumentReader.Read(stream, "export.theme.xml");
        result.Diagnostics.ShouldBeEmpty();
        result.Document.ShouldNotBeNull();
    }

    [Fact]
    public void Export_Of_A_Seed_Theme_With_Pinned_Brand_Tokens_Falls_Back_To_Fixed()
    {
        var config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default)
            .WithToken("ColorPrimary", "#1677FF").WithToken("ColorPrimaryHover", "#4096FF").Build();
        var xml = ThemePresetCatalog.Export("Pinned", "Pinned", config, designLanguage: "Pinned",
            brandColor: ThemeBrandColorSupport.Seed, density: ThemeDensity.Normal);
        XDocument.Parse(xml).Root!.Attribute("BrandColor")!.Value.ShouldBe("Fixed");
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        AtomUI.Theme.Definitions.ThemeDocumentReader.Read(stream, "pinned.theme.xml").Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Package_Profiles_Are_Grouped_By_Family_And_Compiled_Package_Origin()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        model.Presets.Where(item => item.ThemeId.StartsWith("PrimeNG", StringComparison.Ordinal)).Select(item => item.ThemeId).ShouldBe(["PrimeNGAura", "PrimeNGLara", "PrimeNGNora"]);
        model.Presets.Where(item => item.ThemeId.StartsWith("PrimeNG", StringComparison.Ordinal)).ShouldAllBe(item => item.Icon != null);
        model.Presets.Single(item => item.ThemeId == "GalleryPreviewFixture").Icon.ShouldBeNull();
        var families = new[]
        {
            "ShadcnVega", "ShadcnNova", "ShadcnMaia", "ShadcnLyra",
            "ShadcnMira", "ShadcnLuma", "ShadcnSera", "ShadcnRhea"
        };
        model.Presets.Where(item => item.ThemeId.StartsWith("Shadcn", StringComparison.Ordinal))
            .Select(item => item.ThemeId).ShouldBe(families, ignoreOrder: true);
        model.Presets.Where(item => item.ThemeId.StartsWith("Bootstrap", StringComparison.Ordinal))
            .Select(item => item.ThemeId).ShouldBe(new[] { "Bootstrap" });
        var catalog = ((ThemeManager)Application.Current!.GetThemeManager()!).CompiledThemeCatalog;
        var bootstrap = catalog.ResolverSlices.Single(slice => slice.ResolverId == "AtomUI.Themes.Bootstrap");
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            bootstrap.Definitions.ShouldContain(entry => entry.Info.Id == "Bootstrap" + suffix && entry.Revision.ProgramDigest != null);
            model.SelectedPresetId = "Bootstrap";
            model.IsDark = suffix.Contains("Dark");
            model.IsCompact = suffix.Contains("Compact");
            model.PreviewThemeId.ShouldBe("Bootstrap" + suffix);
        }
        model.Presets.Where(item => item.ThemeId.StartsWith("Material", StringComparison.Ordinal))
            .Select(item => item.ThemeId).ShouldBe(new[] { "Material" });
        var material = catalog.ResolverSlices.Single(slice => slice.ResolverId == "AtomUI.Themes.Material");
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            material.Definitions.ShouldContain(entry => entry.Info.Id == "Material" + suffix && entry.Revision.ProgramDigest != null);
            model.SelectedPresetId = "Material";
            model.IsDark = suffix.Contains("Dark");
            model.IsCompact = suffix.Contains("Compact");
            model.PreviewThemeId.ShouldBe("Material" + suffix);
        }
        var package = catalog.ResolverSlices.Single(slice => slice.ResolverId == "AtomUI.Themes.Shadcn");
        foreach (var id in families)
        {
            foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
            {
                package.Definitions.ShouldContain(entry => entry.Info.Id == id + suffix && entry.Revision.ProgramDigest != null);
                model.SelectedPresetId = id;
                model.IsDark = suffix.Contains("Dark");
                model.IsCompact = suffix.Contains("Compact");
                model.PreviewThemeId.ShouldBe(id + suffix);
            }
        }
    }

    [Fact]
    public void Seed_Only_Package_Uses_Orthogonal_Algorithms_When_Mode_Profiles_Are_Absent()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        model.SelectedPresetId = "GalleryPreviewFixture";
        model.IsDark = true;
        model.IsCompact = true;
        model.SelectedPresetId.ShouldBe("GalleryPreviewFixture");
        model.PreviewThemeId.ShouldBe("GalleryPreviewFixture");
        // 与 ThemeSelectionResolver 的规范一致：定义自身链去掉内建 Dark/Compact，再按 Compact、Dark 追加。
        model.PreviewConfig!.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark]);
        model.PreviewConfig.Inherit.ShouldBeFalse();

        // Plain light, non-compact mode resolves to the base definition itself, which keeps its own chain.
        model.IsDark = false;
        model.IsCompact = false;
        model.PreviewThemeId.ShouldBe("GalleryPreviewFixture");
        model.PreviewConfig!.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
        model.PreviewConfig.Inherit.ShouldBeFalse();
    }

    [Fact]
    public void Local_Preview_Uses_Exact_Profile_Values_And_Never_Publishes_The_Root()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager = Application.Current!.GetThemeManager()!;
        var original = manager.CurrentTheme!;
        // A different selected definition plus conspicuous runtime alias/Own overrides must
        // never contaminate the independent preview; restore only after recording zero publishes.
        var polluted = new ThemeConfigBuilder().WithToken("ColorInfoBg", "#123456")
            .WithControl(ButtonTokens.Identity, new ControlThemeConfigBuilder().WithToken("FontWeight", "900").Build()).Build();
        Apply(manager, new ThemeRequest("GalleryPreviewFixture", polluted, ThemeTransitionReason.UserRequest));
        var root = manager.CurrentTheme!;
        var publications = 0;
        void Changed(object? sender, ThemeChangedEventArgs args) { publications++; }
        manager.ThemeChanged += Changed;
        using var model = new ThemePresetsViewModel(null!);
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            var button = page.FindControl<Button>("PreviewPrimary")!;
            var provider = page.FindControl<ThemeConfigProvider>("PreviewProvider")!;
            var initial = button;
            AssertTokens(button, "#020617", "#FFFFFF", 35, 500);
            AssertIndependentProfile(button, (ThemeManager)manager, "PrimeNGAura");
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorInfoBg).ShouldNotBe(Color.Parse("#123456"));
            foreach (var (dark, compact, suffix, color, background, height) in new[]
            {
                // The PrimeNG default primary is noir: slate-950 in light, zinc-50 in dark.
                (true, false, ".Dark", "#FAFAFA", "#09090B", 35),
                (true, true, ".DarkCompact", "#FAFAFA", "#09090B", 31),
                (false, true, ".Compact", "#020617", "#FFFFFF", 31),
                (false, false, "", "#020617", "#FFFFFF", 35)
            })
            {
                model.IsDark = dark; model.IsCompact = compact; Pump();
                model.SelectedPresetId.ShouldBe("PrimeNGAura");
                model.PreviewThemeId.ShouldBe("PrimeNGAura" + suffix);
                // A mode profile keeps its own chain (the PrimeNG Aura algorithms), not the built-in toggles.
                model.PreviewConfig!.Algorithms![0].ShouldBe(new ThemeAlgorithm("PrimeNG", dark ? "Aura.Dark" : "Aura"));
                model.PreviewConfig.Algorithms.Contains(ThemeAlgorithm.Compact).ShouldBe(compact);
                AssertTokens(button, color, background, height, 500);
                AssertIndependentProfile(button, (ThemeManager)manager, model.PreviewThemeId!);
                ControlTokenAccessor.Capture(button).GetGlobal<double>(SharedTokenKind.ControlHeightSM).ShouldBe(compact ? 23.25 : 28);
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
                provider.Config.ShouldBeSameAs(model.PreviewConfig);
            }
            model.SelectedPresetId = "PrimeNGNora"; Pump();
            ControlTokenAccessor.Capture(button).GetOwn<FontWeight, ButtonTokenKind>(ButtonTokenKind.FontWeight).ShouldBe((FontWeight)700);
            foreach (var (id, primary, height) in new[]
            {
                ("ShadcnNova", "#171717", 32d), ("ShadcnMaia", "#171717", 36d), ("ShadcnSera", "#171717", 40d)
            })
            {
                model.SelectedPresetId = id;
                model.IsDark = false; model.IsCompact = false; Pump();
                var tokens = ControlTokenAccessor.Capture(button);
                tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse(primary));
                tokens.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe(height);
                tokens.GetGlobal<bool>(SharedTokenKind.EnableWaveSpirit).ShouldBeFalse();
                AssertIndependentProfile(button, (ThemeManager)manager, id);
                model.IsDark = true; model.IsCompact = true; Pump();
                tokens = ControlTokenAccessor.Capture(button);
                // Neutral dark: the background is neutral-950 and the neutral theme's primary is neutral-200.
                tokens.GetGlobal<Color>(SharedTokenKind.ColorBgContainer).ShouldBe(Color.Parse("#0A0A0A"));
                tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#E5E5E5"));
                tokens.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe(height - 4);
                AssertIndependentProfile(button, (ThemeManager)manager, id + ".DarkCompact");
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
            }
            foreach (var family in new[] { "Bootstrap", "Material" })
            {
                model.SelectedPresetId = family;
                foreach (var (dark, compact, suffix) in new[]
                {
                    (false, false, ""), (true, false, ".Dark"),
                    (false, true, ".Compact"), (true, true, ".DarkCompact")
                })
                {
                    model.IsDark = dark; model.IsCompact = compact; Pump();
                    var tokens = ControlTokenAccessor.Capture(button);
                    tokens.GetGlobal<Color>(SharedTokenKind.ColorBgContainer).ShouldBe(Color.Parse(family == "Material" ? (dark ? "#1C1B1F" : "#FFFBFE") : (dark ? "#080A0C" : "#FFFFFF")));
                    tokens.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe((family == "Material" ? 40 : 38) - (compact ? 4 : 0));
                    if (family == "Bootstrap")
                    {
                        tokens.GetGlobal<Color>(SharedTokenKind.ColorControlOutline).ShouldBe(Color.Parse(dark ? "#0064BA" : "#6FC8FF"));
                    }
                    else
                    {
                        tokens.GetGlobal<Color>(SharedTokenKind.ColorTextLightSolid).ShouldBe(Color.Parse(dark ? "#381E72" : "#FFFFFF"));
                        if (!dark)
                        {
                            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#6750A4"));
                        }
                        tokens.GetGlobal<BoxShadows>(SharedTokenKind.BoxShadows).ShouldBe(BoxShadows.Parse("0 4 8 3 #26000000"));
                    }
                    AssertIndependentProfile(button, (ThemeManager)manager, family + suffix);
                    page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
                    var check = provider.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.CheckBox>().Single();
                    var radio = provider.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.RadioButton>().Single();
                    var mark = check.GetVisualDescendants().OfType<AtomUI.Controls.Icon>().Single(icon => icon.Name == "CheckedMark");
                    var dot = radio.GetVisualDescendants().OfType<TemplatedControl>().Single(control => control.Name == "Indicator");
                    var dotBrush = AvaloniaPropertyRegistry.Instance.GetRegistered(dot.GetType()).Single(property => property.Name == "RadioInnerBackground");
                    var onPrimary = family == "Material" && dark ? Color.Parse("#381E72") : Colors.White;
                    ((ISolidColorBrush)mark.StrokeBrush!).Color.ShouldBe(onPrimary);
                    ((ISolidColorBrush)dot.GetValue(dotBrush)!).Color.ShouldBe(onPrimary);
                    check.Foreground = Brushes.Magenta;
                    radio.Foreground = Brushes.Magenta;
                    check.IsEnabled = false; radio.IsEnabled = false; Pump();
                    check.Foreground.ShouldBeSameAs(Brushes.Magenta);
                    radio.Foreground.ShouldBeSameAs(Brushes.Magenta);
                    var disabled = ControlTokenAccessor.Capture(check).GetGlobal<Color>(SharedTokenKind.ColorTextDisabled);
                    ((ISolidColorBrush)mark.StrokeBrush!).Color.ShouldBe(disabled);
                    ((ISolidColorBrush)dot.GetValue(dotBrush)!).Color.ShouldBe(disabled);
                    check.ClearValue(TemplatedControl.ForegroundProperty);
                    radio.ClearValue(TemplatedControl.ForegroundProperty);
                    check.IsEnabled = true; radio.IsEnabled = true; Pump();
                }
            }
            publications.ShouldBe(0);
            manager.CurrentTheme.ShouldBeSameAs(root);
        }
        finally
        {
            manager.ThemeChanged -= Changed;
            window.Close(); Pump();
            Apply(manager, new ThemeRequest(original.ThemeId, new ThemeConfigBuilder().WithAlgorithms(original.Algorithms.ToArray()).Build(), ThemeTransitionReason.UserRequest));
        }
    }

    [Fact]
    public void Copy_Command_Exports_Active_Profile_And_Own_Overrides_To_Real_Clipboard()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            model.IsDark = true; model.IsCompact = true; Pump();
            var copy = page.FindControl<Button>("CopyThemeButton")!;
            copy.Command!.Execute(copy.CommandParameter);
            for (var i = 0; i < 40 && !model.CopySucceeded; i++) { Pump(); Thread.Sleep(10); }
            model.CopySucceeded.ShouldBeTrue();
            var xml = ReadClipboard(window);
            using (var schema = typeof(IThemeManager).Assembly.GetManifestResourceStream("AtomUI.Theme.Definitions.Schemas.atomui-theme-v1.xsd"))
            {
                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
                using var schemaReader = XmlReader.Create(schema!);
                settings.Schemas.Add("https://atomui.net/schemas/theme/v1", schemaReader);
                using var reader = XmlReader.Create(new StringReader(xml), settings);
                while (reader.Read()) { }
            }
            var root = XDocument.Parse(xml).Root!;
            XNamespace ns = "https://atomui.net/schemas/theme/v1";
            root.Name.ShouldBe(ns + "Theme");
            root.Attribute("Id")!.Value.ShouldBe("PrimeNGAura.DarkCompact");
            root.Attribute("Appearance")!.Value.ShouldBe("Dark");
            // The Aura profiles carry the algorithm chain and seeds; colours and control values come from the
            // PrimeNG algorithms, so the export has no Controls section.
            root.Element(ns + "Algorithms")!.Elements()
                .Select(element => ((string?)element.Attribute("Catalog"), element.Attribute("Id")!.Value))
                .ShouldBe([("PrimeNG", "Aura.Dark"), ((string?)null, "Compact")]);
            Color.Parse(root.Element(ns + "Tokens")!.Elements().Single(token => token.Attribute("Name")!.Value == "ColorPrimary").Attribute("Value")!.Value).ShouldBe(Color.Parse("#020617"));
            root.Element(ns + "Controls").ShouldBeNull();
            model.IsDark = false; Pump(); model.CopySucceeded.ShouldBeFalse();
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Export_Writes_The_Algorithm_Catalog_Only_Outside_AtomUI()
    {
        var external = new ThemeAlgorithm("PrimeNG", "Aura.Dark");
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(ThemeAlgorithm.Default, external)
                     .WithToken("ColorPrimary", "#1677FF")
                     .WithControl(new ControlTokenIdentity("AtomUI", "Button"),
                         new ControlThemeConfigBuilder().WithAlgorithm(ControlAlgorithmMode.Custom)
                                                        .WithAlgorithms(external, ThemeAlgorithm.Compact)
                                                        .WithToken("ContentFontSize", "14").Build())
                     .Build();

        var xml = ThemePresetCatalog.Export("Exported", "Exported", config);

        using (var schema = typeof(IThemeManager).Assembly.GetManifestResourceStream("AtomUI.Theme.Definitions.Schemas.atomui-theme-v1.xsd"))
        {
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            using var schemaReader = XmlReader.Create(schema!);
            settings.Schemas.Add("https://atomui.net/schemas/theme/v1", schemaReader);
            using var reader = XmlReader.Create(new StringReader(xml), settings);
            while (reader.Read()) { }
        }
        XNamespace ns = "https://atomui.net/schemas/theme/v1";
        var algorithms = XDocument.Parse(xml).Descendants(ns + "Algorithm")
                                  .Select(element => ((string?)element.Attribute("Catalog"), element.Attribute("Id")!.Value))
                                  .ToArray();
        algorithms.ShouldBe([
            ((string?)null, "Default"), ("PrimeNG", "Aura.Dark"),
            ("PrimeNG", "Aura.Dark"), ((string?)null, "Compact")
        ]);
    }

    [Fact]
    public void Copy_Command_Contains_Asynchronous_Write_Failure_And_Allows_A_Successful_Retry()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        var errors = new List<Exception>();
        using var exceptions = model.CopyThemeXml.ThrownExceptions.Subscribe(errors.Add);
        try
        {
            window.Show(); Pump();
            var button = page.FindControl<Button>("CopyThemeButton")!;
            var failure = page.FindControl<Avalonia.Controls.TextBlock>("CopyFailureFeedback")!;
            var writeClipboard = model.CopyText!;
            button.Command!.Execute(button.CommandParameter);
            WaitUntil(() => model.CopySucceeded);

            var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            model.CopyText = _ => pending.Task;
            button.Command.Execute(button.CommandParameter);
            Pump();
            model.CopySucceeded.ShouldBeFalse();
            model.CopyFailed.ShouldBeFalse();
            pending.SetException(new IOException("clipboard write failed"));
            WaitUntil(() => model.CopyFailed && button.Command.CanExecute(button.CommandParameter));
            model.CopySucceeded.ShouldBeFalse();
            failure.IsVisible.ShouldBeTrue();
            string.IsNullOrWhiteSpace(failure.Text).ShouldBeFalse();
            errors.ShouldBeEmpty("The command must contain the write fault before ReactiveUI's exception pipeline.");

            model.CopyText = writeClipboard;
            button.Command.Execute(button.CommandParameter);
            WaitUntil(() => model.CopySucceeded);
            model.CopyFailed.ShouldBeFalse();
            failure.IsVisible.ShouldBeFalse();
            ReadClipboard(window).ShouldBe(model.ExportThemeXml());

            // Feedback belongs to the copied configuration, including completion after a mode change.
            var previousProfileCopy = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            model.CopyText = _ => previousProfileCopy.Task;
            button.Command.Execute(button.CommandParameter);
            model.IsDark = true;
            previousProfileCopy.SetResult(true);
            WaitUntil(() => button.Command.CanExecute(button.CommandParameter));
            model.CopySucceeded.ShouldBeFalse();
            model.CopyFailed.ShouldBeFalse();
            errors.ShouldBeEmpty();
        }
        finally { window.Close(); Pump(); }
    }

    // The page tests pump the headless dispatcher on the test thread; an await would resume the continuation away
    // from the UI thread, so these swatch tests stay synchronous like the rest of this class.
    [Fact]
    public void Selecting_A_Swatch_On_A_PrimeNG_Preset_Moves_Hover_Active_And_Focus_Ring()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            // A PrimeNG preset leads with noir, its own seed (slate-950), so it is not listed twice.
            model.PrimarySwatches.ShouldBe([
                Color.Parse("#020617"), Color.Parse("#10B981"), Color.Parse("#3B82F6"), Color.Parse("#8B5CF6"),
                Color.Parse("#F43F5E"), Color.Parse("#F59E0B")
            ]);
            var button = page.FindControl<Button>("PreviewPrimary")!;
            var seeded = model.PreviewConfig;
            model.SelectedPrimary = Color.Parse("#3B82F6"); Pump();

            model.PreviewConfig.ShouldNotBeSameAs(seeded);
            Color.Parse(model.PreviewConfig!.Tokens["ColorPrimary"]).ShouldBe(Color.Parse("#3B82F6"));
            model.PreviewConfig.Algorithms![0].ShouldBe(new ThemeAlgorithm("PrimeNG", "Aura"));
            var tokens = ControlTokenAccessor.Capture(button);
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#3B82F6"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimaryHover).ShouldBe(Color.Parse("#2563EB"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimaryActive).ShouldBe(Color.Parse("#1D4ED8"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorFocusBorder)
                  .ShouldBe(tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary));

            // The selection follows the dark profile: Aura dark maps the seed to blue-400 with a blue-300 hover.
            model.IsDark = true; Pump();
            model.PreviewThemeId.ShouldBe("PrimeNGAura.Dark");
            tokens = ControlTokenAccessor.Capture(button);
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#60A5FA"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimaryHover).ShouldBe(Color.Parse("#93C5FD"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorFocusBorder)
                  .ShouldBe(tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary));
            page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(button);
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Clearing_The_Swatch_Restores_The_Preset_Seed()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            var seeded = model.PreviewConfig;
            var button = page.FindControl<Button>("PreviewPrimary")!;
            var reset = page.FindControl<Button>("ResetPrimaryButton")!;
            var swatches = page.FindControl<ItemsControl>("PrimarySwatchBar")!
                               .GetVisualDescendants().OfType<AtomUI.Desktop.Controls.RadioButton>().ToArray();
            swatches.Select(AutomationProperties.GetName).ShouldBe(["Noir", "Emerald", "Blue", "Violet", "Rose", "Amber"]);
            // With no selection the preset's own seed (noir) is shown as the selected swatch.
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([true, false, false, false, false, false]);
            reset.Command!.CanExecute(reset.CommandParameter).ShouldBeFalse();

            // Keyboard: focus the blue swatch and press Space.
            swatches[2].Focus().ShouldBeTrue();
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Pump();
            model.SelectedPrimary.ShouldBe(Color.Parse("#3B82F6"));
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([false, false, true, false, false, false]);
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#3B82F6"));
            reset.Command.CanExecute(reset.CommandParameter).ShouldBeTrue();

            reset.Command.Execute(reset.CommandParameter); Pump();
            model.SelectedPrimary.ShouldBeNull();
            model.PreviewConfig.ShouldBeSameAs(seeded);
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#020617"));
            AssertIndependentProfile(button, (ThemeManager)Application.Current!.GetThemeManager()!, "PrimeNGAura");
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([true, false, false, false, false, false]);
            reset.Command.CanExecute(reset.CommandParameter).ShouldBeFalse();

            // Selecting the swatch equal to the own seed is the same as clearing: no new config, reset stays disabled.
            swatches[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            model.SelectedPrimary.ShouldBeNull();
            model.PreviewConfig.ShouldBeSameAs(seeded);
            reset.Command.CanExecute(reset.CommandParameter).ShouldBeFalse();
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([true, false, false, false, false, false]);
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Preset_And_Swatch_Bars_Do_Not_Clip_The_Wave_Spirit_Of_Their_Items()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            foreach (var barName in new[] { "PresetBar", "PrimarySwatchBar" })
            {
                var bar = page.FindControl<ItemsControl>(barName)!;
                var items = bar.GetVisualDescendants().OfType<Control>()
                               .Where(c => c is Button or AtomUI.Desktop.Controls.RadioButton).ToList();
                items.ShouldNotBeEmpty(barName);
                foreach (var item in items)
                {
                    // The wave spirit is drawn outside the item's bounds; nothing up to the bar may clip it.
                    foreach (var ancestor in item.GetVisualAncestors().OfType<Visual>())
                    {
                        ancestor.ClipToBounds.ShouldBeFalse($"{barName}: {ancestor.GetType().Name}");
                        if (ReferenceEquals(ancestor, bar)) break;
                    }
                }
            }
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Swatches_Are_Disabled_With_A_Note_On_A_Preset_That_Fixes_Its_Primary_States()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            var bar = page.FindControl<ItemsControl>("PrimarySwatchBar")!;
            var note = page.FindControl<Avalonia.Controls.TextBlock>("PrimaryFixedNote")!;
            var reset = page.FindControl<Button>("ResetPrimaryButton")!;
            model.CanChoosePrimary.ShouldBeTrue();
            bar.IsEffectivelyEnabled.ShouldBeTrue();
            note.IsVisible.ShouldBeFalse();
            model.SelectedPrimary = Color.Parse("#8B5CF6"); Pump();

            // A preset that fixes its brand color keeps the swatch bar disabled with the note.
            foreach (var (dark, compact) in new[] { (false, false), (true, false), (true, true), (false, true) })
            {
                model.SelectedPresetId = "Material";
                model.IsDark = dark; model.IsCompact = compact; Pump();
                model.CanChoosePrimary.ShouldBeFalse(model.PreviewThemeId);
                model.SelectedPrimary.ShouldBeNull();
                bar.IsVisible.ShouldBeTrue();
                bar.IsEffectivelyEnabled.ShouldBeFalse();
                note.IsVisible.ShouldBeTrue();
                string.IsNullOrWhiteSpace(note.Text).ShouldBeFalse();
                reset.Command!.CanExecute(reset.CommandParameter).ShouldBeFalse();
                var config = model.PreviewConfig;
                model.SelectedPrimary = Color.Parse("#3B82F6"); Pump();
                model.SelectedPrimary.ShouldBeNull();
                model.PreviewConfig.ShouldBeSameAs(config);
            }

            model.IsDark = false; model.IsCompact = false;
            model.SelectedPresetId = "PrimeNGAura"; Pump();
            model.CanChoosePrimary.ShouldBeTrue();
            bar.IsEffectivelyEnabled.ShouldBeTrue();
            note.IsVisible.ShouldBeFalse();
            model.SelectedPrimary = Color.Parse("#3B82F6"); Pump();
            model.SelectedPrimary.ShouldBe(Color.Parse("#3B82F6"));
            Color.Parse(model.PreviewConfig!.Tokens["ColorPrimary"]).ShouldBe(Color.Parse("#3B82F6"));
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Own_Seed_Swatch_Follows_The_Previewed_Mode_Profile()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "Material" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            var button = page.FindControl<Button>("PreviewPrimary")!;
            var bar = page.FindControl<ItemsControl>("PrimarySwatchBar")!;
            bool[] Checked() => bar.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.RadioButton>()
                                   .Select(swatch => swatch.IsChecked == true).ToArray();
            model.CanChoosePrimary.ShouldBeFalse();
            model.PrimarySwatches.Count.ShouldBe(6);
            model.PrimarySwatches[^1].ShouldBe(Color.Parse("#6750A4"));
            model.PrimarySwatchItems[^1].IsSelected.ShouldBeTrue();
            Checked().ShouldBe([false, false, false, false, false, true]);
            var light = model.PreviewConfig;
            model.SelectedPrimary = Color.Parse("#6750A4"); Pump();
            model.SelectedPrimary.ShouldBeNull();
            model.PreviewConfig.ShouldBeSameAs(light);

            model.IsDark = true; Pump();
            model.PreviewThemeId.ShouldBe("Material.Dark");
            model.PrimarySwatches.Count.ShouldBe(6);
            model.PrimarySwatches[^1].ShouldBe(Color.Parse("#D0BCFF"));
            model.PrimarySwatchItems.Single(item => item.IsSelected).Color.ShouldBe(Color.Parse("#D0BCFF"));
            Checked().ShouldBe([false, false, false, false, false, true]);
            model.SelectedPrimary.ShouldBeNull();
            Color.Parse(model.PreviewConfig!.Tokens["ColorPrimary"]).ShouldBe(Color.Parse("#D0BCFF"));
            AssertIndependentProfile(button, manager, "Material.Dark");

            // Material declares a fixed brand color, so a swatch click is ignored and the own seed stays selected.
            model.SelectedPrimary = Color.Parse("#F43F5E"); Pump();
            model.SelectedPrimary.ShouldBeNull();
            model.IsDark = false; Pump();
            model.PrimarySwatches[^1].ShouldBe(Color.Parse("#6750A4"));
            Checked().ShouldBe([false, false, false, false, false, true]);
            Color.Parse(model.PreviewConfig!.Tokens["ColorPrimary"]).ShouldBe(Color.Parse("#6750A4"));
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Copied_Theme_Xml_Includes_The_Selected_Primary_And_Validates_Against_The_Xsd()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAura" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            model.SelectedPrimary = Color.Parse("#8B5CF6"); Pump();
            var copy = page.FindControl<Button>("CopyThemeButton")!;
            copy.Command!.Execute(copy.CommandParameter);
            WaitUntil(() => model.CopySucceeded);
            var xml = ReadClipboard(window);
            using (var schema = typeof(IThemeManager).Assembly.GetManifestResourceStream("AtomUI.Theme.Definitions.Schemas.atomui-theme-v1.xsd"))
            {
                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
                using var schemaReader = XmlReader.Create(schema!);
                settings.Schemas.Add("https://atomui.net/schemas/theme/v1", schemaReader);
                using var reader = XmlReader.Create(new StringReader(xml), settings);
                while (reader.Read()) { }
            }
            XNamespace ns = "https://atomui.net/schemas/theme/v1";
            var root = XDocument.Parse(xml).Root!;
            root.Attribute("Id")!.Value.ShouldBe("PrimeNGAura");
            root.Element(ns + "Algorithms")!.Elements()
                .Select(element => ((string?)element.Attribute("Catalog"), element.Attribute("Id")!.Value))
                .ShouldBe([("PrimeNG", "Aura")]);
            var primary = root.Element(ns + "Tokens")!.Elements()
                              .Where(token => token.Attribute("Name")!.Value == "ColorPrimary").ToArray();
            primary.Length.ShouldBe(1);
            Color.Parse(primary[0].Attribute("Value")!.Value).ShouldBe(Color.Parse("#8B5CF6"));
        }
        finally { window.Close(); Pump(); }
    }

    private static void WaitUntil(Func<bool> predicate)
    {
        for (var i = 0; i < 200 && !predicate(); i++) { Pump(); Thread.Sleep(5); }
        predicate().ShouldBeTrue("Copy feedback or command availability did not settle within one second.");
    }

    [Fact]
    public void Nora_Dark_Preview_Renders_Contrasting_Steps_Icons_And_Preserves_Status_Titles()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGNora", IsDark = true };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            // The solid selected background is the coloured-primary Nora rule (noir uses a neutral tint).
            model.SelectedPrimary = Color.Parse("#10B981"); Pump();
            foreach (var compact in new[] { false, true })
            {
                model.IsCompact = compact; Pump();
                var steps = page.GetVisualDescendants().OfType<Steps>().Single();
                var items = steps.GetVisualDescendants().OfType<StepsItem>().ToArray();
                var finishMark = items[0].GetVisualDescendants().OfType<AtomUI.Controls.Icon>().Single(icon => icon.Name == "FinishedMark");
                ((ISolidColorBrush)finishMark.FillBrush!).Color.ShouldBe(Color.Parse("#09090B"));
                var number = items[1].GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>().Single(text => text.Name == "StepNumberText");
                ((ISolidColorBrush)number.Foreground!).Color.ShouldBe(Color.Parse("#09090B"));
                ((ISolidColorBrush)items[0].Foreground!).Color.ShouldBe(Colors.White);
                ((ISolidColorBrush)items[1].Foreground!).Color.ShouldBe(Colors.White);
                ((ISolidColorBrush)items[2].Foreground!).Color.ShouldBe(Color.Parse("#A1A1AA"));
                var tokens = ControlTokenAccessor.Capture(steps);
                tokens.GetOwn<Color, StepsTokenKind>(StepsTokenKind.FinishIconBgColor).ShouldBe(Color.Parse("#10B981"));
                var finishIndicator = items[0].GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_Indicator");
                ThemeScope.GetTokenOwner(finishIndicator).ShouldBe(StepsTokens.Identity);
            }
        }
        finally { window.Close(); Pump(); }
    }

    private static void AssertIndependentProfile(Button button, ThemeManager manager, string id)
    {
        var registry = manager.CurrentSnapshot!.Registry;
        var entry = manager.CompiledThemeCatalog.Get(id);
        var compiled = new ThemeCompiler().Compile(new ThemeCompileInput(entry.Definition, entry.Revision,
            ThemeCompiler.CreateDefinitionDefaults(entry.Definition, registry), registry));
        compiled.Success.ShouldBeTrue();
        var expected = compiled.Snapshot!;
        var actual = ThemeScope.ResolveContext(button)!.Snapshot;
        foreach (var token in registry.GlobalTokens)
        {
            token.Format(actual.GlobalTokenValues.GetValue(token.Slot))
                .ShouldBe(token.Format(expected.GlobalTokenValues.GetValue(token.Slot)), token.Name);
        }
        registry.TryGetControl(typeof(Button), out var descriptor).ShouldBeTrue();
        foreach (var token in descriptor!.OwnTokens)
        {
            token.Format(actual.Controls[descriptor.Slot].ControlTokenValues.GetValue(token.Slot))
                .ShouldBe(token.Format(expected.Controls[descriptor.Slot].ControlTokenValues.GetValue(token.Slot)), token.Name);
        }
    }

    private static string ReadClipboard(Window window)
    {
        using var data = window.Clipboard!.TryGetInProcessDataAsync().GetAwaiter().GetResult();
        return data!.TryGetValueAsync(DataFormat.Text).GetAwaiter().GetResult()!;
    }

    private static void AssertTokens(Button button, string primary, string container, int height, int weight)
    {
        var tokens = ControlTokenAccessor.Capture(button);
        tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse(primary));
        tokens.GetGlobal<Color>(SharedTokenKind.ColorBgContainer).ShouldBe(Color.Parse(container));
        tokens.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe(height);
        tokens.GetOwn<FontWeight, ButtonTokenKind>(ButtonTokenKind.FontWeight).ShouldBe((FontWeight)weight);
    }

    private static void Apply(IThemeManager manager, ThemeRequest request)
    {
        var task = manager.ApplyThemeAsync(request);
        for (var i = 0; i < 200 && !task.IsCompleted; i++) { Pump(); Thread.Sleep(5); }
        task.GetAwaiter().GetResult().Status.ShouldBe(ThemeTransitionStatus.Committed);
        Pump();
    }
    private static void Pump() => Dispatcher.UIThread.RunJobs();
}
