// Bound compiler state only; candidate transport is the shared ConditionalRecord contract.
using AtomUI.Build.Tasks.Registration;
using Internal.TypeSystem;

namespace ILCompiler;

internal static class RegistrationDefinitions
{
    internal sealed record Entry(string Key, string FragmentId, RegistrationTypeIdentity Trigger,
        TypeDesc Condition, MethodDesc Method);

    internal sealed class Group
    {
        public string Id { get; init; }
        public string PackageId { get; init; }
        public TypeDesc Builder { get; init; }
        public MethodDesc Collector { get; init; }
        public MethodDesc Entry { get; init; }
        public MethodDesc FullCollector { get; init; }
        public Entry[] Entries { get; init; }
        public string SymbolName { get; init; }
        public RegistrationTableNode Table { get; set; }
        public Entry[] FrozenEntries { get; set; }
        public bool ScannerRequested { get; set; }
    }
}
