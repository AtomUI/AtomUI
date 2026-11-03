# CheckableTag 与 CheckableTagGroup 选择模型设计

本文档定义 `CheckableTag` 与 `CheckableTagGroup` 的公共契约、选择状态、选项模型、内部组合、生命周期和验证边界。Tag 家族总体设计见 [Tag 桌面版架构设计](overview.md)，源码职责与维护入口见 [Tag 桌面版实现原理](implementation.md)，视觉变量边界见 [Tag Token 设计](token.md)。

## 1. 设计定位

`CheckableTag` 是 Tag 家族中的二态选择控件，用标签视觉表达可切换选项；`CheckableTagGroup` 在同一组选项上提供可取消单选和多选能力。两者使用选择语义，不承担普通 `Tag` 的状态展示、颜色分类和关闭职责。

该模型覆盖：

- 独立 `CheckableTag` 的 pointer、keyboard、command、focus、disabled 和 Form 行为。
- Group 的 primitive option、结构化 option、默认值、双向绑定和动态单选/多选切换。
- `Options` 与 `CheckedItems` 的集合替换和原地集合变化。
- Template 重新应用、容器回收和控件 detach 时的订阅释放。
- Group 公共值与内部 selection/wrapper 的隔离。

## 2. 设计原则

1. `CheckableTag` 复用 Avalonia `ToggleButton` 的输入与切换语义，不重新实现 pointer、keyboard 或 command 状态机。
2. `CheckableTagGroup` 是业务选择状态的唯一 owner；内部选择控件和子项 `IsChecked` 只是单向投影与交互入口。
3. 单选使用 `CheckedItem`，多选使用 `CheckedItems`，内部 `SelectedItem`、`SelectedItems` 和 option wrapper 不进入 public surface。
4. 选项值使用类型自身的相等语义，不通过引用、显示文本或字符串转换匹配。
5. Default 属性只初始化状态，不在初始化后持续覆盖双向绑定或用户交互。
6. `CheckableTag` 与普通 `Tag` 保持 API 和视觉状态边界，不继承颜色、Variant 或关闭能力。
7. 集合、template part 和事件订阅必须存在对称的 acquire/release 路径。
8. 选项解析、状态同步和容器生成使用静态类型与 Avalonia 属性，不依赖反射扫描或字符串属性路径。

## 3. 专项模型与 Public API

### 3.1 CheckableTag

`AbstractCheckableTag` 继承 Avalonia `ToggleButton`，桌面包通过 `CheckableTag` 提供公开控件类型。稳定公共能力为：

| API | 语义 |
| --- | --- |
| `IsChecked` | 当前二态选择值，支持 TwoWay binding；只有 `true` 表示选中，`false` 和 `null` 均投影为未选中。 |
| `IsEnabled` | 控件可用性；禁用时不响应 pointer、keyboard 或 command 切换。 |
| `Content` / `ContentTemplate` | 标签主体内容与模板。 |
| `Command` / `CommandParameter` | 复用 Button 命令入口，命令执行和状态切换遵循 Avalonia `ToggleButton` 顺序。 |
| `Icon` | 可选 `PathIcon`，位于主体内容之前。 |
| `IsMotionEnabled` | 控制 checked、hover、pressed 等状态的视觉过渡。 |

`CheckableTag` 不定义 `DefaultIsChecked`。未绑定时，`IsChecked` 本身保存控件当前状态；绑定场景由 ViewModel 作为外部 owner。

`CheckableTag` 不继承或转发普通 `Tag` 的 `TagColor`、`Variant`、`IsClosable`、`CloseIcon` 和 `Closed`。这组成员不属于可选择标签契约。

### 3.2 Option 模型

结构化选项使用以下契约：

```csharp
public interface ICheckableTagOption
{
    object Value { get; }
    object? Content { get; }
}

public class CheckableTagOption : ICheckableTagOption
{
    public object Value { get; set; } = null!;
    public object? Content { get; set; }
}
```

`Options` 中不实现 `ICheckableTagOption` 的元素按 primitive option 处理：元素本身同时作为 `Value` 和 `Content`。结构化 option 分别使用 `Value` 和 `Content`，复杂内容由 `ItemTemplate` 负责。

选项必须满足：

- `Value` 非 `null`，因为 `null` 是单选模式的空选择值。
- 同一 Group 中的 `Value` 按 `EqualityComparer<object>.Default` 比较后唯一。
- Group 不根据 Content、容器引用或内部 wrapper 身份匹配业务选择值。
- 重复值和空值属于无效选项输入，必须在选项归一阶段明确拒绝，不能产生多个同步选中项。

### 3.3 CheckableTagGroup

