namespace AtomUI.Theme;

/// <summary>
/// Marks a registered control as a pure container for Token ownership: <see cref="ThemeManager"/> does not inject
/// <c>ThemeScope.TokenOwner</c> for it, so the content it hosts keeps the owner inherited from its host (a drop-down
/// of a Select stays owned by the Select). The control's own visuals read its <c>XxxTokenResource</c> explicitly.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class TokenOwnerTransparentAttribute : Attribute
{
}
