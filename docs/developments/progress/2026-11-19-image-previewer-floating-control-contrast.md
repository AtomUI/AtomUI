# ImagePreviewer 浮动控件对比度

`bugfix/shadcn` worktree，macOS 原生 Gallery，Shadcn Luma。脚本通过 Avalonia 指针路由打开「20 remote images」，翻到 PicSum 21（2 / 20），在 1210 × 693 DIP、2× 缩放的窗口上通过 `screencapture -x -o -l` 采样。修复前后的同状态复拍与 1 DIP 扰动复位均为零像素差，扰动可以检出。前后翻页和工具栏的部件矩形一致，已比较原生标题栏和照片内部区域。

| 验收项 | 修复前（浅色） | 修复后（浅色） | 修复后（深色） |
| --- | ---: | ---: | ---: |
| 上一张、下一张图标 | 1.21:1 | 7.94:1 | 16.53:1 |
| 可用的放大图标 | 1.14:1 | 7.94:1 | 16.53:1 |
| 禁用的缩小图标 | 1.05:1 | 2.95:1 | 3.70:1 |
| 首张禁用的上一张图标 | 1.01:1 | 2.95:1 | 3.70:1 |
| 下一张悬停图标 | 12.33:1 | 14.55:1 | 20.75:1 |

照片放大后覆盖工具栏背面时，重新启用的缩小图标实测 8.21:1。已查看前后原生截图，以及浅色悬停、深色、放大状态。实点下一张、缩放和上一张验证索引、缩放状态及边界禁用。原生截图覆盖 macOS 独立预览窗口，浏览器叠层关闭按钮沿用同组 Token。

另外核对了 Gallery 的「Rapid switching」封面演示。启动时每 200 毫秒请求一张新图，模拟加载耗时 300 毫秒。在各 24 次运行采样中，`Immediate` 全部为占位，`WaitForLoaded` 始终显示同一张旧图。停止后最新图会显示，Start 和 Stop 的启用条件与运行状态一致。本次对该示例做现象确认。

已删除临时截图和脚本，工作区取证文件审计通过。清理后使用 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ImagePreviewer.'` 复验，119 项通过。
