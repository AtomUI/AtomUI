# Windows 10 实机验证快速上手：窗口 live resize 与窗口型 Popup 阴影

日期：2026-10-29。状态：待在 Windows 10 上执行。

Windows 11 上已完成并通过脚本化验证的两项工作，需要在 Windows 10 上复测：

| 工作 | Windows 11 结论 | 正式文档 |
|---|---|---|
| 窗口 live resize（默认 `RedirectionSurface` 优先） | 拖左/上边缘时对向边缘缺口帧 0、白屏帧 0（每组 510 帧） | [Windows Live Resize 与窗口装饰架构](../../architecture/systems/windowing/windows-live-resize.md) |
| 窗口型 Popup 阴影统一 + 鼠标穿透（第一层同进程、第二层跨进程） | 全部通过，见 [方案与实施结果](2026-10-29-window-popup-shadow-hit-test-plan.md)“Windows 实施结果” | [Popup 实现原理](../../controls/desktop/other/popup/implementation.md) 8.1、[AtomUI.Native Windows](../../modules/native/windows.md) 第 6 节 |

Windows 11 验证环境：build 26300，2560×1600 @150%，Avalonia 12.1.4，Gallery Release，不挂调试器。

## 1. 准备

### 1.1 代码与构建

使用分支 `feature/windows-popup-shadow-hit-test`（基于 `experiment/windows-live-resize-layout-sync`，同时包含 live resize
与 Popup 阴影两部分改动）。

```powershell
git fetch; git switch feature/windows-popup-shadow-hit-test
dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Release
# 产物：.artifacts\bin\Release\net10.0\AtomUIGallery.Desktop.exe
```

### 1.2 工具

- PowerShell 7（`pwsh`）：脚本用到 `Add-Type` 编译 C#、UI Automation、`System.Drawing`。
- ffmpeg，需要带 `ddagrab` 滤镜（Desktop Duplication 录屏，Windows 8+ 可用）：`winget install Gyan.FFmpeg`，
  装完用 `ffmpeg -hide_banner -filters | Select-String ddagrab` 确认。

### 1.3 验证脚本不在仓库里

脚本位于 `tmp/visual-verification/windows/`，`tmp/` 被 `.gitignore` 忽略，**clone 下来没有**。从 Windows 11 开发机拷贝
整个目录（不含输出目录）到 Win10 机器同一位置：

```powershell
robocopy <源仓库>\tmp\visual-verification\windows <目标仓库>\tmp\visual-verification\windows *.ps1 README.md
```

| 文件 | 用途 |
|---|---|
| `README.md` | 脚本总说明，先读 |
| `backdrop.ps1` | 全屏置顶洋红背景（鼠标穿透、不可激活） |
| `uia-dump.ps1` | 导出 UIA 元素名称与矩形，定位失败时排查用 |
| `resize-test.ps1`、`blank-check.ps1` | live resize 对向边缘缺口 / 白屏统计 |
| `popup-test.ps1`、`native-popup-test.ps1` | overlay / 原生窗口 Popup 截图（live resize 时的回归项） |
| `shadow-common.ps1` | Popup 阴影脚本公共函数（dot-source） |
| `layered-probe.ps1`、`popup-shadow-anim.ps1` | `WS_EX_LAYERED` 前置验证、打开/关闭动效逐帧 |
| `popup-directions.ps1` | 12 方向 × 箭头显示/隐藏/居中定位截图 |
| `popup-shadow-layer1.ps1`、`popup-shadow-layer2.ps1`、`target-window.ps1` | 第一层 / 第二层穿透，跨进程靶子窗口 |

所有脚本默认 `-Exe D:\workspace\projects\AtomUI\.artifacts\bin\Release\net10.0\AtomUIGallery.Desktop.exe`，
仓库路径不同时每次传 `-Exe`。

### 1.4 记录环境

开始前记下，回填结果时一并写入：

```powershell
[Environment]::OSVersion.Version          # Win10 build，例如 19045
Get-CimInstance Win32_VideoController | Select-Object Name, DriverVersion
Add-Type -MemberDefinition '[DllImport("user32.dll")] public static extern int GetSystemMetrics(int i); [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr c);' -Name M -Namespace W
[void][W.M]::SetThreadDpiAwarenessContext([IntPtr]-4); "{0}x{1}" -f [W.M]::GetSystemMetrics(0), [W.M]::GetSystemMetrics(1)   # 物理分辨率
```

外加显示设置里的缩放比例。确认实际合成模式是 `RedirectionSurface`：`native-popup-test.ps1` 输出的浮层 `ex=0x...`
不含 `0x200000`（`WS_EX_NOREDIRECTIONBITMAP`）即为 redirection surface。

## 2. 先改脚本里写死的几何

