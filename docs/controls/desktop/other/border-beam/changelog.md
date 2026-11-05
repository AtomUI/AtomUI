# BorderBeam Changelog

## 2026-11-04

- 4字段采用依赖默认计划；边框/圆角和端点颜色按实际 BorderBeam owner 投射，独立 motion 语义保持。

本文档记录 BorderBeam 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-09-09

- Fixed
  - 修正默认 `Outset=null` 时的边框几何：流光现在贴合内容边界，不再整体绘制在边框外围。
  - 修复仅作用于 `BorderBeamPresenter.Render()` 的绘制坐标，不改变 `Measure`、`Arrange`、`DesiredSize`、`Bounds` 或父容器布局。
  - 显式 `Outset` 仍只控制 Render 阶段的绘制偏移。
- Tests
  - 增加默认边界不外扩、显式 `Outset` 偏移以及布局边界保持不变的回归测试。

## 2026-09-08

- API
  - 新增 `Count` StyledProperty，默认值为 `1`；渲染层将非正值按一个光束处理。
- Theme
  - 默认 ControlTheme 通过 `TemplateBinding` 将 `Count` 转发给现有 `PART_BeamPresenter`，不增加新的
    template part、Token 或 Visual。
- Implementation
  - 在单一 presenter、单一 `Progress`、单一 Animation 和单一 CancellationTokenSource 内按等距 phase
    绘制多个光束。
  - 一次 render 只构建一份圆角路径关键点和周长度量，由所有光束共享；draw call 随 `Count` 线性增加。
  - `Count` 变化只触发重绘，不重启共享动画。
- Gallery
  - 新增 `Show on hover` 和 `Multiple beams` 延迟加载示例；hover 通过页面 Style 组合 `:pointerover` 与
    `IsMotionEnabled`，不新增公共 hover API。
  - `Multiple beams` 在同一稳定示例中纵向展示 `Count=3` 与 `Count=2` 两张卡片，并使用不绑定具体数量的共享文案。
  - 新增 `Custom container` 延迟加载示例，以标准 Avalonia `Border` 验证普通内容可通过 BorderBeam 自身的
    `BorderThickness` 和 `CornerRadius` 显式对齐边界几何，无需新增运行时 API。
  - Show on hover、Multiple beams 和 Custom container 使用 AtomUI 当前版本 `v6.1.8` 的 Showcase 徽标。
  - 重构 `Customized color` 示例：六组预设使用各自的用途、说明、渐变颜色和显式百分比；Card 通过动态
    Filled Tag 展示用途与颜色停靠点，并使用最大宽度 480 的铺满式 Segmented 布局。
  - 新增 `Duration`、`Size` 和 `Line width` 延迟加载示例，分别展示 `3s` / `6s` / `12s` 动画周期、
    `100` / `56` / `160` 流光段尺寸，以及通过匹配 `BorderThickness=2` 对齐流光与内容边框线宽。
  - Duration、Size 和 Line width 使用 AtomUI 当前版本 `v6.1.8` 的 Showcase 徽标。
  - 为九个 BorderBeam 示例补充稳定 `SourceKey`，并同步英语、巴西葡萄牙语、简体中文和繁体中文资源。
- Tests
  - 增加 `Count` 默认值与模板转发、等距相位、非正值回退、实际绘制数量，以及 Gallery 示例和本地化契约测试。
- Docs
  - 落地 [BorderBeam 多光束与悬停展示设计规格](../../../../superpowers/specs/2026-09-08-border-beam-multi-beam-hover-design.md)，
    明确 `Count` 使用单 presenter、单动画时钟和等距 phase 的架构边界。
  - 明确悬停展示通过 `IsMotionEnabled` 与 `:pointerover` 样式组合表达，不引入 `ShowOnHover` 公共属性。
  - 修正实现文档的模板接入描述：默认模板使用 `TemplateBinding`，BorderBeam 不在 `OnApplyTemplate` 中持有
    template part 引用。
  - 明确 `IBorderBeamAwareControl` 是 opt-in 几何契约，当前产品控件没有内置适配。

## 2026-08-25

- Fixed
  - Pause BorderBeam's infinite Avalonia animation when the control is effectively invisible.
- Docs
  - Clarify that invisible includes hidden Visual ancestors, not only `BorderBeam.IsVisible=false`.

## 2026-07-14

- Theme
  - 取消默认主题对 `SharedToken.EnableMotion` 的绑定，使 BorderBeam 流光默认不受全局 motion 开关影响。
  - 保留实例级 `IsMotionEnabled=false` 关闭流光的行为。
- Docs
  - 更新 Gallery 和控件文档中关于 motion 默认语义的说明。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `BorderBeam`.
  - Align generated output paths with `controls/border-beam/index-cn.md` and `controls/border-beam/semantic-cn.md`.

## 2026-06-19

- Docs
  - 新增 `implementation.md`，记录 BorderBeam 边界感知、颜色归一、动画生命周期、连续流光渲染和维护不变量。
  - 将 `overview.md` 收敛为装饰控件定位、公共契约、状态模型、视觉主题模型和验证入口。
  - 在 Other 分类入口中登记 BorderBeam 实现原理文档。

## 2026-06-18

- Docs
  - 新增 BorderBeam 桌面版架构设计文档。
  - 新增 BorderBeam Token 设计文档。
  - 将 BorderBeam 归入桌面控件 `Other` 分类，并在 Gallery 中使用 `Other` 顶层分类承载。
- API
  - 确立 `BorderBeam` 独立包装控件模型。
  - 确立 `IBorderBeamAwareControl` 与 `BorderBeamGeometry` 作为被装饰控件边界感知契约。
  - 确立 `ColorStops` 使用 `DirectProperty` 暴露实例级可变集合，避免共享默认集合。
- Theme
  - 确立 `PART_ContentPresenter` 与 `PART_BeamPresenter` 的模板结构。
  - 确立 BorderBeam 默认不使用全局 AdornerLayer，不修改被装饰控件模板。
  - 落地 `BorderBeamTheme.axaml`，将默认颜色、边框、圆角和动效参数映射到 SharedToken 与 BorderBeamToken。
- Token
  - 确立 BorderBeamToken 只承载 beam 自有尺寸、透明度、动效时长和渐变映射参数。
  - 确立颜色、线宽、圆角和 motion 开关优先复用 SharedToken。
