using Mono.Linker;

namespace AtomUI.TypeMap.Linker;

internal sealed class BackendDiagnostic : Exception
{
    internal int Number { get; }
    internal BackendDiagnostic(int number, string message) : base(message) => Number = number;
    internal void Report(LinkContext context) => context.LogMessage(MessageContainer.CreateCustomErrorMessage(
        $"ATOMUIREG{Number:000}: {Message}", 6500 + Number));
    internal static BackendDiagnostic Unsupported(string message) => new(6, message);
    internal static BackendDiagnostic Conflict(string message) => new(5, message);
    internal static BackendDiagnostic Unlowered(string message) => new(7, message);
}
