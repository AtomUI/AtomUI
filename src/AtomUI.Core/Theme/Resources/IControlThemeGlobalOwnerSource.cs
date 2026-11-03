using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

// Private template assets can reuse a public input theme without acquiring its
// Global identity. This is an owner relationship, not a new resource scope.
internal interface IControlThemeGlobalOwnerSource
{
    Control? ThemeGlobalOwner { get; }
}
