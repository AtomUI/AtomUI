using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Branding;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using AtomUI.Desktop.Controls;

namespace AtomUIGallery.Workspace.Theming;

/// <summary>Finite AtomUI adaptations, expressed through the registered version-one recipe protocol.</summary>
internal static class GalleryBrandPresets
{
    internal static IReadOnlyList<string> Ids { get; } = Array.AsReadOnly(new[]
    {
        "Ant", "shadcn", "Bootstrap", "Square", "HeavyBorder", "LargeType", "Compact", "ReducedMotion"
    });

    internal static ThemeBrandRecipe Create(IThemeManager runtime, string id, bool dark = false)
    {
        if (!Ids.Contains(id, StringComparer.Ordinal))
        {
            throw new ArgumentException("Unknown Gallery brand preset.", nameof(id));
        }
        // The current registered definition supplies its real schema and definition requirements.
        // Presets reset configuration, rather than accidentally inheriting a previous draft's overrides/assets.
        var basis = ThemeBrandRecipes.ExportCurrent(runtime, "Gallery.Preset", "1");
        var algorithms = new List<ThemeAlgorithm> { ThemeAlgorithm.Default };
        if (id == "Compact")
        {
            algorithms.Add(ThemeAlgorithm.Compact);
        }
        if (dark)
        {
            algorithms.Add(ThemeAlgorithm.Dark);
        }
        var config = new ThemeConfigBuilder().WithInherit(false).WithAlgorithms(algorithms.ToArray())
            .WithToken(nameof(DesignToken.ColorPrimary), "#1677ff")
            .WithToken(nameof(DesignToken.ColorLink), "#1677ff");
        switch (id)
        {
            case "shadcn":
                config.WithToken(nameof(DesignToken.ColorPrimary), dark ? "#fafafa" : "#18181b")
                      .WithToken(nameof(DesignToken.ColorPrimaryHover), dark ? "#e4e4e7" : "#27272a")
                      .WithToken(nameof(DesignToken.ColorPrimaryActive), dark ? "#d4d4d8" : "#09090b")
                      .WithToken(nameof(DesignToken.ColorTextLightSolid), dark ? "#18181b" : "#fafafa")
                      .WithToken(nameof(DesignToken.ColorLink), dark ? "#fafafa" : "#18181b")
                      .WithToken(nameof(DesignToken.ColorBgContainer), dark ? "#09090b" : "#ffffff")
                      .WithToken(nameof(DesignToken.ColorBgLayout), dark ? "#18181b" : "#f4f4f5")
                      .WithToken(nameof(DesignToken.ColorBorder), dark ? "#3f3f46" : "#e4e4e7")
                      .WithToken(nameof(DesignToken.BorderRadius), "6")
                      .WithToken(nameof(DesignToken.ControlHeight), "36");
                // Root seed colors participate in Dark palette generation. The solid Button surface
                // is a deliberate brand value, so its own owner uses a direct effective-global override.
                config.WithControl(ControlTokenIdentity.ForControl(typeof(Button), "AtomUI", "Button"),
                    new ControlThemeConfigBuilder().WithAlgorithm(ControlAlgorithmMode.Disabled)
                        .WithToken(nameof(DesignToken.ColorPrimary), dark ? "#fafafa" : "#18181b").Build());
                break;
            case "Bootstrap":
                config.WithToken(nameof(DesignToken.ColorPrimary), "#0d6efd")
                      .WithToken(nameof(DesignToken.ColorPrimaryHover), "#0b5ed7")
                      .WithToken(nameof(DesignToken.ColorPrimaryActive), "#0a58ca")
                      .WithToken(nameof(DesignToken.ColorLink), "#0d6efd")
                      .WithToken(nameof(DesignToken.BorderRadius), "6")
                      .WithToken(nameof(DesignToken.FontSize), "16")
                      .WithToken(nameof(DesignToken.ControlHeight), "38");
                break;
            case "Square":
                foreach (var name in new[] { nameof(DesignToken.BorderRadius), nameof(DesignToken.BorderRadiusXS),
                             nameof(DesignToken.BorderRadiusSM), nameof(DesignToken.BorderRadiusLG), nameof(DesignToken.BorderRadiusOuter) })
                {
                    config.WithToken(name, "0");
                }
                break;
            case "HeavyBorder":
                config.WithToken(nameof(DesignToken.LineWidth), "3")
                      .WithToken(nameof(DesignToken.BorderThickness), "3");
                break;
            case "LargeType":
                config.WithToken(nameof(DesignToken.FontSize), "20")
                      .WithToken(nameof(DesignToken.ControlHeight), "44");
                break;
            case "ReducedMotion":
                config.WithToken(nameof(DesignToken.EnableMotion), "false")
                      .WithToken(nameof(DesignToken.EnableWaveSpirit), "false");
                break;
        }
        return new ThemeBrandRecipe($"Gallery.{id}", "1", basis.ThemeId, config.Build(), [],
            new ThemeBrandRequirements(basis.Requirements.SchemaRevision, basis.Requirements.DefinitionDigest,
                basis.Requirements.ProgramDigest, 0));
    }
}
