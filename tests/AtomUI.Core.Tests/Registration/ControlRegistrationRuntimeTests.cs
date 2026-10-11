using AtomUI.Core.Tests.Theme;
using AtomUI.Generated.AtomUICore;
using AtomUI.Registration;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Registration;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ControlRegistrationRuntimeTests
{
    [Fact]
    public async Task Nested_Packages_Stage_In_Commit_Order_And_Delay_All_Factories()
    {
        await HeadlessTestApp.RunAsync(() =>
        {
            var calls = new List<string>();
            var builder = new AtomUIBuilder(Application.Current!);
            Register("Desktop", prepare: _ =>
            {
                calls.Add("prepare-desktop");
                Register("Common");
            });
            calls.ShouldBe(["prepare-desktop", "provider-Common", "collect-Common", "complete-Common", "provider-Desktop", "collect-Desktop", "complete-Desktop"]);
            builder.ThemeManagerBuilder.ControlPackages.Select(p => p.Id).ShouldBe(["Common", "Desktop"]);
            using var manager = builder.ThemeManagerBuilder.Build();
            manager.InitializeApplication(Application.Current!);
            // 惰性挂载：挂载只记录代理，工厂推迟到首次资源查找，按合并列表逆序（后挂载优先）逐个构造。
            calls.Count.ShouldBe(7);
            manager.Resources.TryGetResource("priority", null, out var priority).ShouldBeTrue();
            priority.ShouldBe("Desktop/z");
            calls.TakeLast(1).ShouldBe(["Desktop/z"]);
            manager.Resources.TryGetResource("missing", null, out _).ShouldBeFalse();
            calls.TakeLast(6).ShouldBe(["Desktop/z", "Desktop/a", "Desktop/shared", "Common/z", "Common/a", "Common/shared"]);
            return Task.CompletedTask;

            void Register(string id, Action<IAtomUIBuilder>? prepare = null) => ControlRegistrationRuntime.RegisterPackage(
                builder, id, () => { calls.Add("provider-" + id); return new Provider(id); },
                collection =>
                {
                    calls.Add("collect-" + id);
                    foreach (var (suffix, phase) in new[] { ("z", ControlResourcePhase.Control), ("shared", ControlResourcePhase.Shared), ("a", ControlResourcePhase.Control) })
                    {
                        var assetId = id + "/" + suffix;
                        collection.AddResource(new ControlThemeResourceRegistration(assetId, phase, 0, () =>
                        {
                            calls.Add(assetId);
                            return new ResourceDictionary { ["priority"] = assetId };
                        }));
                    }
                }, prepare, _ => calls.Add("complete-" + id));
        });
    }

    [Theory]
    [InlineData("prepare")]
    [InlineData("provider")]
    [InlineData("collect")]
    [InlineData("complete")]
    public void Failed_Registration_Poisons_Build_And_Other_Builders_Remain_Independent(string phase)
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new AtomUIBuilder(new Application());
            Should.Throw<InvalidOperationException>(() => ControlRegistrationRuntime.RegisterPackage(builder, "Test",
                () => { Fail("provider"); return new Provider("Test"); }, _ => Fail("collect"), _ => Fail("prepare"), _ => Fail("complete")));
            Should.Throw<InvalidOperationException>(() => builder.ThemeManagerBuilder.Build());
            var other = new AtomUIBuilder(new Application());
            ControlRegistrationRuntime.RegisterPackage(other, "Test", static () => new Provider("Test"), static _ => { });
            using var manager = other.ThemeManagerBuilder.Build();
            return;
            void Fail(string at) { if (phase == at) { throw new InvalidOperationException(at); } }
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Duplicate_And_Reentry_Fail_Before_Prepare_And_Poison_Build(bool recurse)
    {
        var builder = new AtomUIBuilder(new Application());
        var prepares = 0;
        Action<IAtomUIBuilder> prepare = _ => prepares++;
        void Register() => ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), static _ => { }, prepare);
        if (recurse)
        {
            prepare = _ => { prepares++; if (prepares == 1) { Register(); } };
            Should.Throw<InvalidOperationException>(Register);
        }
        else
        {
            Register();
            Should.Throw<InvalidOperationException>(Register);
        }
        prepares.ShouldBe(1);
        Should.Throw<InvalidOperationException>(() => builder.ThemeManagerBuilder.Build());
    }

    [Fact]
    public void Shared_Asset_Deduplicates_But_Conflicting_Metadata_Or_Factory_Fails()
    {
        var builder = new AtomUIBuilder(new Application());
        ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), collection =>
        {
            collection.AddResource(new("shared", ControlResourcePhase.Shared, 0, CreateResource));
            collection.AddResource(new("shared", ControlResourcePhase.Shared, 0, CreateResource));
        });
        builder.ThemeManagerBuilder.ControlPackages.Single().Resources.ShouldHaveSingleItem();
        foreach (var conflict in new[]
        {
            new ControlThemeResourceRegistration("shared", ControlResourcePhase.Shared, 1, CreateResource),
            new ControlThemeResourceRegistration("shared", ControlResourcePhase.Shared, 0, OtherResource)
        })
        {
            var invalid = new AtomUIBuilder(new Application());
            Should.Throw<InvalidOperationException>(() => ControlRegistrationRuntime.RegisterPackage(invalid, "Test", static () => new Provider("Test"), collection =>
            {
                collection.AddResource(new("shared", ControlResourcePhase.Shared, 0, CreateResource));
                collection.AddResource(conflict);
            }));
            Should.Throw<InvalidOperationException>(() => invalid.ThemeManagerBuilder.Build());
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Foreign_Owner_Is_Validated_Only_After_All_Packages_Are_Collected(bool provideOwner)
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new AtomUIBuilder(new Application());
            var owner = ControlTokenIdentity.ForControl(typeof(Button), "Foreign", "Button");
            var factories = 0;
            var asset = ControlThemeAssetContractTests.Asset([], [owner], globals: GeneratedThemeSchema.GetGlobalTokens());
            ControlRegistrationRuntime.RegisterPackage(builder, "First", static () => new Provider("First"), collection =>
            {
                collection.AddThemeAsset(asset, new(asset.AssetId, ControlResourcePhase.Control, 0, () => { factories++; return new ResourceDictionary(); }));
            });
            factories.ShouldBe(0);
            if (provideOwner)
            {
                ControlRegistrationRuntime.RegisterPackage(builder, "Later", static () => new Provider("Later"), collection => collection.AddControl(new(typeof(Button), owner)));
                using var manager = builder.ThemeManagerBuilder.Build();
            }
            else
            {
                Should.Throw<ThemeSchemaException>(() => builder.ThemeManagerBuilder.Build());
            }
            factories.ShouldBe(0);
        });
    }

    [Fact]
    public void Semantic_Only_Contract_Is_Legal_But_Conflicting_Token_Owner_Is_Not()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new AtomUIBuilder(new Application());
            var identity = ControlTokenIdentity.ForControl(typeof(Button), "Test", "Button");
            ControlRegistrationRuntime.RegisterPackage(builder, "Semantic", static () => new Provider("Semantic"), collection =>
                collection.AddSemanticControl(new(typeof(Button), identity,
                    [new SemanticPartDescriptor("root", "root", null, typeof(Button), SemanticPartCardinality.Single, SemanticPartCustomization.Root, null, false, null, false)])));
            using var manager = builder.ThemeManagerBuilder.Build();
            manager.SemanticParts.TryGetControl(identity, out _).ShouldBeTrue();

            var conflicting = new AtomUIBuilder(new Application());
            ControlRegistrationRuntime.RegisterPackage(conflicting, "Semantic", static () => new Provider("Semantic"), collection =>
                collection.AddSemanticControl(builder.ThemeManagerBuilder.ControlPackages.Single().SemanticControls.Single()));
            ControlRegistrationRuntime.RegisterPackage(conflicting, "Token", static () => new Provider("Token"), collection =>
                collection.AddControl(new(typeof(TextBox), new ControlTokenIdentity("Test", "Button"))));
            Should.Throw<ThemeSchemaException>(() => conflicting.ThemeManagerBuilder.Build());
        });
    }

    [Fact]
    public void Ensure_Skips_Only_Completed_Packages_And_Frozen_Builder_Rejects_Registration()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new AtomUIBuilder(new Application());
            var prepares = 0;
            ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), static _ => { });
            ControlRegistrationRuntime.EnsurePackage(builder, "Test", static () => new Provider("Test"), static _ => { }, _ => prepares++);
            prepares.ShouldBe(0);
            using var manager = builder.ThemeManagerBuilder.Build();
            Should.Throw<InvalidOperationException>(() => ControlRegistrationRuntime.EnsurePackage(builder, "Test", static () => new Provider("Test"), static _ => { }, _ => prepares++));
            prepares.ShouldBe(0);
        });
    }

    [Fact]
    public void Ensure_Cannot_Reenter_During_Complete_Or_Build_A_Partial_Application()
    {
        var builder = new AtomUIBuilder(new Application());
        Should.Throw<InvalidOperationException>(() => ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), static _ => { },
            complete: _ => ControlRegistrationRuntime.EnsurePackage(builder, "Test", static () => new Provider("Test"), static _ => { })));
        Should.Throw<InvalidOperationException>(() => builder.ThemeManagerBuilder.Build());
    }

    [Fact]
    public void Collector_Uses_Only_TryGetValue_And_Activates_A_Proxy_Once()
    {
        SelectedFragmentAttribute.Activations = 0;
        var builder = new AtomUIBuilder(new Application());
        ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), collection =>
            ControlRegistrationRuntime.CollectFragments(collection, new TryOnlyMap(new Dictionary<string, Type>
            {
                ["control"] = typeof(SelectedFragmentAttribute), ["style"] = typeof(SelectedFragmentAttribute)
            }), ["absent", "control", "style"]));
        SelectedFragmentAttribute.Activations.ShouldBe(1);
        builder.ThemeManagerBuilder.ControlPackages.Single().Controls.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("illegal")]
    [InlineData("conflict")]
    public void Collector_Rejects_Invalid_Proxy_And_Fragment_Identity_Conflicts(string scenario)
    {
        var builder = new AtomUIBuilder(new Application());
        var types = scenario switch
        {
            "missing" => new[] { typeof(MissingFragmentAttribute) },
            "illegal" => new[] { typeof(Button) },
            _ => new[] { typeof(SelectedFragmentAttribute), typeof(ConflictingFragmentAttribute) }
        };
        var map = types.Select((type, index) => KeyValuePair.Create(index.ToString(), type)).ToDictionary();
        Should.Throw<InvalidOperationException>(() => ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), collection =>
            ControlRegistrationRuntime.CollectFragments(collection, map, map.Keys.ToArray())));
        Should.Throw<InvalidOperationException>(() => builder.ThemeManagerBuilder.Build());
    }

    [Fact]
    public void Caught_Invalid_Proxy_Still_Poisons_The_Package_Collection()
    {
        var builder = new AtomUIBuilder(new Application());
        Should.Throw<InvalidOperationException>(() => ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), collection =>
        {
            try { ControlRegistrationRuntime.CollectFragments(collection, new Dictionary<string, Type> { ["bad"] = typeof(Button) }, ["bad"]); }
            catch (InvalidOperationException) { }
        }));
        Should.Throw<InvalidOperationException>(() => builder.ThemeManagerBuilder.Build());
    }

    [Fact]
    public void Cross_Package_Shared_Resource_Metadata_Is_Validated_Before_Any_Factory()
    {
        var builder = new AtomUIBuilder(new Application());
        var calls = 0;
        Func<IResourceProvider> factory = () => { calls++; return new ResourceDictionary(); };
        foreach (var id in new[] { "First", "Later" })
        {
            ControlRegistrationRuntime.RegisterPackage(builder, id, () => new Provider(id), collection =>
                collection.AddResource(new("shared", ControlResourcePhase.Shared, id == "First" ? 0 : 1, factory)));
        }
        Should.Throw<ThemeSchemaException>(() => builder.ThemeManagerBuilder.Build());
        calls.ShouldBe(0);
    }

    [Fact]
    public async Task Multiple_Fragments_And_Packages_Mount_An_Identical_Asset_Once()
    {
        await HeadlessTestApp.RunAsync(() =>
        {
            var builder = new AtomUIBuilder(Application.Current!);
            var calls = 0;
            Func<IResourceProvider> factory = () => { calls++; return new ResourceDictionary(); };
            var asset = ControlThemeAssetContractTests.Asset(
                [new ControlThemeExportDescriptor(typeof(Button), "NamedButton"), new ControlThemeExportDescriptor(typeof(TextBox), typeof(TextBox))],
                globals: GeneratedThemeSchema.GetGlobalTokens());
            foreach (var id in new[] { "First", "Later" })
            {
                ControlRegistrationRuntime.RegisterPackage(builder, id, () => new Provider(id), collection =>
                {
                    collection.AddThemeAsset(asset, new(asset.AssetId, ControlResourcePhase.Control, 0, factory));
                    collection.AddThemeAsset(asset, new(asset.AssetId, ControlResourcePhase.Control, 0, factory));
                });
            }
            calls.ShouldBe(0);
            using var manager = builder.ThemeManagerBuilder.Build();
            manager.InitializeApplication(Application.Current!);
            // 惰性挂载：去重后的唯一注册项只挂一个代理，首次全量扫描恰好执行一次工厂。
            calls.ShouldBe(0);
            manager.Resources.TryGetResource("missing", null, out _).ShouldBeFalse();
            calls.ShouldBe(1);
            return Task.CompletedTask;
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Semantic_Binding_Validates_Registered_Semantic_Owner_And_Target(bool wrongTarget)
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new AtomUIBuilder(new Application());
            var owner = ControlTokenIdentity.ForControl(typeof(Button), "Test", "Button");
            var semantic = new ControlSemanticDescriptor(typeof(Button), owner,
            [
                new SemanticPartDescriptor("root", "root", null, typeof(Button), SemanticPartCardinality.Single, SemanticPartCustomization.Root, null, false, null, false),
                new SemanticPartDescriptor("content", "content", "semantic-content", typeof(TextBox), SemanticPartCardinality.Single, SemanticPartCustomization.SelectorAndTheme,
                    new ControlThemeSemanticPartDescriptor("ContentTheme", typeof(TextBox)), false, null, false)
            ]);
            var asset = ControlThemeAssetContractTests.Asset([], bindings:
                [new ControlThemeBindingDescriptor(owner, "ContentTheme", wrongTarget ? typeof(Button) : typeof(TextBox))],
                globals: GeneratedThemeSchema.GetGlobalTokens());
            ControlRegistrationRuntime.RegisterPackage(builder, "Asset", static () => new Provider("Asset"), collection =>
                collection.AddThemeAsset(asset, new(asset.AssetId, ControlResourcePhase.Control, 0, CreateResource)));
            ControlRegistrationRuntime.RegisterPackage(builder, "Semantic", static () => new Provider("Semantic"), collection => collection.AddSemanticControl(semantic));
            if (wrongTarget)
            {
                Should.Throw<ThemeSchemaException>(() => builder.ThemeManagerBuilder.Build());
            }
            else { using var manager = builder.ThemeManagerBuilder.Build(); }
        });
    }

    [Fact]
    public void Semantic_Binding_Rejects_Same_FullName_From_A_Different_Assembly()
    {
        var first = CreateSameNamedTarget("SemanticTarget.First");
        var second = CreateSameNamedTarget("SemanticTarget.Second");
        first.FullName.ShouldBe(second.FullName);
        var builder = BuildSemanticBinding(first, second);
        Should.Throw<ThemeSchemaException>(() => builder.ThemeManagerBuilder.Build());
    }

    [Fact]
    public void Semantic_Binding_Accepts_A_Nested_Target_With_Exact_Type_Identity()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = BuildSemanticBinding(typeof(NestedTarget), typeof(NestedTarget));
            using var manager = builder.ThemeManagerBuilder.Build();
        });
    }

    private static AtomUIBuilder BuildSemanticBinding(Type declaredTarget, Type boundTarget)
    {
        var builder = new AtomUIBuilder(new Application());
        var owner = ControlTokenIdentity.ForControl(typeof(Button), "Test", "Button");
        var semantic = new ControlSemanticDescriptor(typeof(Button), owner,
        [
            new SemanticPartDescriptor("root", "root", null, typeof(Button), SemanticPartCardinality.Single, SemanticPartCustomization.Root, null, false, null, false),
            new SemanticPartDescriptor("content", "content", "semantic-content", declaredTarget, SemanticPartCardinality.Single, SemanticPartCustomization.SelectorAndTheme,
                new ControlThemeSemanticPartDescriptor("ContentTheme", declaredTarget), false, null, false)
        ]);
        var asset = ControlThemeAssetContractTests.Asset([], bindings: [new ControlThemeBindingDescriptor(owner, "ContentTheme", boundTarget)],
            globals: GeneratedThemeSchema.GetGlobalTokens());
        ControlRegistrationRuntime.RegisterPackage(builder, "Test", static () => new Provider("Test"), collection =>
        {
            collection.AddSemanticControl(semantic);
            collection.AddThemeAsset(asset, new(asset.AssetId, ControlResourcePhase.Control, 0, CreateResource));
        });
        return builder;
    }

    private static Type CreateSameNamedTarget(string assemblyName) =>
        System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName(assemblyName), System.Reflection.Emit.AssemblyBuilderAccess.Run)
            .DefineDynamicModule(assemblyName).DefineType("Acme.Presenter", System.Reflection.TypeAttributes.Public, typeof(Control)).CreateType()!;

    private sealed class NestedTarget : Control;

    [SelectedFragment]
    private sealed class SelectedFragmentAttribute : ControlRegistrationFragmentAttribute
    {
        internal static int Activations;
        public SelectedFragmentAttribute() { Activations++; }
        public override string FragmentId => "Test/Button";
        public override void Add(ControlPackageRegistrationBuilder builder) =>
            builder.AddControl(new(typeof(Button), ControlTokenIdentity.ForControl(typeof(Button), "Test", "Button")));
    }

    [ConflictingFragment]
    private sealed class ConflictingFragmentAttribute : ControlRegistrationFragmentAttribute
    {
        public override string FragmentId => "Test/Button";
        public override void Add(ControlPackageRegistrationBuilder builder) { }
    }

    private sealed class MissingFragmentAttribute : ControlRegistrationFragmentAttribute
    {
        public override string FragmentId => "Test/Missing";
        public override void Add(ControlPackageRegistrationBuilder builder) { }
    }

    private sealed class TryOnlyMap(IReadOnlyDictionary<string, Type> map) : IReadOnlyDictionary<string, Type>
    {
        public bool TryGetValue(string key, out Type value) => map.TryGetValue(key, out value!);
        public Type this[string key] => throw new NotSupportedException();
        public int Count => throw new NotSupportedException();
        public IEnumerable<string> Keys => throw new NotSupportedException();
        public IEnumerable<Type> Values => throw new NotSupportedException();
        public bool ContainsKey(string key) => throw new NotSupportedException();
        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }

    private static IResourceProvider CreateResource() => new ResourceDictionary();
    private static IResourceProvider OtherResource() => new ResourceDictionary();
    private sealed class Provider : ControlThemesProvider { internal Provider(string id) { Id = id; } }
}
