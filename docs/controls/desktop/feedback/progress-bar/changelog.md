# ProgressBar Changelog

本文档记录 ProgressBar 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 ProgressBar 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。

## 2026-11-21

- 行高：水平进度条（百分比在条外）的最小高度改取 "100%" 文字高度与状态图标尺寸中的较大者，显示百分比和显示状态图标的行等高，堆叠的进度条间距一致（antd 两者同一行盒）。条内百分比与自定义信息内容不变。
- PrimeNG：默认（Large）进度条高度取 PrimeUIX `progressbar.height`（1.125rem = 18，Lara 1.25rem = 20，primeng.dev 实测 18），Middle / Small 为 12 / 8（Lara 14 / 10）。此前 18 放在 Middle，默认进度条为 24。
- Gallery：Mini 示例改为间距 10 的竖排（原 WrapPanel 无间距），Semantic Part 样式示例间距 0 → 10。

## 2026-11-19

- 普通和分段进度条的主题将状态图标尺寸归并到一个内部属性，供布局测量与图标模板共同使用。

## 2026-11-18

- Geometry
  - CircleProgress 和 DashboardProgress 在非方形布局中以短边确定轨道直径，轨道、进度弧和成功弧共用居中的正圆视口。

- Theme
  - 线厚度阶梯由控件专属 Token `LineStrokeThicknessLG`、`LineStrokeThickness` 和 `LineStrokeThicknessSM` 提供，默认 8/6/4。
    线宽与布局使用的字号档位由同一个挂树期资源读取组提供。实际厚度变化时触发重新测量，模板仍设置显示字号。
  - 完成和异常图标的绘制与信息区测量使用同一档位。Large 使用 `LineInfoIconSize`，Middle 和 Small 使用 `LineInfoIconSizeSM`。
- Shadcn
  - `ShadcnProgressBarTokens` 控件阶段：指示条 `--primary`、轨道 `--muted`（此前 antd `ColorInfo` 蓝在所有中性风格上错色）；
    高度/圆角按附录 C 每风格单值（Nova 4/Vega 6/Maia 12·r26/Lyra 4·r0/Mira 4·r8/Luma 12/Sera 2·r0/Rhea 8·r18），
    单值风格三档同值（与 Luma 按钮圆角阶梯 26/26/26 同一表达方式）。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-11-15

- Token
  - `ProgressBarToken` 新增 `InnerLabelColor`（默认 `ColorTextLabel`），替代主题直接读取的 `ColorTextLabel` 作为内嵌百分比文本的候选色；默认值相同，Ant Design 渲染不变。PrimeNG 预设经控件算法写入进度色（`DefaultColor` = 主色、`RemainingColor` = PrimeUIX 轨道色）、`LineBorderRadius` 与 `InnerLabelColor`。

## 2026-11-14

- Performance
  - Line and circle progress part setters bind to the progress control with `{TemplateBinding}` instead of `{Binding …, RelativeSource={RelativeSource TemplatedParent}}`. The sources are Avalonia properties of the same type and the targets are one-way, so values and setter priority are unchanged.

## 2026-11-11

- Fix
  - 在已挂树的进度条上切换模板时，旧信息呈现器先释放 `ProgressInfoContent`，新呈现器再接管；控件实例作为内容时不再因仍有旧视觉父级而抛出 `InvalidOperationException`。
  - 自定义信息内容存在时，百分比包装节点与状态图标改用可释放的 Animation 优先级隐藏，清除内容或换模板时释放；不再写入或清除它们的本地 `IsVisible`，自定义模板上已有的本地值与绑定在内容清除后恢复。共享层仍由代码负责该隐藏，不依赖主题样式。
  - 信息内容在设置后改变尺寸（控件改宽、模板内文本变长、只换 `ProgressInfoContentTemplate`）时，线形与 Steps 的信息区和轨道边界、Circle/Dashboard 的居中位置随之更新。内置模板改用 internal `ProgressInfoPresenter`（样式键仍为 `ContentPresenter`），在期望尺寸变化时使宿主重新测量与排布；线形与 Steps 测量时按呈现器当前期望尺寸刷新信息区。

## 2026-11-07

