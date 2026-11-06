using AtomUI.Toolkits.GalleryBase.Infrastructure;
using AtomUI.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.CustomizeTheme;

public class CustomizeThemeViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static EntityKey ID = "CustomizeTheme";

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string? UrlPathSegment => ID.ToString();

    private ThemeConfig _runtimeThemeConfig;

    public ThemeConfig GreenThemeConfig { get; }
    public ThemeConfig RedThemeConfig { get; }
    public ThemeConfig PurpleThemeConfig { get; }
    public ThemeConfig DarkThemeConfig { get; }
    public ThemeConfig ControlAlgorithmEnabledConfig { get; }
    public ThemeConfig ControlAlgorithmDisabledConfig { get; }
    public ThemeConfig NestedBlueThemeConfig { get; }
    public ThemeConfig NestedGreenThemeConfig { get; }
    public ThemeConfig NestedOrangeThemeConfig { get; }
    public ThemeConfig RuntimeThemeConfig
    {
        get => _runtimeThemeConfig;
        private set => this.RaiseAndSetIfChanged(ref _runtimeThemeConfig, value);
    }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> UseRuntimePrimaryBlue { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> UseRuntimePrimaryGreen { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> UseRuntimePrimaryMagenta { get; }

    public CustomizeThemeViewModel(IScreen screen)
    {
        Activator                  = new ViewModelActivator();
        HostScreen                 = screen;
        GreenThemeConfig = BuildGlobalConfig(
            "#00b96b",
            (nameof(DesignToken.BorderRadius), "2"));
        RedThemeConfig = BuildGlobalConfig(
            "#ff0000",
            (nameof(DesignToken.BorderRadius), "0"));
        PurpleThemeConfig = BuildGlobalConfig("#7D3C98");
        DarkThemeConfig = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Dark).Build();
        ControlAlgorithmEnabledConfig = BuildControlConfig(ControlAlgorithmMode.Global);
        ControlAlgorithmDisabledConfig = BuildControlConfig(ControlAlgorithmMode.Disabled);
        NestedBlueThemeConfig = BuildGlobalConfig("#1677ff");
        NestedGreenThemeConfig = BuildGlobalConfig("#00b96b");
        NestedOrangeThemeConfig = new ThemeConfigBuilder()
                                 .WithInherit(false)
                                 .WithToken(nameof(DesignToken.ColorPrimary), "#faad14")
                                 .Build();
        _runtimeThemeConfig = BuildGlobalConfig("#1677ff");
        UseRuntimePrimaryBlue      = GalleryCommands.Create(() => SetRuntimePrimaryColor("#1677ff"));
        UseRuntimePrimaryGreen     = GalleryCommands.Create(() => SetRuntimePrimaryColor("#00b96b"));
        UseRuntimePrimaryMagenta   = GalleryCommands.Create(() => SetRuntimePrimaryColor("#eb2f96"));
    }

    private void SetRuntimePrimaryColor(string color)
    {
        RuntimeThemeConfig = BuildGlobalConfig(color);
    }

    private static ThemeConfig BuildGlobalConfig(
        string primaryColor,
        params (string Name, string Value)[] additionalTokens)
    {
        var builder = new ThemeConfigBuilder()
                      .WithToken(nameof(DesignToken.ColorPrimary), primaryColor);
        foreach (var token in additionalTokens)
        {
            builder.WithToken(token.Name, token.Value);
        }
        return builder.Build();
    }

    private static ThemeConfig BuildControlConfig(ControlAlgorithmMode algorithm)
    {
        return new ThemeConfigBuilder()
               .WithControl(
                   new ControlTokenIdentity("AtomUI", "Button"),
                   new ControlThemeConfigBuilder()
                       .WithAlgorithm(algorithm)
                       .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                       .Build())
               .WithControl(
                   new ControlTokenIdentity("AtomUI", "LineEdit"),
                   new ControlThemeConfigBuilder()
                       .WithAlgorithm(algorithm)
                       .WithToken(nameof(DesignToken.ColorPrimary), "#eb2f96")
                       .Build())
               .Build();
    }

}
