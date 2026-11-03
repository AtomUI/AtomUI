# CT15 软件 Skia 裁剪一致性候选审查

本记录取代较早SDK8的审查输入；旧批次、拒绝路线、原始失败及全部数值只保留在[权威结果](feedback-region-sdk-results.md)。已获用户正式采用批准，现由仓库内compat/Avalonia.Skia可重建工程接入；实际落地证据见[采用结果](feedback-region-sdk-adoption-results.md)。此前临时阶段与原失败按历史保留，不代表完整CT15自动通过。

## 问题和修正边界

原Avatar明暗12像素恢复可由真实native参数中的1–2行裁剪精确复制，坐标和有符号RGBA差均一致。修正只针对软件Skia：以原语完整范围计算覆盖，保留控件固有clip、同一目标背景和原shadow/fill/stroke顺序，最终仅写回原root dirty region；不改变尺寸、字体、内容、焦点、滚动、原图像采样或动效。GPU及未知外部save/layer作用域保持原路径，不把CPU证据当作GPU验证。

当前只改精确Avalonia12.1.4对象`2d79ba55fae1b1e71c69f93a668080c9029e6a31`的`DrawingContextImpl.cs`、`DrawingContextImpl.Effects.cs`、`FramebufferRenderTarget.cs`和`SurfaceRenderTarget.cs`。没有改Base、SkiaSharp/HarfBuzz/native库或平台默认CompositionOptions；FW1和CT12/13/14原结论保持。程序集版本/公钥标记仍12.1.4.0/c8d484a7012f9a8b，未向NuGet缓存安装修改版。

## 工程合同

- 中间surface、paint、clip path与region副本由当帧context或clip作用域持有、配对释放；不增加控件订阅或常驻控件缓存。
- region从Push起即持独立native快照，调用方可修改/释放原对象；退出后继续绘制有效。原75项及当前73项真实图形、lease、clip/matrix、DPI/变换与native Handle归零检查通过。
- 固有AA clip只应用一次；临时写回后按原矩阵和保存层次恢复Canvas。内部bitmap/rectangle调用临时Canvas借用均finally恢复；对外Canvas/Surface/lease合同保持。
- 像素对齐、无圆角、无描边阴影的矩形沿原快速路径。原66色Tag负载同绘制算法交错CPU测量最终实现同负载交错测量约增加0.139ms/帧（5.95%）；这不是零开销或全负载性能结论。

## 当前验收状态

最终性能收敛版r4：JIT/AOT各1632性质通过，652静态跨运行及完整server参考全部RGBA0，Float32真实clock仍未配对相位。最终73图形/lease/区域scope/native释放检查通过；候选net8零警告，AOT零IL/CS警告、五条原linker警告单列。约+0.139ms/帧（5.95%）是原66-Tag负载的实测成本。

11875取证及949外部会话文件已清理；清理后真实重建精确模块193+7通过，临时C#0、diff检查通过。当前官方产品依赖没有替换；此记录是已工程化验证的唯一候选，正式采用批准已登记并执行，详见采用结果。GPU、其他未覆盖路径、原CT25/FW1和CT12/13/14结论不扩大，整个CT15不因本包通过而自动完成。

## 最终快路径

原root损伤完整覆盖primitive可见范围时，沿原Canvas收窄保守clip并直接绘制；实际截断才分配同背景scratch。两条路径执行同一原绘制命令，保留原clip、matrix、save-count和lease。真实两行截断用于native资源创建/释放正对照，完整覆盖时不要求人为分配scratch。

## 精确可重建补丁

当前r4四文件补丁SHA256：`5f3f7577ce54fed650ae9673f38334f9ad136a35b90a55c85c812c3bd5747def`。SDK输入SHA `9236ca0713e2f8ede9e26f566c1083ce7da825e70d382f69371a0648c513e8a0`。先前r2/r3的通过及失败数据按各自源码范围保留在权威结果。