- API
  - `AbstractProgressBar` 新增可选的 `ProgressInfoContent`（`object?`）与 `ProgressInfoContentTemplate`（`IDataTemplate?`），默认均为 `null`。非空时信息区总是显示（不受 `IsProgressInfoVisible` 约束），百分比标签与两个状态图标隐藏，信息区测量取内容呈现器的期望尺寸；清除后恢复百分比文本与状态图标。
  - 新增模板部件常量 `ProgressInfoContentPresenterPart`（`PART_ProgressInfoContentPresenter`，`ContentPresenter`）与 internal `IsProgressInfoPresented`（`IsProgressInfoVisible` 为真或存在自定义内容），供模板绑定信息区的显示。
  - Steps 与 Dashboard 通过抽象继承同样的行为：布局判断改用 `IsProgressInfoPresented`，Steps 的信息区测量同样取内容呈现器的期望尺寸。
- Fix
  - `AbstractLineProgress` 与 `AbstractGeneralProgressBar` 的 `CalculateExtraInfoSize` 以 `Percentage` 计算文本与完成状态，不再使用 `Value`/`Maximum`；`Minimum` 不为 0 或 `Maximum` 不为 100 时，信息区尺寸与显示的百分比文本一致。
  - `AbstractCircleProgress` 的自动字号以 `BindingPriority.Template` 写入：主题样式值让位于它，调用方设置的本地 `FontSize` 不再被改尺寸或切换大小类型的流程覆盖。
  - `AbstractProgressBar` 的 `IsProgressInfoPresented` 在构造时按实际默认值（`IsProgressInfoVisible` 的默认值或 `ProgressInfoContent`）起算；覆盖了 `IsProgressInfoVisible` 默认值的子类不再在第一次变化前误报“已呈现”并预留信息区空间。
  - `AbstractGeneralProgressBar` 新增 `protected InvalidateExtraInfoSize()`：重新计算信息区尺寸与最小厚度并使测量失效；`ProgressTextFormat` 与 `FontSize` 变化时自动调用，子类自定义尺寸规则的输入变化时也可调用。
  - `AbstractProgressBar` 在 `IsProgressInfoVisible` 或 `Status` 变化时也刷新进度（`UpdateProgress`）：信息区在运行中从隐藏改为显示后，百分比文本立即填充，圆形重新判断文本能否放进圆内；从 `Exception` 状态切回其他状态后，文本不再停留在旧值。
- Theme
  - Desktop 的线形、环形、仪表盘与 Steps 模板在信息区 `Canvas` 中加入 `PART_ProgressInfoContentPresenter`，`Canvas` 的显示改为绑定 `IsProgressInfoPresented`。
- Desktop 影响
  - 行为修复：运行中把 `IsProgressInfoVisible` 由 `false` 改为 `true` 后，信息区文本随即显示（此前为空，直到下一次 `Value`、范围或格式变化）；从 `Exception` 切回其他状态后文本即时更新。初始即显示或不切换这两个属性的场景没有差别。
  - 默认行为不变：`ProgressInfoContent` 默认为 `null`，`IsProgressInfoPresented` 在默认情况下等于 `IsProgressInfoVisible`；默认 `Minimum=0`、`Maximum=100` 时 `Percentage` 等于 `Value`，信息区尺寸不变。
  - 行为修复：自定义 `Minimum`/`Maximum` 时信息区尺寸由实际百分比决定；对圆形进度本地设置 `FontSize` 后不再被覆盖。
  - 行为修复：线形进度（`ProgressBar`）的 `ProgressTextFormat` 或 `FontSize` 变化后，信息区尺寸与最小厚度随之重新计算（此前保持首次模板应用时的尺寸）。默认格式与字号不变时没有差别。
  - 新增成员均为可选，源码与二进制兼容。
  - 视觉对照：无头 Skia 渲染改动前后的 Desktop `ProgressBar`（线形的 Normal/Success/Exception/Active × 0/30/100、隐藏信息区、小尺寸、成功阈值、自定义文本格式）、`StepsProgressBar`、`CircleProgress`（含分段、成功阈值、小尺寸与大尺寸）、`DashboardProgress`（含分段），浅色 1x/2x 与深色 1x 三组截图逐字节一致，`IsProgressInfoPresented`、信息区呈现器、`IndicatorRatio` 与环形几何缓存、删除重复绑定对 Desktop 默认渲染无可见影响。
- Tests
  - `ProgressInfoTests` 增加：运行中打开信息区后文本填充（线形）与圆形重新判断容纳；`Exception` 切回 `Normal` 后文本刷新。
  - `ProgressInfoTests` 增加：覆盖默认隐藏的子类不呈现信息区且不预留空间；`ProgressTextFormat` 与 `FontSize` 变化后信息区尺寸与最小宽度更新。
  - 新增 `ProgressInfoTests`（共享层）：自定义内容替代文本与状态图标、信息区隐藏时仍显示、清除后恢复文本、信息区尺寸取百分比、本地字号在圆形改尺寸后保持、无本地值时自动字号仍生效、Desktop 默认值不变。
  - 新增 `ProgressInfoContentTests`（Desktop）：默认主题下线形、环形、仪表盘、Steps 显示与清除自定义内容，信息区隐藏时仍显示。

