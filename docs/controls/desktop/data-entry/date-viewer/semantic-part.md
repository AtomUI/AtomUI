# DateViewer Semantic Part 契约

本文定义 DateViewer 与 RangeDateViewer 当前公开的定制入口，关联 [架构](overview.md)、
[实现](implementation.md)和 [共享设计](shared-panel-design.md)。系统规则见
[Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

两个 public owner 独立生成 descriptor 与专用 Style。源码声明位于
[DateViewer.SemanticParts.cs](../../../../../src/AtomUI.Desktop.Controls/DateViewer/DateViewer.SemanticParts.cs)和
[RangeDateViewer.SemanticParts.cs](../../../../../src/AtomUI.Desktop.Controls/DateViewer/RangeDateViewer.SemanticParts.cs)。
下表所有 Part 的 Since 为 **6.2.2**，CrossVisualRoot 为 **false**；
root 是隐式实例入口，不添加 semantic-root marker。每个表的标题即 Owner。

### DateViewer

| Part | Selector | SelectorRoute | ContractType | Cardinality | Customization | RuntimeCreated | CrossNestedOwners |
| --- | --- | --- | --- | --- | --- | --- | --- |
| root | `atom|DateViewer` | 不适用 | DateViewer | Single | Root | false | false |
| header | `.semantic-header` | `/template/ .semantic-header` | TemplatedControl | Optional | Selector | false | false |
| body | `.semantic-body` | `/template/ .semantic-body` | Panel | Single | Selector | false | false |
| content | `.semantic-content` | `/template/ .semantic-body > .semantic-content` | TemplatedControl | Multiple | Selector | false | false |
| cell | `.semantic-cell` | `/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell` | TemplatedControl | Multiple | Selector | true | true |
| cellContent | `.semantic-cell-content` | `/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell /template/ .semantic-cell-content` | ContentControl | Multiple | Selector | true | true |
| footer | `.semantic-footer` | `/template/ .semantic-footer` | DashedBorder | Optional | Selector | false | false |

### RangeDateViewer

| Part | Selector | SelectorRoute | ContractType | Cardinality | Customization | RuntimeCreated | CrossNestedOwners |
| --- | --- | --- | --- | --- | --- | --- | --- |
| root | `atom|RangeDateViewer` | 不适用 | RangeDateViewer | Single | Root | false | false |
| header | `.semantic-header` | `/template/ .semantic-header` | TemplatedControl | Multiple | Selector | false | false |
| body | `.semantic-body` | `/template/ .semantic-body` | Panel | Single | Selector | false | false |
| content | `.semantic-content` | `/template/ .semantic-body > .semantic-content` | TemplatedControl | Multiple | Selector | false | false |
| cell | `.semantic-cell` | `/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell` | TemplatedControl | Multiple | Selector | true | true |
| cellContent | `.semantic-cell-content` | `/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell /template/ .semantic-cell-content` | ContentControl | Multiple | Selector | true | true |

RuntimeCreated 的 Cell 在 DatePanel 容器池中创建；CrossNestedOwners 允许明确建模的面板/Cell 主题边界，
不是穿透任意用户内容的许可。数量描述适用模板的节点，运行前无模板不要求存在节点；
隐藏的 Header 或第二面板仍可能保留在树中，不能把 selector 命中数量等同于可见数量。

## 2. 逐 Part 职责与存在条件

### root

对应 public owner 实例，适合 Background、BorderBrush、BorderThickness、CornerRadius、Padding 和布局定制。
不要以根样式设置内部交互 cursor 或业务提交策略；没有专用子模板 Theme 入口。

### header

独立单值 Header 为 Optional；范围 Header 为 Multiple，日期双面板具有各自的标题与导航上下文。
ShowHeader=false 隐藏 Header 区域，层级或面板数量变化时第二 Header 随第二面板可见性变化。
HeaderTemplate 替换内容，但 header marker 保持由 owner 模板拥有。
适合字体、前景和有限布局调整；Header 内部按钮、Name 和 ContentControl 不属于独立 Part。

### body

单值为 Panel，范围为水平 StackPanel；公开 ContractType 为 Panel。
适合正文布局层的有限定制，保持子面板的 Measure/Arrange、间距、命中与裁剪。
不能假定单值与范围的具体布局容器一致。

### content

真实 DatePanel 表面，单值一个，范围按当前面板状态显示一个或两个，Range 的两个模板节点可保留。
ContractType 是 TemplatedControl，不公开内部 DatePanel 类型。
可定制面板前景与合适的布局属性，不能依赖 PART_PrimaryPanel/SecondaryPanel 或用 selector 操纵内部 Grid 行列。

### cell

统一日期、周期和周号容器，ContractType 为 TemplatedControl。
所有网格复用同一 Part 身份；状态、模型与容器池改变不会增删 marker。
可定制 Foreground、Background、BorderBrush、BorderThickness 和 CornerRadius；
默认状态和布局仍须保持可用。容器大小、Padding 或 Margin 调整必须验证整周连续背景、日期命中、
范围两侧边界、内容呈现和实际约束下的裁剪。

模板或范围状态不会将 cell 改成 Button；应用不能依赖旧 CalendarDayButton。
内容替换不取消禁用、键盘、Pointer、焦点和 Automation。

### cellContent

Cell 模板内的 ContentControl，默认值区位于另一个内部节点。
CellTemplate 在默认日期下方添加内容；FullCellTemplate 隐藏默认值框并让内容占据原内容区域，
优先于 CellTemplate。二者使用 DateViewerCellContext，公开模板入口仍由 owner 管理。

适合内容槽的 Padding、对齐和前景；模板内容子树不成为额外公开 Part，
不能把用户控件内部模板或农历业务内容视为 DateViewer 的稳定结构。

### footer

DateViewer 的默认 Today 页脚，ContractType 为 DashedBorder。仅独立使用且 SelectionUnit 为 Date 时显示；
作为 Picker 托管面板或选择其他单位时隐藏，但模板中的 marker 保留。可通过 DateViewerFooterStyle 定制页脚表面与边框，
不借此改变 Today 命令的可用性或选择规则。RangeDateViewer 没有独立的默认页脚，因此不声明 footer Part。

## 3. Selector 与专用 Style

应用在 owner-scoped Style 内使用生成专用类，Setter 的编译类型必须匹配 ContractType。
完整 route 由专用类封装；表格 route 是 descriptor 技术元数据，不要求用户手写跨模板路径。

```xml
<StackPanel.Styles>
    <Style Selector="atom|DateViewer.custom-panel">
        <atom:DateViewerCellStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="Foreground" Value="#722ED1" />
        </atom:DateViewerCellStyle>
    </Style>
    <Style Selector="atom|RangeDateViewer.custom-range">
        <atom:RangeDateViewerCellStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="Foreground" Value="#722ED1" />
        </atom:RangeDateViewerCellStyle>
    </Style>
</StackPanel.Styles>
```

其他生成类按同一 owner + Part 命名：DateViewerHeaderStyle、DateViewerBodyStyle、
DateViewerContentStyle、DateViewerCellContentStyle、DateViewerFooterStyle，以及 RangeDateViewer 对应的已声明 Part 类。
DateViewerCellContentStyle 的 Setter 上下文是 ContentControl；
DateViewerBodyStyle 的 Setter 上下文是 Panel，DateViewerFooterStyle 为 DashedBorder。root 使用实例或 owner Style。

Picker、Calendar 与公共面板属于不同 owner；组合 route 必须由各自 descriptor 明确声明。
专用 Style 不命中目标属于 route 契约缺陷，不能获取私有节点直接赋值作为回退。

## 4. Abstract AXAML Structure

本文不手工维护生成器抽取的 Abstract AXAML Structure。真实主题资产和源码索引见
[实现原理的源码结构与组合模型](implementation.md#5-组合结构模型)；
生成工具必须读取对应 Theme 的真实 ControlTemplate，不能从 Part 表推造 XML。

## 5. Composition Model

DateViewerTheme 的 Border/DockPanel 组合 DateViewerHeader、Panel/DatePanel 和默认 Today 页脚。
RangeDateViewerTheme 通过水平 Header 行与正文行组合两个 Header 和两个 DatePanel；
每个 DatePanel 的 DockPanel 组合周 Header Grid 与 CellHost Grid。
CellHost 的运行时 Cell 在自己的叶子主题中提供 ContentControl。

这些内部类型属于 internal-observable，只用于理解 ownership 与实际 route。
应用依赖公共模板、内容上下文和专用 Style，不能依赖 PART 名、内部伪类或资源宿主实现。

## 6. 定制与兼容边界

Part 名、selector class、route、ContractType、cardinality 与 Since 共同构成公开契约。
删除/改名、收窄类型、改变数量或移除默认主题实现必须按主题兼容变更处理。
默认主题不通过 semantic marker 实现内部视觉状态，应用 Style 不改变选择 owner。

RangeBars、Picker Footer、TimeView、用户内容子树和农历算法不属于本面板 Part。

## 7. 验证策略

定向验证检查生成 descriptor 与全部默认模板 marker 一致、Style Setter 实际生效、
单/双面板、四种网格、模板内容、re-template 和容器回收。
Gallery [独立页面](../../../../../controlgallery/AtomUIGallery/ShowCases/DataDisplay/DateViewer/Views/DateViewerShowCase.axaml)
的 Semantic Parts Tab 使用单个 DateViewer 预览；Examples 保留单值和范围各自的专用 Cell Style 示例，页面验证同时禁止私有节点赋值的代码回退。

宿主完整视觉等价、冷消费与 NativeAOT 验收独立记录；公共面板 Style 命中不证明 Picker 迁移已完成。
