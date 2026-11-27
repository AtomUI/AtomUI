using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// MSBuild task that verifies the conditional registration output of a link run, or that the SDK publish consumed
/// the verified output recorded in an existing receipt.
/// </summary>
public sealed class VerifyConditionalRegistrationOutputTask : RegistrationBuildTask
{
    /// <summary>
    /// The path of the conditional registration input snapshot. The analysis report to verify with is read from the
    /// <c>analysisReportPath</c> it names.
    /// </summary>
    [Required] public string InputsPath { get; set; } = string.Empty;
    /// <summary>
    /// The directory that holds the linker output to verify. With <see cref="VerifyConsumed" /> it is the SDK
    /// publish directory, which must hold every output recorded in the receipt.
    /// </summary>
    [Required] public string OutputDirectory { get; set; } = string.Empty;
    /// <summary>
    /// The path of the verification receipt. The default mode writes a receipt for the verified outputs there; with
    /// <see cref="VerifyConsumed" /> the existing receipt is checked and rewritten as consumed.
    /// </summary>
    [Required] public string ReceiptPath { get; set; } = string.Empty;
    /// <summary>
    /// The path of the linker assembly whose content hash is recorded with the receipt and checked against it.
    /// </summary>
    [Required] public string LinkerAssembly { get; set; } = string.Empty;
    /// <summary>
    /// When <see langword="true" />, checks that every output recorded in the existing receipt is present, with
    /// its recorded hash, under <see cref="OutputDirectory" />, and marks the receipt as consumed. When
    /// <see langword="false" />, verifies the linker output and writes a new receipt.
    /// </summary>
    public bool VerifyConsumed { get; set; }

    /// <summary>
    /// Verifies the conditional registration output, or its consumption by the SDK, as selected by
    /// <see cref="VerifyConsumed" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when verification passed; <see langword="false" /> when it failed and the failure was
    /// logged as <c>ATOMUIREG007</c>. A receipt is deleted before <see langword="false" /> is returned.
    /// </returns>
    public override bool Execute()
    {
        try
        {
            if (VerifyConsumed)
            {
                ConditionalRegistrationOutputVerifier.VerifyConsumed(InputsPath, ReceiptPath, OutputDirectory, LinkerAssembly);
            }
            else
            {
                ConditionalRegistrationOutputVerifier.Verify(InputsPath, OutputDirectory, ReceiptPath, LinkerAssembly);
            }
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or
                                      ArgumentException or BadImageFormatException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            if (File.Exists(ReceiptPath))
            {
                File.Delete(ReceiptPath);
            }
            return Fail("ATOMUIREG007", error);
        }
    }
}
