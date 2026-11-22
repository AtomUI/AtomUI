# Pagination Changelog

本文档记录 Pagination 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-20

- Token
  - `PaginationToken` 新增页码项颜色、悬停、选中态颜色 / 边框、选中字重与圆角 Token（`ItemColor`、`ItemHoverColor`、`ItemHoverBg`、`ItemActiveColor`、`ItemActiveBorderColor`、`ItemActiveHoverColor`、`ItemActiveHoverBorderColor`、`ItemActiveFontWeight`、`ItemBorderRadius`），默认值为原先直接读取的全局 Token。其中 `ItemColor`、`ItemHoverColor`、`ItemHoverBg`、`ItemBorderRadius` 位于 `AbstractPaginationToken`，Pagination 与 SimplePagination 各自主题赋给控件的普通属性，再经模板绑定传给页码项，SimplePagination 的上一页 / 下一页读自身 Token；PrimeNG 采用 36px 圆形页码与实心选中态。
- Fixed
  - Keep page-size and jump-input focus rings visible outside the pagination row. Size the compact layout gap to accommodate the theme's focus ring.
  - In compact Shadcn simple pagination, inset the page input while keeping the navigation targets at their existing size.
- Performance
  - 去掉 `PaginationNavItem` 的 6 个可继承主题输入。Pagination / SimplePagination 在根部把 Own Token 赋给自身的普通属性，模板用 `TemplateBinding` 读取，导航条在准备容器时把项目尺寸、禁用色与内边距直接绑定到各项，快速跳转条随创建一起绑定并在移除时释放。渲染不变，单个分页器分配约少 1%–2%，耗时持平。

## 2026-11-14

- Performance
  - `PaginationNav` creates its nine fixed navigation slots when its template applies instead of in the constructor. Hidden, empty and single-page paginations no longer build and style them (an empty Pagination: 382 KB → 80 KB). The items panel is built only when the presenter is measured, after this point, so containers are prepared with the same item count and order as before.

## 2026-11-10

- Theme：终端 Own 与实际 owner 对齐，共享计算不传播其他控件的覆盖；字段迁移与保留范围见 [Token 设计](token.md)。

## 2026-11-08

- Changed
  - 主题改读控件有效 Token：Pagination、SimplePagination 以各自 TokenResource 读取，共用的页码项以 `OwnerTokenResource` 读取宿主值；
    选中页码字重改读 `FontWeightStrong`（与 antd 一致，默认 600 不变）。
  - `PaginationMiniQuickJumperInputWidth` 接通小尺寸快速跳转与简洁模式输入框宽度，`ItemInputBg` 接通简洁模式输入框背景；
    `PaginationItemPaddingInline` 改为作用于页码文字（此前写在无子元素的背景边框上，从未生效），默认值改为仅水平方向；
    `ItemLinkBg`、`InputOutlineOffset` 标为保留 Token。
  - 可见变化：小尺寸快速跳转输入框宽 44px（此前 50px）；四位及以上页码两侧各有 6px 内间距。

## 2026-10-20

- Fixed
  - `PaginationNavItem` 构造时建立共享初始化会话，覆盖导航容器挂载后才同步的初始页码状态；加载后恢复前景、边框和背景过渡。

## 2026-09-26

- Behavior
  - 页大小或总数变化导致当前页收敛时，只发布最终分页状态的一次 `CurrentPageChanged` 通知，避免递归属性更新产生旧页码重复意图。
- Theme
  - 保持 Pagination 默认顺序 total info、page navigation、page-size changer、quick jumper；分页宿主 Extra Content 只能在 Pagination 外围组合，不得移动 page-size selector。

## 2026-09-22

- Design
  - 定义 page-size changer 的局部组件替换模型：`Pagination` 继续作为 `PageSize` 唯一状态 owner，
    `PaginationSizeChangerContext` 只负责向模板投影有效 `PageSize` / `SizeType` 并转发页大小更新请求。
  - 默认 ComboBox 与自定义模板内容互斥；`IsShowSizeChanger`、页数计算、当前页收敛、禁用态和事件语义保持不变。
- API
  - 定义 `SizeChangerTemplate`，默认值为 `null`；空值使用现有 ComboBox，非空模板以
    `PaginationSizeChangerContext` 为数据上下文。
- Theme
  - `PART_SizeChangerPresenter` 保持唯一 page-size changer 宿主，不新增 Semantic Part 或 Pagination Token。
- Gallery
  - 新增 `pagination-custom-size-changer` ShowCaseItem，使用 `NumericUpDown` 展示 `SizeChangerTemplate`，并标记为 `v6.2.1`。
