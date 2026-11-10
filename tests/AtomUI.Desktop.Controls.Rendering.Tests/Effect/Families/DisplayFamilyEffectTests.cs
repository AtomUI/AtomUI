using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>Every Active Own and consumed global token of the display remainder, with causal state arrangements.</summary>
public partial class DisplayFamilyEffectTests
{
    private sealed record Scenario(Func<Control> Create, Action<Control>? Arrange = null, EffectOptions? Options = null);

    private static IEnumerable<object[]> Rows(string id) => EffectTestData.For("AtomUI", id).ToArray();

    private static void Run(Scenario scenario, ControlTokenIdentity identity, string token) =>
        TokenEffectHarness.AssertHasEffect(scenario.Create, identity, token, scenario.Arrange, options: scenario.Options);

    private static T Part<T>(Control root, string? name = null) where T : Control =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => name is null || control.Name == name)
            .ShouldNotBeNull($"Missing {typeof(T).Name} {name} under {root.GetType().Name}");

    private static void Pseudo(Control control, string value) => ((IPseudoClasses)control.Classes).Add(value);

    public static IEnumerable<object[]> ResultRows() => Rows("Result");
    public static IEnumerable<object[]> EmptyRows() => Rows("Empty");
    public static IEnumerable<object[]> QRCodeRows() => Rows("QRCode");
    public static IEnumerable<object[]> SpinRows() => Rows("Spin");

    [Theory, MemberData(nameof(ResultRows))]
    public void Result_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(ResultScenario(token), ResultTokens.Identity, token);
    }

    [Theory, MemberData(nameof(EmptyRows))]
    public void Empty_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(EmptyScenario(token), EmptyTokens.Identity, token);
    }

    [Theory, MemberData(nameof(QRCodeRows))]
    public void QRCode_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(QRCodeScenario(token), QRCodeTokens.Identity, token);
    }

    [Theory, MemberData(nameof(SpinRows))]
    public void Spin_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(SpinScenario(token), SpinTokens.Identity, token);
    }

    // Fixed rows fail on Shared reads before migration, and cannot disappear when generated consumption changes.
    [Theory]
    [InlineData("Result", "ColorTextHeading")]
    [InlineData("Empty", "ColorTextDescription")]
    [InlineData("QRCode", "ColorSplit")]
    [InlineData("Spin", "ColorPrimary")]
    public void Global_Read_Uses_Registered_Host(string member, string token)
    {
        var (scenario, identity) = member switch
        {
            "Result" => (ResultScenario(token), ResultTokens.Identity),
            "Empty" => (EmptyScenario(token), EmptyTokens.Identity),
            "QRCode" => (QRCodeScenario(token), QRCodeTokens.Identity),
            "Spin" => (SpinScenario(token), SpinTokens.Identity),
            _ => throw new ArgumentOutOfRangeException(nameof(member))
        };
        Run(scenario, identity, token);
    }

    internal static Result CreateResult(ResultStatus status = ResultStatus.Success) => new()
    {
        Header = "Completed", SubHeader = "The request has been processed.", Extra = "Continue", Content = "Details", Status = status
    };

    private static Scenario ResultScenario(string token)
    {
        var status = token switch
        {
            "ResultInfoIconColor" or "ColorInfo" => ResultStatus.Info,
            "ResultWarningIconColor" or "ColorWarning" => ResultStatus.Warning,
            "ResultErrorIconColor" or "ColorError" => ResultStatus.Error,
            "ImageWidth" or "ImageHeight" => ResultStatus.ErrorCode404,
            _ => ResultStatus.Success
        };
        return new Scenario(() => CreateResult(status));
    }

    internal static Empty CreateEmpty(SizeType size = SizeType.Large) => new()
    {
        PresetImage = PresetEmptyImage.Default, SizeType = size, Description = "No records", Footer = "Add a record"
    };

    private static Scenario EmptyScenario(string token) => new(() =>
    {
        var empty = CreateEmpty(token switch
    {
        "EmptyImgHeightMD" or "DescriptionMarginSM" or "UniformlyMarginXS" => SizeType.Middle,
        "EmptyImgHeightSM" => SizeType.Small,
        _ => SizeType.Large
        });
        if (token == "ColorFillQuaternary")
        {
            empty.PresetImage = PresetEmptyImage.Simple;
        }
        return empty;
    });

    [Fact]
    public void QRCode_Null_Color_Fallback_Uses_Effective_Owner_Text_Color() =>
        Run(new Scenario(static () =>
        {
            var code = CreateQRCode();
            code.Color = null;
            return code;
        }), QRCodeTokens.Identity, "ColorText");

    internal static QRCode CreateQRCode(QRCodeStatus status = QRCodeStatus.Active) => new()
    {
        Value = "https://atomui.net", Status = status, IsBordered = true
    };

    private static Scenario QRCodeScenario(string token) => new(() => CreateQRCode(
        token is "QRCodeMaskBackgroundColor" or "ColorBgContainer" ? QRCodeStatus.Scanned : QRCodeStatus.Active), null,
        new EffectOptions { Perturb = token == "BorderRadiusLG" ? static _ => new Avalonia.CornerRadius(40) : null });

    internal static Spin CreateSpin(CustomizableSizeType size = CustomizableSizeType.Middle) => new()
    {
        Width = 200, Height = 140, SizeType = size, IsSpinning = true, IsTipVisible = true, Tip = "Loading", Content = "Content",
        MotionDuration = TimeSpan.FromDays(1), IsMaskBackgroundEnabled = true
    };

    private static Scenario SpinScenario(string token) => new(() => CreateSpin(token switch
    {
        "DotSizeLG" or "IndicatorSizeLG" or "ControlHeight" => CustomizableSizeType.Large,
        "DotSizeSM" or "IndicatorSizeSM" => CustomizableSizeType.Small,
        _ => CustomizableSizeType.Middle
    }), token is "IndicatorDuration" or "MotionDurationSlow" ? static c => c.ClearValue(Spin.MotionDurationProperty) : null,
        new EffectOptions { Region = token is "DotSize" or "DotSizeSM" or "DotSizeLG" or "UniformlyMarginXXS" ? static c => Part<SpinIndicator>(c) : token == "SpacingXXS" ? static c => Part<Control>(c, "IndicatorLayout") : null });
}
