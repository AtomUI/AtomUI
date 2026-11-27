# shadcn/ui 现行取值附录（自动生成）

> 生成方式：2026-11-15 从 `https://ui.shadcn.com/r/colors/<base>.json`（`cssVarsV4`）、`https://ui.shadcn.com/r/styles/base-<style>/<component>.json` 与 `tailwindcss@4.3.5/theme.css`（npm 当前最新版） 下载后由脚本生成，hex 由 OKLCH 按 CSS Color 4 换算到 sRGB 并四舍五入；带透明度的值写成 AtomUI 的 `#AARRGGBB`。不要手改本文件，重新生成即可。

## A. Base Color（7 种）

`--radius` 七种均为 `0.625rem`（10px）。表中每格为 `hex`，括号内为官方 OKLCH。

**规则**（已用脚本核对七种全部命中）：每个变量都是 Tailwind v4.3.5 同名色阶的一档。浅色：foreground 950、primary 900、secondary / muted / accent 100、muted-foreground 500、border / input 200、ring 400，background / card / popover 为白色。暗色：background 950、card / popover / primary-foreground 900、primary 200、secondary / muted / accent 800、foreground 50、muted-foreground 400、ring 500，border 为白色 10%、input 为白色 15%。destructive 浅色 red-600、暗色 red-400。Mauve、Olive、Mist、Taupe 是 Tailwind v4.3 新增的色阶，4.1 中没有。

### A.1 浅色 :root

| 变量 | Neutral | Stone | Zinc | Mauve | Olive | Mist | Taupe |
|---|---|---|---|---|---|---|---|
| `--background` | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) |
| `--foreground` | `#0A0A0A` (0.145 0 0) | `#0C0A09` (0.147 0.004 49.25) | `#09090B` (0.141 0.005 285.823) | `#0C090C` (0.145 0.008 326) | `#0C0C09` (0.153 0.006 107.1) | `#090B0C` (0.148 0.004 228.8) | `#0C0A09` (0.147 0.004 49.3) |
| `--card` | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) |
| `--card-foreground` | `#0A0A0A` (0.145 0 0) | `#0C0A09` (0.147 0.004 49.25) | `#09090B` (0.141 0.005 285.823) | `#0C090C` (0.145 0.008 326) | `#0C0C09` (0.153 0.006 107.1) | `#090B0C` (0.148 0.004 228.8) | `#0C0A09` (0.147 0.004 49.3) |
| `--popover` | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) | `#FFFFFF` (1 0 0) |
| `--popover-foreground` | `#0A0A0A` (0.145 0 0) | `#0C0A09` (0.147 0.004 49.25) | `#09090B` (0.141 0.005 285.823) | `#0C090C` (0.145 0.008 326) | `#0C0C09` (0.153 0.006 107.1) | `#090B0C` (0.148 0.004 228.8) | `#0C0A09` (0.147 0.004 49.3) |
| `--primary` | `#171717` (0.205 0 0) | `#1C1917` (0.216 0.006 56.043) | `#18181B` (0.21 0.006 285.885) | `#1D161E` (0.212 0.019 322.12) | `#1D1D16` (0.228 0.013 107.4) | `#161B1D` (0.218 0.008 223.9) | `#1D1816` (0.214 0.009 43.1) |
| `--primary-foreground` | `#FAFAFA` (0.985 0 0) | `#FAFAF9` (0.985 0.001 106.423) | `#FAFAFA` (0.985 0 0) | `#FAFAFA` (0.985 0 0) | `#FBFBF9` (0.988 0.003 106.5) | `#F9FBFB` (0.987 0.002 197.1) | `#FBFAF9` (0.986 0.002 67.8) |
| `--secondary` | `#F5F5F5` (0.97 0 0) | `#F5F5F4` (0.97 0.001 106.424) | `#F4F4F5` (0.967 0.001 286.375) | `#F3F1F3` (0.96 0.003 325.6) | `#F4F4F0` (0.966 0.005 106.5) | `#F1F3F3` (0.963 0.002 197.1) | `#F3F1F1` (0.96 0.002 17.2) |
| `--secondary-foreground` | `#171717` (0.205 0 0) | `#1C1917` (0.216 0.006 56.043) | `#18181B` (0.21 0.006 285.885) | `#1D161E` (0.212 0.019 322.12) | `#1D1D16` (0.228 0.013 107.4) | `#161B1D` (0.218 0.008 223.9) | `#1D1816` (0.214 0.009 43.1) |
| `--muted` | `#F5F5F5` (0.97 0 0) | `#F5F5F4` (0.97 0.001 106.424) | `#F4F4F5` (0.967 0.001 286.375) | `#F3F1F3` (0.96 0.003 325.6) | `#F4F4F0` (0.966 0.005 106.5) | `#F1F3F3` (0.963 0.002 197.1) | `#F3F1F1` (0.96 0.002 17.2) |
| `--muted-foreground` | `#737373` (0.556 0 0) | `#79716B` (0.553 0.013 58.071) | `#71717B` (0.552 0.016 285.938) | `#79697B` (0.542 0.034 322.5) | `#7C7C67` (0.58 0.031 107.3) | `#67787C` (0.56 0.021 213.5) | `#7C6D67` (0.547 0.021 43.1) |
| `--accent` | `#F5F5F5` (0.97 0 0) | `#F5F5F4` (0.97 0.001 106.424) | `#F4F4F5` (0.967 0.001 286.375) | `#F3F1F3` (0.96 0.003 325.6) | `#F4F4F0` (0.966 0.005 106.5) | `#F1F3F3` (0.963 0.002 197.1) | `#F3F1F1` (0.96 0.002 17.2) |
| `--accent-foreground` | `#171717` (0.205 0 0) | `#1C1917` (0.216 0.006 56.043) | `#18181B` (0.21 0.006 285.885) | `#1D161E` (0.212 0.019 322.12) | `#1D1D16` (0.228 0.013 107.4) | `#161B1D` (0.218 0.008 223.9) | `#1D1816` (0.214 0.009 43.1) |
| `--destructive` | `#E7000B` (0.577 0.245 27.325) | `#E7000B` (0.577 0.245 27.325) | `#E7000B` (0.577 0.245 27.325) | `#E7000B` (0.577 0.245 27.325) | `#E7000B` (0.577 0.245 27.325) | `#E7000B` (0.577 0.245 27.325) | `#E7000B` (0.577 0.245 27.325) |
| `--border` | `#E5E5E5` (0.922 0 0) | `#E7E5E4` (0.923 0.003 48.717) | `#E4E4E7` (0.92 0.004 286.32) | `#E7E4E7` (0.922 0.005 325.62) | `#E8E8E3` (0.93 0.007 106.5) | `#E3E7E8` (0.925 0.005 214.3) | `#E8E4E3` (0.922 0.005 34.3) |
| `--input` | `#E5E5E5` (0.922 0 0) | `#E7E5E4` (0.923 0.003 48.717) | `#E4E4E7` (0.92 0.004 286.32) | `#E7E4E7` (0.922 0.005 325.62) | `#E8E8E3` (0.93 0.007 106.5) | `#E3E7E8` (0.925 0.005 214.3) | `#E8E4E3` (0.922 0.005 34.3) |
| `--ring` | `#A1A1A1` (0.708 0 0) | `#A6A09B` (0.709 0.01 56.259) | `#9F9FA9` (0.705 0.015 286.067) | `#A89EA9` (0.711 0.019 323.02) | `#ABAB9C` (0.737 0.021 106.9) | `#9CA8AB` (0.723 0.014 214.4) | `#ABA09C` (0.714 0.014 41.2) |

### A.2 暗色 .dark

| 变量 | Neutral | Stone | Zinc | Mauve | Olive | Mist | Taupe |
|---|---|---|---|---|---|---|---|
| `--background` | `#0A0A0A` (0.145 0 0) | `#0C0A09` (0.147 0.004 49.25) | `#09090B` (0.141 0.005 285.823) | `#0C090C` (0.145 0.008 326) | `#0C0C09` (0.153 0.006 107.1) | `#090B0C` (0.148 0.004 228.8) | `#0C0A09` (0.147 0.004 49.3) |
| `--foreground` | `#FAFAFA` (0.985 0 0) | `#FAFAF9` (0.985 0.001 106.423) | `#FAFAFA` (0.985 0 0) | `#FAFAFA` (0.985 0 0) | `#FBFBF9` (0.988 0.003 106.5) | `#F9FBFB` (0.987 0.002 197.1) | `#FBFAF9` (0.986 0.002 67.8) |
| `--card` | `#171717` (0.205 0 0) | `#1C1917` (0.216 0.006 56.043) | `#18181B` (0.21 0.006 285.885) | `#1D161E` (0.212 0.019 322.12) | `#1D1D16` (0.228 0.013 107.4) | `#161B1D` (0.218 0.008 223.9) | `#1D1816` (0.214 0.009 43.1) |
| `--card-foreground` | `#FAFAFA` (0.985 0 0) | `#FAFAF9` (0.985 0.001 106.423) | `#FAFAFA` (0.985 0 0) | `#FAFAFA` (0.985 0 0) | `#FBFBF9` (0.988 0.003 106.5) | `#F9FBFB` (0.987 0.002 197.1) | `#FBFAF9` (0.986 0.002 67.8) |
| `--popover` | `#171717` (0.205 0 0) | `#1C1917` (0.216 0.006 56.043) | `#18181B` (0.21 0.006 285.885) | `#1D161E` (0.212 0.019 322.12) | `#1D1D16` (0.228 0.013 107.4) | `#161B1D` (0.218 0.008 223.9) | `#1D1816` (0.214 0.009 43.1) |
| `--popover-foreground` | `#FAFAFA` (0.985 0 0) | `#FAFAF9` (0.985 0.001 106.423) | `#FAFAFA` (0.985 0 0) | `#FAFAFA` (0.985 0 0) | `#FBFBF9` (0.988 0.003 106.5) | `#F9FBFB` (0.987 0.002 197.1) | `#FBFAF9` (0.986 0.002 67.8) |
| `--primary` | `#E5E5E5` (0.922 0 0) | `#E7E5E4` (0.923 0.003 48.717) | `#E4E4E7` (0.92 0.004 286.32) | `#E7E4E7` (0.922 0.005 325.62) | `#E8E8E3` (0.93 0.007 106.5) | `#E3E7E8` (0.925 0.005 214.3) | `#E8E4E3` (0.922 0.005 34.3) |
| `--primary-foreground` | `#171717` (0.205 0 0) | `#1C1917` (0.216 0.006 56.043) | `#18181B` (0.21 0.006 285.885) | `#1D161E` (0.212 0.019 322.12) | `#1D1D16` (0.228 0.013 107.4) | `#161B1D` (0.218 0.008 223.9) | `#1D1816` (0.214 0.009 43.1) |
| `--secondary` | `#262626` (0.269 0 0) | `#292524` (0.268 0.007 34.298) | `#27272A` (0.274 0.006 286.033) | `#2A212C` (0.263 0.024 320.12) | `#2B2B22` (0.286 0.016 107.4) | `#22292B` (0.275 0.011 216.9) | `#2B2422` (0.268 0.011 36.5) |
| `--secondary-foreground` | `#FAFAFA` (0.985 0 0) | `#FAFAF9` (0.985 0.001 106.423) | `#FAFAFA` (0.985 0 0) | `#FAFAFA` (0.985 0 0) | `#FBFBF9` (0.988 0.003 106.5) | `#F9FBFB` (0.987 0.002 197.1) | `#FBFAF9` (0.986 0.002 67.8) |
| `--muted` | `#262626` (0.269 0 0) | `#292524` (0.268 0.007 34.298) | `#27272A` (0.274 0.006 286.033) | `#2A212C` (0.263 0.024 320.12) | `#2B2B22` (0.286 0.016 107.4) | `#22292B` (0.275 0.011 216.9) | `#2B2422` (0.268 0.011 36.5) |
| `--muted-foreground` | `#A1A1A1` (0.708 0 0) | `#A6A09B` (0.709 0.01 56.259) | `#9F9FA9` (0.705 0.015 286.067) | `#A89EA9` (0.711 0.019 323.02) | `#ABAB9C` (0.737 0.021 106.9) | `#9CA8AB` (0.723 0.014 214.4) | `#ABA09C` (0.714 0.014 41.2) |
| `--accent` | `#262626` (0.269 0 0) | `#292524` (0.268 0.007 34.298) | `#27272A` (0.274 0.006 286.033) | `#2A212C` (0.263 0.024 320.12) | `#2B2B22` (0.286 0.016 107.4) | `#22292B` (0.275 0.011 216.9) | `#2B2422` (0.268 0.011 36.5) |
| `--accent-foreground` | `#FAFAFA` (0.985 0 0) | `#FAFAF9` (0.985 0.001 106.423) | `#FAFAFA` (0.985 0 0) | `#FAFAFA` (0.985 0 0) | `#FBFBF9` (0.988 0.003 106.5) | `#F9FBFB` (0.987 0.002 197.1) | `#FBFAF9` (0.986 0.002 67.8) |
| `--destructive` | `#FF6467` (0.704 0.191 22.216) | `#FF6467` (0.704 0.191 22.216) | `#FF6467` (0.704 0.191 22.216) | `#FF6467` (0.704 0.191 22.216) | `#FF6467` (0.704 0.191 22.216) | `#FF6467` (0.704 0.191 22.216) | `#FF6467` (0.704 0.191 22.216) |
| `--border` | `#1AFFFFFF` (1 0 0 / 10%) | `#1AFFFFFF` (1 0 0 / 10%) | `#1AFFFFFF` (1 0 0 / 10%) | `#1AFFFFFF` (1 0 0 / 10%) | `#1AFFFFFF` (1 0 0 / 10%) | `#1AFFFFFF` (1 0 0 / 10%) | `#1AFFFFFF` (1 0 0 / 10%) |
| `--input` | `#26FFFFFF` (1 0 0 / 15%) | `#26FFFFFF` (1 0 0 / 15%) | `#26FFFFFF` (1 0 0 / 15%) | `#26FFFFFF` (1 0 0 / 15%) | `#26FFFFFF` (1 0 0 / 15%) | `#26FFFFFF` (1 0 0 / 15%) | `#26FFFFFF` (1 0 0 / 15%) |
| `--ring` | `#737373` (0.556 0 0) | `#79716B` (0.553 0.013 58.071) | `#71717B` (0.552 0.016 285.938) | `#79697B` (0.542 0.034 322.5) | `#7C7C67` (0.58 0.031 107.3) | `#67787C` (0.56 0.021 213.5) | `#7C6D67` (0.547 0.021 43.1) |

