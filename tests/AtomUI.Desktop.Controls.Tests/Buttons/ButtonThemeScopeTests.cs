using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUITextBox = AtomUI.Desktop.Controls.TextBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

[Collection(ButtonThemeScopeTestCollection.Name)]
public class ButtonThemeScopeTests
{
    private static readonly Color GlobalColorPrimary = Color.Parse("#1677ff");
    private static readonly Color ButtonPrimary = Color.Parse("#00b96b");
    private static readonly Color UpdatedButtonPrimary = Color.Parse("#ff4d4f");
    private static readonly Color TextBoxPrimary = Color.Parse("#722ed1");

    static ButtonThemeScopeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Button_Control_ColorPrimary_Does_Not_Leak_To_Content()
    {
        var content = new Border
        {
            Width  = 12,
            Height = 12
        };
        content.Bind(
            Border.BackgroundProperty,
            new DynamicResourceExtension(SharedTokenKind.ColorPrimary));

        var button = CreatePrimaryButton(content);
        var provider = Provider(
            button,
            ControlSharedToken(ButtonTokens.Identity, nameof(DesignToken.ColorPrimary), "#00b96b"));

        ShowInWindow(provider, () =>
        {
            SetPrimary(button);

            BrushColor(button.Background).ShouldBe(ButtonPrimary);
            BrushColor(content.Background).ShouldBe(GlobalColorPrimary);
        });
    }

    [Fact]
    public void Button_Control_ColorPrimary_Update_Refreshes_Without_Reattach()
    {
        var button = CreatePrimaryButton("Save");
        var provider = Provider(
            button,
            ControlSharedToken(ButtonTokens.Identity, nameof(DesignToken.ColorPrimary), "#00b96b"));

        ShowInWindow(provider, () =>
        {
            SetPrimary(button);
            BrushColor(button.Background).ShouldBe(ButtonPrimary);

            provider.Config = BuildControlConfig(
                ControlSharedToken(ButtonTokens.Identity, nameof(DesignToken.ColorPrimary), "#ff4d4f"));
            FlushThemeUpdates();

            BrushColor(button.Background).ShouldBe(UpdatedButtonPrimary);
        });
    }

    [Fact]
    public void Button_Primary_Text_Color_Follows_Control_ColorPrimary_Update()
    {
        var button = new AtomUIButton
        {
            Color           = ButtonColor.Primary,
            Variant         = ButtonVariant.Text,
            IsMotionEnabled = false,
            Content         = "Save"
        };
        var provider = Provider(
            button,
            ControlSharedToken(ButtonTokens.Identity, nameof(DesignToken.ColorPrimary), "#00b96b"));

        ShowInWindow(provider, () =>
        {
            BrushColor(button.Foreground).ShouldBe(ButtonPrimary);

            provider.Config = BuildControlConfig(
                ControlSharedToken(ButtonTokens.Identity, nameof(DesignToken.ColorPrimary), "#ff4d4f"));
            FlushThemeUpdates();

            BrushColor(button.Foreground).ShouldBe(UpdatedButtonPrimary);
        });
    }