### 减重

- API
  - `AbstractLineProgress` 新增 internal `IndicatorRatio`（0–1，`AffectsRender`，不影响测量），`Percentage` 变化时写入 `Percentage / 100`。自绘路径的填充与 `LineProgressPanel` 的填充部件都按它计算；比例单独变化时 `LineProgressPanel` 在上次排布的槽内直接重排填充部件，面板与宿主的测量、排布保持有效；填充不足最小可见长度时排成空矩形而不切换 `IsVisible`，从 0 起步的过渡同样零布局。成功段仍按 `SuccessThreshold` 计算。
  - `AbstractCircleProgress` 的 `IndicatorAngle` 声明 `AffectsRender`，自绘路径过渡它时逐帧重绘。
  - `AbstractCircleProgress` 新增 internal `RailGeometryBuildCount`（轨道几何累计重建次数）与 `FullTextMeasureCount`（“100%” 文本累计测量次数），供测试与基准观测。
- 实现
  - 环形轨道几何按（槽矩形、`StepCount`、`StepGap`、描边粗细、端点样式）缓存，只在键变化或重新应用模板时重建；成功弧在此之上再按阈值、范围与已填充段数缓存，分段模式的填充按已填充段数缓存。仪表盘的缺口角度或缺口位置变化时主动作废这些缓存。
  - 模板路径与自绘路径共用这些缓存：自绘路径的轨道、填充与成功弧改为 `DrawGeometry` 绘制缓存的几何，不再每帧为每一段新建圆弧。
  - `IndicatorAngle` 单独变化只重建填充弧，轨道与成功弧保持原几何对象。
  - 环形判断信息区能否容纳满值文本时，“100%” 文本尺寸按（`ProgressTextFormat`、`FontSize`、`FontFamily`）缓存。
- Theme
  - `CircleProgressTheme` 的三条 `Path` 删除 6 个与 `AbstractCircleProgressTheme` 重复的模板绑定（`Stroke`、`StrokeLineCap` 各 3 个），改由基础主题的样式绑定提供，取值相同。
  - 三个 `StrokeThickness` 模板绑定保留：基础主题里对应的样式绑定指向 `protected` 的 `StrokeThickness`，实际取不到值，删除后描边粗细为 0。
- Desktop 影响
  - Desktop 主题仍过渡 `Value`：标签逐帧计数，`IndicatorRatio` 每帧随 `Percentage` 更新，默认渲染结果不变。
  - 环形 `Path` 的 `Stroke`、`StrokeLineCap` 由模板优先级改为样式优先级；默认主题下取值不变，不带激活条件的自定义样式现在可以覆盖这两个属性。
  - 线形填充不足最小可见长度时保持 `IsVisible` 为真、排成零尺寸，绘制结果与隐藏相同；成功段的显示规则不变。
  - 自绘路径（只在自定义模板不含路径部件时使用）的成功弧改为按 `Minimum`–`Maximum` 夹取阈值后叠画在填充之上，与模板路径一致；阈值在范围内时绘制结果不变。
  - 新增成员均为 internal，源码与二进制兼容。
- 基准（无头平台，18 帧约 0.3 s；环形扫描为 `Value` 从 1 逐一设到 100，关闭动效）

  | 指标 | 场景 | 改前 | 改后 |
  | --- | --- | --- | --- |
  | 布局轮次 | 线形，Desktop 主题过渡 `Value` 20→80 | 36 | 36 |
  | 布局轮次 | 线形，过渡 `IndicatorRatio` 0.2→0.8（`Value` 写入那一轮之后） | 无此入口 | 0 |
  | 布局轮次 | 线形圆头，过渡 `IndicatorRatio` 0→0.8（`Value` 写入那一轮之后） | 无此入口 | 0 |
  | 布局轮次 | 环形，Desktop 主题过渡 `Value` 20→80 | 36 | 36 |
  | 布局轮次 | 环形模板路径，过渡 `IndicatorAngle`（`Path.Data` 变化使路径重新测量） | 未测 | 33 |
  | 布局轮次 | 环形自绘路径，过渡 `IndicatorAngle` | 未声明 `AffectsRender`，过渡帧不重绘 | 0 |
  | 轨道几何重建 | 环形扫描 | 300 | 0 |
  | 轨道几何重建 | 环形自绘路径、6 段，`IndicatorAngle` 从 0 到 360 每 20° 一帧 | 每帧 6 段圆弧 | 0 |
  | “100%” 文本测量 | 环形扫描 | 100 | 0 |