## B. Theme（17 种彩色主色）

规则（已对 `/create` 实测值逐项校验，全部一致）：浅色 `--primary` = Tailwind v4 的 700 级、暗色 = 800 级、`--primary-foreground` = 50 级；Lime 与 Yellow 例外：浅色 400 级、暗色 500 级、前景 900 级。`--ring`、`--accent`、`--muted` 等其余变量不随 Theme 变化（`--secondary` 改为 zinc 的 100 / 暗色 zinc 的 800 档，见设计文档 6.3）。

| Theme | 浅色 primary | 浅色 foreground | 暗色 primary | 暗色 foreground | 校验 |
|---|---|---|---|---|---|
| Amber | `#BB4D00` (amber-700) | `#FFFBEB` (amber-50) | `#973C00` (amber-800) | `#FFFBEB` (amber-50) | 一致 |
| Blue | `#1447E6` (blue-700) | `#EFF6FF` (blue-50) | `#193CB8` (blue-800) | `#EFF6FF` (blue-50) | 一致 |
| Cyan | `#007595` (cyan-700) | `#ECFEFF` (cyan-50) | `#005F78` (cyan-800) | `#ECFEFF` (cyan-50) | 一致 |
| Emerald | `#007A55` (emerald-700) | `#ECFDF5` (emerald-50) | `#006045` (emerald-800) | `#ECFDF5` (emerald-50) | 一致 |
| Fuchsia | `#A800B7` (fuchsia-700) | `#FDF4FF` (fuchsia-50) | `#8A0194` (fuchsia-800) | `#FDF4FF` (fuchsia-50) | 一致 |
| Green | `#008236` (green-700) | `#F0FDF4` (green-50) | `#016630` (green-800) | `#F0FDF4` (green-50) | 一致 |
| Indigo | `#432DD7` (indigo-700) | `#EEF2FF` (indigo-50) | `#372AAC` (indigo-800) | `#EEF2FF` (indigo-50) | 一致 |
| Lime | `#9AE600` (lime-400) | `#35530E` (lime-900) | `#7CCF00` (lime-500) | `#35530E` (lime-900) | 一致 |
| Orange | `#CA3500` (orange-700) | `#FFF7ED` (orange-50) | `#9F2D00` (orange-800) | `#FFF7ED` (orange-50) | 一致 |
| Pink | `#C6005C` (pink-700) | `#FDF2F8` (pink-50) | `#A3004C` (pink-800) | `#FDF2F8` (pink-50) | 一致 |
| Purple | `#8200DB` (purple-700) | `#FAF5FF` (purple-50) | `#6E11B0` (purple-800) | `#FAF5FF` (purple-50) | 一致 |
| Red | `#C10007` (red-700) | `#FEF2F2` (red-50) | `#9F0712` (red-800) | `#FEF2F2` (red-50) | 一致 |
| Rose | `#C70036` (rose-700) | `#FFF1F2` (rose-50) | `#A50036` (rose-800) | `#FFF1F2` (rose-50) | 一致 |
| Sky | `#0069A8` (sky-700) | `#F0F9FF` (sky-50) | `#00598A` (sky-800) | `#F0F9FF` (sky-50) | 一致 |
| Teal | `#00786F` (teal-700) | `#F0FDFA` (teal-50) | `#005F5A` (teal-800) | `#F0FDFA` (teal-50) | 一致 |
| Violet | `#7008E7` (violet-700) | `#F5F3FF` (violet-50) | `#5D0EC0` (violet-800) | `#F5F3FF` (violet-50) | 一致 |
| Yellow | `#FDC700` (yellow-400) | `#733E0A` (yellow-900) | `#F0B100` (yellow-500) | `#733E0A` (yellow-900) | 一致 |

## C. 各 Style 组件的区别性 class

每个组件只列出该 Style 与其余 Style 不同的 utility（8 种 Style 共有的 class 已去掉，状态与动画类已过滤）。完整源码以 registry JSON 为准。

### button

| Style | 区别性 class |
|---|---|
| Vega | `rounded-md` `text-sm` `font-medium` `focus-visible:ring-3` `focus-visible:ring-ring/50` `bg-background` `shadow-xs` `dark:border-input` `dark:bg-input/30` `dark:hover:bg-input/50` `h-9` `gap-1.5` `px-2.5` `h-6` `rounded-[min(var(--radius-md),8px)]` `px-2` `text-xs` `h-8` `rounded-[min(var(--radius-md),10px)]` `h-10` `size-9` `size-6` `size-8` `size-10` |
| Nova | `rounded-lg` `text-sm` `font-medium` `focus-visible:ring-3` `focus-visible:ring-ring/50` `bg-background` `dark:border-input` `dark:bg-input/30` `dark:hover:bg-input/50` `h-8` `gap-1.5` `px-2.5` `h-6` `rounded-[min(var(--radius-md),10px)]` `px-2` `text-xs` `h-7` `rounded-[min(var(--radius-md),12px)]` `text-[0.8rem]` `h-9` `size-8` `size-6` `size-7` `size-9` |
| Maia | `rounded-4xl` `text-sm` `font-medium` `focus-visible:ring-[3px]` `focus-visible:ring-ring/50` `bg-input/30` `hover:bg-input/50` `h-9` `gap-1.5` `px-3` `h-6` `px-2.5` `text-xs` `h-8` `h-10` `px-4` `size-9` `size-6` `size-8` `size-10` |
| Lyra | `rounded-none` `text-xs` `font-medium` `focus-visible:ring-1` `focus-visible:ring-ring/50` `bg-background` `dark:border-input` `dark:bg-input/30` `dark:hover:bg-input/50` `h-8` `gap-1.5` `px-2.5` `h-6` `px-2` `h-7` `h-9` `size-8` `size-6` `size-7` `size-9` |
| Mira | `rounded-md` `text-xs/relaxed` `font-medium` `focus-visible:ring-2` `focus-visible:ring-ring/30` `hover:bg-input/50` `dark:bg-input/30` `h-7` `px-2` `h-5` `rounded-sm` `text-[0.625rem]` `h-6` `h-8` `px-2.5` `size-7` `size-5` `size-6` `size-8` |
| Luma | `rounded-4xl` `text-sm` `font-medium` `focus-visible:ring-3` `focus-visible:ring-ring/30` `bg-background` `dark:bg-transparent` `dark:hover:bg-input/30` `h-9` `gap-1.5` `px-3` `h-6` `px-2.5` `text-xs` `h-8` `h-10` `px-4` `size-9` `size-6` `size-8` `size-10` |
| Sera | `rounded-none` `text-xs` `font-semibold` `tracking-widest` `uppercase` `focus-visible:ring-2` `focus-visible:ring-ring/30` `bg-transparent` `dark:hover:bg-input/30` `h-10` `gap-1.5` `px-6` `h-7` `px-3` `h-9` `px-4` `h-11` `px-8` `size-10` `size-7` `size-9` `size-11` |
| Rhea | `rounded-2xl` `text-sm` `font-medium` `focus-visible:ring-3` `focus-visible:ring-ring/30` `bg-background` `dark:bg-transparent` `dark:hover:bg-input/30` `h-8` `gap-1.5` `px-3` `h-6` `px-2.5` `text-xs` `h-7` `h-9` `px-4` `size-8` `size-6` `size-7` `size-9` |

### input

| Style | 区别性 class |
|---|---|
| Vega | `h-9` `rounded-md` `border-input` `bg-transparent` `px-2.5` `py-1` `text-base` `shadow-xs` `file:h-7` `file:text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/50` `md:text-sm` `dark:bg-input/30` |
| Nova | `h-8` `rounded-lg` `border-input` `bg-transparent` `px-2.5` `py-1` `text-base` `file:h-6` `file:text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/50` `disabled:bg-input/50` `md:text-sm` `dark:bg-input/30` `dark:disabled:bg-input/80` |
| Maia | `h-9` `rounded-4xl` `border-input` `bg-input/30` `px-3` `py-1` `text-base` `file:h-7` `file:text-sm` `focus-visible:border-ring` `focus-visible:ring-[3px]` `focus-visible:ring-ring/50` `md:text-sm` |
| Lyra | `h-8` `rounded-none` `border-input` `bg-transparent` `px-2.5` `py-1` `text-xs` `file:h-6` `file:text-xs` `focus-visible:border-ring` `focus-visible:ring-1` `focus-visible:ring-ring/50` `disabled:bg-input/50` `md:text-xs` `dark:bg-input/30` `dark:disabled:bg-input/80` |
| Mira | `h-7` `rounded-md` `border-input` `bg-input/20` `px-2` `py-0.5` `text-sm` `file:h-6` `file:text-xs/relaxed` `focus-visible:border-ring` `focus-visible:ring-2` `focus-visible:ring-ring/30` `md:text-xs/relaxed` `dark:bg-input/30` |
| Luma | `h-9` `rounded-3xl` `border-transparent` `bg-input/50` `px-3` `py-1` `text-base` `file:h-7` `file:text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/30` `md:text-sm` |
| Sera | `h-10` `border-transparent` `border-b-input` `bg-transparent` `px-0` `py-1` `text-base` `file:h-7` `file:text-sm` `focus-visible:border-b-ring` `md:text-sm` |
| Rhea | `h-8` `rounded-2xl` `border-transparent` `bg-input/50` `px-2.5` `py-1` `text-base` `file:h-6` `file:text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/30` `md:text-sm` |