`AbstractCheckableTagGroup` 继承 `TemplatedControl`，桌面包通过 `CheckableTagGroup` 提供公开控件类型。稳定属性为：

| API | 默认值 | 语义 |
| --- | --- | --- |
| `Options` | `null` | 选项数据源，支持普通对象和 `ICheckableTagOption`。 |
| `IsMultiple` | `false` | `false` 为可取消单选，`true` 为多选。 |
| `CheckedItem` | `null` | 单选模式当前值，默认 TwoWay binding。 |
| `CheckedItems` | `null` | 多选模式当前值集合，默认 TwoWay binding；`null` 和空集合均表示无选择。 |
| `DefaultCheckedItem` | `null` | 单选模式的一次性初始值。 |
| `DefaultCheckedItems` | `null` | 多选模式的一次性初始值集合。 |
| `ItemTemplate` | `null` | 选项内容模板。 |
| `Orientation` | `Horizontal` | 选项排列方向。 |
| `ItemSpacing` | SharedToken 对应间距 | 同一行或列相邻选项间距。 |
| `LineSpacing` | SharedToken 对应间距 | WrapPanel 相邻行或列间距。 |
| `IsMotionEnabled` | 继承全局动效设置 | 向内部 `CheckableTag` 投影动效开关。 |

Group 级禁用直接使用继承的 `IsEnabled`。Ancestor disabled 必须使所有子项失效，不增加语义重复的 `Disabled` 属性。

### 3.4 事件契约

`CheckedChanged` 是 Group 的统一语义事件，使用 `CheckableTagGroupCheckedChangedEventArgs`：

| 成员 | 语义 |
| --- | --- |
| `IsMultiple` | 事件产生时的选择模式。 |
| `OldCheckedItem` / `NewCheckedItem` | 单选模式的旧值与新值；多选模式为 `null`。 |
| `AddedItems` / `RemovedItems` | 选择集合的增量；单选模式最多各包含一个值。 |

事件只在有效业务选择发生变化时触发。Default 初始化、内部视觉同步、重复设置等价值和无效点击不触发重复事件。

## 4. 状态与模式策略

### 4.1 选择矩阵

| 模式 | 用户操作 | 结果 |
| --- | --- | --- |
| 单选 | 点击未选中的 A | `CheckedItem = A`。 |
| 单选 | 当前 A，点击 A | `CheckedItem = null`。 |
| 单选 | 当前 A，点击 B | 先移除 A，再选中 B，并作为一次语义变化通知。 |
| 多选 | 点击未选中的 A | 将 A 追加到 `CheckedItems` 末尾。 |
| 多选 | 点击已选中的 A | 从 `CheckedItems` 移除 A。 |
| 任意 | Group 或子项失效 | 不改变选择值，不触发命令和 CheckedChanged。 |

多选结果按用户选择顺序保存。外部集合中的重复值在有效选择投影中只出现一次；用户交互产生的新集合必须去重并保持首次出现顺序。

### 4.2 模式切换

`IsMultiple` 运行时变化时执行确定性转换：

```text
Single -> Multiple
  CheckedItem == null ? empty : [CheckedItem]

Multiple -> Single
  CheckedItems 为空 ? null : CheckedItems 中第一个有效值
```

转换只提交一次 Group 语义变化。非当前模式的值不与内部 selection 竞争，也不保留第二套可见选择状态。

### 4.3 Default 与外部值

初始化优先级为：

```text
当前模式下显式 CheckedItem(s)
  -> 当前模式下 DefaultCheckedItem(s)
  -> 空选择
```

Default 值只在首次选择初始化时读取。初始化后修改 Default 属性不覆盖当前选择。Options 延迟到达时，初始化可以等待第一次可解析的选项集合；Options 临时为空或替换时，不因当前值暂时没有匹配项而清除外部绑定值。

外部值中不存在于当前 Options 的元素不产生选中容器。下一次用户交互生成结果时，只从当前有效选项和本次交互构造选择值，避免隐藏的失效值继续参与选择。

## 5. 架构与状态所有权

| 类型 | 稳定职责 |
| --- | --- |
| `AbstractCheckableTag` | 复用 ToggleButton 的输入、选择、命令和 Form 语义，公开 Icon 与动效入口。 |
| `CheckableTag` | 桌面公开类型与 ControlTheme 入口。 |
| `AbstractCheckableTagGroup` | 拥有 Options、模式、公开选择值、Default 初始化、事件、Form 和订阅生命周期。 |
| `CheckableTagGroup` | 桌面公开类型与 Group ControlTheme 入口。 |
| internal option wrapper | 保存归一后的 Value、Content 和 Source，仅作为内部容器数据，不是业务返回值。 |
| internal checkable items control | 拥有 Avalonia SelectionModel、容器生成和 `IsChecked`/selection 映射，不公开业务 API。 |
| `CheckableTagTheme` | 表达二态标签的内容结构和视觉状态。 |
| `CheckableTagGroupTheme` | 组合内部 items control、ItemsPresenter 和 WrapPanel。 |

