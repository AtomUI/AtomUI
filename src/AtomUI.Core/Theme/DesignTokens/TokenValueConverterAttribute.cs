namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks a class that implements <c>ITokenValueConverter</c> for the source generator, which lists it in the
/// generated Token value converter registry. Abstract, generic and non-converter classes are ignored.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TokenValueConverterAttribute : Attribute
{
}