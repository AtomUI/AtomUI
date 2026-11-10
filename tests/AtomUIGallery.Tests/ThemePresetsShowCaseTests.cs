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
using AtomUIGallery.ShowCases.ThemePresets;
using Avalonia;
using Avalonia.Controls;
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
    public void Package_Profiles_Are_Grouped_By_Family_And_Compiled_Package_Origin()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var model = new ThemePresetsViewModel(null!);
        model.Presets.Where(item => item.ThemeId.StartsWith("PrimeNG", StringComparison.Ordinal)).Select(item => item.ThemeId).ShouldBe(["PrimeNGAura", "PrimeNGLara", "PrimeNGNora"]);
        model.Presets.Where(item => item.ThemeId.StartsWith("PrimeNG", StringComparison.Ordinal)).ShouldAllBe(item => item.Icon != null);
        model.Presets.Single(item => item.ThemeId == "GalleryPreviewFixture").Icon.ShouldBeNull();
        var families = new[] { "ShadcnZinc", "ShadcnSlate", "ShadcnStone" };
        model.Presets.Where(item => item.ThemeId.StartsWith("Shadcn", StringComparison.Ordinal))
            .Select(item => item.ThemeId).ShouldBe(families, ignoreOrder: true);
        var catalog = ((ThemeManager)Application.Current!.GetThemeManager()!).CompiledThemeCatalog;
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
        model.PreviewConfig!.Algorithms.ShouldBe([ThemeAlgorithm.Dark, ThemeAlgorithm.Compact]);
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
            AssertTokens(button, "#10B981", "#FFFFFF", 40, 500);
            AssertIndependentProfile(button, (ThemeManager)manager, "PrimeNGAura");
            ControlTokenAccessor.Capture(button).GetGlobal<Color>(SharedTokenKind.ColorInfoBg).ShouldNotBe(Color.Parse("#123456"));
            foreach (var (dark, compact, suffix, color, background, height) in new[]
            {
                (true, false, ".Dark", "#2FB685", "#09090B", 40),
                (true, true, ".DarkCompact", "#2FB685", "#09090B", 36),
                (false, true, ".Compact", "#10B981", "#FFFFFF", 36),
                (false, false, "", "#10B981", "#FFFFFF", 40)
            })
            {
                model.IsDark = dark; model.IsCompact = compact; Pump();
                model.SelectedPresetId.ShouldBe("PrimeNGAura");
                model.PreviewThemeId.ShouldBe("PrimeNGAura" + suffix);
                model.PreviewConfig!.Algorithms!.Contains(ThemeAlgorithm.Dark).ShouldBe(dark);
                AssertTokens(button, color, background, height, 500);
                AssertIndependentProfile(button, (ThemeManager)manager, model.PreviewThemeId!);
                ControlTokenAccessor.Capture(button).GetGlobal<double>(SharedTokenKind.ControlHeightSM).ShouldBe(compact ? 27 : 36);
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
                provider.Config.ShouldBeSameAs(model.PreviewConfig);
            }
            model.SelectedPresetId = "PrimeNGNora"; Pump();
            ControlTokenAccessor.Capture(button).GetOwn<FontWeight, ButtonTokenKind>(ButtonTokenKind.FontWeight).ShouldBe((FontWeight)700);
            foreach (var (id, primary) in new[]
            {
                ("ShadcnZinc", "#18181B"), ("ShadcnSlate", "#0F172A"), ("ShadcnStone", "#1C1917")
            })
            {
                model.SelectedPresetId = id;
                model.IsDark = false; model.IsCompact = false; Pump();
                var tokens = ControlTokenAccessor.Capture(button);
                tokens.GetGlobal<Color>(SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse(primary));
                tokens.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe(36);
                tokens.GetGlobal<bool>(SharedTokenKind.EnableWaveSpirit).ShouldBeFalse();
                AssertIndependentProfile(button, (ThemeManager)manager, id);
                model.IsDark = true; model.IsCompact = true; Pump();
                tokens = ControlTokenAccessor.Capture(button);
                tokens.GetGlobal<Color>(SharedTokenKind.ColorBgContainer).ShouldBe(Color.Parse(primary));
                tokens.GetGlobal<double>(SharedTokenKind.ControlHeight).ShouldBe(32);
                AssertIndependentProfile(button, (ThemeManager)manager, id + ".DarkCompact");
                page.FindControl<Button>("PreviewPrimary").ShouldBeSameAs(initial);
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
            Color.Parse(root.Element(ns + "Tokens")!.Elements().Single(token => token.Attribute("Name")!.Value == "ColorBgContainer").Attribute("Value")!.Value).ShouldBe(Color.Parse("#09090B"));
            Enum.Parse<FontWeight>(root.Element(ns + "Controls")!.Elements().Single(control => control.Attribute("Id")!.Value == "Button")
                .Element(ns + "Tokens")!.Elements().Single(token => token.Attribute("Name")!.Value == "FontWeight").Attribute("Value")!.Value).ShouldBe(FontWeight.Medium);
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