Group 不直接继承 `SelectingItemsControl`。这样可以避免把指向 wrapper 的 `SelectedItem`、`SelectedItems`、`SelectedIndex`、`Selection` 和 `SelectionMode` 暴露为第二套公共状态。

内部 items control 只接受 Group 归一后的选项和选择快照。Group 对外始终返回 option `Value`，不能返回 wrapper、容器或原始 `ICheckableTagOption`，除非该对象本身就是 primitive option 的 Value。

## 6. Template、组合与主题契约

默认组合为：

```text
CheckableTag
  -> content surface
     -> optional Icon
     -> ContentPresenter

CheckableTagGroup
  -> CheckableTagItemsControl#PART_CheckableTagItems
     -> ItemsPresenter
        -> WrapPanel
           -> CheckableTag containers
```

`PART_CheckableTagItems` 的 TemplatePart 类型使用 Avalonia `SelectingItemsControl`，默认具体类型保持 internal。内部 host 直接使用受保护的 SelectionModel、`SelectedItems` 和 `SelectionMode`，并负责 wrapper 选择、容器生成、间距、方向和动效投影。该 part 只声明选择宿主类别，不承诺任意 `ListBox` 或普通 `SelectingItemsControl` 可以替换默认内部 host；Group 不提供指向 wrapper 的公开选择 fallback。

主题状态包括 unchecked、checked、pointerover、pressed、focus、disabled。checked 状态只读取 `IsChecked`，不通过 `TagColor` 或 `Variant` 派生。`CheckableTag` 复用 Tag 家族的尺寸、字号、圆角和内容间距语义，并从 SharedToken 读取 primary、text、hover、disabled 和 focus 颜色。

`CheckableTagGroup` 不定义控件 Token；Group间距读取自身Global.SpacingXS。选择值、`IsMultiple`、checked、hover、pressed、focus 和 disabled 均为实例状态，不写入 `TagToken`。

## 7. 数据流与生命周期

### 7.1 Options 归一

Options 建立或发生 Add、Remove、Replace、Move、Reset 时按以下顺序处理：

1. 解除旧集合通知或旧 wrapper 的 owner 关系。
2. 将每个输入项归一为 Value、Content、Source wrapper。
3. 校验 Value 非空且唯一。
4. 将 wrapper 集合提交给内部 items control。
5. 根据当前模式和公开选择值重建内部选择快照。
6. 在同步保护范围内更新容器 `IsChecked`，不产生 CheckedChanged。

Options 替换后旧 wrapper 和旧容器状态全部失效。公开选择值由 Group 保持，只有当前 Options 中匹配的值产生视觉选择。

### 7.2 外部值到内部选择

```text
CheckedItem / CheckedItems / collection notification
  -> 复制并归一有效值快照
  -> 按 Value 匹配 wrapper
  -> internal SelectionModel
  -> CheckableTag.IsChecked
```

同步过程使用 `_isSynchronizingSelection` 或等价事务边界，并暂时抑制内部 selection 回调。程序化同步不得反向重写公开值。

### 7.3 子项交互到公开值

```text
CheckableTag IsCheckedChanged
  -> internal SelectionModel Select/Deselect
  -> Group 构造新的单选值或多选快照
  -> SetCurrentValue(CheckedItemProperty / CheckedItemsProperty)
  -> CheckedChanged + Form ValueChanged
```

`SetCurrentValue` 用于保留 Avalonia binding。Group 不直接 Add/Remove 外部 `IList`，防止绕过绑定 setter、校验和 ViewModel 的状态 owner。

### 7.4 集合与模板生命周期

- `Options` 和 `CheckedItems` 替换时，先解除旧 `INotifyCollectionChanged` 再订阅新集合。
- `CheckedItems` 原地 Add、Remove、Replace、Move、Clear、Reset 时，使用旧快照与新快照同步视觉并计算事件增量。
- `OnApplyTemplate` 开始时解除旧 `PART_CheckableTagItems` 事件，再连接新 part 并重新投影当前选项、模板和选择状态。
- Attach 时建立集合通知，detach 时释放集合、part 事件和绑定；重新 attach 后根据公开状态恢复。
- 容器重新准备或回收时覆盖 IsChecked、Content、ContentTemplate 和动效投影，不能继承旧 item 状态。

