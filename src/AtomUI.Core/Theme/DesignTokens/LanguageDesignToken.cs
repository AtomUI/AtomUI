namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// The base of a design language's own token class: the roles, geometry and color scales the global tokens have no
/// place for. Only that language's derivations read language tokens; control templates never see them.
/// </summary>
public abstract class LanguageDesignToken : AbstractDesignToken
{
}
