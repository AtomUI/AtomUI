# 软件 Skia 兼容构建

本项目重建 `Avalonia.Skia` 管理程序集，修复软件绘制在完整、单矩形和复合损伤裁剪间不一致的覆盖结果。它保留控件自己的裁剪、背景、阴影次序、矩阵及 native 资源生命周期；GPU 与未知外部保存层沿原路径。

`upstream/` 来自 Avalonia `12.1.4` 的提交 `2d79ba55fae1b1e71c69f93a668080c9029e6a31`。原文件散列、排除的上游工程文件及批准补丁散列见 [upstream-manifest.json](upstream-manifest.json)，许可证和声明随源码保留。功能修改仅有四文件：

- `DrawingContextImpl.cs`：原语覆盖、固有 clip 一次应用、完整覆盖快速路径及资源作用域。
- `DrawingContextImpl.Effects.cs`：现有保存层身份记录。
- `FramebufferRenderTarget.cs`：区分 scene 目标和独立 RenderTargetBitmap。
- `SurfaceRenderTarget.cs`：向真实 compositor 中间层传递 scene 损伤身份。

四个本地修改文件同时按仓库规则补齐花括号；Release 569 个方法体 IL 与格式调整前逐一相同。其他上游源码保持原样。上游版本更新时，必须重新对比这四文件及引用的 API，不能只改版本号。

```sh
dotnet build compat/Avalonia.Skia/AtomUI.Compatibility.Avalonia.Skia.csproj -c Release
```

构建只需仓库内源码与固定 NuGet 依赖，不访问参考 checkout。支持 net10/net8，程序集身份保持 `Avalonia.Skia, Version=12.1.4.0, PublicKeyToken=c8d484a7012f9a8b`。SkiaSharp `3.119.4`、HarfBuzzSharp `8.3.1.3` 和 native 资产保持官方版本，不写入官方 NuGet 缓存。

[仓库接入](../../build/AtomUI.SkiaCompatibility.targets)使现有 Gallery host 与 Avalonia 测试显式引用此项目，并在资产解析阶段排除重复的官方管理 DLL。基础库仍使用原公共依赖；兼容项目不引用 AtomUI 控件、Generator 或 Toolchain。此构建不自动进入对外发布的控件 NuGet 包。

`AtomUIUseSkiaCompatibility=false` 可显式关闭仓库接入，供同条件官方基线验证；它不表示基线缺陷通过。正式批准、原失败、完整矩阵与尚未覆盖的边界见[权威记录](../../docs/superpowers/plans/2026-10-22-custom-theme/feedback-region-sdk-results.md)。
