using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// <see cref="TokenTransitions"/>: transition durations follow the Effective Global duration Tokens of the control that
/// receives the transitions, live, instead of the application value frozen at load.
/// </summary>
[Collection(ThemeConfigProviderTestCollection.Name)]
public class TransitionDurationTests
{
    private static readonly ControlTokenIdentity s_lineEditIdentity = new("AtomUI", "LineEdit");
    private static readonly ControlTokenIdentity s_buttonIdentity = new("AtomUI", "Button");
    private static readonly TimeSpan s_mid = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan s_slow = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan s_literal = TimeSpan.FromMilliseconds(70);
    private static readonly TimeSpan s_override = TimeSpan.FromMilliseconds(500);
    private const string OverrideValue = "00:00:00.5000000";
    private static int s_sink;

    [Fact]
    public void ProvideValue_Returns_Binding_Without_Building_Or_Resolving()
    {
        var built = 0;
        var markup = new TokenTransitions { Content = Content(() => built++) };

        var value = markup.ProvideValue(new TemplateContextServiceProvider(null));

        value.ShouldBeAssignableTo<CompiledBinding>()!.Priority.ShouldBe(BindingPriority.LocalValue);
        built.ShouldBe(0, "the content is built when a target first resolves its durations, not while loading markup");
    }

    [Fact]
    public void ProvideValue_In_Control_Template_Uses_Template_Priority()
    {
        var value = new TokenTransitions { Content = Content() }
            .ProvideValue(new TemplateContextServiceProvider(new ControlTemplateMarker()));

        value.ShouldBeAssignableTo<CompiledBinding>()!.Priority.ShouldBe(BindingPriority.Template);
    }