    [Fact]
    public void Button_Default_Text_Pressed_Background_Uses_ColorFill()
    {
        var colorFill = Color.Parse("#112233");
        var button = new AtomUIButton
        {
            ButtonType      = ButtonType.Text,
            IsMotionEnabled = false,
            Content         = "Text"
        };
        var provider = new ThemeConfigProvider
        {
            Child  = button,
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.ColorFill), "#112233")
                     .WithToken(nameof(DesignToken.ColorBgTextActive), "#445566")
                     .Build()
        };
        ThrowOnCompileFailure(provider);

        ShowInWindow(provider, () =>
        {
            ((IPseudoClasses)button.Classes).Set(StdPseudoClass.Pressed, true);
            FlushThemeUpdates();

            BrushColor(button.Background).ShouldBe(colorFill);
        });
    }

    [Fact]
    public void Nested_TextBox_Uses_TextBox_Control_Config_Inside_Button()
    {
        var textBox = new AtomUITextBox
        {
            Width           = 120,
            IsMotionEnabled = false
        };
        var button = CreatePrimaryButton(textBox);
        var provider = Provider(
            button,
            ControlSharedToken(ButtonTokens.Identity, nameof(DesignToken.ColorPrimary), "#00b96b"),
            ControlSharedToken(TextBoxTokens.Identity, nameof(DesignToken.ColorPrimary), "#722ed1"));

        ShowInWindow(provider, () =>
        {
            SetPrimary(button);
            textBox.Focus();
            FlushThemeUpdates();

            var innerBox = textBox.GetVisualDescendants()
                                  .OfType<PixelAlignedBorder>()
                                  .Single(item => item.Name == "InnerBoxDecorator");
            BrushColor(button.Background).ShouldBe(ButtonPrimary);
            BrushColor(innerBox.BorderBrush).ShouldBe(TextBoxPrimary);
        });
    }

    [Fact]
    public void Button_Control_Shared_Resource_Falls_Back_To_Updated_Global_Token()
    {
        var button = CreatePrimaryButton("Save");
        var provider = new ThemeConfigProvider
        {
            Child  = button,
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                     .Build()
        };
        ThrowOnCompileFailure(provider);

        ShowInWindow(provider, () =>
        {
            SetPrimary(button);
            BrushColor(button.Background).ShouldBe(ButtonPrimary);

            provider.Config = new ThemeConfigBuilder()
                              .WithToken(nameof(DesignToken.ColorPrimary), "#ff4d4f")
                              .Build();
            FlushThemeUpdates();

            BrushColor(button.Background).ShouldBe(UpdatedButtonPrimary);
        });
    }

    private static AtomUIButton CreatePrimaryButton(object? content)
    {
        return new AtomUIButton
        {
            IsMotionEnabled = false,
            Content         = content
        };
    }

    private static void SetPrimary(AtomUIButton button)
    {
        button.ButtonType = ButtonType.Primary;
        FlushThemeUpdates();
    }

    private static ThemeConfigProvider Provider(
        Control content,
        params ControlTokenConfig[] controlConfigs)
    {
        var provider = new ThemeConfigProvider
        {
            Child   = content,
            Config  = BuildControlConfig(controlConfigs)
        };
        ThrowOnCompileFailure(provider);
        return provider;
    }

    private static void ThrowOnCompileFailure(ThemeConfigProvider provider)
    {
        provider.ThemeChangeFailed += static (_, args) =>
        {
            var diagnostics = string.Join(
                Environment.NewLine,
                args.Diagnostics.Select(diagnostic => diagnostic.Message));
            throw new InvalidOperationException(args.Exception?.Message ?? diagnostics);
        };
    }

    private static ControlTokenConfig ControlSharedToken(
        ControlTokenIdentity identity,
        string key,
        string value)
    {
        return new ControlTokenConfig(identity, key, value);
    }

    private static ThemeConfig BuildControlConfig(params ControlTokenConfig[] controlConfigs)
    {
        var builder = new ThemeConfigBuilder();
        foreach (var control in controlConfigs)
        {
            builder.WithControl(
                control.Identity,
                new ControlThemeConfigBuilder()
                    .WithToken(control.Key, control.Value)
                    .Build());
        }

        return builder.Build();
    }

    private static void FlushThemeUpdates()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.RunJobs();
            return;
        }

        Dispatcher.UIThread.Invoke(static () => Dispatcher.UIThread.RunJobs());
    }

    private static Color BrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = content
        };

        try
        {
            window.Show();
            FlushThemeUpdates();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private sealed record ControlTokenConfig(ControlTokenIdentity Identity, string Key, string Value);
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ButtonThemeScopeTestCollection
{
    public const string Name = "ButtonThemeScope";
}
