using System.Diagnostics.CodeAnalysis;
using AtomUI.Controls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Toolkits.GalleryBase.Shell;

internal sealed class GalleryMediaBreakPointResolver : IDisposable
{
    private static readonly (object Key, MediaBreakPoint BreakPoint)[] s_thresholds =
    [
        (SharedTokenKind.ScreenXXXLMin, MediaBreakPoint.ExtraExtraExtraLarge),
        (SharedTokenKind.ScreenXXLMin, MediaBreakPoint.ExtraExtraLarge),
        (SharedTokenKind.ScreenXLMin, MediaBreakPoint.ExtraLarge),
        (SharedTokenKind.ScreenLGMin, MediaBreakPoint.Large),
        (SharedTokenKind.ScreenMDMin, MediaBreakPoint.Medium),
        (SharedTokenKind.ScreenSMMin, MediaBreakPoint.Small)
    ];

    private readonly Action _changed;
    private readonly OwnerTokenSet _resources;
    private readonly int[] _widths = new int[s_thresholds.Length];

    internal GalleryMediaBreakPointResolver(Control host, Action changed)
    {
        _changed = changed;
        // A publish can cross more than one breakpoint as the thresholds update. Preserve the original order
        // of the public events by reporting each changed threshold in the same order as the original bindings.
        _resources = new OwnerTokenSet(host, s_thresholds.Length, HandleThresholdChanged);
        try
        {
            for (var i = 0; i < s_thresholds.Length; i++)
            {
                _resources.SetResourceKey(i, s_thresholds[i].Key);
                _widths[i] = ReadWidth(_resources.GetValue(i), 0);
            }
        }
        catch
        {
            _resources.Dispose();
            throw;
        }
    }

    internal MediaBreakPoint Resolve(double width)
    {
        for (var i = 0; i < s_thresholds.Length; i++)
        {
            if (width >= _widths[i])
            {
                return s_thresholds[i].BreakPoint;
            }
        }
        return MediaBreakPoint.ExtraSmall;
    }

    public void Dispose() => _resources.Dispose();

    private void HandleThresholdChanged(int index, object? previous, object? value)
    {
        var width = ReadWidth(value, _widths[index]);
        if (_widths[index] != width)
        {
            _widths[index] = width;
            _changed();
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Screen tokens are integers. Local resources retain the same implicit conversion used by the previous Avalonia int property bindings.")]
    private static int ReadWidth(object? value, int current)
    {
        if (value is int width)
        {
            return width;
        }
        return ReferenceEquals(value, BindingOperations.DoNothing)
            ? current
            : BindingValue<int>.FromUntyped(value ?? AvaloniaProperty.UnsetValue).GetValueOrDefault();
    }
}
