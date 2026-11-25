using System.Reflection;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeManagerTransactionTests
{
    [Fact]
    public async Task Root_Commit_Publishes_One_Atomic_State()
    {
        var prepared = CreatePrepared("First", "#1677ff");
        var manager = CreateManager((_, _, _) => ValueTask.FromResult(prepared));
        ThemeChangedEventArgs? observed = null;
        manager.ThemeChanged += (_, args) =>
        {
            observed = args;
            manager.CurrentTheme.ShouldBeSameAs(args.State);
            manager.CurrentSnapshot.ShouldBeSameAs(prepared.Snapshot);
        };

        var result = await ApplyAsync(manager, Request("First"));

        result.Status.ShouldBe(ThemeTransitionStatus.Committed);
        result.State.ShouldBeSameAs(manager.CurrentTheme);
        result.State!.TransitionId.ShouldBe(result.TransitionId);
        observed.ShouldNotBeNull();
        observed!.Request.ThemeId.ShouldBe("First");
        observed.State.ShouldBeSameAs(result.State);
    }

    [Fact]
    public async Task Equivalent_Committed_Request_Is_A_NoOp_Without_Preparing_Again()
    {
        var prepared = CreatePrepared("First", "#1677ff");
        var attempts = 0;
        var manager = CreateManager((_, _, _) =>
        {
            attempts++;
            return ValueTask.FromResult(prepared);
        });
        var changed = 0;
        manager.ThemeChanged += (_, _) => changed++;

        var first = await ApplyAsync(manager, Request("First"));
        var second = await ApplyAsync(manager, Request("First"));

        first.Status.ShouldBe(ThemeTransitionStatus.Committed);
        second.Status.ShouldBe(ThemeTransitionStatus.NoOp);
        second.State.ShouldBeSameAs(first.State);
        attempts.ShouldBe(1);
        changed.ShouldBe(1);
    }

    [Fact]
    public async Task Equivalent_Concurrent_Requests_Join_One_InFlight_Transaction()
    {
        var prepared = CreatePrepared("First", "#1677ff");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;
        var manager = CreateManager(async (_, _, token) =>
        {
            token.ShouldBe(CancellationToken.None);
            attempts++;
            started.SetResult();
            await release.Task;
            return prepared;
        });

        var firstTask = ApplyAsync(manager, Request("First"));
        await started.Task;
        var secondTask = ApplyAsync(manager, Request("First"));
        release.SetResult();
        var first = await firstTask;
        var second = await secondTask;

        attempts.ShouldBe(1);
        second.TransitionId.ShouldBe(first.TransitionId);
        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task Caller_Cancellation_Does_Not_Cancel_The_Shared_Transaction()
    {
        var prepared = CreatePrepared("First", "#1677ff");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;
        var manager = CreateManager(async (_, _, token) =>
        {
            token.ShouldBe(CancellationToken.None);
            attempts++;
            started.SetResult();
            await release.Task;
            return prepared;
        });
        using var cancellation = new CancellationTokenSource();

        var canceledWaiter = manager.ApplyThemeAsync(Request("First"), cancellation.Token);
        await started.Task;
        var survivingWaiter = ApplyAsync(manager, Request("First"));
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(async () => await canceledWaiter);
        release.SetResult();

        (await survivingWaiter).Status.ShouldBe(ThemeTransitionStatus.Committed);
        attempts.ShouldBe(1);
        manager.CurrentTheme.ShouldNotBeNull();
    }

    [Fact]
    public async Task Newer_Request_Supersedes_Stale_Preparation_Before_Commit()
    {
        var firstPrepared = CreatePrepared("First", "#1677ff");
        var secondPrepared = CreatePrepared("Second", "#52c41a");
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager(async (request, _, _) =>
        {
            if (request.ThemeId == "First")
            {
                firstStarted.SetResult();
                await releaseFirst.Task;
                return firstPrepared;
            }
            return secondPrepared;
        });

        var firstTask = ApplyAsync(manager, Request("First"));
        await firstStarted.Task;
        var secondTask = ApplyAsync(manager, Request("Second"));
        releaseFirst.SetResult();
        var first = await firstTask;
        var second = await secondTask;

        first.Status.ShouldBe(ThemeTransitionStatus.Superseded);
        second.Status.ShouldBe(ThemeTransitionStatus.Committed);
        manager.CurrentTheme!.ThemeId.ShouldBe("Second");
        manager.CurrentSnapshot.ShouldBeSameAs(secondPrepared.Snapshot);
    }

    [Fact]
    public async Task Stale_Preparation_Failure_Is_Superseded_Without_A_Failure_Event()
    {
        var secondPrepared = CreatePrepared("Second", "#52c41a");
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var failure = ThemeTransactionPreparation.Failed(
            [new ThemeDiagnostic("TEST002", ThemeDiagnosticSeverity.Error, "Test", "$", "stale failure")]);
        var manager = CreateManager(async (request, _, _) =>
        {
            if (request.ThemeId == "First")
            {
                firstStarted.SetResult();
                await releaseFirst.Task;
                return failure;
            }
            return secondPrepared;
        });
        var failureEvents = 0;
        manager.ThemeChangeFailed += (_, _) => failureEvents++;

        var firstTask = ApplyAsync(manager, Request("First"));
        await firstStarted.Task;
        var secondTask = ApplyAsync(manager, Request("Second"));
        releaseFirst.SetResult();

        (await firstTask).Status.ShouldBe(ThemeTransitionStatus.Superseded);
        (await secondTask).Status.ShouldBe(ThemeTransitionStatus.Committed);
        failureEvents.ShouldBe(0);
    }

    [Fact]
    public async Task Pending_Requests_Coalesce_To_The_Latest_Request()
    {
        var firstPrepared = CreatePrepared("First", "#1677ff");
        var thirdPrepared = CreatePrepared("Third", "#722ed1");
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var preparedThemes = new List<string>();
        var manager = CreateManager(async (request, _, _) =>
        {
            preparedThemes.Add(request.ThemeId);
            if (request.ThemeId == "First")
            {
                firstStarted.SetResult();
                await releaseFirst.Task;
                return firstPrepared;
            }
            return thirdPrepared;
        });

        var firstTask = ApplyAsync(manager, Request("First"));
        await firstStarted.Task;
        var secondTask = ApplyAsync(manager, Request("Second"));
        var thirdTask = ApplyAsync(manager, Request("Third"));
        releaseFirst.SetResult();

        (await firstTask).Status.ShouldBe(ThemeTransitionStatus.Superseded);
        (await secondTask).Status.ShouldBe(ThemeTransitionStatus.Superseded);
        (await thirdTask).Status.ShouldBe(ThemeTransitionStatus.Committed);
        preparedThemes.ShouldBe(["First", "Third"]);
    }

    [Fact]
    public async Task Latest_Request_Replaces_Pending_Even_When_It_Equals_The_Active_Request()
    {
        var firstPrepared = CreatePrepared("First", "#1677ff");
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var preparedThemes = new List<string>();
        var manager = CreateManager(async (request, _, _) =>
        {
            preparedThemes.Add(request.ThemeId);
            if (preparedThemes.Count == 1)
            {
                firstStarted.SetResult();
                await releaseFirst.Task;
            }
            return firstPrepared;
        });

        var activeFirst = ApplyAsync(manager, Request("First"));
        await firstStarted.Task;
        var pendingSecond = ApplyAsync(manager, Request("Second"));
        var latestFirst = ApplyAsync(manager, Request("First"));
        releaseFirst.SetResult();

        (await activeFirst).Status.ShouldBe(ThemeTransitionStatus.Superseded);
        (await pendingSecond).Status.ShouldBe(ThemeTransitionStatus.Superseded);
        (await latestFirst).Status.ShouldBe(ThemeTransitionStatus.Committed);
        preparedThemes.ShouldBe(["First", "First"]);
        manager.CurrentTheme!.ThemeId.ShouldBe("First");
    }

    [Fact]
    public async Task Reentrant_Request_Is_Queued_Until_The_Current_Publish_Completes()
    {
        var firstPrepared = CreatePrepared("First", "#1677ff");
        var secondPrepared = CreatePrepared("Second", "#52c41a");
        var manager = CreateManager((request, _, _) => ValueTask.FromResult(
            request.ThemeId == "First" ? firstPrepared : secondPrepared));
        var events = new List<string>();
        Task<ThemeTransitionResult>? reentrant = null;
        manager.ThemeChanged += (_, args) =>
        {
            events.Add(args.State.ThemeId);
            if (args.State.ThemeId == "First")
            {
                reentrant = ApplyAsync(manager, Request("Second"));
                reentrant.IsCompleted.ShouldBeFalse();
            }
        };

        var first = await ApplyAsync(manager, Request("First"));
        var second = await reentrant!;

        first.Status.ShouldBe(ThemeTransitionStatus.Committed);
        second.Status.ShouldBe(ThemeTransitionStatus.Committed);
        events.ShouldBe(["First", "Second"]);
    }

    [Fact]
    public async Task Publish_Observer_Failure_Does_Not_Roll_Back_And_Later_Observers_Run()
    {
        var prepared = CreatePrepared("First", "#1677ff");
        var manager = CreateManager((_, _, _) => ValueTask.FromResult(prepared));
        var laterObserverRan = false;
        var previousSink = Logger.Sink;
        var sink = new RecordingLogSink();
        Logger.Sink = sink;
        manager.ThemeChanged += (_, _) => throw new InvalidOperationException("observer failure");
        manager.ThemeChanged += (_, args) =>
        {
            laterObserverRan = true;
            manager.CurrentTheme.ShouldBeSameAs(args.State);
            manager.CurrentSnapshot.ShouldBeSameAs(prepared.Snapshot);
        };

        ThemeTransitionResult result;
        try
        {
            result = await ApplyAsync(manager, Request("First"));
        }
        finally
        {
            Logger.Sink = previousSink;
        }

        result.Status.ShouldBe(ThemeTransitionStatus.Committed);
        laterObserverRan.ShouldBeTrue();
        result.PublishDiagnostics.ShouldContain(diagnostic =>
            diagnostic.Severity == ThemeDiagnosticSeverity.Warning &&
            diagnostic.Message.Contains("observer failure", StringComparison.Ordinal));
        sink.Messages.ShouldContain(message =>
            message.Contains(nameof(ThemeManager.ThemeChanged), StringComparison.Ordinal) &&
            message.Contains("observer failure", StringComparison.Ordinal));
        manager.CurrentSnapshot.ShouldBeSameAs(prepared.Snapshot);
    }

    [Fact]
    public void Publish_Sets_Explicit_Variant_And_Resources_Before_Result_Observers()
    {
        HeadlessTestApp.Run(() =>
        {
            var prepared = CreatePrepared("First", "#1677ff");
            var manager = CreateManager((_, _, _) => ValueTask.FromResult(prepared));
            manager.ConfigureStartup(Request("First"), null, null, null);
            var application = Application.Current!;
            var observed = false;
            manager.ThemeChanged += (_, _) =>
            {
                observed = true;
                application.RequestedThemeVariant.ShouldBe(ThemeVariant.Light);
                var provider = manager.Resources.MergedDictionaries
                                      .OfType<ThemeTokenResourceProvider>()
                                      .ShouldHaveSingleItem();
                provider.Snapshot.ShouldBeSameAs(prepared.Snapshot);
            };

            manager.InitializeApplication(application);

            observed.ShouldBeTrue();
            application.RequestedThemeVariant.ShouldBe(ThemeVariant.Light);
        });
    }

    [Fact]
    public void First_Root_Publish_Reports_Context_Observer_Failure()
    {
        HeadlessTestApp.Run(() =>
        {
            var prepared = CreatePrepared("First", "#1677ff");
            var manager = CreateManager((_, _, _) => ValueTask.FromResult(prepared));
            manager.ConfigureStartup(Request("First"), null, null, null);
            var application = Application.Current!;
            application.RequestedThemeVariant = ThemeVariant.Dark;
            ThemeChangedEventArgs? observed = null;
            using var subscription = application
                                     .GetObservable(Application.RequestedThemeVariantProperty)
                                     .Subscribe(_ =>
                                     {
                                         if (manager.CurrentSnapshot is not null)
                                         {
                                             manager.RootContext.Published += (_, _) =>
                                                 throw new InvalidOperationException("context observer failure");
                                         }
                                     });
            manager.ThemeChanged += (_, args) => observed = args;

            manager.InitializeApplication(application);

            observed.ShouldNotBeNull();
            observed!.PublishDiagnostics.ShouldContain(diagnostic =>
                diagnostic.Severity == ThemeDiagnosticSeverity.Warning &&
                diagnostic.Message.Contains("context observer failure", StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task Prepare_Failure_Leaves_The_Previously_Committed_State_Unchanged()
    {
        var firstPrepared = CreatePrepared("First", "#1677ff");
        var failure = ThemeTransactionPreparation.Failed(
            [new ThemeDiagnostic("TEST001", ThemeDiagnosticSeverity.Error, "Test", "$", "invalid")],
            new InvalidOperationException("prepare failure"));
        var manager = CreateManager((request, _, _) => ValueTask.FromResult(
            request.ThemeId == "First" ? firstPrepared : failure));
        var failures = 0;
        manager.ThemeChangeFailed += (_, args) =>
        {
            failures++;
            args.Request.ThemeId.ShouldBe("Broken");
        };
        var committed = await ApplyAsync(manager, Request("First"));
        var previousState = manager.CurrentTheme;
        var previousSnapshot = manager.CurrentSnapshot;

        var failed = await ApplyAsync(manager, Request("Broken"));

        committed.Status.ShouldBe(ThemeTransitionStatus.Committed);
        failed.Status.ShouldBe(ThemeTransitionStatus.Failed);
        failed.Diagnostics.ShouldContain(diagnostic => diagnostic.Code == "TEST001");
        manager.CurrentTheme.ShouldBeSameAs(previousState);
        manager.CurrentSnapshot.ShouldBeSameAs(previousSnapshot);
        failures.ShouldBe(1);
    }

    [Fact]
    public async Task Scope_Config_Revision_Change_Supersedes_A_Stale_Root_Preparation()
    {
        var initial = CreatePrepared("Initial", "#1677ff");
        var next = CreatePrepared("Next", "#52c41a");
        var nextStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseNext = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager(async (request, _, _) =>
        {
            if (request.ThemeId == "Next")
            {
                nextStarted.SetResult();
                await releaseNext.Task;
                return next;
            }
            return initial;
        });
        (await ApplyAsync(manager, Request("Initial"))).Status.ShouldBe(ThemeTransitionStatus.Committed);
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.ColorPrimary), "#ff0000")
                     .Build(),
            Child = new Border()
        };
        var root = new LogicalRoot();
        root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
        root.Child = provider;
        var context = provider.GetValue(ThemeScope.ContextProperty)!;
        var previousLocalSnapshot = context.Snapshot;
        var localCommitted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.ThemeChanged += (_, _) => localCommitted.TrySetResult();

        var nextTask = ApplyAsync(manager, Request("Next"));
        nextStarted.Task.IsCompleted.ShouldBeTrue();
        provider.Config = new ThemeConfigBuilder()
                          .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                          .Build();
        context.Snapshot.ShouldBeSameAs(previousLocalSnapshot);
        releaseNext.SetResult();

        (await nextTask).Status.ShouldBe(ThemeTransitionStatus.Superseded);
        await localCommitted.Task;
        context.Snapshot.Global<Avalonia.Media.Color>(nameof(DesignToken.ColorPrimary))
               .ShouldBe(Avalonia.Media.Color.Parse("#00b96b"));
    }

    [Fact]
    public async Task Root_Transaction_Replays_The_Latest_Local_Config_Queued_During_Local_Publish()
    {
        var initial = CreatePrepared("Initial", "#1677ff");
        var next = CreatePrepared("Next", "#52c41a");
        var nextStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager((request, _, _) => ValueTask.FromResult(
            Prepare(request, initial, next, nextStarted)));
        (await ApplyAsync(manager, Request("Initial"))).Status.ShouldBe(ThemeTransitionStatus.Committed);
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.ColorPrimary), "#ff0000")
                     .Build(),
            Child = new Border()
        };
        var root = new LogicalRoot();
        root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
        root.Child = provider;
        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        var finalConfig = new ThemeConfigBuilder()
                          .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                          .Build();
        var finalLocalCommit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ThemeTransitionResult>? rootTransition = null;
        var transactionUsersDuringLocalPublish = -1;
        var localCommits = 0;
        provider.ThemeChanged += (_, _) =>
        {
            localCommits++;
            if (localCommits == 1)
            {
                transactionUsersDuringLocalPublish = GetTransactionExecutionUsers(manager);
                provider.Config = finalConfig;
                rootTransition = ApplyAsync(manager, Request("Next"));
                nextStarted.Task.IsCompleted.ShouldBeFalse();
            }
            else
            {
                finalLocalCommit.TrySetResult();
            }
        };

        provider.Config = new ThemeConfigBuilder()
                         .WithToken(nameof(DesignToken.ColorPrimary), "#fa8c16")
                         .Build();

        transactionUsersDuringLocalPublish.ShouldBeGreaterThan(0);
        (await rootTransition!).Status.ShouldBe(ThemeTransitionStatus.Committed);
        await finalLocalCommit.Task.WaitAsync(TestContext.Current.CancellationToken);
        context.Snapshot.Global<Avalonia.Media.Color>(nameof(DesignToken.ColorPrimary))
               .ShouldBe(Avalonia.Media.Color.Parse("#00b96b"));
        manager.ScopeGraph.TryGetNode(provider, out var node).ShouldBeTrue();
        node!.LastValidConfig.ShouldBeSameAs(finalConfig);

        static ThemeTransactionPreparation Prepare(
            ThemeRequest request,
            ThemeTransactionPreparation initial,
            ThemeTransactionPreparation next,
            TaskCompletionSource nextStarted)
        {
            if (request.ThemeId == "Next")
            {
                nextStarted.TrySetResult();
                return next;
            }

            return initial;
        }
    }

    [Fact]
    public async Task Dispose_Supersedes_Active_And_Queued_Root_Transactions()
    {
        var prepared = CreatePrepared("First", "#1677ff");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager(async (_, _, _) =>
        {
            started.TrySetResult();
            await release.Task;
            return prepared;
        });

        var active = ApplyAsync(manager, Request("First"));
        await started.Task;
        var queued = ApplyAsync(manager, Request("Second"));

        manager.Dispose();

        (await queued.WaitAsync(TestContext.Current.CancellationToken))
            .Status.ShouldBe(ThemeTransitionStatus.Superseded);
        Should.Throw<ObjectDisposedException>(() => manager.ApplyThemeAsync(Request("Third")));

        release.TrySetResult();
        (await active.WaitAsync(TestContext.Current.CancellationToken))
            .Status.ShouldBe(ThemeTransitionStatus.Superseded);
        manager.CurrentTheme.ShouldBeNull();
        manager.CurrentSnapshot.ShouldBeNull();
    }

    private static ThemeManager CreateManager(ThemePrepareDelegate prepare)
    {
        return new ThemeManager(static () => true, prepare);
    }

    private static ThemeRequest Request(string themeId)
    {
        return new ThemeRequest(themeId, null, ThemeTransitionReason.UserRequest);
    }

    private static Task<ThemeTransitionResult> ApplyAsync(
        ThemeManager manager,
        ThemeRequest request)
    {
        return manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken);
    }

    private static int GetTransactionExecutionUsers(ThemeManager manager)
    {
        var field = typeof(ThemeManager).GetField(
            "_transactionExecutionUsers",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (int)field.GetValue(manager)!;
    }

    private static ThemeTransactionPreparation CreatePrepared(string themeId, string color)
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        registry.TryGetGlobalToken(nameof(DesignToken.ColorPrimary), out var descriptor).ShouldBeTrue();
        var parsed = descriptor!.Parse(color);
        var token = new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
        var input = TypedThemeSnapshotCacheTests.CreateInput(registry, globalTokens: [token]);
        var snapshot = new ThemeCompiler().Compile(input).Snapshot!;
        return ThemeTransactionPreparation.Succeeded(
            snapshot,
            ThemeSnapshotCacheKey.Create(input));
    }

    private sealed class LogicalRoot : Decorator, ILogicalRoot
    {
    }

    private sealed class RecordingLogSink : ILogSink
    {
        internal List<string> Messages { get; } = new();

        public bool IsEnabled(LogEventLevel level, string area) => true;

        public void Log(
            LogEventLevel level,
            string area,
            object? source,
            string messageTemplate)
        {
            Messages.Add(messageTemplate);
        }

        public void Log(
            LogEventLevel level,
            string area,
            object? source,
            string messageTemplate,
            params object?[] propertyValues)
        {
            Messages.Add(string.Format(messageTemplate, propertyValues));
        }
    }
}
