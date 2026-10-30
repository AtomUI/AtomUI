using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AtomUI.Registration.Shared;

internal static class CoreRegistrationModeNormalizer
{
    internal const string TemplateId = "atomui.trimmed-switch.v1";
    private const string SwitchName = "AtomUI.Registration.Trimmed";

    internal static void Normalize(AssemblyDefinition core,
        Func<TypeReference, TypeDefinition?> resolveType, Func<MethodReference, MethodDefinition?> resolveMethod)
    {
        var framework = core.CustomAttributes.SingleOrDefault(attribute => attribute.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute");
        if (core.Name.Name != "AtomUI.Core" || framework?.ConstructorArguments.Count != 1 ||
            !Equals(framework.ConstructorArguments[0].Value, ".NETCoreApp,Version=v8.0"))
        {
            throw Invalid("The net8 mode normalizer requires the validated net8 Core implementation.");
        }
        var owner = core.MainModule.GetType("AtomUI.Registration.ControlRegistrationRuntime") ?? throw Invalid("Missing registration runtime.");
        var getter = owner.Methods.SingleOrDefault(method => method.Name == "get_IsTrimmed") ?? throw Invalid("Missing IsTrimmed getter.");
        var property = owner.Properties.SingleOrDefault(property => property.Name == "IsTrimmed");
        var boolean = resolveType(getter.ReturnType);
        if (!owner.IsPublic || !owner.IsAbstract || !owner.IsSealed || owner.HasGenericParameters ||
            property?.GetMethod != getter || !getter.IsPublic || !getter.IsStatic || !getter.IsGetter || getter.HasParameters ||
            getter.HasGenericParameters || !getter.HasBody || getter.Body.HasExceptionHandlers || boolean?.FullName != "System.Boolean" ||
            boolean.Module.Assembly.Name.FullName != "System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e" ||
            getter.Body.Variables.Any(variable => resolveType(variable.VariableType) != boolean))
        {
            throw Invalid("Unknown IsTrimmed signature or local-variable contract.");
        }
        var body = getter.Body.Instructions.Where(instruction => instruction.OpCode.Code != Code.Nop).ToArray();
        if (body.Length is < 4 or > 64 || body.Count(instruction => instruction.OpCode.Code == Code.Ldstr) != 1 ||
            body.Count(instruction => instruction.OpCode.Code == Code.Call) != 1 ||
            !Equals(body.Single(instruction => instruction.OpCode.Code == Code.Ldstr).Operand, SwitchName))
        {
            throw Invalid("Unknown IsTrimmed instruction template.");
        }
        var call = body.Single(instruction => instruction.OpCode.Code == Code.Call).Operand as MethodReference;
        var target = call is null ? null : resolveMethod(call);
        if (call is null || call is GenericInstanceMethod || call.HasThis || call.ExplicitThis || call.CallingConvention != MethodCallingConvention.Default ||
            target is null || target.Name != "TryGetSwitch" || !target.IsPublic || !target.IsStatic || target.HasGenericParameters ||
            target.DeclaringType.FullName != "System.AppContext" || target.Module.Assembly != boolean.Module.Assembly ||
            target.Parameters.Count != 2 || resolveType(target.ReturnType) != boolean ||
            resolveType(target.Parameters[0].ParameterType) is not { FullName: "System.String" } textType || textType.Module.Assembly != boolean.Module.Assembly ||
            target.Parameters[1].ParameterType is not ByReferenceType result || resolveType(result.ElementType) != boolean)
        {
            throw Invalid("IsTrimmed must call the actual CoreLib AppContext.TryGetSwitch(string, bool&) method.");
        }
        ValidateStructure(body, getter.Body.Variables.Count);
        foreach (var found in new[] { false, true })
        {
            foreach (var enabled in new[] { false, true })
            {
                ValidateOutcome(body, getter.Body.Variables.Count, found, enabled);
            }
        }
        getter.DebugInformation.SequencePoints.Clear();
        getter.DebugInformation.Scope = null;
        getter.DebugInformation.StateMachineKickOffMethod = null;
        getter.DebugInformation.CustomDebugInformations.Clear();
        getter.CustomDebugInformations.Clear();
        getter.Body = new MethodBody(getter) { MaxStackSize = 1 };
        var il = getter.Body.GetILProcessor();
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Ret);
    }