脚本只在 2560×1600 @150% 上跑过，窗口位置与尺寸是物理像素常量。**屏幕小于约 2300×1500 时必须先调整**，否则窗口
超出屏幕、拖动范围越界或浮层贴边翻转，结果不可信。

| 脚本 | 写死的值 | 需要的屏幕 | 调整方法 |
|---|---|---|---|
| `resize-test.ps1` | 左边缘：`Place(700,150,1500,1100)`，拖动 `L-400 ~ L+250`；上边缘：`Place(500,480,1500,1000)`，拖动 `T-400 ~ T+250` | ≥ 2200×1480 | 1920×1080 时可改为左：`Place(450,60,1200,900)`；上：`Place(350,450,1200,600)`，拖动幅度改为 `-300 ~ +200` |
| `resize-test.ps1` | `FindResizeHandle` 只在可见边缘 ±8px 内探测 `HTLEFT`/`HTTOP` | — | Win10 的不可见 resize 边框可能更宽，报 `resize handle not found` 时把 `-8..8` 扩到 `-16..16` |
| `shadow-common.ps1` 的 `Start-Gallery` | 默认 `(250,80,2050,1400)` | ≥ 2300×1480 | 改默认值或调用处传参；Gallery 宽度太小会改变 InfoFlyout 页面布局（按钮换行） |
| `popup-shadow-layer2.ps1` | 主窗口 `(250,420,2050,1100)`，按钮滚到距主窗口上边缘 < 170px | 高 ≥ 1520 | 主窗口上方需留出约 300px 放靶子窗口和超出的浮层 |
| `popup-directions.ps1` | 滚动到分段控件 y < 480；中性点 = 分段控件右侧 600px | 高 ≥ 1200 | 目标是让 12 方向按钮组位于屏幕中部，上下左右各留 ≥ 300px |
| `popup-test.ps1` | 标题栏菜单坐标按 150% 写死偏移 | — | 缩放不同需改，或只跑 `native-popup-test.ps1` |
| 全部 | `ddagrab=output_idx=0`、`backdrop.ps1` 用主屏 | — | 在主显示器上测试；洋红背景没铺满全屏时检查进程 DPI 感知 |

先跑一次冒烟测试确认 UIA 导航、截图和窗口放置都正常：

```powershell
cd tmp\visual-verification\windows
.\native-popup-test.ps1 -Tag win10-smoke     # 输出浮层窗口矩形与 ex 样式，截图在 popups\
```

导航元素找不到时用 `.\uia-dump.ps1 -Filter InfoFlyout` 查看实际名称（Gallery 语言需为简体中文）。

## 3. 窗口 live resize

```powershell
.\resize-test.ps1 -Tag w10-left                       # 拖左边缘，统计右边缘
.\resize-test.ps1 -Tag w10-top -Edge top              # 拖上边缘，统计下边缘
.\resize-test.ps1 -Tag w10-fast -Period 0.6           # 两倍速度
.\blank-check.ps1 -Tags w10-left,w10-top,w10-fast     # 整窗白屏帧
```

输出在 `runs\`：`<Tag>.result.txt`、`<Tag>.scan.png`（每帧中线拼接，固定边缘应是直线）、`<Tag>.mkv`。

通过标准（Windows 11 基线）：`gapFrames=0`、`blankFrames=0`；`scan.png` 中固定边缘为直线，无黑/白条纹
（`gapFrames` 只统计洋红缺口，黑/白条纹要看 scan 图）。

再按 [live resize 文档](../../architecture/systems/windowing/windows-live-resize.md)“Windows 10 实机验证”手动走一遍：

1. 快速来回拖左边缘、上边缘，对向边缘固定。
2. resize 期间无黑框、透明条、原生标题栏按钮闪现。
3. 最大化、还原、全屏、焦点切换、退出全屏后装饰正常。
4. 缩到最小高度后继续 resize 无旧帧叠加、无错误窗口移动。

Windows 10 特有关注点：

- 暗色主题下非客户区边框走 Win10 专用路径（`SetWindowFrameDarkModeWindows`，build 17763 ~ 22000），切换主题后边框颜色应立即正确。
- Win10 没有 `DWMWA_VISIBLE_FRAME_BORDER_THICKNESS`，可见边框改用 `SM_CXBORDER`/`SM_CYBORDER`；注意 1px 边框是否缺失或错位。
- Win10 窗口为直角，没有 DWM 圆角，截图与 Win11 不同属正常。

## 4. 窗口型 Popup 阴影

按顺序执行。**第 1 步不通过就停下记录现象，不要继续**：第二层依赖 `WS_EX_LAYERED` 在该系统上保持逐像素透明。

### 4.1 前置：`WS_EX_LAYERED` + 透明（最关键）

```powershell
.\popup-shadow-anim.ps1 -Tag w10       # 输出 shadow\w10\：open/close.tile.png、steady.png、逐帧 open\ close\
.\layered-probe.ps1 -Tag w10-probe     # 外部对已打开浮层加 LAYERED 前后对比，popups\w10-probe.*.png
```

- 输出的 `ex=0x...` 应含 `0x80000`（`WS_EX_LAYERED`，挂接时设置）。
- `steady.png` 与 tile 中透明圆角、阴影无黑框/黑底，阴影是柔和渐变而不是实色。
- 打开/关闭每帧阴影与内容一起缩放淡入淡出，没有阴影滞后或残留。
- 浮层窗口外围不应出现系统矩形投影（`CS_DROPSHADOW`）。

不兼容时的退路：在 `src/AtomUI.Native/Windows/WindowsPopupShadowHitTestHook.cs` 构造函数中对 Win10 不设置
`WS_EX_LAYERED`/`SetLayeredWindowAttributes`，且不进入 `BeginPassThrough`（只保留第一层），并把现象记录到方案文档。

### 4.2 定位：12 方向 × 箭头三种模式

“箭头”示例默认是 overlay，需要本地临时改 Gallery（**不要提交**）：在
`controlgallery/AtomUIGallery/ShowCases/DataDisplay/InfoFlyout/Views/InfoFlyoutShowCase.axaml` 的“箭头”示例 `Grid.Styles` 中加

```xml
<Style Selector="atom|FlyoutHost">
    <Setter Property="ShouldUseOverlayPopup" Value="False" />
