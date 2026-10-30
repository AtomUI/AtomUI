using System.Buffers.Binary;
using Internal.IL;
using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

namespace ILCompiler;

// atomui.trimmed-switch.v1: mirror the managed backend's bounded semantic template.
// This recognizes the existing getter, not arbitrary methods with a matching name.
internal static class TrimmedSwitchTemplate
{
    internal const string Id = "atomui.trimmed-switch.v1";
    internal const string SwitchName = "AtomUI.Registration.Trimmed";
    private sealed record Operation(int Offset, int Next, int Code, object Operand);
    private sealed record LocalAddress(int Index);

    public static void Validate(MethodDesc getter, TypeDesc booleanType, ILProvider provider)
    {
        if (getter is not EcmaMethod || !getter.IsPublic || getter.Signature.Flags != MethodSignatureFlags.Static ||
            getter.Signature.Length != 0 || getter.Signature.ReturnType != booleanType || getter.HasInstantiation)
        {
            throw new InvalidDataException("Invalid trimmed-switch getter signature");
        }
        MethodIL body = provider.GetMethodIL(getter);
        if (body == null || body.GetExceptionRegions().Length != 0 || body.GetILBytes().Length > 256)
        {
            throw new InvalidDataException("Unsupported trimmed-switch getter body");
        }
        var locals = body.GetLocals();
        if (locals.Any(local => local.Type != booleanType || local.IsPinned))
        {
            throw new InvalidDataException("Trimmed-switch template accepts only bool locals");
        }
        TypeSystemContext context = getter.Context;
        MetadataType appContext = context.SystemModule.GetKnownType("System", "AppContext");
        MethodDesc expectedCall = appContext.GetMethods().Single(m => m.Name == "TryGetSwitch" &&
            m.Signature.Flags == MethodSignatureFlags.Static && m.Signature.ReturnType == booleanType &&
            m.Signature.Length == 2 && m.Signature[0] == context.GetWellKnownType(WellKnownType.String) &&
            m.Signature[1] == booleanType.MakeByRefType());
        List<Operation> operations = Decode(body, expectedCall, locals.Length);
        foreach (bool found in new[] { false, true })
        {
            foreach (bool value in new[] { false, true })
            {
                if (Evaluate(operations, locals.Length, found, value) != (found && value))
                {
                    throw new InvalidDataException("Trimmed-switch getter does not implement found && value");
                }
            }
        }
    }

