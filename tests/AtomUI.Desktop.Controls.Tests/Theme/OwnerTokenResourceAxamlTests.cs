using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
    public void Compiled_Axaml_Resolves_Owner_Token_Resource_In_Setter_Property_And_Template()
    {
        var view = new OwnerTokenResourceView();
        var provider = new ThemeConfigProvider
        {
            Child = view,
            Config = new ThemeConfigBuilder()
                     .WithControl(
                         ButtonTokens.Identity,
                         new ControlThemeConfigBuilder()
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

            // Direct use on a ControlTemplate element takes Template priority: the owner value applies,
            // and a triggered style setter on the same property still wins over it.
            FrameColor(view, "TemplatedHost").ShouldBe(s_buttonBorder);
            FrameColor(view, "OverriddenTemplatedHost").ShouldBe(Color.Parse("#00FF00"));
            view.FindControl<ContentControl>("OverriddenTemplatedHost")!.Classes.Remove("override");
            Dispatcher.UIThread.RunJobs();
            FrameColor(view, "OverriddenTemplatedHost").ShouldBe(s_buttonBorder);
        }
        finally
        {
            window.Close();
        }
    }

    private static Color BorderColor(Control view, string name) =>
        BrushColor(view.FindControl<Border>(name).ShouldNotBeNull());

    private static Color FrameColor(Control view, string hostName) =>
        BrushColor(view.FindControl<ContentControl>(hostName)
                       .ShouldNotBeNull()
                       .GetVisualDescendants()
                       .OfType<Border>()
                       .Single(border => border.Name == "PART_Frame"));

    private static Color BrushColor(Border border) =>
        border.BorderBrush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
}
