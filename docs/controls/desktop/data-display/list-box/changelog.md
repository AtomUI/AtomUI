# ListBox Changelog

本文档记录 ListBox 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-10-24

- Own默认改为DependencyDefaultsV2/Revision1，保留缺省值、字段/类型及资源ID。
- ItemDisabledColor接通实际disabled文字，其有效Global默认、明确Own/透明值及清除均进入原style Setter。

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