- Tests
  - 新增 `ProgressRenderCostTests`（共享层）：比例跟随百分比、过渡比例期间零布局且填充宽度跟随比例、圆头从 0 起步过渡零布局、轨道只在尺寸或分段变化时重建、自绘路径分段模式扫描角度时轨道不重建、角度变化只重建填充弧、满值文本尺寸缓存、过渡 `Value` 时标签计数且比例逐帧跟随。

## 2026-08-15

- Fix
  - Replace transparent line/circle/dashboard Semantic proxies with the actual visible template renderers, retaining owner rendering only as a legacy custom-template fallback.
  - Replace line rail, track, and success Shapes with `Border` box visuals, and make `LineProgressPanel` their sole layout owner so Bounds are applied once without owner-side child layout or `Shape.Stretch` transforms.
  - Make `StepsProgressPanel` the sole layout owner for runtime tracks and indicator so deferred Examples render Start/Center/End positions correctly on first attachment without a Semantic Parts Tab round trip.
  - Restore visible Steps percentage text by inheriting the owner `Foreground` instead of binding the label to the line-only `PercentageLabelColor` state.
  - Align Semantic highlight Bounds with the Ant Design structure: rail covers the full groove, track covers the completed geometry, and indicator covers the percentage or status information region instead of the whole control.
  - Add `CircleProgressPanel` as the sole Circle/Dashboard Path layout owner so rail and track use the centered `body size - StrokeThickness` viewport that matches the Ant Design SVG radius contract.
- Gallery
  - Make explicit `PartDescriptions` order the Semantic Preview display order while retaining descriptor order for undescribed parts.
  - Align the ProgressBar Semantic Preview structure with the Ant Design `v6.1.3` demo: keep the selector toolbar inside the preview stage, preserve the middle token gap, and top-align the 200px line, steps, circle, and dashboard preview area.
  - Vertically center the type selector and Gradient switch on the Semantic Preview toolbar.
  - Synchronize the five Progress Semantic Part descriptions with the official English wording and all Gallery locales.

## 2026-08-14

- Feature
  - Publish owner-scoped `root`, `body`, `rail`, `track`, and `indicator` Semantic Parts for the ProgressBar family; `StepsProgressBar` follows the upstream steps structure and does not publish a rail.
  - Project line progress onto stable template `Border` targets, circle and dashboard geometry onto stable template Shapes, and expose each runtime steps Rectangle as a reusable `track` target.
  - Generate concrete owner Style types while keeping default themes independent from `.semantic-*` selectors.
- Fix
  - Preserve `ChunkWidth=NaN` as the automatic sizing state so runtime Large/Middle/Small changes continue to resolve to `14/6/2` instead of retaining the first computed width.
  - Release and rebuild runtime steps targets on detach and template reapplication without retaining old parents or subscriptions.
- Theme
  - Give `CircleProgress` and `DashboardProgress` concrete leaf templates so source generation validates every public owner instead of relying on an inherited abstract-only template.
- Gallery
  - Add a truly deferred four-type Semantic Preview aligned with the Ant Design `v6.1.3` `_semantic.tsx` example.
  - Reproduce the Ant Design `v6.1.3` `style-class.tsx` visual example with six percentages, root padding/radius, rail color/radius, hue gradients, and CSS-ease transitions; label it with the current AtomUI version and AtomUI Semantic Part terminology.
  - Match the upstream semantic style example's vertical rhythm without adding a second `SpacingLG` gap between naturally measured ProgressBar rows.
- Docs
  - Add the complete ProgressBar Semantic Part contract and synchronize overview, implementation, Gallery, LLMS sources, compatibility boundaries, and verification requirements.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ProgressBar`.
  - Align generated output paths with `controls/progress-bar/index-cn.md` and `controls/progress-bar/semantic-cn.md`.

## 2026-06-22

- Docs
  - Establish ProgressBar desktop architecture documentation under `docs/controls/desktop/feedback/progress-bar/overview.md`.
  - Add ProgressBar implementation documentation for shared RangeBase state, progress normalization, line / step / circle / dashboard rendering, lifecycle and maintenance invariants.
  - Add ProgressBar Token documentation for default colors, line spacing, line icon sizes, circle inner info limits and token compatibility boundaries.
  - Add ProgressBar documentation links to the Feedback category entry.