    private static List<Operation> Decode(MethodIL body, MethodDesc expectedCall, int localCount)
    {
        byte[] bytes = body.GetILBytes();
        var result = new List<Operation>();
        int position = 0;
        int calls = 0;
        int strings = 0;
        while (position < bytes.Length)
        {
            int offset = position;
            int code = bytes[position++];
            if (code == 0xfe)
            {
                if (position == bytes.Length)
                {
                    throw new InvalidDataException("Truncated trimmed-switch opcode");
                }
                code = 0xfe00 | bytes[position++];
            }
            object operand = null;
            if (code is 0x72 or 0x28)
            {
                RequireBytes(bytes, position, 4);
                operand = body.GetObject(BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(position, 4)));
                position += 4;
                if (code == 0x72)
                {
                    strings++;
                    if (!Equals(operand, SwitchName))
                    {
                        throw new InvalidDataException("Trimmed-switch getter queries another key");
                    }
                }
                else
                {
                    calls++;
                    if (!Equals(operand, expectedCall))
                    {
                        throw new InvalidDataException("Trimmed-switch getter calls an unexpected method");
                    }
                }
            }
            else if (code is >= 0x2b and <= 0x2d)
            {
                RequireBytes(bytes, position, 1);
                int delta = unchecked((sbyte)bytes[position++]);
                operand = position + delta;
            }
            else if (code is >= 0x38 and <= 0x3a)
            {
                RequireBytes(bytes, position, 4);
                int delta = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(position, 4));
                position += 4;
                operand = position + delta;
            }
            else if (code is >= 0x11 and <= 0x13)
            {
                RequireBytes(bytes, position, 1);
                operand = (int)bytes[position++];
            }
            else if (code is >= 0xfe0c and <= 0xfe0e)
            {
                RequireBytes(bytes, position, 2);
                operand = (int)BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(position, 2));
                position += 2;
            }
            else if (code is not (0x00 or >= 0x06 and <= 0x0d or 0x16 or 0x17 or 0x2a or 0x5f))
            {
                throw new InvalidDataException("Unexpected trimmed-switch template opcode: " + code);
            }
            result.Add(new Operation(offset, position, code, operand));
        }
        if (calls != 1 || strings != 1)
        {
            throw new InvalidDataException("Trimmed-switch getter must contain one fixed lookup");
        }
        foreach (Operation operation in result)
        {
            if (operation.Code is >= 0x2b and <= 0x2d or >= 0x38 and <= 0x3a)
            {
                int target = (int)operation.Operand;
                if (target <= operation.Offset || !result.Any(op => op.Offset == target))
                {
                    throw new InvalidDataException("Trimmed-switch getter must have valid forward branches");
                }
            }
            int local = LocalIndex(operation);
            if (local >= localCount)
            {
                throw new InvalidDataException("Invalid trimmed-switch local index");
            }
        }
        return result;
    }

    private static bool Evaluate(List<Operation> operations, int localCount, bool found, bool value)
    {
        var byOffset = operations.ToDictionary(op => op.Offset);
        var stack = new Stack<object>();
        var locals = new bool?[localCount];
        int pc = 0;
        int calls = 0;
        for (int steps = 0; steps <= operations.Count; steps++)
        {
            if (!byOffset.TryGetValue(pc, out Operation op))
            {
                throw new InvalidDataException("Trimmed-switch path has no return");
            }
            pc = op.Next;
            int local = LocalIndex(op);
            if (op.Code is 0x06 or 0x07 or 0x08 or 0x09 or 0x11 or 0xfe0c)
            {
                stack.Push(locals[local] ?? throw new InvalidDataException("Uninitialized switch local"));
            }
            else if (op.Code is 0x0a or 0x0b or 0x0c or 0x0d or 0x13 or 0xfe0e)
            {
                locals[local] = (bool)stack.Pop();
            }
            else if (op.Code is 0x12 or 0xfe0d)
            {
                stack.Push(new LocalAddress(local));
            }
            else if (op.Code is 0x16 or 0x17)
            {
                stack.Push(op.Code == 0x17);
            }
            else if (op.Code == 0x72)
            {
                stack.Push(op.Operand);
            }
            else if (op.Code == 0x28)
            {
                int index = ((LocalAddress)stack.Pop()).Index;
                if (!Equals(stack.Pop(), SwitchName))
                {
                    throw new InvalidDataException("Unexpected trimmed-switch argument flow");
                }
                locals[index] = value;
                stack.Push(found);
                calls++;
            }
            else if (op.Code is 0x2b or 0x38)
            {
                pc = (int)op.Operand;
            }
            else if (op.Code is 0x2c or 0x39 or 0x2d or 0x3a)
            {
                bool condition = (bool)stack.Pop();
                bool branchWhenTrue = op.Code is 0x2d or 0x3a;
                if (condition == branchWhenTrue)
                {
                    pc = (int)op.Operand;
                }
            }
            else if (op.Code == 0x5f)
            {
                bool right = (bool)stack.Pop();
                bool left = (bool)stack.Pop();
                stack.Push(left && right);
            }
            else if (op.Code == 0x2a)
            {
                if (calls != 1 || stack.Count != 1 || stack.Peek() is not bool)
                {
                    throw new InvalidDataException("Invalid trimmed-switch return flow");
                }
                return (bool)stack.Pop();
            }
        }
        throw new InvalidDataException("Trimmed-switch path did not terminate");
    }

    private static int LocalIndex(Operation op) => op.Code switch
    {
        >= 0x06 and <= 0x09 => op.Code - 0x06,
        >= 0x0a and <= 0x0d => op.Code - 0x0a,
        >= 0x11 and <= 0x13 or >= 0xfe0c and <= 0xfe0e => (int)op.Operand,
        _ => -1
    };

    private static void RequireBytes(byte[] bytes, int offset, int count)
    {
        if (offset + count > bytes.Length)
        {
            throw new InvalidDataException("Truncated trimmed-switch operand");
        }
    }
}
