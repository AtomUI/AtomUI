# 鱼跃龙门 SVG 动画 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 产出一个独立的 SMIL 矢量动画文件 `resources/animations/yu-yue-long-men.svg`：金鲤破水跃过“龙门”牌楼、在门顶化金龙盘旋升天，12 秒无缝循环。

**Architecture:** 全部几何由 Python 生成器按关键帧计算后输出为静态 SVG + `<animate>`（单一 12s 时钟，`values`/`keyTimes` 显式给出，不含脚本）。鱼、龙的身体是“脊柱样条 + 宽度剖面”生成的形变路径，鳞、鳍、鬣、爪等附属件与躯干共用同一组关键帧时刻，靠同源采样保持贴合。生成器与自检工具放在系统临时目录，不进入仓库。

**Tech Stack:** Python 3.9 标准库 + fontTools（仅取字形轮廓）；SMIL；Chrome 无头（CDP 定时截图）做逐帧自检。

**Spec:** 无独立 spec。用户重发原始需求选择跳过 brainstorming 问答，设计决策全部内联在本文「Design Decisions」一节，均为执行者的判断而非用户确认项。

## Global Constraints

- 在主线 `release/6.0` 上工作，不得创建分支或 worktree（AGENTS.md）。
- **未经用户明确指令不得 git commit**：每个任务结束只整理工作区，最终整体交用户审查。
- 生成器、截图、视频、字形中间产物一律放 `/tmp/yuylm/`（仓库“临时工具不入库”规则）；仓库内只新增两个未跟踪文件：成品 SVG 与本计划。
- 成品 SVG：不含 `<script>`、`<text>`、`<image>`、外部引用；`viewBox="0 0 1200 1200"`；周期 `T = 12s`；所有 `<animate>`/`<animateTransform>` 的 `dur="12s"`、`repeatCount="indefinite"`。
- 所有动画通道首末关键帧取值相同（循环无缝）。
- 非动画查看器展示的基准属性 = 英雄帧（`T_HERO = 6.6s`）。
- 下载的字体文件只由 `tools/glyphs.py` 以 `python3 -I` 读取，路径以参数传入。
- 体积预算：成品 ≤ 900 KB；动画元素数 ≤ 220；鱼、龙分组内不使用 `filter` 属性。
- 视觉验收按用户全局规则：agent 产出书面步骤，证据由用户回传；回传前只能标“待视觉验收”。

## Design Decisions

**画幅与构图**（1200 × 1200）：地平线 `y=838`，水面均高 `y=846`；红日圆心 `(600, 520)` 半径 262，位于牌楼之后；牌楼（四柱三楼，局部坐标盒 `x∈[-240,240]`、`y∈[-540,0]`）底座中点锚定 `(600, 828)`，立在江心礁石上；金鲤全长约 205（躯干 150），自画面左侧水下入场，在牌楼**前方**沿弧线跃起，顶点在门楼正上方（头部参考点约 `(548, 150)`）；金龙身长 850，沿门顶上方空间先向右再回勾盘旋，最终由顶部离场。

**时间轴**（秒）：入场游动 0–1.9；蓄势 1.9–2.35；发力 2.35–2.60；破水 2.62；上升 2.62–4.20；滞空化龙 4.20–4.95；金龙舒展 4.45–5.90；盘旋定格 5.90–7.40；离场 7.40–10.20；金屑回落 9.60–11.20；复归平静 11.20–12.00。以上为初始值，实现时可在 ±0.3s 内微调。

**色彩锚点**：天顶 `#1b1140` → 紫红 `#8f2a55` → 地平线橙 `#f59a4a`；红日 `#fff3c4 → #ffcf4d → #ff8a2e → #ef5a23`；水下 `#134a5c → #071c33`；柱 `#c8322a`；彩画石青 `#2f5f9e`、石绿 `#2f8f6b`；琉璃瓦 `#2c8a68`、脊 `#e6b84a`；匾底 `#101a33`、金字 `#f6d06b`；鲤 `#c86a12 → #f0881e → #ffb43a → #ffe29a`；龙 `#ffe08a → #f4b740 → #c97a14`，描边 `#6b2a08`，鬣焰 `#ff5a1f → #ffb02e`。

