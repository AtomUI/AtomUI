# FloatButton Changelog

本文档记录 FloatButton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-22

- Changed
  - FloatButton / FloatButtonGroup 所在的 `ScopeAwareOverlayLayer` 成为 T4（内容浮动），从共享 `AdornerLayer` 下方移到它上方：内容中的 Badge 和键盘焦点描边不再画在 FloatButton 之上；FloatButton 仍低于同宿主的作用域 Drawer 与作用域 Dialog。
  - 同一宿主中 FloatButton 从 Watermark 和吸顶 Tab 之下移到它们之上，与 Ant Design 一致（watermark 9 < affix 10 < FloatButton 1000）。
  - `ScopeAwareOverlayLayer` 不再承载 Dialog。
  - 浮动状态下的 FloatButton / FloatButtonGroup 自带表面宿主，其内部控件的焦点描边与 Badge 画在 FloatButton 表面之上。
  - ScrollViewer 滚动内容中的 Drawer 仍低于该 ScrollViewer 视口上的 FloatButton。
- API
  - FloatButton 的 public / protected API 不变。
- Docs
  - 层归属见 [视觉层规范](../../../../architecture/systems/rendering/visual-layers.md)。
- Known limits
  - Windows 10 实机（Gallery，150% 缩放）只验证了 FloatButton 自身 Badge 画在各自表面之上、未被相邻 FloatButton 盖住。FloatButton 盖住内容 Badge 没有 Gallery 示例，Windows 未实测，仅有 Headless 证据；macOS 与 Linux 未实测。
- Token：`FloatButtonOutlineAppearance` 不再经 `AppearanceSettings` 传入，它的字段改为 `FloatButtonToken` 上的六个公开 Own Token：
  `HasOutline`、`OutlineOnlyShadow`、`CapsuleDescription`、`UppercaseDescription`、`DescriptionWeight`、
  `OutlineDescriptionLineHeight`（见 [FloatButton Token 设计](token.md)）。默认值等于此前没有描边外观时的行为，各设计语言渲染不变。

## 2026-11-20

- Shadcn 的 Default 浮动按钮使用对应风格的描边、底色与按压反馈。BackTop 和圆形组按按钮绘制描边，方形组按容器绘制外框。Vega 的描边默认阴影为 `shadow-xs`。
- Shadcn 的带说明浮动按钮改为内容测量，图标与文字横排。方形纯图标按钮使用可辨的风格圆角，Gallery 的说明示例按视口调整排列并共用垂直中心线。

## 2026-11-17

- Changed
  - 默认（Default）浮动按钮的表面与前景改由 `FloatButtonToken` 自有槽位 `DefaultBg` / `DefaultHoverBg` / `DefaultActiveBg` /
    `DefaultIconColor` 表达（Ant 默认分别为 `ColorBgElevated` / `ColorBgTextHover` / 透明 / `ColorText`，渲染不变）；
    `AbstractFloatButtonTheme` 改绑这组槽位，并移除已被 `ButtonType` 两个分支完全遮蔽的基座 `Foreground`（`ColorText`）死读。
  - 方形胶囊容器（`FloatButtonItemsControl`）的填充由 `OwnerTokenResource ColorBgElevated` 改绑同一表面槽位
    `FloatButtonTokenResource DefaultBg`：容器失去阴影后（PrimeNG）必须由填充表达表面，否则白底在白卡片上不可见；
    Ant 基线 `DefaultBg` 仍等于 `ColorBgElevated`，渲染不变。
  - `PrimeNGFloatButtonTokens` 补官方 `button.secondary` 表面：`surface.100` / `200` / `300` / `600`（暗色 `800` / `700` /
    `600` / `300`）。官方没有“白色无阴影”按钮，Ant 白底默认在无阴影后会不可见，故中性浮动按钮落到 secondary。
  - 浮动按钮族阴影改由 `FloatButtonToken` 自有 Token `FloatButtonShadows` 表达：`AbstractFloatButtonTheme`（独立按钮与
    嵌入圆钮）、`FloatButtonGroupTheme` 与 `FloatButtonGroupHostTheme` 的方形容器改绑 `FloatButtonTokenResource
    FloatButtonShadows`；默认值仍为全局 `BoxShadowsSecondary`，Ant Design 渲染不变。
  - `AtomUI.Themes.PrimeNG` 新增 `PrimeNGFloatButtonTokens`（Aura/Lara/Nora × 亮/暗共六个算法）：PrimeNG 设计语言下
    浮动按钮族无阴影，对齐 `@primeuix/themes` 3.1.1 官方取值（button 各 severity 与 speeddial 均无 resting 阴影）。

## 2026-11-08