    private static void ValidateStructure(Instruction[] body, int localCount)
    {
        foreach (var instruction in body)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldstr:
                case Code.Call:
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.And:
                case Code.Ret:
                    break;
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                case Code.Ldloca:
                case Code.Ldloca_S:
                    if (Local(instruction) is var index && (index < 0 || index >= localCount))
                    {
                        throw Invalid("Invalid IsTrimmed local.");
                    }
                    break;
                case Code.Br:
                case Code.Br_S:
                case Code.Brtrue:
                case Code.Brtrue_S:
                case Code.Brfalse:
                case Code.Brfalse_S:
                    if (Branch(body, instruction) <= Array.IndexOf(body, instruction))
                    {
                        throw Invalid("IsTrimmed template cannot contain backward or unknown branches.");
                    }
                    break;
                default:
                    throw Invalid("Unsupported instruction in IsTrimmed: " + instruction.OpCode);
            }
        }
    }

    private static void ValidateOutcome(Instruction[] body, int localCount, bool found, bool enabled)
    {
        var stack = new Stack<object>();
        var locals = new int?[localCount];
        var calls = 0;
        for (var index = 0; index < body.Length; index++)
        {
            var instruction = body[index];
            switch (instruction.OpCode.Code)
            {
                case Code.Ldstr: stack.Push(SwitchName); break;
                case Code.Ldloca:
                case Code.Ldloca_S: stack.Push(new LocalAddress(Local(instruction))); break;
                case Code.Call:
                    if (Pop(stack) is not LocalAddress address || !Equals(Pop(stack), SwitchName))
                    {
                        throw Invalid("Unexpected IsTrimmed call arguments.");
                    }
                    locals[address.Index] = enabled ? 1 : 0;
                    stack.Push(found ? 1 : 0);
                    calls++;
                    break;
                case Code.Ldc_I4_0: stack.Push(0); break;
                case Code.Ldc_I4_1: stack.Push(1); break;
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3: stack.Push(locals[Local(instruction)] ?? throw Invalid("Uninitialized IsTrimmed local.")); break;
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3: locals[Local(instruction)] = Boolean(Pop(stack)); break;
                case Code.And: stack.Push(Boolean(Pop(stack)) & Boolean(Pop(stack))); break;
                case Code.Br:
                case Code.Br_S: index = Branch(body, instruction) - 1; break;
                case Code.Brtrue:
                case Code.Brtrue_S:
                case Code.Brfalse:
                case Code.Brfalse_S:
                    var take = Boolean(Pop(stack)) != 0;
                    if (instruction.OpCode.Code is Code.Brfalse or Code.Brfalse_S)
                    {
                        take = !take;
                    }
                    if (take)
                    {
                        index = Branch(body, instruction) - 1;
                    }
                    break;
                case Code.Ret:
                    var actual = Boolean(Pop(stack));
                    if (stack.Count != 0 || calls != 1 || actual != (found && enabled ? 1 : 0))
                    {
                        throw Invalid("IsTrimmed must return found && enabled for every outcome.");
                    }
                    return;
            }
        }
        throw Invalid("IsTrimmed template does not return.");
    }

    private static int Local(Instruction instruction) => instruction.OpCode.Code switch
    {
        Code.Ldloc_0 or Code.Stloc_0 => 0,
        Code.Ldloc_1 or Code.Stloc_1 => 1,
        Code.Ldloc_2 or Code.Stloc_2 => 2,
        Code.Ldloc_3 or Code.Stloc_3 => 3,
        _ => instruction.Operand is VariableDefinition variable ? variable.Index : -1
    };

    private static int Branch(Instruction[] body, Instruction instruction)
    {
        var target = instruction.Operand as Instruction;
        while (target?.OpCode.Code == Code.Nop)
        {
            target = target.Next;
        }
        return target is null ? -1 : Array.IndexOf(body, target);
    }

    private static object Pop(Stack<object> stack) => stack.TryPop(out var value) ? value : throw Invalid("Unbalanced IsTrimmed evaluation stack.");
    private static int Boolean(object value) => value is int number && number is 0 or 1 ? number : throw Invalid("Non-boolean IsTrimmed value.");
    private static InvalidDataException Invalid(string message) => new(TemplateId + ": " + message);
    private readonly record struct LocalAddress(int Index);
}
