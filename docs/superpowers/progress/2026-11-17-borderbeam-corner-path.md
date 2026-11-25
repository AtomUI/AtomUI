# BorderBeam 流线与轨道圆角分离修复（环半径与内容脱钩 + 路径半径错源）

日期：2026-11-17（当日二次返工，首版修复被用户否决后重新取证）
状态：已修复（v2），待用户审查与真机视觉验收（未提交）
分支：`fix/shadcn`（worktree：`/Users/chinboy/Projects/dotnet/AtomUIV6-fix-shadcn`）

## 现象与返工

用户首次报告：Shadcn Luma 下 BorderBeam 流线与卡片轨道圆角分离（红框在卡片角部）。
首版修复（路径半径从 `BeamSize` 改为取宿主圆角的**最大角**）交付后用户反馈：**Luma 没修好，且把 Ant Design 改出了 bug**。本次按脚本视觉验证重新取证，推翻首版结论。

## 真根因（两层，均有量化取证）

**第一层（Luma 分离的主因）：环半径与内容卡片脱钩。**
`IBorderBeamAwareControl` 机制（让宿主内容把自己的边框厚度+圆角喂给 BorderBeam）**没有任何实现者**，
`EffectiveBorderBeamGeometry` 永远退回 BorderBeam 自身的 token 圆角：

- 探针实测（Gallery 同款组合）：Luma 下 `EffectiveBorderBeamGeometry.CornerRadius = 26`（`BorderRadiusLG`），
  而内容 Card 显式 `CornerRadius=8` → **光带的裁剪环是 26 半径的曲线，卡片角是 8**，两者在角部分离。
- AntD 下 token 恰好也是 8（与卡片巧合一致）→ 分离不可见。

**第二层（路径半径错源，两层主题共有）：**
`Render` 用 `BeamSize`（默认 100）作为运动路径圆角 → 路径与环是两条不同曲线。

**首版修复错在哪**：把路径半径改成"宿主圆角的**最大角**"——均匀圆角下恰好同时解决两层，
看起来有效；但它把非均匀圆角（Gallery "Non-uniform radius" 示例 `20,20,0,0`）的**底部直角也抹成了 20 的圆弧**，
路径在直角处离开环 → AntD（默认主题即触发）出现新缺陷。这正是用户看到的 "Ant Design 也改出 bug"。

## 最终修复（v2）

1. **`Card : IBorderBeamAwareControl`**（`src/AtomUI.Desktop.Controls/Card/Card.cs`）：
   `GetBorderBeamGeometry() => new(BorderThickness, CornerRadius)`，`BorderThickness/CornerRadius` 变更时触发
   `BorderBeamGeometryChanged` → 环+路径跟随内容卡片的真实边框，任何主题不再依赖 token 巧合。
2. **采样器每角半径**（`BorderBeamPresenter.CreateMetrics` 现接收完整 `CornerRadius`）：
   WinUI keypoints 天然携带非均匀圆角；角部弧长/切线由 keypoints 推导，非均匀自动正确。
   直角（0）处路径保持直角，不取最大值。
3. `BeamSize` 回归其本职（光带尺寸/渐变长度），不再参与路径几何。

## 脚本视觉验证（真实 Gallery 页面，冻结多相位，逐角弧量化）

度量：对页面上每个 beam 的每个**圆角弧**采样，统计光像素"在卡片角弧上（≤3.5px）vs 偏离（≥6px）"。
Luma 原色为中性灰不可检出，探针统一以公共 API 设为饱和蓝（不影响几何）。

| 状态 | 主题 | 头部经过的角弧（代表性） | 结论 |
|---|---|---|---|
| 基线（仿真旧行为：BeamSize 路径 + Card 不感知） | Luma | `on=0/off=78`、`on=0/off=127` | 光全部偏离卡片角弧 → 分离实锤 |
| 基线 | AntD | `on=300~613/off≤3` | AntD 原本贴合（token 巧合=8），无分离 |
| **v2 修复后** | **Luma** | `on=548~638/off≤4` | 光钉在卡片 8px 角弧上 |
| **v2 修复后** | **AntD** | `on=613~638/off≤4` | 同样贴合，无回退 |

非均匀示例（`20,20,0,0`）由几何回归测试锁定：底边采样点 y==60（直边上）、顶角弧上 x∈(100,120)。

## 回归测试

`tests/AtomUI.Desktop.Controls.Rendering.Tests/BorderBeamCornerPathRenderingTests.cs`（重写）：
- `Guide_Path_Keeps_The_Square_Corners_Of_A_Non_Uniform_Ring`：底直角不被抹圆（锁定首版回归）；
- `Guide_Path_Rounds_Each_Corner_With_Its_Own_Radius`：每角各自半径；
- `Aware_Card_Feeds_The_Beam_Its_Own_Frame`：Card 几何流入 beam，且改 Card 圆角即时生效（锁定 Luma 根因）。

## 模块验证

- Rendering.Tests（BorderBeam 全部）15/15；`DisplayFamilyEffectTests` 240/240；
- Desktop.Controls.Tests（BorderBeam+Card 过滤）156/156；Gallery（BorderBeam 过滤）4/4；
- `git diff --check` 干净；临时探针与全部媒体已删除（证据只留本表数字）。

