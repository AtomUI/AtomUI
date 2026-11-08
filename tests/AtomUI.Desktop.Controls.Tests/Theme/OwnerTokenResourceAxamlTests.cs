using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme;

[Collection(ControlEffectiveGlobalBindingTestCollection.Name)]
public class OwnerTokenResourceAxamlTests
{
    private static readonly Color s_buttonBorder = Color.Parse("#FF0000");

    static OwnerTokenResourceAxamlTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Compiled_Axaml_Resolves_Owner_Token_Resource_In_Setter_And_Property()
    {
        var view = new OwnerTokenResourceView();
        var provider = new ThemeConfigProvider
        {
            Child = view,
            Config = new ThemeConfigBuilder()
                     .WithControl(
                         ButtonTokens.Identity,
                         new ControlThemeConfigBuilder()
                             .WithAlgorithm(ControlAlgorithmMode.Disabled)
                             .WithToken(nameof(DesignToken.ColorBorder), "#FF0000")
                             .Build())
                     .Build()
        };
        provider.ThemeChangeFailed += static (_, args) =>
        {
            throw new InvalidOperationException(args.Exception?.Message ?? "Theme compilation failed.");
        };
        var window = new AvaloniaWindow { Width = 240, Height = 160, Content = provider };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            BorderColor(view, "OwnedFrame").ShouldBe(s_buttonBorder);
            BorderColor(view, "DirectFrame").ShouldBe(s_buttonBorder);
            var unowned = BorderColor(view, "UnownedFrame");
            unowned.ShouldNotBe(s_buttonBorder);
            view.TryFindResource(SharedTokenKind.ColorBorder, view.ActualThemeVariant, out var global).ShouldBeTrue();
            unowned.ShouldBe(global.ShouldBeAssignableTo<ISolidColorBrush>()!.Color);
        }
        finally
        {
            window.Close();
        }
    }

    private static Color BorderColor(Control view, string name) =>
        view.FindControl<Border>(name)
            .ShouldNotBeNull()
            .BorderBrush.ShouldBeAssignableTo<ISolidColorBrush>()!
            .Color;
}
