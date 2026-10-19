using System.Reflection;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class ButtonBehaviorTests
{
    static ButtonBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Button_ApplyTemplate_Configures_WaveSpiritType_From_Current_Shape()
    {
        var button = new AtomUIButton
        {
            Shape           = ButtonShape.Circle,
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue(button, "WaveSpiritType")
                .ToString()
                .ShouldBe("CircleWave");
        });
    }

    [Fact]
    public void Button_ApplyTemplate_Configures_EffectiveBorderThickness_From_Current_Type()
    {
        var borderThickness = new Thickness(4);
        var button = new AtomUIButton
        {
            ButtonType      = ButtonType.Link,
            BorderThickness = borderThickness,
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Button_ButtonType_Change_Refreshes_EffectiveBorderThickness()
    {
        var borderThickness = new Thickness(3);
        var button = new AtomUIButton
        {
            ButtonType      = ButtonType.Link,
            BorderThickness = borderThickness,
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(new Thickness(0));

            button.ButtonType = ButtonType.Default;
            Dispatcher.UIThread.RunJobs();
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(borderThickness);

            button.ButtonType = ButtonType.Text;
            Dispatcher.UIThread.RunJobs();
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(new Thickness(0));
        });
    }

    [Theory]
    [InlineData(ButtonType.Default, false, "Default", "Outlined", true)]
    [InlineData(ButtonType.Dashed, false, "Default", "Dashed", true)]
    [InlineData(ButtonType.Primary, false, "Primary", "Solid", true)]
    [InlineData(ButtonType.Text, false, "Default", "Text", false)]
    [InlineData(ButtonType.Link, false, "Default", "Link", false)]
    [InlineData(ButtonType.Default, true, "Danger", "Outlined", true)]
    [InlineData(ButtonType.Primary, true, "Danger", "Solid", true)]
    [InlineData(ButtonType.Text, true, "Danger", "Text", false)]
    [InlineData(ButtonType.Link, true, "Danger", "Link", false)]
    public void Button_Compatibility_Apis_Configure_Effective_Color_And_Variant(
        ButtonType buttonType,
        bool isDanger,
        string expectedColor,
        string expectedVariant,
        bool expectedBordered)
    {
        var borderThickness = new Thickness(2);
        var button = new AtomUIButton
        {
            ButtonType      = buttonType,
            IsDanger        = isDanger,
            BorderThickness = borderThickness,
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue(button, "EffectiveColor")
                .ToString()
                .ShouldBe(expectedColor);
            GetInternalPropertyValue(button, "EffectiveVariant")
                .ToString()
                .ShouldBe(expectedVariant);
            GetInternalPropertyValue<bool>(button, "EffectiveIsBordered")
                .ShouldBe(expectedBordered);
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(expectedBordered ? borderThickness : new Thickness(0));
        });
    }

    [Theory]
    [InlineData("default")]
    [InlineData("primary")]
    [InlineData("danger")]
    public void Button_Text_Variant_Uses_Expected_Semantic_State_Colors(string scenario)
    {
        var button = CreateTextVariantButton(scenario);

        ShowInWindow(button, () =>
        {
            var expected = scenario switch
            {
                "default" => new TextVariantColors(
                    SharedTokenKind.ColorText,
                    SharedTokenKind.ColorText,
                    SharedTokenKind.ColorText,
                    SharedTokenKind.ColorFillTertiary,
                    SharedTokenKind.ColorFill),
                "primary" => new TextVariantColors(
                    SharedTokenKind.ColorPrimary,
                    SharedTokenKind.ColorPrimaryHover,
                    SharedTokenKind.ColorPrimaryActive,
                    SharedTokenKind.ColorPrimaryBg,
                    SharedTokenKind.ColorPrimaryBorder),
                "danger" => new TextVariantColors(
                    SharedTokenKind.ColorError,
                    SharedTokenKind.ColorErrorHover,
                    SharedTokenKind.ColorErrorActive,
                    SharedTokenKind.ColorErrorBg,
                    SharedTokenKind.ColorErrorBgActive),
                _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
            };

            BrushShouldHaveSameColor(button.Foreground, GetThemeResource<IBrush>(expected.Text));
            BrushShouldBeTransparent(button.Background);

            SetPseudoClass(button, StdPseudoClass.PointerOver, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(button.Foreground, GetThemeResource<IBrush>(expected.TextHover));
            BrushShouldHaveSameColor(button.Background, GetThemeResource<IBrush>(expected.BackgroundHover));

            SetPseudoClass(button, StdPseudoClass.Pressed, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(button.Foreground, GetThemeResource<IBrush>(expected.TextPressed));
            BrushShouldHaveSameColor(button.Background, GetThemeResource<IBrush>(expected.BackgroundPressed));
        });
    }

    [Theory]
    [InlineData(ButtonType.Default, false, false)]
    [InlineData(ButtonType.Dashed, false, false)]
    [InlineData(ButtonType.Primary, false, false)]
    [InlineData(ButtonType.Link, false, false)]
    [InlineData(ButtonType.Text, false, false)]
    [InlineData(ButtonType.Default, true, false)]
    [InlineData(ButtonType.Primary, true, false)]
    [InlineData(ButtonType.Text, true, false)]
    [InlineData(ButtonType.Default, false, true)]
    [InlineData(ButtonType.Primary, false, true)]
    public void Button_Compatibility_Visuals_Project_Effective_Variant_Colors(
        ButtonType buttonType,
        bool isDanger,
        bool isGhost)
    {
        var button = new AtomUIButton
        {
            ButtonType      = buttonType,
            IsDanger        = isDanger,
            IsGhost         = isGhost,
            Content         = "Button",
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            BrushShouldHaveSameColor(
                button.Foreground,
                GetInternalPropertyValue<IBrush?>(button, "VariantTextBrush"));
            BrushShouldHaveSameColor(
                button.Background,
                GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundBrush"));
            BrushShouldHaveSameColor(
                button.BorderBrush,
                GetInternalPropertyValue<IBrush?>(button, "VariantBorderBrush"));

            SetPseudoClass(button, StdPseudoClass.PointerOver, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(
                button.Foreground,
                GetInternalPropertyValue<IBrush?>(button, "VariantTextHoverBrush"));
            BrushShouldHaveSameColor(
                button.Background,
                GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundHoverBrush"));
            BrushShouldHaveSameColor(
                button.BorderBrush,
                GetInternalPropertyValue<IBrush?>(button, "VariantBorderHoverBrush"));

            SetPseudoClass(button, StdPseudoClass.Pressed, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(
                button.Foreground,
                GetInternalPropertyValue<IBrush?>(button, "VariantTextPressedBrush"));
            BrushShouldHaveSameColor(
                button.Background,
                GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundPressedBrush"));
            BrushShouldHaveSameColor(
                button.BorderBrush,
                GetInternalPropertyValue<IBrush?>(button, "VariantBorderPressedBrush"));
        });
    }

    [Fact]
    public void Button_Explicit_Color_And_Variant_Override_ButtonType_And_Danger()
    {
        var button = new AtomUIButton
        {
            ButtonType      = ButtonType.Primary,
            IsDanger        = true,
            Color           = ButtonColor.Pink,
            Variant         = ButtonVariant.Filled,
            BorderThickness = new Thickness(2),
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue<ButtonColor>(button, "EffectiveColor")
                .ShouldBe(ButtonColor.Pink);
            GetInternalPropertyValue<ButtonVariant>(button, "EffectiveVariant")
                .ShouldBe(ButtonVariant.Filled);
            GetInternalPropertyValue<bool>(button, "EffectiveIsBordered")
                .ShouldBeFalse();
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Button_IconPlacement_Defaults_To_Start_And_Can_Move_Icon_To_End()
    {
        var iconPlacementProperty = typeof(AtomUIButton).GetProperty(
            "IconPlacement",
            BindingFlags.Instance | BindingFlags.Public);
        iconPlacementProperty.ShouldNotBeNull();
        iconPlacementProperty!.PropertyType.IsEnum.ShouldBeTrue();

        var startButton = CreateIconPlacementButton();
        var endButton   = CreateIconPlacementButton();
        var endValue    = Enum.Parse(iconPlacementProperty.PropertyType, "End");
        iconPlacementProperty.SetValue(endButton, endValue);

        var host = new StackPanel
        {
            Children =
            {
                startButton,
                endButton
            }
        };

        ShowInWindow(host, () =>
        {
            iconPlacementProperty.GetValue(startButton)!.ToString().ShouldBe("Start");

            var startIcon = FindTemplateIcon(startButton);
            var endIcon   = FindTemplateIcon(endButton);

            DockPanel.GetDock(startIcon).ShouldBe(Dock.Left);
            DockPanel.GetDock(endIcon).ShouldBe(Dock.Right);
            endIcon.Margin.Left.ShouldBe(startIcon.Margin.Right);
            endIcon.Margin.Right.ShouldBe(startIcon.Margin.Left);
        });
    }

    [Fact]
    public void Button_Solid_Variant_Without_Color_Uses_Primary_Color()
    {
        var button = new AtomUIButton
        {
            Variant         = ButtonVariant.Solid,
            BorderThickness = new Thickness(2),
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue<ButtonColor>(button, "EffectiveColor")
                .ShouldBe(ButtonColor.Primary);
            GetInternalPropertyValue<ButtonVariant>(button, "EffectiveVariant")
                .ShouldBe(ButtonVariant.Solid);
            GetInternalPropertyValue<bool>(button, "EffectiveIsBordered")
                .ShouldBeTrue();
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(button.BorderThickness);
        });
    }

    [Fact]
    public void Button_Default_Color_And_Solid_Variant_Uses_Default_Solid_Surface()
    {
        var button = new AtomUIButton
        {
            Color           = ButtonColor.Default,
            Variant         = ButtonVariant.Solid,
            BorderThickness = new Thickness(2),
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            button.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
            GetInternalPropertyValue<ButtonColor>(button, "EffectiveColor")
                .ShouldBe(ButtonColor.Default);
            GetInternalPropertyValue<ButtonVariant>(button, "EffectiveVariant")
                .ShouldBe(ButtonVariant.Solid);
            GetInternalPropertyValue<bool>(button, "EffectiveIsBordered")
                .ShouldBeTrue();
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(button.BorderThickness);
            BrushShouldHaveSameColor(button.Foreground, GetInternalPropertyValue<IBrush?>(button, "VariantTextBrush"));
            BrushShouldHaveSameColor(button.Background, GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundBrush"));

            var foreground = GetSolidBrushColor(button.Foreground);
            var background = GetSolidBrushColor(button.Background);
            background.A.ShouldBe((byte)255);
            foreground.ShouldNotBe(background);
        });
    }

    [Theory]
    [InlineData("button-type-primary")]
    [InlineData("explicit-primary-solid")]
    public void Button_Primary_Variant_Background_Follows_Scoped_ColorPrimary(
        string scenario)
    {
        var scopedPrimaryColor = Color.Parse("#00A1D6");
        var button             = CreateScopedPrimaryButton(scenario);
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithToken("ColorPrimary", "#00A1D6")
                     .Build(),
            Child = button
        };

        ShowInWindow(provider, () =>
        {
            BrushShouldHaveColor(button.Background, scopedPrimaryColor);
            BrushShouldHaveColor(GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundBrush"),
                scopedPrimaryColor);
            BrushShouldHaveSameColor(button.Background,
                GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundBrush"));
        });
    }

    [Fact]
    public void Button_Ghost_Solid_Variant_Becomes_Outlined()
    {
        var button = new AtomUIButton
        {
            Color           = ButtonColor.Primary,
            Variant         = ButtonVariant.Solid,
            IsGhost         = true,
            BorderThickness = new Thickness(2),
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            GetInternalPropertyValue<ButtonColor>(button, "EffectiveColor")
                .ShouldBe(ButtonColor.Primary);
            GetInternalPropertyValue<ButtonVariant>(button, "EffectiveVariant")
                .ShouldBe(ButtonVariant.Outlined);
            GetInternalPropertyValue<bool>(button, "EffectiveIsBordered")
                .ShouldBeTrue();
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(button.BorderThickness);
        });
    }

    [Theory]
    [InlineData(ButtonVariant.Text)]
    [InlineData(ButtonVariant.Link)]
    public void Button_Text_And_Link_Variants_Keep_Normal_Surface_Transparent_And_Shadowless(
        ButtonVariant variant)
    {
        var button = new AtomUIButton
        {
            Color           = ButtonColor.Pink,
            Variant         = variant,
            BorderThickness = new Thickness(2),
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            BrushShouldBeTransparent(button.Background);
            BrushShouldBeTransparent(GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundBrush"));
            GetInternalPropertyValue<BoxShadows>(button, "VariantShadow")
                .ShouldBe(new BoxShadows());
            GetInternalPropertyValue<Thickness>(button, "EffectiveBorderThickness")
                .ShouldBe(new Thickness(0));

            var shadowFrame = FindTemplateBorder(button, "ShadowsFrame");
            BrushShouldBeTransparent(shadowFrame.Background);
            shadowFrame.BoxShadow.ShouldBe(new BoxShadows());
        });
    }

    [Fact]
    public void Button_Root_Background_Renders_On_Frame_And_Survives_Interaction_States()
    {
        var customBackground = CreateGradientBrush();
        var button = new AtomUIButton
        {
            ButtonType      = ButtonType.Primary,
            Background      = customBackground,
            IsMotionEnabled = false
        };

        ShowInWindow(button, () =>
        {
            var frame = FindTemplateDashedBorder(button, "Frame");
            frame.Background.ShouldBeSameAs(customBackground,
                "The root Background set on the Button must render directly on the frame.");

            // A customized root background intentionally freezes the hover/pressed/disabled
            // color changes of that slot, matching antd inline style semantics.
            SetPseudoClass(button, StdPseudoClass.PointerOver, true);
            Dispatcher.UIThread.RunJobs();
            frame.Background.ShouldBeSameAs(customBackground);

            SetPseudoClass(button, StdPseudoClass.Pressed, true);
            Dispatcher.UIThread.RunJobs();
            frame.Background.ShouldBeSameAs(customBackground);

            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            frame.Background.ShouldBeSameAs(customBackground);

            button.IsEnabled  = true;
            button.ClearValue(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty);
            SetPseudoClass(button, StdPseudoClass.PointerOver, false);
            SetPseudoClass(button, StdPseudoClass.Pressed, false);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(
                frame.Background,
                GetInternalPropertyValue<IBrush?>(button, "VariantBackgroundBrush"),
                "Clearing the root background must restore the theme variant state machine.");
        });
    }

    [Fact]
    public void Button_Surface_Does_Not_Expose_A_Parallel_CustomBackground_Channel()
    {
        foreach (var path in new[]
                 {
                     "src/AtomUI.Desktop.Controls/Buttons/Button.cs",
                     "src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml",
                     "src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonTheme.axaml",
                     "src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonBaseTheme.axaml"
                 })
        {
            ReadRepoFile(path).ShouldNotContain("CustomBackground",
                customMessage: $"{path} must use the standard Background surface channel.");
        }
    }

    [Theory]
    [InlineData(ButtonColor.Pink, ButtonVariant.Solid, false)]
    [InlineData(ButtonColor.Pink, ButtonVariant.Outlined, true)]
    [InlineData(ButtonColor.Cyan, ButtonVariant.Dashed, true)]
    public void Button_WaveSpiritBrush_Follows_Final_Root_Visual_Color(
        ButtonColor color,
        ButtonVariant variant,
        bool useBorderBrush)
    {
        var button = new AtomUIButton
        {
            Color           = color,
            Variant         = variant,
            Width           = 120,
            Height          = 40,
            IsMotionEnabled = true
        };

        ShowInWindow(button, window =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");
            button.DisableTransitions();

            Click(button, window);

            var waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            var expectedBrush = useBorderBrush ? button.BorderBrush : button.Background;
            BrushShouldHaveSameColor(waveBrush, expectedBrush);
        });
    }

    [Fact]
    public void Button_WaveSpiritBrush_Uses_Final_Visual_Color_When_Wave_Is_Triggered()
    {
        var semanticBorder = new SolidColorBrush(Color.Parse("#D9D9D9"));
        var semanticBackground = new SolidColorBrush(Color.Parse("#171717"));
        var button = new AtomUIButton
        {
            Width               = 120,
            Height              = 40,
            ButtonType          = ButtonType.Primary,
            Content             = "Semantic Button",
            BorderBrush         = semanticBorder,
            Background          = semanticBackground,
            IsMotionEnabled     = true,
            IsWaveSpiritEnabled = true
        };

        ShowInWindow(button, window =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");

            Click(button, window);

            var waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            BrushShouldHaveSameColor(waveBrush, semanticBorder);

            var updatedBorder = new SolidColorBrush(Color.Parse("#13C2C2"));
            button.DisableTransitions();
            button.BorderBrush = updatedBorder;
            Dispatcher.UIThread.RunJobs();

            Click(button, window);

            waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            BrushShouldHaveSameColor(waveBrush, updatedBorder);
        });
    }

    [Theory]
    [InlineData("transparent")]
    [InlineData("white")]
    public void Button_WaveSpiritBrush_Falls_Back_From_Invalid_Border_To_Final_Background(string border)
    {
        var background = new SolidColorBrush(Color.Parse("#171717"));
        var button = new AtomUIButton
        {
            Width               = 120,
            Height              = 40,
            BorderBrush         = border == "transparent" ? Brushes.Transparent : Brushes.White,
            Background          = background,
            IsMotionEnabled     = true,
            IsWaveSpiritEnabled = true
        };

        ShowInWindow(button, window =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");

            Click(button, window);

            var waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            BrushShouldHaveSameColor(waveBrush, background);
        });
    }

    [Fact]
    public void Button_WaveSpiritBrush_Uses_Theme_Default_When_Root_Has_No_Valid_Solid_Color()
    {
        var button = new AtomUIButton
        {
            Width               = 120,
            Height              = 40,
            BorderBrush         = CreateGradientBrush(),
            Background          = Brushes.Transparent,
            IsMotionEnabled     = true,
            IsWaveSpiritEnabled = true
        };

        ShowInWindow(button, window =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");

            Click(button, window);

            var waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            BrushShouldHaveSameColor(
                waveBrush,
                GetThemeResource<IBrush>(SharedTokenKind.ColorPrimary));
        });
    }

    [Fact]
    public void Button_Custom_Root_Background_Is_A_WaveSpiritBrush_Source()
    {
        var customBackground = new SolidColorBrush(Color.Parse("#13C2C2"));
        var button = new AtomUIButton
        {
            ButtonType         = ButtonType.Primary,
            BorderBrush        = Brushes.Transparent,
            Background         = customBackground,
            Width              = 120,
            Height             = 40,
            IsMotionEnabled    = true,
            IsWaveSpiritEnabled = true
        };

        ShowInWindow(button, window =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");
            button.DisableTransitions();

            Click(button, window);

            var waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            BrushShouldHaveSameColor(waveBrush, customBackground);
        });
    }

    [Fact]
    public void Button_WaveRange_Update_Reconfigures_Cached_WavePainter()
    {
        var button = new AtomUIButton
        {
            Width           = 120,
            Height          = 36,
            ButtonType      = ButtonType.Primary,
            Content         = "Save",
            IsMotionEnabled = true
        };

        ShowInWindow(button, () =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");
            SetPublicPropertyValue(waveSpiritDecorator, "SizeMotionDuration", TimeSpan.FromSeconds(1));
            SetPublicPropertyValue(waveSpiritDecorator, "OpacityMotionDuration", TimeSpan.FromSeconds(1));
            SetPublicPropertyValue(waveSpiritDecorator, "WaveRange", 0d);

            InvokePublicMethod(waveSpiritDecorator, "Play");
            var wavePainter = GetPrivateFieldValue(waveSpiritDecorator, "_wavePainter");
            GetPublicPropertyValue<double>(wavePainter, "WaveRange").ShouldBe(0d);

            SetPublicPropertyValue(waveSpiritDecorator, "WaveRange", 6d);

            GetPublicPropertyValue<double>(wavePainter, "WaveRange").ShouldBe(6d);
        });
    }

    [Theory]
    [InlineData("motion")]
    [InlineData("wave-spirit")]
    public void Button_Disabling_Wave_Gates_Cancels_Active_Wave(string gate)
    {
        var button = new AtomUIButton
        {
            Width                = 120,
            Height               = 36,
            ButtonType           = ButtonType.Primary,
            Content              = "Save",
            IsMotionEnabled      = true,
            IsWaveSpiritEnabled  = true
        };

        ShowInWindow(button, () =>
        {
            var waveSpiritDecorator = GetPrivateFieldValue(button, "_waveSpiritDecorator");
            SetPublicPropertyValue(waveSpiritDecorator, "SizeMotionDuration", TimeSpan.FromSeconds(1));
            SetPublicPropertyValue(waveSpiritDecorator, "OpacityMotionDuration", TimeSpan.FromSeconds(1));

            InvokePublicMethod(waveSpiritDecorator, "Play");
            GetInternalPropertyValue<bool>(waveSpiritDecorator, "IsPlaying").ShouldBeTrue();

            if (gate == "motion")
            {
                button.IsMotionEnabled = false;
            }
            else
            {
                button.IsWaveSpiritEnabled = false;
            }
            Dispatcher.UIThread.RunJobs();

            GetInternalPropertyValue<bool>(waveSpiritDecorator, "IsPlaying").ShouldBeFalse();
        });
    }

    [Fact]
    public void Button_Default_Frame_Uses_AtomUI_Border_Rendering()
    {
        var button = new AtomUIButton
        {
            ButtonType      = ButtonType.Default,
            BorderThickness = new Thickness(1),
            BorderBrush     = Brushes.Black,
            IsMotionEnabled = false
        };

        ShowInWindow(button, window =>
        {
            window.SetRenderScaling(1.5);
            Dispatcher.UIThread.RunJobs();

            var frame = FindTemplateDashedBorder(button, "Frame");
            RenderToDrawingGroup(frame);

            GetBorderRenderHelperThickness(frame).ShouldBe(new Thickness(2d / 3d));
        });
    }

    private static AtomUIButton CreateTextVariantButton(string scenario)
    {
        var button = new AtomUIButton
        {
            Content         = "Text",
            IsMotionEnabled = false
        };

        switch (scenario)
        {
            case "default":
                button.ButtonType = ButtonType.Text;
                break;
            case "primary":
                button.Color   = ButtonColor.Primary;
                button.Variant = ButtonVariant.Text;
                break;
            case "danger":
                button.ButtonType = ButtonType.Text;
                button.IsDanger   = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        }

        return button;
    }

    private static AtomUIButton CreateScopedPrimaryButton(string scenario)
    {
        var button = new AtomUIButton
        {
            IsMotionEnabled = false
        };

        switch (scenario)
        {
            case "button-type-primary":
                button.ButtonType = ButtonType.Primary;
                break;
            case "explicit-primary-solid":
                button.Color   = ButtonColor.Primary;
                button.Variant = ButtonVariant.Solid;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        }

        return button;
    }

    private static LinearGradientBrush CreateGradientBrush()
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop { Color = Colors.MediumPurple, Offset = 0 },
                new GradientStop { Color = Colors.DeepSkyBlue, Offset   = 1 }
            }
        };
    }

    private static AtomUIButton CreateIconPlacementButton()
    {
        return new AtomUIButton
        {
            Content         = "Search",
            Icon            = new PathIcon { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") },
            IsMotionEnabled = false
        };
    }

    private static object GetInternalPropertyValue(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        var value = property.GetValue(target);
        value.ShouldNotBeNull();
        return value;
    }

    private static T GetInternalPropertyValue<T>(object target, string propertyName)
    {
        return (T)GetInternalPropertyValue(target, propertyName);
    }

    private static object GetPrivateFieldValue(object target, string fieldName)
    {
        var field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var value = field.GetValue(target);
        value.ShouldNotBeNull();
        return value;
    }

    private static T GetPublicPropertyValue<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        property.ShouldNotBeNull();
        return (T)property.GetValue(target)!;
    }

    private static void SetPublicPropertyValue<T>(object target, string propertyName, T value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        property.ShouldNotBeNull();
        property.SetValue(target, value);
    }

    private static void InvokePublicMethod(object target, string methodName)
    {
        var method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public);
        method.ShouldNotBeNull();
        method.Invoke(target, null);
    }

    private static void Click(Control control, AvaloniaWindow window)
    {
        var point = GetControlCenter(control, window);

        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static Point GetControlCenter(Control control, AvaloniaWindow window)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();
        return point.Value;
    }

    private static Border FindTemplateBorder(Control control, string name)
    {
        var border = control.GetVisualDescendants()
                            .OfType<Border>()
                            .SingleOrDefault(item => item.Name == name);
        border.ShouldNotBeNull();
        return border;
    }

    private static DashedBorder FindTemplateDashedBorder(Control control, string name)
    {
        var border = control.GetVisualDescendants()
                            .OfType<DashedBorder>()
                            .SingleOrDefault(item => item.Name == name);
        border.ShouldNotBeNull();
        return border!;
    }

    private static DrawingGroup RenderToDrawingGroup(Control control)
    {
        var drawingGroup = new DrawingGroup();
        using (var context = drawingGroup.Open())
        {
            control.Render(context);
        }

        return drawingGroup;
    }

    private static Thickness GetBorderRenderHelperThickness(DashedBorder border)
    {
        var helperField = typeof(DashedBorder).GetField(
            "_borderRenderHelper",
            BindingFlags.Instance | BindingFlags.NonPublic);
        helperField.ShouldNotBeNull();

        var helper = helperField!.GetValue(border);
        helper.ShouldNotBeNull();

        var thicknessField = helper!.GetType().GetField(
            "_borderThickness",
            BindingFlags.Instance | BindingFlags.NonPublic);
        thicknessField.ShouldNotBeNull();

        return (Thickness)thicknessField!.GetValue(helper)!;
    }

    private static IconPresenter FindTemplateIcon(Control control)
    {
        var icon = control.GetVisualDescendants()
                          .OfType<IconPresenter>()
                          .SingleOrDefault(item => item.Name == "PART_ButtonIcon");
        icon.ShouldNotBeNull();
        return icon!;
    }

    private static void BrushShouldBeTransparent(IBrush? brush)
    {
        if (brush is null)
        {
            return;
        }

        GetSolidBrushColor(brush).A.ShouldBe((byte)0);
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected, string? message = null)
    {
        GetSolidBrushColor(actual).ShouldBe(GetSolidBrushColor(expected), message);
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }

    private static void BrushShouldHaveColor(IBrush? actual, Color expected)
    {
        GetSolidBrushColor(actual).ShouldBe(expected);
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void SetPseudoClass(Control control, string pseudoClass, bool value)
    {
        ((IPseudoClasses)control.Classes).Set(pseudoClass, value);
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, _ => assertion());
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
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
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed record TextVariantColors(
        SharedTokenKind Text,
        SharedTokenKind TextHover,
        SharedTokenKind TextPressed,
        SharedTokenKind BackgroundHover,
        SharedTokenKind BackgroundPressed);
}
