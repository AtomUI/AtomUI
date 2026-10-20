using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker;

internal sealed class VerifyTypeMapsStep : IStep
{
    private readonly IReadOnlyList<MapSlot> _slots;
    private readonly VerificationReceipt? _receipt;
    private readonly RegistrationMetadataIndex _metadata;

    internal VerifyTypeMapsStep(IReadOnlyList<MapSlot> slots, VerificationReceipt? receipt = null,
        RegistrationMetadataIndex? metadata = null)
    {
        _slots = slots;
        _receipt = receipt;
        _metadata = metadata ?? new RegistrationMetadataIndex();
    }

    public void Process(LinkContext context)
    {
        try
        {
            foreach (var slot in _slots)
            {
                Verify(context, slot);
            }

            if (_receipt is not null)
            {
                _receipt.SweepVerified = true;
            }

            context.LogMessage($"AtomUI TypeMap ABI 1: verified {_slots.Count} surviving accessor(s) after Sweep.");
        }
        catch (BackendDiagnostic diagnostic)
        {
            diagnostic.Report(context);
        }
    }

    private void Verify(LinkContext context, MapSlot slot)
    {
        var method = slot.Accessor;
        var assembly = slot.Group.Module?.Assembly;
        if (assembly is null)
        {
            throw BackendDiagnostic.Unlowered($"Accessor/helper/target for '{slot.AccessorIdentity}' did not survive Sweep with its exact marked identity.");
        }

        var types = _metadata.Types(assembly);
        // Keep the exact pre-Sweep MethodDefinitions. Sweep may discard the identifying attributes.
        if (!method.HasBody || method.DeclaringType is null || !method.DeclaringType.Methods.Contains(method) ||
            !types.Contains(method.DeclaringType) ||
            context.Annotations.GetAction(assembly) != AssemblyAction.Link ||
            !slot.Group.NestedTypes.Any(t => t.Methods.Contains(slot.Create) && t.Methods.Contains(slot.Add) && t.Methods.Contains(slot.Complete)) ||
            slot.Entries.Any(e => !types.Contains(e.Target)))
        {
            throw BackendDiagnostic.Unlowered($"Accessor/helper/target for '{slot.AccessorIdentity}' did not survive Sweep with its exact marked identity.");
        }

        slot.Contract.Validate(assembly, method, _metadata);
    }
}