## 遗留说明

- 非感知内容（如示例中的普通 `Border`）仍以 BorderBeam 自身 `CornerRadius` 为环——Gallery "Custom container"
  示例显式同时设置了 beam 与内容圆角，语义自洽；文档口径：**内容自带边框圆角时建议用 Card 或自行实现
  `IBorderBeamAwareControl`**。
- 首版交付的进展记录已由本文覆盖；控件 changelog 同步改写。

## 补记（同日第三轮）：动画卡顿 + 转角跳变

用户在真窗口观察反馈：动画卡、转角跳。定位与处置：

- **卡顿根因**：`Render` 每帧重建裁剪几何、路径度量、渐变画刷（纯分配churn→GC 停顿）。
  修复：三对象输入键控缓存，稳态零分配。
- **转角跳**：几何上不成立——新增连续性契约测试（r=0/8/26，720 等步进，步长 ≤ 均匀值×1.01；
  直角处的弦缩短 0.707× 为物理正确，仅断言上界）。此前感知的"跳"与掉帧（卡顿）叠加；
  100px 长光带过 8px 小圆角本身是"快速甩过"（上游同构行为），缓存修复后如仍有感知，
  属产品参数议题（BeamSize/圆角比例），另立项。
- **脚本视觉验收（真窗口 + 原生 screencapture）13/13 全 PASS**：
  Demo 在 repo 外临时目录（/tmp/atomui-shadcn-acceptance），有效性门（A/A=0、1DIP 可检出、复原=0）通过；
  覆盖 AntD 回归（Empty 灰阶 4 档/QRCode 角部 0%/CheckBox 方形/Beam 直角贴合）、Luma（胶囊 wedge=0、
  Empty 4 档、QRCode r26、CheckBox 方形、Beam onRing=2341/offRing=0）、Maia（胶囊/CheckBox）。
  证据：evidence/results.md + 25 张原生 PNG + identity.txt（已随会话保留在临时目录，媒体不入库）。


## 终版补记（同日第四轮）：换算法（用户拍板）

用户连续两轮反馈"卡顿/转角跳"并明确"换一个算法"。终版放弃自研路径采样绘制，改用官方 border-beam 同构
**锥形渐变（conic-gradient）旋转 + 环带填充**：

- 构造：缓存环带几何（外内圆角矩形 Exclude）+ 一个 `ConicGradientBrush`（Count 窗口重复于同一刷），
  每帧仅 `Angle = Progress*360` + 一次 `DrawGeometry`。
- 性能实测：绘制 6.4µs/帧（Count 无关）；旧实现每帧仅重建就 17–18µs 且纯分配；300 帧 0 GC（全代）；
  软件光栅整帧 3.9ms。
- 正确性：角部贴合由几何保证；连续性由角度周期保证（永久测试 `BorderBeamMotionRenderingTests`：角度均匀
  推进 ±60%、不得中途消失；r=8/26 双例）。中间各版（弧长采样→分段弦渐变→中线描边→DrawLine 切片）的教训：
  在"沿路径渐变"上自拟实现，每版都在角部或 GC 上翻车；官方构造一行画刷全解决。
- 套件：Desktop 23/23、Rendering 245/245（含 Effect 家族）、Gallery 4/4。


## 终版（2026-11-18 凌晨）：保留原始算法 + 两处零行为变化的性能修复

用户判断正确：原始绘制算法（旋转渐变条 + 环带裁剪）本身正确，Luma 分离真因是环半径取 token(26) 而非卡片圆角(8)——
该缺陷已由 `Card : IBorderBeamAwareControl` 根因修复。期间自研替代算法（弧长切片→锥形渐变→原生 Skia→预算刷表）
逐个引入新缺陷（环绕弦、首尾反向、每帧刷子快照 GC、合成器坐标），全部废弃，`BorderBeamPresenter` 的绘制主体回退
到 HEAD 原版，仅保留两处零行为变化的修复：

1. 输入键控三缓存（裁剪几何 / 路径度量 / 渐变刷）；
2. 渐变刷缓存为 `ToImmutable()` 快照——可变渐变刷每帧在绘制上下文里被转换，正是周期性 GC 停顿的来源。

**最终证据**：
- 3600 帧（60 秒等效，60fps 步进）全代 0 次 GC（此前的 300 帧探针为假阴性，未跨 GC 周期）；
- 真窗口脚本验收（原生 screencapture）：G1/G2 有效性门、B1 Luma r=8 与 B3 Maia r=26 每帧在场（73/84 样本）
  且卡外泄漏=0（基线差分度量）、B2 非均匀(20,20,0,0) 贴环 711 且泄漏=0、**P1 帧率：9 光带全速 5 秒，
  p50=8.3ms / p95=13.9ms / max=24.2ms**；
- 套件：Desktop 23/23、Rendering 243/243（含 DisplayFamily Effect 全家）、Gallery 4/4；
- 永久测试：`Aware_Card_Feeds_The_Beam_Its_Own_Frame`（分离根因守卫）、`Beam_Stays_On_The_Ring_At_Every_Frame`
  （每帧在场）、`Path_Sampler_Advances_Continuously_Through_Corners`（采样器连续性 720 步）。
