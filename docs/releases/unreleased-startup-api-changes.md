# Pending startup API migration

This document describes the approved, unreleased startup API change in the working tree. It does not assign a release
version or change package versions. The eventual release must call out this source and binary compatibility break.

Chinese version: [启动 API 迁移](unreleased-startup-api-changes.zh-CN.md).

## Removed APIs

`IAtomUIStartupEvents`, `AtomUIStartupEvents`, `StartupStage`, `StartupStageEventArgs`, and both
`Application.GetStartupEvents()` and `IAtomUIBuilder.GetStartupEvents()` are removed. Remove configure-time event
subscriptions and rebuild consumers. `Deferred(Action)` remains the declaration entry point with the existing phase
capability checks.

## Await the shared startup result

After `UseAtomUI`, obtain the application's `IAtomUIStartup` through `Application.GetStartup()`:

```csharp
var startup = application.GetStartup();
try
{
    await startup.Completion.WaitAsync(windowCancellation);
    BuildContent();
}
catch (OperationCanceledException) when (windowCancellation.IsCancellationRequested)
{
    // The window stopped waiting; other observers and application startup continue.
}
catch (Exception exception)
{
    ShowStartupError(exception);
}
```

`State` is `Configuring`, `WaitingForHost`, `Initializing`, `Ready`, `Failed`, or `Canceled`. Synchronous startup is already
`Ready` when `UseAtomUI` returns. Deferred startup completes after the host is ready and framework initialization and
cleanup finish. Late observers await the same result. Failures preserve the original exception; application startup
cancellation yields a canceled task. Disposing services after `Ready` does not rewrite that outcome. A failure thrown
synchronously by `UseAtomUI` still propagates synchronously.

Replace `StageTwoCompleted`/`StageTwoFailed` handlers with the task's success/error paths. Use `ProgressChanged` only for
optional UI-thread progress; events do not replay and subscriber exceptions cannot change startup's result. The caller's
`WaitAsync` cancellation only stops that caller. `GetStartup()` throws if AtomUI has not been registered.

Framework readiness does not mean application content has rendered. Gallery schedules shell creation separately and
cancels its wait and queued work when its window closes. See [startup architecture](../architecture/foundations/startup-and-registration.md).
