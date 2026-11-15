using AtomUI.Media;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class TextMeasureCacheTests
{
    static TextMeasureCacheTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<string, double, FontWeight> Samples => new()
    {
        { "0", 14, FontWeight.Normal },
        { "8", 14, FontWeight.Normal },
        { "2028-12-28 22:58:58", 14, FontWeight.Normal },
        { "2028-12-28 22:58:58", 16, FontWeight.SemiBold },
        { "09:58:58 上午", 12, FontWeight.Normal },
        { "Wednesday, September 28, 2028", 14, FontWeight.Bold },
        { "", 14, FontWeight.Normal }
    };

    [Theory]
    [MemberData(nameof(Samples))]
    public void Cached_Measurement_Equals_Fresh_Measurement(string text, double fontSize, FontWeight fontWeight)
    {
        var fontFamily = FontFamily.Default;
        var fresh      = TextUtils.CalculateTextSize(text, fontSize, fontFamily, FontStyle.Normal, fontWeight);

        var first  = TextUtils.CalculateTextSizeCached(text, fontSize, fontFamily, FontStyle.Normal, fontWeight);
        var second = TextUtils.CalculateTextSizeCached(text, fontSize, fontFamily, FontStyle.Normal, fontWeight);

        first.ShouldBe(fresh);
        second.ShouldBe(fresh);
    }

    [Fact]
    public void Different_Font_Inputs_Do_Not_Share_A_Measurement()
    {
        const string text = "2028-12-28";
        var fontFamily = FontFamily.Default;

        var regular = TextUtils.CalculateTextSizeCached(text, 14, fontFamily, FontStyle.Normal, FontWeight.Normal);
        var larger  = TextUtils.CalculateTextSizeCached(text, 20, fontFamily, FontStyle.Normal, FontWeight.Normal);
        var bold    = TextUtils.CalculateTextSizeCached(text, 14, fontFamily, FontStyle.Normal, FontWeight.Bold);

        larger.ShouldBe(TextUtils.CalculateTextSize(text, 20, fontFamily, FontStyle.Normal, FontWeight.Normal));
        bold.ShouldBe(TextUtils.CalculateTextSize(text, 14, fontFamily, FontStyle.Normal, FontWeight.Bold));
        larger.Width.ShouldBeGreaterThan(regular.Width);
    }
}