## 8. Form、资源、性能与 AOT 边界

`CheckableTag` 的 Form 值为二态 bool：Get 返回 `IsChecked == true`，Set 将 `true` 设为选中，其余值设为未选中，Clear 设为 `false`。

`CheckableTagGroup` 的 Form 值依模式解释：单选 Get/Set/Clear 对应 `CheckedItem`，多选对应 `CheckedItems`。`CheckedItems` 原地集合变化和模式转换必须通知 Form value changed；内部视觉同步不重复通知。

性能与 AOT 边界：

- Options 重建和全量选择同步为 O(N)，只在选项、模式或公开选择值变化时执行，不进入 Render 热路径。
- 单次子项点击通过 value 索引或稳定 wrapper 映射更新，不能扫描 VisualTree 查找业务值。
- 外部集合始终复制为内部快照，不能把 ViewModel 集合作为 internal `SelectedItems` 直接持有。
- 默认 WrapPanel 不承诺大数据虚拟化；CheckableTagGroup 面向有限选项集合，不为隐藏项预创建额外视觉。
- option wrapper 不承载 DynamicResource，不需要成为资源宿主，也不反向持有控件或 Theme owner。
- 属性同步使用 AvaloniaProperty、强类型 getter 和事件，不使用 ReflectionBinding、字符串 path 或运行时类型扫描。

## 9. 兼容性与定制边界

- `CheckableTag` 的二态切换、Content、Icon、Command、IsEnabled、IsChecked 和 Form 语义属于稳定契约。
- `CheckableTagGroup` 默认单选，单选可取消；`IsMultiple=true` 才启用多选。
- `CheckedItem` 和 `CheckedItems` 表达 option Value，不表达 option wrapper 或控件实例。
- Value 非空且唯一是 Options 契约，定制 option 类型必须保持稳定相等语义。
- Default 属性只初始化一次；自定义主题不能改变其状态优先级。
- `PART_CheckableTagItems`、checked/disabled/focus 状态、ControlTheme key 和容器类型关系属于主题兼容边界。
- 自定义视觉通过 ContentTemplate、ItemTemplate、Style 和 ControlTheme 完成，不通过重新引入普通 Tag 的颜色或关闭 API 完成。
- 应用定制 Group 主题时必须保留内部选择 host 的状态隔离；不能用普通 `ListBox` 替换该 host，也不得把 internal SelectionModel 变成第二套 public state。

## 10. 验证要求

| 层级 | 必须覆盖的行为 |
| --- | --- |
| CheckableTag API | IsChecked TwoWay、Content、Icon、Command、IsEnabled、IsMotionEnabled 和二态 null 归一。 |
| CheckableTag 输入 | Pointer、Space、focus、disabled、Checked/Unchecked 顺序和重复输入。 |
| Group Options | primitive/structured option、Value/Content 映射、空值、重复值和 ItemTemplate。 |
| 单选 | 默认模式、选择、切换、再次点击取消、外部设置和 Default 初始化。 |
| 多选 | 追加、移除、顺序、去重、空集合、外部设置和 Default 初始化。 |
| 模式 | Single/Multiple 运行时转换、只触发一次语义事件和 Form 通知。 |
| 集合 | Options 与 CheckedItems 的 Add、Remove、Replace、Move、Clear、Reset 和整体替换。 |
| 生命周期 | Template 重应用、detach/reattach、集合替换和容器回收不重复订阅、不残留状态。 |
| 事件与 Form | Added/Removed、Old/New、无变化不通知、Get/Set/Clear 和集合原地变化。 |
| Theme | unchecked/checked/hover/pressed/focus/disabled、Light/Dark 和 Browser/Desktop 一致性。 |
| Gallery | 独立 Checkable、可取消 Single 和 Multiple 示例，并展示 TwoWay 状态。 |
| AOT | 不新增 ReflectionBinding、运行时扫描或 trimming warning；Gallery NativeAOT publish 可用。 |


CT15补充：Group自身Global.FontFamily沿真实继承进入当前子项，Native字体家族优先；Checkable明确自身Global.FontSizeSM时覆盖TagFontSize家族回退。两者不接管对方的选择/颜色状态。container的Local投射绑定由membership持有与dispose，退役对象不再保留旧ItemsControl/Group；重挂恢复当前模板与motion，模板更新按框架规则重建容器。


CT15当前仅[限定表](../../../../superpowers/plans/2026-10-22-custom-theme/tag-consumers-visual-review.md)26对及列明SelectTag派生影响已获批准；恢复/跨运行微差、CT13既定边界和完整Tag/CT15保留，不授权提交。