**细节清单**（每项在对应任务的目检步骤中逐条确认）：

- 鲤：两对触须；长基背鳍带锯齿状硬棘；叉形尾鳍；胸/腹/臀鳍带鳍条；交错排列的圆鳞；鳃盖线；侧线；端位口与唇；带高光的眼。
- 龙（“九似”）：鹿角、驼形头、圆目、蛇颈、腹甲、鳞、鹰爪五趾、虎掌、牛耳；龙须、鬃鬣、背焰鬣、焰尾；四足（近侧在身前、远侧在身后）。
- 牌楼：四柱带石础、雀替、大额枋与龙门枋彩画、斗拱一列、庑殿顶（正脊、鸱吻、垂脊走兽）、瓦当滴水花边檐口、檐下圆形与方形椽头、两次楼翘角、摇摆红灯笼、礁石与浪花。
- 动画原理：蓄势 C 形弯、尾鳍与鳍的跟随、破水缓出、顶点定格、闪光遮换、龙须鬃毛与灯笼的次级运动、水滴重力抛物线。

**关键帧密度**：鱼在其活动区间 16 fps，龙 14 fps，特效通道每个效果 8–16 个采样；密度是否足够由 `test_keyframe_density` 判定。

## Review Focus

1. **不支持 SMIL 的查看器**（Markdown 预览、缩略图、静态导出）：显示基准属性，必须是一幅完整构图而不是空场景。→ `test_fallback_equals_hero_frame`。
2. **形变路径命令结构不一致**（浏览器静默不插值，画面跳变）：→ `test_morph_structure_constant`。
3. **循环接缝跳动**：→ `test_channels_first_equals_last`、`test_dragon_window_zero_at_cycle_ends`。
4. **关键帧过稀导致曲率处折线感**：→ `test_keyframe_density`。
5. **字稿缺失或被替换成系统字体**：→ `test_no_text_elements`（只允许轮廓路径）。
6. **大范围运动元素挂滤镜导致掉帧**：→ `test_no_filters_on_moving_subjects`。
7. **英雄帧龙头龙角被画幅上沿裁切**：→ `test_hero_bbox_inside_canvas`。

---

## 文件结构

生成器根目录 `/tmp/yuylm/`：

| 路径 | 职责 |
| --- | --- |
| `src/yylm/geom.py` | 向量运算、过点样条 `Spline`（弧长参数化）、一维剖面 `Profile`、路径串格式化 |
| `src/yylm/track.py` | 缓动函数、分段关键帧轨道 `Track`（可周期回绕）、采样时刻 `sample_times` |
| `src/yylm/emit.py` | 动画模式/烘焙模式下统一的元素与 `<animate>` 发射 |
| `src/yylm/palette.py` | 颜色常量与渐变 defs |
| `src/yylm/layout.py` | 画幅、锚点、时间轴常量 |
| `src/yylm/sky.py` | 天空、红日、远山、云、雾气、飞鸟、残星 |
| `src/yylm/water.py` | 江面波浪、水下背景、水纹、波光、前景浪 |
| `src/yylm/gate.py` | 牌楼、礁石、灯笼 |
| `src/yylm/fish.py` | 鲤的姿态模型与各部件路径 |
| `src/yylm/dragon.py` | 龙的路径、躯干、头、四足、鬣、尾 |
| `src/yylm/fx.py` | 水花、涟漪、水滴、闪光、鳞片与金屑粒子 |
| `src/yylm/build.py` | 分层组装与命令行入口 |
| `tools/glyphs.py` | 从字体取字形轮廓，输出 `yylm/glyph_data.py` |
| `tools/shot.mjs` | Chrome CDP：对真实 SVG 按时刻批量截图 |
| `tools/sheet.py` | 多帧拼成联络表 |
| `check/test_*.py` | 单元与成品结构检查（`unittest`） |

---

### Task 0: 脚手架

**Files:**
- Create: `/tmp/yuylm/src/yylm/__init__.py`、`/tmp/yuylm/check/__init__.py`

