# ListBox Changelog

本文档记录 ListBox 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-11-14

- Design
  - 确立独立 ListBox 的键盘焦点模型（overview §8.4）：条目是焦点目标，列表是一个 Tab 停靠点；单选时选择跟随焦点，
    多选时方向键只移动焦点、Space / Enter 切换、`Shift` 范围选择，与 TreeView 的提交策略一致。PageUp / PageDown 由
    ScrollViewer 滚动视口（Avalonia 12.1.6 虚拟化面板无翻页导航）。
- Theme
  - `ListBoxItemTheme` 的 `Frame` 成为模板焦点目标并内侧绘制焦点描边（`IsTemplateFocusTarget`、`FocusOutlineAdorner.IsInset`）。
- Fixed
  - CandidateList 只认 `IListItemData.IsEnabled`，Mentions 的禁用选项（`IMentionOption`）因此仍可被键盘导航选中并按启用项渲染。新增 internal `ItemEnabledResolver`，由宿主提供数据项启用判定，同时驱动候选可用性和容器 `IsEnabled`（prepare 时按数据项写入，不沿用按 index 保存的旧值）；未设置时行为不变。
  - CandidateList（AutoComplete、Mentions 的候选列表）在虚拟化回收后高亮错位：回收容器继承了活动候选的高亮，滚回后活动候选又不高亮。原因是高亮在 `NotifyRestoreDefaultContext` 中按尚未更新的 `VirtualIndex` 判断，且该回调只对 `IListItemData` 执行（Mentions 选项不经过）。改为在 `PrepareContainerForItemOverride` 的基础 prepare 之后按本次 `index` 设置。
  - 默认主题下 ListBox 无法获得键盘焦点：`ListBoxItemTheme` 设置了 `Focusable=False`，点击条目后焦点为空，方向键无效。
    条目恢复可聚焦；CandidateList、Cascader 搜索结果列表的条目主题改为显式 `Focusable=False`，焦点仍留在宿主输入框。
  - 每按一次方向键选择移动两项：`OnKeyDown` 先 `MoveSelection` 再调用 Avalonia `ListBox.OnKeyDown` 又移动一次。方向键
    改为只由 ListBox 处理。
  - 多选模式下 Space / Enter 整体替换选择：Avalonia `ListBoxItem` 的按键提交被拦截，改按多选提交策略切换。
- Docs
  - `implementation.md` §7.5 更新 CandidateList 扩展点与候选高亮的恢复时机，补充候选可用性判定与 `ItemEnabledResolver`。
  - `overview.md` 新增 §8.4 键盘焦点模型，§5 补充焦点描边、§8.3 补充 CandidateList 的宿主焦点约定；`implementation.md`
    重写 keyboard navigation 流程。

## 2026-11-10

- Theme：终端 Own 与实际 owner 对齐，共享计算不传播其他控件的覆盖；字段迁移与保留范围见 [Token 设计](token.md)。

## 2026-11-08

- Changed
  - 主题改读控件有效 Token：`ListBoxTheme` / `ListBoxItemTheme` 以 `OwnerTokenResource` 读取全局 Token（CandidateList 与 Cascader 过滤列表复用
    这两个主题，各自跟随自己的 owner）；`ListBoxItem` 标为 Token owner 透明条目容器，`Controls[ListBox]` 作用于条目及其过渡时长。
  - `ItemDisabledColor` 接通 disabled 条目文字色（默认 `ColorTextDisabled`，渲染不变）。
  - 条目分割线改为 internal `SplitLineWidth`（读取 `LineWidth`）只画底边，取代 `BorderThickness="0,0,0,1"` 字面量；应用或 Semantic Part 样式
    设置的 `BorderThickness` 仍整体替换分割线。
- Fixed
  - 选中标记的样式选择器指向 `IconPresenter`，与模板中的 `IconTemplatePresenter` 不匹配，主色、`IconSize` 与 `SelectedIndicatorMargin`
    从未生效；可见变化：选中标记现为 `ColorPrimary`，尺寸为 `IconSize`。

## 2026-10-20

- Fixed
  - `ListBoxItem` 在构造阶段抑制过渡，覆盖生成容器挂载后才写入的初始选择；自身加载完成后恢复最新集合。父控件在清理容器之前复位会话，使虚拟化清理和下次挂载共用初始化边界。

## 2026-08-18

- Design
  - 确立 root 外框与条目分割线统一使用 `ColorSplit` 的视觉基线，root 外框承担列表闭合线。
  - 条目表面保持直角，`ContentPadding` / `ItemMargin` 归零，条目直接贴合 root 外框内边缘。
  - 定义条目分割线状态机：最后一项分割线由 root 外框下边缘闭合，`IsBorderless` 时保留；集合变化后重新同步所有
    已实现容器。
  - root `Frame` 开启 `ClipContentToCornerRadius` 内容裁剪（DashedBorder 按外框内边缘构建圆角裁剪几何），条目
    hover / selected 背景不溢出圆角内边缘；外框环由 `Frame` 自身一次绘制，无叠加节点。
- Token
  - `ContentPadding` 与 `ItemMargin` 默认值改为 `Thickness(0)`；`ItemPaddingSM` / `ItemPadding` / `ItemPaddingLG`
    改为仅水平 padding。
- Docs
  - 新增 [ListBox Semantic Part 契约](semantic-part.md)，定义 `root` / `item` 的 Selector、类型、数量、分割线基线与
    验证契约。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ListBox`.
  - Align generated output paths with `controls/list-box/index-cn.md` and `controls/list-box/semantic-cn.md`.

## 2026-06-20

- Docs
  - 新增 ListBox 桌面版架构设计文档，记录控件定位、公共契约、行为状态模型、视觉主题模型、兼容性不变量和专项模型。
  - 新增 ListBox 桌面版实现原理文档，记录源码职责、容器生命周期、过滤、点击、空状态、虚拟化上下文和 CandidateList 复用边界。
  - 新增 ListBox Token 设计文档，记录 root 结构、条目文字、条目背景、条目间距、选中指示器和过滤 Token 边界。
  - 在 Data Display 分类入口中登记 ListBox 文档。
