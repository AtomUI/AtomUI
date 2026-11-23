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
[ControlDerivation]
internal sealed class TokenOnlyDerivation : IControlDerivation<LanguageDesignToken, TokenOnlyToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<TokenOnlyToken> view) =>
        view.Token.FixtureValue = 17;
}
[ControlDerivation]
internal sealed class IdentityOnlyDerivation : IControlDerivation<LanguageDesignToken, IdentityOnlyToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<IdentityOnlyToken> view) =>
        view.Token.FixtureValue = 23;
}
[ControlDerivation]
internal sealed class EnumOnlyDerivation : IControlDerivation<LanguageDesignToken, EnumOnlyToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<EnumOnlyToken> view) =>
        view.Token.FixtureValue = 31;
}
[ControlDerivation]
internal sealed class TokenKeyOnlyDerivation : IControlDerivation<LanguageDesignToken, TokenKeyOnlyToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<TokenKeyOnlyToken> view) =>
        view.Token.FixtureValue = 37;
}
[ControlDerivation]
internal sealed class ExtensionOnlyDerivation : IControlDerivation<LanguageDesignToken, ExtensionOnlyToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<ExtensionOnlyToken> view) =>
        view.Token.FixtureValue = 41;
}