### textarea

| Style | 区别性 class |
|---|---|
| Vega | `rounded-md` `border-input` `bg-transparent` `px-2.5` `py-2` `text-base` `shadow-xs` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/50` `md:text-sm` `dark:bg-input/30` |
| Nova | `rounded-lg` `border-input` `bg-transparent` `px-2.5` `py-2` `text-base` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/50` `disabled:bg-input/50` `md:text-sm` `dark:bg-input/30` `dark:disabled:bg-input/80` |
| Maia | `resize-none` `rounded-xl` `border-input` `bg-input/30` `px-3` `py-3` `text-base` `focus-visible:border-ring` `focus-visible:ring-[3px]` `focus-visible:ring-ring/50` `md:text-sm` |
| Lyra | `rounded-none` `border-input` `bg-transparent` `px-2.5` `py-2` `text-xs` `focus-visible:border-ring` `focus-visible:ring-1` `focus-visible:ring-ring/50` `disabled:bg-input/50` `md:text-xs` `dark:bg-input/30` `dark:disabled:bg-input/80` |
| Mira | `resize-none` `rounded-md` `border-input` `bg-input/20` `px-2` `py-2` `text-sm` `focus-visible:border-ring` `focus-visible:ring-2` `focus-visible:ring-ring/30` `md:text-xs/relaxed` `dark:bg-input/30` |
| Luma | `resize-none` `rounded-2xl` `border-transparent` `bg-input/50` `px-3` `py-3` `text-base` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/30` `md:text-sm` |
| Sera | `resize-none` `rounded-none` `border-transparent` `border-b-input` `bg-transparent` `px-0` `py-3` `text-base` `focus-visible:border-b-ring` `md:text-sm` |
| Rhea | `resize-none` `rounded-2xl` `border-transparent` `bg-input/50` `px-2.5` `py-2` `text-base` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/30` `md:text-sm` |

### select

