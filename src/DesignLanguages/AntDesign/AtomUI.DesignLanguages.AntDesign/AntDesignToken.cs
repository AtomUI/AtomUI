using AtomUI.Theme.DesignTokens;

namespace AtomUI.DesignLanguages.AntDesign;

/// <summary>
/// Ant Design's own vocabulary. The global tokens are Ant Design shaped, so the language needs no tokens of its own;
/// the class exists so that control derivations bind to Ant Design and a brand language can derive from it.
/// </summary>
[LanguageDesignToken]
public partial class AntDesignToken : LanguageDesignToken
{
}