- [ ] 建目录与空包文件；确认 `/usr/bin/python3` 可从任意目录以 `PYTHONPATH=/tmp/yuylm/src:/tmp/yuylm/check` 导入 `yylm`。

### Task 1: 几何与时间基础库

**Files:**
- Create: `src/yylm/geom.py`、`src/yylm/track.py`
- Test: `check/test_geom.py`、`check/test_track.py`

**Interfaces:**
- `Spline(pts, closed=False, samples=400)`：属性 `length`；`pos(s)`、`tangent(s)`、`normal(s)`，`s∈[0,length]` 越界钳制，`normal` 为切向顺时针转 90°（y 向下坐标系中行进方向的右侧）。
- `Profile(knots)`：`__call__(x)`，端点外钳制。
- `catmull_rom_path(points, closed=False, tension=0.5) -> str`（相对坐标三次曲线）；`smooth_path(points, closed=False, tension=0.5) -> str`（中点二次曲线，紧凑版）；`polyline_path(points, closed=False) -> str`。
- `fmt(v) -> str`：1 位小数，不输出 `-0`、`nan`、`inf`。
- `ease(name) -> Callable[[float], float]`，`name∈{linear, inQuad, outQuad, inCubic, outCubic, inOutCubic, outQuart, outQuint, inOutSine, outBack}`。
- `Track(keys, period=None)`：`keys=[(t, value[, ease]), ...]`，`value` 为 float 或二元组；`__call__(t)`；`period` 给定时按周期回绕。
- `sample_times(segments) -> list[float]`：`segments=[(t0, t1, fps), ...]`，并集去重升序，总是包含 0 与 12.0。

- [ ] **Step 1: 写失败测试** `check/test_geom.py`：
  - `test_spline_passes_through_knots`：开口样条 `pos` 在各节点弧长处与节点距离 < 0.5。
  - `test_spline_circle_length`：12 边等距点闭合样条近似半径 100 的圆，`length` 与 `2π·100` 相对误差 < 3%。
  - `test_spline_arc_parameter`：曲线上 `|pos(s+1) − pos(s) − 1| < 0.02`（取 20 个 `s`）。
  - `test_fmt_edge_values`：`fmt(-0.04) == "0"`，`fmt(12.25) == "12.3"`（或 `"12.2"`，二者之一），`fmt(float("nan"))` 抛 `ValueError`。
  - `test_smooth_path_commands`：以 `M` 开头且只含 `M m Q q L l Z z`。
- [ ] **Step 2: 写失败测试** `check/test_track.py`：
  - `test_track_linear_and_out_quad`：`Track([(0, 0.0, "linear"), (2, 10.0)])` 在 `t=1` 为 5；`Track([(0, 0.0, "outQuad"), (2, 10.0)])` 在 `t=1` 为 7.5。
  - `test_track_periodic`：`Track([(0, 0.0), (4, 8.0)], period=4)` 在 `t=5` 与 `t=1` 相等。
  - `test_sample_times_bounds`：`sample_times([(1, 2, 10)])` 首元素 0、末元素 12.0，中间相邻差为 0.1。
- [ ] **Step 3: 运行** `PYTHONPATH=/tmp/yuylm/src:/tmp/yuylm/check /usr/bin/python3 -m unittest test_geom test_track -v`，预期 FAIL（模块不存在）。
- [ ] **Step 4: 实现** `geom.py`、`track.py`。
- [ ] **Step 5: 运行同一命令**，预期 OK。

### Task 2: 发射器、调色板、布局与组装骨架

**Files:**
- Create: `src/yylm/emit.py`、`src/yylm/palette.py`、`src/yylm/layout.py`、`src/yylm/build.py`
- Test: `check/test_emit.py`、`check/test_svg.py`

**Interfaces:**
- `emit.py`：
  - `Anim(attr, times, values, calcMode="linear", extra=None)`；`TransformAnim(type_, times, values, extra=None)`。
  - `E(tag, inner="", anims=(), transform=None, **attrs)` → 元素；`.render()` 返回字符串。
  - `set_mode(mode)`：`"anim"` 或 `("bake", t)`；`hero_value(anim)` 在 anim 模式取 `T_HERO` 处取值、bake 模式取 `t` 处取值（线性插值 `values`），据此写入基准属性。
  - `group(inner, anims=(), **attrs)`：含 `TransformAnim` 时自动嵌套 `translate → rotate → scale` 三层 `<g>`。