| Style | 区别性 class |
|---|---|
| Vega | `scroll-my-1` `p-1` `rounded-md` `border-input` `bg-transparent` `py-2` `pr-2` `pl-2.5` `text-sm` `shadow-xs` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/50` `data-[size=default]:h-9` `data-[size=sm]:h-8` `dark:bg-input/30` `dark:hover:bg-input/50` `size-4` `min-w-36` `shadow-md` `ring-foreground/10` `px-2` `py-1.5` `rounded-sm` `pr-8` `pl-2` `-mx-1` `my-1` `bg-border` |
| Nova | `scroll-my-1` `p-1` `rounded-lg` `border-input` `bg-transparent` `py-2` `pr-2` `pl-2.5` `text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/50` `data-[size=default]:h-8` `data-[size=sm]:h-7` `data-[size=sm]:rounded-[min(var(--radius-md),10px)]` `dark:bg-input/30` `dark:hover:bg-input/50` `size-4` `min-w-36` `shadow-md` `ring-foreground/10` `px-1.5` `rounded-md` `pr-8` `pl-1.5` `-mx-1` `my-1` `bg-border` |
| Maia | `scroll-my-1` `p-1` `rounded-4xl` `border-input` `bg-input/30` `px-3` `py-2` `text-sm` `focus-visible:border-ring` `focus-visible:ring-[3px]` `focus-visible:ring-ring/50` `data-[size=default]:h-9` `data-[size=sm]:h-8` `dark:hover:bg-input/50` `size-4` `min-w-36` `rounded-2xl` `shadow-2xl` `ring-foreground/5` `py-2.5` `gap-2.5` `rounded-xl` `pr-8` `pl-3` `-mx-1` `my-1` `bg-border/50` |
| Lyra | `scroll-my-1` `rounded-none` `border-input` `bg-transparent` `py-2` `pr-2` `pl-2.5` `focus-visible:border-ring` `focus-visible:ring-1` `focus-visible:ring-ring/50` `data-[size=default]:h-8` `data-[size=sm]:h-7` `data-[size=sm]:rounded-none` `dark:bg-input/30` `dark:hover:bg-input/50` `size-4` `min-w-36` `shadow-md` `ring-foreground/10` `px-2` `pr-8` `pl-2` `-mx-1` `bg-border` |
| Mira | `scroll-my-1` `p-1` `rounded-md` `border-input` `bg-input/20` `px-2` `py-1.5` `text-xs/relaxed` `focus-visible:border-ring` `focus-visible:ring-2` `focus-visible:ring-ring/30` `data-[size=default]:h-7` `data-[size=sm]:h-6` `dark:bg-input/30` `dark:hover:bg-input/50` `size-3.5` `min-w-32` `rounded-lg` `shadow-md` `ring-foreground/10` `min-h-7` `-mx-1` `my-1` `bg-border/50` |
| Luma | `scroll-my-1.5` `p-1.5` `rounded-3xl` `border-transparent` `bg-input/50` `px-3` `py-2` `text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/30` `data-[size=default]:h-9` `data-[size=sm]:h-8` `size-4` `min-w-36` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `py-2.5` `gap-2.5` `rounded-2xl` `pr-8` `pl-3` `font-medium` `-mx-1.5` `my-1.5` `bg-border` |
| Sera | `scroll-my-1.5` `p-1.5` `rounded-none` `border-transparent` `border-b-input` `bg-transparent` `px-0` `py-2` `text-sm` `focus-visible:border-b-ring` `data-[size=default]:h-10` `data-[size=sm]:h-9` `size-3.5` `min-w-36` `shadow-md` `ring-foreground/10` `px-3` `font-semibold` `tracking-wider` `uppercase` `gap-2.5` `pr-8` `pl-3` `size-4` `-mx-1.5` `my-1.5` `bg-border/50` |
| Rhea | `scroll-my-1.5` `p-1` `rounded-2xl` `border-transparent` `bg-input/50` `px-3` `py-2` `text-sm` `focus-visible:border-ring` `focus-visible:ring-3` `focus-visible:ring-ring/30` `data-[size=default]:h-8` `data-[size=sm]:h-7` `size-4` `min-w-36` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `px-2` `min-h-7` `rounded-xl` `py-1.5` `pr-8` `pl-2` `-mx-1` `my-1` `bg-border` |

### combobox

| Style | 区别性 class |
|---|---|
| Vega | `@/registry/base-vega/ui/button` `@/registry/base-vega/ui/input-group` `size-4` `rounded-md` `shadow-md` `ring-foreground/10` `*:data-[slot=input-group]:m-1` `*:data-[slot=input-group]:h-8` `*:data-[slot=input-group]:border-input/30` `*:data-[slot=input-group]:bg-input/30` `*:data-[slot=input-group]:shadow-none` `scroll-py-1` `p-1` `gap-2` `rounded-sm` `py-1.5` `pr-8` `pl-2` `text-sm` `-mx-1` `my-1` `bg-border` `min-h-9` `gap-1.5` `border-input` `bg-transparent` `px-2.5` `shadow-xs` `focus-within:border-ring` `focus-within:ring-3` `focus-within:ring-ring/50` `has-data-[slot=combobox-chip]:px-1.5` `dark:bg-input/30` `h-[calc(--spacing(5.5))]` `bg-muted` `px-1.5` `has-data-[slot=combobox-chip-remove]:pr-0` `-ml-1` |
| Nova | `@/registry/base-nova/ui/button` `@/registry/base-nova/ui/input-group` `size-4` `rounded-lg` `shadow-md` `ring-foreground/10` `*:data-[slot=input-group]:m-1` `*:data-[slot=input-group]:h-8` `*:data-[slot=input-group]:border-input/30` `*:data-[slot=input-group]:bg-input/30` `*:data-[slot=input-group]:shadow-none` `scroll-py-1` `p-1` `gap-2` `rounded-md` `py-1` `pr-8` `pl-1.5` `text-sm` `py-1.5` `-mx-1` `my-1` `bg-border` `min-h-8` `border-input` `bg-transparent` `px-2.5` `focus-within:border-ring` `focus-within:ring-3` `focus-within:ring-ring/50` `has-data-[slot=combobox-chip]:px-1` `dark:bg-input/30` `h-[calc(--spacing(5.25))]` `rounded-sm` `bg-muted` `px-1.5` `has-data-[slot=combobox-chip-remove]:pr-0` `-ml-1` |
| Maia | `@/registry/base-maia/ui/button` `@/registry/base-maia/ui/input-group` `size-4` `rounded-2xl` `shadow-2xl` `ring-foreground/5` `*:data-[slot=input-group]:m-1` `*:data-[slot=input-group]:h-9` `*:data-[slot=input-group]:border-none` `*:data-[slot=input-group]:bg-input/30` `*:data-[slot=input-group]:shadow-none` `scroll-py-1` `p-1` `gap-2.5` `rounded-xl` `pr-8` `pl-3` `text-sm` `px-3.5` `py-2.5` `-mx-1` `my-1` `bg-border/50` `min-h-9` `gap-1.5` `rounded-4xl` `border-input` `bg-input/30` `px-2.5` `py-1.5` `focus-within:border-ring` `focus-within:ring-[3px]` `focus-within:ring-ring/50` `has-data-[slot=combobox-chip]:px-1.5` `h-[calc(--spacing(5.5))]` `bg-muted-foreground/10` `has-data-[slot=combobox-chip-remove]:pr-0` `-ml-1` |
| Lyra | `@/registry/base-lyra/ui/button` `@/registry/base-lyra/ui/input-group` `size-4` `rounded-none` `shadow-md` `ring-foreground/10` `*:data-[slot=input-group]:m-1` `*:data-[slot=input-group]:h-8` `*:data-[slot=input-group]:border-input/30` `*:data-[slot=input-group]:bg-input/30` `*:data-[slot=input-group]:shadow-none` `scroll-py-1` `gap-2` `pr-8` `pl-2` `-mx-1` `bg-border` `min-h-8` `border-input` `bg-transparent` `px-2.5` `py-1` `focus-within:border-ring` `focus-within:ring-1` `focus-within:ring-ring/50` `has-data-[slot=combobox-chip]:px-1` `dark:bg-input/30` `h-[calc(--spacing(5.25))]` `bg-muted` `px-1.5` `has-data-[slot=combobox-chip-remove]:pr-0` `-ml-1` |
| Mira | `@/registry/base-mira/ui/button` `@/registry/base-mira/ui/input-group` `size-3.5` `rounded-lg` `shadow-md` `ring-foreground/10` `*:data-[slot=input-group]:m-1` `*:data-[slot=input-group]:h-7` `*:data-[slot=input-group]:border-none` `*:data-[slot=input-group]:bg-input/20` `*:data-[slot=input-group]:shadow-none` `dark:bg-popover` `scroll-py-1` `p-1` `min-h-7` `gap-2` `rounded-md` `py-1` `text-xs/relaxed` `py-1.5` `-mx-1` `my-1` `bg-border/50` `border-input` `bg-input/20` `py-0.5` `focus-within:border-ring` `focus-within:ring-2` `focus-within:ring-ring/30` `has-data-[slot=combobox-chip]:px-1` `dark:bg-input/30` `h-[calc(--spacing(4.75))]` `rounded-[calc(var(--radius-sm)-2px)]` `bg-muted-foreground/10` `px-1.5` `has-data-[slot=combobox-chip-remove]:pr-0` `-ml-1` |
| Luma | `@/registry/base-luma/ui/button` `@/registry/base-luma/ui/input-group` `size-4` `rounded-3xl` `shadow-lg` `ring-foreground/5` `*:data-[slot=input-group]:m-1.5` `*:data-[slot=input-group]:h-8` `*:data-[slot=input-group]:border-input/30` `*:data-[slot=input-group]:bg-input/50` `*:data-[slot=input-group]:shadow-none` `dark:ring-foreground/10` `scroll-py-1.5` `p-1.5` `gap-2.5` `rounded-2xl` `pr-8` `pl-3` `text-sm` `px-3` `py-2.5` `-mx-1.5` `my-1.5` `bg-border` `min-h-9` `gap-1.5` `border-transparent` `bg-input/50` `py-1.5` `focus-within:border-ring` `focus-within:ring-3` `focus-within:ring-ring/30` `has-data-[slot=combobox-chip]:px-1.5` `h-[calc(--spacing(5.5))]` `bg-input` `has-data-[slot=combobox-chip-remove]:pr-0` `dark:bg-input/60` `-ml-1` |
| Sera | `@/registry/base-sera/ui/button` `@/registry/base-sera/ui/input-group` `size-3.5` `rounded-none` `shadow-md` `ring-foreground/10` `*:data-[slot=input-group]:m-1.5` `*:data-[slot=input-group]:h-8` `*:data-[slot=input-group]:border-transparent` `*:data-[slot=input-group]:bg-transparent` `*:data-[slot=input-group]:px-2.5` `*:data-[slot=input-group]:focus-within:border-transparent` `scroll-py-1.5` `p-1.5` `gap-2.5` `pr-8` `pl-3` `text-sm` `size-4` `px-3` `font-semibold` `tracking-wider` `uppercase` `-mx-1.5` `my-1.5` `bg-border/50` `min-h-10` `gap-1.5` `border-transparent` `border-b-input` `bg-transparent` `px-0` `py-1.5` `focus-within:border-b-ring` `has-data-[slot=combobox-chip]:px-0` `h-[calc(--spacing(5.5))]` `bg-muted` `has-data-[slot=combobox-chip-remove]:pr-0` `-ml-1` |
| Rhea | `@/registry/base-rhea/ui/button` `@/registry/base-rhea/ui/input-group` `size-4` `rounded-2xl` `shadow-lg` `ring-foreground/5` `*:data-[slot=input-group]:m-1` `*:data-[slot=input-group]:h-8` `*:data-[slot=input-group]:border-input/30` `*:data-[slot=input-group]:bg-input/50` `*:data-[slot=input-group]:shadow-none` `dark:ring-foreground/10` `scroll-py-1` `p-1` `min-h-7` `gap-2` `rounded-xl` `py-1.5` `pr-8` `pl-2` `text-sm` `-mx-1` `my-1` `bg-border` `min-h-8` `border-transparent` `bg-input/50` `px-2.5` `py-1` `focus-within:border-ring` `focus-within:ring-3` `focus-within:ring-ring/30` `has-data-[slot=combobox-chip]:px-1` `h-[calc(--spacing(5.25))]` `bg-input` `px-1.5` `has-data-[slot=combobox-chip-remove]:pr-0.5` `dark:bg-input/60` `-ml-0.5` `size-4.5` `aria-disabled:pointer-events-none` |

### checkbox

| Style | 区别性 class |
|---|---|
| Vega | `size-4` `rounded-[4px]` `border-input` `shadow-xs` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` |
| Nova | `size-4` `rounded-[4px]` `border-input` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` |
| Maia | `size-4` `rounded-[6px]` `border-input` `focus-visible:ring-[3px]` `focus-visible:ring-ring/50` `dark:bg-input/30` |
| Lyra | `size-4` `rounded-none` `border-input` `focus-visible:ring-1` `focus-visible:ring-ring/50` `dark:bg-input/30` |
| Mira | `size-4` `rounded-[4px]` `border-input` `focus-visible:ring-2` `focus-visible:ring-ring/30` `dark:bg-input/30` |
| Luma | `size-4` `rounded-[5px]` `border-transparent` `bg-input/90` `focus-visible:ring-3` `focus-visible:ring-ring/30` |
| Sera | `size-4.5` `rounded-none` `border-input` `bg-transparent` `focus-visible:ring-2` `focus-visible:ring-ring/30` |
| Rhea | `size-4` `rounded-[5px]` `border-transparent` `bg-input/90` `focus-visible:ring-3` `focus-visible:ring-ring/30` |

### radio-group

| Style | 区别性 class |
|---|---|
| Vega | `gap-3` `size-4` `border-input` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` `data-checked:border-primary` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` |
| Nova | `gap-2` `size-4` `border-input` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` `data-checked:border-primary` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` |
| Maia | `gap-3` `size-4` `border-input` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` `data-checked:border-primary` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` |
| Lyra | `gap-2` `size-4` `border-input` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` `data-checked:border-primary` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` |
| Mira | `gap-3` `size-4` `border-input` `focus-visible:ring-3` `focus-visible:ring-ring/50` `dark:bg-input/30` `data-checked:border-primary` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` |
| Luma | `gap-3` `size-4` `border-transparent` `bg-input/90` `focus-visible:ring-3` `focus-visible:ring-ring/30` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` `dark:size-2.5` |
| Sera | `gap-3` `size-4.5` `border-input` `bg-transparent` `focus-visible:ring-2` `focus-visible:ring-ring/30` `data-checked:border-foreground` `bg-foreground` |
| Rhea | `gap-3` `size-4` `rounded-2xl` `border-transparent` `bg-input/90` `focus-visible:ring-3` `focus-visible:ring-ring/30` `data-checked:bg-primary` `data-checked:text-primary-foreground` `dark:data-checked:bg-primary` `bg-primary-foreground` `dark:size-2.5` |

### switch

| Style | 区别性 class |
|---|---|
| Vega | `rounded-full` `border` `border-transparent` `shadow-xs` `focus-visible:ring-3` `focus-visible:ring-ring/50` `data-[size=default]:h-[18.4px]` `data-[size=default]:w-[32px]` `data-[size=sm]:h-[14px]` `data-[size=sm]:w-[24px]` `data-unchecked:bg-input` `dark:data-unchecked:bg-input/80` |
| Nova | `rounded-full` `border` `border-transparent` `focus-visible:ring-3` `focus-visible:ring-ring/50` `data-[size=default]:h-[18.4px]` `data-[size=default]:w-[32px]` `data-[size=sm]:h-[14px]` `data-[size=sm]:w-[24px]` `data-unchecked:bg-input` `dark:data-unchecked:bg-input/80` |
| Maia | `rounded-full` `border` `border-transparent` `focus-visible:ring-[3px]` `focus-visible:ring-ring/50` `data-[size=default]:h-[18.4px]` `data-[size=default]:w-[32px]` `data-[size=sm]:h-[14px]` `data-[size=sm]:w-[24px]` `data-unchecked:bg-input` `dark:data-unchecked:bg-input/80` |
| Lyra | `rounded-full` `border` `border-transparent` `focus-visible:ring-1` `focus-visible:ring-ring/50` `data-[size=default]:h-[18.4px]` `data-[size=default]:w-[32px]` `data-[size=sm]:h-[14px]` `data-[size=sm]:w-[24px]` `data-unchecked:bg-input` `dark:data-unchecked:bg-input/80` |
| Mira | `rounded-full` `border` `border-transparent` `focus-visible:ring-2` `focus-visible:ring-ring/30` `data-[size=default]:h-[16.6px]` `data-[size=default]:w-[28px]` `data-[size=sm]:h-[14px]` `data-[size=sm]:w-[24px]` `data-unchecked:bg-input` `dark:data-unchecked:bg-input/80` |
| Luma | `rounded-full` `border-2` `focus-visible:ring-3` `focus-visible:ring-ring/30` `data-[size=default]:h-5` `data-[size=default]:w-11` `data-[size=sm]:h-4` `data-[size=sm]:w-7` `data-checked:border-primary` `data-unchecked:border-transparent` `data-unchecked:bg-input/90` `shadow-sm` `not-dark:bg-clip-padding` `data-checked:translate-x-[calc(100%-8px)]` `data-unchecked:translate-x-0` |
| Sera | `rounded-none` `border` `focus-visible:ring-2` `focus-visible:ring-ring/30` `data-[size=default]:h-4.5` `data-[size=default]:w-8.25` `data-[size=sm]:h-3.5` `data-[size=sm]:w-6.25` `data-checked:border-primary` `data-unchecked:border-input/50` `data-unchecked:bg-input` `data-unchecked:translate-x-0.25` |
| Rhea | `rounded-2xl` `border-2` `focus-visible:ring-3` `focus-visible:ring-ring/30` `data-[size=default]:h-5` `data-[size=default]:w-8` `data-[size=sm]:h-4` `data-[size=sm]:w-6` `data-checked:border-primary` `data-unchecked:border-transparent` `data-unchecked:bg-input/90` `shadow-sm` `not-dark:bg-clip-padding` `data-checked:translate-x-[calc(100%-4px)]` `data-unchecked:translate-x-0` |

### slider

| Style | 区别性 class |
|---|---|
| Vega | `rounded-full` `bg-muted` `data-horizontal:h-1.5` `data-vertical:w-1.5` `size-4` `border` `border-primary` `bg-white` `shadow-sm` `ring-ring/50` `hover:ring-4` `focus-visible:ring-4` |
| Nova | `rounded-full` `bg-muted` `data-horizontal:h-1` `data-vertical:w-1` `size-3` `border` `border-ring` `bg-white` `ring-ring/50` `after:-inset-2` `hover:ring-3` `focus-visible:ring-3` `active:ring-3` |
| Maia | `rounded-4xl` `bg-muted` `data-horizontal:h-3` `data-vertical:w-3` `size-4` `border` `border-primary` `bg-white` `shadow-sm` `ring-ring/50` `hover:ring-4` `focus-visible:ring-4` |
| Lyra | `rounded-none` `bg-muted` `data-horizontal:h-1` `data-vertical:w-1` `size-3` `border` `border-ring` `bg-white` `ring-ring/50` `after:-inset-2` `hover:ring-1` `focus-visible:ring-1` `active:ring-1` |
| Mira | `rounded-md` `bg-muted` `data-horizontal:h-1` `data-vertical:w-1` `size-3` `border` `border-ring` `bg-white` `ring-ring/30` `after:-inset-2` `hover:ring-2` `focus-visible:ring-2` `active:ring-2` |
| Luma | `rounded-full` `bg-input/90` `data-horizontal:h-2` `data-vertical:w-2` `h-4` `w-6` `bg-white` `shadow-md` `ring-1` `ring-black/10` `not-dark:bg-clip-padding` `hover:ring-4` `hover:ring-ring/30` `focus-visible:ring-4` `focus-visible:ring-ring/30` `data-vertical:h-6` `data-vertical:w-4` |
| Sera | `bg-input/50` `data-horizontal:h-0.5` `data-vertical:w-0.5` `size-3` `border-none` `hover:ring-2` `hover:ring-ring/30` `focus-visible:ring-2` `focus-visible:ring-ring/30` |
| Rhea | `rounded-2xl` `bg-input/90` `data-horizontal:h-1` `data-vertical:w-1` `size-4` `bg-white` `shadow-md` `ring-1` `ring-black/10` `not-dark:bg-clip-padding` `hover:ring-4` `hover:ring-ring/30` `focus-visible:ring-4` `focus-visible:ring-ring/30` |

### progress

| Style | 区别性 class |
|---|---|
| Vega | `h-1.5` `rounded-full` `text-sm` `font-medium` |
| Nova | `h-1` `rounded-full` `text-sm` `font-medium` |
| Maia | `h-3` `rounded-4xl` `text-sm` `font-medium` |
| Lyra | `h-1` `rounded-none` `text-xs` |
| Mira | `h-1` `rounded-md` `text-xs/relaxed` `font-medium` |
| Luma | `h-3` `rounded-full` `text-sm` `font-medium` |
| Sera | `h-0.5` `rounded-none` `text-xs` `font-semibold` `tracking-wide` `uppercase` `text-sm` |
| Rhea | `h-2` `rounded-2xl` `text-sm` `font-medium` |

### toggle

| Style | 区别性 class |
|---|---|
| Vega | `gap-1` `rounded-md` `text-sm` `font-medium` `focus-visible:ring-ring/50` `shadow-xs` `h-9` `min-w-9` `px-2.5` `h-8` `min-w-8` `h-10` `min-w-10` |
| Nova | `gap-1` `rounded-lg` `text-sm` `font-medium` `focus-visible:ring-ring/50` `data-[state=on]:bg-muted` `h-8` `min-w-8` `px-2.5` `h-7` `min-w-7` `rounded-[min(var(--radius-md),12px)]` `text-[0.8rem]` `h-9` `min-w-9` |
| Maia | `gap-1` `rounded-4xl` `text-sm` `font-medium` `focus-visible:ring-ring/50` `h-9` `min-w-9` `px-3` `h-8` `min-w-8` `h-10` `min-w-10` `px-4` |
| Lyra | `gap-1` `rounded-none` `text-xs` `font-medium` `focus-visible:ring-ring/50` `data-[state=on]:bg-muted` `h-8` `min-w-8` `px-2.5` `h-7` `min-w-7` `h-9` `min-w-9` |
| Mira | `gap-1` `rounded-md` `text-xs` `font-medium` `focus-visible:ring-ring/50` `data-[state=on]:bg-muted` `h-7` `min-w-7` `px-2` `h-6` `min-w-6` `rounded-[min(var(--radius-md),8px)]` `text-[0.625rem]` `h-8` `min-w-8` `px-2.5` |
| Luma | `gap-1` `rounded-3xl` `text-sm` `font-medium` `focus-visible:ring-ring/30` `h-9` `min-w-9` `px-3` `h-8` `min-w-8` `h-10` `min-w-10` `px-4` |
| Sera | `gap-1.5` `rounded-none` `text-xs` `font-semibold` `tracking-widest` `uppercase` `focus-visible:ring-ring/30` `h-10` `min-w-10` `px-6` `h-9` `min-w-9` `px-4` `h-11` `min-w-11` `px-8` |
| Rhea | `gap-1` `rounded-2xl` `text-sm` `font-medium` `focus-visible:ring-ring/30` `h-8` `min-w-8` `px-2.5` `h-7` `min-w-7` `h-9` `min-w-9` |

### tabs

| Style | 区别性 class |
|---|---|
| Vega | `rounded-lg` `p-[3px]` `data-[variant=line]:rounded-none` `gap-1.5` `rounded-md` `border-transparent` `px-2` `py-1` `text-sm` `font-medium` |
| Nova | `rounded-lg` `p-[3px]` `data-[variant=line]:rounded-none` `gap-1.5` `rounded-md` `border-transparent` `px-1.5` `py-0.5` `text-sm` `font-medium` |
| Maia | `rounded-4xl` `p-[3px]` `data-[variant=line]:rounded-none` `gap-1.5` `rounded-xl` `border-transparent` `px-2` `py-1` `text-sm` `font-medium` |
| Lyra | `rounded-none` `p-[3px]` `data-[variant=line]:rounded-none` `gap-1.5` `border-transparent` `px-1.5` `py-0.5` `text-xs` `font-medium` `text-xs/relaxed` |
| Mira | `rounded-lg` `p-[3px]` `data-[variant=line]:rounded-none` `gap-1.5` `rounded-md` `border-transparent` `px-1.5` `py-0.5` `text-xs` `font-medium` `text-xs/relaxed` |
| Luma | `rounded-full` `p-1` `data-[variant=line]:rounded-none` `border-transparent!` `px-3` `py-1` `text-sm` `font-medium` |
| Sera | `p-1` `border-transparent` `px-4` `py-1.5` `text-xs` `font-semibold` `tracking-wider` `uppercase` `text-sm` |
| Rhea | `rounded-2xl` `p-[3px]` `data-[variant=line]:rounded-none` `gap-1.5` `border-transparent!` `px-1.5` `py-0.5` `text-sm` `font-medium` |

### badge

| Style | 区别性 class |
|---|---|
| Vega | `h-5` `gap-1` `rounded-4xl` `border` `border-transparent` `px-2` `py-0.5` `text-xs` `font-medium` `[&>svg]:size-3!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |
| Nova | `h-5` `gap-1` `rounded-4xl` `border` `border-transparent` `px-2` `py-0.5` `text-xs` `font-medium` `[&>svg]:size-3!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |
| Maia | `h-5` `gap-1` `rounded-4xl` `border` `border-transparent` `px-2` `py-0.5` `text-xs` `font-medium` `[&>svg]:size-3!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `bg-input/30` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |
| Lyra | `h-5` `gap-1` `rounded-none` `border` `border-transparent` `px-2` `py-0.5` `text-xs` `font-medium` `[&>svg]:size-3!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |
| Mira | `h-5` `gap-1` `rounded-full` `border` `border-transparent` `px-2` `py-0.5` `text-[0.625rem]` `font-medium` `[&>svg]:size-2.5!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `bg-input/20` `dark:bg-input/30` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |
| Luma | `h-5` `gap-1` `rounded-3xl` `border` `border-transparent` `px-2` `py-0.5` `text-xs` `font-medium` `[&>svg]:size-3!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |
| Sera | `gap-1.5` `rounded-none` `border-0` `bg-transparent` `px-0` `py-0` `text-[0.625rem]` `font-semibold` `tracking-widest` `uppercase` `[&>svg]:size-3!` `[a]:hover:text-foreground/70` `text-muted-foreground` `[a]:hover:text-foreground` `hover:text-foreground` |
| Rhea | `h-5` `gap-1` `rounded-2xl` `border` `border-transparent` `px-2` `py-0.5` `text-xs` `font-medium` `[&>svg]:size-3!` `bg-primary` `text-primary-foreground` `[a]:hover:bg-primary/80` `bg-secondary` `text-secondary-foreground` `[a]:hover:bg-secondary/80` `border-border` `[a]:hover:bg-muted` `[a]:hover:text-muted-foreground` `hover:bg-muted` `hover:text-muted-foreground` `dark:hover:bg-muted/50` `text-primary` |

### card

| Style | 区别性 class |
|---|---|
| Vega | `rounded-xl` `shadow-xs` `ring-foreground/10` `[--card-spacing:--spacing(6)]` `data-[size=sm]:[--card-spacing:--spacing(4)]` `*:[img:first-child]:rounded-t-xl` `*:[img:last-child]:rounded-b-xl` `gap-1` `rounded-t-xl` `text-base` `leading-normal` `font-medium` `gap-3` `rounded-b-xl` `[.border-t]:pt-(--card-spacing)` |
| Nova | `rounded-xl` `ring-foreground/10` `[--card-spacing:--spacing(4)]` `has-data-[slot=card-footer]:pb-0` `data-[size=sm]:[--card-spacing:--spacing(3)]` `data-[size=sm]:has-data-[slot=card-footer]:pb-0` `*:[img:first-child]:rounded-t-xl` `*:[img:last-child]:rounded-b-xl` `gap-1` `rounded-t-xl` `text-base` `leading-snug` `font-medium` `rounded-b-xl` `border-t` `bg-muted/50` `p-(--card-spacing)` |
| Maia | `rounded-2xl` `ring-foreground/10` `[--card-spacing:--spacing(6)]` `data-[size=sm]:[--card-spacing:--spacing(4)]` `*:[img:first-child]:rounded-t-xl` `*:[img:last-child]:rounded-b-xl` `gap-2` `rounded-t-xl` `text-base` `font-medium` `rounded-b-xl` `[.border-t]:pt-(--card-spacing)` |
| Lyra | `rounded-none` `text-xs/relaxed` `ring-foreground/10` `[--card-spacing:--spacing(4)]` `has-data-[slot=card-footer]:pb-0` `data-[size=sm]:[--card-spacing:--spacing(3)]` `data-[size=sm]:has-data-[slot=card-footer]:pb-0` `*:[img:first-child]:rounded-none` `*:[img:last-child]:rounded-none` `gap-1` `font-medium` `border-t` `p-(--card-spacing)` |
| Mira | `rounded-lg` `text-xs/relaxed` `ring-foreground/10` `[--card-spacing:--spacing(4)]` `data-[size=sm]:[--card-spacing:--spacing(3)]` `*:[img:first-child]:rounded-t-lg` `*:[img:last-child]:rounded-b-lg` `gap-1` `rounded-t-lg` `font-medium` `rounded-b-lg` `[.border-t]:pt-(--card-spacing)` |
| Luma | `rounded-4xl` `shadow-md` `ring-foreground/5` `[--card-spacing:--spacing(6)]` `data-[size=sm]:[--card-spacing:--spacing(4)]` `dark:ring-foreground/10` `*:[img:first-child]:rounded-t-4xl` `*:[img:last-child]:rounded-b-4xl` `gap-1.5` `rounded-t-4xl` `text-base` `font-medium` `rounded-b-4xl` `[.border-t]:pt-(--card-spacing)` |
| Sera | `shadow-sm` `ring-foreground/5` `[--card-spacing:--spacing(8)]` `data-[size=sm]:[--card-spacing:--spacing(5)]` `*:[img:first-child]:rounded-none` `*:[img:last-child]:rounded-none` `gap-1.5` `rounded-none` `text-lg` `font-semibold` `tracking-wider` `uppercase` `leading-relaxed` `[.border-t]:pt-(--card-spacing)` |
| Rhea | `rounded-[min(var(--radius-4xl),24px)]` `shadow-sm` `ring-foreground/5` `[--card-spacing:--spacing(5)]` `data-[size=sm]:[--card-spacing:--spacing(4)]` `dark:ring-foreground/10` `*:[img:first-child]:rounded-t-[min(var(--radius-4xl),24px)]` `*:[img:last-child]:rounded-b-[min(var(--radius-4xl),24px)]` `gap-1.5` `rounded-t-[min(var(--radius-4xl),24px)]` `text-base` `font-medium` `rounded-b-[min(var(--radius-4xl),24px)]` `[.border-t]:pt-(--card-spacing)` |

### alert

| Style | 区别性 class |
|---|---|
| Vega | `gap-0.5` `rounded-lg` `px-4` `py-3` `text-sm` `has-[>svg]:gap-x-2.5` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `[&_p:not(:last-child)]:mb-4` `top-2.5` `right-3` |
| Nova | `gap-0.5` `rounded-lg` `px-2.5` `py-2` `text-sm` `has-[>svg]:gap-x-2` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `[&_p:not(:last-child)]:mb-4` `top-2` `right-2` |
| Maia | `gap-0.5` `rounded-lg` `px-4` `py-3` `text-sm` `has-[>svg]:gap-x-2.5` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `[&_p:not(:last-child)]:mb-4` `top-2.5` `right-3` |
| Lyra | `gap-0.5` `rounded-none` `px-2.5` `py-2` `text-xs` `has-[>svg]:gap-x-2` `*:[svg]:translate-y-0` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `text-xs/relaxed` `[&_p:not(:last-child)]:mb-2` `top-[calc(--spacing(1.25))]` `right-[calc(--spacing(1.25))]` |
| Mira | `gap-0.5` `rounded-lg` `px-2` `py-1.5` `text-xs/relaxed` `has-[>svg]:gap-x-1.5` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-3.5` `font-medium` `[&_p:not(:last-child)]:mb-4` `top-1.5` `right-2` |
| Luma | `gap-0.5` `rounded-2xl` `px-4` `py-3` `text-sm` `has-[>svg]:gap-x-2.5` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `[&_p:not(:last-child)]:mb-4` `top-2.5` `right-3` |
| Sera | `gap-1` `bg-background` `px-4` `py-3` `text-sm` `after:-inset-y-px` `after:-left-px` `after:w-0.5` `has-[>svg]:gap-x-2.5` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-4` `after:bg-foreground` `after:bg-destructive` `font-semibold` `[&_p:not(:last-child)]:mb-4` `top-2.5` `right-3` |
| Rhea | `gap-0.5` `rounded-2xl` `px-4` `py-3` `text-sm` `has-[>svg]:gap-x-2.5` `*:[svg]:translate-y-0.5` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `[&_p:not(:last-child)]:mb-4` `top-2.5` `right-3` |

### table

| Style | 区别性 class |
|---|---|
| Vega | `text-sm` `h-10` `px-2` `text-foreground` `p-2` |
| Nova | `text-sm` `h-10` `px-2` `text-foreground` `p-2` |
| Maia | `text-sm` `h-12` `px-3` `text-foreground` `p-3` |
| Lyra | `text-xs` `h-10` `px-2` `text-foreground` `p-2` |
| Mira | `text-xs` `h-10` `px-2` `text-foreground` `p-2` |
| Luma | `text-sm` `h-12` `px-3` `text-foreground` `p-3` |
| Sera | `text-sm` `h-12` `px-3` `text-xs` `tracking-wider` `uppercase` `p-3` |
| Rhea | `text-sm` `h-10` `px-2` `text-foreground` `p-2` |

### calendar

| Style | 区别性 class |
|---|---|
| Vega | `@/registry/base-vega/ui/button` `p-3` `[--cell-radius:var(--radius-md)]` `[--cell-size:--spacing(8)]` |
| Nova | `@/registry/base-nova/ui/button` `p-2` `[--cell-radius:var(--radius-md)]` `[--cell-size:--spacing(7)]` |
| Maia | `@/registry/base-maia/ui/button` `p-3` `[--cell-radius:var(--radius-4xl)]` `[--cell-size:--spacing(8)]` |
| Lyra | `@/registry/base-lyra/ui/button` `p-2` `[--cell-size:--spacing(7)]` |
| Mira | `@/registry/base-mira/ui/button` `p-3` `[--cell-radius:var(--radius-md)]` `[--cell-size:--spacing(6)]` |
| Luma | `@/registry/base-luma/ui/button` `p-3` `[--cell-radius:var(--radius-4xl)]` `[--cell-size:--spacing(8)]` |
| Sera | `@/registry/base-sera/ui/button` `p-3` `[--cell-radius:0]` `[--cell-size:--spacing(8)]` |
| Rhea | `@/registry/base-rhea/ui/button` `p-3` `[--cell-radius:var(--radius-2xl)]` `[--cell-size:--spacing(8)]` |

### dropdown-menu

| Style | 区别性 class |
|---|---|
| Vega | `min-w-32` `rounded-md` `p-1` `shadow-md` `ring-foreground/10` `px-2` `py-1.5` `font-medium` `data-inset:pl-8` `gap-2` `rounded-sm` `text-sm` `min-w-[96px]` `shadow-lg` `pl-2` `-mx-1` `my-1` `bg-border` |
| Nova | `min-w-32` `rounded-lg` `p-1` `shadow-md` `ring-foreground/10` `px-1.5` `py-1` `font-medium` `data-inset:pl-7` `gap-1.5` `rounded-md` `text-sm` `min-w-[96px]` `shadow-lg` `pl-1.5` `-mx-1` `my-1` `bg-border` |
| Maia | `min-w-48` `rounded-2xl` `p-1` `shadow-2xl` `ring-foreground/5` `dark:ring-foreground/10` `px-3` `py-2.5` `data-inset:pl-9.5` `gap-2.5` `rounded-xl` `py-2` `text-sm` `gap-2` `min-w-36` `pl-3` `-mx-1` `my-1` `bg-border/50` |
| Lyra | `min-w-32` `rounded-none` `shadow-md` `ring-foreground/10` `px-2` `py-2` `data-inset:pl-7` `gap-2` `min-w-[96px]` `shadow-lg` `pl-2` `-mx-1` `bg-border` |
| Mira | `min-w-32` `rounded-lg` `p-1` `shadow-md` `ring-foreground/10` `px-2` `py-1.5` `data-inset:pl-7.5` `min-h-7` `gap-2` `rounded-md` `py-1` `text-xs/relaxed` `pl-2` `-mx-1` `my-1` `bg-border/50` `text-[0.625rem]` |
| Luma | `min-w-48` `rounded-3xl` `p-1.5` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `px-3` `py-2.5` `data-inset:pl-9.5` `gap-2.5` `rounded-2xl` `py-2` `text-sm` `font-medium` `gap-2` `min-w-36` `pl-3` `-mx-1.5` `my-1.5` `bg-border/50` |
| Sera | `min-w-48` `rounded-none` `p-1.5` `shadow-md` `ring-foreground/10` `px-3` `py-2` `font-semibold` `tracking-wider` `uppercase` `data-inset:pl-9.5` `gap-2.5` `font-medium` `gap-2` `min-w-36` `pl-3` `-mx-1.5` `my-1.5` `bg-border/50` |
| Rhea | `min-w-32` `rounded-2xl` `p-1` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `px-2` `py-1` `data-inset:pl-7` `min-h-7` `gap-2` `rounded-xl` `py-1.5` `text-sm` `min-w-[96px]` `pl-2` `-mx-1` `my-1` `bg-border/50` |

### context-menu

| Style | 区别性 class |
|---|---|
| Vega | `min-w-36` `rounded-md` `p-1` `shadow-md` `ring-foreground/10` `px-2` `py-1.5` `font-medium` `data-inset:pl-8` `gap-2` `rounded-sm` `text-sm` `focus:*:[svg]:text-accent-foreground` `shadow-lg` `pl-2` `-mx-1` `my-1` `bg-border` |
| Nova | `min-w-36` `rounded-lg` `p-1` `shadow-md` `ring-foreground/10` `px-1.5` `py-1` `font-medium` `data-inset:pl-7` `gap-1.5` `rounded-md` `text-sm` `focus:*:[svg]:text-accent-foreground` `shadow-lg` `pl-1.5` `-mx-1` `my-1` `bg-border` |
| Maia | `min-w-48` `rounded-2xl` `p-1` `shadow-2xl` `ring-foreground/5` `px-3` `py-2.5` `data-inset:pl-9.5` `gap-2.5` `rounded-xl` `py-2` `text-sm` `focus:*:[svg]:text-accent-foreground` `shadow-lg` `gap-2` `pl-3` `-mx-1` `my-1` `bg-border/50` |
| Lyra | `min-w-36` `rounded-none` `shadow-md` `ring-foreground/10` `px-2` `py-2` `data-inset:pl-7` `gap-2` `focus:*:[svg]:text-accent-foreground` `shadow-lg` `pl-2` `-mx-1` `bg-border` |
| Mira | `min-w-32` `rounded-lg` `p-1` `shadow-md` `ring-foreground/10` `px-2` `py-1.5` `data-inset:pl-7.5` `min-h-7` `gap-2` `rounded-md` `py-1` `text-xs/relaxed` `not-data-[variant=destructive]:focus:**:text-accent-foreground` `shadow-lg` `pl-2` `focus:**:text-accent-foreground` `-mx-1` `my-1` `bg-border/50` `text-[0.625rem]` |
| Luma | `min-w-48` `rounded-3xl` `p-1.5` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `px-3` `py-2.5` `data-inset:pl-9.5` `gap-2.5` `rounded-2xl` `py-2` `text-sm` `font-medium` `focus:*:[svg]:text-accent-foreground` `pl-3` `-mx-1.5` `my-1.5` `bg-border/50` |
| Sera | `min-w-48` `rounded-none` `p-1.5` `shadow-md` `ring-foreground/10` `px-3` `py-2` `font-semibold` `tracking-wider` `uppercase` `data-inset:pl-9.5` `gap-2.5` `font-medium` `focus:*:[svg]:text-accent-foreground` `pl-3` `-mx-1.5` `my-1.5` `bg-border/50` |
| Rhea | `min-w-36` `rounded-2xl` `p-1` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `px-2` `py-1` `data-inset:pl-7` `min-h-7` `gap-2` `rounded-xl` `py-1.5` `text-sm` `focus:*:[svg]:text-accent-foreground` `pl-2` `-mx-1` `my-1` `bg-border/50` |

### menubar

| Style | 区别性 class |
|---|---|
| Vega | `@/registry/base-vega/ui/dropdown-menu` `h-9` `gap-1` `rounded-md` `shadow-xs` `rounded-sm` `px-2` `py-1` `text-sm` `min-w-36` `shadow-md` `ring-foreground/10` `gap-2` `py-1.5` `data-inset:pl-8` `pr-2` `pl-8` `left-2` `bg-border` `shadow-lg` |
| Nova | `@/registry/base-nova/ui/dropdown-menu` `h-8` `gap-0.5` `rounded-lg` `p-[3px]` `rounded-sm` `px-1.5` `py-[2px]` `text-sm` `min-w-36` `shadow-md` `ring-foreground/10` `gap-1.5` `rounded-md` `py-1` `data-inset:pl-7` `pr-1.5` `pl-7` `left-1.5` `bg-border` `shadow-lg` |
| Maia | `@/registry/base-maia/ui/dropdown-menu` `h-9` `rounded-4xl` `rounded-xl` `px-2` `py-0.75` `text-sm` `min-w-48` `rounded-2xl` `shadow-2xl` `ring-foreground/5` `gap-2.5` `px-3` `py-2` `data-inset:pl-9.5` `pr-3` `pl-9.5` `left-3` `px-3.5` `py-2.5` `bg-border/50` `gap-2` |
| Lyra | `@/registry/base-lyra/ui/dropdown-menu` `h-8` `gap-0.5` `rounded-none` `px-1.5` `py-[calc(--spacing(0.8))]` `min-w-36` `shadow-md` `ring-foreground/10` `gap-2` `px-2` `py-2` `data-inset:pl-8` `pr-28` `pl-8` `left-1.5` `pr-2` `bg-border` `shadow-lg` |
| Mira | `@/registry/base-mira/ui/dropdown-menu` `h-9` `rounded-lg` `rounded-[calc(var(--radius-md)-2px)]` `px-2` `py-[calc(--spacing(0.85))]` `text-xs/relaxed` `shadow-md` `ring-foreground/10` `min-h-7` `gap-2` `rounded-md` `py-1` `data-inset:pl-7.5` `py-1.5` `pr-2` `pl-7.5` `left-2` `bg-border/50` `text-[0.625rem]` |
| Luma | `@/registry/base-luma/ui/dropdown-menu` `h-9` `rounded-3xl` `rounded-2xl` `px-2` `py-0.75` `text-sm` `min-w-48` `p-1.5` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `gap-2.5` `px-3` `py-2` `data-inset:pl-9.5` `pr-3` `pl-9.5` `left-3` `px-3.5` `py-2.5` `bg-border/50` `gap-2` |
| Sera | `@/registry/base-sera/ui/dropdown-menu` `h-10` `rounded-none` `px-2` `py-0.75` `text-sm` `min-w-48` `p-1.5` `shadow-md` `ring-foreground/10` `gap-2.5` `px-3` `py-2` `data-inset:pl-9.5` `pr-3` `pl-9.5` `tracking-wider` `uppercase` `left-3` `px-3.5` `font-semibold` `bg-border/50` `gap-2` |
| Rhea | `@/registry/base-rhea/ui/dropdown-menu` `h-8` `rounded-2xl` `p-[3px]` `px-1.5` `py-[2px]` `text-sm` `min-w-36` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `min-h-7` `gap-2` `rounded-xl` `px-2` `py-1.5` `data-inset:pl-7` `pr-1.5` `pl-7` `left-1.5` `py-1` `bg-border/50` |

### popover

| Style | 区别性 class |
|---|---|
| Vega | `gap-4` `rounded-md` `p-4` `shadow-md` `ring-foreground/10` `gap-1` `font-medium` |
| Nova | `gap-2.5` `rounded-lg` `p-2.5` `shadow-md` `ring-foreground/10` `gap-0.5` `font-medium` |
| Maia | `gap-4` `rounded-2xl` `p-4` `shadow-2xl` `ring-foreground/5` `gap-1` `text-base` `font-medium` |
| Lyra | `gap-2.5` `rounded-none` `p-2.5` `text-xs` `shadow-md` `ring-foreground/10` `gap-1` `font-medium` `text-xs/relaxed` |
| Mira | `gap-4` `rounded-lg` `p-2.5` `text-xs` `shadow-md` `ring-foreground/10` `gap-1` `font-medium` |
| Luma | `gap-4` `rounded-3xl` `p-4` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `gap-1` `text-base` `font-medium` |
| Sera | `gap-4` `rounded-none` `p-4` `shadow-md` `ring-foreground/10` `gap-1` `text-xs` `font-semibold` `uppercase` `mt-0.5` `leading-relaxed` |
| Rhea | `gap-4` `rounded-3xl` `p-4` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` `gap-1` `text-base` `font-medium` |

