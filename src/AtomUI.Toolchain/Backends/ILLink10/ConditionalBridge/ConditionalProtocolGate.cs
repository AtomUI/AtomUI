using AtomUI.Build.Tasks.Registration;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

internal static class ConditionalProtocolGate
{
    internal static void Validate(IEnumerable<ConditionalRegistrationMetadata> metadata, string coreIdentity,
        Func<AssemblyNameReference, AssemblyDefinition?> resolve)
    {
        foreach (var input in metadata.Where(input => input.HasPackageMarker && input.Records.Count == 0))
        {
            var assembly = resolve(AssemblyNameReference.Parse(input.AssemblyIdentity)) ?? throw new InvalidDataException("Cannot resolve marked package.");
            var marker = assembly.CustomAttributes.Single(attribute => attribute.AttributeType.FullName == "AtomUI.Registration.ControlPackageMarkerAttribute");
            if (marker.ConstructorArguments.Count != 3 || marker.ConstructorArguments[1].Value is not TypeReference groupReference)
            {
                throw new InvalidDataException("Marked package has an invalid Group declaration.");
            }
            var group = groupReference.Resolve();
            var official = assembly.MainModule.GetTypes().SelectMany(type => type.Methods).Where(method =>
                method.IsStatic && method.HasBody && method.CustomAttributes.Any(attribute =>
                    attribute.AttributeType.FullName == "AtomUI.Registration.GeneratedTypeMapAccessorAttribute" &&
                    attribute.AttributeType.Resolve().Module.Assembly.Name.FullName == coreIdentity &&
                    attribute.ConstructorArguments.Count == 1 && attribute.ConstructorArguments[0].Value is TypeReference reference && reference.Resolve() == group))
                .Any(method => method.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Call &&
                    instruction.Operand is GenericInstanceMethod call && call.Name == "GetOrCreateExternalTypeMapping" &&
                    call.GenericArguments.Count == 1 && call.GenericArguments[0].Resolve() == group &&
                    call.DeclaringType.FullName == "System.Runtime.InteropServices.TypeMapping" &&
                    call.Resolve().Module.Assembly.Name.Name == "System.Private.CoreLib"));
            // An official empty map can have no TypeMap entry attributes. Its closed, marked accessor
            // is still protocol evidence; the later official backend validates its complete shape.
            if (!official)
            {
                throw new InvalidDataException("Marked control package has neither conditional records nor an official TypeMap accessor: " + input.AssemblyIdentity);
            }
        }
    }
}
