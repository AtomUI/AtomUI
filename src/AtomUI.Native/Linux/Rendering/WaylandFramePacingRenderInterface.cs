using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;

namespace AtomUI.Native;

internal sealed class WaylandFramePacingRenderInterface(
    IPlatformRenderInterface inner,
    WaylandFramePacingCompatibility compatibility) : IPlatformRenderInterface
{
    public IGeometryImpl CreateEllipseGeometry(Rect rect) => inner.CreateEllipseGeometry(rect);
    public IGeometryImpl CreateLineGeometry(Point p1, Point p2) => inner.CreateLineGeometry(p1, p2);
    public IGeometryImpl CreateRectangleGeometry(Rect rect) => inner.CreateRectangleGeometry(rect);
    public IStreamGeometryImpl CreateStreamGeometry() => inner.CreateStreamGeometry();
    public IGeometryImpl CreateGeometryGroup(FillRule fillRule, IReadOnlyList<IGeometryImpl> children) => inner.CreateGeometryGroup(fillRule, children);
    public IGeometryImpl CreateCombinedGeometry(GeometryCombineMode combineMode, IGeometryImpl g1, IGeometryImpl g2) => inner.CreateCombinedGeometry(combineMode, g1, g2);
    public IGeometryImpl BuildGlyphRunGeometry(GlyphRun glyphRun) => inner.BuildGlyphRunGeometry(glyphRun);
    public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi) => inner.CreateRenderTargetBitmap(size, dpi);
    public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat format, AlphaFormat alphaFormat) => inner.CreateWriteableBitmap(size, dpi, format, alphaFormat);
    public IBitmapImpl LoadBitmap(string fileName) => inner.LoadBitmap(fileName);
    public IBitmapImpl LoadBitmap(Stream stream) => inner.LoadBitmap(stream);
    public IWriteableBitmapImpl LoadWriteableBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => inner.LoadWriteableBitmapToWidth(stream, width, interpolationMode);
    public IWriteableBitmapImpl LoadWriteableBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => inner.LoadWriteableBitmapToHeight(stream, height, interpolationMode);
    public IWriteableBitmapImpl LoadWriteableBitmap(string fileName) => inner.LoadWriteableBitmap(fileName);
    public IWriteableBitmapImpl LoadWriteableBitmap(Stream stream) => inner.LoadWriteableBitmap(stream);
    public IBitmapImpl LoadBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => inner.LoadBitmapToWidth(stream, width, interpolationMode);
    public IBitmapImpl LoadBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => inner.LoadBitmapToHeight(stream, height, interpolationMode);
    public IBitmapImpl ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => inner.ResizeBitmap(bitmapImpl, destinationSize, interpolationMode);
    public IBitmapImpl LoadBitmap(PixelFormat format, AlphaFormat alphaFormat, IntPtr data, PixelSize size, Vector dpi, int stride) => inner.LoadBitmap(format, alphaFormat, data, size, dpi, stride);
    public IGlyphRunImpl CreateGlyphRun(GlyphTypeface glyphTypeface, double fontRenderingEmSize, IReadOnlyList<GlyphInfo> glyphInfos, Point baselineOrigin) => inner.CreateGlyphRun(glyphTypeface, fontRenderingEmSize, glyphInfos, baselineOrigin);
    public IPlatformRenderInterfaceContext CreateBackendContext(IPlatformGraphicsContext? graphicsApiContext) => new Context(inner.CreateBackendContext(graphicsApiContext), compatibility);
    public bool IsSupportedBitmapPixelFormat(PixelFormat format) => inner.IsSupportedBitmapPixelFormat(format);
    public IPlatformRenderInterfaceRegion CreateRegion() => inner.CreateRegion();
    public bool SupportsIndividualRoundRects => inner.SupportsIndividualRoundRects;
    public AlphaFormat DefaultAlphaFormat => inner.DefaultAlphaFormat;
    public PixelFormat DefaultPixelFormat => inner.DefaultPixelFormat;
    public bool SupportsRegions => inner.SupportsRegions;

    private sealed class Context(
        IPlatformRenderInterfaceContext inner,
        WaylandFramePacingCompatibility compatibility) : IPlatformRenderInterfaceContext
    {
        private IEnumerable<IPlatformRenderSurface> Wrap(IEnumerable<IPlatformRenderSurface> surfaces) => surfaces.Select(compatibility.Wrap);
        public IRenderTarget CreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces) => inner.CreateRenderTarget(Wrap(surfaces));
        public bool IsReadyToCreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces) => inner.IsReadyToCreateRenderTarget(Wrap(surfaces));
        public IDrawingContextLayerImpl CreateOffscreenRenderTarget(PixelSize size, Vector scaling, bool text) => inner.CreateOffscreenRenderTarget(size, scaling, text);
        public bool IsLost => inner.IsLost;
        public IReadOnlyDictionary<Type, object> PublicFeatures => inner.PublicFeatures;
        public PixelSize? MaxOffscreenRenderTargetPixelSize => inner.MaxOffscreenRenderTargetPixelSize;
        public object? TryGetFeature(Type type) => inner.TryGetFeature(type);
        public void Dispose() => inner.Dispose();
    }
}
