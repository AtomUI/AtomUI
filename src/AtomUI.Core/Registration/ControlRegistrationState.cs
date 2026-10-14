namespace AtomUI.Registration;

internal sealed class ControlRegistrationState
{
    private readonly Dictionary<string, bool> _packages = new(StringComparer.Ordinal);
    private Exception? _failure;
    private bool _frozen;

    internal bool CanDeferFragments => !_stageTwoWindowUsed;

    internal void ThrowIfInitialConfigurationUnavailable()
    {
        ThrowIfUnavailable();
        if (_stageTwoWindowUsed)
        {
            throw new InvalidOperationException(
                "This operation is only available during initial AtomUI configuration, before Build or Deferred execution.");
        }
    }

    internal void ThrowIfUnavailable()
    {
        if (_failure is not null)
        {
            throw new InvalidOperationException("Control package registration failed; this builder cannot be used again.", _failure);
        }
        if (_frozen)
        {
            throw new InvalidOperationException("Control package registration is already frozen.");
        }
    }

    internal bool IsStaged(string packageId)
    {
        ThrowIfUnavailable();
        return _packages.TryGetValue(packageId, out var staged) && staged;
    }

    internal void Enter(string packageId)
    {
        ThrowIfUnavailable();
        if (!_packages.TryAdd(packageId, false))
        {
            throw new InvalidOperationException($"Control package '{packageId}' is already registered or being registered.");
        }
    }

    internal void Stage(string packageId)
    {
        ThrowIfUnavailable();
        _packages[packageId] = true;
    }

    internal void Freeze()
    {
        ThrowIfUnavailable();
        if (_packages.Values.Any(static staged => !staged))
        {
            throw new InvalidOperationException("Cannot build while a control package is being registered.");
        }
        _frozen = true;
    }

    // 分级启动：Stage 2 注册窗口受控重开（一次性）。Build 冻结后仅可重开一次，
    // 窗口关闭后永久冻结（晚期注册维持现行抛异常行为）。
    private bool _stageTwoWindowUsed;

    internal void ReopenForStageTwo()
    {
        // 注意不能用 ThrowIfUnavailable（frozen 恰是重开的前提）；只拦失败态。
        if (_failure is not null)
        {
            throw new InvalidOperationException(
                "Control package registration failed; this builder cannot be used again.", _failure);
        }
        if (!_frozen || _stageTwoWindowUsed)
        {
            throw new InvalidOperationException(
                "Stage 2 registration window is unavailable (not frozen, or already used).");
        }
        _stageTwoWindowUsed = true;
        _frozen = false;
    }

    internal void CloseStageTwoWindow()
    {
        if (!_frozen && _stageTwoWindowUsed)
        {
            _frozen = true;
        }
    }

    internal void Fail(Exception exception) => _failure ??= exception;
}
