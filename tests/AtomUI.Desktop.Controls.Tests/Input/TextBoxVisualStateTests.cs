using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AtomUIEmbeddedTextBox = AtomUI.Desktop.Controls.EmbeddedTextBox;
using AtomUISearchEdit = AtomUI.Desktop.Controls.SearchEdit;
using AtomUITextArea = AtomUI.Desktop.Controls.TextArea;
using AtomUITextBox = AtomUI.Desktop.Controls.TextBox;
using AtomUIScrollViewer = AtomUI.Desktop.Controls.ScrollViewer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class TextBoxVisualStateTests
{
    static TextBoxVisualStateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Disabled_TextBox_Uses_Disabled_Text_Foreground()
    {
        var textBox = new AtomUITextBox
        {
            Width           = 160,
            Text            = "3",
            IsEnabled       = false,
            IsMotionEnabled = false
        };

        ShowInWindow(textBox, () =>
        {
            var scrollViewer = textBox.GetVisualDescendants()
                                      .OfType<AtomUIScrollViewer>()
                                      .Single(item => item.Name == "ScrollViewer");

            BrushShouldHaveSameColor(
                scrollViewer.Foreground,
                GetThemeResource<IBrush>(SharedTokenKind.ColorTextDisabled));
        });
    }

    [Fact]
    public void TextBox_InnerBoxDecorator_Uses_Shared_Input_Frame_Tokens_For_Default_Hover_And_Focus()
    {
        var textBox = new AtomUITextBox
        {
            Width           = 180,
            IsMotionEnabled = false
        };

        ShowInWindow(textBox, () =>
        {
            var frame = FindTemplatePart<InputControlFrame>(textBox, "PART_InputControlFrame");
            var border = FindTemplatePart<PixelAlignedBorder>(textBox, "InnerBoxDecorator");
            var expectedThickness = GetThemeResource<Thickness>(SharedTokenKind.BorderThickness);
            var defaultBorder     = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var hoverBorder       = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimaryHover);
            var activeBorder      = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimary);
            var activeShadow      = GetThemeResource<BoxShadows>(SharedTokenKind.InputActiveShadow);

            textBox.BorderThickness.ShouldBe(expectedThickness);
            border.BorderThickness.ShouldBe(expectedThickness);
            BrushShouldHaveSameColor(border.BorderBrush, defaultBorder);

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();

            border.BorderThickness.ShouldBe(expectedThickness);
            BrushShouldHaveSameColor(border.BorderBrush, hoverBorder);

            textBox.Focus();
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue();
            frame.IsInputFocusWithin.ShouldBeTrue();
            border.BorderThickness.ShouldBe(expectedThickness);
            BrushShouldHaveSameColor(border.BorderBrush, activeBorder);
            border.BoxShadow.ShouldBe(activeShadow);
        });
    }

    [Fact]
    public void TextBox_Input_Frame_Width_Remains_Stable_When_Text_Changes()
    {
        var textBox = new AtomUITextBox
        {
            Width           = 360,
            PlaceholderText = "AtomUI TextBox",
            IsMotionEnabled = false
        };

        ShowInWindow(textBox, () =>
        {
            var frame = FindTemplatePart<InputControlFrame>(textBox, "PART_InputControlFrame");

            frame.Bounds.Width.ShouldBe(textBox.Bounds.Width);

            textBox.Text = "2";
            Dispatcher.UIThread.RunJobs();
            frame.Bounds.Width.ShouldBe(textBox.Bounds.Width);

            textBox.Text = "2wewwerwewerwerwe";
            Dispatcher.UIThread.RunJobs();
            frame.Bounds.Width.ShouldBe(textBox.Bounds.Width);
        }, window => window.Width = 480);
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Global, true)]
    [InlineData(ControlAlgorithmMode.Disabled, false)]
    public void LineEdit_Hover_Uses_Control_Effective_ColorPrimaryHover(
        ControlAlgorithmMode algorithm,
        bool expectDerivedHover)
    {
        AssertCompositeInputHoverUsesControlEffectiveColorPrimaryHover(
            new AtomUILineEdit
            {
                Width           = 180,
                IsMotionEnabled = false
            },
            LineEditTokens.Identity,
            algorithm,
            expectDerivedHover);
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Global)]
    [InlineData(ControlAlgorithmMode.Disabled)]
    public void LineEdit_FocusWithin_Uses_Control_Effective_ColorPrimary(ControlAlgorithmMode algorithm)
    {
        AssertCompositeInputFocusWithinUsesControlEffectiveColorPrimary(
            new AtomUILineEdit
            {
                Width           = 180,
                IsMotionEnabled = false
            },
            LineEditTokens.Identity,
            algorithm);
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Global, true)]
    [InlineData(ControlAlgorithmMode.Disabled, false)]
    public void SearchEdit_Hover_Uses_Control_Effective_ColorPrimaryHover(
        ControlAlgorithmMode algorithm,
        bool expectDerivedHover)
    {
        AssertCompositeInputHoverUsesControlEffectiveColorPrimaryHover(
            new AtomUISearchEdit
            {
                Width           = 180,
                IsMotionEnabled = false
            },
            SearchEditTokens.Identity,
            algorithm,
            expectDerivedHover);
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Global)]
    [InlineData(ControlAlgorithmMode.Disabled)]
    public void SearchEdit_FocusWithin_Uses_Control_Effective_ColorPrimary(ControlAlgorithmMode algorithm)
    {
        AssertCompositeInputFocusWithinUsesControlEffectiveColorPrimary(
            new AtomUISearchEdit
            {
                Width           = 180,
                IsMotionEnabled = false
            },
            SearchEditTokens.Identity,
            algorithm);
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Global, true)]
    [InlineData(ControlAlgorithmMode.Disabled, false)]
    public void TextArea_Hover_Uses_Control_Effective_ColorPrimaryHover(
        ControlAlgorithmMode algorithm,
        bool expectDerivedHover)
    {
        AssertCompositeInputHoverUsesControlEffectiveColorPrimaryHover(
            new AtomUITextArea
            {
                Width           = 180,
                Height          = 80,
                IsMotionEnabled = false
            },
            TextAreaTokens.Identity,
            algorithm,
            expectDerivedHover);
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Global)]
    [InlineData(ControlAlgorithmMode.Disabled)]
    public void TextArea_FocusWithin_Uses_Control_Effective_ColorPrimary(ControlAlgorithmMode algorithm)
    {
        AssertCompositeInputFocusWithinUsesControlEffectiveColorPrimary(
            new AtomUITextArea
            {
                Width           = 180,
                Height          = 80,
                IsMotionEnabled = false
            },
            TextAreaTokens.Identity,
            algorithm);
    }

    [Fact]
    public void TextBox_Uses_Customizable_SizeType_Contract_And_Control_Shared_Token_Resources()
    {
        typeof(ICustomizableSizeTypeAware).IsAssignableFrom(typeof(AtomUITextBox)).ShouldBeTrue();

        var property = typeof(AtomUITextBox).GetProperty(
            "SizeType",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        property.ShouldNotBeNull();
        property!.PropertyType.ShouldBe(typeof(CustomizableSizeType));

        var field = typeof(AtomUITextBox).GetField(
            "SizeTypeProperty",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        field.ShouldNotBeNull();
        field!.FieldType.ShouldBe(typeof(StyledProperty<CustomizableSizeType>));

        TextBoxTokens.Identity.Id.ShouldBe(nameof(AtomUI.Desktop.Controls.TextBox));

        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Input/Themes/TextBoxTheme.axaml");
        source.ShouldContain("TextBoxTokenResource FontSize");
        source.ShouldNotContain("ControlTokenScope.Identity");
        source.ShouldNotContain("TokenSharedTokenResource");
    }

    [Fact]
    public void EmbeddedTextBox_Is_Internal_TextBox_For_Template_Owned_Input_Chrome()
    {
        typeof(AtomUITextBox).IsAssignableFrom(typeof(AtomUIEmbeddedTextBox)).ShouldBeTrue();
        typeof(ICustomizableSizeTypeAware).IsAssignableFrom(typeof(AtomUIEmbeddedTextBox)).ShouldBeTrue();

        var embeddedTextBox = new AtomUIEmbeddedTextBox
        {
            Width     = 160,
            Text      = "embedded",
            IsEnabled = false
        };

        ShowInWindow(embeddedTextBox, () =>
        {
            embeddedTextBox.SizeType.ShouldBe(CustomizableSizeType.Custom);
            embeddedTextBox.Padding.ShouldBe(new Thickness(0));
            embeddedTextBox.BorderThickness.ShouldBe(new Thickness(0));
            embeddedTextBox.IsCustomFontSize.ShouldBeTrue();
            embeddedTextBox.IsAllowClear.ShouldBeFalse();
            embeddedTextBox.IsEnableRevealButton.ShouldBeFalse();

            var border = FindTemplatePart<PixelAlignedBorder>(embeddedTextBox, "InnerBoxDecorator");
            border.BorderThickness.ShouldBe(new Thickness(0));
            BrushShouldHaveSameColor(border.Background, Brushes.Transparent);
            BrushShouldHaveSameColor(border.BorderBrush, Brushes.Transparent);
        });
    }

    [Fact]
    public void EmbeddedTextBox_Theme_Suppresses_Own_Disabled_Chrome()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Input/Themes/EmbeddedTextBoxTheme.axaml");

        source.ShouldContain("TargetType=\"atom:EmbeddedTextBox\"");
        source.ShouldContain("<Setter Property=\"Background\" Value=\"Transparent\" />");
        source.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"Transparent\" />");
        source.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"0\" />");
        source.ShouldContain("<Setter Property=\"Padding\" Value=\"0\" />");
        source.ShouldContain("<Style Selector=\"^:disabled /template/ atom|PixelAlignedBorder#InnerBoxDecorator\">");
        source.ShouldContain("<Setter Property=\"Background\" Value=\"Transparent\" />");
        source.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"Transparent\" />");
        source.ShouldNotContain("ColorBgContainerDisabled");
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml")]
    public void TemplateOwned_Input_Chrome_Uses_EmbeddedTextBox(string relativePath)
    {
        var source = ReadRepoFile(relativePath);

        var expectedInputType = relativePath.Contains("InfoPicker", StringComparison.OrdinalIgnoreCase) ||
                                relativePath.Contains("RangeDatePicker", StringComparison.OrdinalIgnoreCase)
            ? "<atom:InfoPickerTextBox"
            : "<atom:EmbeddedTextBox";

        source.ShouldContain(expectedInputType);
        source.ShouldNotContain("<atom:TextBox");
    }

    [Fact]
    public void TextBox_Theme_Uses_TextBox_Token_Resources()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Input/Themes/TextBoxTheme.axaml");

        // The owner level must stay free of dead surface defaults: the frame theme owns
        // the rest state and the owner value is the relay's customization signal.
        source.ShouldNotContain("TextBoxTokenResource ColorBorder");
        source.ShouldContain("TextBoxTokenResource BorderThickness");
        source.ShouldContain("TextBoxTokenResource BorderRadiusLG");
        source.ShouldContain("TextBoxTokenResource BorderRadius");
        source.ShouldContain("TextBoxTokenResource BorderRadiusSM");
        source.ShouldContain("TextBoxTokenResource ContentPaddingLG");
        source.ShouldContain("TextBoxTokenResource ContentPadding");
        source.ShouldContain("TextBoxTokenResource ContentPaddingSM");
        source.ShouldContain("TextBoxTokenResource EnableMotion");
        source.ShouldNotContain("IsCustomPadding");
        source.ShouldContain("TextBoxTokenResource SpacingXXS");
        source.ShouldContain("TextBoxTokenResource FontHeightLG");
        source.ShouldContain("TextBoxTokenResource FontHeight");
        source.ShouldContain("TextBoxTokenResource FontHeightSM");
        source.ShouldContain("TextBoxTokenResource FontSizeLG");
        source.ShouldContain("TextBoxTokenResource FontSize");
        source.ShouldContain("TextBoxTokenResource FontSizeSM");
        source.ShouldContain("TextBoxTokenResource ColorTextPlaceholder");
        var frameTheme = ReadRepoFile(
            "src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml");
        frameTheme.ShouldContain("OwnerTokenResource ColorTextDisabled");
        source.ShouldNotContain("ControlTokenScope.Identity");
        source.ShouldNotContain("LineEditTokenResource");
        source.ShouldNotContain("AddOnDecoratedBoxTokenResource");
        source.ShouldNotContain("AddOn");
        source.ShouldNotContain("TextBoxTokenResource LineHeight");
        source.ShouldNotContain("TextBoxTokenResource InputFontSize");
        source.ShouldNotContain("TextBoxTokenResource PlaceholderColor");
        source.ShouldNotContain("TextBoxTokenResource ContentMargin");
        source.ShouldNotContain("TextBoxTokenResource DisabledColor");
        source.ShouldNotContain("TextBoxTokenResource AddOnSpacing");
        source.ShouldNotContain("PART_LeftInnerContentLayout");
        source.ShouldNotContain("PART_RightInnerContentLayout");
        source.ShouldNotContain("PART_LeftInnerContent");
        source.ShouldNotContain("PART_RightInnerContent");
        source.ShouldContain("<atom:InputClearIconButton");
        source.ShouldContain("Name=\"PART_ClearButton\"");
        source.ShouldContain("<atom:RevealButton");
        source.ShouldContain("Name=\"PART_RevealButton\"");
    }

    [Theory]
    [InlineData(CustomizableSizeType.Large, "ContentPaddingLG")]
    [InlineData(CustomizableSizeType.Middle, "ContentPadding")]
    [InlineData(CustomizableSizeType.Small, "ContentPaddingSM")]
    public void TextBox_Padding_Follows_TextBox_Size_Tokens(CustomizableSizeType sizeType, string tokenKind)
    {
        var textBox = new AtomUITextBox
        {
            Width           = 180,
            SizeType        = sizeType,
            IsMotionEnabled = false
        };

        ShowInWindow(textBox, () =>
        {
            textBox.Padding.ShouldBe(GetTextBoxTokenResource<Thickness>(tokenKind));
        });
    }

    [Theory]
    [InlineData(CustomizableSizeType.Large, nameof(SharedTokenKind.ControlHeightLG), double.NaN, null)]
    [InlineData(CustomizableSizeType.Middle, nameof(SharedTokenKind.ControlHeight), double.NaN, null)]
    [InlineData(CustomizableSizeType.Small, nameof(SharedTokenKind.ControlHeightSM), double.NaN, null)]
    [InlineData(CustomizableSizeType.Custom, null, 42.0, "12,0")]
    public void TextBox_Frame_Fills_SizeType_And_Custom_Control_Height(
        CustomizableSizeType sizeType,
        string? tokenKindName,
        double height,
        string? paddingText)
    {
        var textBox = new AtomUITextBox
        {
            Width           = 360,
            SizeType        = sizeType,
            Height          = height,
            Text            = sizeType.ToString(),
            IsMotionEnabled = false
        };
        if (paddingText is not null)
        {
            textBox.Padding = Thickness.Parse(paddingText);
        }

        var expectedHeight = tokenKindName is null
            ? height
            : GetThemeResource<double>((SharedTokenKind)Enum.Parse(typeof(SharedTokenKind), tokenKindName));

        ShowInWindow(textBox, () =>
        {
            var frame = FindTemplatePart<InputControlFrame>(textBox, "PART_InputControlFrame");

            textBox.Bounds.Height.ShouldBe(expectedHeight, 0.5);
            frame.Bounds.Height.ShouldBe(textBox.Bounds.Height, 0.5);
        }, window => window.Height = 180);
    }

    [Fact]
    public void TextBox_Custom_Size_Does_Not_Apply_TextBox_Padding_Token()
    {
        var textBox = new AtomUITextBox
        {
            Width           = 180,
            SizeType        = CustomizableSizeType.Custom,
            IsMotionEnabled = false
        };

        ShowInWindow(textBox, () =>
        {
            textBox.Padding.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void TextBox_Default_Motion_State_Follows_EnableMotion_Resource()
    {
        var textBox = new AtomUITextBox
        {
            Width = 180
        };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                             .WithToken(nameof(SharedTokenKind.EnableMotion), "false")
                     .Build(),
            Child = textBox
        };

        ShowInWindow(provider, () =>
        {
            var border = FindTemplatePart<PixelAlignedBorder>(textBox, "InnerBoxDecorator");

            textBox.IsMotionEnabled.ShouldBeFalse();
            border.Transitions.ShouldBeNull();
        });
    }

    [Fact]
    public void TextBox_InnerBoxDecorator_Transitions_Animate_Background_Border_And_Shadow()
    {
        var textBox = new AtomUITextBox
        {
            Width           = 180,
            IsMotionEnabled = true
        };

        ShowInWindow(textBox, () =>
        {
            var border = FindTemplatePart<PixelAlignedBorder>(textBox, "InnerBoxDecorator");
            var transitionProperties = border.Transitions.ShouldNotBeNull()
                                             .Select(transition => transition.Property)
                                             .ToArray();

            transitionProperties.ShouldContain(PixelAlignedBorder.BackgroundProperty);
            transitionProperties.ShouldContain(PixelAlignedBorder.BorderBrushProperty);
            transitionProperties.ShouldContain(PixelAlignedBorder.BoxShadowProperty);
        });
    }

    [Fact]
    public void TextBox_Clear_And_Reveal_Buttons_Use_LineEdit_Default_Icons()
    {
        var textBox = new AtomUITextBox
        {
            Width                = 180,
            Text                 = "secret",
            PasswordChar         = '*',
            IsAllowClear         = true,
            IsEnableRevealButton = true,
            IsMotionEnabled      = false
        };
        var lineEdit = new AtomUILineEdit
        {
            Width                = 180,
            Text                 = "secret",
            PasswordChar         = '*',
            IsAllowClear         = true,
            IsEnableRevealButton = true,
            IsMotionEnabled      = false
        };
        var layout = new StackPanel
        {
            Children =
            {
                textBox,
                lineEdit
            }
        };

        ShowInWindow(layout, () =>
        {
            var textBoxClear  = FindTemplatePart<InputClearIconButton>(textBox, "PART_ClearButton");
            var lineEditClear = FindTemplatePart<InputClearIconButton>(lineEdit, "PART_ClearButton");
            var textBoxReveal  = FindTemplatePart<RevealButton>(textBox, "PART_RevealButton");
            var lineEditReveal = FindTemplatePart<RevealButton>(lineEdit, "PART_RevealButton");

            textBoxClear.IsVisible.ShouldBeTrue();
            lineEditClear.IsVisible.ShouldBeTrue();
            textBoxReveal.IsVisible.ShouldBeTrue();
            lineEditReveal.IsVisible.ShouldBeTrue();

            textBoxClear.Icon.ShouldNotBeNull();
            lineEditClear.Icon.ShouldNotBeNull();
            textBoxClear.Icon!.GetType().ShouldBe(lineEditClear.Icon!.GetType());

            AssertSameIconType(textBoxReveal, lineEditReveal, AbstractToggleIconButton.CheckedIconProperty);
            AssertSameIconType(textBoxReveal, lineEditReveal, AbstractToggleIconButton.UnCheckedIconProperty);
        });
    }

    [Theory]
    [MemberData(nameof(TextInputControlTypesWithPlaceholder))]
    public void Placeholder_Hides_While_Ime_Preedit_Text_Is_Rendered(Type textInputType)
    {
        var textInput = CreateTextInputWithPlaceholder(textInputType);

        ShowInWindow(textInput, () =>
        {
            var placeholder = FindTemplatePart<TextBlock>(textInput, "Placeholder");
            var presenter   = FindTemplatePart<TextPresenter>(textInput, "PART_TextPresenter");

            placeholder.IsVisible.ShouldBeTrue();

            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, "测");
            Dispatcher.UIThread.RunJobs();

            placeholder.IsVisible.ShouldBeFalse(
                "IME preedit text is rendered by TextPresenter before TextBox.Text is committed, so the placeholder must not remain over it.");

            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, string.Empty);
            Dispatcher.UIThread.RunJobs();

            placeholder.IsVisible.ShouldBeTrue(
                "The placeholder should return when IME preedit text is cleared and the input text is still empty.");
        });
    }

    [Fact]
    public void LineEdit_Forwards_DataValidationErrors_To_AddOnDecoratedBox()
    {
        var lineEdit = new AtomUILineEdit
        {
            Width = 180
        };
        var validationError = new InvalidOperationException("native");

        ShowInWindow(lineEdit, () =>
        {
            var addOnDecoratedBox = lineEdit.GetVisualDescendants()
                                            .OfType<global::AtomUI.Desktop.Controls.AddOnDecoratedBox>()
                                            .Single(item => item.Name == global::AtomUI.Desktop.Controls.AddOnDecoratedBox.AddOnDecoratedBoxPart);

            DataValidationErrors.SetError(lineEdit, validationError);
            Dispatcher.UIThread.RunJobs();

            DataValidationErrors.GetHasErrors(addOnDecoratedBox).ShouldBeTrue();
            DataValidationErrors.GetErrors(addOnDecoratedBox).ShouldBe([validationError]);
        });
    }

    [Fact]
    public void LineEdit_DataValidationError_Overrides_Manual_Warning_For_AddOn_EffectiveStatus()
    {
        var lineEdit = new AtomUILineEdit
        {
            Width  = 180,
            Status = InputControlStatus.Warning
        };
        var validationError = new InvalidOperationException("native");

        ShowInWindow(lineEdit, () =>
        {
            var addOnDecoratedBox = lineEdit.GetVisualDescendants()
                                            .OfType<global::AtomUI.Desktop.Controls.AddOnDecoratedBox>()
                                            .Single(item => item.Name == global::AtomUI.Desktop.Controls.AddOnDecoratedBox.AddOnDecoratedBoxPart);

            addOnDecoratedBox.EffectiveStatus.ShouldBe(InputControlStatus.Warning);

            DataValidationErrors.SetError(lineEdit, validationError);
            Dispatcher.UIThread.RunJobs();

            addOnDecoratedBox.EffectiveStatus.ShouldBe(InputControlStatus.Error);

            DataValidationErrors.ClearErrors(lineEdit);
            Dispatcher.UIThread.RunJobs();

            addOnDecoratedBox.EffectiveStatus.ShouldBe(InputControlStatus.Warning);
        });
    }

    [Fact]
    public void LineEdit_Manual_Error_Status_Does_Not_Set_Native_Error_PseudoClass()
    {
        var lineEdit = new AtomUILineEdit
        {
            Width  = 180,
            Status = InputControlStatus.Error
        };

        ShowInWindow(lineEdit, () =>
        {
            lineEdit.Classes.Contains(":error").ShouldBeFalse();
            DataValidationErrors.GetHasErrors(lineEdit).ShouldBeFalse();
        });
    }

    [Fact]
    public void LineEdit_Form_Status_Preserves_Explicit_Status_And_Native_Error_Priority()
    {
        var lineEdit = new AtomUILineEdit
        {
            Width  = 180,
            Status = InputControlStatus.Error
        };
        var validationError = new InvalidOperationException("native");

        ShowInWindow(lineEdit, () =>
        {
            var frame = lineEdit.GetVisualDescendants()
                                .OfType<InputControlFrame>()
                                .Single();

            ((IFormItemAware)lineEdit).NotifyValidateStatus(FormValidateStatus.Warning);
            Dispatcher.UIThread.RunJobs();

            lineEdit.Status.ShouldBe(InputControlStatus.Error);
            lineEdit.FormStatus.ShouldBe(FormValidateStatus.Warning);
            frame.EffectiveStatus.ShouldBe(InputControlStatus.Warning);

            DataValidationErrors.SetError(lineEdit, validationError);
            Dispatcher.UIThread.RunJobs();
            frame.EffectiveStatus.ShouldBe(InputControlStatus.Error);

            ((IFormItemAware)lineEdit).NotifyValidateStatus(FormValidateStatus.Default);
            Dispatcher.UIThread.RunJobs();

            lineEdit.FormStatus.ShouldBe(FormValidateStatus.Default);
            DataValidationErrors.GetHasErrors(lineEdit).ShouldBeTrue();
            frame.EffectiveStatus.ShouldBe(InputControlStatus.Error);

            DataValidationErrors.ClearErrors(lineEdit);
            Dispatcher.UIThread.RunJobs();
            frame.EffectiveStatus.ShouldBe(InputControlStatus.Error);
        });
    }

    public static TheoryData<Type> TextInputControlTypesWithPlaceholder()
    {
        return new TheoryData<Type>
        {
            typeof(AtomUITextBox),
            typeof(AtomUILineEdit),
            typeof(AtomUISearchEdit),
            typeof(AtomUITextArea)
        };
    }

    private static Control CreateTextInputWithPlaceholder(Type textInputType)
    {
        if (textInputType == typeof(AtomUITextBox))
        {
            return new AtomUITextBox { Width = 180, PlaceholderText = "请输入" };
        }

        if (textInputType == typeof(AtomUILineEdit))
        {
            return new AtomUILineEdit { Width = 180, PlaceholderText = "请输入" };
        }

        if (textInputType == typeof(AtomUISearchEdit))
        {
            return new AtomUISearchEdit { Width = 180, PlaceholderText = "搜索" };
        }

        if (textInputType == typeof(AtomUITextArea))
        {
            return new AtomUITextArea { Width = 180, Height = 80, PlaceholderText = "请输入" };
        }

        throw new ArgumentOutOfRangeException(nameof(textInputType), textInputType, null);
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static T GetTextBoxTokenResource<T>(string kindName)
    {
        var tokenKind = (TextBoxTokenKind)Enum.Parse(typeof(TextBoxTokenKind), kindName);
        return GetThemeResource<T>(tokenKind);
    }

    private static void AssertCompositeInputHoverUsesControlEffectiveColorPrimaryHover(
        Control control,
        ControlTokenIdentity identity,
        ControlAlgorithmMode algorithm,
        bool expectDerivedHover)
    {
        var provider = new ThemeConfigProvider
        {
            Config = CreateControlPrimaryConfig(identity, algorithm),
            Child  = control
        };

        ShowInWindow(provider, () =>
        {
            control.TryFindResource(
                    SharedTokenKind.ColorPrimaryHover,
                    out var globalHoverResource)
                .ShouldBeTrue();
            var globalHover = GetSolidBrushColor((IBrush)globalHoverResource!);
            control.TryFindResource(ControlTokenResourceKey.Global(identity, SharedTokenKind.ColorPrimaryHover),
                    out var controlHoverResource).ShouldBeTrue();
            var controlHover = GetSolidBrushColor((IBrush)controlHoverResource!);
            if (expectDerivedHover)
            {
                controlHover.ShouldNotBe(globalHover);
            }
            else
            {
                controlHover.ShouldBe(globalHover);
            }

            var decoratedBox = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBox>(
                control,
                global::AtomUI.Desktop.Controls.AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var contentFrame = FindTemplatePart<PixelAlignedBorder>(control, "PART_ContentFrame");

            decoratedBox.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(contentFrame.BorderBrush).ShouldBe(controlHover);
        });
    }

    private static void AssertCompositeInputFocusWithinUsesControlEffectiveColorPrimary(
        Control control,
        ControlTokenIdentity identity,
        ControlAlgorithmMode algorithm)
    {
        var provider = new ThemeConfigProvider
        {
            Config = CreateControlPrimaryConfig(identity, algorithm),
            Child  = control
        };

        ShowInWindow(provider, () =>
        {
            control.TryFindResource(ControlTokenResourceKey.Global(identity, SharedTokenKind.ColorPrimary), out var controlPrimaryResource)
                .ShouldBeTrue();
            var controlPrimary = GetSolidBrushColor((IBrush)controlPrimaryResource!);
            var decoratedBox = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBox>(
                control,
                global::AtomUI.Desktop.Controls.AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var contentFrame = FindTemplatePart<PixelAlignedBorder>(control, "PART_ContentFrame");

            decoratedBox.IsInnerBoxHover = true;
            decoratedBox.Focus();
            Dispatcher.UIThread.RunJobs();

            decoratedBox.IsFocused.ShouldBeTrue();
            control.IsFocused.ShouldBeFalse();
            control.IsKeyboardFocusWithin.ShouldBeTrue();
            GetSolidBrushColor(contentFrame.BorderBrush).ShouldBe(controlPrimary);
        });
    }

    private static ThemeConfig CreateControlPrimaryConfig(
        ControlTokenIdentity identity,
        ControlAlgorithmMode algorithm)
    {
        return new ThemeConfigBuilder()
               .WithToken(nameof(SharedTokenKind.ColorPrimary), "#eb2f96")
               .WithToken(nameof(SharedTokenKind.ColorPrimaryHover), "#f759ab")
               .WithControl(identity, new ControlThemeConfigBuilder()
                   .WithAlgorithm(algorithm)
                   .WithToken(nameof(SharedTokenKind.ColorPrimary), "#52c41a")
                   .Build())
               .Build();
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected)
    {
        GetSolidBrushColor(actual).ShouldBe(GetSolidBrushColor(expected));
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : Control
    {
        var part = control.GetVisualDescendants()
                          .OfType<T>()
                          .SingleOrDefault(item => item.Name == name);
        part.ShouldNotBeNull();
        return part!;
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void SetPseudoClass(Control control, string pseudoClass, bool value)
    {
        ((IPseudoClasses)control.Classes).Set(pseudoClass, value);
    }

    private static void AssertSameIconType(AvaloniaObject actualOwner,
                                           AvaloniaObject expectedOwner,
                                           StyledProperty<PathIcon?> property)
    {
        var actual   = actualOwner.GetValue(property);
        var expected = expectedOwner.GetValue(property);

        actual.ShouldNotBeNull();
        expected.ShouldNotBeNull();
        actual!.GetType().ShouldBe(expected!.GetType());
    }

    private static void ShowInWindow(Control content, Action assertion, Action<AvaloniaWindow>? configureWindow = null)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = content
        };
        configureWindow?.Invoke(window);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
