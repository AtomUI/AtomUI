# AtomUI.Themes.Shadcn

对齐 shadcn/ui 现行版本（2026-11-15 实测）的预设主题包。所有取值以自动生成的取值附录为唯一基准：
`docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-data.md`。

## 三个维度

| 维度 | AtomUI 承载 | 取值 |
| --- | --- | --- |
| Style（设计语言） | 8 个设计语言，每个 4 个 profile（亮/暗 × 常规/紧凑） | Vega、**Nova**（包内默认）、Maia、Lyra、Mira、Luma、Sera、Rhea |
| Theme（品牌色） | `BrandColor="Seed"`，种子写入 `ColorPrimary` | Neutral（跟随 Base Color）+ 17 种彩色（Tailwind 700 级，Lime/Yellow 为 400 级） |
| Base Color（中性色） | `NeutralColor="Seed"`，种子写入 `ColorNeutral` | Neutral、Stone、Zinc、Mauve、Olive、Mist、Taupe（7 种） |

- 颜色全部来自 **Tailwind v4.3.5 色板**（npm 最新版）：shadcn 的颜色变量没有自定义值，都是某个色阶的某一档。
  色板内置于 `ShadcnTailwindPalette`（脚本生成，勿手改），取档规则见 `ShadcnBaseColors`。
- 中性色种子 = 该 Base Color 浅色 `--muted-foreground`（色阶 500 档）；未命中官方 7 档的自定义种子按 OKLCH
  生成 11 档色阶（明度取 Neutral 阶梯、色相取种子、色度按 Zinc 比例缩放）。
- Theme 种子 = 官方浅色主色（700/400 档），明暗两态都能命中官方值；暗色取 800/500 档，前景取 50/900 档。
  自定义主色暗色按官方 17 主题的平均明度差/色度比派生，前景保证与主色对比度 ≥ 4.5。
- Style 的几何（高度/圆角/内边距/字号/焦点环）与弹层阴影档来自 shadcn registry（附录 C），与 Tailwind 无关。
- Compact 是 AtomUI 扩展：`AtomUI:Compact` 在该 Style 尺寸上再压缩一级（高度 -4）。
- profile 的 XML 只写算法链与默认种子（`ColorPrimary=#171717`、`ColorNeutral=#737373`，Neutral 浅色值），
  其余全部由算法给出。

## 颜色映射（Base Color → 全局 Token）

| shadcn 变量 | AtomUI Token |
| --- | --- |
| `--background` | `ColorBgLayout`、`ColorBgContainer`、`ColorBgBase`（种子副本） |
| `--card` | Card 控件级 `ColorBgContainer`（控件阶段覆写） |
| `--popover` | `ColorBgElevated` |
| `--foreground` | `ColorText`、`ColorTextHeading`、`ColorIcon`、`ColorIconHover`、`ColorBgSpotlight`（Tooltip 反色底） |
| `--primary` / `--primary-foreground` | `ColorPrimary` / `ColorTextLightSolid`；实色底 `ColorBgSolid*` 取主色与其 80% 合成 |
| `--muted-foreground` | `ColorTextSecondary`、`ColorTextDescription`、`ColorTextPlaceholder`；50% 合成作 `ColorTextTertiary/Quaternary/Disabled` |
| `--muted` | `ColorFillTertiary`、`ColorFillQuaternary`、`ColorFillContent`、`ColorBgContainerDisabled` |
| `--secondary` | `ColorFillSecondary`（跟随 Base Color，不随 Theme 变化，见下） |
| `--accent` / `--accent-foreground` | `ControlItemBgHover`、`ControlItemBgActive`、`ColorFill` / `ControlItemColorActive` |
| `--border` | `ColorBorder`、`ColorBorderSecondary`、`ColorSplit`（暗色为 `#1AFFFFFF`） |
| `--input` | 输入族控件级 `ColorBorder`（暗色为 `#26FFFFFF`，与 border 不同） |
| `--ring` | 输入族聚焦边框（控件级 `ColorPrimary` 覆写）、`ColorControlOutline`（按 Style 焦点环透明度） |
| `--destructive` | `ColorError`（及其 Ant 派生） |
| 无 | `ColorSuccess`、`ColorWarning`、`ColorInfo` 沿用 AtomUI 算法（shadcn 不定义） |

