using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// OtpLineEdit: the identity-less cells (based on the shared InputControlFrame theme) in every size, variant and status,
/// the separators and the clear button. The cell theme reads the OtpLineEdit Tokens explicitly; the InputControlFrame base
/// theme and the clear button read their owner, which is the OtpLineEdit (the cells sit in an unregistered ItemsControl).
/// </summary>
public partial class DataEntryFamilyEffectTests
{
    internal static OtpLineEdit CreateOtp(
        CustomizableSizeType size = CustomizableSizeType.Middle,
        InputControlStyleVariant variant = InputControlStyleVariant.Outlined,
        InputControlStatus status = InputControlStatus.Default,
        string? text = "12",
        bool placeholder = false,
        bool clear = false,
        bool disabled = false) =>
        new()
        {
            Length          = 4,
            Text            = text,
            SizeType        = size,
            StyleVariant    = variant,
            Status          = status,
            PlaceholderText = placeholder ? "*" : null,
            IsAllowClear    = clear,
            Separator       = "-",
            IsEnabled       = !disabled
        };

    private static Control OtpCellAt(Control otp, int index) =>
        otp.GetVisualDescendants().OfType<OtpLineEditCell>().ElementAt(index);

    /// <summary>The InputControlFrame owner reads the cell theme replaces for every state, with the reason.</summary>
    private static readonly Dictionary<string, string> s_otpNotRendered = new(StringComparer.Ordinal);

    public static IEnumerable<object[]> OtpLineEditRows()
    {
        var rows = MapRows("OtpLineEdit").ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in InputFamilyEffectTests.s_inputControlFrame.Tokens
                                                     .Concat(InputFamilyEffectTests.s_clearButton.Tokens)
                                                     .Distinct(StringComparer.Ordinal)
                                                     .Order(StringComparer.Ordinal))
        {
            if (!s_otpNotRendered.ContainsKey(token) && seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    [Theory]
    [MemberData(nameof(OtpLineEditRows))]
    public void OtpLineEdit_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(OtpScenario(token), OtpLineEditTokens.Identity, token);
    }

    private static Scenario OtpScenario(string token)
    {
        var firstCell = new EffectOptions { Region = static otp => OtpCellAt(otp, 0) };
        switch (token)
        {
            case "JoinCells":
                return new Scenario(static () => new OtpLineEdit { Length = 4, Text = "12" });
            case "CellWidthLG" or "ControlHeightLG" or "FontSizeLG" or "BorderRadiusLG":
                return new Scenario(static () => CreateOtp(CustomizableSizeType.Large));
            case "CellWidthSM" or "ControlHeightSM" or "FontSizeSM" or "BorderRadiusSM":
                return new Scenario(static () => CreateOtp(CustomizableSizeType.Small));
            // Hovered and pressed outlined cells.
            case "ColorPrimaryHover":
                return new Scenario(static () => CreateOtp(), static otp => Pseudo(OtpCellAt(otp, 0), ":pointerover"), firstCell);
            case "ColorPrimary" or "InputActiveShadow":
                return new Scenario(static () => CreateOtp(), static otp => Pseudo(OtpCellAt(otp, 0), ":pressed"), firstCell);
            // Filled cells.
            case "ColorFillTertiary":
                return new Scenario(static () => CreateOtp(variant: InputControlStyleVariant.Filled));
            case "ColorFillSecondary":
                return new Scenario(static () => CreateOtp(variant: InputControlStyleVariant.Filled),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pointerover"), firstCell);
            // Error and warning cells (outlined border, filled background and text, hover and pressed states).
            case "ColorError":
                return new Scenario(static () => CreateOtp(status: InputControlStatus.Error));
            case "ColorErrorBorderHover":
                return new Scenario(static () => CreateOtp(status: InputControlStatus.Error),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pointerover"), firstCell);
            case "InputErrorActiveShadow":
                return new Scenario(static () => CreateOtp(status: InputControlStatus.Error),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pressed"), firstCell);
            case "ColorErrorBg" or "ColorErrorText":
                return new Scenario(static () => CreateOtp(variant: InputControlStyleVariant.Filled, status: InputControlStatus.Error));
            case "ColorErrorBgHover":
                return new Scenario(static () => CreateOtp(variant: InputControlStyleVariant.Filled, status: InputControlStatus.Error),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pointerover"), firstCell);
            case "ColorWarning":
                return new Scenario(static () => CreateOtp(status: InputControlStatus.Warning));
            case "ColorWarningBorderHover":
                return new Scenario(static () => CreateOtp(status: InputControlStatus.Warning),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pointerover"), firstCell);
            case "InputWarningActiveShadow":
                return new Scenario(static () => CreateOtp(status: InputControlStatus.Warning),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pressed"), firstCell);
            case "ColorWarningBg" or "ColorWarningText":
                return new Scenario(static () => CreateOtp(variant: InputControlStyleVariant.Filled, status: InputControlStatus.Warning));
            case "ColorWarningBgHover":
                return new Scenario(static () => CreateOtp(variant: InputControlStyleVariant.Filled, status: InputControlStatus.Warning),
                    static otp => Pseudo(OtpCellAt(otp, 0), ":pointerover"), firstCell);
            // Disabled cells.
            case "ColorTextDisabled" or "ColorBgContainerDisabled":
                return new Scenario(static () => CreateOtp(disabled: true));
            // Empty cells show the placeholder.
            case "ColorTextPlaceholder":
                return new Scenario(static () => CreateOtp(text: null, placeholder: true));
            // The clear button (shown with text).
            case "ColorTextQuaternary" or "ColorTextTertiary" or "IconSize":
                return new Scenario(static () => CreateOtp(clear: true), token == "ColorTextTertiary"
                    ? static otp => Pseudo(Descendant<Control>(otp, "PART_ClearButton"), ":pointerover")
                    : null, new EffectOptions { Region = static otp => Descendant<Control>(otp, "PART_ClearButton") });
            default:
                // CellWidth / ControlHeight, CellGap / UniformlyPaddingXXS, ColorBgContainer, ColorBorder, BorderThickness,
                // BorderRadius, ColorText, FontSize, EnableMotion (read back).
                return new Scenario(static () => CreateOtp());
        }
    }

    [Fact]
    public void Otp_NotRendered_Reasons_Name_Owner_Reads()
    {
        // Guards the exclusions: each must be an owner read of a primitive the OtpLineEdit carries.
        foreach (var token in s_otpNotRendered.Keys)
        {
            InputFamilyEffectTests.s_inputControlFrame.Tokens.Concat(InputFamilyEffectTests.s_clearButton.Tokens)
                                  .ShouldContain(token);
        }
    }
}
