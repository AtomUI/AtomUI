using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;

namespace Fixture.ThirdParty;

public class TokenOnly : Control;
[ControlDesignToken]
public sealed partial class TokenOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; } = 17;
}
public class IdentityOnly : Control;
[ControlDesignToken]
public sealed partial class IdentityOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; } = 23;
}
public class EnumOnly : Control;
[ControlDesignToken]
public sealed partial class EnumOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; } = 31;
}
public class TokenKeyOnly : Control;
[ControlDesignToken]
public sealed partial class TokenKeyOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; } = 37;
}
public class ExtensionOnly : Control;
[ControlDesignToken]
public sealed partial class ExtensionOnlyToken : AbstractControlDesignToken
{
    public partial double FixtureValue { get; set; } = 41;
}
[SemanticPart("leaf", SelectorClass = "semantic-fixture-leaf", ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Optional, Customization = SemanticPartCustomization.Selector,
    SelectorRoute = ">> .semantic-fixture-leaf", Since = "1.0.0", CrossNestedOwners = true, RuntimeCreated = true, RestHidden = true)]
public class SemanticOnly : Control;
