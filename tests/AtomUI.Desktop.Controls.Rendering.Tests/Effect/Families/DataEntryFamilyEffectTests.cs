using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the data-entry remainder (docs/engineering/development/theme-effect-tests.md): Form (its
/// Token-owner transparent FormItem and FormItemDecorator), Descriptions (identity-less item parts), Upload (the Token-owner
/// transparent UploadTrigger, UploadDefaultDropArea and UploadDropZone, identity-less list items), Mentions (the Token-owner
/// transparent CandidateList and CandidateListItem in its popup), OtpLineEdit (identity-less cells) and Segmented (the
/// Token-owner transparent SegmentedItem). Every Active Own Token and every consumed global Token of each member visibly
/// changes the member when overridden through <c>Controls[member]</c>.
/// </summary>
/// <remarks>
/// The identity-less and transparent parts of these members read their host's TokenResource explicitly (each part has a
/// single host), so the consumption map attributes their reads to the host. CandidateList inherits the shared ListBox themes;
/// its owner-read lists are drift-guarded in the Mentions tests. The OtpLineEdit
/// cells are based on the shared InputControlFrame theme, whose owner reads run as OtpLineEdit rows through
/// <see cref="InputFamilyEffectTests.s_inputControlFrame"/>.
/// </remarks>
public partial class DataEntryFamilyEffectTests
{
    /// <summary>
    /// The member's consumption-map rows. A member without any map entry fails fast: a shrinking consumption map
    /// (a lost consumer, a renamed control) must surface as a failing theory, not as a silently empty one.
    /// </summary>
    private static IEnumerable<object[]> MapRows(string controlId) =>
        EffectTestData.For("AtomUI", controlId).ToArray();

    /// <summary>
    /// The rows of a Token-owner transparent part. Such a part reads its host's Tokens, so the generated map may
    /// attribute all of its reads to the host and hold no rows under the part's own id; that is the only case
    /// where an empty set is legitimate.
    /// </summary>
    private static IEnumerable<object[]> TransparentPartRows(string partId)
    {
        try
        {
            return EffectTestData.For("AtomUI", partId).ToArray();
        }
        catch (ArgumentException)
        {
            return [];
        }
    }

    /// <summary>The member's map rows plus the rows of the transparent parts it hosts (they read the host's Tokens).</summary>
    private static IEnumerable<object[]> HostRows(string controlId, params string[] transparentParts)
    {
        var rows = MapRows(controlId).ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in transparentParts.SelectMany(static part => TransparentPartRows(part).Select(static row => (string)row[0]))
                                              .Distinct(StringComparer.Ordinal)
                                              .Order(StringComparer.Ordinal))
        {
            if (seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    private static void Run(Scenario scenario, ControlTokenIdentity identity, string token) =>
        TokenEffectHarness.AssertHasEffect(scenario.Create, identity, token, scenario.Arrange, options: scenario.Options);

    private sealed record Scenario(Func<Control> Create, Action<Control>? Arrange = null, EffectOptions? Options = null);

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    /// <summary>The first visual descendant of <paramref name="root"/> of type T, named <paramref name="name"/> if given.</summary>
    private static T Descendant<T>(Visual root, string? name = null)
        where T : Control =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => name is null || control.Name == name)
            .ShouldNotBeNull($"No {typeof(T).Name}{(name is null ? string.Empty : " named " + name)} under {root.GetType().Name}.");

    private static void WithProvider(Control content, Action<ThemeConfigProvider> body) =>
        FeedbackFamilyEffectTests.WithProvider(content, body);

    private static void WaitFor(Func<bool> condition) => FeedbackFamilyEffectTests.WaitFor(condition);

    // ---- Form ------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// A two-item form: a required item with a tooltip and a decorated line edit (the decorator's extra text), and an optional
    /// item with help and extra text. <paramref name="status"/> is the validate status of both items.
    /// </summary>
    internal static Form CreateForm(
        FormLayout layout = FormLayout.Horizontal,
        CustomizableSizeType size = CustomizableSizeType.Middle,
        FormValidateStatus status = FormValidateStatus.Default,
        bool feedback = false)
    {
        var form = new Form
        {
            Width      = 520,
            FormLayout = layout,
            SizeType   = size
        };
        form.Items.Add(new FormItem
        {
            LabelText                 = "Name",
            FieldName                 = "name",
            IsRequired                = true,
            Tooltip                   = "The full name",
            IsValidateFeedbackEnabled = feedback,
            ValidateStatus            = status,
            Content = feedback
                ? new LineEdit { Width = 180, Text = "Ada" }
                : new FormItemDecorator { Child = new LineEdit { Width = 180, Text = "Ada" }, Extra = "Extra note" }
        });
        form.Items.Add(new FormItem
        {
            LabelText      = "City",
            FieldName      = "city",
            Help           = "Where you live",
            Extra          = "Shown on the profile",
            ValidateStatus = status,
            Content        = new LineEdit { Width = 180 }
        });
        return form;
    }

    private static FormItem FormItemAt(Control form, int index) =>
        form.GetVisualDescendants().OfType<FormItem>().ElementAt(index);

    private static Control LabelOf(Control form, int index = 0) => Descendant<Panel>(FormItemAt(form, index), "PART_LabelLayout");

    public static IEnumerable<object[]> FormRows() => HostRows("Form", "FormItem", "FormItemDecorator");

    [Theory]
    [MemberData(nameof(FormRows))]
    public void Form_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(FormScenario(token), FormTokens.Identity, token);
    }

    [Fact]
    public void Vertical_Label_Padding_Changes_The_Vertical_Label_Row() =>
        Run(FormScenario("VerticalLabelPadding"), FormTokens.Identity, "VerticalLabelPadding");

    // Fixed rows remain present when the pre-migration consumption map has no control-scoped global reads.
    [Theory]
    [InlineData("Form", "ColorTextDescription")]
    [InlineData("Descriptions", "ColorSplit")]
    [InlineData("Upload", "ColorFillSecondary")]
    [InlineData("Mentions", "ColorBgElevated")]
    [InlineData("OtpLineEdit", "ColorBorder")]
    [InlineData("Segmented", "ColorTextDisabled")]
    public void Family_Global_Read_Uses_The_Registered_Host(string control, string token)
    {
        var (scenario, identity) = control switch
        {
            "Form" => (FormScenario(token), FormTokens.Identity),
            "Descriptions" => (DescriptionsScenario(token), DescriptionsTokens.Identity),
            "Upload" => (UploadScenario(token), UploadTokens.Identity),
            "Mentions" => (MentionsScenario(token), MentionsTokens.Identity),
            "OtpLineEdit" => (OtpScenario(token), OtpLineEditTokens.Identity),
            "Segmented" => (SegmentedScenario(token), SegmentedTokens.Identity),
            _ => throw new ArgumentOutOfRangeException(nameof(control))
        };
        Run(scenario, identity, token);
    }

    private static Scenario FormScenario(string token)
    {
        switch (token)
        {
            // The label: text, size, required mark and colon.
            case "LabelColor" or "ColorTextHeading" or "LabelFontSize" or "FontSize" or
                "LabelColonMargin" or "UniformlyMarginXXS" or "UniformlyMarginXS" or "SpacingXXS":
                return new Scenario(static () => CreateForm(), null,
                    new EffectOptions { Region = static form => LabelOf(form) });
            // The required mark (ColorError is also the error feedback icon colour).
            case "LabelRequiredMarkColor" or "ColorError":
                return new Scenario(static () => CreateForm(), null,
                    new EffectOptions { Region = static form => Descendant<Avalonia.Controls.TextBlock>(FormItemAt(form, 0), "PART_DefaultRequireMark") });
            // The tooltip icon after the label.
            case "ColorIcon" or "IconSize":
                return new Scenario(static () => CreateForm(), null,
                    new EffectOptions { Region = static form => Descendant<IconPresenter>(FormItemAt(form, 0), "TooltipIconPresenter") });
            // Help and extra texts.
            case "ColorTextDescription":
                return new Scenario(static () => CreateForm(), null,
                    new EffectOptions { Region = static form => FormItemAt(form, 1) });
            case "ColorWarningText":
                return new Scenario(static () => CreateForm(status: FormValidateStatus.Warning), null,
                    new EffectOptions { Region = static form => FormItemAt(form, 1) });
            case "ColorErrorText":
                return new Scenario(static () => CreateForm(status: FormValidateStatus.Error), null,
                    new EffectOptions { Region = static form => FormItemAt(form, 1) });
            // Validate feedback icons in the line edit suffix. An item with feedback validates when it attaches (no
            // validators: Success), so the warning status is set once the form is shown.
            case "ColorSuccess":
                return new Scenario(static () => CreateForm(status: FormValidateStatus.Success, feedback: true), null,
                    new EffectOptions { Region = static form => Descendant<LineEdit>(FormItemAt(form, 0)) });
            case "ColorWarning":
                return new Scenario(static () => CreateForm(feedback: true),
                    static form => FormItemAt(form, 0).ValidateStatus = FormValidateStatus.Warning,
                    new EffectOptions { Region = static form => Descendant<LineEdit>(FormItemAt(form, 0)) });
            // Vertical layout: label padding and margin.
            case "VerticalLabelPadding" or "VerticalLabelMargin" or "UniformlyPaddingXS":
                return new Scenario(static () => CreateForm(FormLayout.Vertical));
            // Inline layout: the gap between items.
            case "InlineItemSpacing" or "Spacing":
                return new Scenario(static () => CreateForm(FormLayout.Inline));
            // Label row heights per size.
            case "ControlHeightLG":
                return new Scenario(static () => CreateForm(size: CustomizableSizeType.Large));
            case "ControlHeightSM":
                return new Scenario(static () => CreateForm(size: CustomizableSizeType.Small));
            default:
                // FormItemSpacing / SpacingLG (gap below each item), ControlHeight, SpacingXS (the decorator's extra gap),
                // EnableMotion (read back).
                return new Scenario(static () => CreateForm());
        }
    }
}
