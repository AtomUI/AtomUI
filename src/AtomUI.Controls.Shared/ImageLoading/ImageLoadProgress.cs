namespace AtomUI.Controls;

/// <summary>
/// A snapshot of how far a load has advanced: the stage it is in and, while bytes are transferred, how many
/// of them have arrived.
/// </summary>
/// <param name="Stage">
/// The stage the load has entered.
/// </param>
/// <param name="BytesReceived">
/// The bytes transferred so far in this stage. It is 0 in stages that transfer nothing.
/// </param>
/// <param name="TotalBytes">
/// The total size announced for this stage, or <see langword="null" /> when it is unknown.
/// </param>
/// <param name="Fraction">
/// The transferred share from 0 to 1, or <see langword="null" /> when <paramref name="TotalBytes" /> is
/// <see langword="null" /> or not greater than zero.
/// </param>
public readonly record struct ImageLoadProgress(
    ImageLoadStage Stage,
    long BytesReceived,
    long? TotalBytes,
    double? Fraction)
{
    internal static ImageLoadProgress Create(
        ImageLoadStage stage,
        long bytesReceived = 0,
        long? totalBytes = null)
    {
        double? fraction = totalBytes > 0
            ? Math.Clamp((double)bytesReceived / totalBytes.Value, 0, 1)
            : null;
        return new ImageLoadProgress(stage, bytesReceived, totalBytes, fraction);
    }
}
