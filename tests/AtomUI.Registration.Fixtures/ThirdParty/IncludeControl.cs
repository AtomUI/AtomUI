using AtomUI.Generated.AtomUIRegistrationFixturesThirdParty;
using AtomUI;
using Avalonia.Controls;

namespace Fixture.ThirdParty;
public class IncludeControl : ContentControl;
public class UnusedThirdPartyControl : ContentControl;
public static class Registration
{
    public static IAtomUIBuilder UseFixtureControls(this IAtomUIBuilder builder)
    {
        GeneratedControlTokenRuleRegistration.Register(builder.Theme);
        return AtomUI.Generated.AtomUIRegistrationFixturesThirdParty.PackageMetadata.Definition.Register(builder);
    }
}
