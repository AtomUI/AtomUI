using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeTokenResourceProviderTests
{
    [Fact]
    public void Typed_Own_Keys_Validate_Owner_Without_Removing_Boxed_Enum_Support()
    {
        var provider = new ThemeTokenResourceProvider(Compile());
        var correct = ControlTokenIdentity.ForControl(typeof(ButtonThemeTestControl), "AtomUI", CompilerButtonToken.ID);
        var wrong = ControlTokenIdentity.ForControl(typeof(DerivedButtonThemeTestControl), "AtomUI", CompilerButtonToken.ID);
        provider.TryGetResource(ControlTokenResourceKey.Own(correct, CompilerButtonTokenKind.Height), null, out var own).ShouldBeTrue();
        own.ShouldBe(32d);
        provider.TryGetResource(CompilerButtonTokenKind.Height, null, out var boxed).ShouldBeTrue();
        boxed.ShouldBe(32d);
        Should.Throw<InvalidOperationException>(() => provider.TryGetResource(ControlTokenResourceKey.Own(wrong, CompilerButtonTokenKind.Height), null, out _));
        Should.Throw<InvalidOperationException>(() => provider.TryGetResource(ControlTokenResourceKey.Global(wrong, SharedTokenKind.ColorPrimary), null, out _));
        Should.Throw<InvalidOperationException>(() => provider.TryGetResource(ControlTokenResourceKey.Own(correct, BrushTokenKind.Brush), null, out _));
    }

    [Fact]
    public void ControlShared_Key_Resolves_Private_Value_Without_Changing_Global()
    {
        var snapshot = CompileButtonPrimary("#00b96b");
        var provider = new ThemeTokenResourceProvider(snapshot);

        provider.TryGetResource(SharedTokenKind.ColorPrimary, null, out var global).ShouldBeTrue();
        provider.TryGetResource(
            SharedKey("AtomUI", CompilerButtonToken.ID, SharedTokenKind.ColorPrimary),
            null,
            out var control).ShouldBeTrue();

        control.ShouldNotBe(global);
    }

    [Fact]
    public void ControlShared_Key_Falls_Back_To_Global_Value_For_Registered_Unconfigured_Control()
    {
        var snapshot = Compile();
        var provider = new ThemeTokenResourceProvider(snapshot);

        provider.TryGetResource(SharedTokenKind.ColorPrimary, null, out var global).ShouldBeTrue();
        provider.TryGetResource(
            SharedKey("AtomUI", CompilerButtonToken.ID, SharedTokenKind.ColorPrimary),
            null,
            out var control).ShouldBeTrue();

        control.ShouldBeSameAs(global);
    }

    [Fact]
    public void ControlShared_Key_Uses_Catalog_As_Part_Of_Control_Identity()
    {
        var snapshot = CompileCatalogControls();
        var provider = new ThemeTokenResourceProvider(snapshot);

        provider.TryGetResource(
            SharedKey("First", CompilerButtonToken.ID, SharedTokenKind.ColorPrimary),
            null,
            out var firstValue).ShouldBeTrue();
        provider.TryGetResource(
            SharedKey("Second", CompilerButtonToken.ID, SharedTokenKind.ColorPrimary),
            null,
            out var secondValue).ShouldBeTrue();

        firstValue.ShouldNotBe(secondValue);
    }

    [Fact]
    public void ControlShared_Keys_Isolate_Parent_Child_Control_Pairs_While_Global_Fallbacks_Remain_Shared()
    {
        var snapshot = CompileCrossControlSnapshot();
        var provider = new ThemeTokenResourceProvider(snapshot);
        provider.TryGetResource(SharedTokenKind.ColorInfo, null, out var globalInfo).ShouldBeTrue();

        foreach (var (parentControl, childControl) in CrossControlPairs)
        {
            provider.TryGetResource(
                SharedKey("AtomUI", parentControl, SharedTokenKind.ColorPrimary),
                null,
                out var parentPrimary).ShouldBeTrue();
            provider.TryGetResource(
                SharedKey("AtomUI", childControl, SharedTokenKind.ColorPrimary),
                null,
                out var childPrimary).ShouldBeTrue();

            parentPrimary.ShouldNotBe(childPrimary);

            provider.TryGetResource(
                SharedKey("AtomUI", parentControl, SharedTokenKind.ColorInfo),
                null,
                out var parentInfo).ShouldBeTrue();
            provider.TryGetResource(
                SharedKey("AtomUI", childControl, SharedTokenKind.ColorInfo),
                null,
                out var childInfo).ShouldBeTrue();

            parentInfo.ShouldBeSameAs(globalInfo);
            childInfo.ShouldBeSameAs(globalInfo);
        }
    }

    [Fact]
    public void Provider_Returns_False_For_Unknown_Resources_And_Throws_For_Unknown_Control_Ids()
    {
        var provider = new ThemeTokenResourceProvider(Compile());

        provider.TryGetResource("Missing", null, out var unknownResource).ShouldBeFalse();
        unknownResource.ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => provider.TryGetResource(
            ControlTokenResourceKey.Global(
                new AtomUI.Theme.Schema.ControlTokenIdentity("AtomUI", "Missing"),
                SharedTokenKind.ColorPrimary),
            null,
            out _));
    }

    [Fact]
    public void Provider_Resolves_Control_Own_Token_Keys()
    {
        var provider = new ThemeTokenResourceProvider(Compile());

        provider.TryGetResource(CompilerButtonTokenKind.Height, null, out var value).ShouldBeTrue();

        value.ShouldBe(32d);
    }

    [Fact]
    public void Provider_Returns_A_Copy_For_Mutable_Control_Own_Resource_Values()
    {
        var descriptor = CreateBrushControlDescriptor();
        var provider = new ThemeTokenResourceProvider(
            ThemeTestSnapshotFactory.Compile(new ThemeConfigBuilder().Build(), descriptor));

        provider.TryGetResource(BrushTokenKind.Brush, null, out var firstValue).ShouldBeTrue();
        provider.TryGetResource(BrushTokenKind.Brush, null, out var secondValue).ShouldBeTrue();

        var first = firstValue.ShouldBeOfType<SolidColorBrush>();
        var second = secondValue.ShouldBeOfType<SolidColorBrush>();
        first.ShouldNotBeSameAs(second);
        first.Opacity = 0.1;
        second.Opacity.ShouldBe(0.75);
    }

    [Fact]
    public void Provider_Resolves_Csharp_Keys_By_Exact_Control_Type()
    {
        var provider = new ThemeTokenResourceProvider(Compile(controlPrimary: "#00b96b"));

        provider.TryGetResource(
            ControlTokenResourceKey.Global(
                typeof(ButtonThemeTestControl),
                SharedTokenKind.ColorPrimary),
            null,
            out var effectiveGlobal).ShouldBeTrue();
        provider.TryGetResource(
            ControlTokenResourceKey.Own(
                typeof(ButtonThemeTestControl),
                CompilerButtonTokenKind.Height),
            null,
            out var own).ShouldBeTrue();

        effectiveGlobal.ShouldNotBeNull();
        own.ShouldBe(32d);
        Should.Throw<InvalidOperationException>(() => provider.TryGetResource(
            ControlTokenResourceKey.Global(
                typeof(DerivedButtonThemeTestControl),
                SharedTokenKind.ColorPrimary),
            null,
            out _));
    }

    [Fact]
    public void Provider_Attaches_To_The_Owning_Resource_Host()
    {
        var provider = new ThemeTokenResourceProvider(Compile());
        var owner = new Border();

        owner.Resources.MergedDictionaries.Add(provider);

        provider.Owner.ShouldBeSameAs(owner);
    }

    [Fact]
    public void PrepareSnapshot_Is_Silent_Until_One_Publish_Notification()
    {
        var provider = new ThemeTokenResourceProvider(Compile());
        var owner = new Border();
        owner.Resources.MergedDictionaries.Add(provider);
        var notifications = 0;
        ((IResourceHost)owner).ResourcesChanged += (_, _) => notifications++;

        provider.PrepareSnapshot(Compile(globalPrimary: "#00b96b"));

        notifications.ShouldBe(0);
        provider.PublishSnapshotChanged();

        notifications.ShouldBe(1);
    }

    [Fact]
    public void ReplaceSnapshot_Publishes_One_Resource_Notification()
    {
        var provider = new ThemeTokenResourceProvider(Compile());
        var owner = new Border();
        owner.Resources.MergedDictionaries.Add(provider);
        var notifications = 0;
        ((IResourceHost)owner).ResourcesChanged += (_, _) => notifications++;

        provider.ReplaceSnapshot(Compile(globalPrimary: "#00b96b"));

        notifications.ShouldBe(1);
    }

    internal static ThemeSnapshot Compile(
        string? globalPrimary = null,
        string? controlPrimary = null)
    {
        var builder = new ThemeConfigBuilder();
        if (globalPrimary is not null)
        {
            builder.WithToken(nameof(DesignToken.ColorPrimary), globalPrimary);
        }
        if (controlPrimary is not null)
        {
            builder.WithControl(
                new AtomUI.Theme.Schema.ControlTokenIdentity("AtomUI", CompilerButtonToken.ID),
                new ControlThemeConfigBuilder()
                    .WithAlgorithm(ControlAlgorithmMode.Disabled)
                    .WithToken(nameof(DesignToken.ColorPrimary), controlPrimary)
                    .Build());
        }
        return ThemeTestSnapshotFactory.Compile(
            builder.Build(),
            ThemeCompilerTests.CreateCompilerButtonDescriptor());
    }

    private static ThemeSnapshot CompileButtonPrimary(string primary)
    {
        return Compile(controlPrimary: primary);
    }

    private static ThemeSnapshot CompileCrossControlSnapshot()
    {
        var controlSharedOverrides = new Dictionary<string, string>
        {
            [CompilerButtonToken.ID] = "#ff4d4f",
            [CompilerIconToken.ID] = "#1677ff",
            [CompilerLineEditToken.ID] = "#00b96b",
            [CompilerAddOnDecoratedBoxToken.ID] = "#faad14",
            [CompilerSelectToken.ID] = "#722ed1",
            [CompilerDatePickerToken.ID] = "#13c2c2",
            [CompilerDialogToken.ID] = "#eb2f96",
            [CompilerDataGridToken.ID] = "#52c41a"
        };
        var descriptors = new[]
        {
            ThemeCompilerTests.CreateCompilerButtonDescriptor(),
            CreateCrossControlDescriptor(CompilerIconToken.ID, static () => new CompilerIconToken()),
            CreateCrossControlDescriptor(CompilerLineEditToken.ID, static () => new CompilerLineEditToken()),
            CreateCrossControlDescriptor(CompilerAddOnDecoratedBoxToken.ID, static () => new CompilerAddOnDecoratedBoxToken()),
            CreateCrossControlDescriptor(CompilerSelectToken.ID, static () => new CompilerSelectToken()),
            CreateCrossControlDescriptor(CompilerDatePickerToken.ID, static () => new CompilerDatePickerToken()),
            CreateCrossControlDescriptor(CompilerDialogToken.ID, static () => new CompilerDialogToken()),
            CreateCrossControlDescriptor(CompilerDataGridToken.ID, static () => new CompilerDataGridToken())
        };
        var builder = new ThemeConfigBuilder()
                      .WithToken(nameof(DesignToken.ColorInfo), "#722ed1");
        foreach (var entry in controlSharedOverrides)
        {
            builder.WithControl(
                new AtomUI.Theme.Schema.ControlTokenIdentity("AtomUI", entry.Key),
                new ControlThemeConfigBuilder()
                    .WithAlgorithm(ControlAlgorithmMode.Disabled)
                    .WithToken(nameof(DesignToken.ColorPrimary), entry.Value)
                    .Build());
        }
        return ThemeTestSnapshotFactory.Compile(builder.Build(), descriptors);
    }

    internal static ThemeSnapshot CompileCatalogControls()
    {
        var first = CreateCrossControlDescriptor(
            CompilerButtonToken.ID,
            static () => new CompilerButtonToken(),
            "First");
        var second = CreateCrossControlDescriptor(
            CompilerButtonToken.ID,
            static () => new CompilerButtonToken(),
            "Second");
        var config = new ThemeConfigBuilder()
                     .WithControl(
                         first.Identity,
                         new ControlThemeConfigBuilder()
                             .WithAlgorithm(ControlAlgorithmMode.Disabled)
                             .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                             .Build())
                     .WithControl(
                         second.Identity,
                         new ControlThemeConfigBuilder()
                             .WithAlgorithm(ControlAlgorithmMode.Disabled)
                             .WithToken(nameof(DesignToken.ColorPrimary), "#ff4d4f")
                             .Build())
                     .Build();
        return ThemeTestSnapshotFactory.Compile(config, first, second);
    }

    private static AtomUI.Theme.Schema.ControlTokenDescriptor CreateCrossControlDescriptor(
        string id,
        Func<AbstractControlDesignToken> factory,
        string catalog = "AtomUI")
    {
        return new AtomUI.Theme.Schema.ControlTokenDescriptor(
            ThemeTestControlTypes.For(catalog, id),
            new AtomUI.Theme.Schema.ControlTokenIdentity(catalog, id));
    }

    private static AtomUI.Theme.Schema.ControlTokenDescriptor CreateBrushControlDescriptor()
    {
        var brush = new AtomUI.Theme.Schema.TokenDescriptor(
            "Brush",
            0,
            AtomUI.Theme.Schema.TokenStage.Control,
            typeof(SolidColorBrush),
            BrushTokenKind.Brush,
            static value => ThemeTokenValueParser.Parse<SolidColorBrush>(value),
            static value => ((SolidColorBrush)value!).ToString() ?? string.Empty,
            static token => ((BrushControlToken)token).Brush,
            static (token, value) => ((BrushControlToken)token).Brush = (SolidColorBrush)value!,
            static token => ((BrushControlToken)token).Brush);
        return new AtomUI.Theme.Schema.ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", BrushControlToken.ID),
            new AtomUI.Theme.Schema.ControlTokenIdentity("AtomUI", BrushControlToken.ID),
            [brush],
            static () => new BrushControlToken(),
            static (token, appearance) =>
                ((BrushControlToken)token).CalculateTokenValues(appearance == ThemeAppearance.Dark));
    }

    private static readonly (string ParentControl, string ChildControl)[] CrossControlPairs =
    [
        (CompilerButtonToken.ID, CompilerIconToken.ID),
        (CompilerLineEditToken.ID, CompilerAddOnDecoratedBoxToken.ID),
        (CompilerSelectToken.ID, CompilerButtonToken.ID),
        (CompilerDatePickerToken.ID, CompilerButtonToken.ID),
        (CompilerDialogToken.ID, CompilerButtonToken.ID),
        (CompilerDataGridToken.ID, CompilerButtonToken.ID)
    ];

    private static object SharedKey(
        string catalog,
        string id,
        SharedTokenKind kind)
    {
        return ControlTokenResourceKey.Global(
            new AtomUI.Theme.Schema.ControlTokenIdentity(catalog, id),
            kind);
    }
}

