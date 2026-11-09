using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Gives a part the Token owner of its host. A registered control between the host and the part (the popup's
/// ArrowDecoratedBox, a ScrollViewer, the DateViewer of a Calendar) would otherwise become the owner that the part's
/// <c>OwnerTokenResource</c> reads and <c>TokenTransitions</c> resolve. The binding keeps following the host's owner;
/// disposing it clears the forwarded value, so the part inherits again.
/// </summary>
internal static class TokenOwnerForwarding
{
    public static IDisposable Forward(Control host, Control part)
    {
        var binding = part.Bind(ThemeScope.TokenOwnerProperty, host.GetObservable(ThemeScope.TokenOwnerProperty));
        return new Forwarded(part, binding);
    }

    private sealed class Forwarded(Control part, IDisposable binding) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            binding.Dispose();
            part.ClearValue(ThemeScope.TokenOwnerProperty);
        }
    }
}
