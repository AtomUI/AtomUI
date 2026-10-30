using System;
using System.Collections.Generic;
using System.Linq;
using ILCompiler.DependencyAnalysis;
using ILCompiler.DependencyAnalysisFramework;
using Internal.Text;
using Internal.TypeSystem;

namespace ILCompiler;

internal sealed class RegistrationTableNode : ObjectNode, ISymbolDefinitionNode
{
    internal const int HeaderSize = 16;
    internal const int FormatVersion = 1;
    private readonly RegistrationDefinitions.Group _group;

    public RegistrationTableNode(RegistrationDefinitions.Group group)
    {
        _group = group;
    }

    public override bool StaticDependenciesAreComputed => true;
    public override bool HasConditionalStaticDependencies => true;
    public override bool IsShareable => false;
    public int Offset => 0;
    public override int ClassCode => -1941056105;
    // Absolute function pointers need loader rebasing. On Unix they must not be
    // placed in executable/read-only text pages (Mach-O __TEXT fixups fault).
    public override ObjectNodeSection GetSection(NodeFactory factory) => factory.Target.IsWindows
        ? ObjectNodeSection.ReadOnlyDataSection
        : ObjectNodeSection.DataSection;
    protected override string GetName(NodeFactory factory) => _group.SymbolName;

    public void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder builder)
    {
        builder.Append(_group.SymbolName);
    }

    public override int CompareToImpl(ISortableNode other, CompilerComparer comparer)
    {
        return string.Compare(_group.SymbolName, ((RegistrationTableNode)other)._group.SymbolName, StringComparison.Ordinal);
    }

    public override IEnumerable<CombinedDependencyListEntry> GetConditionalStaticDependencies(NodeFactory factory)
    {
        foreach (RegistrationDefinitions.Entry entry in _group.Entries)
        {
            // For the admitted non-generic static managed signature, ILC8 ldftn uses
            // this same MethodEntrypoint; no fat pointer/unboxing/context thunk applies.
            yield return new CombinedDependencyListEntry(factory.MethodEntrypoint(entry.Method),
                factory.NecessaryTypeSymbol(entry.Condition), "AtomUI condition " + entry.Key);
        }
    }

    public RegistrationDefinitions.Entry[] Selected(NodeFactory factory)
    {
        return _group.Entries.Where(e => factory.NecessaryTypeSymbol(e.Condition).Marked).ToArray();
    }

    public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
    {
        // Conditional edges above own all dependencies. No candidate relocation is
        // exposed during graph marking; the final table introduces no new roots.
        if (relocsOnly)
        {
            return new ObjectData(Array.Empty<byte>(), Array.Empty<Relocation>(), factory.Target.PointerSize, new ISymbolDefinitionNode[] { this });
        }
        MethodDesc[] methods = Selected(factory).Select(e => e.Method).Distinct().ToArray();
        var data = new ObjectDataBuilder(factory, relocsOnly);
        data.RequireInitialPointerAlignment();
        data.AddSymbol(this);
        data.EmitInt(FormatVersion);
        data.EmitInt(factory.Target.PointerSize);
        data.EmitInt(methods.Length);
        data.EmitInt(0);
        foreach (MethodDesc method in methods)
        {
            IMethodNode target = factory.MethodEntrypoint(method);
            if (!target.Marked)
            {
                throw new InvalidOperationException("Registration table references an unanalyzed entrypoint: " + method);
            }
            data.EmitPointerReloc(target);
        }
        return data.ToObjectData();
    }
}