### hover-card

| Style | 区别性 class |
|---|---|
| Vega | `w-64` `rounded-lg` `p-4` `text-sm` `shadow-md` `ring-foreground/10` |
| Nova | `w-64` `rounded-lg` `p-2.5` `text-sm` `shadow-md` `ring-foreground/10` |
| Maia | `w-72` `rounded-2xl` `p-4` `text-sm` `shadow-2xl` `ring-foreground/5` |
| Lyra | `w-64` `rounded-none` `p-2.5` `text-xs/relaxed` `shadow-md` `ring-foreground/10` |
| Mira | `w-72` `rounded-lg` `p-2.5` `text-xs/relaxed` `shadow-md` `ring-foreground/10` |
| Luma | `w-72` `rounded-3xl` `p-4` `text-sm` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` |
| Sera | `w-72` `p-4` `text-sm` `shadow-md` `ring-foreground/10` |
| Rhea | `w-72` `rounded-3xl` `p-4` `text-sm` `shadow-lg` `ring-foreground/5` `dark:ring-foreground/10` |

### tooltip

| Style | 区别性 class |
|---|---|
| Vega | `rounded-md` `**:data-[slot=kbd]:rounded-sm` `rounded-[2px]` |
| Nova | `rounded-md` `**:data-[slot=kbd]:rounded-sm` `rounded-[2px]` |
| Maia | `rounded-2xl` `**:data-[slot=kbd]:rounded-4xl` `rounded-[2px]` `data-[side=inline-end]:translate-x-[1.5px]` `data-[side=inline-start]:translate-x-[-1.5px]` `data-[side=left]:translate-x-[-1.5px]` `data-[side=right]:translate-x-[1.5px]` |
| Lyra | `rounded-none` `**:data-[slot=kbd]:rounded-none` |
| Mira | `rounded-md` `**:data-[slot=kbd]:rounded-sm` `rounded-[2px]` |
| Luma | `rounded-xl` `**:data-[slot=kbd]:rounded-lg` `rounded-[2px]` `data-[side=inline-end]:translate-x-[1.5px]` `data-[side=inline-start]:translate-x-[-1.5px]` `data-[side=left]:translate-x-[-1.5px]` `data-[side=right]:translate-x-[1.5px]` |
| Sera | `rounded-none` `**:data-[slot=kbd]:rounded-none` `data-[side=inline-end]:translate-x-[1.5px]` `data-[side=inline-start]:translate-x-[-1.5px]` |
| Rhea | `rounded-xl` `**:data-[slot=kbd]:rounded-lg` `rounded-[2px]` `data-[side=inline-end]:translate-x-[1.5px]` `data-[side=inline-start]:translate-x-[-1.5px]` `data-[side=left]:translate-x-[-1.5px]` `data-[side=right]:translate-x-[1.5px]` |

### dialog

| Style | 区别性 class |
|---|---|
| Vega | `@/registry/base-vega/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-6` `rounded-xl` `p-6` `ring-foreground/10` `sm:max-w-md` `top-4` `right-4` `leading-none` `font-medium` |
| Nova | `@/registry/base-nova/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `rounded-xl` `p-4` `ring-foreground/10` `sm:max-w-sm` `top-2` `right-2` `-mx-4` `-mb-4` `rounded-b-xl` `border-t` `bg-muted/50` `text-base` `leading-none` `font-medium` |
| Maia | `@/registry/base-maia/ui/button` `bg-black/80` `supports-backdrop-filter:backdrop-blur-xs` `gap-6` `rounded-4xl` `p-6` `ring-foreground/5` `sm:max-w-md` `top-4` `right-4` `text-base` `leading-none` `font-medium` |
| Lyra | `@/registry/base-lyra/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `rounded-none` `p-4` `text-xs/relaxed` `ring-foreground/10` `sm:max-w-sm` `top-2` `right-2` `gap-1` `text-left` `font-medium` |
| Mira | `@/registry/base-mira/ui/button` `bg-black/80` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `rounded-xl` `p-4` `text-xs/relaxed` `ring-foreground/10` `sm:max-w-sm` `top-2` `right-2` `gap-1` `font-medium` |
| Luma | `@/registry/base-luma/ui/button` `bg-black/30` `supports-backdrop-filter:backdrop-blur-sm` `gap-6` `rounded-4xl` `p-6` `shadow-xl` `ring-foreground/5` `sm:max-w-md` `dark:ring-foreground/10` `top-4` `right-4` `bg-secondary` `gap-1.5` `text-base` `leading-none` `font-medium` |
| Sera | `@/registry/base-sera/ui/button` `bg-black/20` `supports-backdrop-filter:backdrop-blur-sm` `gap-6` `rounded-none` `p-6` `shadow-md` `ring-foreground/10` `sm:max-w-md` `top-5` `right-5` `bg-secondary` `text-lg` `leading-none` `font-semibold` `tracking-wider` `uppercase` `mt-0.5` `leading-relaxed` |
| Rhea | `@/registry/base-rhea/ui/button` `bg-black/30` `supports-backdrop-filter:backdrop-blur-sm` `gap-6` `rounded-[min(var(--radius-4xl),24px)]` `p-6` `shadow-xl` `ring-foreground/5` `sm:max-w-md` `dark:ring-foreground/10` `top-4` `right-4` `bg-secondary` `gap-1.5` `text-base` `leading-none` `font-medium` |

### alert-dialog

| Style | 区别性 class |
|---|---|
| Vega | `@/registry/base-vega/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-6` `rounded-xl` `p-6` `ring-foreground/10` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-lg` `gap-1.5` `has-data-[slot=alert-dialog-media]:gap-x-6` `size-16` `rounded-md` `*:[svg:not([class*='size-'])]:size-8` `text-lg` `font-medium` |
| Nova | `@/registry/base-nova/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `rounded-xl` `p-4` `ring-foreground/10` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-sm` `gap-1.5` `has-data-[slot=alert-dialog-media]:gap-x-4` `-mx-4` `-mb-4` `rounded-b-xl` `border-t` `bg-muted/50` `size-10` `rounded-md` `*:[svg:not([class*='size-'])]:size-6` `text-base` `font-medium` |
| Maia | `@/registry/base-maia/ui/button` `bg-black/80` `supports-backdrop-filter:backdrop-blur-xs` `gap-6` `rounded-4xl` `p-6` `ring-foreground/5` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-md` `gap-1.5` `has-data-[slot=alert-dialog-media]:gap-x-6` `size-16` `rounded-full` `*:[svg:not([class*='size-'])]:size-8` `text-lg` `font-medium` |
| Lyra | `@/registry/base-lyra/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `rounded-none` `p-4` `ring-foreground/10` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-sm` `gap-1.5` `has-data-[slot=alert-dialog-media]:gap-x-4` `size-10` `*:[svg:not([class*='size-'])]:size-6` `font-medium` `text-xs/relaxed` |
| Mira | `@/registry/base-mira/ui/button` `bg-black/80` `supports-backdrop-filter:backdrop-blur-xs` `gap-3` `rounded-xl` `p-4` `ring-foreground/10` `data-[size=sm]:max-w-64` `data-[size=default]:sm:max-w-sm` `gap-1` `has-data-[slot=alert-dialog-media]:gap-x-4` `size-8` `rounded-md` `*:[svg:not([class*='size-'])]:size-4` `font-medium` `text-xs/relaxed` |
| Luma | `@/registry/base-luma/ui/button` `bg-black/30` `supports-backdrop-filter:backdrop-blur-sm` `gap-6` `rounded-4xl` `p-6` `shadow-xl` `ring-foreground/5` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-md` `dark:ring-foreground/10` `gap-1.5` `has-data-[slot=alert-dialog-media]:gap-x-6` `size-16` `rounded-full` `*:[svg:not([class*='size-'])]:size-8` `text-lg` `font-medium` |
| Sera | `@/registry/base-sera/ui/button` `bg-black/20` `supports-backdrop-filter:backdrop-blur-sm` `gap-6` `rounded-none` `p-6` `shadow-md` `ring-foreground/10` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-md` `has-data-[slot=alert-dialog-media]:gap-x-6` `size-16` `*:[svg:not([class*='size-'])]:size-8` `text-lg` `font-semibold` `tracking-wider` `uppercase` `mt-0.5` `leading-relaxed` |
| Rhea | `@/registry/base-rhea/ui/button` `bg-black/30` `supports-backdrop-filter:backdrop-blur-sm` `gap-6` `rounded-[min(var(--radius-4xl),24px)]` `p-6` `shadow-xl` `ring-foreground/5` `data-[size=sm]:max-w-xs` `data-[size=default]:sm:max-w-md` `dark:ring-foreground/10` `gap-1.5` `has-data-[slot=alert-dialog-media]:gap-x-6` `size-16` `rounded-full` `*:[svg:not([class*='size-'])]:size-8` `text-lg` `font-medium` |

### sheet

| Style | 区别性 class |
|---|---|
| Vega | `@/registry/base-vega/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `shadow-lg` `top-4` `right-4` `gap-1.5` `p-4` `font-medium` |
| Nova | `@/registry/base-nova/ui/button` `bg-black/10` `supports-backdrop-filter:backdrop-blur-xs` `gap-4` `shadow-lg` `top-3` `right-3` `gap-0.5` `p-4` `text-base` `font-medium` |
| Maia | `@/registry/base-maia/ui/button` `bg-black/80` `supports-backdrop-filter:backdrop-blur-xs` `shadow-lg` `top-4` `right-4` `gap-1.5` `p-6` `text-base` `font-medium` |
| Lyra | `@/registry/base-lyra/ui/button` `bg-black/10` `text-xs/relaxed` `supports-backdrop-filter:backdrop-blur-xs` `shadow-lg` `top-3` `right-3` `gap-0.5` `p-4` `font-medium` |
| Mira | `@/registry/base-mira/ui/button` `bg-black/80` `supports-backdrop-filter:backdrop-blur-xs` `text-xs/relaxed` `shadow-lg` `top-4` `right-4` `gap-1.5` `p-6` `font-medium` |
| Luma | `@/registry/base-luma/ui/button` `bg-black/30` `supports-backdrop-filter:backdrop-blur-sm` `shadow-xl` `top-4` `right-4` `bg-secondary` `gap-1.5` `p-6` `text-base` `font-medium` |
| Sera | `@/registry/base-sera/ui/button` `bg-black/20` `supports-backdrop-filter:backdrop-blur-sm` `shadow-md` `top-4` `right-4` `bg-secondary` `gap-1.5` `p-8` `text-lg` `font-semibold` `tracking-wider` `uppercase` `mt-0.5` `leading-relaxed` |
| Rhea | `@/registry/base-rhea/ui/button` `bg-black/30` `supports-backdrop-filter:backdrop-blur-sm` `shadow-xl` `top-4` `right-4` `bg-secondary` `gap-1.5` `p-6` `text-base` `font-medium` |


