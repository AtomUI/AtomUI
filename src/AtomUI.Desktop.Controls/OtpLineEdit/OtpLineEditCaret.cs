using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal sealed class OtpLineEditCaret : Rectangle
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan> BlinkDurationProperty =
        AvaloniaProperty.Register<OtpLineEditCaret, TimeSpan>(nameof(BlinkDuration));

    public static readonly StyledProperty<Easing> BlinkEasingProperty =
        AvaloniaProperty.Register<OtpLineEditCaret, Easing>(nameof(BlinkEasing), new LinearEasing());

    public static readonly StyledProperty<Animation?> BlinkBlueprintProperty =
        AvaloniaProperty.Register<OtpLineEditCaret, Animation?>(nameof(BlinkBlueprint));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<OtpLineEditCaret, bool>(nameof(IsActive));

    public TimeSpan BlinkDuration
    {
        get => GetValue(BlinkDurationProperty);
        set => SetValue(BlinkDurationProperty, value);
    }

    public Easing BlinkEasing
    {
        get => GetValue(BlinkEasingProperty);
        set => SetValue(BlinkEasingProperty, value);
    }

    public Animation? BlinkBlueprint
    {
        get => GetValue(BlinkBlueprintProperty);
        set => SetValue(BlinkBlueprintProperty, value);
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    #endregion

    private Style? _blinkStyle;
    private Animation? _appliedBlueprint;
    private TimeSpan _appliedDuration;
    private Easing? _appliedEasing;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateBlink();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopBlink();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BlinkDurationProperty || change.Property == BlinkEasingProperty ||
            change.Property == BlinkBlueprintProperty || change.Property == IsActiveProperty)
        {
            UpdateBlink();
        }
    }

    private void UpdateBlink()
    {
        var blueprint = BlinkBlueprint;
        var duration = BlinkDuration;
        var easing = BlinkEasing;
        if (VisualRoot is null || !IsActive || duration <= TimeSpan.Zero || blueprint is null)
        {
            StopBlink();
            return;
        }

        if (_blinkStyle is not null && ReferenceEquals(blueprint, _appliedBlueprint) &&
            duration == _appliedDuration && HasSameEasing(easing, _appliedEasing))
        {
            return;
        }

        // ControlTheme owns the inputs and keyframes. Its shared Animation cannot bind to an
        // individual caret, and RunAsync rejects Infinite. A leaf-owned native Style supplies
        // per-instance timing without another clock; native visibility pauses retain the phase.
        var animation = CreateBlink(blueprint, duration, easing);
        StopBlink();
        var style = new Style { Animations = { animation } };
        Styles.Add(style);
        _blinkStyle = style;
        _appliedBlueprint = blueprint;
        _appliedDuration = duration;
        _appliedEasing = animation.Easing;
    }

    private void StopBlink()
    {
        if (_blinkStyle is not null)
        {
            Styles.Remove(_blinkStyle);
            _blinkStyle = null;
        }
        _appliedBlueprint = null;
        _appliedEasing = null;
    }

    private static Animation CreateBlink(Animation blueprint, TimeSpan duration, Easing easing)
    {
        if (!TokenValueOperations.TryCopy(easing, out var copy) || copy is not Easing ownedEasing)
        {
            throw new NotSupportedException($"No AOT easing copy is defined for '{easing.GetType()}'.");
        }

        var animation = new Animation
        {
            Duration = duration,
            Easing = ownedEasing,
            IterationCount = blueprint.IterationCount,
            PlaybackBehavior = PlaybackBehavior.OnlyIfVisible
        };
        // This private blueprint contains only Cue-based, literal Opacity keyframes. It is
        // deliberately not a generic animation cloner (KeyFrame timing mode is internal).
        foreach (var source in blueprint.Children)
        {
            var keyframe = new KeyFrame { Cue = source.Cue };
            foreach (var setter in source.Setters)
            {
                keyframe.Setters.Add(new Setter(setter.Property!, setter.Value));
            }
            animation.Children.Add(keyframe);
        }
        return animation;
    }

    private static bool HasSameEasing(Easing value, Easing? applied)
    {
        if (ReferenceEquals(value, applied))
        {
            return true;
        }
        if (applied is null || value.GetType() != applied.GetType())
        {
            return false;
        }
        if (value is SplineEasing spline && applied is SplineEasing otherSpline)
        {
            return spline.X1 == otherSpline.X1 && spline.Y1 == otherSpline.Y1 &&
                   spline.X2 == otherSpline.X2 && spline.Y2 == otherSpline.Y2;
        }
        if (value is SpringEasing spring && applied is SpringEasing otherSpring)
        {
            return spring.Mass == otherSpring.Mass && spring.Stiffness == otherSpring.Stiffness &&
                   spring.Damping == otherSpring.Damping && spring.InitialVelocity == otherSpring.InitialVelocity;
        }
        return TokenValueOperations.TryCopy(value, out var copy) && ReferenceEquals(value, copy);
    }
}
