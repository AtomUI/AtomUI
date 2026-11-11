using AtomUI.Core.Tests.Theme;
using AtomUI.Registration;
using AtomUI.Startup.Data;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Registration;

/// <summary>
/// The warm startup query path reads materialized descriptors and the package's terminal first-use state
/// without the gate; these contracts pin the locked semantics that the fast path must leave intact.
/// </summary>
[Collection(ThemeConfigProviderTestCollection.Name)]
public class StartupWarmPathTests
{
    private const int Threads = 8;
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(10);
    private static int s_sink;

    [Fact]
    public void Concurrent_First_GetDescriptor_Materializes_Once_And_Returns_One_Instance()
    {
        using var arrived = new CountdownEvent(Threads);
        var activations = 0;
        var entry = CreateEntry(builder =>
        {
            arrived.Wait(s_timeout).ShouldBeTrue("every caller must be waiting before the single materialization completes");
            Interlocked.Increment(ref activations);
            builder.AddControl(ButtonDescriptor());
        });

        var results = RunConcurrently(() =>
        {
            arrived.Signal();
            return entry.GetDescriptor();
        });

        activations.ShouldBe(1);
        results.ShouldAllBe(descriptor => ReferenceEquals(descriptor, results[0]));
        results[0].Slot.ShouldBe(0);
        entry.GetDescriptor().ShouldBeSameAs(results[0]);
        MeasureAllocations(() => entry.GetDescriptor() is not null).ShouldBe(0, "warm descriptor read must not allocate");
    }

    [Fact]
    public void Failed_Materialization_Is_Rethrown_On_Every_Call()
    {
        var activations = 0;
        var entry = CreateEntry(_ =>
        {
            activations++;
            throw new InvalidOperationException("fragment failed");
        });

        for (var attempt = 0; attempt < 3; attempt++)
        {
            Should.Throw<InvalidOperationException>(() => entry.GetDescriptor()).Message.ShouldBe("fragment failed");
        }
        activations.ShouldBe(3);
    }

    [Fact]
    public void Recursive_Materialization_Still_Throws()
    {
        StartupControlEntry? self = null;
        var entry = CreateEntry(_ => self!.Materialize());
        self = entry;

        Should.Throw<InvalidOperationException>(() => entry.GetDescriptor())
              .Message.ShouldStartWith("Recursive Control registration materialization");
        Should.Throw<InvalidOperationException>(() => entry.Materialize())
              .Message.ShouldStartWith("Recursive Control registration materialization");
    }

    [Fact]
    public void Semantic_Descriptor_Is_Materialized_Once_And_Read_Warm()
    {
        var activations = 0;
        var semantic = new ControlSemanticDescriptor(typeof(Button), ButtonIdentity(),
        [
            new SemanticPartDescriptor("root", "root", null, typeof(Button), SemanticPartCardinality.Single,
                SemanticPartCustomization.Root, null, false, null, false)
        ]);
        var entry = CreateEntry(builder =>
        {
            activations++;
            builder.AddSemanticControl(semantic);
        }, hasSemantic: true, hasTokenDescriptor: false);

        var first = entry.GetSemantic();
        first.ShouldBeSameAs(semantic);
        entry.GetSemantic().ShouldBeSameAs(first);
        activations.ShouldBe(1);
        MeasureAllocations(() => entry.GetSemantic() is not null).ShouldBe(0, "warm semantic read must not allocate");
    }

    [Fact]
    public void FirstUse_Runs_Initializers_Exactly_Once_Under_Concurrency_Then_Exits_Warm()
    {
        HeadlessTestApp.Run(() =>
        {
            using var started = new CountdownEvent(Threads);
            var initializations = 0;
            var package = CreatePackage(_ =>
            {
                started.Wait(s_timeout).ShouldBeTrue("every caller must be waiting before the single initialization completes");
                Interlocked.Increment(ref initializations);
            });
            using var manager = new AtomUIBuilder(Application.Current!.GetType()).ThemeManagerBuilder.Build();
            package.PrepareContext(Application.Current!, manager);

            RunConcurrently(() =>
            {
                started.Signal();
                package.FirstUse();
                return true;
            });

            initializations.ShouldBe(1);
            package.FirstUse();
            initializations.ShouldBe(1);
            MeasureAllocations(() => { package.FirstUse(); return true; }).ShouldBe(0, "warm first-use exit must not allocate");
            package.Release();
            package.FirstUse();
            initializations.ShouldBe(1);
        });
    }

