using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class TokenWriteRecordingTests
{
    [Fact]
    public void Writes_Are_Not_Recorded_Unless_Recording_Is_On()
    {
        var token = new RecordingProbeToken();
        token.Width = 10;
        token.Background = Colors.Red;

        token.IsRecordingWrites.ShouldBeFalse();
        token.BeginRecordingWrites();
        token.EndRecordingWrites().ShouldBeEmpty();
    }

    [Fact]
    public void Recording_Captures_Names_In_First_Write_Order_Without_Duplicates()
    {
        var token = new RecordingProbeToken();
        token.BeginRecordingWrites();

        token.Background = Colors.Red;
        token.Width      = 4;
        token.Background = Colors.Blue;
        token.Padding    = new Thickness(2);
        token.Width      = 8;

        token.EndRecordingWrites().ShouldBe(
            [nameof(RecordingProbeToken.Background), nameof(RecordingProbeToken.Width), nameof(RecordingProbeToken.Padding)]);
    }

    [Fact]
    public void Writing_The_Default_Value_Is_Still_Recorded()
    {
        var token = new RecordingProbeToken();
        token.BeginRecordingWrites();

        token.Background = Colors.Transparent;
        token.Width      = 0;
        token.Padding    = default(Thickness);

        token.EndRecordingWrites().ShouldBe(
            [nameof(RecordingProbeToken.Background), nameof(RecordingProbeToken.Width), nameof(RecordingProbeToken.Padding)]);
    }

    [Fact]
    public void EndRecording_Turns_Recording_Off()
    {
        var token = new RecordingProbeToken();
        token.BeginRecordingWrites();
        token.Width = 1;
        token.IsRecordingWrites.ShouldBeTrue();

        token.EndRecordingWrites().ShouldBe([nameof(RecordingProbeToken.Width)]);
        token.IsRecordingWrites.ShouldBeFalse();

        token.Background = Colors.Red;
        token.BeginRecordingWrites();
        token.EndRecordingWrites().ShouldBeEmpty();
    }

    [Fact]
    public void BeginRecording_Twice_Throws()
    {
        var token = new RecordingProbeToken();
        token.BeginRecordingWrites();

        Should.Throw<InvalidOperationException>(() => token.BeginRecordingWrites());
    }

    [Fact]
    public void A_Token_That_Never_Records_Allocates_No_Log()
    {
        var token = new RecordingProbeToken();
        WriteMany(token, 1000);

        var before = GC.GetAllocatedBytesForCurrentThread();
        WriteMany(token, 1000);
        var after = GC.GetAllocatedBytesForCurrentThread();

        (after - before).ShouldBe(0L);
        token.IsRecordingWrites.ShouldBeFalse();
    }

    private static void WriteMany(RecordingProbeToken token, int count)
    {
        for (var i = 0; i < count; i++)
        {
            token.Width      = i;
            token.Background = Colors.Transparent;
            token.Padding    = default;
        }
    }

    // Mirrors the shape the generator emits for a partial token property.
    private sealed class RecordingProbeToken : AbstractDesignToken
    {
        public double Width
        {
            get => field;
            set
            {
                field = value;
                RecordWrite(nameof(Width));
            }
        }

        public Color Background
        {
            get => field;
            set
            {
                field = value;
                RecordWrite(nameof(Background));
            }
        }

        public Thickness Padding
        {
            get => field;
            set
            {
                field = value;
                RecordWrite(nameof(Padding));
            }
        }
    }
}