internal abstract class CrossControlCompilerToken : AbstractControlDesignToken
{
    public double Height { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        Height = EffectiveGlobalToken.ControlHeight;
    }
}

internal sealed class CompilerIconToken : CrossControlCompilerToken
{
    internal const string ID = "Icon";
}

internal sealed class CompilerLineEditToken : CrossControlCompilerToken
{
    internal const string ID = "LineEdit";
}

internal sealed class CompilerAddOnDecoratedBoxToken : CrossControlCompilerToken
{
    internal const string ID = "AddOnDecoratedBox";
}

internal sealed class CompilerSelectToken : CrossControlCompilerToken
{
    internal const string ID = "Select";
}

internal sealed class CompilerDatePickerToken : CrossControlCompilerToken
{
    internal const string ID = "DatePicker";
}

internal sealed class CompilerDialogToken : CrossControlCompilerToken
{
    internal const string ID = "Dialog";
}

internal sealed class CompilerDataGridToken : CrossControlCompilerToken
{
    internal const string ID = "DataGrid";
}

internal enum BrushTokenKind
{
    Brush
}

internal sealed class BrushControlToken : AbstractControlDesignToken
{
    internal const string ID = "BrushControl";

    public SolidColorBrush? Brush { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        Brush = new SolidColorBrush(Colors.CornflowerBlue, 0.75);
    }
}
