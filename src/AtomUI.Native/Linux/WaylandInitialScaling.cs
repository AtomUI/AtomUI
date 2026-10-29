using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using NWayland;
using NWayland.Interop;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native;

/// <summary>
/// Predicts a Wayland toplevel's scale before its first layout.
/// </summary>
/// <remarks>
/// Avalonia starts every Wayland toplevel at scale 1 and adopts the compositor's preferred scale only after the
/// surface is mapped and has entered an output (mutter sends <c>wp_fractional_scale.preferred_scale</c> then).
/// The first layout and frames therefore run at scale 1 and the whole window re-lays out once the real scale
/// arrives. Buffers are sized from the UI scale passed with each frame, so adopting the outputs' scale up front
/// renders the first frame correctly; the later compositor notification then matches and changes nothing. When
/// outputs disagree the prediction is skipped, and a wrong prediction is corrected by the compositor as before.
/// Deliberately version-bound: these members are framework integration details.
/// </remarks>
internal static class WaylandInitialScaling
{
    private const string WindowBaseImplTypeName = "Avalonia.Wayland.WindowBaseImpl";
    private const string WorkerClientTypeName = "Avalonia.Wayland.Server.WaylandWorkerClient";
    private const string WorkerTypeName = "Avalonia.Wayland.Server.WaylandWorker";
    private const string GlobalsTypeName = "Avalonia.Wayland.Server.Transient.WaylandGlobals";
    private const string TrackerTypeName = "Avalonia.Wayland.Server.Transient.WaylandOutputsTracker";
    private const string OutputTypeName = TrackerTypeName + "+Output";
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    // Protocol scales are multiples of 1/120.
    private const double FractionalDenominator = 120;

    private static readonly Lazy<Members?> Bound = new(Bind);

    internal static void Apply(ITopLevelImpl? impl)
    {
        if (impl is null || Bound.Value is not { } members || !members.WindowBaseImpl.IsInstanceOfType(impl))
        {
            return;
        }
        try
        {
            if (PredictScale(members, impl) is { } scale && Math.Abs(scale - impl.RenderScaling) > 1e-6)
            {
                members.UpdateScaling.Invoke(impl, [scale]);
            }
        }
        catch (Exception error) when (error is TargetInvocationException or InvalidCastException or NullReferenceException)
        {
            // Prediction is an optimisation; the compositor's scale still arrives through the framework.
        }
    }

    private static double? PredictScale(Members members, object impl)
    {
        var client = members.Client.GetValue(impl);
        var worker = client is null ? null : members.Worker.GetValue(client);
        var globals = worker is null ? null : members.Globals.GetValue(worker);
        var tracker = globals is null ? null : members.Outputs.GetValue(globals);
        if (tracker is null || members.OutputList.GetValue(tracker) is not IList outputs || outputs.Count == 0)
        {
            return null;
        }
        var fractional = (bool)members.HasFractionalScaling.GetValue(globals)!;
        double? predicted = null;
        foreach (var output in outputs)
        {
            if (output is null || OutputScale(members, output, fractional) is not { } scale)
            {
                return null;
            }
            if (predicted is { } previous && Math.Abs(previous - scale) > 1e-6)
            {
                return null;
            }
            predicted = scale;
        }
        return predicted;
    }

    private static double? OutputScale(Members members, object output, bool fractional)
    {
        if (!fractional)
        {
            var integer = (int)members.IntegerScale.GetValue(output)!;
            return integer >= 1 ? integer : null;
        }
        var mode = (PixelSize)members.ModeSize.GetValue(output)!;
        var logical = (PixelSize)members.LogicalSize.GetValue(output)!;
        if (members.Transform.GetValue(output) is WlOutput.TransformEnum transform &&
            transform is WlOutput.TransformEnum.k_90 or WlOutput.TransformEnum.k_270 or
                WlOutput.TransformEnum.Flipped90 or WlOutput.TransformEnum.Flipped270)
        {
            mode = new PixelSize(mode.Height, mode.Width);
        }
        if (mode.Width <= 0 || mode.Height <= 0 || logical.Width <= 0 || logical.Height <= 0)
        {
            return null;
        }
        var horizontal = (double)mode.Width / logical.Width;
        var vertical = (double)mode.Height / logical.Height;
        if (Math.Abs(horizontal - vertical) > 0.01 || horizontal < 1 || horizontal > 8)
        {
            return null;
        }
        return Math.Round(horizontal * FractionalDenominator) / FractionalDenominator;
    }