- `layout.py`：`W`、`H`、`T`、`T_HERO`、`HORIZON_Y`、`SURFACE_Y`、`SUN`、`GATE_ANCHOR`、`FISH_BODY`、`DRAGON_LEN` 及时间轴各节点常量（名同 Design Decisions 中的阶段名大写：`T_WINDUP`、`T_LUNGE`、`T_BREACH`、`T_APEX`、`T_UNFURL0`、`T_UNFURL1`、`T_HOLD1`、`T_GONE`、`T_RAIN0`、`T_RAIN1`）。
- `build.py`：`build_svg(bake=None) -> str`；命令行 `python3 -m yylm.build --out FILE [--bake T]`。

- [ ] **Step 1: 写失败测试** `check/test_emit.py`：
  - `test_anim_counts`：`Anim` 构造时 `len(times) != len(values)` 抛 `ValueError`。
  - `test_nested_transform_groups`：`group` 带平移与旋转通道时输出三层 `<g transform="translate(…)">`、`<g transform="rotate(…)">` 及内层内容。
  - `test_bake_writes_base_and_no_animate`：bake 模式输出不含 `<animate`，基准 `transform` 等于该时刻取值。
- [ ] **Step 2: 写失败测试** `check/test_svg.py`（成品级，函数 `checks(svg_text) -> list[str]` 返回违规描述）：良构 XML 且 `viewBox` 正确；无 `script/text/image` 与 `http` 引用；`id` 唯一且 `url(#…)`/`href="#…"` 可解析；所有 `animate*` 的 `dur="12s"`、`repeatCount="indefinite"`、`values` 与 `keyTimes` 数量相等、`keyTimes` 严格递增且首 0 末 1、首末取值相同；`d` 动画各帧命令序列一致；无 `nan/inf`；体积与动画元素数不超预算。另含 `main`：读文件、打印违规、非零退出。
- [ ] **Step 3: 运行** 测试，预期 FAIL。
- [ ] **Step 4: 实现** 四个模块；`build_svg` 先只输出底色矩形加一个 12s 平移通道的占位矩形。
- [ ] **Step 5: 运行** 测试，预期 OK；`python3 -m yylm.build --out /tmp/yuylm/out/skeleton.svg` 后 `check/test_svg.py` 的 `main` 输出无违规。

### Task 3: 截图与联络表工具

**Files:**
- Create: `tools/shot.mjs`、`tools/sheet.py`
- Test: `check/test_shot.py`

**Interfaces:**
- `node tools/shot.mjs --svg FILE --out DIR --times 0,1.5,3 --size 1200`：对**真实成品**（含 SMIL）调用 `pauseAnimations()` + `setCurrentTime(t)` 后逐帧 `Page.captureScreenshot`，输出 `DIR/f_0000.00.png` 等；同一 Chrome 进程内完成。
- `python3 tools/sheet.py --dir DIR --cols 4 --out SHEET.png [--crop x,y,w,h] [--scale 1.0]`。

- [ ] **Step 1: 写失败测试** `check/test_shot.py`：`test_captures_two_distinct_frames` —— 生成一个方块在 0–12s 内从左移到右的最小 SMIL SVG，截 `t=0` 与 `t=6`，断言两张 PNG 均生成且 PIL 逐像素差异非零、且方块质心 x 后者大于前者。
- [ ] **Step 2: 运行**，预期 FAIL（工具不存在）。
- [ ] **Step 3: 实现** 两个工具（Node 24 自带 `WebSocket`，不装依赖）。
- [ ] **Step 4: 运行**，预期 OK。

### Task 4: 天空、红日、远山、云雾

**Files:** Create `src/yylm/sky.py`；Modify `src/yylm/build.py`、`src/yylm/palette.py`

**Interfaces:** `sky_layer() -> str`（天空底、残星、红日与日晕、光芒、掠日薄云）；`mountains_layer() -> str`；`clouds_back() -> str` / `clouds_front() -> str`；`mist_band() -> str`；`birds() -> str`。云雾漂移一律用“按瓦片宽度平移一个周期”或“12s 内整数次往复”实现。

