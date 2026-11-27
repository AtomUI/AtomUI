using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

/// <summary>
/// Installs the scoped-theme integration into the application-local Avalonia runtime input.
/// </summary>
internal static class ScopedThemeAdapter
{
    private const string Avalonia122Net10Hash = "007AB726F2BD1F90FB275C90B11F7DC8DDA33E0AFAE382E538F251F30438098D";

    public static void Prepare(string input, string output)
    {
        if (string.Equals(Path.GetFullPath(input), Path.GetFullPath(output), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Scoped theme output must be separate from the Avalonia package input.");
        }
        using var source = File.OpenRead(input);
        if (Convert.ToHexString(SHA256.HashData(source)) != Avalonia122Net10Hash)
        {
            throw new InvalidDataException($"Scoped themes require the official Avalonia 12.2.0 net10.0 runtime assembly. Unexpected input: '{input}'.");
        }
        source.Position = 0;
        using var assembly = AssemblyDefinition.ReadAssembly(source);
        var module = assembly.MainModule;
        var element = module.GetType("Avalonia.StyledElement");
        var eligibility = AddCallback(element, "_atomuiScopedThemeTarget", module.TypeSystem.Boolean);
        var resolver = AddCallback(element, "_atomuiScopedThemeResource", module.TypeSystem.Object, module.TypeSystem.Object);
        var refresh = AddCallback(element, "_atomuiScopedThemeRefresh", module.TypeSystem.Void);
        var canResolve = AddMethod(element, "UsesAtomUIScopedTheme", module.TypeSystem.Boolean);
        var body = canResolve.Body.GetILProcessor();
        var invoke = body.Create(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldsfld, eligibility);
        body.Emit(OpCodes.Dup);
        body.Emit(OpCodes.Brtrue, invoke);
        body.Emit(OpCodes.Pop);
        body.Emit(OpCodes.Ldc_I4_0);
        body.Emit(OpCodes.Ret);
        body.Append(invoke);
        body.Emit(OpCodes.Callvirt, GetInvoke(eligibility));
        body.Emit(OpCodes.Ret);

        var apply = FindMethod(element, "ApplyStyling", 0);
        var getTheme = FindMethod(element, "GetEffectiveTheme", 0);
        var prepare = AddMethod(element, "PrepareAtomUIScopedThemeSources", module.TypeSystem.Void);
        EmitStylePreparation(prepare, apply, canResolve);
        foreach (var entry in new[] { apply, getTheme })
        {
            var il = entry.Body.GetILProcessor();
            var start = entry.Body.Instructions[0];
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(start, il.Create(OpCodes.Call, prepare));
        }

        DeferImplicitLookupDuringInitialization(getTheme, canResolve);
        RouteImplicitLookup(getTheme, canResolve, resolver);
        RefreshImplicitThemeOnResourceChange(element, canResolve, refresh);
        DetachStylesFromStylingChildren(element, eligibility);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        var pending = output + "." + Guid.NewGuid().ToString("N");
        try
        {
            assembly.Write(pending, new WriterParameters { DeterministicMvid = true, Timestamp = 0 });
            File.Move(pending, output, overwrite: true);
        }
        finally
        {
            File.Delete(pending);
        }
    }

    private static void EmitStylePreparation(MethodDefinition prepare, MethodDefinition apply, MethodDefinition canResolve)
    {
        var element = prepare.DeclaringType;
        var il = prepare.Body.GetILProcessor();
        var done = il.Create(OpCodes.Ret);
        var getStore = FindCalledMethod(apply, "GetValueStore");
        var begin = FindCalledMethod(apply, "BeginStyling");
        var end = FindCalledMethod(apply, "EndStyling");
        var stylesApplied = FindField(element, "_stylesApplied");
        var parentApplied = FindField(element, "_templatedParentThemeApplied");

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, stylesApplied);
        il.Emit(OpCodes.Brtrue, done);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, FindField(element, "_initCount"));
        il.Emit(OpCodes.Brtrue, done);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, canResolve);
        il.Emit(OpCodes.Brfalse, done);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, getStore);
        il.Emit(OpCodes.Callvirt, begin);

        // Match the native style order. Both sources must settle before implicit lookup.
        var tryStart = il.Create(OpCodes.Ldarg_0);
        var applyStyles = il.Create(OpCodes.Ldarg_0);
        il.Append(tryStart);
        il.Emit(OpCodes.Ldfld, parentApplied);
        il.Emit(OpCodes.Brtrue, applyStyles);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, FindCalledMethod(apply, "ApplyTemplatedParentControlTheme"));
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stfld, parentApplied);
        il.Append(applyStyles);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, FindCalledMethod(apply, "ApplyStyles"));
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stfld, stylesApplied);
        il.Emit(OpCodes.Leave, done);

        var finallyStart = il.Create(OpCodes.Ldarg_0);
        il.Append(finallyStart);
        il.Emit(OpCodes.Call, getStore);
        il.Emit(OpCodes.Callvirt, end);
        il.Emit(OpCodes.Endfinally);
        il.Append(done);
        prepare.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = tryStart,
            TryEnd = finallyStart,
            HandlerStart = finallyStart,
            HandlerEnd = done,
        });
    }

    private static void DeferImplicitLookupDuringInitialization(MethodDefinition getTheme, MethodDefinition canResolve)
    {
        var instructions = getTheme.Body.Instructions;
        var implicitCache = instructions.First(op => op.OpCode == OpCodes.Ldfld &&
            op.Operand is FieldReference { Name: "_implicitTheme" }).Previous;
        var il = getTheme.Body.GetILProcessor();
        var start = il.Create(OpCodes.Ldarg_0);
        foreach (var op in instructions.Where(op => ReferenceEquals(op.Operand, implicitCache)))
        {
            op.Operand = start;
        }
        il.InsertBefore(implicitCache, start);
        il.InsertBefore(implicitCache, il.Create(OpCodes.Ldfld, FindField(getTheme.DeclaringType, "_initCount")));
        il.InsertBefore(implicitCache, il.Create(OpCodes.Brfalse, implicitCache));
        il.InsertBefore(implicitCache, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(implicitCache, il.Create(OpCodes.Call, canResolve));
        il.InsertBefore(implicitCache, il.Create(OpCodes.Brfalse, implicitCache));
        il.InsertBefore(implicitCache, il.Create(OpCodes.Ldnull));
        il.InsertBefore(implicitCache, il.Create(OpCodes.Ret));
    }

    private static void RouteImplicitLookup(MethodDefinition getTheme, MethodDefinition canResolve, FieldDefinition resolver)
    {
        var element = getTheme.DeclaringType;
        var module = element.Module;
        var call = getTheme.Body.Instructions.Single(op => op.Operand is MethodReference { Name: "TryFindResource" });
        var nativeFind = (MethodReference)call.Operand;
        var find = AddMethod(element, "FindAtomUIImplicitThemeResource", module.TypeSystem.Boolean, isStatic: true);
        foreach (var parameter in nativeFind.Parameters)
        {
            find.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        }

        var il = find.Body.GetILProcessor();
        var native = il.Create(OpCodes.Ldarg_0);
        var invoke = il.Create(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, element);
        il.Emit(OpCodes.Call, canResolve);
        il.Emit(OpCodes.Brfalse, native);
        il.Emit(OpCodes.Ldsfld, resolver);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Brtrue, invoke);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Br, native);
        il.Append(invoke);
        // Store the returned object to the native out parameter. Avalonia checks its type.
        var value = new VariableDefinition(module.TypeSystem.Object);
        find.Body.Variables.Add(value);
        il.Emit(OpCodes.Castclass, element);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Callvirt, GetInvoke(resolver));
        il.Emit(OpCodes.Stloc, value);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, value);
        il.Emit(OpCodes.Stind_Ref);
        il.Emit(OpCodes.Ldloc, value);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Cgt_Un);
        il.Emit(OpCodes.Ret);
        il.Append(native);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Call, nativeFind);
        il.Emit(OpCodes.Ret);
        call.Operand = find;
    }

    private static void RefreshImplicitThemeOnResourceChange(TypeDefinition element, MethodDefinition canResolve, FieldDefinition refresh)
    {
        var notify = FindMethod(element, "NotifyResourcesChanged", 2);
        var afterDeduplication = notify.Body.Instructions.Single(instruction =>
            instruction.OpCode == OpCodes.Stfld &&
            instruction.Operand is FieldReference { Name: "_lastResourcesChangedEventArgs" }).Next;
        var il = notify.Body.GetILProcessor();
        var invoke = il.Create(OpCodes.Ldarg_0);
        void Emit(Instruction instruction) => il.InsertBefore(afterDeduplication, instruction);

        // Initial attach already resolved the theme, and native invalidation clears the cache
        // before looking again. Reentrant notifications cannot restart that lookup.
        Emit(il.Create(OpCodes.Ldarg_2));
        Emit(il.Create(OpCodes.Brfalse, afterDeduplication));
        Emit(il.Create(OpCodes.Ldarg_0));
        Emit(il.Create(OpCodes.Ldfld, FindField(element, "_logicalRoot")));
        Emit(il.Create(OpCodes.Brfalse, afterDeduplication));
        Emit(il.Create(OpCodes.Ldarg_0));
        Emit(il.Create(OpCodes.Ldfld, FindField(element, "_implicitTheme")));
        Emit(il.Create(OpCodes.Brfalse, afterDeduplication));
        Emit(il.Create(OpCodes.Ldarg_0));
        Emit(il.Create(OpCodes.Call, canResolve));
        Emit(il.Create(OpCodes.Brfalse, afterDeduplication));
        Emit(il.Create(OpCodes.Ldsfld, refresh));
        Emit(il.Create(OpCodes.Dup));
        Emit(il.Create(OpCodes.Brtrue, invoke));
        Emit(il.Create(OpCodes.Pop));
        Emit(il.Create(OpCodes.Br, afterDeduplication));
        Emit(invoke);
        Emit(il.Create(OpCodes.Callvirt, GetInvoke(refresh)));
    }

    private static void DetachStylesFromStylingChildren(TypeDefinition element, FieldDefinition eligibility)
    {
        var detach = FindMethod(element, "DetachStyles", 1);
        var invalidate = FindMethod(element, "InvalidateStyles", 1);
        var getChildren = FindCalledMethod(invalidate, "GetInheritanceChildren");
        var getInheritanceParent = FindMethod(element.Module.GetType("Avalonia.AvaloniaObject"), "get_InheritanceParent", 0);
        var native = detach.Body.Instructions.First(instruction =>
            instruction.OpCode == OpCodes.Ldfld && instruction.Operand is FieldReference { Name: "_logicalChildren" }).Previous;
        var nativeCall = detach.Body.Instructions.Single(instruction =>
            instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference { Name: "DetachStyles" });

        var detachLogical = AddMethod(element, "DetachAtomUILogicalChildStyles", element.Module.TypeSystem.Void, isStatic: true);
        detachLogical.Parameters.Add(new ParameterDefinition(element));
        detachLogical.Parameters.Add(new ParameterDefinition(detach.Parameters[0].ParameterType));
        detachLogical.Parameters.Add(new ParameterDefinition(element));
        var helperIl = detachLogical.Body.GetILProcessor();
        var callDetach = helperIl.Create(OpCodes.Ldarg_0);
        var skipDetach = helperIl.Create(OpCodes.Ret);
        helperIl.Emit(OpCodes.Ldsfld, eligibility);
        helperIl.Emit(OpCodes.Brfalse, callDetach);
        helperIl.Emit(OpCodes.Ldarg_0);
        helperIl.Emit(OpCodes.Call, getInheritanceParent);
        helperIl.Emit(OpCodes.Ldarg_2);
        helperIl.Emit(OpCodes.Beq, skipDetach);
        helperIl.Append(callDetach);
        helperIl.Emit(OpCodes.Ldarg_1);
        helperIl.Emit(OpCodes.Call, detach);
        helperIl.Append(skipDetach);

        var il = detach.Body.GetILProcessor();
        il.InsertBefore(nativeCall, il.Create(OpCodes.Ldarg_0));
        nativeCall.Operand = detachLogical;

        var entry = il.Create(OpCodes.Ldsfld, eligibility);
        foreach (var instruction in detach.Body.Instructions.Where(instruction => ReferenceEquals(instruction.Operand, native)))
        {
            instruction.Operand = entry;
        }
        foreach (var handler in detach.Body.ExceptionHandlers.Where(handler => ReferenceEquals(handler.HandlerEnd, native)))
        {
            handler.HandlerEnd = entry;
        }
        var index = new VariableDefinition(element.Module.TypeSystem.Int32);
        var condition = il.Create(OpCodes.Ldloc, index);
        var children = new VariableDefinition(getChildren.ReturnType);
        var child = new VariableDefinition(element);
        detach.Body.Variables.Add(index);
        detach.Body.Variables.Add(children);
        detach.Body.Variables.Add(child);
        var start = il.Create(OpCodes.Ldloc, children);
        var next = il.Create(OpCodes.Ldloc, index);
        void Emit(Instruction instruction) => il.InsertBefore(native, instruction);

        // Styling follows InheritanceParent, while reparented hosts can still reside in _logicalChildren
        // with a redirected InheritanceParent. Traverse InheritanceChildren first, then fall through to
        // _logicalChildren and skip children already visited via InheritanceParent == this.
        Emit(entry);
        Emit(il.Create(OpCodes.Brfalse, native));
        Emit(il.Create(OpCodes.Ldarg_0));
        Emit(il.Create(OpCodes.Call, getChildren));
        Emit(il.Create(OpCodes.Stloc, children));
        Emit(il.Create(OpCodes.Ldloc, children));
        Emit(il.Create(OpCodes.Brfalse, native));
        Emit(il.Create(OpCodes.Ldc_I4_0));
        Emit(il.Create(OpCodes.Stloc, index));
        Emit(il.Create(OpCodes.Br, condition));
        Emit(start);
        Emit(il.Create(OpCodes.Ldloc, index));
        Emit(il.Create(OpCodes.Callvirt, FindCalledMethod(invalidate, "get_Item")));
        Emit(il.Create(OpCodes.Isinst, element));
        Emit(il.Create(OpCodes.Stloc, child));
        Emit(il.Create(OpCodes.Ldloc, child));
        Emit(il.Create(OpCodes.Brfalse, next));
        Emit(il.Create(OpCodes.Ldloc, child));
        Emit(il.Create(OpCodes.Ldarg_1));
        Emit(il.Create(OpCodes.Call, detach));
        Emit(next);
        Emit(il.Create(OpCodes.Ldc_I4_1));
        Emit(il.Create(OpCodes.Add));
        Emit(il.Create(OpCodes.Stloc, index));
        Emit(condition);
        Emit(il.Create(OpCodes.Ldloc, children));
        Emit(il.Create(OpCodes.Callvirt, FindCalledMethod(invalidate, "get_Count")));
        Emit(il.Create(OpCodes.Blt, start));
    }

    private static FieldDefinition AddCallback(TypeDefinition element, string name, TypeReference result, params TypeReference[] arguments)
    {
        // Reference the same System.Runtime contract used by Avalonia, not the tool's runtime implementation.
        var runtime = element.Module.AssemblyReferences.Single(reference => reference.Name == "System.Runtime");
        var returnsValue = result.MetadataType != MetadataType.Void;
        var arity = arguments.Length + (returnsValue ? 2 : 1);
        var delegateName = returnsValue ? "Func" : "Action";
        var definition = new TypeReference("System", delegateName + "`" + arity, element.Module, runtime);
        for (var index = 0; index < arity; index++)
        {
            definition.GenericParameters.Add(new GenericParameter(definition));
        }
        var signature = new GenericInstanceType(definition);
        signature.GenericArguments.Add(element);
        foreach (var argument in arguments)
        {
            signature.GenericArguments.Add(argument);
        }
        if (returnsValue)
        {
            signature.GenericArguments.Add(result);
        }
        var field = new FieldDefinition(name, FieldAttributes.Private | FieldAttributes.Static, signature);
        element.Fields.Add(field);
        return field;
    }

    private static MethodReference GetInvoke(FieldDefinition callback)
    {
        var signature = (GenericInstanceType)callback.FieldType;
        var parameters = signature.ElementType.GenericParameters;
        var returnsValue = signature.ElementType.Name.StartsWith("Func`", StringComparison.Ordinal);
        var parameterCount = parameters.Count - (returnsValue ? 1 : 0);
        var invoke = new MethodReference("Invoke", returnsValue ? parameters[^1] : callback.Module.TypeSystem.Void, signature) { HasThis = true };
        for (var index = 0; index < parameterCount; index++)
        {
            invoke.Parameters.Add(new ParameterDefinition(parameters[index]));
        }
        return invoke;
    }

    private static MethodDefinition AddMethod(TypeDefinition owner, string name, TypeReference result, bool isStatic = false)
    {
        var flags = MethodAttributes.Private | MethodAttributes.HideBySig;
        if (isStatic)
        {
            flags |= MethodAttributes.Static;
        }
        var method = new MethodDefinition(name, flags, result);
        owner.Methods.Add(method);
        return method;
    }

    private static MethodReference FindCalledMethod(MethodDefinition method, string name) =>
        method.Body.Instructions.Select(op => op.Operand).OfType<MethodReference>().First(called => called.Name == name);

    private static MethodDefinition FindMethod(TypeDefinition type, string name, int parameters) =>
        type.Methods.Single(method => method.Name == name && method.Parameters.Count == parameters);

    private static FieldDefinition FindField(TypeDefinition type, string name) =>
        type.Fields.Single(field => field.Name == name);
}
