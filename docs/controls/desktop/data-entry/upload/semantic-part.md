# Upload Semantic Part 契约

本文档定义 `Upload` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。整体设计见
[Upload 桌面版架构设计](overview.md)，真实模板与容器生命周期见
[Upload 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

Ant Design 6 的 Upload Semantic DOM 包含 `root`、`list`、`item` 和 `trigger`。AtomUI 的 `Upload` 公开其中能够由
同一 owner-relative route 稳定表达的 `root`、`list` 与 `item`：Text/Picture 模式的 trigger 位于 list 外，
PictureCard/PictureCircle 模式的 trigger 作为 display-only append slot 位于 list 内，两类路径不能由同一个稳定
`SelectorRoute` 表达。触发入口继续通过 public `UploadTrigger` owner 自身定制，不使用宽泛 descendant selector，
也不穿透用户提供的 TriggerContent 子树。

## 1. Semantic Parts

| Part | Selector | SelectorRoute | ContractType | Cardinality | RuntimeCreated |
| --- | --- | --- | --- | --- | --- |
| `root` | owner | 不适用 | `Upload` | `Single` | `false` |
| `list` | `.semantic-list` | `/template/ .semantic-list` | `ItemsControl` | `Single` | `false` |
| `item` | `.semantic-item` | `/template/ .semantic-list > .semantic-item` | `TemplatedControl` | `Multiple` | `true` |

三个 Part 的 `Customization` 分别为 `Root`、`Selector`、`Selector`，`CrossVisualRoot` 均为 `false`，`Since`
均为 `6.0`。`root` 是隐式 Part，不声明 `.semantic-root` marker。

### 1.1 `root`

`root` 是 `Upload` owner 本身，承载 `Files`、`TriggerContent`、`ListType`、`IsShowUploadList`、输入管线、上传队列、
Form 值投影和生命周期。每个 Upload 实例恰好一个 root，并作为 `list` 与 `item` 生成 Style 的 owner scope。

root 定制直接使用 `Upload` 的 public 控件契约，例如尺寸、对齐、透明度、裁剪、Classes 和实例 Styles。当前内置模板
不把 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 或 `Padding` 投影为独立根 frame，因此这些属性
不构成 Upload root 的表面绘制保证。应用需要完整替换根表面时，应提供自己的 ControlTheme，并同时实现本文件定义的
`list` 和 `item` marker 契约。

### 1.2 `list`

`list` 表示当前 `ListType` 使用的唯一文件列表：

- Text 与 Picture 模式由 `UploadList` 实现。
- PictureCard 与 PictureCircle 模式由 `UploadPictureShapeList` 实现。

两种实现都是 internal 类型，因此公共 `ContractType` 使用最低稳定 public 类型 `ItemsControl`。应用可以稳定设置
`Background`、`Padding`、`Margin`、尺寸、对齐、透明度等 `ItemsControl`/`TemplatedControl` 属性，不得依赖
`UploadList`、`UploadPictureShapeList`、`PART_ItemsPresenter` 或内部 ScrollViewer 的具体类型与名称。

每个内置 Upload 模板恰好实例化一个 list 节点。`IsShowUploadList=false` 只隐藏节点，不移除 marker，因此 cardinality
仍为 `Single`。切换 `ListType` 会重新选择根 ControlTemplate，但新的模板继续提供相同 `semantic-list` 契约。

### 1.3 `item`

`item` 表示每个真实 `UploadFileItem` 对应的列表容器。Text、Picture、PictureCard 和 PictureCircle 分别使用
`UploadTextListItem`、`UploadPictureListItem` 或 `UploadPictureShapeListItem`，这些实现均为 internal 的
`TemplatedControl` 派生类型，因此公共 `ContractType` 为 `TemplatedControl`。

item marker 在 `UploadList.CreateContainerForItemOverride` 创建容器时通过生成的 `UploadSemanticParts.ItemClass`
一次性添加。Pending、Uploading、Success、Failed 和 Cancelled 只替换或切换容器内部模板，不替换 item owner，
因此 marker、route 和应用 Semantic Style 在状态变化期间保持稳定。

`item` 明确不包含以下节点：

- PictureCard/PictureCircle 的 `UploadAppendContentItem` append trigger；它不进入 `Files`。
- item 内部的缩略图、文件名、进度条、遮罩和操作按钮；这些节点尚未形成独立公共 Part。
- 用户 `TriggerContent` 或 `ItemTemplate` 生成的子树。

空文件集合允许零个 item；非空集合中每个真实文件恰好一个 marker，所以 cardinality 为 `Multiple`。

## 2. ListType 与状态矩阵

| ListType | list 实现 | item 实现 | Trigger 位置 | 尺寸基线 |
| --- | --- | --- | --- | --- |
| `Text` | `UploadList` | `UploadTextListItem` | list 外 | 内容高度、Text list Token 与 Shared spacing |
| `Picture` | `UploadList` | `UploadPictureListItem` | list 外 | Picture preview/content Token 与 item margin |
| `PictureCard` | `UploadPictureShapeList` | `UploadPictureShapeListItem` | list 内 append slot | `PictureCardSize` 与 WrapPanel spacing |
| `PictureCircle` | `UploadPictureShapeList` | `UploadPictureShapeListItem` | list 内 append slot | `PictureCardSize`、圆形 corner radius 与 WrapPanel spacing |

Upload 不实现 `SizeType`。Semantic Style 总是在当前 ListType 已选定的完整 Token/模板尺寸基线上叠加 Setter；应用不得
把 PictureCard/PictureCircle 的固定卡片尺寸假定为 Text/Picture 的默认尺寸，也不得通过 Semantic Part 绕过
`ListType` 的状态模板选择。

| 文件状态 | item marker | 内部呈现 |
| --- | --- | --- |
| `Pending` | 保留 | Text header 或 Picture pending content |
| `Uploading` | 保留 | 进度条或 uploading content |
| `Success` | 保留 | 成功文本、图片 preview 或 fallback content |
| `Failed` | 保留 | 错误视觉、tooltip 或 fallback content |
| `Cancelled` | 保留 | 取消视觉或 fallback content |

## 3. Selector 用法

应用侧先限定 Upload owner，再使用生成的 Style 类型进入 Part，不手写模板路径：

```xml
<Style Selector="atom|Upload.semantic-demo">
    <Setter Property="MaxWidth" Value="560" />
    <Setter Property="Opacity" Value="0.94" />

    <atom:UploadListStyle x:SetterTargetType="ItemsControl">
        <Setter Property="Background" Value="#08000000" />
        <Setter Property="Padding" Value="8" />
    </atom:UploadListStyle>

    <atom:UploadItemStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Opacity" Value="0.72" />
    </atom:UploadItemStyle>
</Style>
```

`UploadListStyle` 与 `UploadItemStyle` 位于 `AtomUI.Theme.Styling`，由生成器根据
`Upload.SemanticParts.cs` 静态生成。`x:SetterTargetType` 只提供 Setter 的 AXAML 编译期类型上下文，不参与
`.semantic-*` 匹配；类型必须分别兼容 `ItemsControl` 与 `TemplatedControl`。

owner selector 可以继续包含业务 class、属性或伪类。例如应用可以只在 PictureCard 模式下设置 list padding：

```xml
<Style Selector="atom|Upload[ListType=PictureCard].semantic-demo">
    <atom:UploadListStyle x:SetterTargetType="ItemsControl">
        <Setter Property="Padding" Value="12" />
    </atom:UploadListStyle>
</Style>
```

## 4. 模板、生命周期与定制边界

- `UploadTheme.axaml` 的 Text/Picture 与 PictureCard/PictureCircle 两套模板都必须在实际 list 控件上静态声明
  `Classes.semantic-list="True"`。
- `item` marker 只由容器创建路径添加，不在 prepare/clear 或状态变化时反复增删；容器回收保留同一职责身份。
- `UploadAppendContentItem` 不能获得 `semantic-item` marker，否则 trigger 会被错误计入文件 item cardinality。
- AtomUI 内置主题不得使用 `.semantic-list` 或 `.semantic-item` 实现默认视觉；marker 只服务应用 Semantic Style、
  Gallery Preview 和构建期契约验证。
- 应用替换 Upload ControlTheme 时负责在每个模板变体中实现相同 marker、route、ContractType 和 cardinality。
- 删除或重命名 Part、修改 route、收窄 ContractType、把 append trigger 纳入 item 或改变 cardinality 都属于公共主题
  API 的破坏性变更。
- `UploadTrigger`、`UploadDropZone` 与 `UploadDefaultDropArea` 是独立 public controls；应用通过这些 control 的 root
  API/Style 定制触发与拖动表面，不把它们当作 Upload 的 template-internal Part。

## 5. 资源、性能与 AOT

Semantic Part 改造不增加反射、运行时 descriptor 扫描、订阅、timer、DynamicResource 或 C# binding：

- descriptor、Style 类型、class 常量和注册信息均由 Source Generator 静态生成。
- `list` 使用两个静态 AXAML marker，每个活动模板只实例化一个。
- `item` 只在容器创建时追加一个 class，不在 measure、arrange、render、进度更新或文件状态热路径分配对象。
- route 只跨一个 `/template/` 边界和一个直接 logical child 边界，不使用 descendant selector。
- AOT 注册遵循 [TypeMap 注册管线](../../../../architecture/foundations/aot-typemap-registration.md)；Upload 与专用 Semantic Style 都应成为相应契约片段的保留条件，不使用运行时类型发现。

## 6. 验证要求

- descriptor 测试验证 `root`、`list`、`item` 的名称、route、ContractType、cardinality、RuntimeCreated、StyleType 和
  `Since`。
- 主题测试验证两套 Upload 根模板都使用静态 `Classes.semantic-list="True"`，且不存在字面量 semantic class。
- 控件测试覆盖 Text、Picture、PictureCard、PictureCircle，每个模式恰好一个 list、每个真实文件一个 item，append
  trigger 没有 item marker。
- 生成 Style 测试验证 `UploadListStyle` 与 `UploadItemStyle` 通过完整 route 命中真实运行时节点。
- Gallery 高亮测试验证 root/list/item 的 adorner 数量，并验证离开 Semantic Parts 页签后释放高亮会话。
- Gallery 示例与四语种描述必须与本文件的 Part 名称和定制边界一致。

对应回归位于：

- `tests/AtomUI.Desktop.Controls.Tests/Upload/UploadSemanticPartTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/UploadSemanticPartHighlightTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/UploadShowCasePageTests.cs`