## D. 重新生成

数据源与脚本（Python 3，无第三方依赖）。把 registry JSON 下载到同一目录（`colors-<base>.json`、`<style>/<component>.json`、`tw-theme.css` 取自 `https://unpkg.com/tailwindcss@4.3.5/theme.css`、`themes-observed.json`）后运行 `python3 -I gen.py <输出路径>`。脚本先校验 A 节规则（任一变量不是规则所说的 Tailwind 色阶就退出），再校验 B 节规则。`themes-observed.json` 是 2026-11-15 在 `/create`（Style Nova、Base Neutral）逐个选择 Theme 读到的 `--primary` / `--primary-foreground`。

<details><summary>gen.py</summary>

```python
import json, math, re, sys, os

D = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1]


def oklch_hex(s):
    s = s.strip().replace('oklch(', '').replace(')', '')
    alpha = None
    if '/' in s:
        s, a = s.split('/')
        a = a.strip()
        alpha = float(a[:-1]) / 100 if a.endswith('%') else float(a)
    parts = s.split()
    L = float(parts[0][:-1]) / 100 if parts[0].endswith('%') else float(parts[0])
    C = float(parts[1]); H = float(parts[2])
    a_ = C * math.cos(math.radians(H)); b_ = C * math.sin(math.radians(H))
    l_ = L + 0.3963377774 * a_ + 0.2158037573 * b_
    m_ = L - 0.1055613458 * a_ - 0.0638541728 * b_
    s_ = L - 0.0894841775 * a_ - 1.2914855480 * b_
    l, m, s3 = l_ ** 3, m_ ** 3, s_ ** 3
    r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s3
    g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s3
    b = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s3
    f = lambda x: 12.92 * x if x <= 0.0031308 else 1.055 * x ** (1 / 2.4) - 0.055
    rgb = ''.join('%02X' % round(max(0, min(1, f(c))) * 255) for c in (r, g, b))
    if alpha is not None:
        return '#%02X%s' % (round(alpha * 255), rgb)
    return '#' + rgb


lines = []
w = lines.append
w('# shadcn/ui 现行取值附录（自动生成）')
w('')
w('> 生成方式：2026-11-15 从 `https://ui.shadcn.com/r/colors/<base>.json`（`cssVarsV4`）、'
  '`https://ui.shadcn.com/r/styles/base-<style>/<component>.json` 与 `tailwindcss@4.3.5/theme.css`（npm 当前最新版） 下载后由脚本生成，'
  'hex 由 OKLCH 按 CSS Color 4 换算到 sRGB 并四舍五入；带透明度的值写成 AtomUI 的 `#AARRGGBB`。不要手改本文件，重新生成即可。')
