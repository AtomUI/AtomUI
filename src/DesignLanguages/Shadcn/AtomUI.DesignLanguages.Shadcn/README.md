# AtomUI.DesignLanguages.Shadcn

shadcn/ui 设计语言。

| 对标 | 版本 |
| --- | --- |
| shadcn/ui（ui.shadcn.com/create 与 registry） | 2026-11-15 实测的现行版本 |
| Tailwind CSS 色板 | v4.3.5（`theme.css`，oklch → sRGB 按 CSS Color 4 换算） |

取值以自动生成的取值附录为唯一基准：`docs/developments/specs/2026-11-15-shadcn-latest-alignment-data.md`（附录 A
Base Color、附录 B Theme、附录 C 各风格 registry class）。各轴中只有紧凑密度是 AtomUI 扩展，其余（8 种风格、明暗、
桌面形态）对标官方。

## 启用

```csharp
builder.UseDesktopControls();
builder.UseShadcn(language => language.AddDesktopControls().AddDesktopColorPicker().AddDesktopDataGrid());
builder.WithInitialTheme(ThemeSelection.Of("Shadcn", ShadcnStyles.Nova));
```

各控件包的取值在对应的取值包里：`AtomUI.Controls.Themes.Shadcn`、`AtomUI.Desktop.Controls.Themes.Shadcn`、
`AtomUI.Desktop.Controls.ColorPicker.Themes.Shadcn`、`AtomUI.Desktop.Controls.DataGrid.Themes.Shadcn`。

## 轴与种子

| 维度 | 承载 | 取值 |
| --- | --- | --- |
| 风格 | 语言的风格轴（`ShadcnStyles`） | Vega、**Nova**（默认）、Maia、Lyra、Mira、Luma、Sera、Rhea |
| 明暗 | 明暗轴 | Light、Dark |
| 密度 | 密度轴 | Default；Compact 是 AtomUI 扩展：在该风格尺寸上按 Ant 紧凑算法再压缩一级（高度 -4） |
| 形态 | 形态轴 | Desktop |
| Theme（品牌色） | 可调种子 `ColorPrimary` | Neutral（跟随 Base Color，默认 neutral-900 `#171717`）+ 17 种彩色（Tailwind 700 级，Lime/Yellow 为 400 级） |
| Base Color（中性色） | 可调的语言种子 `BaseColor`（`ShadcnToken.BaseColor`） | Neutral（默认 `#737373`）、Stone、Zinc、Mauve、Olive、Mist、Taupe |

- 颜色全部来自 **Tailwind v4.3.5 色板**（`TokenEvaluation/ShadcnTailwindPalette.cs`，脚本生成，勿手改）：shadcn 的颜色变量
  都是某个色阶的某一档，取档规则见 `ShadcnBaseColors`。从仓库根目录运行
  `python3 scripts/generate-shadcn-tailwind-palette.py /path/to/tailwindcss/theme.css` 可重新生成该文件。
- 中性色种子 = 该 Base Color 浅色 `--muted-foreground`（色阶 500 档）；未命中官方 7 档的自定义种子按 OKLCH 生成 11 档
  色阶（明度取 Neutral 阶梯、色相取种子、色度按 Zinc 比例缩放）。
- Theme 种子 = 官方浅色主色（700/400 档），暗色取 800/500 档，前景取 50/900 档；自定义主色暗色按官方 17 主题的平均
  明度差/色度比派生，前景保证与主色对比度 ≥ 4.5。
- 语言种子默认值（`DefineSeeds`）：`ColorPrimary=#171717`、`EnableWaveSpirit=false`，语言种子 `BaseColor=#737373`。

## 语言 Token（`ShadcnToken`）

| 类 | 内容 |
| --- | --- |
| 角色色 | `Background`…`Ring`：当前明暗下的 16 个 Base Color 变量；`ThemePrimary`、`ThemePrimaryForeground`、`HasColorTheme` |
| 图表 | `Chart1`…`Chart5` 对应 `--chart-1`…`--chart-5`，主题可逐项覆盖 |
| 几何 | 风格的高度、按钮/输入/勾选框/进度条圆角、内边距、字号、按钮字重、焦点环宽度与浓度、卡片圆角/内边距/阴影/描边环 |
| 弹层 | 菜单、Popover、Dialog、Drawer 的阴影档与圆角/内边距，描边环浓度与当前明暗的 `OutlineRing`，Tooltip 圆角，遮罩浓度 |

控件推导读全局 Token 里有位置的值，只有全局词汇里没有的概念才读语言 Token。

## 组件配方

取值包在控件推导中从 `context.Language` 创建只读配方。表格、列表项和弹层配方接收该控件的 `view.Global`。
输入框另接收 `context.Variant`，用于分辨背景浓度与边框形态。图表配方直接读取语言 Token。

| API | 取值 |
| --- | --- |
| `GetTableRecipe(global)` | 表头、行、悬停、选中、边框、尺寸档单元格内边距、容器圆角。沿用桌面 DataGrid 的角色映射。 |
| `GetChartRecipe()` | `Series1`…`Series5` 按顺序读取当前主题的五个图表系列色。 |
| `GetListItemRecipe(global)` | 列表悬停、选中与选择面板聚焦的底色及前景色。 |
| `GetMenuOverlayRecipe(global)`、`GetPopoverOverlayRecipe(global)`、`GetDialogOverlayRecipe(global)` | 面板底色、描边环、阴影、圆角、内边距与 Dialog 遮罩。描边环可随阴影绘制，也可由带箭头的面板单独绘制。 |
| `GetFieldRecipe(global, variant)` | 输入背景、边框、悬停与聚焦、焦点环、圆角、占位符和禁用，以及 Sera 底边框与 Luma、Rhea 透明边框。 |

