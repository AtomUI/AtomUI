# Gallery ImagePreviewer 快速切换

在 `bugfix/shadcn` 的 Gallery → ImagePreviewer → Rapid switching 复现。原先每 200 毫秒更换目标，模拟加载耗时 300 毫秒，后续请求在前一张显示前就已到来。示例将间隔改为 700 毫秒，保留 300 毫秒的加载过程，为新图留出约 400 毫秒的展示时间。四种语言的说明同步描述两种模式。

## 脚本视觉验收

使用真实 Gallery 页面、原有图片和 Shadcn Luma 浅色主题。通过 Avalonia 指针输入操作开关、Start 和 Stop，按实际 macOS 窗口编号截图。两版分别在相同状态下复拍与复位，封面区域像素差均为零，1 DIP 偏移可以检出。

| 验收项 | 修复前 | 修复后 |
| --- | --- | --- |
| 立即切换 | 运行中 0 张新图，加载占位持续可见 | 约 2.9 秒内显示 3 张新图，每次等待时出现占位图 |
| 等待加载 | 运行中 0 张新图，始终保留同一张 | 约 2.9 秒内显示 3 张新图，等待时的上一张与加载前像素一致 |
| Start 和 Stop | 空闲与运行状态正确 | 按钮状态正确，停止后没有新增切换请求，最后一次加载正常完成 |

已查看修复前后的原生截图。两种模式的前两次出图在共同可见区域分别有 95.5% 和 99.5% 的像素明显变化。临时驱动观察运行状态，截图独立确认占位图和实际图片。

临时驱动和截图已清理，取证文件审计通过。清理后运行 `dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUIGallery.Tests.ShowCases.ImagePreviewer|FullyQualifiedName~AtomUIGallery.Tests.Localization.GalleryCatalogCoverageTests|FullyQualifiedName~AtomUIGallery.Tests.Localization.GalleryLocalizationTests'`，15 项通过。