w('')

# ---- A. base colors
bases = ['neutral', 'stone', 'zinc', 'mauve', 'olive', 'mist', 'taupe']
data = {b: json.load(open(os.path.join(D, f'colors-{b}.json')))['cssVarsV4'] for b in bases}
keys = [k for k in data['neutral']['light'] if not k.startswith(('chart', 'sidebar')) and k != 'radius']
w('## A. Base Color（7 种）')
w('')
w('`--radius` 七种均为 `0.625rem`（10px）。表中每格为 `hex`，括号内为官方 OKLCH。')
w('')
w('**规则**（已用脚本核对七种全部命中）：每个变量都是 Tailwind v4.3.5 同名色阶的一档。浅色：foreground 950、primary 900、secondary / muted / accent 100、muted-foreground 500、border / input 200、ring 400，background / card / popover 为白色。暗色：background 950、card / popover / primary-foreground 900、primary 200、secondary / muted / accent 800、foreground 50、muted-foreground 400、ring 500，border 为白色 10%、input 为白色 15%。destructive 浅色 red-600、暗色 red-400。Mauve、Olive、Mist、Taupe 是 Tailwind v4.3 新增的色阶，4.1 中没有。')
w('')
for mode in ('light', 'dark'):
    w(f'### A.{1 if mode == "light" else 2} {"浅色 :root" if mode == "light" else "暗色 .dark"}')
    w('')
    w('| 变量 | ' + ' | '.join(b.capitalize() for b in bases) + ' |')
    w('|---|' + '---|' * len(bases))
    for k in keys:
        cells = []
        for b in bases:
            v = data[b][mode].get(k)
            cells.append('—' if v is None else f'`{oklch_hex(v)}` ({v.replace("oklch(", "").replace(")", "")})')
        w(f'| `--{k}` | ' + ' | '.join(cells) + ' |')
    w('')

