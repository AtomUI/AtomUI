using System.Reflection;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeSnapshotTests
{
    [Fact]
    public void Snapshot_Types_Do_Not_Expose_Mutable_Token_Builders()
    {
        AssertDoesNotExposeMutableToken(typeof(ThemeSnapshot));
        AssertDoesNotExposeMutableToken(typeof(ControlThemeSnapshot));
    }

    [Fact]
    public void Token_Value_Table_Freezes_Reference_Values_And_Owns_Its_Dense_Storage()
    {
        var brush = new SolidColorBrush(Colors.Red);
        var builder = new SnapshotToken
        {
            Brush = brush
        };
        var descriptor = new TokenDescriptor(
            "Brush",
            0,
            TokenStage.Control,
            typeof(IBrush),
            SnapshotResourceKey.Brush,
            static value => Brush.Parse(value),
            static value => value?.ToString() ?? string.Empty,
            static token => ((SnapshotToken)token).Brush,
            static (token, value) => ((SnapshotToken)token).Brush = (IBrush?)value,
            static token => ((SnapshotToken)token).Brush?.ToImmutable());

        var table = TokenValueTable.Freeze(builder, [descriptor]);
        brush.Color = Colors.Blue;
        builder.Brush = Brushes.Green;

        table.Count.ShouldBe(1);
        var frozenBrush = table.Get<SolidColorBrush>(descriptor.Slot);
        frozenBrush.ShouldNotBeSameAs(brush);
        frozenBrush.Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Palette_Info_Is_A_Deeply_Immutable_Value()
    {
        typeof(PaletteInfo).GetProperty(nameof(PaletteInfo.Primary))!.CanWrite.ShouldBeFalse();
        typeof(PaletteInfo).GetProperty(nameof(PaletteInfo.ColorSequence))!.CanWrite.ShouldBeFalse();

        var source = new[] { Colors.Red, Colors.Blue };
        var palette = new PaletteInfo(Colors.Red, source);
        source[0] = Colors.Green;

        palette.ColorSequence[0].ShouldBe(Colors.Red);
        Should.Throw<NotSupportedException>(() =>
            ((IList<Color>)palette.ColorSequence)[0] = Colors.Green);
    }

    [Fact]
    public void Resource_Projection_Freezes_Mutable_Brush_Values()
    {
        var source = new SolidColorBrush(Colors.Red);

        var projected = ThemeResourceValue.Project<IBrush>(source)
                                          .ShouldBeAssignableTo<IImmutableSolidColorBrush>()!;
        source.Color = Colors.Blue;

        projected.Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Dense_Token_Value_Reads_Do_Not_Allocate_After_Warmup()
    {
        var builder = new SnapshotNumberToken
        {
            Value = 42
        };
        var descriptor = new TokenDescriptor(
            "Value",
            0,
            TokenStage.Control,
            typeof(double),
            SnapshotResourceKey.Value,
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((SnapshotNumberToken)token).Value,
            static (token, value) => ((SnapshotNumberToken)token).Value = (double)value!,
            static token => ((SnapshotNumberToken)token).Value);
        var table = TokenValueTable.Freeze(builder, [descriptor]);
        _ = table.Get<double>(0);

        var before = GC.GetAllocatedBytesForCurrentThread();
        var total = 0d;
        for (var index = 0; index < 10_000; index++)
        {
            total += table.Get<double>(0);
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        total.ShouldBe(420_000d);
        allocated.ShouldBe(0);
    }

    private static void AssertDoesNotExposeMutableToken(Type snapshotType)
    {
        var properties = snapshotType.GetProperties(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        properties.ShouldNotContain(static property =>
            typeof(AbstractDesignToken).IsAssignableFrom(property.PropertyType));
    }

    private enum SnapshotResourceKey
    {
        Brush,
        Value
    }

    private sealed class SnapshotToken : AbstractDesignToken
    {
        public IBrush? Brush { get; set; }
    }

    private sealed class SnapshotNumberToken : AbstractDesignToken
    {
        public double Value { get; set; }
    }
}
