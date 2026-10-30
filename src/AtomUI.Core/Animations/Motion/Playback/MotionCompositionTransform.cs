using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;

namespace AtomUI.Animations;

/// <summary>
/// Writes motion translate/scale/rotation into the target's composition visual instead of RenderTransform.
/// </summary>
/// <remarks>
/// Visual.SynchronizeCompositionProperties owns Offset, Size, Opacity and TransformMatrix but never writes
/// Translation, Scale, RotationAngle or CenterPoint. Writing those channels avoids InvalidateVisual and draw
/// list re-recording per frame, composes after the application's RenderTransform, and hit testing follows the
/// rendered result through the compositor readback.
/// Each channel has one owner per visual. A superseding presentation takes over the channels it animates
/// without a reset in between; a released owner restores only the channels it still owns to the baseline
/// captured before the first motion, so other composition users of the same visual keep their own values.
/// </remarks>
internal sealed class MotionCompositionTransform : IDisposable
{
    private static readonly ConditionalWeakTable<CompositionVisual, ChannelOwners> Owners = new();

    private readonly Control _target;
    private readonly RelativePoint _origin;
    private readonly bool _translates;
    private readonly bool _scales;
    private readonly bool _rotates;
    private CompositionVisual? _visual;
    private ChannelOwners? _owners;

    internal MotionCompositionTransform(Control target, MotionPlan plan)
    {
        _target = target;
        _origin = plan.Origin;
        foreach (var track in plan.Tracks)
        {
            switch (track.Kind)
            {
                case MotionKind.TranslateX or MotionKind.TranslateY:
                    _translates = true;
                    break;
                case MotionKind.Scale or MotionKind.ScaleX or MotionKind.ScaleY:
                    _scales = true;
                    break;
                case MotionKind.Rotation:
                    _rotates = true;
                    break;
            }
        }
    }

    private bool Centers => _scales || _rotates;

    internal void Attach(in MotionValues values)
    {
        if (_visual is not null)
        {
            return;
        }
        var visual = ElementComposition.GetElementVisual(_target);
        if (visual is null)
        {
            return;
        }
        _visual = visual;
        _owners = Owners.GetValue(visual, static value => new ChannelOwners(value));
        if (_translates)
        {
            _owners.Translation = this;
        }
        if (_scales)
        {
            _owners.Scale = this;
        }
        if (_rotates)
        {
            _owners.Rotation = this;
        }
        if (Centers)
        {
            _owners.Center = this;
        }
        Apply(values);
    }

    internal void Apply(in MotionValues values)
    {
        if (_visual is not { } visual || _owners is not { } owners)
        {
            return;
        }
        if (ReferenceEquals(owners.Center, this))
        {
            var center = _origin.ToPixels(_target.Bounds.Size);
            visual.CenterPoint = new Vector3D(center.X, center.Y, 0);
        }
        if (ReferenceEquals(owners.Translation, this))
        {
            visual.Translation = new Vector3D(values.X, values.Y, 0);
        }
        if (ReferenceEquals(owners.Scale, this))
        {
            visual.Scale = new Vector3D(values.ScaleX, values.ScaleY, 1);
        }
        if (ReferenceEquals(owners.Rotation, this))
        {
            visual.RotationAngle = (float)(values.Rotation * Math.PI / 180);
        }
    }

    public void Dispose()
    {
        var visual = _visual;
        var owners = _owners;
        _visual = null;
        _owners = null;
        if (visual is null || owners is null)
        {
            return;
        }
        if (ReferenceEquals(owners.Translation, this))
        {
            owners.Translation = null;
            visual.Translation = owners.BaseTranslation;
        }
        if (ReferenceEquals(owners.Scale, this))
        {
            owners.Scale = null;
            visual.Scale = owners.BaseScale;
        }
        if (ReferenceEquals(owners.Rotation, this))
        {
            owners.Rotation = null;
            visual.RotationAngle = owners.BaseRotation;
        }
        if (ReferenceEquals(owners.Center, this))
        {
            owners.Center = null;
            visual.CenterPoint = owners.BaseCenter;
        }
        if (owners.IsEmpty)
        {
            Owners.Remove(visual);
        }
    }

    private sealed class ChannelOwners(CompositionVisual visual)
    {
        internal readonly Vector3D BaseTranslation = visual.Translation;
        internal readonly Vector3D BaseScale = visual.Scale;
        internal readonly Vector3D BaseCenter = visual.CenterPoint;
        internal readonly float BaseRotation = visual.RotationAngle;
        internal MotionCompositionTransform? Translation;
        internal MotionCompositionTransform? Scale;
        internal MotionCompositionTransform? Rotation;
        internal MotionCompositionTransform? Center;

        internal bool IsEmpty => Translation is null && Scale is null && Rotation is null && Center is null;
    }
}