# ---- A rule check: every base color variable is a Tailwind step
_tw = open(os.path.join(D, 'tw-theme.css')).read()
_pal = {}
for _m in re.finditer(r'--color-([a-z]+)-(\d+):\s*oklch\(([^)]*)\)', _tw):
    _pal.setdefault(_m.group(1), {})[_m.group(2)] = _m.group(3).replace('none', '0')
def _n(s):
    return tuple(round(float(x.rstrip('%')) / 100, 3) if x.endswith('%') else round(float(x), 3)
                 for x in s.replace('oklch(', '').replace(')', '').split('/')[0].split())
RULE = {
    'light': {'background': 'W', 'foreground': '950', 'card': 'W', 'card-foreground': '950', 'popover': 'W',
              'popover-foreground': '950', 'primary': '900', 'primary-foreground': '50', 'secondary': '100',
              'secondary-foreground': '900', 'muted': '100', 'muted-foreground': '500', 'accent': '100',
              'accent-foreground': '900', 'destructive': 'red-600', 'border': '200', 'input': '200', 'ring': '400'},
    'dark': {'background': '950', 'foreground': '50', 'card': '900', 'card-foreground': '50', 'popover': '900',
             'popover-foreground': '50', 'primary': '200', 'primary-foreground': '900', 'secondary': '800',
             'secondary-foreground': '50', 'muted': '800', 'muted-foreground': '400', 'accent': '800',
             'accent-foreground': '50', 'destructive': 'red-400', 'border': 'W10', 'input': 'W15', 'ring': '500'}}
_bad = []
for _b in bases:
    for _mode, _rules in RULE.items():
        for _k, _step in _rules.items():
            _v = data[_b][_mode][_k]
            if _step == 'W': _ok = _n(_v) == (1.0, 0.0, 0.0)
            elif _step.startswith('W'): _ok = _v.replace(' ', '') == 'oklch(100/' + _step[1:] + '%)'
            elif _step.startswith('red'): _ok = _n(_v) == _n(_pal['red'][_step[4:]])
            else: _ok = _n(_v) == _n(_pal[_b][_step])
            if not _ok: _bad.append((_b, _mode, _k, _v, _step))
if _bad:
    print('BASE RULE MISMATCH', _bad, file=sys.stderr)
    sys.exit(1)

# ---- B. theme colors
tw = open(os.path.join(D, 'tw-theme.css')).read()
pal = {m.group(1): {} for m in re.finditer(r'--color-([a-z]+)-50:', tw)}
for m in re.finditer(r'--color-([a-z]+)-(\d+):\s*(oklch\([^)]*\))', tw):
    pal.setdefault(m.group(1), {})[m.group(2)] = m.group(3).replace(' none', ' 0')
themes = ['amber', 'blue', 'cyan', 'emerald', 'fuchsia', 'green', 'indigo', 'lime', 'orange', 'pink', 'purple', 'red',
          'rose', 'sky', 'teal', 'violet', 'yellow']
light_on_dark = {'lime', 'yellow'}
# observed on ui.shadcn.com/create (Style Nova, Base Neutral), used to verify the rule
observed = json.load(open(os.path.join(D, 'themes-observed.json')))
w('## B. Theme（17 种彩色主色）')
w('')
w('规则（已对 `/create` 实测值逐项校验，全部一致）：浅色 `--primary` = Tailwind v4 的 700 级、暗色 = 800 级、'
  '`--primary-foreground` = 50 级；Lime 与 Yellow 例外：浅色 400 级、暗色 500 级、前景 900 级。'
  '`--ring`、`--accent`、`--muted` 等其余变量不随 Theme 变化（`--secondary` 改为 zinc 的 100 / 暗色 zinc 的 800 档，'
  '见设计文档 6.3）。')
w('')
w('| Theme | 浅色 primary | 浅色 foreground | 暗色 primary | 暗色 foreground | 校验 |')
w('|---|---|---|---|---|---|')
bad = []
for t in themes:
    lp, dp, fg = ('400', '500', '900') if t in light_on_dark else ('700', '800', '50')
    vals = [pal[t][lp], pal[t][fg], pal[t][dp], pal[t][fg]]
    norm = lambda s: ' '.join(str(round(float(x.rstrip('%')) / 100, 6)) if x.endswith('%') else str(round(float(x.replace('none', '0')), 6))
                              for x in s.replace('oklch(', '').replace(')', '').split())
    o = observed[t.capitalize()]
    obs = [o['L']['primary'], o['L']['primary-foreground'], o['D']['primary'], o['D']['primary-foreground']]
    ok = all(norm(a) == norm(b) for a, b in zip(vals, obs))
    if not ok:
        bad.append(t)
    w(f'| {t.capitalize()} | `{oklch_hex(vals[0])}` ({t}-{lp}) | `{oklch_hex(vals[1])}` ({t}-{fg}) | '
      f'`{oklch_hex(vals[2])}` ({t}-{dp}) | `{oklch_hex(vals[3])}` ({t}-{fg}) | {"一致" if ok else "**不一致**"} |')
w('')
if bad:
    print('MISMATCH', bad, file=sys.stderr)

# ---- C. style classes
styles = ['vega', 'nova', 'maia', 'lyra', 'mira', 'luma', 'sera', 'rhea']
comps = ['button', 'input', 'textarea', 'select', 'combobox', 'checkbox', 'radio-group', 'switch', 'slider', 'progress',
         'toggle', 'tabs', 'badge', 'card', 'alert', 'table', 'calendar', 'dropdown-menu', 'context-menu', 'menubar',
         'popover', 'hover-card', 'tooltip', 'dialog', 'alert-dialog', 'sheet']
noise = re.compile(r'aria-invalid|destructive/|dark:aria|data-\[disabled|\[&_svg|has-data-\[icon|in-data-\[slot|'
                   r'data-open:|data-closed:|data-\[state=delayed|fade-|zoom-|slide-in|slide-out|^duration-|'
                   r'^transition|^animate|^select-none|^outline-none|^outline-hidden|^whitespace|^shrink|^inline-flex|'
                   r'^flex|^items-|^justify-|^relative|^absolute|^z-|^pointer-events|^cursor|^overflow|^origin|^group')
w('## C. 各 Style 组件的区别性 class')
w('')
w('每个组件只列出该 Style 与其余 Style 不同的 utility（8 种 Style 共有的 class 已去掉，状态与动画类已过滤）。'
  '完整源码以 registry JSON 为准。')
w('')
for c in comps:
    sets = {}
    for s in styles:
        j = json.load(open(os.path.join(D, s, f'{c}.json')))
        src = '\n'.join(f['content'] for f in j['files'])
        us = []
        for lit in re.findall(r'"([^"\n]{3,})"', src):
            for u in lit.split():
                if re.match(r'^[a-z0-9\-\[\]:!/*&()._,%>=~@\'#]+$', u) and not noise.search(u) and '-' in u or u in ('border', 'shadow', 'rounded', 'uppercase'):
                    if u not in us:
                        us.append(u)
        sets[s] = us
    common = set(sets['nova']).intersection(*[set(v) for v in sets.values()])
    w(f'### {c}')
    w('')
    w('| Style | 区别性 class |')
    w('|---|---|')
    for s in styles:
        diff = [u for u in sets[s] if u not in common]
        w(f'| {s.capitalize()} | ' + ' '.join(f'`{u}`' for u in diff[:90]).replace('|', '\\|') + ' |')
    w('')

open(OUT, 'w').write('\n'.join(lines) + '\n')
print('ok', len(lines))
```

</details>
