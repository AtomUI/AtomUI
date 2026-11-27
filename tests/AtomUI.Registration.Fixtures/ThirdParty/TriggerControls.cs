using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Controls;

namespace Fixture.ThirdParty;

public class TokenOnly : Control;
[ControlDesignToken]
public sealed partial class TokenOnlyToken : AbstractControlDesignToken
{
    /// <summary>
    /// The only own token of the <c>TokenOnly</c> fixture control, which exists to trigger a path of the
    /// registration generator. <c>TokenOnlyTokenRule</c> sets it to 17 under every design language.
    /// </summary>
    public partial double FixtureValue { get; set; }
}
public class IdentityOnly : Control;
[ControlDesignToken]
public sealed partial class IdentityOnlyToken : AbstractControlDesignToken
{
    /// <summary>
    /// The only own token of the <c>IdentityOnly</c> fixture control, which exists to trigger a path of the
    /// registration generator. <c>IdentityOnlyTokenRule</c> sets it to 23 under every design language.
    /// </summary>
    public partial double FixtureValue { get; set; }
}
public class EnumOnly : Control;
[ControlDesignToken]
public sealed partial class EnumOnlyToken : AbstractControlDesignToken
{
    /// <summary>
    /// The only own token of the <c>EnumOnly</c> fixture control, which exists to trigger a path of the
    /// registration generator. <c>EnumOnlyTokenRule</c> sets it to 31 under every design language.
    /// </summary>
    public partial double FixtureValue { get; set; }
}
public class TokenKeyOnly : Control;
[ControlDesignToken]
public sealed partial class TokenKeyOnlyToken : AbstractControlDesignToken
{
    /// <summary>
    /// The only own token of the <c>TokenKeyOnly</c> fixture control, which exists to trigger a path of the
    /// registration generator. <c>TokenKeyOnlyTokenRule</c> sets it to 37 under every design language.
    /// </summary>
    public partial double FixtureValue { get; set; }
}
public class ExtensionOnly : Control;
[ControlDesignToken]
public sealed partial class ExtensionOnlyToken : AbstractControlDesignToken
{
    /// <summary>
    /// The only own token of the <c>ExtensionOnly</c> fixture control, which exists to trigger a path of the
    /// registration generator. <c>ExtensionOnlyTokenRule</c> sets it to 41 under every design language.
    /// </summary>
    public partial double FixtureValue { get; set; }
}
// The leaf part of the fixture control: a runtime-created Border with the class semantic-fixture-leaf.
// Present only when the owner creates it. SemanticOnlyLeafStyle applies to that Border.
[SemanticPart("leaf", SelectorClass = "semantic-fixture-leaf", ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Optional, Customization = SemanticPartCustomization.Selector,
    SelectorRoute = ">> .semantic-fixture-leaf", Since = "1.0.0", CrossNestedOwners = true, RuntimeCreated = true, RestHidden = true)]
public class SemanticOnly : Control;

// The fixture values, the same under every design language (a control package may carry such generic derivations).
[ControlTokenRule]
internal sealed class TokenOnlyTokenRule : IControlTokenRule<LanguageDesignToken, TokenOnlyToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<TokenOnlyToken> target) =>
        target.Own.FixtureValue = 17;
}
[ControlTokenRule]
internal sealed class IdentityOnlyTokenRule : IControlTokenRule<LanguageDesignToken, IdentityOnlyToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<IdentityOnlyToken> target) =>
        target.Own.FixtureValue = 23;
}
[ControlTokenRule]
internal sealed class EnumOnlyTokenRule : IControlTokenRule<LanguageDesignToken, EnumOnlyToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<EnumOnlyToken> target) =>
        target.Own.FixtureValue = 31;
}
[ControlTokenRule]
internal sealed class TokenKeyOnlyTokenRule : IControlTokenRule<LanguageDesignToken, TokenKeyOnlyToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<TokenKeyOnlyToken> target) =>
        target.Own.FixtureValue = 37;
}
[ControlTokenRule]
internal sealed class ExtensionOnlyTokenRule : IControlTokenRule<LanguageDesignToken, ExtensionOnlyToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ExtensionOnlyToken> target) =>
        target.Own.FixtureValue = 41;
}
