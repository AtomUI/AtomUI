# Transfer Token 设计

本文档定义 Transfer 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Transfer 整体架构见 [Transfer 桌面版架构设计](overview.md)，内部实现原理见 [Transfer 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Transfer Changelog](changelog.md)。

## 1. 定位

Transfer Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ListTransferToken` / `TreeTransferToken`：分别对应 `ListTransfer` / `TreeTransfer`。
- `TransferListViewToken`：对应独立列表视图 `TransferListView` 的分页边距。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ListWidth`、`ListWidthLG`、`ListHeight`、`ItemHeight`、`HeaderHeight` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `ItemPadding`、`HeaderPadding`、`PaginationMargin`、`DataGridSelectionHeaderMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 按源码 Token 语义维护 |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Transfer 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Transfer Token 时必须评估以下范围：

- `ListTransfer`、`TreeTransfer`
- `TransferListView` / `TransferTreeView` 及其条目
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |

## 7. 有效 Token 与作用域边界

`ListTransfer` 与 `TreeTransfer` 分别拥有列表宽高、标题高度和内边距；`TransferListView` 拥有分页边距。
共享装饰框和操作间距通过 `OwnerTokenResource` 读取宿主的有效全局 Token。筛选框外边距、搜索图标颜色和
选择菜单图标尺寸先在无身份装饰框读取，再由 AXAML 绑定投射到内嵌控件；LineEdit 本身保留独立身份。
`TransferListItem` / `TransferTreeViewItem` 对 Token owner 透明，继承实际 TransferListView / TransferTreeView 的全局与动效。
TransferListView、TransferTreeView 使用自己的 Own，复用真实列表/树展示计算；ListView/TreeView 的来源覆盖不进入 Transfer 视图。分组专用 GroupHeaderPadding 和图标式 SelectedIndicatorMargin 只被原 ListView 模板消费，不添加给使用 CheckBox 的 Transfer 模板。

`TreeTransfer.ListWidthLG` 为 Reserved：树穿梭视图不支持分页，没有可达的分页加宽状态。字段与默认值保留以维持配置兼容；
普通宽度使用 `TreeTransfer.ListWidth`。`ListTransfer.ListWidthLG` 仍用于分页列表。

## 独立 Token 身份与共享计算

ListTransfer/TreeTransfer 的4个共同布局字段共享 AbstractTransferToken；TreeTransfer.ListWidthLG 仍 Reserved，ListTransfer.ListWidthLG 仍用于分页加宽，保持终端差异。