配方在本次推导内使用。主题通过覆盖配方对应的语言或全局 Token 改变结果。

图表配色采用 shadcn 注册表默认配置中单独的 Neutral `chartColor`。AtomUI 在明暗两种外观下均使用
Tailwind neutral 的 300、500、600、700、800 级，依次为 `#D4D4D4`、`#737373`、`#525252`、`#404040`、
`#262626`。Base Color 和主题主色有各自的推导。来源见
[默认配置](https://github.com/shadcn-ui/ui/blob/e62c9435fff729a175f38983dee73b2cb4f6a069/apps/v4/registry/config.ts#L165-L170)、
[Neutral 浅色](https://github.com/shadcn-ui/ui/blob/e62c9435fff729a175f38983dee73b2cb4f6a069/apps/v4/registry/themes.ts#L27-L31)和
[Neutral 深色](https://github.com/shadcn-ui/ui/blob/e62c9435fff729a175f38983dee73b2cb4f6a069/apps/v4/registry/themes.ts#L61-L65)。

## 全局推导

Map：先跑复制自 Ant 的计算器（默认，或暗色叠在默认上），背景与文字基色取 shadcn 的 `--background`/`--foreground`；
再写入 shadcn 变量与风格几何；紧凑密度最后跑复制自 Ant 的紧凑计算器。Alias：复制自 Ant 的 Alias 公式，焦点环颜色与
宽度写在其公式处（后面的输入框阴影读它们），其余 shadcn 变量写在公式之后。

## 颜色映射（Base Color → 全局 Token）

| shadcn 变量 | AtomUI Token |
| --- | --- |
| `--background` | `ColorBgLayout`、`ColorBgContainer`、`ColorBgBase` |
| `--card` | Card 控件视图的 `ColorBgContainer` |
| `--popover` | `ColorBgElevated` |
| `--foreground` | `ColorText`、`ColorTextHeading`、`ColorIcon`、`ColorIconHover`、`ColorBgSpotlight`（Tooltip 反色底） |
| `--primary` / `--primary-foreground` | `ColorPrimary` / `ColorTextLightSolid`；实色底 `ColorBgSolid*` 取主色与其 80% 合成 |
| `--muted-foreground` | `ColorTextSecondary`、`ColorTextDescription`、`ColorTextPlaceholder`；50% 合成作 `ColorTextTertiary/Quaternary/Disabled` |
| `--muted` | `ColorFillTertiary`、`ColorFillQuaternary`、`ColorFillContent`、`ColorBgContainerDisabled` |
| `--secondary` | `ColorFillSecondary`（跟随 Base Color） |
| `--accent` / `--accent-foreground` | `ControlItemBgHover`、`ControlItemBgActive`、`ColorFill` / `ControlItemColorActive` |
| `--border` | `ColorBorder`、`ColorBorderSecondary`、`ColorSplit`（暗色为 `#1AFFFFFF`） |
| `--input` | 输入族控件视图的 `ColorBorder`（暗色为 `#26FFFFFF`） |
| `--ring` | 输入族聚焦边框（控件视图的 `ColorPrimary`、`ColorPrimaryHover`）、`ColorControlOutline`（按风格焦点环浓度） |
| `--destructive` | `ColorError`（及其 Ant 派生） |
| 无 | `ColorSuccess`、`ColorWarning`、`ColorInfo` 沿用 Ant 的计算（shadcn 不定义） |

品牌派生：`ColorPrimaryHover/Active` = 主色 80% 合成（按下为位移而非变色）；`ColorPrimaryBg/BgHover/Border/BorderHover`
= 10%/20%/30%/50% 合成（AtomUI 近似，shadcn 无对应变量）。

## 弹层（阴影与描边环）

- 描边环 = `--foreground` 按风格浓度（多数 10%，Maia/Luma/Rhea 浅色 5%、暗色 10%）的 1px 扩散阴影，排在阴影之前。
- 菜单/Select 面板（`Popup`）：环 + 菜单阴影档；带箭头弹层（`FlyoutHost`）：仅 Popover 阴影，`ArrowDecoratedBox` 以环色
  一体描边；Dialog：控件视图的 `BoxShadows` = 环 + Dialog 阴影档；Drawer 四向 = Drawer 阴影档，Vega 贴边一侧 1px 边框；
  遮罩 = `ColorBgMask` 按风格浓度。
- Tooltip：反色、无阴影无环、圆角按风格（Lyra/Sera 直角）。

## 不支持 / 近似（与官方的差异）

| 项 | 说明 |
| --- | --- |
| 全大写与加宽字距（Sera） | 只对齐字号与字重 |
| 焦点外环（边框外 1-3px 半透明环） | 聚焦边框改用 ring 色 |
| 遮罩背景模糊 | 只对齐遮罩颜色与浓度 |
| Popover 圆角 | 弹层框体无圆角 Token，随全局 BorderRadius |
| 子菜单阴影比主菜单高一级（部分风格） | 都用主菜单档 |
| 彩色 Theme 下 `--secondary` 变 zinc 100/800 | 全局 Secondary 跟随 Base Color；按钮族控件视图用 zinc |
| Compact 尺寸梯度 | 按 Ant 比例重算小/大号与字号，非该风格梯度等量再压一级 |
| Nova 等的 Card/Dialog 灰底底栏 | 不做 |
| 字体、图标库 | 非目标 |

## 控件取值

每个控件的取值在取值包里，规则表、不支持清单与各控件的目标等级见
`../AtomUI.Desktop.Controls.Themes.Shadcn/README.md` 等各取值包的 README。控件推导可以在控件视图里覆盖全局 Token（含
`ColorPrimary`、`BorderRadius` 等种子位置的值），只替换该控件看到的那一项，不触发重新派生。
