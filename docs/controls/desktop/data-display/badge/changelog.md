# Badge Changelog

本文档记录 Badge 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-14

- API
  - `AbstractCountBadge` 提供程序集内扩展点 `SynchronizesDecoratedTarget`。Mobile 的组合徽标可以通过此入口负责常驻目标及计数、圆点之间的移交。
- Fix
  - 组合徽标挂树时由实际宿主接管目标，避免外层计数徽标和内层圆点徽标重复设置同一个逻辑父级。
- Desktop 影响
  - 扩展点默认开启。Desktop 的隐藏指示器仍保留装饰目标，模板与公开 API 无变化。
- Tests
  - Desktop Badge 定向测试 34 项通过。Mobile 原失败的 14 项通过复验（包含 Skia 指纹与 Gallery 导航）。

## 2026-11-11

- Fix
  - `CountBadge` 与 `DotBadge` 的目标挂载和替换不再依赖指示器可见性：首次挂树时为零计数、空字符串 `Content` 或 `BadgeIsVisible=false`，以及隐藏期间替换 `DecoratedTarget`，目标都会挂入；旧目标在替换时移除。

## 2026-11-08

- Token
  - 指示器主题改读宿主徽标的 `CountBadgeTokenResource` / `DotBadgeTokenResource` / `RibbonBadgeTokenResource`，
    `Controls[X]` 覆盖到达装饰层中的指示器。
  - 预设色不再写死 14 个 RGB 常量，改取主题快照预设色板第 6 级（antd `darkColor`），主题变化时重算，暗色主题跟随暗色色板。
- Fixed
  - 清除或设为无效颜色时恢复主题颜色；DotBadge 外环随 `ShadowColor` 重绘；CountBadge 外环改由内部 `IndicatorShadows`
    投射到指示器边框。
  - DotBadge、CountBadge 装饰器脱离 AdornerLayer 时先清除 AdornedElement 再移除视觉，不再让旧被装饰视觉的组合引用残留。

## 2026-11-07

- API
  - `AbstractCountBadge` 新增 `Content`（`object?`，默认 `null`）与 `IsBordered`（`bool`，默认 `true`）。非空 `Content` 替代计数文本且始终显示，空字符串隐藏徽标，`null` 沿用计数逻辑；`IsBordered=false` 清除 Adorner 的白色描边阴影。
  - `AbstractDotBadge` 新增 `IsBordered`（`bool`，默认 `false`，与 Desktop 现有无描边外观一致），作为主题 selector 钩子。
  - `AbstractCountBadgeAdorner` 新增 `Content`、`IsBordered` 以及内部只读的 `PresentedContent`（`Content ?? CountText`），模板绑定 `PresentedContent`。
  - `BadgeColor`、`DotColor` 接受 `primary`、`success`、`warning`、`error`、`danger`（`danger` 等同 `error`），大小写不敏感。此前这些名称解析为 `null`，指示器不着色。
  - 内部扩展点 `AbstractCountBadge.CreateBadgeAdorner()`（`private protected abstract`）与 `NotifyDecoratedTargetChanged(Control?)`（`private protected virtual`）签名不变，供 AtomUI.Mobile 子类使用。
- Theme
  - CountBadge 模板中的计数文本由 `atom:TextBlock#BadgeText` 改为 `ContentPresenter#PART_ContentPresenter`（内容为 `PresentedContent`，实际渲染为 Avalonia `TextBlock`）。名称 `BadgeText` 不再存在，按该名称写的外部 selector 不再命中；仓库内无此类用法。字号、内边距与前景仍通过 Token 传递。
  - 预设色名在 CountBadge 与 DotBadge 上从主题快照的调色板取第 6 色。浅色主题下取值与此前固定的十六进制值一致，深色主题下改用深色调色板；颜色名随主题切换重新解析，挂树期间订阅主题变化，离树期间发生的切换在重新挂树时补齐。
  - `FloatButton` 与 `RibbonBadge` 仍使用固定的浅色预设表，深色下与 CountBadge/DotBadge 不一致，保持不变。
- Fix
  - 颜色在 Adorner 创建之前写入时不再抛异常（`ConfigureDotColor` 增加空 Adorner 保护）。
