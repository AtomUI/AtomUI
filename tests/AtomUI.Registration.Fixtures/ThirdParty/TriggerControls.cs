using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Controls;

namespace Fixture.ThirdParty;

public class TokenOnly : Control;
[ControlDesignToken]
public sealed partial class TokenOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; }
}
public class IdentityOnly : Control;
[ControlDesignToken]
public sealed partial class IdentityOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; }
}
public class EnumOnly : Control;
[ControlDesignToken]
public sealed partial class EnumOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; }
}
public class TokenKeyOnly : Control;
[ControlDesignToken]
public sealed partial class TokenKeyOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; }
}
public class ExtensionOnly : Control;
[ControlDesignToken]
public sealed partial class ExtensionOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; }
}
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