    [Fact]
    public void ProvideValue_Without_Content_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            new TokenTransitions().ProvideValue(new TemplateContextServiceProvider(null)));
    }

    [Fact]
    public void Declared_Durations_Resolve_And_Literal_Durations_Are_Kept()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var button = fixture.CreateOwner(typeof(ButtonThemeTestControl));
            fixture.Show(button);

            Durations(button).ShouldBe((s_mid, s_slow, s_literal));
        });
    }

    [Fact]
    public void Root_MotionDurationMid_Override_Changes_The_Transition_Duration()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var button = fixture.CreateOwner(typeof(ButtonThemeTestControl));
            var unowned = fixture.CreateAnimated();
            fixture.Show(new StackPanel { Children = { button, unowned } });
            Durations(button).Mid.ShouldBe(s_mid);

            fixture.Apply(RootMotionConfig(), ThemeTransitionReason.UserRequest);

            Durations(button).ShouldBe((s_override, s_slow, s_literal));
            Durations(unowned).Mid.ShouldBe(s_override);
        });
    }

    [Fact]
    public void Root_EnableMotion_False_Yields_Zero_Durations()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var button = fixture.CreateOwner(typeof(ButtonThemeTestControl));
            var unowned = fixture.CreateAnimated();
            fixture.Show(new StackPanel { Children = { button, unowned } });

            fixture.Apply(new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build(),
                ThemeTransitionReason.UserRequest);

            Durations(button).ShouldBe((TimeSpan.Zero, TimeSpan.Zero, s_literal));
            Durations(unowned).ShouldBe((TimeSpan.Zero, TimeSpan.Zero, s_literal));
        });
    }

    [Fact]
    public void Controls_Button_Override_Changes_Only_The_Button_Transitions()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var primitive = fixture.CreateAnimated();
            var button = fixture.CreateOwner(typeof(ButtonThemeTestControl), primitive);
            var lineEdit = fixture.CreateOwner(typeof(LineEditThemeTestControl));
            var unowned = fixture.CreateAnimated();
            fixture.Show(new StackPanel { Children = { button, lineEdit, unowned } });

            fixture.Apply(ButtonMotionConfig(), ThemeTransitionReason.UserRequest);

            Durations(button).Mid.ShouldBe(s_override);
            Durations(primitive).Mid.ShouldBe(s_override, "a primitive in the Button's template follows its owner");
            Durations(lineEdit).Mid.ShouldBe(s_mid);
            Durations(unowned).Mid.ShouldBe(s_mid);
            button.Transitions.ShouldBeSameAs(primitive.Transitions, "equal durations share one built instance");
            lineEdit.Transitions.ShouldBeSameAs(unowned.Transitions);
            button.Transitions.ShouldNotBeSameAs(lineEdit.Transitions);
        });
    }

    [Fact]
    public void Theme_Switch_Leaves_No_Stale_Duration()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(ButtonMotionConfig());
            var button = fixture.CreateOwner(typeof(ButtonThemeTestControl));
            var lineEdit = fixture.CreateOwner(typeof(LineEditThemeTestControl));
            fixture.Show(new StackPanel { Children = { button, lineEdit } });
            Durations(button).Mid.ShouldBe(s_override);

            fixture.Apply(new ThemeConfigBuilder().Build(), ThemeTransitionReason.UserRequest);

            Durations(button).Mid.ShouldBe(s_mid);
            button.Transitions.ShouldBeSameAs(lineEdit.Transitions);

            fixture.Apply(new ThemeConfigBuilder().WithToken("EnableMotion", "false").Build(),
                ThemeTransitionReason.UserRequest);
            fixture.Apply(new ThemeConfigBuilder().Build(), ThemeTransitionReason.UserRequest);

            Durations(button).Mid.ShouldBe(s_mid);
            Durations(lineEdit).Mid.ShouldBe(s_mid);
        });
    }

    [Fact]
    public void Owner_Change_Retargets_The_Durations()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(ButtonMotionConfig());
            var primitive = fixture.CreateAnimated();
            var buttonHost = new Decorator();
            var lineEditHost = new Decorator();
            fixture.Show(new StackPanel
            {
                Children =
                {
                    fixture.CreateOwner(typeof(ButtonThemeTestControl), buttonHost),
                    fixture.CreateOwner(typeof(LineEditThemeTestControl), lineEditHost)
                }
            });

            buttonHost.Child = primitive;
            Dispatcher.UIThread.RunJobs();
            Durations(primitive).Mid.ShouldBe(s_override);

            buttonHost.Child = null;
            lineEditHost.Child = primitive;
            Dispatcher.UIThread.RunJobs();
            Durations(primitive).Mid.ShouldBe(s_mid);
            BindingHandlerCount(primitive).ShouldBe(3, "retargeting must not accumulate subscriptions");
        });
    }

    [Fact]
    public void Detached_Target_Has_No_Transitions_Until_It_Resolves_Its_Tokens()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var animated = fixture.CreateAnimated();
            animated.ApplyStyling();

            animated.Transitions.ShouldBeNull();

            fixture.Show(animated);
            Durations(animated).Mid.ShouldBe(s_mid);
        });
    }

    [Fact]
    public void Removing_The_Setter_Detaches_The_Per_Target_Binding()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var animated = fixture.CreateAnimated();
            fixture.Show(animated);
            BindingHandlerCount(animated).ShouldBe(3);

            animated.Theme = null;
            Dispatcher.UIThread.RunJobs();

            BindingHandlerCount(animated).ShouldBe(0);
            animated.Transitions.ShouldBeNull();
        });
    }

    [Fact]
    public void Detached_Control_Is_Collectable()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var references = ShowAndDetach(fixture);
            fixture.Apply(RootMotionConfig(), ThemeTransitionReason.UserRequest);

            Collect();

            references.Control.IsAlive.ShouldBeFalse("the shared factory or built transitions must not retain targets");
            references.Primitive.IsAlive.ShouldBeFalse();
        });
    }

    [Fact]
    public void Refresh_With_Unchanged_Durations_Does_Not_Allocate()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(ButtonMotionConfig());
            var primitive = fixture.CreateAnimated();
            fixture.Show(fixture.CreateOwner(typeof(ButtonThemeTestControl), primitive));
            Durations(primitive).Mid.ShouldBe(s_override);
            var binding = Bindings(primitive).ShouldHaveSingleItem();
            var transitions = primitive.Transitions;

            for (var index = 0; index < 100; index++)
            {
                binding.Refresh();
            }
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 10_000; index++)
            {
                binding.Refresh();
            }
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Volatile.Write(ref s_sink, primitive.Transitions!.Count);

            allocated.ShouldBe(0, "re-resolving the published durations must not allocate");
            primitive.Transitions.ShouldBeSameAs(transitions);
        });
    }

    [Fact]
    public void Shared_Variants_Are_Bounded_And_Later_Vectors_Still_Resolve()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder().Build());
            var animated = fixture.CreateAnimated();
            fixture.Show(animated);

            for (var step = 1; step <= TokenTransitionsFactory.MaxSharedVariants + 2; step++)
            {
                fixture.Apply(new ThemeConfigBuilder()
                              .WithToken("MotionDurationMid", TimeSpan.FromMilliseconds(step * 10).ToString())
                              .Build(),
                    ThemeTransitionReason.UserRequest);
                Durations(animated).Mid.ShouldBe(TimeSpan.FromMilliseconds(step * 10));
            }

            fixture.Factory.SharedVariantCount.ShouldBe(TokenTransitionsFactory.MaxSharedVariants);
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Control, WeakReference Primitive) ShowAndDetach(Fixture fixture)
    {
        var primitive = fixture.CreateAnimated();
        var button = fixture.CreateOwner(typeof(ButtonThemeTestControl), primitive);
        var host = new StackPanel { Children = { button } };
        fixture.Show(host);
        Durations(button).Mid.ShouldBe(s_mid);
        Durations(primitive).Mid.ShouldBe(s_mid);
        var shared = button.Transitions!;
        CollectionChangedHandlerCount(shared).ShouldBe(2, "both attached controls run the shared transitions");
        host.Children.Clear();
        Dispatcher.UIThread.RunJobs();
        // The compositor keeps detached visuals until the next render pass.
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
        CollectionChangedHandlerCount(shared).ShouldBe(0,
            "the theme-lifetime shared transitions must not keep handlers of detached controls");
        return (new WeakReference(button), new WeakReference(primitive));
    }

    private static int CollectionChangedHandlerCount(Transitions transitions)
    {
        var field = typeof(Avalonia.Collections.AvaloniaList<ITransition>)
            .GetField("_collectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (field.GetValue(transitions) as Delegate)?.GetInvocationList().Length ?? 0;
    }

    private static void Collect()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private static ThemeConfig RootMotionConfig() =>
        new ThemeConfigBuilder().WithToken("MotionDurationMid", OverrideValue).Build();

    private static ThemeConfig ButtonMotionConfig() =>
        new ThemeConfigBuilder()
            .WithControl(s_buttonIdentity,
                new ControlThemeConfigBuilder().WithToken("MotionDurationMid", OverrideValue).Build())
            .Build();

    /// <summary>
    /// The durations of the three transitions the test content declares: MotionDurationMid on Opacity,
    /// MotionDurationSlow on Width and a literal 70ms on Height.
    /// </summary>
    private static (TimeSpan Mid, TimeSpan Slow, TimeSpan Literal) Durations(Animatable target)
    {
        var transitions = target.Transitions.ShouldNotBeNull().OfType<TransitionBase>().ToArray();
        return (Duration(transitions, Visual.OpacityProperty),
            Duration(transitions, Layoutable.WidthProperty),
            Duration(transitions, Layoutable.HeightProperty));
    }

    private static TimeSpan Duration(TransitionBase[] transitions, AvaloniaProperty property) =>
        transitions.Single(transition => transition.Property == property).Duration;

    /// <summary>
    /// AXAML equivalent:
    /// <c>&lt;atom:TokenTransitions&gt;&lt;Transitions&gt;&lt;DoubleTransition Property="Opacity"
    /// atom:TokenTransitions.Duration="MotionDurationMid" /&gt;…&lt;/Transitions&gt;&lt;/atom:TokenTransitions&gt;</c>,
    /// whose compiled content is a deferred factory.
    /// </summary>
    private static Func<IServiceProvider?, object?> Content(Action? onBuild = null) => _ =>
    {
        onBuild?.Invoke();
        var opacity = new DoubleTransition { Property = Visual.OpacityProperty };
        TokenTransitions.SetDuration(opacity, SharedTokenKind.MotionDurationMid);
        var width = new DoubleTransition { Property = Layoutable.WidthProperty };
        TokenTransitions.SetDuration(width, SharedTokenKind.MotionDurationSlow);
        var height = new DoubleTransition { Property = Layoutable.HeightProperty, Duration = s_literal };
        return new TemplateResult<Transitions>(new Transitions { opacity, width, height }, new NameScope());
    };

    private static IEnumerable<TransitionDurationBinding> Bindings(StyledElement target) =>
        EventHandlers(typeof(StyledElement), target, nameof(StyledElement.ResourcesChanged))
            .Select(static handler => handler.Target)
            .OfType<TransitionDurationBinding>();

    private static int BindingHandlerCount(StyledElement target)
    {
        var propertyChanged = EventHandlers(typeof(AvaloniaObject), target, "_propertyChanged");
        var resourcesChanged = EventHandlers(typeof(StyledElement), target, nameof(StyledElement.ResourcesChanged));
        var themeVariantChanged = EventHandlers(typeof(StyledElement), target, nameof(StyledElement.ActualThemeVariantChanged));
        return propertyChanged.Concat(resourcesChanged)
                              .Concat(themeVariantChanged)
                              .Count(static handler => handler.Target is TransitionDurationBinding);
    }

    private static Delegate[] EventHandlers(Type declaringType, StyledElement target, string fieldName)
    {
        var field = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (field.GetValue(target) as Delegate)?.GetInvocationList() ?? [];
    }

    private sealed class Fixture : IDisposable
    {
        private readonly Window _window;
        private readonly ThemeSchemaRegistry _registry;
        private readonly TokenTransitions _markup;
        private readonly object _transitionsValue;

        internal Fixture(ThemeConfig config)
        {
            _registry = TypedThemeSnapshotCacheTests.CreateRegistry([
                new ControlTokenDescriptor(typeof(LineEditThemeTestControl), s_lineEditIdentity),
                new ControlTokenDescriptor(typeof(ButtonThemeTestControl), s_buttonIdentity)
            ]);
            Manager = new ThemeManager(
                static () => true,
                (request, _, _) => ValueTask.FromResult(Prepare(_registry, request.Config)));
            Apply(config, ThemeTransitionReason.Startup);
            // One markup instance, as one Setter in a theme: every target shares it.
            _markup = new TokenTransitions { Content = Content() };
            _transitionsValue = _markup.ProvideValue(new TemplateContextServiceProvider(null));
            _window = new Window();
            _window.Resources.MergedDictionaries.Add(Manager.Resources);
            _window.SetValue(ThemeScope.ContextProperty, Manager.RootContext);
        }

        internal ThemeManager Manager { get; }

        internal TokenTransitionsFactory Factory =>
            (TokenTransitionsFactory)typeof(TokenTransitions)
                                     .GetField("_factory", BindingFlags.Instance | BindingFlags.NonPublic)!
                                     .GetValue(_markup)!;

        internal void Apply(ThemeConfig config, ThemeTransitionReason reason)
        {
            Manager.ApplyThemeAsync(new ThemeRequest("Test", config, reason))
                   .GetAwaiter()
                   .GetResult()
                   .Status.ShouldBe(ThemeTransitionStatus.Committed);
            Dispatcher.UIThread.RunJobs();
        }

        /// <summary>A control without Token identity whose theme installs the token transitions.</summary>
        internal FrameTestPrimitive CreateAnimated() => new()
        {
            Theme = new ControlTheme(typeof(FrameTestPrimitive))
            {
                Setters = { new Setter(Animatable.TransitionsProperty, _transitionsValue) }
            }
        };

        /// <summary>A registered control whose theme installs the token transitions on the control itself.</summary>
        internal TemplatedControl CreateOwner(Type controlType, Control? templateContent = null)
        {
            var theme = new ControlTheme(controlType)
            {
                Setters =
                {
                    new Setter(Animatable.TransitionsProperty, _transitionsValue),
                    new Setter(
                        TemplatedControl.TemplateProperty,
                        new FuncControlTemplate((_, _) => templateContent ?? new Border()))
                }
            };
            Manager.NotifyControlThemeResolved(controlType, theme);
            var owner = (TemplatedControl)Activator.CreateInstance(controlType)!;
            owner.Theme = theme;
            return owner;
        }

        internal void Show(Control content)
        {
            _window.Content = content;
            _window.Show();
            (content as TemplatedControl)?.ApplyTemplate();
            if (content is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    (child as TemplatedControl)?.ApplyTemplate();
                }
            }
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
            _window.Resources.MergedDictionaries.Remove(Manager.Resources);
        }

        private static ThemeTransactionPreparation Prepare(ThemeSchemaRegistry registry, ThemeConfig? config)
        {
            registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var defaultAlgorithm).ShouldBeTrue();
            var definition = new PreparedThemeDefinition(
                "TestTheme",
                "Test Theme",
                ThemeAppearance.Light,
                ThemeAppearance.Light,
                true,
                [defaultAlgorithm!],
                Array.Empty<BoundTokenValue>(),
                Array.Empty<ControlThemeDefinition>(),
                new ThemeSourceLocation("test", 1, 1, "/Theme"));
            var normalized = ThemeConfigNormalizer.Normalize(config ?? new ThemeConfigBuilder().Build(), registry);
            normalized.Success.ShouldBeTrue();
            var effective = ThemeConfigMerger.Merge(
                ThemeCompiler.CreateDefinitionDefaults(definition, registry),
                null,
                normalized.Config!).EffectiveConfig;
            var input = new ThemeCompileInput(
                definition,
                new ThemeDefinitionRevision("test", "1", effective.Fingerprint.ToString()),
                effective,
                registry);
            var result = new ThemeCompiler().Compile(input);
            result.Success.ShouldBeTrue();
            return ThemeTransactionPreparation.Succeeded(result.Snapshot!, ThemeSnapshotCacheKey.Create(input));
        }
    }

    /// <summary>Answers only the control-template context query; everything else is resolved per target later.</summary>
    private sealed class TemplateContextServiceProvider(IAvaloniaXamlIlControlTemplateProvider? templateProvider)
        : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IAvaloniaXamlIlControlTemplateProvider))
            {
                return templateProvider;
            }
            throw new InvalidOperationException($"TokenTransitions must not request '{serviceType}'.");
        }
    }

    private sealed class ControlTemplateMarker : IAvaloniaXamlIlControlTemplateProvider
    {
    }
}
