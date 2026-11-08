using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeTokenValueParserTests
{
    [Fact]
    public void Parse_Supports_Every_BuiltIn_Generated_Descriptor_Value_Type()
    {
        ThemeTokenValueParser.Parse<Dimension>("25%").ShouldBe(new Dimension(25, DimensionUnitType.Percentage));
        ThemeTokenValueParser.Parse<LineStyle>("Dashed").ShouldBe(LineStyle.Dashed);
        ThemeTokenValueParser.Parse<TextDecorationInfo?>("underline solid #1677FF 1").ShouldNotBeNull();
        ThemeTokenValueParser.Parse<Easing>("0.1,0.9,0.2,1.0").ShouldNotBeNull();
        ThemeTokenValueParser.Parse<CornerRadius>("6").ShouldBe(new CornerRadius(6));
        ThemeTokenValueParser.Parse<BoxShadow>("0 1 2 0 #66000000").Blur.ShouldBe(2);
        ThemeTokenValueParser.Parse<BoxShadows>("0 1 2 0 #66000000").Count.ShouldBe(1);
        ThemeTokenValueParser.Parse<Color>("#1677FF").ShouldBe(Color.Parse("#1677FF"));
        ThemeTokenValueParser.Parse<Color?>("#1677FF").ShouldBe(Color.Parse("#1677FF"));
        ThemeTokenValueParser.Parse<FontFamily>("Arial").ShouldBe(FontFamily.Parse("Arial"));
        ThemeTokenValueParser.Parse<FontWeight>("SemiBold").ShouldBe(FontWeight.SemiBold);
        ThemeTokenValueParser.Parse<IBrush>("#1677FF").ShouldNotBeNull();
        ThemeTokenValueParser.Parse<IImmutableBrush>("#1677FF").ShouldNotBeNull();
        ThemeTokenValueParser.Parse<ImmutableTransform>("1,0,0,1,4,8").ShouldNotBeNull();
        ThemeTokenValueParser.Parse<PenLineCap>("Round").ShouldBe(PenLineCap.Round);
        ThemeTokenValueParser.Parse<PenLineJoin>("Bevel").ShouldBe(PenLineJoin.Bevel);
        ThemeTokenValueParser.Parse<SolidColorBrush>("#1677FF").Color.ShouldBe(Color.Parse("#1677FF"));
        ThemeTokenValueParser.Parse<Point>("12,8").ShouldBe(new Point(12, 8));
        ThemeTokenValueParser.Parse<Size>("12,8").ShouldBe(new Size(12, 8));
        ThemeTokenValueParser.Parse<Thickness>("8,4,8,4").ShouldBe(new Thickness(8, 4, 8, 4));
        ThemeTokenValueParser.Parse<bool>("true").ShouldBeTrue();
        ThemeTokenValueParser.Parse<double>("1.5").ShouldBe(1.5);
        ThemeTokenValueParser.Parse<int>("14").ShouldBe(14);
        ThemeTokenValueParser.Parse<TimeSpan>("00:00:00.1000000").ShouldBe(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void Parse_Requires_Case_Sensitive_Enum_Names()
    {
        Should.Throw<InvalidOperationException>(() => ThemeTokenValueParser.Parse<PenLineCap>("round"));
        Should.Throw<InvalidOperationException>(() => ThemeTokenValueParser.Parse<LineStyle>("dashed"));
    }

    [Fact]
    public void Format_Produces_Canonical_Distinct_Values_For_Fingerprints()
    {
        ThemeTokenValueFormatter.Format(ThemeTokenValueParser.Parse<double>("1.50"))
                                .ShouldBe(ThemeTokenValueFormatter.Format(
                                    ThemeTokenValueParser.Parse<double>("1.5")));

        var blue = ThemeTokenValueParser.Parse<IBrush>("#1677FF");
        var red = ThemeTokenValueParser.Parse<IBrush>("#FF4D4F");
        ThemeTokenValueFormatter.Format(blue).ShouldNotBe(ThemeTokenValueFormatter.Format(red));
    }

    [Fact]
    public void Format_Round_Trips_Transforms_Through_The_Parser()
    {
        var scale  = ThemeTokenValueParser.Parse<ImmutableTransform>("1,0,0,0.75,0,0");
        var offset = ThemeTokenValueParser.Parse<ImmutableTransform>("1,0,0,1,4,8");

        var formatted = ThemeTokenValueFormatter.Format(scale);
        formatted.ShouldBe("1,0,0,0.75,0,0");
        ThemeTokenValueParser.Parse<ImmutableTransform>(formatted).Value.ShouldBe(scale.Value);
        ThemeTokenValueFormatter.Format(offset).ShouldNotBe(formatted);
    }
}
