# Icon 创建成本优化

日期：2026-11-14。分支 `perf/control-performance-audit`。macOS arm64（Apple M1 Max），net10.0 Release，Avalonia 12.1.6，Headless。
前置评估见 [2026-11-14-control-performance-audit.md](2026-11-14-control-performance-audit.md)。

## 资格

Icon 是被实例化最多的基元：Button 页 66 个隐藏的加载图标加 48 个可见图标，Icon 页约 100 个，DropdownButton、TreeView、
Pagination 等页面都有几十个。每次进入页面都会重新创建，满足“实例数大于 5、操作频率每会话多于 1 次”的门槛。

## 改动

| 改动 | 文件 | 结构变化 |
| --- | --- | --- |
| 去掉 Icon 的控件模板 | `IconTheme.axaml` | 模板里唯一的 `Border` 只复刻了 `Icon.Render` 已绘制的背景、`Width`/`Height` 已给出的尺寸和 `ICustomHitTest` 已覆盖的命中；每个可见 Icon 从 2 个 visual 变为 1 个 |
| 在 `Render` 中复现原模板 `Border` 的背景输出 | `Icon.cs` `Render` | 图形之后按自身矩形再填一次 `Background`；完全透明的背景跳过，输出不变 |
| 加载动画只在 Icon 自身可见时跟踪祖先 | `Icon.cs` | 隐藏的加载图标不再向祖先链逐层订阅 `PropertyChanged`；自身 `IsVisible` 由 `OnPropertyChanged` 处理；去掉 attach 时那次随后就被 `Loaded` 覆盖的重复订阅 |
| 未启动过旋转的 Icon 不做停止复位 | `Icon.cs` | 静态 Icon 在加载和 detach 时不再写 Composition `RotationAngle` |

新增两个 internal 只读探针 `IsLoadingAnimationRunning`、`IsTrackingEffectiveVisibility` 供测试使用，没有新增公共 API。

### 未改动：DrawPens

`DrawPens` 是只写的：仓库内没有任何读取方，渲染由 `DrawingInstruction.BuildPen` 每次自建 `Pen`。删掉它能让每个 Icon 再少约 8.5 KB、
少约 4 µs（实验数据见下文）。但 [implementation.md](../../controls/desktop/general/icon/implementation.md) 把它定为 protected 派生契约，
要求改动创建时机或形态前独立评审兼容性，所以本次保持原样，作为待决事项。

## 视觉与行为

- **视觉零差异**：25 个场景（各主题图标、自定义画刷与尺寸、Margin、不透明 / 半透明 / 渐变 / 显式透明 / 带 Opacity 的 `Background`、
  Canvas 偏移放置、带图标 Button、加载中 Button、IconButton、IconPresenter）新旧版本逐像素比较，除加载中图标外差异像素为 0，布局完全一致。
  加载中图标的 13 个像素差异来自旋转动画的截帧时刻：同一构建连续三次截图之间也有 5 到 11 个像素差异。
- **指针事件 Source**：命中目标从 Icon 内部的 `Border` 变为 Icon 本身。命中范围不变，事件仍沿原路由冒泡，不影响视觉。
- **保留的既有问题（未修）**：非透明 `Background` 会盖住图形，`Render` 里的第一次填充还画在父坐标偏移处。这是改动前就有的输出，
  性能改动保持原样；是否修正需要作为独立的视觉变更决定。

## 验证

- 新增测试 13 个：加载动画随自身可见、祖先可见、隐藏祖先下显示、动画类型切换、detach 和重新挂载的启停 6 个，
  隐藏和静态 Icon 不跟踪祖先 2 个，无模板尺寸、Token 尺寸、命中 3 个，背景渲染 2 个（渲染测试项目，在新旧代码上都通过，锁定输出不变）。
  6 个行为测试和静态 Icon 测试先在旧代码上通过，再改代码；“隐藏 Icon 不跟踪祖先”在旧代码上失败，是本次要建立的性能契约。
- 回归：Desktop.Controls、Rendering、Gallery、GalleryBase、Controls、Core、DataGrid、Icons.Shared 八个项目共 5962 个测试全部通过。
- `AtomUI.Performance` 的 `--verify-icon-hidden-slots`、`--verify-icon-provider-cache`、`--verify-button-states` 在新旧代码上给出完全相同的
  49 条断言失败，`--verify-antdesign-metadata` 在新旧代码上同样失败。这些验证器断言的是已回滚的按需创建架构，已过期，不是本次引入。
  另外该工具自 `3a1008edd` 起编译失败（`SpaceItemsAlignment` 移到 `AtomUI.Controls` 命名空间），本次补了 using。

## 测量

前后两个 Release 构建放在不同目录，交替运行，避免机器负载漂移只影响一侧。

### 控件级（AtomUI.ControlBaseline，每类型 300 实例，4 轮交替，取每轮最小值的中位数）

