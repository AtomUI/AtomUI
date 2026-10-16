# AtomUI Performance Tools

本目录存放 AtomUI 性能测量和验证工具。工具分为两类，数据口径不能混用。

## 工具分类

| 工具 | 类型 | 用途 | 数据口径 |
| --- | --- | --- | --- |
| [AtomUI.Performance](AtomUI.Performance/AtomUI.Performance.csproj) | 控件级基准 | 批量创建单个控件或小组合，观测实例化、布局、visual tree、分配和专项行为验证 | 微基准，不代表 Gallery 页面打开体验 |
| [AtomUI.Localization.Performance](AtomUI.Localization.Performance/AtomUI.Localization.Performance.csproj) | Localization 基准 | 测量 Registry/Snapshot 构建、`ILocalizer.Get`、语言切换和 Snapshot 内存增长 | 纯 Localization 运行时基线，不加载桌面控件主题 |
| [AtomUI.GalleryPerformance](AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj) | Gallery 场景复现 | 启动 Gallery Workspace，走真实 route/view/XAML，测量 showcase 从导航触发到视觉树和布局稳定 | 体验路径基准，必须复现 Gallery 真实使用方式 |
| [AtomUI.StartupPerformance](AtomUI.StartupPerformance/README.md) | 启动基准 | 冷启动进程：平台初始化、AtomUI 初始化、窗口首帧与渲染批次、UI 线程最长阻塞 | 独立进程交错采样；单次运行只作 smoke |

## AtomUI.Performance 结构

`AtomUI.Performance` 按职责和控件体系拆分：

- `Program.cs`: 入口、参数解析后的调度，不放具体控件创建和验证逻辑。
- `Core/`: runner、结果模型、树统计、Markdown/table 渲染。
- `Suites/AddOnDecoratedBox/`: AddOnDecoratedBox、LineEdit、TextArea、SearchEdit、CompactSpace 相关场景和专项验证。
- `Suites/Button/`: Button、DropdownButton、SplitButton 相关场景和状态/模板验证。
- `Suites/ButtonSpinner/`: ButtonSpinner、Gallery shape batch、NumericUpDown 联动基线。
- `Suites/Drawer/`: Drawer 关闭态、extra/footer、嵌套场景基线。
- `Suites/GroupBox/`: GroupBox header icon lazy、Gallery shape 和状态/生命周期验证。
- `Suites/Icon/`: Icon micro benchmark、隐藏 icon slot、AntDesign metadata、provider cache 验证。
- `Suites/ImagePreviewer/`: ImagePreviewer 关闭态 source list lazy、Gallery shape 和状态/生命周期验证。
- `Suites/NavMenu/`: NavMenu/NavMenuItem 默认路径、全局关闭订阅、container binding 生命周期验证。
- `Suites/ScrollViewer/`: ScrollViewer/ScrollBar 模板、overlay host、lite/auto-hide 和 motion 状态验证。
- `TestSupport/`: 断言、测试 brush、marker template、probe icon 等测试辅助类型。

新增控件级基准时优先在 `Suites/<ControlName>/` 下建文件；跨控件复用能力放到 `Core/` 或 `TestSupport/`。

## 使用原则

- 控件优化先用 `AtomUI.Performance` 建立低噪声控件级基线，定位单实例和批量实例化成本。
- Gallery 体验结论必须用 `AtomUI.GalleryPerformance` 或同类 Gallery 场景工具确认。
- Gallery 场景工具必须加载真实 Gallery XAML，不能用合成控件替代真实 showcase。
- Gallery 场景工具需要在结果里输出源 XAML 形态和运行时视觉树形态，用来证明测量对象一致。
- `Cold first navigation` 必须用 `--cold-iterations <N>` 做独立进程多样本统计；单样本只用于 smoke，不能作为优化提升或回退结论。
- 启动结论必须用 `AtomUI.StartupPerformance` 的独立进程交错采样（`run_samples.py`，每变体至少 10 轮）；单次运行只作 smoke。采样期间不得并行构建或测试。
- 启动期的 UI 卡顿不得用 `DispatcherTimer` 测量：它由平台 runloop 服务，主循环开始前在 macOS 上几乎不触发，会得到恒为 0 的假结果。用 `AtomUI.StartupPerformance` 的后台心跳探针（`Mark.FirstUiPump` / `Mark.MaxUiGap`）。
- 文档中必须说明触发点、是否包含鼠标事件、是否包含 GPU 上屏、是否是 headless。

## 当前命令

控件级 AddOnDecoratedBox/LineEdit 基准：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --count 60 \
  --markdown /tmp/atomui-addon-control.md
```

控件级 Icon 基准：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite icon --count 60 \
  --markdown /tmp/icon-micro-baseline.md
```

控件级 Button 基准：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite button --count 60 \
  --markdown /tmp/atomui-button-control.md
