using System.Diagnostics;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Shouldly;
using SkiaSharp;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect;

/// <summary>
/// Perturb-and-compare helper for Token effect tests (docs/engineering/development/theme-effect-tests.md):
/// overrides one Token through <c>ThemeConfig.Controls[identity].Tokens[token]</c> on a local
/// <see cref="ThemeConfigProvider"/> and asserts the pixels inside the Control's bounds change.
/// </summary>
public static class TokenEffectHarness
{
    public const double DefaultThresholdRatio = 0.005;
    public const double ThinLineThresholdRatio = 0.001;
    public const int DefaultChannelTolerance = 8;

    private const double WindowWidth = 800;
    private const double WindowHeight = 600;
    private static readonly TimeSpan s_configApplyTimeout = TimeSpan.FromSeconds(10);

    private static readonly HashSet<string> s_thinLineTokens = new(StringComparer.Ordinal)
    {
        "LineWidth",
        "LineWidthBold",
        "ControlOutlineWidth",
        "FocusVisualBorderThickness"
    };

    /// <summary>
    /// Asserts that overriding <paramref name="token"/> for <paramref name="identity"/> visibly changes the Control.
    /// Pixel comparisons run on a baseline with <c>EnableMotion=false</c> so frames show settled visuals instead of
    /// transition starts. Thin-line Tokens are checked against at most <see cref="ThinLineThresholdRatio"/>.
    /// TimeSpan and bool Tokens are not compared by pixels: the value read back through
    /// <see cref="ControlTokenAccessor"/> must change instead.
    /// </summary>
    /// <param name="create">Creates the Control under test.</param>
    /// <param name="identity">Token identity whose config entry receives the override.</param>
    /// <param name="token">Own Token field name, or global Token name, as written in the config.</param>
    /// <param name="arrange">Puts the attached Control into a state where the Token is visible.</param>
    /// <param name="thresholdRatio">Minimum ratio of changed pixels within the Control's bounds.</param>
    /// <param name="channelTolerance">A pixel changes when any channel differs by more than this.</param>
    public static void AssertHasEffect(
        Func<Control> create,
        ControlTokenIdentity identity,
        string token,
        Action<Control>? arrange = null,
        double thresholdRatio = DefaultThresholdRatio,
        int channelTolerance = DefaultChannelTolerance)
    {
        ArgumentNullException.ThrowIfNull(create);
        ArgumentNullException.ThrowIfNull(token);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(thresholdRatio);
        ArgumentOutOfRangeException.ThrowIfNegative(channelTolerance);

        var target         = ResolveToken(identity, token);
        var comparesPixels = target.ValueType != typeof(TimeSpan) && target.ValueType != typeof(bool);
        var control        = create();
        if (!target.ControlType.IsInstanceOfType(control))
        {
            throw new ArgumentException(
                $"Control '{control.GetType().FullName}' does not match identity '{identity}' " +
                $"('{target.ControlType.FullName}').",
                nameof(create));
        }

        var provider = new ThemeConfigProvider { Config = CreateConfig(comparesPixels, null) };
        provider.Child = new AvaloniaBorder
        {
            Padding             = new Thickness(24),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment   = VerticalAlignment.Top,
            Child               = control
        };
        var window = new AvaloniaWindow
        {
            Width   = WindowWidth,
            Height  = WindowHeight,
            Content = provider
        };

        ThemeChangeFailedEventArgs? failure = null;
        EventHandler<ThemeChangeFailedEventArgs> onFailed = (_, args) => failure = args;
        provider.ThemeChangeFailed += onFailed;
        try
        {
            window.Show();
            FrameCapture.Refresh(window);
            failure.ShouldBeNull(DescribeFailure(failure));
            arrange?.Invoke(control);
            FrameCapture.Refresh(window);

            var current = target.Read(control) ?? throw new NotSupportedException(
                $"Token '{token}' of '{identity}' resolves to null; add an explicit perturbation case.");
            var perturbed = PerturbFor(identity, token, current);
            var formatted = ThemeTokenValueFormatter.Format(perturbed);
            formatted.ShouldNotBe(ThemeTokenValueFormatter.Format(current),
                $"Perturbation of '{token}' must produce a different value.");

            using var before = comparesPixels ? FrameCapture.Capture(window) : null;
            var boundsBefore = before is null ? default : GetPixelBounds(control, window, before);

            provider.Config = CreateConfig(comparesPixels, (identity, token, formatted));
            WaitForValue(control, target, formatted, () => failure is not null);
            failure.ShouldBeNull(DescribeFailure(failure));
            ThemeTokenValueFormatter.Format(target.Read(control)).ShouldBe(formatted,
                $"Override Controls[{identity}].Tokens[{token}] = '{formatted}' did not reach the Control.");

            if (before is null)
            {
                return;
            }

            FrameCapture.Refresh(window);
            using var after   = FrameCapture.Capture(window);
            var boundsAfter   = GetPixelBounds(control, window, after);
            var region        = SKRectI.Union(boundsBefore, boundsAfter);
            var area          = region.Width * region.Height;
            var changed       = FrameCapture.CountDifferentPixels(before, after, region, channelTolerance);
            var ratio         = (double)changed / area;
            var threshold     = ResolveThreshold(token, thresholdRatio);
            ratio.ShouldBeGreaterThan(threshold,
                $"Token '{token}' of '{identity}' changed from '{ThemeTokenValueFormatter.Format(current)}' to " +
                $"'{formatted}' but only {changed}/{area} pixels changed inside the Control bounds; a Token without " +
                "visible effect is not Active (make it Reserved, delete it, or arrange a state where it shows).");
        }
        finally
        {
            provider.ThemeChangeFailed -= onFailed;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>Whether the spec's thin-line threshold applies to the Token.</summary>
    public static bool IsThinLineToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return s_thinLineTokens.Contains(token) ||
               token.Contains("BorderThickness", StringComparison.Ordinal);
    }

    /// <summary>The effective threshold: thin-line Tokens use at most <see cref="ThinLineThresholdRatio"/>.</summary>
    public static double ResolveThreshold(string token, double thresholdRatio)
    {
        return IsThinLineToken(token) ? Math.Min(thresholdRatio, ThinLineThresholdRatio) : thresholdRatio;
    }

    private static ThemeConfig CreateConfig(
        bool settleMotion,
        (ControlTokenIdentity Identity, string Token, string Value)? overrideEntry)
    {
        var builder = new ThemeConfigBuilder();
        if (settleMotion)
        {
            // Motion off zeroes transition durations, so a captured frame shows the final visual state.
            builder.WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        }
        if (overrideEntry is { } entry)
        {
            builder.WithControl(
                entry.Identity,
                new ControlThemeConfigBuilder().WithToken(entry.Token, entry.Value).Build());
        }

        return builder.Build();
    }

    private static object PerturbFor(ControlTokenIdentity identity, string token, object current)
    {
        try
        {
            return TokenPerturbation.Perturb(current);
        }
        catch (NotSupportedException exception)
        {
            throw new NotSupportedException(
                $"Token '{token}' of '{identity}' has value type '{current.GetType().FullName}' without a " +
                "perturbation; the family effect test must add an explicit case (effect tests do not skip Tokens).",
                exception);
        }
    }

    private static TokenTarget ResolveToken(ControlTokenIdentity identity, string token)
    {
        var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager)) as ThemeManager ??
                      throw new InvalidOperationException("The test application has no ThemeManager.");
        var registry = manager.RootContext.Snapshot.Registry;
        if (!registry.TryGetControl(identity, out var descriptor))
        {
            throw new ArgumentException($"Token identity '{identity}' is not registered.", nameof(identity));
        }

