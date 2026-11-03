# FloatButton Changelog

本文档记录 FloatButton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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

- Preserve eight Own defaults under DependencyDefaultsV2/revision1; leave the two legacy scalar badge-offset fields pending a documented classification review.
- Apply existing typography and composition Global roles to the actual public owners while retaining native priorities and Host ownership.
- Retire badge visuals/projections on mode, template and tree changes; restore token colour after clearing instance colour and connect BackTop's declared badge layout.
- Release Group child projections at membership exit and project list Padding into both visible templates.
- Preserve normal BackTop visibility across rapid state/template/tree changes; release retired scroll requests and retain exact scroll completion plus caller Offset bindings.
- Keep full CT15 visual/AOT, input/platform and family exit acceptance open.

## 2026-10-29

- Theme / Implementation
  - Host的运行时按钮完整接入/移除逻辑子集合，跟随局部主题与热资源更新；定位使用视觉父层，卸载时解除层尺寸订阅。

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


CT15清理后限定验收：FloatButton精确46项通过、NET8零警告零错误，临时49用例/媒体/驱动已清理；65对具体静态及两offset分类仍待用户审查，完整家族不因此关闭。详见[正式限定结果](../../../../superpowers/plans/2026-10-22-custom-theme/float-button-consumers-results.md)。


2026-10-31：65对具体静态（含列明派生换行/裁剪）与两scalar offset Reserved兼容分类已获用户限定批准；两scalar保持未消费，以公开BadgeOffset Point定制，完整家族/CT15及其他边界保留。该批准不授权提交。

## 2026-11-01

- Bind BackTop's target scroll subscription to visual membership, so replacing Target before attach or after detach does not retain the retired button through a living ScrollViewer. Keep attach-time progress restoration and normal return-to-top behavior.