```

Icon / AddOn 专项行为验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-accessories --verify-effective-brushes --verify-addon-states \
  --verify-antdesign-metadata --verify-icon-hidden-slots --verify-icon-provider-cache
```

Button 状态和模板验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-button-states
```

ButtonSpinner 状态和生命周期验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-buttonspinner-states
```

控件级 ButtonSpinner 基准：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite buttonspinner --count 60 \
  --markdown /tmp/atomui-buttonspinner-control.md
```

控件级 Select 基准与 handle 输入状态传播测量：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite select --count 60 \
  --markdown /tmp/atomui-select-control.md

dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --measure-select-interactions --count 1000 \
  --markdown /tmp/select-interactions-current.md
```

控件级 Breadcrumb 基准与状态验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite breadcrumb --count 60 \
  --markdown /tmp/atomui-breadcrumb-control.md

dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-breadcrumb-states
```

控件级 NavMenu 基准与状态验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite navmenu --count 80 \
  --markdown /tmp/atomui-navmenu-control.md

dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-navmenu-states
```

控件级 Drawer 基准：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite drawer --count 60 \
  --markdown /tmp/drawer-control-baseline.md
```

控件级 ImagePreviewer 基准：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite imagepreviewer --count 60 \
  --markdown /tmp/atomui-imagepreviewer-control.md
```

ImagePreviewer 状态和生命周期验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-imagepreviewer-states
```

控件级 ScrollViewer 基准与状态验证：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite scrollviewer --count 220 \
  --markdown /tmp/atomui-scrollviewer-control.md

dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-scrollviewer-states
```

Localization 运行时基线与状态验证：

```bash
dotnet run --project tools/performances/AtomUI.Localization.Performance/AtomUI.Localization.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite localization --count 10 \
  --verify-localization-states \
  --markdown /tmp/atomui-localization-baseline.md
```

该工具直接使用编译后的 Catalog descriptor 和字符串表，不初始化控件主题，也不解析 XLIFF。指标口径如下：

- `Snapshot startup build`: 使用预先构造的 descriptor/翻译表输入，构建 Registry 和所有支持语言 Snapshot 的耗时与当前线程分配。
- `ILocalizer.Get`: 预热后的强类型查询吞吐与分配；`--count` 会换算为至少 1,000 次调用。
- `Language switch commit`: 已预构建 Snapshot 之间切换的提交耗时、分配、资源通知和语言事件数量。
- `Snapshot memory`: 单轴 Catalog、unit、language 扫描及组合规模下的保留内存粗测、逻辑文本字节和 Snapshot slot 数量。`Retained KiB` 是 16 个同时存活 runtime 的平均值，仍受 GC 与进程噪声影响；线性规模判断以 `Logical text KiB` 和 `Snapshot slots` 为稳定信号。
- NuGet 包和 NativeAOT 发布体积必须从真实 `pack`/`publish` 产物记录，不使用进程内微基准估算。

Gallery 真实 `LineEditShowCase` 导航基准：

```bash
dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase lineedit --label optimized --cold-iterations 10 --iterations 30 --warmup 5 --timeout-ms 30000 \
  --markdown /tmp/lineedit-showcase-navigation-optimized.md
```

Gallery 真实 `IconShowCase` 导航基准：

```bash
dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase icon --label icon-baseline --cold-iterations 10 \
  --iterations 10 --warmup 5 --timeout-ms 30000 \
  --markdown /tmp/icon-showcase-navigation-baseline.md
```

Gallery 真实 `ImagePreviewerShowCase` 导航基准：

```bash
dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase imagepreviewer --label imagepreviewer \
  --cold-iterations 10 --iterations 40 --warmup 6 --timeout-ms 45000 \
  --markdown /tmp/atomui-imagepreviewer-gallery.md
```

Gallery 工具支持多个真实 ShowCase，常用例子包括 `lineedit`、`icon`、`button`、`space`、`select`、`cascader`、`drawer`、`groupbox`、`imagepreviewer`、`menu`、`breadcrumb`。`menu` 的 ready 条件包含 `NavMenu` 默认路径完整展开，避免把未完全显示的 `791 visuals` 形态误判为完成。例如验证 Icon Phase 4 影响到的控件真实场景：

```bash
dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase select --label phase4-icon-slots \
  --iterations 10 --warmup 2 --timeout-ms 30000 \
  --markdown /tmp/select-showcase-navigation-phase4.md
```

## 扩展建议

- 新控件先补控件级 scenario，再补对应 Gallery showcase scenario。
- 多个控件共享的基础能力，例如 addon、popup、selector、虚拟化，应优先在控件级工具里做小样本和批量样本对照。
- 对用户可感知的打开、切换、弹出、滚动路径，应在 Gallery 工具里用真实页面复现。
