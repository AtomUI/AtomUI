using AtomUI;
using Avalonia.Controls;

namespace Fixture.ThirdParty;
public class IncludeControl : ContentControl;
public class UnusedThirdPartyControl : ContentControl;
public static class Registration
{
    public static IAtomUIBuilder UseFixtureControls(this IAtomUIBuilder builder) =>
        AtomUI.Generated.AtomUIRegistrationFixturesThirdParty.GeneratedControlPackageRegistration.Definition.Register(builder);
}