| 控件 | 前 µs | 后 µs | 变化 | 前 KB | 后 KB | 变化 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| SearchOutlined | 27.6 | 21.1 | −24% | 38.5 | 29.9 | −22% |
| LoadingOutlined | 27.4 | 19.6 | −29% | 38.6 | 29.8 | −23% |
| Result | 211.0 | 199.2 | −6% | 143.7 | 134.5 | −6% |
| Button | 375.8 | 419.8 | 见下 | 211.1 | 193.6 | −8% |

Button 的计时在默认 GC 下变慢约 10%，原因已定位到测量环境：该基准同时保留 300 个 Button（约 60 MB 存活对象），
优化后每个样本的 GC 次数从 10.6 降到 9.6，但单次回收的压缩成本上升（采样中 `relocate_survivors` 从 2.5% 升到 13.6%），
而产品代码路径明显变少（`Icon.OnLoaded` 6.5% → 0，`CompositeDisposable.Dispose` 5.7% → 0，委托移除 5.3% → 0）。
把 gen0 预算调大、让计时区间内不发生 GC 时，两轮分别为 203 → 215 µs 和 214 → 188 µs，差异在噪声内。
真实 Gallery 页面上 Button 是变快的（见下）。

DrawPens 实验（未合入）：在本次改动之上再去掉 Pen，SearchOutlined 26.9 → 18.2 KB、22.9 → 8.4 µs 中，Pen 约占 8.5 KB 和 4 µs；
单独去掉 Pen 时 Icon 为 38.5 → 26.9 KB（−30%）、26.5 → 22.9 µs（−14%）。

### Gallery 页面（`--audit`，16 个图标密集页，4 轮交替，每轮 12 次稳态导航）

| 页面 | Visuals | 稳态最小 ms | 稳态中位 ms | 每次分配 MB | 更快轮数 |
| --- | --- | --- | --- | --- | ---: |
| dropdownbutton | 610 → 575 | 30.0 → 21.2（−29%） | 31.6 → 24.6（−22%） | 12.6 → 11.3（−10%） | 4/4 |
| treeview | 1997 → 1937 | 92.9 → 85.9（−8%） | 100.6 → 91.3（−9%） | 31.8 → 30.7（−3%） | 4/4 |
| lineedit | 1788 → 1734 | 74.3 → 70.2（−6%） | 82.1 → 74.1（−10%） | 27.7 → 27.1（−2%） | 2/4 |
| button | 1449 → 1389 | 74.5 → 71.2（−4%） | 123.6 → 105.9（−14%） | 28.4 → 23.8（−16%） | 2/4 |
| pagination | 1019 → 979 | 36.8 → 35.3（−4%） | 38.8 → 37.1（−4%） | 17.0 → 16.6（−3%） | 3/4 |
| 16 页合计 | 18956 → 18242（−3.8%） | 757.3 → 736.9（−2.7%） | | 304.1 → 290.8（−4.4%） | |

dropdownbutton 的大幅下降来自分配量跨过 GC 门槛：每次导航的 gen0 回收从 2 次降到 1 次。

前一轮里 tag、select、alert、result、avatar 看上去慢了 2% 到 5%。用 8 轮交替复测：select、alert、result、avatar 为 −2% 到 −8%
或持平，tag 为 +7.6%。再用模板改动、跟踪改动分开构建的四个变体跑 6 轮，tag 四个变体每轮中位数的中位数为
62.16 / 62.20 / 62.54 / 62.55 ms，优化版在 6 轮中赢 3 轮。tag 页 visual 少 35 个、分配少 2%，找不到增加工作的机制，
结论是与噪声无法区分，没有证据证明为零。

### 冷启动首次导航（标准导航口径，12 个独立进程）

| 页面 | 指标 | 前 | 后 | 变化 |
| --- | --- | ---: | ---: | ---: |
| button | 冷启动均值 / 中位 / P95 | 566.7 / 566.0 / 583.6 ms | 535.3 / 534.1 / 561.8 ms | −5.5% / −5.6% / −3.7% |
| button | 稳态均值 / 中位 / P95（20 次） | 127.1 / 120.3 / 148.6 ms | 109.1 / 106.3 / 125.6 ms | −14% / −12% / −15% |
| button | 冷启动分配 | 31.5 MB | 26.8 MB | −15% |
| icon | 冷启动均值 / 中位 | 446.6 / 442.6 ms | 422.3 / 419.2 ms | −5.4% / −5.3% |

icon 页“前”这一组稳态样本呈双峰（66 到 235 ms），不作结论；该页以交替审计的 −1% 为准。

## 复杂度

- 新增方法 0 个；新增 try/finally 标志 0 个；新增 disposable 字段 0 个；新增状态 1 个（`IsLoadingAnimationRunning`，只在启动和停止两处赋值）。
- AXAML 删除 8 行；C# 增 36 行、删 23 行（多数为注释与测试探针）。
- Theme Static Rule：没有把 AXAML 节点搬进 C#，删除的节点没有独立职责。

## 回退

```bash
git checkout release/6.0 -- src/AtomUI.Core/Controls/Icon/Icon.cs src/AtomUI.Controls/Icon/Themes/IconTheme.axaml
```