    [Fact]
    public void FirstUse_Failure_Is_Rethrown_On_Every_Call_And_Released_Package_Returns_Early()
    {
        HeadlessTestApp.Run(() =>
        {
            var initializations = 0;
            var failing = CreatePackage(_ =>
            {
                initializations++;
                throw new InvalidOperationException("first-use failed");
            });
            using var manager = new AtomUIBuilder(Application.Current!.GetType()).ThemeManagerBuilder.Build();
            failing.PrepareContext(Application.Current!, manager);
            Should.Throw<InvalidOperationException>(failing.FirstUse).Message.ShouldBe("first-use failed");
            Should.Throw<InvalidOperationException>(failing.FirstUse).Message.ShouldBe("first-use failed");
            initializations.ShouldBe(1);

            var released = CreatePackage(_ => initializations++);
            released.PrepareContext(Application.Current!, manager);
            released.Release();
            released.FirstUse();
            initializations.ShouldBe(1);

            var notReady = CreatePackage(_ => initializations++);
            Should.Throw<InvalidOperationException>(notReady.FirstUse)
                  .Message.ShouldStartWith("Control package was consumed before its lifecycle context was ready");
            initializations.ShouldBe(1);
        });
    }

    private static ControlTokenIdentity ButtonIdentity() => ControlTokenIdentity.ForControl(typeof(Button), "Test", "Button");

    private static ControlTokenDescriptor ButtonDescriptor() => new(typeof(Button), ButtonIdentity());

    private static EnabledControlPackage CreatePackage(Action<ControlPackageContext>? firstUse = null,
        Action<ControlPackageRegistrationBuilder>? fragment = null)
    {
        var definition = new ControlPackageDefinition("Test", "unused",
            _ => fragment is null ? null : new DelegateFragmentAttribute(fragment), static _ => true);
        var options = new ControlPackageOptions();
        if (firstUse is not null)
        {
            options.OnFirstUse(firstUse);
        }
        return new EnabledControlPackage(definition, options);
    }

    private static StartupControlEntry CreateEntry(Action<ControlPackageRegistrationBuilder> fragment,
        bool hasSemantic = false, bool hasTokenDescriptor = true)
    {
        var assembly = typeof(Button).Assembly.GetName().Name!;
        var data = new StartupControlData("Test", "Button", typeof(Button).FullName!, ["control"], [],
            hasSemantic, hasTokenDescriptor, assemblyName: assembly);
        return new StartupControlEntry(CreatePackage(fragment: fragment), assembly, data) { Slot = 0 };
    }

    private static T[] RunConcurrently<T>(Func<T> action)
    {
        var results = new T[Threads];
        var failures = new Exception?[Threads];
        var threads = new Thread[Threads];
        for (var index = 0; index < Threads; index++)
        {
            var slot = index;
            threads[index] = new Thread(() =>
            {
                try
                {
                    results[slot] = action();
                }
                catch (Exception error)
                {
                    failures[slot] = error;
                }
            });
            threads[index].Start();
        }
        foreach (var thread in threads)
        {
            thread.Join(s_timeout).ShouldBeTrue("a concurrent caller did not finish");
        }
        failures.OfType<Exception>().ShouldBeEmpty();
        return results;
    }

    private static long MeasureAllocations(Func<bool> read)
    {
        for (var index = 0; index < 100; index++)
        {
            read().ShouldBeTrue();
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var hits = 0;
        for (var index = 0; index < 10_000; index++)
        {
            hits += read() ? 1 : 0;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Volatile.Write(ref s_sink, hits);
        return allocated;
    }

    private sealed class DelegateFragmentAttribute(Action<ControlPackageRegistrationBuilder> add) : ControlRegistrationFragmentAttribute
    {
        public override string FragmentId => "Test/Button";
        public override void Add(ControlPackageRegistrationBuilder builder) => add(builder);
    }
}
