using AtomUI.Theme.Compilation;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

internal sealed class ThemeTokenResolver
{
    internal ThemeSnapshot Capture(AvaloniaObject owner)
    {
        return GetContext(owner).Snapshot;
    }

    internal (ThemeSnapshot Snapshot, int ControlSlot) CaptureControl(Control owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        return CaptureControl(owner, owner.GetType());
    }

    internal (ThemeSnapshot Snapshot, int ControlSlot) CaptureControl(
        Control owner,
        Type controlType)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(controlType);
        var snapshot = GetContext(owner).Snapshot;
        if (!snapshot.Registry.TryGetControl(controlType, out var descriptor))
        {
            throw new InvalidOperationException(
                $"Control type '{controlType.FullName}' is not registered in the captured theme schema.");
        }

        return (snapshot, descriptor.Slot);
    }

    internal T GetGlobal<T>(ThemeSnapshot snapshot, int slot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return snapshot.GlobalTokenValues.Get<T>(slot);
    }

    internal T GetControl<T>(
        ThemeSnapshot snapshot,
        int controlSlot,
        int tokenSlot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfNegative(controlSlot);
        if ((uint)controlSlot >= (uint)snapshot.Controls.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(controlSlot));
        }
        if ((uint)tokenSlot >= (uint)snapshot.Registry.Controls[controlSlot].OwnTokens.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenSlot));
        }
        snapshot.Registry.NotifyControlFirstUse(controlSlot);
        return snapshot.Controls[controlSlot].ControlTokenValues.Get<T>(tokenSlot);
    }

    internal T GetOwn<T>(
        ThemeSnapshot snapshot,
        int controlSlot,
        object resourceKey)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentOutOfRangeException.ThrowIfNegative(controlSlot);
        if ((uint)controlSlot >= (uint)snapshot.Controls.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(controlSlot));
        }
        if (!snapshot.Registry.TryGetControlResourceSlot(resourceKey, out var resourceControlSlot) ||
            resourceControlSlot != controlSlot)
        {
            throw new InvalidOperationException(
                $"Own Token resource key '{resourceKey}' does not belong to the captured Control.");
        }

        foreach (var token in snapshot.Registry.Controls[controlSlot].OwnTokens)
        {
            if (Equals(token.ResourceKey, resourceKey))
            {
                snapshot.Registry.NotifyControlFirstUse(controlSlot);
                return snapshot.Controls[controlSlot].ControlTokenValues.Get<T>(token.Slot);
            }
        }

        throw new InvalidOperationException(
            $"Own Token resource key '{resourceKey}' is not registered in the captured Control schema.");
    }

    internal int GetControlSlot(ThemeSnapshot snapshot, Schema.ControlTokenIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!snapshot.Registry.TryGetControl(identity, out var descriptor))
        {
            throw new KeyNotFoundException(
                $"Control Token identity '{identity}' is not registered in the captured theme snapshot.");
        }

        return descriptor.Slot;
    }

    internal T GetEffectiveGlobal<T>(
        ThemeSnapshot snapshot,
        int controlSlot,
        int tokenSlot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfNegative(controlSlot);
        if ((uint)controlSlot >= (uint)snapshot.Controls.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(controlSlot));
        }

        if ((uint)tokenSlot >= (uint)snapshot.Registry.GlobalTokens.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenSlot));
        }
        snapshot.Registry.NotifyControlFirstUse(controlSlot);
        return snapshot.Controls[controlSlot]
                       .GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, tokenSlot);
    }

    internal PaletteInfo GetPresetPalette(
        ThemeSnapshot snapshot,
        PresetPrimaryColor primaryColor)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(primaryColor);
        if (!snapshot.PresetColorPalettes.TryGetValue(primaryColor, out var palette))
        {
            throw new KeyNotFoundException(
                $"Preset color palette '{primaryColor}' is not available in the captured theme snapshot.");
        }

        return palette;
    }

    internal IDisposable Subscribe(
        AvaloniaObject owner,
        Action<ThemeSnapshot> changed)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(changed);
        if (owner is not StyledElement styledOwner)
        {
            throw new InvalidOperationException(
                "Theme Token subscriptions require a StyledElement owner.");
        }
        ThemeScope.ResolveContext(styledOwner);
        return new ContextSubscription(styledOwner, changed);
    }

    private static ThemeContext GetContext(AvaloniaObject owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (owner is StyledElement styledOwner &&
            ThemeScope.ResolveContext(styledOwner) is { } context)
        {
            return context;
        }
        throw new InvalidOperationException(
            "The owner has no ThemeContext. Attach it to an AtomUI themed tree or provide an explicit owner context.");
    }

    private sealed class ContextSubscription : IDisposable
    {
        private readonly Action<ThemeSnapshot> _changed;
        private readonly IDisposable _contextPropertySubscription;
        private ThemeContext? _context;
        private bool _initializing = true;
        private bool _disposed;

        internal ContextSubscription(
            StyledElement owner,
            Action<ThemeSnapshot> changed)
        {
            _changed = changed;
            _contextPropertySubscription = owner
                                           .GetObservable(ThemeScope.ContextProperty)
                                           .Subscribe(AttachContext);
            _initializing = false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _contextPropertySubscription.Dispose();
            AttachContext(null);
        }

        private void AttachContext(ThemeContext? context)
        {
            if (ReferenceEquals(_context, context))
            {
                return;
            }
            if (_context is not null)
            {
                _context.Published -= HandlePublished;
            }
            _context = context;
            if (!_disposed && context is not null)
            {
                context.Published += HandlePublished;
                if (!_initializing)
                {
                    _changed(context.Snapshot);
                }
            }
        }

        private void HandlePublished(object? sender, EventArgs args)
        {
            if (!_disposed && sender is ThemeContext context)
            {
                _changed(context.Snapshot);
            }
        }
    }
}