```diff
--- a/src/Skia/Avalonia.Skia/DrawingContextImpl.cs
+++ b/src/Skia/Avalonia.Skia/DrawingContextImpl.cs
@@ -43,6 +43,14 @@
         private readonly ISkiaGpuRenderSession? _session;
         private bool _leased;
         private bool _useOpacitySaveLayer;
+        private int _outerRasterLayerSaveCount;
+        private readonly bool _captureRootDamage;
+        private SKSurface? _primitiveRaster;
+        private SKPaint? _rectangleCoveragePaint;
+        private bool _rasterizingPrimitive;
+        private SKRegion? _primitiveDamageRegion;
+        private int _primitiveDamageSaveCount;
+        private readonly List<PrimitiveClip> _primitiveClips = new();
 
         /// <summary>
         /// Context create info.
@@ -63,6 +71,7 @@
             /// Makes DPI to be applied as a hidden matrix transform
             /// </summary>
             public bool ScaleDrawingToDpi;
+            public bool CaptureRootDamage;
             
             /// <summary>
             /// Dpi for intermediate surfaces
@@ -187,6 +196,7 @@
             Canvas = createInfo.Canvas ?? createInfo.Surface?.Canvas
                 ?? throw new ArgumentException("Invalid create info - no Canvas provided", nameof(createInfo));
 
+            _captureRootDamage = createInfo.CaptureRootDamage;
             _intermediateSurfaceDpi = createInfo.Dpi;
             _disposables = disposables;
             _disableSubpixelTextRendering = createInfo.DisableSubpixelTextRendering;
@@ -219,7 +229,7 @@
         /// <summary>
         /// Skia canvas.
         /// </summary>
-        public SKCanvas Canvas { get; }
+        public SKCanvas Canvas { get; private set; }
         public SKSurface? Surface { get; }
 
         public RenderOptions RenderOptions { get; set; }
@@ -254,7 +264,12 @@
             paint.BlendMode = RenderOptions.BitmapBlendingMode.ToSKBlendMode();
             paint.IsAntialias = RenderOptions.EdgeMode != EdgeMode.Aliased;
 
-            drawableImage.Draw(this, s, d, samplingOptions, paint);
+            if (drawableImage is not (ImmutableBitmap or WriteableBitmapImpl) ||
+                !TryDrawCompletePrimitive(null, null, paint, destRect, bitmap: drawableImage,
+                    bitmapSource: s, bitmapDestination: d, bitmapSampling: samplingOptions))
+            {
+                drawableImage.Draw(this, s, d, samplingOptions, paint);
+            }
             SKPaintCache.Shared.ReturnReset(paint);
         }
 
@@ -277,7 +292,11 @@
             {
                 using (stroke)
                 {
-                    Canvas.DrawLine((float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y, stroke.Paint);
+                    var lineBounds = new Rect(p1,p2).Normalize().Inflate(Math.Max(0,pen.Thickness));
+                    if (!TryDrawCompletePrimitive(null,null,stroke.Paint,lineBounds,lineStart:p1,lineEnd:p2))
+                    {
+                        Canvas.DrawLine((float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y, stroke.Paint);
+                    }
                 }
             }
         }
@@ -293,7 +312,10 @@
             {
                 using (var fill = CreatePaint(_fillPaint, brush, rect))
                 {
-                    Canvas.DrawPath(impl.FillPath, fill.Paint);
+                    if (!TryDrawCompletePrimitive(impl.FillPath, null, fill.Paint, rect))
+                    {
+                        Canvas.DrawPath(impl.FillPath, fill.Paint);
+                    }
                 }
             }
 
@@ -303,8 +325,235 @@
             {
                 using (stroke)
                 {
-                    Canvas.DrawPath(impl.StrokePath, stroke.Paint);
-                }
+                    if (!TryDrawCompletePrimitive(impl.StrokePath, null, stroke.Paint, geometry.GetRenderBounds(pen)))
+                    {
+                        Canvas.DrawPath(impl.StrokePath, stroke.Paint);
+                    }
+                }
+            }
+        }
+
+        // Rasterize AA coverage using the complete primitive bounds. Partial
+        // raster clips can change Skia's coverage. Seed with the actual current
+        // destination so SrcOver retains its original integer rounding, then copy
+        // only through the unchanged caller clip. All buffers are per-call leases.
+        private bool TryDrawCompletePrimitive(SKPath? path, SKRoundRect? roundRect, SKPaint paint, Rect bounds, SKTextBlob? textBlob = null, Point origin = default, IDrawableBitmapImpl? bitmap = null,
+            SKRect bitmapSource = default, SKRect bitmapDestination = default, SKSamplingOptions bitmapSampling = default,
+            RoundedRect? rectangle = null, IBrush? rectangleBrush = null, IPen? rectanglePen = null, BoxShadows rectangleShadows = default, Point? lineStart = null, Point lineEnd = default)
+        {
+            if (_grContext is not null || _rasterizingPrimitive || !paint.IsAntialias || Canvas.IsClipEmpty || _outerRasterLayerSaveCount != 0 || Surface is null || paint.ImageFilter is not null ||
+                paint.MaskFilter is not null || paint.BlendMode != SKBlendMode.SrcOver)
+            {
+                return false;
+            }
+            var matrix = Canvas.TotalMatrix;
+            if (matrix.Persp0 != 0 || matrix.Persp1 != 0 || matrix.Persp2 != 1)
+            {
+                return false;
+            }
+            var device = matrix.MapRect(bounds.ToSKRect());
+            var post = _postTransform ?? Matrix.Identity;
+            foreach (var shadow in rectangleShadows)
+            {
+                if (shadow == default || shadow.IsInset)
+                {
+                    continue;
+                }
+                var extent = Math.Ceiling(3 * SkBlurRadiusToSigma(shadow.Blur)) + 1;
+                var caster = bounds.Inflate(Math.Max(0, shadow.Spread) + extent);
+                var actual = matrix.MapRect(caster.ToSKRect());
+                actual.Offset((float)(shadow.OffsetX * post.M11 + shadow.OffsetY * post.M21),
+                    (float)(shadow.OffsetX * post.M12 + shadow.OffsetY * post.M22));
+                device = SKRect.Union(device, actual);
+            }
+            if (!float.IsFinite(device.Left) || !float.IsFinite(device.Top) ||
+                !float.IsFinite(device.Right) || !float.IsFinite(device.Bottom) ||
+                device.Left < int.MinValue + 4096d || device.Top < int.MinValue + 4096d ||
+                device.Right > int.MaxValue - 4096d || device.Bottom > int.MaxValue - 4096d)
+            {
+                return false;
+            }
+            device.Inflate(1, 1);
+            var visible = device;
+            if (_primitiveClips.Count > 0)
+            {
+                visible = SKRect.Intersect(visible, _primitiveClips[_primitiveClips.Count - 1].Domain);
+            }
+            if (_primitiveDamageRegion is null || visible.IsEmpty || (_primitiveDamageRegion.IsRect &&
+                _primitiveDamageRegion.Contains(new SKRectI(
+                    (int)Math.Floor(visible.Left), (int)Math.Floor(visible.Top),
+                    (int)Math.Ceiling(visible.Right), (int)Math.Ceiling(visible.Bottom)))))
+            {
+                return false;
+            }
+            var clip = Canvas.DeviceClipBounds;
+            if (Canvas.IsClipRect && clip.Left <= device.Left && clip.Top <= device.Top &&
+                clip.Right >= device.Right && clip.Bottom >= device.Bottom)
+            {
+                return false;
+            }
+            var afterDamage = false;
+            var intrinsicCount = 0;
+            foreach (var clipEntry in _primitiveClips)
+            {
+                if (clipEntry.IsDamage)
+                {
+                    afterDamage = true;
+                }
+                else if (afterDamage)
+                {
+                    intrinsicCount++;
+                }
+            }
+            if (_primitiveDamageSaveCount == 0 || Canvas.SaveCount != _primitiveDamageSaveCount + intrinsicCount)
+            {
+                return false;
+            }
+            if (_primitiveDamageRegion.Contains(new SKRectI((int)Math.Floor(visible.Left),
+                (int)Math.Floor(visible.Top), (int)Math.Ceiling(visible.Right), (int)Math.Ceiling(visible.Bottom))))
+            {
+                var saved = Canvas.SaveCount;
+                var nativeMatrix = Canvas.TotalMatrix44;
+                var cachedMatrix = _currentTransform;
+                Canvas.Save();
+                try
+                {
+                    Canvas.ResetMatrix();
+                    Canvas.ClipRect(new SKRect((float)Math.Floor(device.Left), (float)Math.Floor(device.Top),
+                        (float)Math.Ceiling(device.Right), (float)Math.Ceiling(device.Bottom)));
+                    Canvas.SetMatrix(nativeMatrix);
+                    DrawPrimitive(Canvas, path, roundRect, paint, textBlob, origin, bitmap, bitmapSource,
+                        bitmapDestination, bitmapSampling, rectangle, rectangleBrush, rectanglePen,
+                        rectangleShadows, lineStart, lineEnd);
+                }
+                finally
+                {
+                    Canvas.RestoreToCount(saved);
+                    _currentTransform = cachedMatrix;
+                }
+                return true;
+            }
+            if (_primitiveRaster is null)
+            {
+                using var snapshot = Surface.Snapshot();
+                _primitiveRaster = SKSurface.Create(snapshot.Info, Surface.SurfaceProperties);
+            }
+            if (_primitiveRaster is null)
+            {
+                return false;
+            }
+            SKImageInfo surfaceInfo;
+            using (var snapshot = _primitiveRaster.Snapshot())
+            {
+                surfaceInfo = snapshot.Info;
+            }
+            var left = Math.Max(0, (int)Math.Floor(device.Left) - 1);
+            var top = Math.Max(0, (int)Math.Floor(device.Top) - 1);
+            var right = Math.Min(surfaceInfo.Width, (int)Math.Ceiling(device.Right) + 1);
+            var bottom = Math.Min(surfaceInfo.Height, (int)Math.Ceiling(device.Bottom) + 1);
+            var width = right - left;
+            var height = bottom - top;
+            if (width <= 0 || height <= 0 || width > 8192 || height > 8192 || Surface is null)
+            {
+                return false;
+            }
+            var info = surfaceInfo.WithSize(width, height);
+            using var destination = new SKBitmap(info);
+            if (!Surface.ReadPixels(info, destination.GetPixels(), destination.RowBytes, left, top))
+            {
+                return false;
+            }
+            var raster = _primitiveRaster.Canvas;
+            raster.RestoreToCount(1);
+            raster.ResetMatrix();
+            using var copy = new SKPaint { BlendMode = SKBlendMode.Src, IsAntialias = false };
+            raster.DrawBitmap(destination, left, top, copy);
+            raster.Save();
+            foreach (var clipEntry in _primitiveClips)
+            {
+                clipEntry.Apply(raster);
+            }
+            raster.SetMatrix(matrix);
+            DrawPrimitive(raster, path, roundRect, paint, textBlob, origin, bitmap, bitmapSource, bitmapDestination, bitmapSampling, rectangle, rectangleBrush, rectanglePen, rectangleShadows, lineStart, lineEnd);
+            using var completed = _primitiveRaster.Snapshot(new SKRectI(left, top, right, bottom));
+            var matrix44 = Canvas.TotalMatrix44;
+            Canvas.RestoreToCount(_primitiveDamageSaveCount);
+            try
+            {
+                Canvas.ResetMatrix();
+                Canvas.DrawImage(completed, left, top, copy);
+            }
+            finally
+            {
+                afterDamage = false;
+                foreach (var clipEntry in _primitiveClips)
+                {
+                    if (clipEntry.IsDamage)
+                    {
+                        afterDamage = true;
+                    }
+                    else if (afterDamage)
+                    {
+                        Canvas.SetMatrix(clipEntry.Matrix);
+                        Canvas.Save();
+                        clipEntry.Apply(Canvas);
+                    }
+                }
+                Canvas.SetMatrix(matrix44);
+            }
+            return true;
+        }
+
+        private void DrawPrimitive(SKCanvas raster, SKPath? path, SKRoundRect? roundRect, SKPaint paint,
+            SKTextBlob? textBlob, Point origin, IDrawableBitmapImpl? bitmap, SKRect bitmapSource,
+            SKRect bitmapDestination, SKSamplingOptions bitmapSampling, RoundedRect? rectangle,
+            IBrush? rectangleBrush, IPen? rectanglePen, BoxShadows rectangleShadows, Point? lineStart, Point lineEnd)
+        {
+            if (lineStart is { } startPoint)
+            {
+                raster.DrawLine((float)startPoint.X,(float)startPoint.Y,(float)lineEnd.X,(float)lineEnd.Y,paint);
+            }
+            else if (rectangle is { } completeRectangle)
+            {
+                var target = Canvas;
+                var savedTransform = _currentTransform;
+                Canvas = raster;
+                _rasterizingPrimitive = true;
+                try
+                {
+                    DrawRectangleCore(rectangleBrush, rectanglePen, completeRectangle, rectangleShadows);
+                }
+                finally
+                {
+                    _rasterizingPrimitive = false;
+                    Canvas = target;
+                    _currentTransform = savedTransform;
+                }
+            }
+            else if (path is not null)
+            {
+                raster.DrawPath(path, paint);
+            }
+            else if (bitmap is not null)
+            {
+                var target = Canvas;
+                Canvas = raster;
+                try
+                {
+                    bitmap.Draw(this, bitmapSource, bitmapDestination, bitmapSampling, paint);
+                }
+                finally
+                {
+                    Canvas = target;
+                }
+            }
+            else if (textBlob is not null)
+            {
+                raster.DrawText(textBlob, (float)origin.X, (float)origin.Y, paint);
+            }
+            else
+            {
+                raster.DrawRoundRect(roundRect!, paint);
             }
         }
 
@@ -414,6 +663,40 @@
 
         /// <inheritdoc />
         public void DrawRectangle(IBrush? brush, IPen? pen, RoundedRect rect, BoxShadows boxShadows = default)
+        {
+            if (rect.Rect.Width <= 0 || rect.Rect.Height <= 0)
+            {
+                return;
+            }
+            CheckLease();
+            if (_rasterizingPrimitive || RenderOptions.EdgeMode == EdgeMode.Aliased)
+            {
+                DrawRectangleCore(brush, pen, rect, boxShadows);
+                return;
+            }
+            if (!rect.IsRounded && boxShadows.Count == 0 && (pen?.Thickness ?? 0) == 0)
+            {
+                var matrix = Canvas.TotalMatrix;
+                var device = matrix.MapRect(rect.Rect.ToSKRect());
+                if (matrix.SkewX == 0 && matrix.SkewY == 0 && matrix.Persp0 == 0 && matrix.Persp1 == 0 &&
+                    device.Left == MathF.Floor(device.Left) && device.Top == MathF.Floor(device.Top) &&
+                    device.Right == MathF.Floor(device.Right) && device.Bottom == MathF.Floor(device.Bottom))
+                {
+                    DrawRectangleCore(brush, pen, rect, boxShadows);
+                    return;
+                }
+            }
+            _rectangleCoveragePaint ??= new SKPaint { IsAntialias = true };
+            var bounds = boxShadows.TransformBounds(rect.Rect.Inflate(Math.Max(0, pen?.Thickness ?? 0) / 2));
+            if (!TryDrawCompletePrimitive(null, null, _rectangleCoveragePaint, bounds, rectangle: rect,
+                rectangleBrush: brush, rectanglePen: pen, rectangleShadows: boxShadows))
+            {
+                DrawRectangleCore(brush, pen, rect, boxShadows);
+            }
+        }
+
+        private void DrawRectangleCore(IBrush? brush, IPen? pen, RoundedRect rect, BoxShadows boxShadows)
+
         {
             if (rect.Rect.Height <= 0 || rect.Rect.Width <= 0)
                 return;
@@ -480,7 +763,10 @@
                 {
                     if (isRounded)
                     {
-                        Canvas.DrawRoundRect(skRoundRect, fill.Paint);
+                        if (!TryDrawCompletePrimitive(null, skRoundRect, fill.Paint, rect.Rect))
+                            {
+                                Canvas.DrawRoundRect(skRoundRect, fill.Paint);
+                            }
                     }
                     else
                     {
@@ -525,7 +811,10 @@
                 {
                     if (isRounded)
                     {
-                        Canvas.DrawRoundRect(skRoundRect, stroke.Paint);
+                        if (!TryDrawCompletePrimitive(null, skRoundRect, stroke.Paint, rect.Rect.Inflate(pen.Thickness / 2)))
+                        {
+                            Canvas.DrawRoundRect(skRoundRect, stroke.Paint);
+                        }
                     }
                     else
                     {
@@ -632,8 +921,13 @@
 
                 var textBlob = glyphRunImpl.GetTextBlob(effectiveTextOptions, RenderOptions);
 
-                Canvas.DrawText(textBlob, (float)glyphRun.BaselineOrigin.X,
-                    (float)glyphRun.BaselineOrigin.Y, paintWrapper.Paint);
+                var textBounds = glyphRun.Bounds.Union(new Rect(textBlob.Bounds.Left + glyphRun.BaselineOrigin.X,
+                    textBlob.Bounds.Top + glyphRun.BaselineOrigin.Y, textBlob.Bounds.Width, textBlob.Bounds.Height));
+                if (!TryDrawCompletePrimitive(null, null, paintWrapper.Paint, textBounds, textBlob, glyphRun.BaselineOrigin))
+                {
+                    Canvas.DrawText(textBlob, (float)glyphRun.BaselineOrigin.X,
+                        (float)glyphRun.BaselineOrigin.Y, paintWrapper.Paint);
+                }
             }
         }
 
@@ -648,13 +942,21 @@
         public void PushClip(Rect clip)
         {
             CheckLease();
+            var damage = _captureRootDamage && _primitiveClips.Count == 0 && _primitiveDamageRegion is null;
+            PushPrimitiveClip(new PrimitiveClip { Rect = clip.ToSKRect(), IsDamage = damage }, damage ? null : clip.ToSKRect());
             Canvas.Save();
             Canvas.ClipRect(clip.ToSKRect());
+            if (damage)
+            {
+                _primitiveDamageRegion = new SKRegion(Canvas.DeviceClipBounds);
+                _primitiveDamageSaveCount = Canvas.SaveCount;
+            }
         }
 
         public void PushClip(RoundedRect clip)
         {
             CheckLease();
+            PushPrimitiveClip(new PrimitiveClip { Rounded = clip }, clip.Rect.ToSKRect());
             Canvas.Save();
 
             // Get the rounded rectangle
@@ -680,13 +982,114 @@
         {
             var r = ((SkiaRegionImpl)region).Region;
             CheckLease();
+            var damage = _primitiveDamageRegion is null;
+            PushPrimitiveClip(new PrimitiveClip { IsDamage = damage, Region = damage ? null : new SKRegion(r) },
+                damage ? null : new SKRect(r.Bounds.Left, r.Bounds.Top, r.Bounds.Right, r.Bounds.Bottom));
             Canvas.Save();
+            _primitiveDamageRegion ??= new SKRegion(r);
             Canvas.ClipRegion(r);
+            if (damage)
+            {
+                _primitiveDamageSaveCount = Canvas.SaveCount;
+            }
+        }
+
+        private sealed class PrimitiveClip : IDisposable
+        {
+            public SKMatrix Matrix;
+            public SKRect? Rect;
+            public RoundedRect? Rounded;
+            public SKPath? Path;
+            public SKRegion? Region;
+            public SKRect Domain;
+            public bool IsDamage;
+            public void Apply(SKCanvas canvas)
+            {
+                if (IsDamage)
+                {
+                    return;
+                }
+                canvas.SetMatrix(Matrix);
+                if (Rect is { } rect)
+                {
+                    canvas.ClipRect(rect);
+                }
+                else if (Rounded is { } rounded)
+                {
+                    using var native = new SKRoundRect();
+                    native.SetRectRadii(rounded.Rect.ToSKRect(), new[] {
+                        rounded.RadiiTopLeft.ToSKPoint(), rounded.RadiiTopRight.ToSKPoint(),
+                        rounded.RadiiBottomRight.ToSKPoint(), rounded.RadiiBottomLeft.ToSKPoint() });
+                    canvas.ClipRoundRect(native, antialias: true);
+                }
+                else if (Path is not null)
+                {
+                    canvas.ClipPath(Path, SKClipOperation.Intersect, true);
+                }
+                else if (Region is not null)
+                {
+                    canvas.ClipRegion(Region);
+                }
+            }
+            public void Dispose()
+            {
+                Path?.Dispose();
+                Region?.Dispose();
+            }
+        }
+
+        private void PushPrimitiveClip(PrimitiveClip clip, SKRect? bounds)
+        {
+            clip.Matrix = Canvas.TotalMatrix;
+            var current = Canvas.DeviceClipBounds;
+            clip.Domain = _primitiveClips.Count > 0 ? _primitiveClips[_primitiveClips.Count - 1].Domain :
+                new SKRect(current.Left, current.Top, current.Right, current.Bottom);
+            if (bounds is { } rect)
+            {
+                clip.Domain = SKRect.Intersect(clip.Domain, clip.Matrix.MapRect(rect));
+            }
+            _primitiveClips.Add(clip);
+        }
+
+        private void PopPrimitiveClip()
+        {
+            var index = _primitiveClips.Count - 1;
+            var clip = _primitiveClips[index];
+            _primitiveClips.RemoveAt(index);
+            if (clip.IsDamage)
+            {
+                _primitiveDamageRegion?.Dispose();
+                _primitiveDamageRegion = null;
+                _primitiveDamageSaveCount = 0;
+            }
+            clip.Dispose();
+        }
+
+        private void SaveRasterLayer(SKPaint? paint)
+        {
+            Canvas.SaveLayer(paint);
+            if (_outerRasterLayerSaveCount == 0)
+            {
+                _outerRasterLayerSaveCount = Canvas.SaveCount;
+            }
+        }
+
+        private void SaveRasterLayer(SKRect bounds, SKPaint? paint)
+        {
+            Canvas.SaveLayer(bounds, paint);
+            if (_outerRasterLayerSaveCount == 0)
+            {
+                _outerRasterLayerSaveCount = Canvas.SaveCount;
+            }
         }
 
         private void RestoreCanvas()
         {
             _currentTransform = null;
+            if (Canvas.SaveCount == _outerRasterLayerSaveCount)
+            {
+                _outerRasterLayerSaveCount = 0;
+            }
             Canvas.Restore();
         }
         
@@ -695,12 +1098,13 @@
         {
             CheckLease();
             RestoreCanvas();
+            PopPrimitiveClip();
         }
 
         public void PushLayer(Rect bounds)
         {
             CheckLease();
-            Canvas.SaveLayer(bounds.ToSKRect(), null!);
+            SaveRasterLayer(bounds.ToSKRect(), null!);
         }
 
         public void PopLayer()
@@ -727,11 +1131,11 @@
                 if (bounds.HasValue)
                 {
                     var rect = bounds.Value.ToSKRect();
-                    Canvas.SaveLayer(rect, new SKPaint { ColorF = new SKColorF(0, 0, 0, (float)opacity) });
+                    SaveRasterLayer(rect, new SKPaint { ColorF = new SKColorF(0, 0, 0, (float)opacity) });
                 }
                 else
                 {
-                    Canvas.SaveLayer(new SKPaint { ColorF = new SKColorF(0, 0, 0, (float)opacity) });
+                    SaveRasterLayer(new SKPaint { ColorF = new SKColorF(0, 0, 0, (float)opacity) });
                 }
             }
             else
@@ -792,6 +1196,17 @@
             CheckLease();
             try
             {
+                foreach (var clipEntry in _primitiveClips)
+                {
+                    clipEntry.Dispose();
+                }
+                _primitiveClips.Clear();
+                _primitiveDamageRegion?.Dispose();
+                _primitiveDamageRegion = null;
+                _rectangleCoveragePaint?.Dispose();
+                _rectangleCoveragePaint = null;
+                _primitiveRaster?.Dispose();
+                _primitiveRaster = null;
                 // Return leased paints.
                 SKPaintCache.Shared.ReturnReset(_strokePaint);
                 SKPaintCache.Shared.ReturnReset(_fillPaint);
@@ -820,6 +1235,7 @@
         public void PushGeometryClip(IGeometryImpl clip)
         {
             CheckLease();
+            PushPrimitiveClip(new PrimitiveClip { Path = new SKPath(((GeometryImpl)clip).FillPath) }, clip.Bounds.ToSKRect());
             Canvas.Save();
             Canvas.ClipPath(((GeometryImpl)clip).FillPath, SKClipOperation.Intersect, true);
         }
@@ -829,6 +1245,7 @@
         {
             CheckLease();
             RestoreCanvas();
+            PopPrimitiveClip();
         }
 
         /// <inheritdoc />
@@ -838,7 +1255,7 @@
 
             var paint = SKPaintCache.Shared.Get();
 
-            Canvas.SaveLayer(bounds.ToSKRect(), paint);
+            SaveRasterLayer(bounds.ToSKRect(), paint);
             _maskStack.Push((Canvas.TotalMatrix, CreatePaint(paint, mask, bounds)));
         }
 
@@ -850,7 +1267,7 @@
             var paint = SKPaintCache.Shared.Get();
             paint.BlendMode = SKBlendMode.DstIn;
             
-            Canvas.SaveLayer(paint);
+            SaveRasterLayer(paint);
             SKPaintCache.Shared.ReturnReset(paint);
             
             var (transform, paintWrapper) = _maskStack.Pop();
@@ -1511,7 +1928,8 @@
                 Gpu = _gpu,
                 Session = _session,
                 DisableManualFbo = !isLayer,
-                UseScaledDrawing = useScaledDrawing
+                UseScaledDrawing = useScaledDrawing,
+                CaptureRootDamage = isLayer && _captureRootDamage
             };
 
             return new SurfaceRenderTarget(createInfo);
--- a/src/Skia/Avalonia.Skia/DrawingContextImpl.Effects.cs
+++ b/src/Skia/Avalonia.Skia/DrawingContextImpl.Effects.cs
@@ -14,9 +14,9 @@
         var paint = SKPaintCache.Shared.Get();
         paint.ImageFilter = filter;
         if (effectClipRect.HasValue)
-            Canvas.SaveLayer(effectClipRect.Value.ToSKRect(), paint);
+            SaveRasterLayer(effectClipRect.Value.ToSKRect(), paint);
         else
-            Canvas.SaveLayer(paint);
+            SaveRasterLayer(paint);
         SKPaintCache.Shared.ReturnReset(paint);
     }
 
--- a/src/Skia/Avalonia.Skia/FramebufferRenderTarget.cs
+++ b/src/Skia/Avalonia.Skia/FramebufferRenderTarget.cs
@@ -13,6 +13,7 @@
     internal class FramebufferRenderTarget : IRenderTarget
     {
         private readonly bool _useScaledDrawing;
+        private readonly bool _captureRootDamage;
         private SKImageInfo _currentImageInfo;
         private IntPtr _currentFramebufferAddress;
         private SKSurface? _framebufferSurface;
@@ -29,6 +30,7 @@
         public FramebufferRenderTarget(IFramebufferPlatformSurface platformSurface, bool useScaledDrawing = false)
         {
             _useScaledDrawing = useScaledDrawing;
+            _captureRootDamage = platformSurface is not IRenderTargetBitmapImpl;
             _renderTarget = platformSurface.CreateFramebufferRenderTarget();
         }
 
@@ -76,7 +78,8 @@
             {
                 Surface = _framebufferSurface,
                 Dpi = framebuffer.Dpi,
-                ScaleDrawingToDpi = _useScaledDrawing
+                ScaleDrawingToDpi = _useScaledDrawing,
+                CaptureRootDamage = _captureRootDamage
             };
 
             properties = new()
--- a/src/Skia/Avalonia.Skia/SurfaceRenderTarget.cs
+++ b/src/Skia/Avalonia.Skia/SurfaceRenderTarget.cs
@@ -14,6 +14,7 @@
     internal class SurfaceRenderTarget : IDrawableBitmapImpl, IDrawingContextLayerWithRenderContextAffinityImpl
     {
         private readonly bool _useScaledDrawing;
+        private readonly bool _captureRootDamage;
         private readonly ISkiaSurface _surface;
         private readonly SKCanvas _canvas;
         private readonly bool _disableLcdRendering;
@@ -47,6 +48,7 @@
         public SurfaceRenderTarget(CreateInfo createInfo)
         {
             _useScaledDrawing = createInfo.UseScaledDrawing;
+            _captureRootDamage = createInfo.CaptureRootDamage;
             PixelSize = new PixelSize(createInfo.Width, createInfo.Height);
             Dpi = createInfo.Dpi;
 
@@ -111,6 +113,7 @@
                 Surface = _surface.Surface,
                 Dpi = Dpi,
                 ScaleDrawingToDpi = _useScaledDrawing,
+                CaptureRootDamage = _captureRootDamage,
                 DisableSubpixelTextRendering = _disableLcdRendering,
                 GrContext = _grContext,
                 Gpu = _gpu,
@@ -226,6 +229,7 @@
 
             public bool DisableManualFbo;
             public bool UseScaledDrawing;
+            public bool CaptureRootDamage;
         }
 
         public bool HasRenderContextAffinity => _grContext != null;
```
