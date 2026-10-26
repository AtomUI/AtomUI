# Segmented Changelog

本文档记录 Segmented 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-10-26

- Token
  - 14项默认接入V2，三项自动最小高度依赖有效TrackPadding，明确下游值及0继续优先。
  - 自动高度增加0下限，保留大padding配置及自然内容测量；标准默认、字段和资源身份保持。
- Docs
  - 同步Segmented／SegmentedItem的生成注册与Own职责，移除旧ScopeProvider构造描述。

## 2026-10-20

- Fixed
  - 根控件与 item 使用共享初始化会话暂时覆盖有效过渡集合；初次选中块和选中外观直接提交，取消动态 Ready selector。
  - 相同最终目标的重复排列保留正在执行的位移和尺寸过渡，首次切换继续使用原有时长与缓动。
- Docs
  - 同步 [共享状态动效初始化设计](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)
    及容器有效性、重挂载、重模板和延迟 Items 的实际实现与验证。

## 2026-10-03

- Fixed
  - 水平自然和 expanding 布局按最终交叉轴高度排列 item，保留自然宽度与 item 尺寸约束。

## 2026-09-17

- Motion
  - 选中滑块飞行动画对齐上游 antd 契约：位移与尺寸同时插值，时长 `MotionDurationSlow`（300ms），缓动
    `cubic-bezier(0.645, 0.045, 0.355, 1)`（motionEaseInOut），替代此前的 200ms 线性过渡。
  - item 文字与图标颜色切换加入 `MotionDurationMid` 柔化过渡，对齐上游 item color transition。
- Design
  - 选中背景改为由选中滑块独占承载：item 的 `:selected` 不再绘制 `ItemSelectedBg`，任何时刻选中背景只由
    根控件 render 层滑块表达，消除滑块飞行期间目标项提前点亮的双重选中视觉。
  - 滑块飞行期间屏蔽非选中 item 的 hover/pressed 背景遮罩（item 内部状态 `IsThumbMotionActive`），对齐上游
    `thumb ~ item::after` 透明规则。
- Implementation
  - `SetupSelectedThumbRect(bool animate)` 区分选择切换（允许飞行）与布局校准（瞬移）；滑块首次出现、
    无选中来源、布局跟随一律瞬移。
  - 无选中项（如 Form `ClearFormValue`）时滑块停止绘制，不再残留幽灵滑块。
  - 飞行抑制态由过渡时长驱动的 `DispatcherTimer` 清除，控件 detach 时停止并清理。
- Tests
  - 新增 `SegmentedThumbMotionTests`：滑块过渡时长/缓动契约、选中 item 背景透明、飞行抑制态生命周期、
    motion 关闭不置位、清除选中隐藏滑块、detach 清理。

## 2026-08-19

- Design
  - 公开 Semantic Part：`Segmented` 单一 owner 发布 `root` / `item` / `icon` / `label` 四个 Part，对齐上游 6.6.0
    `SegmentedSemanticType`（`classNames` / `styles` 均为 `{ root?, icon?, label?, item? }`）。
  - `SegmentedItem` 不持有独立 descriptor：它是运行时容器，职责由 `item` Part 表达；item 模板内
    `IconPresenter#IconPresenter` 与 `ContentPresenter#Content` 通过 `icon` / `label` Part 以多跳 route
    （`> .semantic-item /template/ .semantic-*`）公开。
  - 选中滑块不属于任何 Part：由 owner `Render` 直接绘制、没有 Visual 节点，上游也没有对应 Semantic key。
- Implementation
  - `Segmented.CreateContainerForItemOverride` 与 `PrepareContainerForItemOverride` 用生成常量
    `SegmentedSemanticParts.ItemClass` 幂等建立 `semantic-item` marker，覆盖 generated 与 explicit 两条容器路径。
  - `SegmentedItemTheme.axaml` 的 `IconPresenter` / `ContentPresenter` 模板节点声明
    `Classes.semantic-icon` / `Classes.semantic-label` 静态 marker。
  - 生成 `SegmentedItemStyle` / `SegmentedIconStyle` / `SegmentedLabelStyle` owner-scoped 语义样式类。
- Gallery
  - ShowCase 迁移到 `GalleryShowCaseHost`，新增 Semantic Parts 预览（单个 preview 同时承载横向 + 纵向两个 Segmented，
    悬停 `root` / `item` / `icon` / `label` 卡片跨两个控件同时高亮）与自定义语义结构的样式示例
    （`segmented-semantic-part`，含横向与纵向两个实例）。
- Docs
  - 新增 [Segmented Semantic Part 契约](semantic-part.md)，定义四个 Part 的 Selector、类型、数量语义、尺寸基线与
    验证契约。

## 2026-08-17

- Layout and Theme
  - `OptionButtonGroup` 根渲染补画自身 `Background`（此前该属性被忽略），背景绘制在外框、分隔线与选中描边之下；Calendar Header 的 Month/Year 切换组由此获得白色容器背景且不遮挡边框。

## 2026-07-29

- Design
  - 定义横向和纵向共享同一选择、容器和滑块模型的方向布局契约。
  - 定义垂直 `IsExpanding` 只适配父容器宽度、不扩展父容器高度的组合语义。
  - 定义 `SegmentedShape.Default` / `Round`，Round 统一覆盖轨道、item 和滑块为胶囊几何。
- API
  - 为根控件定义默认 `Horizontal` 的 `Orientation` 和默认 `Default` 的 `Shape`。
- Interaction
  - 定义四方向键按前后顺序循环选择，并跳过 disabled 和 hidden item。
- Layout and Theme
  - 选中滑块尺寸以容器最终 `Bounds.Size` 为准，并在 arrange 完成后校准。
  - Round 不新增 Design Token、Visual、template part 或伪类，Shape 覆盖位于 SizeType 圆角分支之后。
- Gallery
  - 按参考设计源码新增 Vertical、Round Shape 与 Dynamic 示例；Round 示例支持通过 `small`、`medium`、`large` 动态切换胶囊控件尺寸，Dynamic 示例支持运行时追加选项。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Segmented`.
  - Align generated output paths with `controls/segmented/index-cn.md` and `controls/segmented/semantic-cn.md`.

## 2026-06-22

- Docs
  - 建立 Segmented 控件文档目录，补齐 `overview.md`、`implementation.md`、`token.md` 和 `changelog.md`。
  - 记录 Segmented 的公共契约、单选状态、选中滑块、expanding 布局、Custom SizeType、主题结构、Token 分类和验证策略。
  - 在 Data Display 分类入口中登记 Segmented 文档。