- Performance
  - 祖先可见性跟踪只在 Adorner 模式（存在 `DecoratedTarget` 且已挂树）下创建，范围为徽标到 Adorner 层宿主（不含宿主）之间的祖先；宿主及以上的隐藏由整层隐藏覆盖，可见行为不变。内联模式不再订阅。示例结构下订阅数 10 降为 6，内联模式 6 降为 0。
- Desktop 影响
  - 现有用法的行为不变：新增属性均有保持原外观的默认值。
  - 深色主题下 CountBadge 与 DotBadge 的预设色发生变化，浅色不变；无头渲染对照中 14 个预设色浅色逐像素一致，深色最大通道差 35。
  - 缺陷修复：`success`、`warning`、`error` 等状态名称此前在 Desktop 上无法解析，指示器渲染为未着色（白色、近乎不可见）；现在解析为主题状态色，浅色与深色均生效（无头渲染对照中浅色与深色下该类徽标均由白色变为主题状态色）。
- Tests
  - 新增 `BadgeExtensionTests`：`Content`、`IsBordered`、状态名与预设名解析、`danger` 等同 `error`、深浅主题切换（含离树期间切换）、可见性跟踪范围、滚动容器内徽标随目标移动、空字符串内容隐藏、Dot `IsBordered`。
  - 新增 `CountBadgeContentTests`：在真实 Desktop 模板上验证计数文本、`Content` 覆盖、字号与前景传递、`IsBordered` 去除描边。

## 2026-10-28

- Motion
  - Count/Dot 改用 `Motion` 子类，移除 Actor、过渡订阅和重复的显隐取消源。
  - 私有 Border 承载动效，indicator 语义节点保留独立样式和变换；专用 Style route 不变。
  - 同一目标和 layer 反向时保持挂载与实际矩阵，旧隐藏请求不能移除新显示状态。
  - 保持计数隐藏期间文本、standalone Dot 静态显示、布局锚定和祖先可见性同步。

## 2026-09-27

- Implementation
  - Rework `RibbonBadgeAdorner` fold layout so the ribbon body and fold triangle are derived from the same coordinate model, and corner transform changes rebuild the rendered fold geometry.
- Tests
  - Add Ribbon fold regression coverage for standalone Start bounds and dynamic corner transform updates.
- Docs
  - Document the RibbonBadge layout and fold rendering contract: body rect, content rect and fold triangle must come from one coordinate model, with pixel-level rendering verification for the fold geometry.
  - Clarify `RibbonBadgeToken` offset, corner transform, darken amount and text padding semantics; align the token scope list with the existing Count/Dot/Ribbon token classes.

## 2026-08-13

- Docs
  - Split the complete `CountBadge`, `DotBadge`, and `RibbonBadge` Semantic Part contracts into `semantic-part.md`; keep the
    overview as a support summary and the implementation document as the runtime node mapping source.

## 2026-08-12

- Semantic Part
  - Add independent descriptors for `CountBadge`, `DotBadge`, and `RibbonBadge` with stable `root`, `indicator`, and ribbon `content` contracts.
  - Preserve Count/Dot owner-scoped styling across Avalonia 12 `AdornerLayer` by separating visual host from logical/style owner.
  - Add static semantic markers to all applicable Badge themes and generated selector-class usage for the runtime Ribbon indicator.
- Gallery
  - Add a truly deferred Semantic Parts Tab with independent Count, Dot, and Ribbon previews.
  - Register only each Count/Dot owner's runtime Adorner as an additional preview root; keep Ribbon on the inline path.
  - Extend `GalleryShowCaseHost` to manage multiple independent previews in one deferred content root.
  - Add a full-width, deferred `v6.1.3` example that demonstrates owner-scoped Semantic Part styling for Count and Ribbon badges.
- Tests
  - Cover descriptor metadata, marker placement, complete owner-scoped selector routes, nested owner isolation, Adorner detach cleanup, deferred Gallery creation, and precise cross-root preview registration.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Badge`.
  - Align generated output paths with `controls/badge/index-cn.md` and `controls/badge/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Badge desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Badge desktop architecture documentation under `docs/controls/desktop/data-display/badge/overview.md`.
  - Add Badge implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Badge control-level changelog.
  - Add Badge Token documentation covering BadgeToken.
