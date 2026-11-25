using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native;

internal enum WaylandInitialScalingStatus
{
    NotApplicable,
    Skipped,
    Applied,
    Incompatible,
    Failed
}

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
/// The private integration is checked by its actual member contract, independently of package build metadata.
/// </remarks>
internal static class WaylandInitialScaling
{
    private const string WindowBaseImplTypeName = "Avalonia.Wayland.WindowBaseImpl";
    private const string WorkerClientTypeName = "Avalonia.Wayland.Server.WaylandWorkerClient";
    private const string WorkerTypeName = "Avalonia.Wayland.Server.WaylandWorker";
    private const string GlobalsTypeName = "Avalonia.Wayland.Server.Transient.WaylandGlobals";
    private const string TrackerTypeName = "Avalonia.Wayland.Server.Transient.WaylandOutputsTracker";
    private const string OutputTypeName = TrackerTypeName + "+Output";
    // DynamicDependency uses documentation-ID type names, where nested types are separated by '.'.
    private const string OutputDependencyTypeName = TrackerTypeName + ".Output";
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    // Protocol scales are multiples of 1/120.
    private const double FractionalDenominator = 120;

    private static readonly Lazy<Members> Bound = new(Bind);

    internal static WaylandInitialScalingStatus Apply(ITopLevelImpl? impl, out string reason)
    {
        reason = "The window does not use the Avalonia Wayland backend.";
        if (impl is null || impl.GetType().Assembly.GetName().Name != "Avalonia.Wayland")
        {
            return WaylandInitialScalingStatus.NotApplicable;
        }
        // This constructor-time optimisation must never overwrite a scale already supplied by the backend.
        if (!double.IsFinite(impl.RenderScaling) || impl.RenderScaling <= 0)
        {
            reason = $"The backend supplies invalid render scaling {impl.RenderScaling}; no prediction was applied.";
            return WaylandInitialScalingStatus.Failed;
        }
        if (Math.Abs(impl.RenderScaling - 1) > 1e-6)
        {
            reason = $"The backend already supplies render scaling {impl.RenderScaling}; prediction was skipped.";
            return WaylandInitialScalingStatus.Skipped;
        }
        Members members;
        try
        {
            members = Bound.Value;
        }
        catch (Exception error) when (error is TypeLoadException or MissingMemberException or NotSupportedException or MemberAccessException or AmbiguousMatchException)
        {
            reason = DescribeFailure(impl, error);
            return WaylandInitialScalingStatus.Incompatible;
        }
        if (!members.WindowBaseImpl.IsInstanceOfType(impl))
        {
            reason = $"The Wayland window implementation {impl.GetType().FullName} does not implement {WindowBaseImplTypeName}.";
            return WaylandInitialScalingStatus.Incompatible;
        }
        try
        {
            if (PredictScale(members, impl, out reason) is not { } scale || Math.Abs(scale - 1) <= 1e-6)
            {
                return WaylandInitialScalingStatus.Skipped;
            }
            members.UpdateScaling.Invoke(impl, [scale]);
            if (Math.Abs(impl.RenderScaling - scale) > 1e-6)
            {
                reason = $"UpdateScaling did not apply predicted scale {scale}; actual={impl.RenderScaling}.";
                return WaylandInitialScalingStatus.Failed;
            }
            reason = $"Predicted render scaling {scale} before the first layout.";
            return WaylandInitialScalingStatus.Applied;
        }
        catch (Exception error) when (error is TargetInvocationException or InvalidCastException or NullReferenceException or InvalidOperationException or MemberAccessException or ArgumentException)
        {
            reason = DescribeFailure(impl, error is TargetInvocationException { InnerException: { } inner } ? inner : error);
            return WaylandInitialScalingStatus.Failed;
        }
    }

    private static string DescribeFailure(ITopLevelImpl impl, Exception error) =>
        $"{error.Message} Backend={impl.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}; NWayland={typeof(WlOutput).Assembly.GetName().Version}.";

    private static double? PredictScale(Members members, object impl, out string reason)
    {
        reason = "Wayland output information is not ready for scale prediction.";
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
                reason = "Wayland output mode/logical size does not provide a valid scale prediction.";
                return null;
            }
            if (predicted is { } previous && Math.Abs(previous - scale) > 1e-6)
            {
                reason = "Wayland outputs use different scales; initial scaling prediction was skipped.";
                return null;
            }
            predicted = scale;
        }
        reason = $"The output scale {predicted} matches the initial backend scale.";
        return predicted;
    }

    private static double? OutputScale(Members members, object output, bool fractional)
    {
        if (!members.OutputType.IsInstanceOfType(output))
        {
            throw new InvalidOperationException($"Wayland Outputs contains {output.GetType().FullName}; expected {members.OutputType.FullName}.");
        }
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
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, OutputDependencyTypeName, "Avalonia.Wayland")]
    private static Members Bind()
    {
        var impl = Type.GetType(WindowBaseImplTypeName + ", Avalonia.Wayland", true)!;
        var client = Type.GetType(WorkerClientTypeName + ", Avalonia.Wayland", true)!;
        var worker = Type.GetType(WorkerTypeName + ", Avalonia.Wayland", true)!;
        var globals = Type.GetType(GlobalsTypeName + ", Avalonia.Wayland", true)!;
        var tracker = Type.GetType(TrackerTypeName + ", Avalonia.Wayland", true)!;
        var output = Type.GetType(OutputTypeName + ", Avalonia.Wayland", true)!;
        var members = new Members(
            impl,
            output,
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
        if (!typeof(ITopLevelImpl).IsAssignableFrom(impl))
        {
            throw new NotSupportedException($"{WindowBaseImplTypeName} must implement {typeof(ITopLevelImpl).FullName}.");
        }
        WaylandMemberContract.ReadProperty(members.Client, impl, "Client", client);
        WaylandMemberContract.InstanceMethod(members.UpdateScaling, impl, "UpdateScaling", typeof(void), typeof(double));
        WaylandMemberContract.ReadField(members.Worker, client, "_worker", worker);
        WaylandMemberContract.ReadProperty(members.Globals, worker, "Globals", globals);
        WaylandMemberContract.ReadProperty(members.Outputs, globals, "Outputs", tracker);
        WaylandMemberContract.ReadProperty(members.HasFractionalScaling, globals, "HasFractionalScaling", typeof(bool));
        if (members.OutputList.GetMethod is not { IsStatic: false } || members.OutputList.GetIndexParameters().Length != 0 ||
            !members.OutputList.PropertyType.IsGenericType ||
            members.OutputList.PropertyType.GetGenericTypeDefinition() != typeof(List<>) ||
            members.OutputList.PropertyType.GenericTypeArguments[0] != output)
        {
            throw new NotSupportedException($"{TrackerTypeName}.Outputs must expose a readable instance List<{OutputTypeName}>; found {members.OutputList.PropertyType}.");
        }
        WaylandMemberContract.ReadProperty(members.ModeSize, output, "ModeSize", typeof(PixelSize));
        WaylandMemberContract.ReadProperty(members.LogicalSize, output, "LogicalSize", typeof(PixelSize));
        WaylandMemberContract.ReadProperty(members.IntegerScale, output, "Scale", typeof(int));
        WaylandMemberContract.ReadProperty(members.Transform, output, "Transform", typeof(WlOutput.TransformEnum));
        return members;
    }

    private sealed record Members(
        Type WindowBaseImpl,
        Type OutputType,
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
