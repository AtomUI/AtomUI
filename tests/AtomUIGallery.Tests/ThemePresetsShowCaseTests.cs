using System.Xml.Linq;
using System.Reactive.Linq;
using System.Xml;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using AtomUIGallery.ShowCases.ThemePresets;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
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
    public void Primary_Choice_Follows_The_Adjustable_And_Locked_Seeds()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        model.SelectedPresetId = "DaybreakBlue";
        model.CanChoosePrimary.ShouldBeTrue();
        // A named theme that leaves the Ant Design brand seed open can choose a primary.
        model.SelectedPresetId = "GalleryPreviewFixture";
        model.CanChoosePrimary.ShouldBeTrue();
        // A theme that locks the brand seed cannot.
        model.SelectedPresetId = "GalleryFixedBrandFixture";
        model.CanChoosePrimary.ShouldBeFalse();
        var exported = model.ExportThemeXml();
        ValidateV2(exported);
        var root = XElement.Parse(exported);
        var primary = root.Element(root.Name.Namespace + "Tokens")!.Elements().Single();
        primary.Attribute("Name")!.Value.ShouldBe("ColorPrimary");
        primary.Attribute("Locked")!.Value.ShouldBe("true");
    }

    [Fact]
    public void Selected_Swatch_Survives_Mode_Toggles_On_A_Seed_Family()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
        var rose = Color.Parse("#F43F5E");
        model.SelectedPrimary = rose;
        foreach (var (dark, compact) in new[] { (true, false), (true, true), (false, true), (false, false) })
        {
            model.IsDark = dark;
            model.IsCompact = compact;
            model.PreviewThemeId.ShouldBe("DaybreakBlue");
            model.PreviewConfig!.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
            model.PreviewConfig.Density.ShouldBe(compact ? DesignDensity.Compact : DesignDensity.Default);
            model.SelectedPrimary.ShouldBe(rose);
            Color.Parse(model.PreviewConfig.Tokens["ColorPrimary"]).ShouldBe(rose);
        }
    }

    [Fact]
    public void Export_Writes_Design_Language_Metadata()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
        model.IsDark = true;
        model.IsCompact = true;
        model.PreviewThemeId.ShouldBe("DaybreakBlue");
        var xml = model.ExportThemeXml();
        var root = XDocument.Parse(xml).Root!;
        root.Attribute("Language")!.Value.ShouldBe("AntDesign");
        root.Attribute("Appearance")!.Value.ShouldBe("Dark");
        root.Attribute("Density")!.Value.ShouldBe("Compact");
        root.Attribute("DesignLanguage").ShouldBeNull();
        root.Attribute("BrandColor").ShouldBeNull();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var result = AtomUI.Theme.Definitions.ThemeDocumentReader.Read(stream, "export.theme.xml");
        result.Diagnostics.ShouldBeEmpty();
        result.Document.ShouldNotBeNull();

        model.SelectedPresetId = model.Presets.Single(item => item.Name == "Shadcn Maia").ThemeId;
        model.NeutralSwatchItems.Single(swatch => swatch.IsSelected).Color.ShouldBe(Color.Parse("#737373"));
        model.SelectedPrimary = Color.Parse("#1447E6");
        model.SelectedNeutral = Color.Parse("#71717B");
        var languageExport = model.ExportThemeXml();
        ValidateV2(languageExport);
        var languageRoot = XDocument.Parse(languageExport).Root!;
        var ns = languageRoot.Name.Namespace;
        languageRoot.Attribute("Id")!.Value.ShouldBe("ShadcnMaia");
        languageRoot.Attribute("Name")!.Value.ShouldBe("Shadcn Maia");
        languageRoot.Attribute("Language")!.Value.ShouldBe("Shadcn");
        languageRoot.Attribute("Style")!.Value.ShouldBe("Maia");
        languageRoot.Attribute("Appearance")!.Value.ShouldBe("Dark");
        languageRoot.Attribute("Density")!.Value.ShouldBe("Compact");
        var primary = languageRoot.Element(ns + "Tokens")!.Elements().Single();
        primary.Attribute("Name")!.Value.ShouldBe("ColorPrimary");
        primary.Attribute("Value")!.Value.ShouldBe("#1447E6");
        var baseColor = languageRoot.Element(ns + "LanguageTokens")!.Elements().Single();
        baseColor.Attribute("Name")!.Value.ShouldBe("BaseColor");
        baseColor.Attribute("Value")!.Value.ShouldBe("#71717B");

        model.SelectedPresetId = "ShadcnNovaFixture";
        model.SelectedNeutral = null;
        var defaults = model.PreviewConfig;
        model.NeutralSwatchItems.Single(swatch => swatch.IsSelected).Color.ShouldBe(Color.Parse("#737373"));
        model.SelectedNeutral = Color.Parse("#737373");
        model.SelectedNeutral.ShouldBeNull();
        model.PreviewConfig.ShouldBeSameAs(defaults);
        XElement.Parse(model.ExportThemeXml()).Element(ns + "LanguageTokens").ShouldBeNull();
    }

    [Fact]
    public void Language_Styles_Replace_The_Package_Profiles_And_Named_Themes_Preview_Every_Mode()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        var catalog = ((ThemeManager)Application.Current!.GetThemeManager()!).CompiledThemeCatalog;
        var primeNG = catalog.Languages.Single(language => language.Id == "PrimeNG");
        primeNG.Styles.Select(style => style.Id).ShouldBe(["Aura", "Lara", "Nora"]);
        primeNG.DefaultStyle.ShouldBe(new DesignStyle("Aura"));
        primeNG.Densities.ShouldBe([DesignDensity.Default, DesignDensity.Compact]);
        var shadcn = catalog.Languages.Single(language => language.Id == "Shadcn");
        shadcn.Styles.Select(style => style.Id).ShouldBe(["Vega", "Nova", "Maia", "Lyra", "Mira", "Luma", "Sera", "Rhea"]);
        shadcn.DefaultStyle.ShouldBe(new DesignStyle("Nova"));
        shadcn.Densities.ShouldBe([DesignDensity.Default, DesignDensity.Compact]);

        var styleLabels = new[]
        {
            "PrimeNG Aura", "PrimeNG Lara", "PrimeNG Nora",
            "Shadcn Luma", "Shadcn Lyra", "Shadcn Maia", "Shadcn Mira", "Shadcn Nova", "Shadcn Rhea", "Shadcn Sera", "Shadcn Vega"
        };
        var languageChoices = model.Presets.Take(styleLabels.Length).ToArray();
        languageChoices.Select(item => item.Name).ShouldBe(styleLabels);
        languageChoices.ShouldAllBe(item => item.Icon != null);
        model.Presets.ShouldNotContain(item => item.ThemeId == "DaybreakBlue");
        model.SelectedPresetId.ShouldBe(languageChoices[0].ThemeId);
        model.PreviewThemeId.ShouldBe("PrimeNGAura");
        model.PreviewConfig!.Language.ShouldBe("PrimeNG");
        model.PreviewConfig.Style.ShouldBe(new DesignStyle("Aura"));
        model.PreviewConfig.Appearance.ShouldBe(ThemeAppearance.Light);
        model.PreviewConfig.Density.ShouldBe(DesignDensity.Default);
        model.PreviewConfig.Inherit.ShouldBeFalse();

        foreach (var choice in languageChoices)
        {
            model.SelectedPresetId = choice.ThemeId;
            var names = choice.Name.Split(' ');
            foreach (var (dark, compact) in new[] { (false, false), (true, false), (false, true), (true, true) })
            {
                model.IsDark = dark;
                model.IsCompact = compact;
                model.PreviewThemeId.ShouldBe(names[0] + names[1]);
                model.PreviewConfig!.Language.ShouldBe(names[0]);
                model.PreviewConfig.Style.ShouldBe(new DesignStyle(names[1]));
                model.PreviewConfig.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
                model.PreviewConfig.Density.ShouldBe(compact ? DesignDensity.Compact : DesignDensity.Default);
                model.PreviewConfig.Inherit.ShouldBeFalse();
            }
        }

        // The implicit language entries are in the catalog but are not presets, and no language package ships a
        // PrimeNG or Shadcn named theme: the only ones come from this test project's fixtures.
        foreach (var language in new[] { "AntDesign", "PrimeNG", "Shadcn" })
        {
            catalog.Get(ThemeInfo.LanguageDefaultId(language)).Info.IsLanguageDefault.ShouldBeTrue();
            model.Presets.ShouldNotContain(item => item.ThemeId == ThemeInfo.LanguageDefaultId(language));
        }
        var fixtures = catalog.ResolverSlices.Single(slice => slice.Definitions.Any(entry => entry.Info.Id == "GalleryPreviewFixture"));
        catalog.ResolverSlices.Where(slice => !ReferenceEquals(slice, fixtures)).SelectMany(slice => slice.Definitions)
               .ShouldNotContain(entry => !entry.Info.IsLanguageDefault && (entry.Info.Language == "PrimeNG" || entry.Info.Language == "Shadcn"));

        model.Presets.Where(item => item.ThemeId.StartsWith("PrimeNG", StringComparison.Ordinal)).Select(item => item.ThemeId)
             .ShouldBe(["PrimeNGAuraFixture", "PrimeNGNoraFixture"]);
        model.Presets.Where(item => item.ThemeId.StartsWith("PrimeNG", StringComparison.Ordinal) ||
                                    item.ThemeId.StartsWith("Shadcn", StringComparison.Ordinal))
             .ShouldAllBe(item => item.Icon != null);
        model.Presets.Single(item => item.ThemeId == "GalleryPreviewFixture").Icon.ShouldBeNull();
        foreach (var (id, style) in new[]
                 {
                     ("PrimeNGAuraFixture", "Aura"), ("PrimeNGNoraFixture", "Nora"),
                     ("ShadcnNovaFixture", "Nova"), ("ShadcnMaiaFixture", "Maia"), ("ShadcnSeraFixture", "Sera")
                 })
        {
            fixtures.Definitions.ShouldContain(entry => entry.Info.Id == id && entry.Revision.ProgramDigest != null);
            foreach (var (dark, compact) in new[] { (false, false), (true, false), (false, true), (true, true) })
            {
                model.SelectedPresetId = id;
                model.IsDark = dark;
                model.IsCompact = compact;
                model.PreviewThemeId.ShouldBe(id);
                model.PreviewConfig!.Style.ShouldBe(new DesignStyle(style));
                model.PreviewConfig.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
                model.PreviewConfig.Density.ShouldBe(compact ? DesignDensity.Compact : DesignDensity.Default);
            }
        }
    }

    [Fact]
    public void Open_Theme_Preview_Writes_The_Chosen_Appearance_And_Density()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        model.SelectedPresetId = "GalleryPreviewFixture";
        model.IsDark = true;
        model.IsCompact = true;
        model.SelectedPresetId.ShouldBe("GalleryPreviewFixture");
        model.PreviewThemeId.ShouldBe("GalleryPreviewFixture");
        // The theme fixes no axis, so the preview writes the chosen ones next to the theme's language.
        model.PreviewConfig!.Language.ShouldBe("AntDesign");
        model.PreviewConfig.Appearance.ShouldBe(ThemeAppearance.Dark);
        model.PreviewConfig.Density.ShouldBe(DesignDensity.Compact);
        model.PreviewConfig.Inherit.ShouldBeFalse();

        model.IsDark = false;
        model.IsCompact = false;
        model.PreviewThemeId.ShouldBe("GalleryPreviewFixture");
        model.PreviewConfig!.Appearance.ShouldBe(ThemeAppearance.Light);
        model.PreviewConfig.Density.ShouldBe(DesignDensity.Default);
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
        // A PrimeNG Aura named theme stands for the v1 PrimeNGAura profile family.
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAuraFixture" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            var button = page.FindControl<Button>("PreviewPrimary")!;
            var provider = page.FindControl<ThemeConfigProvider>("PreviewProvider")!;
            var initial = button;
            AssertTokens(button, "#020617", "#FFFFFF", 35, 500);
            AssertIndependentProfile(button, (ThemeManager)manager, "PrimeNGAuraFixture");
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorInfoBg).ShouldNotBe(Color.Parse("#123456"));
            foreach (var (dark, compact, color, background, height) in new[]
            {
                // The PrimeNG default primary is noir: slate-950 in light, zinc-50 in dark.
                (true, false, "#FAFAFA", "#09090B", 35),
                (true, true, "#FAFAFA", "#09090B", 31),
                (false, true, "#020617", "#FFFFFF", 31),
                (false, false, "#020617", "#FFFFFF", 35)
            })
            {
                model.IsDark = dark; model.IsCompact = compact; Pump();
                model.SelectedPresetId.ShouldBe("PrimeNGAuraFixture");
                model.PreviewThemeId.ShouldBe("PrimeNGAuraFixture");
                model.PreviewConfig!.Language.ShouldBe("PrimeNG");
                model.PreviewConfig.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
                model.PreviewConfig.Density.ShouldBe(compact ? DesignDensity.Compact : DesignDensity.Default);
                AssertTokens(button, color, background, height, 500);
                AssertIndependentProfile(button, (ThemeManager)manager, model.PreviewThemeId!, dark, compact);
                ControlTokenAccessor.Capture(button).GetGlobal<double>(SharedTokenKind.ControlHeightSM).ShouldBe(compact ? 23.25 : 28);
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
                provider.Config.ShouldBeSameAs(model.PreviewConfig);
            }
            model.SelectedPresetId = "PrimeNGNoraFixture"; Pump();
            ControlTokenAccessor.Capture(button).GetOwn<FontWeight, ButtonTokenKind>(ButtonTokenKind.FontWeight).ShouldBe((FontWeight)700);
            foreach (var (id, primary, height) in new[]
            {
                ("ShadcnNovaFixture", "#171717", 32d), ("ShadcnMaiaFixture", "#171717", 36d), ("ShadcnSeraFixture", "#171717", 40d)
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
                AssertIndependentProfile(button, (ThemeManager)manager, id, dark: true, compact: true);
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
            }
            model.SelectedPresetId = model.Presets.Single(item => item.Name == "PrimeNG Aura").ThemeId;
            model.IsDark = false;
            model.IsCompact = false;
            Pump();
            AssertTokens(button, "#020617", "#FFFFFF", 35, 500);
            AssertIndependentProfile(button, (ThemeManager)manager, ThemeInfo.LanguageDefaultId("PrimeNG"));

            model.SelectedPresetId = model.Presets.Single(item => item.Name == "Shadcn Maia").ThemeId;
            model.IsDark = true;
            model.IsCompact = true;
            model.SelectedNeutral = Color.Parse("#71717B");
            Pump();
            var shadcnPreview = ControlTokenAccessor.Capture(button);
            shadcnPreview.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe(32);
            shadcnPreview.GetGlobal<Color>(SharedTokenKind.ColorBgContainer).ShouldBe(Color.Parse("#09090B"));
            page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
            publications.ShouldBe(0);
            manager.CurrentTheme.ShouldBeSameAs(root);
        }
        finally
        {
            manager.ThemeChanged -= Changed;
            window.Close(); Pump();
            Apply(manager, new ThemeRequest(original.ThemeId, new ThemeConfigBuilder().WithAppearance(original.Appearance).WithDensity(original.Variant.Density).Build(), ThemeTransitionReason.UserRequest));
        }
    }

    [Fact]
    public void Local_Preview_Uses_The_Ant_Design_Theme_Values_And_Never_Publishes_The_Root()
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
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
        var page = new ThemePresetsShowCase { DataContext = model };
        var window = new Window { Width = 1200, Height = 900, Content = page };
        try
        {
            window.Show(); Pump();
            var button = page.FindControl<Button>("PreviewPrimary")!;
            var provider = page.FindControl<ThemeConfigProvider>("PreviewProvider")!;
            var initial = button;
            AssertTokens(button, "#1677FF", "#FFFFFF", 32, 400);
            AssertIndependentProfile(button, (ThemeManager)manager, "DaybreakBlue");
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorInfoBg).ShouldNotBe(Color.Parse("#123456"));
            foreach (var (dark, compact, color, background, height) in new[]
            {
                // Ant Design dark maps the #1677FF seed to #1668DC on a #141414 container; compact controls are 28.
                (true, false, "#1668DC", "#141414", 32),
                (true, true, "#1668DC", "#141414", 28),
                (false, true, "#1677FF", "#FFFFFF", 28),
                (false, false, "#1677FF", "#FFFFFF", 32)
            })
            {
                model.IsDark = dark; model.IsCompact = compact; Pump();
                model.SelectedPresetId.ShouldBe("DaybreakBlue");
                model.PreviewThemeId.ShouldBe("DaybreakBlue");
                model.PreviewConfig!.Language.ShouldBe("AntDesign");
                model.PreviewConfig.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
                model.PreviewConfig.Density.ShouldBe(compact ? DesignDensity.Compact : DesignDensity.Default);
                AssertTokens(button, color, background, height, 400);
                ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorInfoBg).ShouldNotBe(Color.Parse("#123456"));
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
                provider.Config.ShouldBeSameAs(model.PreviewConfig);
            }
            AssertIndependentProfile(button, (ThemeManager)manager, "DaybreakBlue");
            publications.ShouldBe(0);
            manager.CurrentTheme.ShouldBeSameAs(root);
        }
        finally
        {
            manager.ThemeChanged -= Changed;
            window.Close(); Pump();
            Apply(manager, new ThemeRequest(original.ThemeId, new ThemeConfigBuilder().WithAppearance(original.Appearance).WithDensity(original.Variant.Density).Build(), ThemeTransitionReason.UserRequest));
        }
    }

    [Fact]
    public void Copy_Command_Exports_Active_Profile_And_Own_Overrides_To_Real_Clipboard()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
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
            ValidateV2(xml);
            var root = XDocument.Parse(xml).Root!;
            XNamespace ns = "https://atomui.net/schemas/theme/v2";
            root.Name.ShouldBe(ns + "Theme");
            root.Attribute("Id")!.Value.ShouldBe("DaybreakBlue");
            root.Attribute("Language")!.Value.ShouldBe("AntDesign");
            root.Attribute("Appearance")!.Value.ShouldBe("Dark");
            root.Attribute("Density")!.Value.ShouldBe("Compact");
            // Daybreak Blue carries only its seeds; colours and control values come from the Ant Design derivations,
            // so the export has no Controls section.
            Color.Parse(root.Element(ns + "Tokens")!.Elements().Single(token => token.Attribute("Name")!.Value == "ColorPrimary").Attribute("Value")!.Value).ShouldBe(Color.Parse("#1677FF"));
            root.Element(ns + "Controls").ShouldBeNull();
            model.IsDark = false; Pump(); model.CopySucceeded.ShouldBeFalse();
        }
        finally { window.Close(); Pump(); }
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
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGAuraFixture" };
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
            model.PreviewConfig.Language.ShouldBe("PrimeNG");
            var tokens = ControlTokenAccessor.Capture(button);
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#3B82F6"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimaryHover).ShouldBe(Color.Parse("#2563EB"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimaryActive).ShouldBe(Color.Parse("#1D4ED8"));
            tokens.GetGlobal<Color>(SharedTokenKind.ColorFocusBorder)
                  .ShouldBe(tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary));

            // The selection follows the dark profile: Aura dark maps the seed to blue-400 with a blue-300 hover.
            model.IsDark = true; Pump();
            model.PreviewThemeId.ShouldBe("PrimeNGAuraFixture");
            model.PreviewConfig!.Appearance.ShouldBe(ThemeAppearance.Dark);
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
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
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
            swatches.Select(AutomationProperties.GetName).ShouldBe(["Emerald", "Blue", "Violet", "Rose", "Amber", "Preset color"]);
            // With no selection the preset's own seed (#1677FF) is shown as the selected swatch.
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([false, false, false, false, false, true]);
            reset.Command!.CanExecute(reset.CommandParameter).ShouldBeFalse();

            // Keyboard: focus the blue swatch and press Space.
            swatches[1].Focus().ShouldBeTrue();
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Pump();
            model.SelectedPrimary.ShouldBe(Color.Parse("#3B82F6"));
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([false, true, false, false, false, false]);
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#3B82F6"));
            reset.Command.CanExecute(reset.CommandParameter).ShouldBeTrue();

            reset.Command.Execute(reset.CommandParameter); Pump();
            model.SelectedPrimary.ShouldBeNull();
            model.PreviewConfig.ShouldBeSameAs(seeded);
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse("#1677FF"));
            AssertIndependentProfile(button, (ThemeManager)Application.Current!.GetThemeManager()!, "DaybreakBlue");
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([false, false, false, false, false, true]);
            reset.Command.CanExecute(reset.CommandParameter).ShouldBeFalse();

            // Selecting the swatch equal to the own seed is the same as clearing: no new config, reset stays disabled.
            swatches[5].RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            model.SelectedPrimary.ShouldBeNull();
            model.PreviewConfig.ShouldBeSameAs(seeded);
            reset.Command.CanExecute(reset.CommandParameter).ShouldBeFalse();
            swatches.Select(swatch => swatch.IsChecked == true).ShouldBe([false, false, false, false, false, true]);
        }
        finally { window.Close(); Pump(); }
    }

    [Fact]
    public void Preset_And_Swatch_Bars_Do_Not_Clip_The_Wave_Spirit_Of_Their_Items()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
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
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
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
                model.SelectedPresetId = "GalleryFixedBrandFixture";
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
            model.SelectedPresetId = "DaybreakBlue"; Pump();
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
    public void Copied_Theme_Xml_Includes_The_Selected_Primary_And_Validates_Against_The_Xsd()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "DaybreakBlue" };
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
            ValidateV2(xml);
            XNamespace ns = "https://atomui.net/schemas/theme/v2";
            var root = XDocument.Parse(xml).Root!;
            root.Attribute("Id")!.Value.ShouldBe("DaybreakBlue");
            root.Attribute("Language")!.Value.ShouldBe("AntDesign");
            var primary = root.Element(ns + "Tokens")!.Elements()
                              .Where(token => token.Attribute("Name")!.Value == "ColorPrimary").ToArray();
            primary.Length.ShouldBe(1);
            Color.Parse(primary[0].Attribute("Value")!.Value).ShouldBe(Color.Parse("#8B5CF6"));
        }
        finally { window.Close(); Pump(); }
    }

    private static void ValidateV2(string xml)
    {
        using var schema = typeof(IThemeManager).Assembly.GetManifestResourceStream("AtomUI.Theme.Definitions.Schemas.atomui-theme-v2.xsd");
        var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
        using var schemaReader = XmlReader.Create(schema!);
        settings.Schemas.Add("https://atomui.net/schemas/theme/v2", schemaReader);
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        while (reader.Read()) { }
    }

    private static void WaitUntil(Func<bool> predicate)
    {
        for (var i = 0; i < 200 && !predicate(); i++) { Pump(); Thread.Sleep(5); }
        predicate().ShouldBeTrue("Copy feedback or command availability did not settle within one second.");
    }

    [Fact]
    public void Nora_Dark_Preview_Renders_Contrasting_Steps_Icons_And_Primary_Active_Title()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!) { SelectedPresetId = "PrimeNGNoraFixture", IsDark = true };
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
                // PrimeUIX steps item.label.active.color is the primary colour.
                ((ISolidColorBrush)items[1].Foreground!).Color.ShouldBe(Color.Parse("#10B981"));
                ((ISolidColorBrush)items[2].Foreground!).Color.ShouldBe(Color.Parse("#A1A1AA"));
                var tokens = ControlTokenAccessor.Capture(steps);
                tokens.GetOwn<Color, StepsTokenKind>(StepsTokenKind.FinishIconBgColor).ShouldBe(Color.Parse("#10B981"));
                var finishIndicator = items[0].GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_Indicator");
                ThemeScope.GetTokenOwner(finishIndicator).ShouldBe(StepsTokens.Identity);
            }
        }
        finally { window.Close(); Pump(); }
    }

    // The preview must equal the theme compiled on its own in the previewed variant (the theme's open appearance and
    // density axes take the page's toggles, the successor of compiling the v1 mode profile).
    private static void AssertIndependentProfile(Button button, ThemeManager manager, string id, bool dark = false, bool compact = false)
    {
        var registry = manager.CurrentSnapshot!.Registry;
        var entry = manager.CompiledThemeCatalog.Get(id);
        var definition = entry.Definition;
        var defaults = dark || compact
            ? NormalizedThemeConfig.CreateCanonical(false, definition.Axes with
                {
                    Appearance = definition.Appearance ?? (dark ? ThemeAppearance.Dark : ThemeAppearance.Light),
                    Density = definition.Density ?? (compact ? DesignDensity.Compact : DesignDensity.Default)
                }, [], [], [], definition)
            : ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var compiled = new ThemeCompiler().Compile(new ThemeCompileInput(entry.Definition, entry.Revision,
            defaults, registry));
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