</Style>
```

构建后跑 `.\popup-directions.ps1 -Tag native`；把 Value 改成 `True` 再构建跑 `-Tag overlay`；最后 `git checkout` 该文件。
两组截图在 `shadow\dirs\native|overlay\`，用 ffmpeg PSNR 或 `shadow-common.ps1` 的 `Compare-Png` 逐张对比。

通过标准：36 组中 frame、阴影、箭头、margin 与 overlay 重合；只允许文字子像素栅格化差异（Win11 上 12 组完全一致，
其余差异只在文字字形上）。箭头尖与按钮的间距应为 `MarginToAnchor`（4 DIP）。

### 4.3 第一层（同进程）

```powershell
.\popup-shadow-layer1.ps1 -Tag w10
```

| 输出项 | 期望 |
|---|---|
| `A frames with popup visible AND 聚焦我 hovered` | > 0（Win11：18 / 96），`shadow\layer1\w10\hover.evidence.png` 中浮层可见且“聚焦我”为悬停边框 |
| `B hover popup closed after pointer moved onto its shadow` | True |
| `C click popup dismissed` | True |
| `C 聚焦我 received the click` | 默认 False（light dismiss 遮罩吞掉点击，与 overlay 一致）；给“点击我”的 `atom:Flyout` 本地加 `OverlayDismissEventPassThrough="True"` 后应为 True |

### 4.4 第二层（跨进程）

```powershell
.\popup-shadow-layer2.ps1 -Tag w10      # 不启动洋红背景；会额外起一个 WinForms 靶子进程
```

| 输出项 | 期望 |
|---|---|
| `popup ... exceeds main top by Npx` | N ≥ 60，否则调整主窗口位置/滚动 |
| `1 popup has WS_EX_TRANSPARENT on outside shadow` | True |
| `1 WindowFromPoint ... -> target process` | True |
| `1 target log` | 含 `ENTER` |
| `2 WS_EX_TRANSPARENT removed on content` / `2 target log` | True / 含 `LEAVE` |
| `2 click on content keeps popup open` | True |
| `3 inside main window: no WS_EX_TRANSPARENT` | True |
| `4 target log` | 含 `CLICK`（随后 Gallery 失活、浮层关闭属正常） |
| `5 content frames differing from first` | `0 / N`（切换样式不重绘、无闪烁） |

`shadow\layer2\w10\on-shadow.png` 中靶子按钮应为橙色（悬停），浮层阴影叠在靶子上方。

## 5. 结果回填

- 本文档末尾追加“Windows 10 结果”：环境（build、分辨率、缩放、显卡/驱动）、各步骤输出、截图路径、脚本改动。
- live resize：更新 [live resize 文档](../../architecture/systems/windowing/windows-live-resize.md) 实测表与“Windows 10 实机验证”状态。
- Popup：更新 [方案与实施结果](2026-10-29-window-popup-shadow-hit-test-plan.md)“未覆盖”一项，以及
  [Popup 实现原理](../../controls/desktop/other/popup/implementation.md) 第 11 节验证记录、
  [AtomUI.Native Windows](../../modules/native/windows.md) 6.3 兼容性说明。
- 发现问题时先保留 `runs\`、`shadow\`、`popups\` 下的录像与截图，再改代码。