- [ ] **Step 1: 扩展 `test_svg.py`**：新增 `test_drift_channels_loop`——所有 `translate` 通道首末取值相同（已含于通用检查，此处以 `sky` 输出单独跑一遍 `checks`）。
- [ ] **Step 2: 运行**，预期 FAIL（模块不存在）。
- [ ] **Step 3: 实现** 并接入 `build_svg` 分层。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** `shot.mjs --times 0,6` 全幅帧：红日被牌楼位置（此时为空）居中衬底；远山三层层次分明、雾带分隔；祥云位于上角且不压门顶化龙区（`y∈[40,300]`、`x∈[380,1120]` 保持通透）。

### Task 5: 江水

**Files:** Create `src/yylm/water.py`；Modify `src/yylm/build.py`

**Interfaces:** `water_far() -> str`（远水反光带）；`waves_mid() -> str`；`underwater_bg() -> str`（含水纹与光柱，位于鱼之后、鱼需被其色调罩住的部分由 `underwater_tint() -> str` 承担）；`waves_front() -> str`（画面底缘前景浪）；`glitter() -> str`。

- [ ] **Step 1: 写失败测试** `check/test_water.py`：`band_profile(x, seed) -> float` 为浪带顶缘高度函数，周期 `TILE = 1200`。`test_band_profile_periodic`：取 20 个 `x`，`band_profile(x) == band_profile(x + TILE)`；`test_band_profile_not_flat`：同一 `seed` 下极差 > 20。
- [ ] **Step 2: 运行**，预期 FAIL。
- [ ] **Step 3: 实现** 并接入分层：顺序为 牌楼 → 中景浪 → 鱼 → 水下色调 → 前景浪 → 水花。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** `--times 0,3,6`：三层浪速不同且 0s 与 12s 帧逐像素一致（用 `sheet.py` 之外另做 PIL 差分，差异为 0）；水面线在礁石处有浪花环。

### Task 6: 牌楼与字稿

**Files:** Create `tools/glyphs.py`、`src/yylm/gate.py`；由 `glyphs.py` 生成 `src/yylm/glyph_data.py`；Modify `src/yylm/build.py`

**Interfaces:**
- `tools/glyphs.py`：`python3 -I tools/glyphs.py --font /tmp/yuylm-dl-fonts/MaShanZheng-Regular.ttf --text 龙门 --out src/yylm/glyph_data.py`，输出 `GLYPHS = {"龙": (width, "M…Z…"), "门": …}`（路径已归一化到 1em 高、基线在 `y=0`、y 向下）。
- `gate.py`：`gate_group() -> str`（整体 `<g id="gate" transform="translate(600 828)">`，内部再分 `gate_static()` 与 `lanterns()`）。

- [ ] **Step 1: 扩展 `test_svg.py`**：`test_plaque_glyphs_present`——成品含 `id="plaque-text"` 且其内两个 `<path>` 均非空、`d` 长度 > 200。
- [ ] **Step 2: 运行**，预期 FAIL。
- [ ] **Step 3: 生成字稿并实现牌楼**（细节清单中牌楼一项逐条落图）。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** 牌楼局部 2 倍放大帧（`sheet.py --crop 340,270,520,580 --scale 2`）：主次三楼比例、鸱吻与走兽、斗拱列、瓦当滴水花边、椽头、雀替、匾文“龙门”楷书金字清晰、灯笼与礁石浪花齐全；无部件互相穿插错位。

### Task 7: 金鲤静姿

**Files:** Create `src/yylm/fish.py`；Modify `src/yylm/build.py`、`layout.py`

