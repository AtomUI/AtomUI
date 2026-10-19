using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Reflection;

// Avalonia 12.1.4 shares measure scratch between Grids but only clears it during Arrange.
// A retained measure-only Grid can therefore keep another Grid's definitions and owner alive.
// Remove this boundary after upgrading to a verified upstream measure-finally cleanup.
internal static class GridMeasureCacheCompatibility
{
    [DynamicDependency("s_tempDefinitionsDataSlot", typeof(Grid))]
    internal static bool EnsureInitialized()
    {
        Dispatcher.UIThread.VerifyAccess();
        try
        {
            var field = typeof(Grid).GetField("s_tempDefinitionsDataSlot", BindingFlags.Static | BindingFlags.NonPublic);
            if (field?.FieldType != typeof(LocalDataStoreSlot) || field.GetValue(null) is not LocalDataStoreSlot slot)
            {
                StartupProcess.LogWarning("Avalonia Grid measure cache changed; AtomUI cache isolation requires revalidation.");
                return false;
            }
            if (Thread.GetData(slot) is IsolatedMeasureCache)
            {
                return true;
            }
            if (!RetainsMeasuredDefinitions(slot))
            {
                return false;
            }

            // Do not clear a shared array: a nested Measure may still be using it. Declining
            // reuse gives each Grid its own scratch and leaves the current layout untouched.
            Thread.SetData(slot, new IsolatedMeasureCache());
            return true;
        }
        catch (MemberAccessException exception)
        {
            StartupProcess.LogWarning(exception);
            return false;
        }
        catch (NotSupportedException exception)
        {
            StartupProcess.LogWarning(exception);
            return false;
        }
    }

    private static bool RetainsMeasuredDefinitions(LocalDataStoreSlot slot)
    {
        var previous = Thread.GetData(slot);
        try
        {
            // Isolate the probe from application scratch. Upstream Measure cleanup leaves
            // this array empty, automatically disabling the shim when that fix is adopted.
            Thread.SetData(slot, null);
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,*"),
                Children = { new Border { Width = 40, Height = 20 } }
            };
            grid.ColumnDefinitions[0].MaxWidth = 100;
            grid.Measure(new Size(300, 100));
            var definitions = (Thread.GetData(slot) as WeakReference)?.Target as Array;
            GC.KeepAlive(grid);
            if (definitions is not null)
            {
                foreach (var definition in definitions)
                {
                    if (definition is not null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        finally
        {
            Thread.SetData(slot, previous);
        }
    }

    private sealed class IsolatedMeasureCache() : WeakReference(null)
    {
        // A statically compiled virtual override works under NativeAOT. Grid still owns
        // its array for the whole measure; only cross-Grid reuse is disabled.
        public override object? Target
        {
            get => null;
            set { }
        }
    }
}