    [DynamicDependency("get_Client", WindowBaseImplTypeName, "Avalonia.Wayland")]
    [DynamicDependency("UpdateScaling", WindowBaseImplTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_worker", WorkerClientTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_Globals", WorkerTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_Outputs", GlobalsTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_HasFractionalScaling", GlobalsTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_Outputs", TrackerTypeName, "Avalonia.Wayland")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, OutputTypeName, "Avalonia.Wayland")]
    private static Members? Bind()
    {
        var version = Type.GetType("Avalonia.WaylandPlatformOptions, Avalonia.Wayland")?.Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!OperatingSystem.IsLinux() || version != "12.1.4+2d79ba55fae1b1e71c69f93a668080c9029e6a31" ||
            typeof(WlProxy).Assembly.GetName().Version != new Version(0, 11, 0, 0))
        {
            return null;
        }
        try
        {
            var impl = Type.GetType(WindowBaseImplTypeName + ", Avalonia.Wayland", true)!;
            var client = Type.GetType(WorkerClientTypeName + ", Avalonia.Wayland", true)!;
            var worker = Type.GetType(WorkerTypeName + ", Avalonia.Wayland", true)!;
            var globals = Type.GetType(GlobalsTypeName + ", Avalonia.Wayland", true)!;
            var tracker = Type.GetType(TrackerTypeName + ", Avalonia.Wayland", true)!;
            var output = Type.GetType(OutputTypeName + ", Avalonia.Wayland", true)!;
            var members = new Members(
                impl,
                impl.GetProperty("Client", Instance) ?? throw new MissingMemberException(WindowBaseImplTypeName, "Client"),
                impl.GetMethod("UpdateScaling", Instance, [typeof(double)]) ?? throw new MissingMethodException(WindowBaseImplTypeName, "UpdateScaling"),
                client.GetField("_worker", Instance) ?? throw new MissingFieldException(WorkerClientTypeName, "_worker"),
                worker.GetProperty("Globals", Instance) ?? throw new MissingMemberException(WorkerTypeName, "Globals"),
                globals.GetProperty("Outputs", Instance) ?? throw new MissingMemberException(GlobalsTypeName, "Outputs"),
                globals.GetProperty("HasFractionalScaling", Instance) ?? throw new MissingMemberException(GlobalsTypeName, "HasFractionalScaling"),
                tracker.GetProperty("Outputs", Instance) ?? throw new MissingMemberException(TrackerTypeName, "Outputs"),
                output.GetProperty("ModeSize", Instance) ?? throw new MissingMemberException(OutputTypeName, "ModeSize"),
                output.GetProperty("LogicalSize", Instance) ?? throw new MissingMemberException(OutputTypeName, "LogicalSize"),
                output.GetProperty("Scale", Instance) ?? throw new MissingMemberException(OutputTypeName, "Scale"),
                output.GetProperty("Transform", Instance) ?? throw new MissingMemberException(OutputTypeName, "Transform"));
            return members.ModeSize.PropertyType == typeof(PixelSize) && members.LogicalSize.PropertyType == typeof(PixelSize) &&
                   members.IntegerScale.PropertyType == typeof(int) && members.HasFractionalScaling.PropertyType == typeof(bool) &&
                   members.Transform.PropertyType == typeof(WlOutput.TransformEnum)
                ? members : null;
        }
        catch (Exception error) when (error is TypeLoadException or MissingMemberException)
        {
            return null;
        }
    }

    private sealed record Members(
        Type WindowBaseImpl,
        PropertyInfo Client,
        MethodInfo UpdateScaling,
        FieldInfo Worker,
        PropertyInfo Globals,
        PropertyInfo Outputs,
        PropertyInfo HasFractionalScaling,
        PropertyInfo OutputList,
        PropertyInfo ModeSize,
        PropertyInfo LogicalSize,
        PropertyInfo IntegerScale,
        PropertyInfo Transform);
}