        foreach (var own in descriptor.OwnTokens)
        {
            if (own.Name == token)
            {
                var kind = (Enum)own.ResourceKey;
                return new TokenTarget(
                    descriptor.ControlType,
                    own.ValueType,
                    owner => ControlTokenAccessor.Capture(owner).GetOwn<object>(kind));
            }
        }

        if (registry.TryGetGlobalToken(token, out var global))
        {
            var kind = Enum.Parse<SharedTokenKind>(token);
            return new TokenTarget(
                descriptor.ControlType,
                global.ValueType,
                owner => ControlTokenAccessor.Capture(owner).GetEffectiveGlobal<object>(kind));
        }

        throw new ArgumentException(
            $"'{token}' is neither an Own Token of '{identity}' nor a global Token.",
            nameof(token));
    }

    private static void WaitForValue(Control control, TokenTarget target, string expected, Func<bool> failed)
    {
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            Dispatcher.UIThread.RunJobs();
            if (failed() || ThemeTokenValueFormatter.Format(target.Read(control)) == expected ||
                stopwatch.Elapsed > s_configApplyTimeout)
            {
                return;
            }
            Thread.Sleep(1);
        }
    }

    private static SKRectI GetPixelBounds(Control control, AvaloniaWindow window, SKBitmap frame)
    {
        var origin = control.TranslatePoint(new Point(), window) ?? throw new InvalidOperationException(
            "The Control under test is not inside the test window.");
        var size   = control.Bounds.Size;
        var left   = Math.Clamp((int)Math.Floor(origin.X), 0, frame.Width);
        var top    = Math.Clamp((int)Math.Floor(origin.Y), 0, frame.Height);
        var right  = Math.Clamp((int)Math.Ceiling(origin.X + size.Width), 0, frame.Width);
        var bottom = Math.Clamp((int)Math.Ceiling(origin.Y + size.Height), 0, frame.Height);
        if (right <= left || bottom <= top)
        {
            throw new InvalidOperationException(
                "The Control under test has empty visible bounds; arrange it into a visible state.");
        }

        return new SKRectI(left, top, right, bottom);
    }

    private static string DescribeFailure(ThemeChangeFailedEventArgs? failure)
    {
        if (failure is null)
        {
            return string.Empty;
        }

        var diagnostics = string.Join("; ", failure.Diagnostics.Select(static diagnostic => diagnostic.ToString()));
        return $"Theme config was rejected: {diagnostics} {failure.Exception?.Message}";
    }

    private sealed record TokenTarget(Type ControlType, Type ValueType, Func<Control, object?> Read);
}
