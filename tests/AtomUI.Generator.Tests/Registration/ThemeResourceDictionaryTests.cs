// Explicit ControlTheme resource dictionaries must retain the same lexical lookup as implicit resources.
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests.Registration;

public class ThemeResourceDictionaryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Theme_Resource_Dictionary_Resolves_Local_And_Included_Keys(bool include)
    {
        var content = include
            ? "<ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"/Resources/Themes/ResourcesTheme.axaml\" /></ResourceDictionary.MergedDictionaries>"
            : "<SolidColorBrush x:Key=\"Accent\">Red</SolidColorBrush>";
        var theme = Asset("Button", "<ControlTheme x:Key=\"{x:Type local:Button}\" TargetType=\"local:Button\"><ControlTheme.Resources><ResourceDictionary>" + content +
            "</ResourceDictionary></ControlTheme.Resources><Setter Property=\"Background\" Value=\"{StaticResource Accent}\" /></ControlTheme>");
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }", theme,
            Asset("Resources", "<SolidColorBrush x:Key=\"Accent\">Red</SolidColorBrush>"));
        result.Diagnostics.ShouldBeEmpty();
    }
}