- Changed
  - FloatButton 家族主题的全局 Token 改读各自身份：`AbstractFloatButtonTheme` 与内部 `FloatButtonItemsControlTheme` 用
    `OwnerTokenResource`，Host、Group、BackTop 主题用各自的 `XxxTokenResource`；徽标色改读 owner 的有效 `ColorError`。
  - 图标颜色跟随按钮 `Foreground`（`OwnerTokenResource ColorText`），`Controls[FloatButton]` 的 `ColorText` 生效。
  - `FloatButtonItemsControl` 不再自行设置方形阴影，`FloatButtonGroup.BoxShadow`（含 Host 转发）到达菜单面板。
- Fixed
  - BackTop 滚动投射 `IsActive` 改用 `SetCurrentValue`，不再以本地值覆盖调用方或主题的绑定来源。
  - BackTopFloatButton 模板的徽标画布改名为 `PART_BadgeLayout`，徽标可以显示。
- Removed
  - 删除无消费者的 Own Token `SquareBadgeOffset`、`CircleBadgeOffset`。

## 2026-10-31

- Behavior
  - Issue #518：`FloatButtonGroup` 交互统一以 current value 语义回写 `IsOpen`，直接使用 group 并设置或绑定 `IsOpen` 时也能经 TwoWay 回写；移除 host 侧重复的 open/close request 链路。
  - 设置本地值 `IsOpen=True` 的 group 点击 trigger 会收起（此前保持打开）。
  - 禁用的 group 不再响应 click trigger 的打开/收起。
  - `Clicked`、`Opened`、`Closed` 事件此前已声明但从未触发，现按文档时机触发。
- Gallery
  - 受控模式示例两个 group 均改为 `Trigger="Click"`，对齐 Ant Design `controlled` demo。

## 2026-10-27

- Motion
  - BackTop 直接在既有 Panel 上声明 Fade 显隐，移除 Actor 包装、方向 guard 和独立显隐执行器。
  - FloatButtonGroup 通过静态菜单表面隔离 list 自定义变换，播放与连续替代统一交由 Core Motion。
  - 保持 trigger 尺寸决定的四方向位移、透明度反向连续性、进度环和 Semantic Part 路由。


## 2026-09-12

- API
  - Add `IsShowProgress` (`bool`, default `false`) to `BackTopFloatButton` and `BackTopFloatButtonHost`, aligned with antd 6.6.0 `FloatButton.BackTop` `showProgress`.
- Theme
  - Add the internal `BackTopProgressRing` template node to `BackTopFloatButtonTheme.axaml`; visibility defaults to hidden and is enabled by the `IsShowProgress=True` selector. Stroke width and colors consume global tokens (`LineWidthBold`, `ColorBorderSecondary`, `ColorPrimary`); no control token is introduced.
- Implementation
  - Derive the internal scroll progress from the `Target` ScrollViewer in `ScrollChanged` and `OnLoaded`; Avalonia `ScrollChanged` covers extent/viewport changes, so no dedicated resize listener is required.

## 2026-08-16

- Semantic Parts
  - Add Ant Design-aligned Semantic Part descriptors for `FloatButton`, `BackTopFloatButton` and `FloatButtonGroup`.
  - Add static template markers and generated owner-scoped style contracts without runtime discovery; `FloatButtonTheme` now owns its leaf shape templates so the generator can statically validate markers.
  - Document the lazily materialized trigger-mode `list` part and the exclusion of badge adorners, tooltip and item-level group parts.
- Gallery
  - Add the Semantic Parts Preview and Ant Design-aligned custom styling example using AtomUI `v6.1.3` version metadata.

## 2026-07-09

- Docs
  - Document the FloatButton host command projection model based on Avalonia Button command semantics.
  - Clarify that `FloatButtonGroupHost` trigger controls open/close while child `FloatButton` items own business commands.
  - Define group child `DataContext` inheritance requirements for command bindings inside overlay-hosted groups.
- API
  - Establish `Command` and `CommandParameter` as host-level projected command contracts for `FloatButtonHost` and `BackTopFloatButtonHost`.
- Implementation
  - Define lifecycle and test requirements for host-to-overlay command projection, `CanExecute` behavior and group child command binding.

## 2026-07-06

- API
  - Make `FloatButtonGroup.IsOpen` and inherited `FloatButtonGroupHost.IsOpen` default to `BindingMode.TwoWay`.
- Implementation
  - Use current value updates for group and host open/close interactions so controlled bindings are not replaced by style-priority values.
- Docs
  - Document the controlled open state contract for group and host usage.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `FloatButton`.
  - Align generated output paths with `controls/float-button/index-cn.md` and `controls/float-button/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete FloatButton desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish FloatButton desktop architecture documentation under `docs/controls/desktop/general/float-button/overview.md`.
  - Add FloatButton implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add FloatButton control-level changelog.
  - Add FloatButton Token documentation covering FloatButtonToken.