**Interfaces:**
- `FishState(pos, heading, bend, wave_phase, wave_amp, fin_spread, fin_flap, cut, glow)`（`cut` 为已被龙身吞并的头部比例 0–1）。
- `fish_state(t) -> FishState`。
- `fish_spine(st) -> list[tuple[float, Vec]]`：`[(u, 点), …]`，`u∈[0,1]` 吻端到尾柄末端，**局部坐标**（原点在头部参考点 `u=0.1`，`+x` 指向吻端，`−y` 为背侧）。
- `fish_rigid(st) -> tuple[Vec, float]`：`(平移, 旋转角度)`，把局部坐标带入世界坐标。
- `fish_parts(st) -> dict[str, str]`：键为 `body, sheen, dorsal, caudal, rays_caudal, pectoral_near, pectoral_far, pelvic, anal, scales, gill, lateral, barbel_near, barbel_far, eye_lid`，值为局部坐标路径串。
- `fish_group(times) -> str`。

- [ ] **Step 1: 写失败测试** `check/test_fish.py`：
  - `test_scales_inside_body`：静止姿态下每个鳞弧的端点与控制点均在躯干轮廓多边形内（射线法）。
  - `test_cut_removes_head_side`：`cut=0.4` 时 `body` 路径的吻端点位于 `cut=0` 时脊柱 `u=0.4` 处附近（距离 < 3）。
  - `test_fins_anchor_on_spine`：各鳍根部首点落在轮廓上（到轮廓折线距离 < 1.5）。
- [ ] **Step 2: 运行**，预期 FAIL。
- [ ] **Step 3: 实现静姿** 并接入 `build_svg`（位置暂取顶点滞空姿态，`opacity` 通道先恒 1）。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** 鱼局部 4 倍放大帧：细节清单中“鲤”一项逐条确认；轮廓无折角；鳞片随体型由大变小；鳍半透明带鳍条；两对触须可辨。

### Task 8: 金鲤动姿与破水

**Files:** Modify `src/yylm/fish.py`、`src/yylm/fx.py`（新建）、`build.py`

**Interfaces:**
- `fx.py`：`splash(t_breach) -> str`（水冠、水柱、泡沫环）；`ripples() -> str`；`droplets(n=26) -> str`（各水滴沿自身抛物线，落到水面即消失）；`fx_group() -> str`。

- [ ] **Step 1: 扩展 `check/test_fish.py` 与 `check/test_model.py`（新建）**：
  - `test_fish_state_continuous`：在 `[0, 12)` 上以 1/240s 步长采样，相邻 `pos` 距离的最大值 < 60（无跳变）。
  - `test_fish_hidden_after_consumed`：`t > T_UNFURL0 + 0.7` 时鱼分组 `opacity` 通道取值为 0。
  - `test_droplets_obey_gravity`：每个水滴通道在飞行段相邻采样的二阶差分 `Δ²y` 恒正且近似常数。
- [ ] **Step 2: 运行**，预期 FAIL。
- [ ] **Step 3: 实现** 入场、蓄势、发力、破水、上升、滞空各段及水花特效。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** `[0, 5.0]` 每 0.25s 一帧的联络表：蓄势时身体呈 C 形且尾向腹侧滞后；离水瞬间水冠与泡沫环出现；空中身体舒展、胸鳍张开、背鳍竖立；尾鳍摆动滞后于躯干；水滴沿抛物线回落。

### Task 9: 金龙静姿

**Files:** Create `src/yylm/dragon.py`；Modify `build.py`

**Interfaces:**
- `dragon_path() -> Spline`：世界坐标中线，`[0, 175]` 段与鱼滞空姿态脊柱重合。
- `head_s(t) -> float`：龙头沿路径的弧长位置；活动区间外返回使躯干窗口为空的哨兵值 `-1`。
- `body_frames(s_head, t) -> list[Frame]`，`Frame(center, tangent, normal, half_dorsal, half_ventral, u_rel)`。
- `dragon_group(times) -> str`；`dragon_head_group(times) -> str`；`dragon_legs(times) -> str`。

- [ ] **Step 1: 写失败测试** `check/test_dragon.py`：
  - `test_path_matches_fish_hold_spine`：`s∈[0,150]` 取 10 点，令 `u = 1 − s/150`，`dragon_path().pos(s)` 与 `fish_spine(fish_state(T_APEX))` 中 `u` 处的点经 `fish_rigid` 变换到世界坐标后距离 < 2.5。
  - `test_body_never_self_intersects`：在 `[T_UNFURL1, T_GONE]` 每 0.2s 取一帧，躯干轮廓多边形自交测试为假。
  - `test_head_inside_canvas_at_hero`：`T_HERO` 时头部所有路径点（含角、须）`y ≥ 8`。
