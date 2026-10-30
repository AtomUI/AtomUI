using AtomUI.Registration.Shared;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

/// <summary>Converts validated net8 declarations to official TypeMap10 input. It never selects fragments.</summary>
internal sealed class ConditionalTypeMapBridge
{
    private readonly AssemblyDefinition _application;
    private readonly AssemblyDefinition _core;
    private readonly AssemblyDefinition _coreLib;

    internal ConditionalTypeMapBridge(AssemblyDefinition application, AssemblyDefinition core, AssemblyDefinition coreLib)
    {
        _application = application;
        _core = core;
        _coreLib = coreLib;
    }

    internal void Apply(IReadOnlyList<BoundConditionalRegistrationGroup> groups)
    {
        // Validate all added member slots before changing any collector.
        foreach (var group in groups)
        {
            if (group.Collector.DeclaringType.Methods.Any(method => method.Name == "GetConditionalTypeMap"))
            {
                throw new InvalidDataException("Conditional TypeMap accessor already exists: " + group.Identity);
            }
            RequireBrowserHelper(group.GroupType);
            var framework = group.Collector.Module.Assembly.CustomAttributes.SingleOrDefault(attribute =>
                attribute.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute");
            if (framework?.ConstructorArguments.Count != 1 || !Equals(framework.ConstructorArguments[0].Value, ".NETCoreApp,Version=v8.0"))
            {
                throw new InvalidDataException("The conditional bridge accepts only generated net8 package implementations.");
            }
        }
        foreach (var group in groups)
        {
            Apply(group);
        }
    }

    private void Apply(BoundConditionalRegistrationGroup group)
    {
        var module = group.Collector.Module;
        var systemType = PlatformType("System.Type");
        var query = PlatformType("System.Runtime.InteropServices.TypeMapping").Methods.Single(method =>
            method.Name == "GetOrCreateExternalTypeMapping" && method.IsStatic && method.GenericParameters.Count == 1 && method.Parameters.Count == 0);
        var accessor = new MethodDefinition("GetConditionalTypeMap", MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.ImportReference(query.ReturnType));
        group.Collector.DeclaringType.Methods.Add(accessor);
        var closedQuery = new GenericInstanceMethod(module.ImportReference(query));
        closedQuery.GenericArguments.Add(group.GroupType);
        var queryIl = accessor.Body.GetILProcessor();
        queryIl.Emit(OpCodes.Call, closedQuery);
        queryIl.Emit(OpCodes.Ret);

        var accessorAttribute = _core.MainModule.GetType("AtomUI.Registration.GeneratedTypeMapAccessorAttribute") ??
            throw new InvalidDataException("Core lacks the generated accessor ABI.");
        var accessorMarker = new CustomAttribute(module.ImportReference(accessorAttribute.Methods.Single(method => method.IsConstructor && method.Parameters.Count == 1)));
        accessorMarker.ConstructorArguments.Add(new(module.ImportReference(systemType), group.GroupType));
        accessor.CustomAttributes.Add(accessorMarker);
        PreserveBrowserHelper(accessor, RequireBrowserHelper(group.GroupType));

        var keys = new List<string>();
        var uniqueKeys = new HashSet<string>(StringComparer.Ordinal);
        var entryAttribute = PlatformType("System.Runtime.InteropServices.TypeMapAttribute`1");
        foreach (var fragment in group.Fragments.OrderBy(fragment => fragment.CollectionOrderKey, StringComparer.Ordinal))
        {
            foreach (var condition in fragment.Conditions)
            {
                var key = "conditional-v1:" + condition.Identity;
                if (!uniqueKeys.Add(key))
                {
                    throw new InvalidDataException("Duplicate bridged TypeMap key: " + key);
                }
                keys.Add(key);
                var attribute = new CustomAttribute(ClosedConstructor(module, entryAttribute, group.GroupType, 3));
                attribute.ConstructorArguments.Add(new(module.TypeSystem.String, key));
                attribute.ConstructorArguments.Add(new(module.ImportReference(systemType), fragment.ProxyType));
                attribute.ConstructorArguments.Add(new(module.ImportReference(systemType), module.ImportReference(condition.TriggerType)));
                module.Assembly.CustomAttributes.Add(attribute);
            }
        }
        AddAssemblyTarget(group);

        var runtime = _core.MainModule.GetType("AtomUI.Registration.ControlRegistrationRuntime") ??
            throw new InvalidDataException("Core lacks registration runtime.");
        var collect = runtime.Methods.Single(method => method.Name == "CollectFragments" && method.IsStatic && method.Parameters.Count == 3);
        ResetBody(group.Collector);
        var il = group.Collector.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, accessor);
        il.Emit(OpCodes.Ldc_I4, keys.Count);
        il.Emit(OpCodes.Newarr, module.TypeSystem.String);
        for (var index = 0; index < keys.Count; index++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldstr, keys[index]);
            il.Emit(OpCodes.Stelem_Ref);
        }
        il.Emit(OpCodes.Call, module.ImportReference(collect));
        il.Emit(OpCodes.Ret);
    }

    private void AddAssemblyTarget(BoundConditionalRegistrationGroup group)
    {
        var module = _application.MainModule;
        var assemblyName = group.Collector.Module.Assembly.Name.FullName;
        foreach (var attribute in _application.CustomAttributes)
        {
            if (attribute.AttributeType is GenericInstanceType generic && generic.ElementType.FullName == "System.Runtime.InteropServices.TypeMapAssemblyTargetAttribute`1" &&
                generic.GenericArguments.Count == 1 && generic.GenericArguments[0].Resolve() == group.GroupType)
            {
                if (attribute.ConstructorArguments.Count != 1 || !Equals(attribute.ConstructorArguments[0].Value, assemblyName))
                {
                    throw new InvalidDataException("Conflicting TypeMap assembly target for bridged Group.");
                }
                return;
            }
        }
        var target = new CustomAttribute(ClosedConstructor(module, PlatformType("System.Runtime.InteropServices.TypeMapAssemblyTargetAttribute`1"), group.GroupType, 1));
        target.ConstructorArguments.Add(new(module.TypeSystem.String, assemblyName));
        _application.CustomAttributes.Add(target);
    }

    private void PreserveBrowserHelper(MethodDefinition accessor, TypeDefinition helper)
    {
        var module = accessor.Module;
        var members = PlatformType("System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes");
        var dependency = PlatformType("System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute");
        var constructor = dependency.Methods.Single(method => method.IsConstructor && method.Parameters.Count == 2 &&
            method.Parameters[0].ParameterType.FullName == members.FullName && method.Parameters[1].ParameterType.FullName == "System.Type");
        var attribute = new CustomAttribute(module.ImportReference(constructor));
        attribute.ConstructorArguments.Add(new(module.ImportReference(members), Convert.ToInt32(members.Fields.Single(field => field.Name == "PublicMethods").Constant)));
        attribute.ConstructorArguments.Add(new(module.ImportReference(PlatformType("System.Type")), helper));
        accessor.CustomAttributes.Add(attribute);
    }

    private TypeDefinition PlatformType(string name) => _coreLib.MainModule.GetType(name) ??
        throw new InvalidDataException("Target CoreLib lacks official TypeMap dependency: " + name);

    private static TypeDefinition RequireBrowserHelper(TypeDefinition group)
    {
        var helper = group.NestedTypes.SingleOrDefault(type => type.Name == "BrowserMap");
        if (helper is null || helper.HasGenericParameters || new[] { "Create", "Add", "Complete" }.Any(name =>
                helper.Methods.Count(method => method.Name == name && method.IsPublic && method.IsStatic && method.HasBody) != 1))
        {
            throw new InvalidDataException("Conditional package lacks its existing BrowserMap ABI helper.");
        }
        return helper;
    }

    private static MethodReference ClosedConstructor(ModuleDefinition module, TypeDefinition attribute, TypeReference group, int parameterCount)
    {
        var constructor = attribute.Methods.Single(method => method.IsConstructor && method.Parameters.Count == parameterCount);
        var closedType = new GenericInstanceType(module.ImportReference(attribute));
        closedType.GenericArguments.Add(module.ImportReference(group));
        var method = new MethodReference(".ctor", module.ImportReference(constructor.ReturnType), closedType)
        {
            HasThis = true,
            CallingConvention = constructor.CallingConvention
        };
        foreach (var parameter in constructor.Parameters)
        {
            method.Parameters.Add(new ParameterDefinition(module.ImportReference(parameter.ParameterType)));
        }
        return method;
    }

    private static void ResetBody(MethodDefinition method)
    {
        method.DebugInformation.SequencePoints.Clear();
        method.DebugInformation.Scope = null;
        method.DebugInformation.StateMachineKickOffMethod = null;
        method.DebugInformation.CustomDebugInformations.Clear();
        method.CustomDebugInformations.Clear();
        method.Body = new MethodBody(method) { MaxStackSize = 6 };
    }
}
