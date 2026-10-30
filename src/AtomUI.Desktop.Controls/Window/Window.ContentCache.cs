using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class Window
{
    private Panel? _contentCacheHost;
    private WindowContentBackdrop? _contentBackdrop;
    private int _contentCacheLeases;

    /// <summary>
    /// Caches the window content as a bitmap while <paramref name="overlay"/> animates above it, so each overlay
    /// frame composites the cached content instead of re-rendering it. Returns null when the cache cannot reproduce
    /// direct rendering exactly, or when the overlay itself lies inside the cached content (every overlay frame would
    /// then invalidate the cache); callers keep the uncached path.
    /// </summary>
    /// <remarks>
    /// The cached root must sit on whole device pixels with its content clipped to it, and every text pixel must
    /// lie on an opaque base inside the cache. The backdrop supplies that base from the live frame brush, which
    /// must therefore be an opaque solid color with no frame layer content above it.
    /// </remarks>
    internal IDisposable? AcquireContentCache(Visual overlay)
    {
        if (_contentCacheHost is { } host && (ReferenceEquals(host, overlay) || host.IsVisualAncestorOf(overlay)))
        {
            return null;
        }
        if (_contentCacheLeases == 0 && !TryEnableContentCache())
        {
            return null;
        }
        _contentCacheLeases++;
        return Disposable.Create(ReleaseContentCache);
    }

    private void AttachContentCacheParts(INameScope nameScope)
    {
        DisableContentCache();
        _contentCacheLeases = 0;
        _contentCacheHost = nameScope.Find<Panel>("PART_ContentCacheHost");
        _contentBackdrop = nameScope.Find<WindowContentBackdrop>("PART_ContentBackdrop");
    }

    private void ReleaseContentCache()
    {
        if (_contentCacheLeases == 0)
        {
            return;
        }
        if (--_contentCacheLeases == 0)
        {
            DisableContentCache();
        }
    }

    private bool TryEnableContentCache()
    {
        if (_contentCacheHost is not { } host || _contentBackdrop is not { } backdrop || WindowFrameLayer is not null ||
            FindContentCacheFrame() is not { } frame || frame.Background is not ISolidColorBrush { Color.A: 255, Opacity: 1 } ||
            !IsOnDevicePixels(host))
        {
            return false;
        }
        backdrop.Activate(frame);
        host.ClipToBounds = true;
        // ClearType keeps subpixel text in the layer. The host is already on whole device pixels; the framework's
        // snapping floors the world origin without tolerance, so 61.99999999999999 snaps a whole pixel away.
        // The scale guard keeps the truncated layer size from losing a pixel to the same rounding (2042.9999999999998
        // becomes 2042); it is small enough to vanish in Skia's float transform.
        host.CacheMode = new BitmapCache { EnableClearType = true, SnapsToDevicePixels = false, RenderAtScale = 1 + 1e-12 };
        // The composition visual only observes CacheMode during a property sync.
        host.InvalidateVisual();
        return true;
    }

    private void DisableContentCache()
    {
        if (_contentCacheHost is { } host && host.CacheMode is not null)
        {
            host.CacheMode = null;
            host.ClipToBounds = false;
            host.InvalidateVisual();
        }
        _contentBackdrop?.Deactivate();
    }

    // Drawn decorations paint the frame background in a layer beside the window, under the top-level host.
    private Border? FindContentCacheFrame() =>
        (this.GetVisualAncestors().LastOrDefault() ?? this).GetVisualDescendants().OfType<Border>()
            .FirstOrDefault(border => border.Name == "PART_WindowFrame");

    private bool IsOnDevicePixels(Visual host)
    {
        if (host.TranslatePoint(default, this) is not { } origin)
        {
            return false;
        }
        var scaling = RenderScaling;
        return IsWhole(origin.X * scaling) && IsWhole(origin.Y * scaling) &&
               IsWhole(host.Bounds.Width * scaling) && IsWhole(host.Bounds.Height * scaling);

        static bool IsWhole(double value) => Math.Abs(value - Math.Round(value)) < 0.001;
    }
}