- [ ] **Step 2: 运行**，预期 FAIL。
- [ ] **Step 3: 实现** 龙头（局部坐标朝 `+x`、腹侧 `+y`）、躯干、腹甲、鳞、背鬣、四足五爪、焰尾。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** 龙头 3 倍、龙身 2 倍放大帧：“九似”逐条确认；口内牙齿与舌、鼻须卷曲、肘毛、爪形清晰；腹甲与背鬣贴合躯干；近侧足在身前、远侧足在身后且色深。

### Task 10: 化龙、盘旋、离场与特效

**Files:** Modify `src/yylm/dragon.py`、`src/yylm/fx.py`、`src/yylm/fish.py`、`build.py`

**Interfaces:** `fx.py` 新增 `flash() -> str`、`shockwave() -> str`、`scale_particles() -> str`、`sparkles() -> str`、`trail_dust() -> str`、`golden_rain() -> str`。

- [ ] **Step 1: 写失败测试** `check/test_model.py`：
  - `test_channels_first_equals_last`：所有 `Anim` 首末取值字符串相同。
  - `test_dragon_window_zero_at_cycle_ends`：`head_s(0) == head_s(12) == -1`。
  - `test_keyframe_density`：几何通道在相邻关键帧中点，线性插值与模型取值距离 ≤ 2.5；不透明度通道 ≤ 0.03。
  - `test_fallback_equals_hero_frame`：动画模式输出剔除所有 `<animate…/>` 后与 `build_svg(bake=T_HERO)` 逐字节相同。
  - `test_morph_structure_constant`、`test_no_filters_on_moving_subjects`（文件级，放在 `test_svg.py` 的 `checks` 中并由 `test_model.py` 调用）。
- [ ] **Step 2: 运行**，预期 FAIL。
- [ ] **Step 3: 实现** 轨迹跟随（躯干窗口 `[head_s − 850, head_s]`）、沿体侧波动、由龙头至龙尾逐步舒展、回勾定格、离场；鱼身被自头至尾吞并并以光带遮住接缝；闪光、冲击环、鳞片四散、金屑、尾迹、金雨。
- [ ] **Step 4: 运行** 测试，预期 OK。
- [ ] **Step 5: 目检** `[4.0, 11.5]` 每 0.25s 一帧的联络表：闪光瞬间完成鱼龙替换、无穿帮帧；龙身舒展时无自交与折角；定格姿态完整且龙头回望门楼；离场后画面与 0s 仅差环境动画。

### Task 11: 收尾验证与交付

- [ ] **Step 1** 全量运行 `PYTHONPATH=… /usr/bin/python3 -m unittest discover -s /tmp/yuylm/check -v`，预期全部 OK；记录条数。
- [ ] **Step 2** `python3 -m yylm.build --out /Users/chinboy/Projects/dotnet/AtomUIV6/resources/animations/yu-yue-long-men.svg`，随后 `check/test_svg.py <该文件>` 无违规，记录体积与动画元素数。
- [ ] **Step 3** 每 0.2s 一帧共 60 帧截全周期，用 `ffmpeg` 合成 `/tmp/yuylm/out/preview.mp4` 观看一遍完整节奏；再对 `t=0` 与 `t=12`（即 `11.999`）做 PIL 差分确认接缝。
- [ ] **Step 4** 在 Safari 中打开成品（`open -a Safari`）确认 WebKit 下 SMIL 形变与遮罩行为正常——此项如无法自动取证，转入交付说明的书面验收步骤。
- [ ] **Step 5** `git status --short` 确认仓库内只多出成品 SVG 与本计划两个未跟踪文件；清理 `/tmp/yuylm/out` 中的帧图与视频（保留生成器源码以便用户索取）。
- [ ] **Step 6** 交付说明：成品链接、生成器位置、实际执行的验证命令与结果、**书面视觉验收步骤**（打开方式、各时段预期现象、判定标准、需回传的截图/录屏范围），并标注“待视觉验收”。
