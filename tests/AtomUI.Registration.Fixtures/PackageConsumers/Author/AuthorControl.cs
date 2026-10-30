using AtomUI;
namespace Fixture.NuGetAuthor;
public class AuthorControl : Avalonia.Controls.Control;
public class TypedKeyHostControl : Avalonia.Controls.Control;
public class TypedKeyDependencyControl : Avalonia.Controls.Control;
public class SharedResourceControl : Avalonia.Controls.Control;
public class UnrelatedResourceControl : Avalonia.Controls.Control;

// The target is portable; only this optional named theme is unavailable on Browser.
[System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal partial class DesktopOverlayTheme : Avalonia.Controls.ResourceDictionary
{
    public DesktopOverlayTheme() => Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
}

public static class Registration
{
    public static IAtomUIBuilder UseAuthorControls(this IAtomUIBuilder builder) =>
        AtomUI.Generated.FixtureNuGetAuthor.GeneratedControlPackageRegistration.Definition.Register(builder);
}