- Docs
  - 在 overview 与 implementation 中补充公共契约、组合结构、状态流、生命周期、AOT 边界和验证矩阵。

## 2026-09-21

- Behavior
  - 页码窗口对齐 `@rc-component/pagination` 1.4.0：默认保持 7 个中间项，`JumpPrevious` / `JumpNext`
    默认跳转 5 页，`IsShowLessItems=True` 时缩减为 5 个中间项并跳转 3 页。
  - 快速跳页项默认显示省略号，悬停或键盘聚焦时显示主色双箭头；点击与 Enter 使用同一页码变更路径。
  - 页码重排原地复用仍处于同一槽位的快速跳页容器和图标，仅清理不再使用的尾部容器，避免点击后丢失
    `:pointerover` 并重启透明度过渡。
- API
  - 新增 `IsShowLessItems` 与 `IsShowPrevNextJumpers`。
- Semantic Part
  - `JumpPrevious` / `JumpNext` 继续排除在 `Pagination.item` 之外，固定容器池由 11 个缩减为 9 个。

## 2026-08-22

- Semantic Part
  - 为 `Pagination` 与 `SimplePagination` 公开 `root` + `item` Semantic Part，对齐 Ant Design 6 的
    `PaginationSemanticType`（since 6.2.0）；新增 `Pagination.SemanticParts.cs` 与
    `SimplePagination.SemanticParts.cs` descriptor，生成 `PaginationItemStyle` 与
    `SimplePaginationItemStyle`（`AtomUI.Theme.Styling`）。
  - `Pagination.item` 为运行时标记（`RuntimeCreated`），路由经 `semantic-scope-nav` 作用域；Ellipsis 单元格
    动态移除 `semantic-item` marker，对应上游 jump-prev / jump-next 不接受 `styles.item`。`SimplePagination.item`
    为静态标记，只覆盖上一页/下一页，排除快速跳转输入与信息文本。
  - `SimplePagination` 新增 AtomUI 扩展的 `info` Part（`TextBlock`，静态标记），覆盖
    `PART_InfoIndicator`（"当前页 / 总页数"），生成 `SimplePaginationInfoStyle` 用于格式化分页信息文本；
    上游 `PaginationSemanticType` 无对应成员。
  - 新增 `docs/controls/desktop/navigation/pagination/semantic-part.md`，overview 与 implementation 同步链接。
- API
  - `AbstractPagination` 新增 `BorderDashArray` / `BorderDashOffset` 根框架属性，补齐 `styles.root` 的虚线
    边框定制面；`Pagination` 与 `SimplePagination` 共用。
- Theme
  - `PaginationTheme.axaml` 在 `PART_Nav` 上声明 `Classes.semantic-scope-nav="True"` 作用域标记；
    `SimplePaginationTheme.axaml` 在上一页/下一页节点上声明 `Classes.semantic-item="True"` 静态标记，
    并在 `PART_InfoIndicator` 上声明 `Classes.semantic-info="True"` 静态标记。
  - 两个根模板在根布局外包裹 TemplateBind 根视觉属性的 `atom:DashedBorder`：`Background` /
    `BackgroundSizing` / `BorderBrush` / `BorderThickness` / `CornerRadius` / `Padding` 来自
    `TemplatedControl`，`StrokeDashArray` / `StrokeDaskOffset` 来自 `BorderDashArray` / `BorderDashOffset`，
    使 `root` Part 视觉定制（含虚线边框）可渲染，默认值不改变既有外观。
- Gallery
  - Showcase 迁移到 `GalleryShowCaseHost`，增加 Pagination 与 SimplePagination 两个 `SemanticPartPreview`
    （SimplePagination 预览含 `root` / `item` / `info` 三个 Part 描述）以及 `pagination-semantic-part` 语义样式
    示例：两行 `Pagination` 共享虚线 root 边框与内边距，对象式 `styles.item` 圆角、函数式
    （`SizeType=Small`）`styles.item` 背景与间距，并用嵌套 `^:selected` 保留主题选中态，整体对齐 antd
    style-class 示例。

## 2026-07-06

- API
  - 将 `CurrentPage` 和 `PageSize` 设为默认 `TwoWay` 受控分页状态。
- Implementation
  - 内部页码和页大小更新改为 `SetCurrentValue`，避免破坏外部 binding owner。
- Gallery
  - 增加默认双向绑定示例，标记为 `v6.0.8`。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Pagination`.
  - Align generated output paths with `controls/pagination/index-cn.md` and `controls/pagination/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Pagination desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Pagination desktop architecture documentation under `docs/controls/desktop/navigation/pagination/overview.md`.
  - Add Pagination implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Pagination control-level changelog.
  - Add Pagination Token documentation covering PaginationToken.
