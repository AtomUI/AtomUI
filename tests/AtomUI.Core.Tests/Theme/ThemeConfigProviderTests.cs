using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeConfigProviderTests
{
    [Fact]
    public void First_Attach_Synchronously_Publishes_Context_Resources_And_Variant()
    {
        var manager = CreateInitializedManager();
        var provider = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#ff0000"),
            Child = new Border()
        };

        using var root = Attach(manager, provider);

        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        context.Snapshot.Global<Color>(nameof(DesignToken.ColorPrimary)).ShouldBe(Color.Parse("#ff0000"));
        context.ResourceProvider.ShouldBeSameAs(GetTokenResourceProvider(provider));
        provider.RequestedThemeVariant.ShouldBe(ThemeVariant.Light);
    }

    [Fact]
    public void First_Attach_Continues_Context_Publish_When_Variant_Observer_Fails()
    {
        var manager = CreateInitializedManager();
        var provider = new ThrowingVariantThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#ff0000"),
            Child = new Border()
        };
        ThemeChangedEventArgs? observed = null;
        provider.ThemeChanged += (_, args) => observed = args;

        using var root = Attach(manager, provider);

        provider.ContextPublished.ShouldBeTrue();
        observed.ShouldNotBeNull();
        observed!.PublishDiagnostics.ShouldContain(diagnostic =>
            diagnostic.Severity == ThemeDiagnosticSeverity.Warning &&
            diagnostic.Message.Contains("variant update failed", StringComparison.Ordinal));
    }

    [Fact]
    public void Config_Replacement_Updates_The_Stable_Context_And_Resource_Provider()
    {
        var manager = CreateInitializedManager();
        var provider = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#ff0000"),
            Child = new Border()
        };
        using var root = Attach(manager, provider);
        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        var resourceProvider = GetTokenResourceProvider(provider);

        provider.Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#00b96b");

        provider.GetValue(ThemeScope.ContextProperty).ShouldBeSameAs(context);
        GetTokenResourceProvider(provider).ShouldBeSameAs(resourceProvider);
        context.Snapshot.Global<Color>(nameof(DesignToken.ColorPrimary)).ShouldBe(Color.Parse("#00b96b"));
    }

    [Fact]
    public void Local_ThemeChanged_Uses_A_Local_Transition_State()
    {
        var manager = CreateInitializedManager();
        var provider = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#ff0000"),
            Child = new Border()
        };
        using var root = Attach(manager, provider);
        var rootTransitionId = manager.CurrentTheme!.TransitionId;
        ThemeChangedEventArgs? observed = null;
        provider.ThemeChanged += (_, args) => observed = args;

        provider.Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#00b96b");

        observed.ShouldNotBeNull();
        observed!.Request.Reason.ShouldBe(ThemeTransitionReason.LocalConfigChanged);
        observed.State.TransitionId.ShouldBeGreaterThan(rootTransitionId);
    }

    [Fact]
    public void Nested_Providers_Inherit_The_Parent_Effective_Config()
    {
        var manager = CreateInitializedManager();
        var child = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#00b96b"),
            Child = new Border()
        };
        var parent = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.BorderRadius), "12"),
            Child = child
        };

        using var root = Attach(manager, parent);

        var snapshot = child.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull().Snapshot;
        snapshot.Global<Color>(nameof(DesignToken.ColorPrimary)).ShouldBe(Color.Parse("#00b96b"));
        snapshot.Global<CornerRadius>(nameof(DesignToken.BorderRadius)).ShouldBe(new CornerRadius(12));
    }

    [Fact]
    public void Empty_Inherited_Provider_Reuses_The_Parent_Snapshot()
    {
        var manager = CreateInitializedManager();
        typeof(ThemeManager)
            .GetField("_themeSnapshotCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(manager, new ThemeSnapshotCache(new ThemeCacheOptions(SnapshotRetainedBytesLimit: 1)));
        var child = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child = new Border()
        };
        var parent = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.BorderRadius), "12"),
            Child = child
        };

        using var root = Attach(manager, parent);

        var parentContext = parent.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        var childContext = child.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        childContext.ShouldNotBeSameAs(parentContext);
        childContext.ResourceProvider.ShouldNotBeSameAs(parentContext.ResourceProvider);
        childContext.Snapshot.ShouldBeSameAs(parentContext.Snapshot);
    }

    [Fact]
    public void Equivalent_Inherited_Provider_Reuses_The_Parent_Snapshot()
    {
        var manager = CreateInitializedManager();
        typeof(ThemeManager)
            .GetField("_themeSnapshotCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(manager, new ThemeSnapshotCache(new ThemeCacheOptions(SnapshotRetainedBytesLimit: 1)));
        var child = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.BorderRadius), "12"),
            Child = new Border()
        };
        var parent = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.BorderRadius), "12"),
            Child = child
        };

        using var root = Attach(manager, parent);

        var parentSnapshot = parent.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull().Snapshot;
        child.GetValue(ThemeScope.ContextProperty)
             .ShouldNotBeNull()
             .Snapshot.ShouldBeSameAs(parentSnapshot);
    }

    [Fact]
    public async Task Root_Transition_Does_Not_Notify_A_Local_Resource_Provider_Directly()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var prepared = CreatePrepared();
            var changedPrepared = CreatePrepared(nameof(DesignToken.ColorPrimary), "#00b96b");
            var manager = new ThemeManager(
                static () => true,
                (request, _, _) => ValueTask.FromResult(
                    request.ThemeId == "Changed" ? changedPrepared : prepared));
            manager.ConfigureStartup(
                new ThemeRequest("Test", null, ThemeTransitionReason.Startup),
                null,
                null,
                null);
            manager.InitializeApplication(Application.Current!);
            var provider = new ThemeConfigProvider
            {
                Config = new ThemeConfigBuilder().Build(),
                Child = new Border()
            };
            using var root = Attach(manager, provider);
            var notifications = 0;
            ((Avalonia.Controls.IResourceHost)provider).ResourcesChanged += (_, _) => notifications++;

            await manager.ApplyThemeAsync(
                new ThemeRequest("Changed", null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);

            notifications.ShouldBe(0);
        });
    }

    [Fact]
    public void Inherit_False_Uses_Library_Defaults_Instead_Of_Parent_Overrides()
    {
        var manager = CreateInitializedManager();
        var child = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().WithInherit(false).Build(),
            Child = new Border()
        };
        var parent = new ThemeConfigProvider
        {
            Config = ConfigWithToken(nameof(DesignToken.BorderRadius), "12"),
            Child = child
        };

        using var root = Attach(manager, parent);

        child.GetValue(ThemeScope.ContextProperty)
             .ShouldNotBeNull()
             .Snapshot.Global<CornerRadius>(nameof(DesignToken.BorderRadius))
             .ShouldBe(new CornerRadius(6));
    }

    [Fact]
    public void Inherit_False_Uses_Library_Defaults_Instead_Of_Root_Effective_Config()
    {
        var manager = CreateInitializedManager(
            CreatePrepared(nameof(DesignToken.BorderRadius), "20"));
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().WithInherit(false).Build(),
            Child = new Border()
        };

        using var root = Attach(manager, provider);

        provider.GetValue(ThemeScope.ContextProperty)
                .ShouldNotBeNull()
                .Snapshot.Global<CornerRadius>(nameof(DesignToken.BorderRadius))
                .ShouldBe(new CornerRadius(6));
    }

    [Fact]
    public void Local_Density_Keeps_The_Parent_Appearance()
    {
        var manager = CreateInitializedManager(CreatePreparedWithAxes(ThemeAppearance.Dark, null));
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithDensity(DesignDensity.Compact)
                     .Build(),
            Child = new Border()
        };

        using var root = Attach(manager, provider);

        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        context.Snapshot.Appearance.ShouldBe(ThemeAppearance.Dark);
        context.Snapshot.Variant.Density.ShouldBe(DesignDensity.Compact);
        provider.RequestedThemeVariant.ShouldBe(ThemeVariant.Dark);
    }

    [Fact]
    public void Local_Appearance_Overrides_The_Parent_Appearance()
    {
        var manager = CreateInitializedManager(CreatePreparedWithAxes(ThemeAppearance.Dark, DesignDensity.Compact));
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithAppearance(ThemeAppearance.Light)
                     .Build(),
            Child = new Border()
        };

        using var root = Attach(manager, provider);

        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        context.Snapshot.Appearance.ShouldBe(ThemeAppearance.Light);
        context.Snapshot.Variant.Density.ShouldBe(DesignDensity.Compact);
        provider.RequestedThemeVariant.ShouldBe(ThemeVariant.Light);
    }

    [Fact]
    public void Invalid_Config_Keeps_The_Published_Snapshot_And_Last_Valid_Config()
    {
        var manager = CreateInitializedManager();
        var valid = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#ff0000");
        var provider = new ThemeConfigProvider
        {
            Config = valid,
            Child = new Border()
        };
        using var root = Attach(manager, provider);
        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        var snapshot = context.Snapshot;
        ThemeChangeFailedEventArgs? failure = null;
        provider.ThemeChangeFailed += (_, args) => failure = args;

        provider.Config = ConfigWithToken("MissingToken", "1");

        failure.ShouldNotBeNull();
        context.Snapshot.ShouldBeSameAs(snapshot);
        manager.ScopeGraph.TryGetNode(provider, out var node).ShouldBeTrue();
        node!.LastValidConfig.ShouldBeSameAs(valid);
    }

    [Fact]
    public void Detach_And_Reattach_Uses_A_New_Registration_Without_Leaving_The_Old_Edge()
    {
        var manager = CreateInitializedManager();
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child = new Border()
        };
        var firstRoot = Attach(manager, provider);
        manager.ScopeGraph.TryGetNode(provider, out var firstNode).ShouldBeTrue();
        var firstId = firstNode!.RegistrationId;

        firstRoot.Child = null;
        manager.ScopeGraph.TryGetNode(provider, out _).ShouldBeFalse();
        using var secondRoot = Attach(manager, provider);

        manager.ScopeGraph.TryGetNode(provider, out var secondNode).ShouldBeTrue();
        secondNode!.RegistrationId.ShouldBeGreaterThan(firstId);
        firstRoot.Dispose();
    }

    [Fact]
    public void Config_Replacement_Does_Not_Retain_The_Previous_Immutable_Config()
    {
        var manager = CreateInitializedManager();
        var provider = new ThemeConfigProvider
        {
            Child = new Border()
        };
        using var root = Attach(manager, provider);

        var oldConfig = SetTemporaryConfig(provider);
        provider.Config = new ThemeConfigBuilder().Build();
        Collect();

        oldConfig.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Detached_Provider_Context_And_Resource_Provider_Are_Collectible()
    {
        var manager = CreateInitializedManager();
        var references = AttachAndDetachTemporaryProvider(manager);

        Collect();

        references.Provider.IsAlive.ShouldBeFalse();
        references.Context.IsAlive.ShouldBeFalse();
        references.ResourceProvider.IsAlive.ShouldBeFalse();
    }

    private static ThemeManager CreateInitializedManager()
    {
        return CreateInitializedManager(CreatePrepared());
    }

    private static ThemeManager CreateInitializedManager(ThemeTransactionPreparation prepared)
    {
        var manager = new ThemeManager(
            static () => true,
            (_, _, _) => ValueTask.FromResult(prepared));
        manager.ApplyThemeAsync(
                   new ThemeRequest("Test", null, ThemeTransitionReason.Startup))
               .GetAwaiter()
               .GetResult()
               .Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager;
    }

    private static ThemeTransactionPreparation CreatePrepared()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        var input = TypedThemeSnapshotCacheTests.CreateInput(registry);
        var snapshot = new ThemeCompiler().Compile(input).Snapshot!;
        return ThemeTransactionPreparation.Succeeded(snapshot, ThemeSnapshotCacheKey.Create(input));
    }

    private static ThemeTransactionPreparation CreatePrepared(string tokenName, string tokenValue)
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        registry.TryGetGlobalToken(tokenName, out var descriptor).ShouldBeTrue();
        var parsed = descriptor!.Parse(tokenValue);
        var token = new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
        var input = TypedThemeSnapshotCacheTests.CreateInput(registry, globalTokens: [token]);
        var snapshot = new ThemeCompiler().Compile(input).Snapshot!;
        return ThemeTransactionPreparation.Succeeded(snapshot, ThemeSnapshotCacheKey.Create(input));
    }

    private static ThemeTransactionPreparation CreatePreparedWithAxes(ThemeAppearance? appearance, DesignDensity? density)
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        var input = TypedThemeSnapshotCacheTests.CreateInput(registry, appearance: appearance, density: density);
        var snapshot = new ThemeCompiler().Compile(input).Snapshot!;
        return ThemeTransactionPreparation.Succeeded(snapshot, ThemeSnapshotCacheKey.Create(input));
    }

    private static ThemeConfig ConfigWithToken(string name, string value)
    {
        return new ThemeConfigBuilder().WithToken(name, value).Build();
    }

    /// <summary>
    /// Verifies that an attached provider binds inherited <see cref="TextElement.ForegroundProperty" /> to its scoped
    /// <c>ColorText</c> token, preserves local overrides, and releases the binding when detached.
    /// </summary>
    [Fact]
    public void Attached_Provider_Binds_Inherited_TextElement_Foreground_To_Scoped_ColorText_And_Preserves_Local_Override()
    {
        var manager = CreateInitializedManager();
        var childText = new TextBlock { Text = "Scoped" };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithAppearance(ThemeAppearance.Dark)
                     .WithToken(nameof(DesignToken.ColorText), "#f1f2f3")
                     .Build(),
            Child = childText
        };

        using var root = Attach(manager, provider);

        var scopedBrush = TextElement.GetForeground(provider).ShouldBeAssignableTo<ISolidColorBrush>().ShouldNotBeNull();
        scopedBrush.Color.ShouldBe(Color.Parse("#f1f2f3"));
        (childText.Foreground as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#f1f2f3"));

        provider.Config = new ThemeConfigBuilder()
                          .WithAppearance(ThemeAppearance.Light)
                          .WithToken(nameof(DesignToken.ColorText), "#112233")
                          .Build();
        (childText.Foreground as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#112233"));

        var localOverride = new SolidColorBrush(Colors.Crimson);
        TextElement.SetForeground(provider, localOverride);
        TextElement.GetForeground(provider).ShouldBeSameAs(localOverride);
        childText.Foreground.ShouldBeSameAs(localOverride);

        provider.ClearValue(TextElement.ForegroundProperty);
        (childText.Foreground as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#112233"));

        root.Child = null;
        provider.IsSet(TextElement.ForegroundProperty).ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that an attached provider binds inherited <see cref="TextElement.FontSizeProperty" /> and
    /// <see cref="TextElement.FontFamilyProperty" /> to its scoped tokens, preserves local overrides, and releases
    /// both bindings when detached.
    /// </summary>
    [Fact]
    public void Attached_Provider_Binds_Inherited_TextElement_FontSize_And_FontFamily_To_Scoped_Tokens_And_Preserves_Local_Overrides()
    {
        var manager = CreateInitializedManager();
        var childText = new TextBlock { Text = "Scoped" };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.FontSize), "12")
                     .WithToken(nameof(DesignToken.FontFamily), "Arial")
                     .Build(),
            Child = childText
        };

        using var root = Attach(manager, provider);

        TextElement.GetFontSize(provider).ShouldBe(12d);
        childText.FontSize.ShouldBe(12d);
        TextElement.GetFontFamily(provider).Name.ShouldBe("Arial");
        childText.FontFamily.Name.ShouldBe("Arial");

        provider.Config = new ThemeConfigBuilder()
                          .WithToken(nameof(DesignToken.FontSize), "16")
                          .WithToken(nameof(DesignToken.FontFamily), "Courier New")
                          .Build();
        childText.FontSize.ShouldBe(16d);
        childText.FontFamily.Name.ShouldBe("Courier New");

        TextElement.SetFontSize(provider, 20d);
        var localFamily = new FontFamily("Georgia");
        TextElement.SetFontFamily(provider, localFamily);
        childText.FontSize.ShouldBe(20d);
        childText.FontFamily.ShouldBeSameAs(localFamily);

        provider.ClearValue(TextElement.FontSizeProperty);
        provider.ClearValue(TextElement.FontFamilyProperty);
        childText.FontSize.ShouldBe(16d);
        childText.FontFamily.Name.ShouldBe("Courier New");

        root.Child = null;
        provider.IsSet(TextElement.FontSizeProperty).ShouldBeFalse();
        provider.IsSet(TextElement.FontFamilyProperty).ShouldBeFalse();
    }

    private static ThemeTokenResourceProvider GetTokenResourceProvider(ThemeConfigProvider provider)
    {
        return provider.Resources.MergedDictionaries
                       .OfType<ThemeTokenResourceProvider>()
                       .ShouldHaveSingleItem();
    }

    private static TestRoot Attach(ThemeManager manager, Control child)
    {
        var root = new TestRoot();
        root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
        root.Child = child;
        return root;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference SetTemporaryConfig(ThemeConfigProvider provider)
    {
        var config = ConfigWithToken(nameof(DesignToken.ColorPrimary), "#ff0000");
        provider.Config = config;
        return new WeakReference(config);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CollectibleReferences AttachAndDetachTemporaryProvider(ThemeManager manager)
    {
        var root = new TestRoot();
        root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child = new Border()
        };
        root.Child = provider;
        var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
        var resourceProvider = context.ResourceProvider;
        root.Child = null;
        provider.Child = null;
        return new CollectibleReferences(
            new WeakReference(provider),
            new WeakReference(context),
            new WeakReference(resourceProvider));
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

    private sealed class TestRoot : Decorator, ILogicalRoot, IDisposable
    {
        public void Dispose()
        {
            Child = null;
            ClearValue(ThemeScope.ContextProperty);
        }
    }

    private sealed class ThrowingVariantThemeConfigProvider : ThemeConfigProvider
    {
        private bool _throwOnNextVariantChange;

        internal bool ContextPublished { get; private set; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ThemeScope.ContextProperty &&
                change.GetNewValue<ThemeContext?>() is { } context)
            {
                context.Published += (_, _) => ContextPublished = true;
                _throwOnNextVariantChange = true;
            }
            else if (change.Property == RequestedThemeVariantProperty &&
                     _throwOnNextVariantChange)
            {
                _throwOnNextVariantChange = false;
                throw new InvalidOperationException("variant update failed");
            }
        }
    }

    private sealed record CollectibleReferences(
        WeakReference Provider,
        WeakReference Context,
        WeakReference ResourceProvider);
}
