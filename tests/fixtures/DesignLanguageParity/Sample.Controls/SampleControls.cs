using AtomUI;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

[assembly: XmlnsDefinition("https://atomui.net/samples/controls", "Sample.Controls")]
[assembly: XmlnsDefinition("https://atomui.net/samples/controls", "Sample.Controls.DesignTokens")]

namespace Sample.Controls;

/// <summary>
/// A badge whose surface comes from <see cref="SampleBadgeToken" />.
/// </summary>
public class SampleBadge : TemplatedControl;

/// <summary>
/// A tag whose surface comes from <see cref="SampleTagToken" />.
/// </summary>
public class SampleTag : TemplatedControl;

public static class SampleControlsBuilderExtensions
{
    /// <summary>
    /// Enables the sample control package. Its values come from the value package of the enabled language.
    /// </summary>
    public static IAtomUIBuilder UseSampleControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AtomUI.Generated.SampleControls.PackageMetadata.Definition.Register(builder);
    }
}
