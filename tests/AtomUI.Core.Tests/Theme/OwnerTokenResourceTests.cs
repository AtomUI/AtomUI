using System.Reflection;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class OwnerTokenResourceTests
{
    private static readonly ControlTokenIdentity s_lineEditIdentity = new("AtomUI", "LineEdit");
    private static readonly ControlTokenIdentity s_buttonIdentity = new("AtomUI", "Button");
    private static readonly Color s_lineEditBorder = Color.Parse("#FF0000");
    private static readonly Color s_buttonBorder = Color.Parse("#00FF00");
    private static int s_lookupHits;

    [Fact]
    public void ProvideValue_Returns_Binding_Without_Resolving_Owner()
    {
        var value = new OwnerTokenResourceExtension(SharedTokenKind.ColorBorder)
            .ProvideValue(new ThrowingServiceProvider());

        value.ShouldBeAssignableTo<BindingBase>();
        value.ShouldNotBeOfType<DynamicResourceExtension>();
    }

    [Fact]
    public void Primitive_Reads_Owner_Effective_Global_Value()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();
            var lineEdit = fixture.CreateOwner(typeof(LineEditThemeTestControl), primitive);

            fixture.Show(lineEdit);

            BorderColor(primitive).ShouldBe(s_lineEditBorder);
            fixture.GlobalBorderColor().ShouldNotBe(s_lineEditBorder);
        });
    }

    [Fact]
    public void Primitive_Without_Owner_Falls_Back_To_Global()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();

            fixture.Show(primitive);

            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBeNull();
            BorderColor(primitive).ShouldBe(fixture.GlobalBorderColor());
            BorderColor(primitive).ShouldNotBe(s_lineEditBorder);
        });
    }

    [Fact]
    public void Owner_Change_Retargets_Binding()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(new ThemeConfigBuilder()
                                            .WithControl(s_lineEditIdentity, BorderConfig("#FF0000"))
                                            .WithControl(s_buttonIdentity, BorderConfig("#00FF00"))
                                            .Build());
            var primitive = fixture.CreatePrimitive();
            var lineEditHost = new Decorator();
            var buttonHost = new Decorator();
            var panel = new StackPanel
            {
                Children =
                {
                    fixture.CreateOwner(typeof(LineEditThemeTestControl), lineEditHost),
                    fixture.CreateOwner(typeof(ButtonThemeTestControl), buttonHost)
                }
            };
            fixture.Show(panel);

            lineEditHost.Child = primitive;
            BorderColor(primitive).ShouldBe(s_lineEditBorder);

            lineEditHost.Child = null;
            buttonHost.Child = primitive;
            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_buttonIdentity);
            BorderColor(primitive).ShouldBe(s_buttonBorder);

            buttonHost.Child = null;
            panel.Children.Add(primitive);
            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBeNull();
            BorderColor(primitive).ShouldBe(fixture.GlobalBorderColor());

            panel.Children.Remove(primitive);
            lineEditHost.Child = primitive;
            BorderColor(primitive).ShouldBe(s_lineEditBorder);
            OwnerBindingHandlerCount(primitive).ShouldBe(3, "retargeting must not accumulate subscriptions");
        });
    }

    [Fact]
    public void Removing_The_Setter_Detaches_The_Per_Target_Binding()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();
            fixture.Show(fixture.CreateOwner(typeof(LineEditThemeTestControl), primitive));
            OwnerBindingHandlerCount(primitive).ShouldBe(3);

            primitive.Theme = null;
            Dispatcher.UIThread.RunJobs();

            OwnerBindingHandlerCount(primitive).ShouldBe(0);
            primitive.BorderBrush.ShouldBeNull();

            fixture.Apply(LineEditBorderConfig("#0000FF"), ThemeTransitionReason.UserRequest);

            primitive.BorderBrush.ShouldBeNull();
        });
    }

    [Fact]
    public void Theme_Publish_Refreshes_Owner_Value()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();
            fixture.Show(fixture.CreateOwner(typeof(LineEditThemeTestControl), primitive));
            BorderColor(primitive).ShouldBe(s_lineEditBorder);

            fixture.Apply(LineEditBorderConfig("#0000FF"), ThemeTransitionReason.UserRequest);

            BorderColor(primitive).ShouldBe(Color.Parse("#0000FF"));

            fixture.Apply(new ThemeConfigBuilder().Build(), ThemeTransitionReason.UserRequest);

            BorderColor(primitive).ShouldBe(fixture.GlobalBorderColor());
        });
    }

    [Fact]
    public void Popup_Content_Inherits_Owner()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();
            var popup = new Popup { Child = primitive };
            var lineEdit = fixture.CreateOwner(typeof(LineEditThemeTestControl), popup);
            fixture.EnablePopupOverlayLayer();
            fixture.Show(lineEdit);

            popup.PlacementTarget = lineEdit;
            popup.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            try
            {
                primitive.GetVisualAncestors().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
                primitive.GetVisualAncestors().ShouldNotContain(lineEdit);
                primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
                BorderColor(primitive).ShouldBe(s_lineEditBorder);
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [Fact]
    public void Unregistered_Owner_Falls_Back_To_Global_Without_Throwing()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();
            primitive.SetValue(ThemeScope.TokenOwnerProperty, new ControlTokenIdentity("AtomUI", "Missing"));

            fixture.Show(primitive);

            BorderColor(primitive).ShouldBe(fixture.GlobalBorderColor());

            fixture.Apply(LineEditBorderConfig("#0000FF"), ThemeTransitionReason.UserRequest);

            BorderColor(primitive).ShouldBe(fixture.GlobalBorderColor());
        });
    }

    [Fact]
    public void Query_Hot_Path_Does_Not_Allocate()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new Fixture(LineEditBorderConfig("#FF0000"));
            var primitive = fixture.CreatePrimitive();
            fixture.Show(fixture.CreateOwner(typeof(LineEditThemeTestControl), primitive));
            fixture.Apply(LineEditBorderConfig("#0000FF"), ThemeTransitionReason.UserRequest);
            BorderColor(primitive).ShouldBe(Color.Parse("#0000FF"));

            var ownerKey = OwnerTokenBinding.GetResourceKey(s_lineEditIdentity, SharedTokenKind.ColorBorder);
            var globalKey = OwnerTokenBinding.GetResourceKey(null, SharedTokenKind.ColorBorder);
            var provider = fixture.Manager.RootContext.ResourceProvider;

            MeasureAllocations(() => provider.TryGetResource(ownerKey, null, out _))
                .ShouldBe(0, "owner effective global lookup must not allocate");
            MeasureAllocations(() => provider.TryGetResource(globalKey, null, out _))
                .ShouldBe(0, "global fallback lookup must not allocate");
            MeasureAllocations(() => primitive.TryFindResource(ownerKey, primitive.ActualThemeVariant, out _))
                .ShouldBe(0, "refresh lookup through the primitive's resource chain must not allocate");
        });
    }

    private static ControlThemeConfig BorderConfig(string color) =>
        new ControlThemeConfigBuilder()
            .WithToken("ColorBorder", color)
            .Build();

    private static ThemeConfig LineEditBorderConfig(string color) =>
        new ThemeConfigBuilder()
            .WithControl(s_lineEditIdentity, BorderConfig(color))
            .Build();

    /// <summary>
    /// Counts the target's PropertyChanged, ResourcesChanged and ActualThemeVariantChanged handlers that
    /// belong to an <see cref="OwnerTokenBinding"/>; one live binding owns exactly one of each.
    /// </summary>
    private static int OwnerBindingHandlerCount(StyledElement target)
    {
        var propertyChanged = EventHandlers(typeof(AvaloniaObject), target, "_propertyChanged");
        var resourcesChanged = EventHandlers(typeof(StyledElement), target, nameof(StyledElement.ResourcesChanged));
        var themeVariantChanged = EventHandlers(typeof(StyledElement), target, nameof(StyledElement.ActualThemeVariantChanged));
        return propertyChanged.Concat(resourcesChanged)
                              .Concat(themeVariantChanged)
                              .Count(static handler => handler.Target is OwnerTokenBinding);
    }

    private static Delegate[] EventHandlers(Type declaringType, StyledElement target, string fieldName)
    {
        var field = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (field.GetValue(target) as Delegate)?.GetInvocationList() ?? [];
    }

    private static Color BorderColor(TemplatedControl primitive) =>
        primitive.BorderBrush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;

    private static long MeasureAllocations(Func<bool> lookup)
    {
        for (var index = 0; index < 100; index++)
        {
            lookup().ShouldBeTrue();
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var hits = 0;
        for (var index = 0; index < 10_000; index++)
        {
            hits += lookup() ? 1 : 0;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Volatile.Write(ref s_lookupHits, hits);
        return allocated;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly ThemeSchemaRegistry _registry;
        private readonly Window _window;

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
            _window = new Window();
            _window.Resources.MergedDictionaries.Add(Manager.Resources);
        }

        internal ThemeManager Manager { get; }

        internal void Apply(ThemeConfig config, ThemeTransitionReason reason)
        {
            Manager.ApplyThemeAsync(new ThemeRequest("Test", config, reason))
                   .GetAwaiter()
                   .GetResult()
                   .Status.ShouldBe(ThemeTransitionStatus.Committed);
            Dispatcher.UIThread.RunJobs();
        }

        internal FrameTestPrimitive CreatePrimitive() => new()
        {
            // AXAML 等价写法：<Setter Property="BorderBrush" Value="{atom:OwnerTokenResource ColorBorder}" />
            Theme = new ControlTheme(typeof(FrameTestPrimitive))
            {
                Setters =
                {
                    new Setter(
                        TemplatedControl.BorderBrushProperty,
                        new OwnerTokenResourceExtension(SharedTokenKind.ColorBorder)
                            .ProvideValue(new ThrowingServiceProvider()))
                }
            }
        };

        internal TemplatedControl CreateOwner(Type controlType, Control templateContent)
        {
            var theme = new ControlTheme(controlType)
            {
                Setters =
                {
                    new Setter(
                        TemplatedControl.TemplateProperty,
                        new FuncControlTemplate((_, _) => templateContent))
                }
            };
            Manager.NotifyControlThemeResolved(controlType, theme);
            var owner = (TemplatedControl)Activator.CreateInstance(controlType)!;
            owner.Theme = theme;
            return owner;
        }

        /// <summary>
        /// Headless popups are overlay popups; they need the window's PART_VisualLayerManager, which an
        /// untemplated test window lacks. The popup content is then hosted by the overlay layer, outside the
        /// owner's visual subtree.
        /// </summary>
        internal void EnablePopupOverlayLayer()
        {
            _window.Template = new FuncControlTemplate<Window>((window, scope) =>
                new VisualLayerManager
                {
                    Name  = "PART_VisualLayerManager",
                    Child = new ContentPresenter
                    {
                        Name = "PART_ContentPresenter",
                        [!ContentPresenter.ContentProperty] = window[!ContentControl.ContentProperty]
                    }.RegisterInNameScope(scope)
                }.RegisterInNameScope(scope));
        }

        internal void Show(Control content)
        {
            _window.Content = content;
            _window.Show();
            (content as TemplatedControl)?.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
        }

        internal Color GlobalBorderColor()
        {
            _window.TryFindResource(SharedTokenKind.ColorBorder, _window.ActualThemeVariant, out var value)
                   .ShouldBeTrue();
            return value.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
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

    private sealed class ThrowingServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            throw new InvalidOperationException($"OwnerTokenResource must not request '{serviceType}'.");
        }
    }
}
