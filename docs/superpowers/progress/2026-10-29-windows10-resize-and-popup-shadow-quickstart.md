# Windows 10 实机验证快速上手：窗口 live resize 与窗口型 Popup 阴影

日期：2026-10-29。状态：Windows 10 已执行，结果见文末“Windows 10 结果”。live resize 与定位各有一项未达 Windows 11 基线，待决定是否修复。

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

## Windows 10 结果（2026-10-29）

环境：Windows 10 Pro build 19045，2560×1600 @150%（144 DPI），NVIDIA GeForce RTX 4060 Laptop（32.0.16.1074）+ Intel UHD
Graphics（32.0.101.7082），Avalonia 12.1.4，Gallery Release，不挂调试器。主窗口 `ex=0x40100`，不含
`WS_EX_NOREDIRECTIONBITMAP`，即默认 `RedirectionSurface`。

执行方式：Windows 11 的 PowerShell 脚本没有拷贝到这台机器，改用一个 C# 控制台工具按本文第 3、4 节的相同方法执行
（洋红背景、`WM_NCHITTEST` 探测 resize 起点、`ddagrab` 60fps 录制、UI Automation 定位、WinForms 跨进程靶子）。
源码保存在 `tmp/visual-verification/windows/vv/`，日志在 `vv.log`，输出在 `runs\`、`shadow\` 下。屏幕与 Win11 相同，
因此没有修改第 2 节里写死的几何参数。

### live resize

| Tag | 拖动边缘 | 帧数 | 缺口帧 | 白屏帧 | 黑条帧 |
|---|---|---|---|---|---|
| `w10-left` / `w10-left2` | 左 | 373 / 379 | 0 / 0 | 0 / 0 | 2 / 4 |
| `w10-fast`（2 倍速） | 左 | 386 | 0 | 0 | 3 |
| `w10-top` / `w10-top2` | 上 | 403 / 410 | 1 / 3 | 0 / 0 | 0 / 0 |
| `w10-fasttop`（2 倍速） | 上 | 407 | 4 | 0 | 0 |
| `base-left` / `base-fast`（旧顺序，`ex=0x240100`） | 左 | 399 / 422 | 143 / 177 | 0 / 2 | 0 / 0 |
| `base-top` / `base-fasttop`（旧顺序） | 上 | 406 / 430 | 151 / 170 | 0 / 1 | 0 / 0 |

- 与旧的合成顺序相比，`RedirectionSurface` 优先在 Win10 上同样有效：缺口帧从 35%～42% 降到 0～1%。
- **未达 Win11 基线（0 / 0）**：
  - 拖左边缘时，每组有 2～4 帧在固定的右边缘出现黑色竖条，宽约 20～60 物理像素，只持续 1 帧。
    这是窗口变宽时新露出的区域还没有渲染内容，scan 图中只是零星的单行。
  - 拖上边缘时，每组有 1～4 帧在固定的下边缘出现洋红缺口。大多只有几像素，`w10-fasttop` 第 297 帧约 280 物理像素。
    抽帧图在 `tmp/visual-verification/windows/stripe-left.png` 和 `gap-top.png`。
- 前几次 `w10-left` 的结果 `overshootFrames≈680` 是录制区域设置错误，修正工具后重测，不计入上表。
- 装饰（`runs\decor-w10fs\`）：正常、最大化、还原、失去焦点（Notepad 置前）、重新激活、全屏、退出全屏后标题栏与边框都正常；
  缩到最小高度 166px 后继续拖动，窗口上边缘不移动，也没有旧帧叠加。
- 没有执行：暗色主题下 Win10 边框颜色、`SM_CXBORDER` 1px 边框逐像素检查、`popup-test.ps1`（overlay 标题栏菜单）。

### 窗口型 Popup 阴影

| 步骤 | 结果 |
|---|---|
| 4.1 `WS_EX_LAYERED` + 动效 | 浮层 `ex=0x80080`（含 `WS_EX_LAYERED`，不含 `NOREDIRECTIONBITMAP`）；261 帧中浮层矩形内纯黑像素最多 0；`shadow\w10\steady.png` 圆角透明、阴影柔和，没有系统矩形投影；打开动效逐帧阴影与内容同帧。`layered-probe` 未单独执行（挂接时已设置 LAYERED，由动效录制覆盖） |
| 4.2 定位 36 组 | 横向全部一致。6 组（显示/居中 × 下方、下左、下右）逐像素一致；**其余 30 组窗口型整体比 overlay 高 1 物理像素（150% 下约 0.67 DIP）**，上下边缘与文字一起错 1 行，所以出现的“文字差异”实际是平移，不是栅格化差异。差异图见 `shadow\dirs\mask-*.png` |
| 4.3 第一层 | B、C 通过；C 默认 `聚焦我` 没有收到点击，加 `OverlayDismissEventPassThrough=True` 后收到点击。A：3 次运行分别为 0/48、7/47、6/47（Win11 18/96）。第一次为 0，原因未查明 |
| 4.4 第二层 | 浮层超出主窗口 119px：`WS_EX_TRANSPARENT` 已设置，该点 `WindowFromPoint` 命中靶子进程；靶子日志为 `ENTER LEAVE CLICK ENTER`；回到内容后样式移除，点击内容浮层不关闭；主窗口内的阴影不启用第二层；内容区 278 帧中 0 帧变化。第一次运行只超出 57px，取到的阴影测试点仍在主窗口内，属于工具问题，不是产品失败 |

结论：`WS_EX_LAYERED` 在 Win10 + `RedirectionSurface` 下兼容，第一层和第二层行为与 Win11 一致，不需要启用第 4.1 节的退路。

### 待决定

1. **Win10 live resize 残余单帧缺陷**：拖左边缘时右边缘有黑条，拖上边缘时下边缘有缺口，频率低于 1%。是否需要继续排查。
2. **窗口型 Popup 纵向差 1 物理像素**：只出现在部分方向，横向没有差异。推测是 native popup 的 DIP→物理像素换算
   （截断）与 overlay 的布局舍入（四舍五入）不一致，**还没有查源码确认**：本地没有 Avalonia 参考源码，也没有去 GitHub 核对。
   Win11 记录的“仅文字子像素差异”也应改用边缘采样复查。
