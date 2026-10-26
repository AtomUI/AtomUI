# Transfer Token 设计

本文定义 Transfer 家族专属 Token 的语义、消费者和兼容边界。通用规则见 [控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)，家族定位见 [架构设计](overview.md)，实际组合与生命周期见 [实现原理](implementation.md)，变化见 [Changelog](changelog.md)。

## 1. 定位

Transfer 是家族名称。实际 Token scope 为：

| Token 类型 | 公开 owner／identity | 字段数 |
| --- | --- | ---: |
| `ListTransferToken` | `ListTransfer`／`AtomUI:ListTransfer` | 5 |
| `TreeTransferToken` | `TreeTransfer`／`AtomUI:TreeTransfer` | 5 |
| `TransferListViewToken` | `TransferListView`／`AtomUI:TransferListView` | 1 |

三个源码文件位于 `src/AtomUI.Desktop.Controls/Transfer/`。`AbstractTransfer` 负责共享状态与模板协调，没有独立 `TransferToken`。Token 不保存目标 key、当前选择、过滤、树展开、分页页码或视图实例。

## 2. Token 分类

### 2.1 分区尺寸

`ListTransferToken` 与 `TreeTransferToken` 各自定义同名字段，保持独立 identity 和资源标识：

- `ListWidth`：非伸展、未分页分区的默认宽度，180 DIP。
- `ListWidthLG`：现有非伸展分页状态分支的宽度，250 DIP。ListTransfer 由公开 PageSize 进入。TreeTransfer 没有公开 PageSize，owner wrapper 为 internal；但 TransferListView 导出的共享 Avalonia 属性标识可以触发同一分支。它是既有条件消费者，不表示源树支持分页。
- `ListHeight`：两个装饰器的视图区域默认高度，200 DIP。
- `HeaderHeight`：分区 header 默认高度，读取有效 `Global.ControlHeightLG`。

`IsStretchView=true` 时宽度由 owner 的列布局分配；两个宽度 Token 不强行覆盖伸展分支。实例 `ListWidth`／`ListHeight` 本地值仍遵守 Avalonia 属性优先级。`SizeType` 的当前转发边界见实现文档 §5.2，不能从 `LG` 字段名推导出新的三档尺寸分支。

### 2.2 分区内距

两类 owner 各自的 `HeaderPadding` 同时投影到分区 header 与可选 footer。水平内距读取有效 `Global.UniformlyPaddingSM`，垂直内距为：

```text
Ceiling((Global.ControlHeightLG - Global.LineWidth - Global.FontHeight) / 2)
```

结果单位是 DIP；保留该公式的取整和各边语义，不增加统一钳制。它读取有效 Global，独立于 Own `HeaderHeight`，明确改动 `HeaderHeight` 不会自动改写 `HeaderPadding`。

### 2.3 视图分页间距

`TransferListViewToken.PaginationMargin` 默认为 `(0, Global.UniformlyMarginXXS)`，用于列表视图模板的上、下分页 presenter。分页是否启用由视图状态及选择器决定，不写入 Token。

## 3. 控件专项模型中的 Token 使用

三个 Token 均以 `DependencyDefaultsV2` 的静态默认函数求值。11 个默认只依赖有效 Global 或常量，没有 Own→Own 依赖；Control 专属 Global 进入对应 owner 的有效输入。明确 Own 优先，包含 `0` 和零 `Thickness`，移除 Own 后恢复当前 Global 派生值。

根 owner 的 ControlTheme 把各自的尺寸和 header 资源传递给两个 `TransferItemDecorator`。`TransferListViewTheme` 使用自己的分页间距资源，并复用 `ListView` 的主题结构。文本、颜色、边框、圆角及内嵌输入／按钮等资源按实际 SharedToken 和嵌套控件合同消费，不能虚构为这三个 Token 的额外 Own 字段。

## 4. 控件家族影响

- `ListTransfer` 的源、目标列表分区消费 `ListTransferToken`；分页宽度由 `PageSize` 引出的现有状态分支选择。
- `TreeTransfer` 的树源、列表目标分区消费 `TreeTransferToken`；树节点和目标列表各自保留嵌套控件的资源职责。
- `TransferListView` 的分页 presenter 消费 `TransferListViewToken`，包含直接使用及作为源／目标视图的实例。
- `TransferItemDecorator` 接收 owner 模板投影的 header 高度、内距与分区布局值，不新增独立 Own scope。

配置或迁移分别核对两个具体 owner 及分页视图。TreeTransfer.ListWidthLG 归为 Active 条件消费者，保留资源、默认值与现有分支；默认非分页树不消费大宽度。此分类不扩展公开能力：树源 IsSupportPagination=false，没有 TreeTransfer.PageSize，也不为主题编辑新增或推荐树分页开关。

## 5. 兼容性要求

- 保持三个 identity、字段名称／类型、TokenKind 枚举数值、资源 key 和 dense slot；枚举数值与 slot 分开验证。
- 保持 180／250／200 常量和有效 Global 的原默认公式，不把 `HeaderPadding` 改为 Own 依赖。
- 声明 `Revision = 1` 参与生成求值摘要，不代表运行期 `EvaluatorRevision` 的字面值是 1。
- 保持明确 Own／撤回、Global／Control Global、本地实例属性优先级、分页／伸展及当前尺寸转发语义。
- `TargetKeys`、`SelectedKeys`、mask、选择计数、分页状态与用户集合由原状态 owner 管理，不能进入默认函数。

## 6. 验证策略

默认求值变化核对明暗实际资源值、类型、key 枚举数值及 dense slot，覆盖 Global、Control Global、明确 Own 0、撤回和同实例重挂。实际消费者至少覆盖两类 owner 的两个装饰器与 `TransferListView` 分页 presenter。

视觉兼容使用实际产品 Window 和完整明暗主题，保留数据、分区宽高、选中／目标 key、分页分支与原滚动／裁剪；先 A/A，再做 1 DIP 检错及同历史新旧比较。Headless/Skia、真实原生窗口与 NativeAOT 分别报告，不从资源或单元测试通过推定原生最终像素、真实输入或完整家族验收。