品牌派生（`hover:bg-primary/80` 等）：`ColorPrimaryHover/Active` = 主色 80% 合成（按下为位移而非变色）；
`ColorPrimaryBg/BgHover/Border/BorderHover` = 10%/20%/30%/50% 合成（AtomUI 近似，shadcn 无对应变量）。

## 弹层（阴影与描边环）

- 描边环 = `--foreground` 按 Style 透明度（多数 10%，Maia/Luma/Rhea 浅色 5%/暗色 10%）的 1px 扩散阴影，
  排在阴影之前（与 CSS 绘制顺序一致）。
- 菜单/Select 面板（`Popup`）：环 + MenuShadow 档；带箭头弹层（`FlyoutHost`）：仅 PopoverShadow，
  `ArrowDecoratedBox` 以环色一体描边（含箭头两侧）；Dialog：控件级 `BoxShadows` = 环 + DialogShadow 档
  （Nova/Vega/Maia/Lyra/Mira 无阴影仅有环；Luma/Rhea xl、Sera md）；Drawer 四向 = DrawerShadow 档，
  贴边一侧可由 `EdgeBorderColor/Thickness` 画 1px 边框；遮罩 = `ColorBgMask` 按 Style 浓度。
- Tooltip：反色（`--foreground` 底 / `--background` 字）、无阴影无环、圆角按 Style（Lyra/Sera 直角）。
- Toast/sonner：官方由 sonner 绘制、registry 无 class，未实测前沿用全局默认（Ant 档），待实测后修正。

## 不支持 / 近似（与官方的差异）

| 项 | 说明 |
| --- | --- |
| 全大写与加宽字距（Sera） | 只对齐字号与字重；大写/字距需文本渲染支持 |
| 下划线输入框（Sera） | 保留四边细边框；应用可用 AtomUI 既有 `Underlined` 变体近似 |
| 焦点外环（边框外 1-3px 半透明环） | 聚焦边框改用 ring 色；外环需改模板，另立项 |
| 遮罩背景模糊 | 只对齐遮罩颜色与浓度 |
| 按钮按下下移 1px | 不做 |
| 禁用 = 整体 50% 透明 | 用禁用色近似；彩色主色下有偏差 |
| Popover 圆角 | 弹层框体无圆角 Token，随全局 BorderRadius（Maia 26 vs 官方 popover 18 等） |
| 子菜单阴影比主菜单高一级（部分 Style） | 都用主菜单档 |
| 彩色 Theme 下 `--secondary` 变 zinc 100/800 | Secondary 跟随 Base Color（差异极小：#F5F5F5 对 #F4F4F5） |
| Compact 尺寸梯度 | `AtomUI:Compact` 按 Ant 比例重算小/大号（SM=75%、XS=50%、LG=125%）与字号，非该 Style 梯度等量再压一级 |
| 追加 `AtomUI:Dark` 的混链 | Map 颜色退化为 Ant 暗色公式（只有 Alias 与控件作用域保留 shadcn 取值）；暗色请用各 Style 的 `.Dark` profile |
| Nova 等的 Card/Dialog 灰底底栏 | 不做 |
| 字体、图标库 | 非目标（主题 Token 之外） |
| 透明色直接使用 | `bg-primary/80` 类合成到背景上为不透明色，避免叠在非白底时偏离官方 |

半透明边框（暗色 `--border`/`--input`）为 alpha 色，直接使用，与官方一致。

## 与其他包的关系

- Ant Design、PrimeNG、Bootstrap、Material 的渲染不受本包影响（Core 的 `ColorNeutral` 种子被它们的算法忽略）。
- 启用：`builder.Theme.UseShadcnThemes()`（注册 16 个 Style 算法、控件阶段与 32 个 profile）。
