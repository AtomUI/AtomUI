using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

public sealed class VerifyConditionalRegistrationOutputTask : RegistrationBuildTask
{
    [Required] public string InputsPath { get; set; } = string.Empty;
    [Required] public string OutputDirectory { get; set; } = string.Empty;
    [Required] public string ReceiptPath { get; set; } = string.Empty;
    [Required] public string LinkerAssembly { get; set; } = string.Empty;
    public bool VerifyConsumed { get; set; }

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
