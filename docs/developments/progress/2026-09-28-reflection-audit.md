# Avalonia 12.1.3 背景下的 AtomUI 反射访问全局评估

日期：2026-09-28。范围：`src/`、`controlgallery/` 的生产反射访问，使用 `tests/`、AXAML、生成器和文档核对调用与契约。

本文记录实施前的只读评估，当时未修改生产代码、依赖版本或测试，AtomUI 引用 Avalonia 12.1.2。
用户批准后的实际执行与验证见 [升级执行日志](2026-09-28-avalonia-12.1.3-upgrade-log.md)。
参考源码为本地 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`，HEAD 为 12.1.3 / `8eeda4f6f546165b3f72e63c9f42247abb306905`；
历史可用性通过 12.1.2 / `d3c867a9e2de379249b03dbeb3495bd7f076a81a` 复核。

## 结论

有明确精简空间，主要来自历史遗留封装、已有公开入口和控件自己已经持有的模板部件。不是 Avalonia 12.1.3 把大批 internal 属性改成了 public。

- **4 个内部封装文件没有代码调用者，可以直接列入删除候选。**
- **3 个封装文件可以在迁移现有调用后整体移除。**
- 其他文件中存在无调用的成员、重复读取的模板部件，以及需要保留但可以缩小维护范围的反射。
- `OpenedPopups` 是本次新增能力，但不能替代 popup 创建、light-dismiss 注册租约、内部回调和输入根访问。
- 下述“可删除”指源代码调用与契约审查结论；实际删除仍须编译和受影响验证。未运行基准测试，不宣称具体性能或产物体积收益。

## 1. 无调用的内部封装

全局检索时区分同名普通方法、Roslyn symbol API、JSON 属性访问和自递归，未把它们算作反射封装的消费者。

| 文件 | 反射目标 | 结论 |
| --- | --- | --- |
| `src/AtomUI.Core/Media/TextFormatting/TextParagraphPropertiesReflectionExtensions.cs` | `TextParagraphProperties.LineSpacing` | `GetLineSpacing`、`SetLineSpacing` 无代码调用。可以删除整个内部类。`docs/architecture/systems/typography/consumption.md:113` 仍提及它，落地时同步清理该过时说明。 |
| `src/AtomUI.Core/Data/DynamicResourceReflectionExtension.cs` | `DynamicResourceExtension._anchor` | `SetAnchor` 无代码调用，可以删除整个内部类。删除此无调用封装不等于移除正常 DynamicResource 或 scoped resource host 功能。 |
| `src/AtomUI.Controls/ItemsControl/ItemsSourceViewReflectionExtensions.cs` | `ItemsSourceView.TryGetInitializedSource` | 封装无代码调用，可以删除文件。 |
| `src/AtomUI.Controls/Primitives/TextSearchReflectionExtensions.cs` | `TextSearch.GetEffectiveText` | 封装无代码调用，可以删除文件。 |

这些目标在上游仍非公开；删除依据是无消费者，不是可以直接访问。

## 2. 有等价替代路径的封装

### 2.1 TopLevel.LayoutManager → UpdateLayout

文件：`src/AtomUI.Desktop.Controls/TopLevel/TopLevelReflectionExtensions.cs`。

现有 6 处调用只取得 layout manager 后执行 `ExecuteLayoutPass()`：

- `NumericUpDown/NumericUpDown.cs:366`
- `TreeView/TreeView.CheckedState.cs:184`
- `TreeView/TreeView.StateReplay.cs:385`
- `TreeView/TreeView.PathTraversal.cs:136`
- `NavMenu/NavMenu.cs:1493`
- `Cascader/CascaderView.ExpandAndCollapse.cs:259`

上游 `src/Avalonia.Base/Layout/Layoutable.cs:226` 的公开实现就是：

```csharp
public void UpdateLayout() => this.GetLayoutManager()?.ExecuteLayoutPass();
```

保持当前 topLevel、null 条件和调用时机，替换为 `topLevel?.UpdateLayout()`，即可删除整个 Desktop TopLevel 反射文件。
这不适用于 Controls 包中读取 `LastPointerPosition` 的同名文件。

### 2.2 TextBlock：删除不需要的反射

文件：`src/AtomUI.Desktop.Controls/TextBlock/TextBlockReflectionExtensions.cs`。

- `GetMaxSizeFromConstraint` 在上游本来就是 **protected**，12.1.2 和 12.1.3 均如此。
  `SelectableTextBlock` 派生自 TextBlock，其 `this.GetMaxSizeFromConstraint()` 按 C# 成员解析优先使用继承实例方法，不能把这个文本命中误算成反射扩展的运行时调用。
  可以删除对应无用扩展与 MethodInfo。
- `HasComplexContent` 在上游仍为 internal，但实现恰好是 `Inlines != null && Inlines.Count > 0`。
  `SelectableTextBlock.cs:162,330,412,494` 的 4 个调用可直接使用同一判定，保留选择、复制和 Inline 文本语义。

完成这两项后可删除整个文件。上游证据：`src/Avalonia.Controls/TextBlock.cs:378,383`。

### 2.3 TextLayout.CreateTextParagraphProperties → 公开类型构造

文件：`src/AtomUI.Desktop.Controls/Input/Utils/TextLayoutReflectionExtensions.cs`。

唯一调用为 `Input/TextArea.cs:157`，用于计算固定行数对应的高度。
上游 `src/Avalonia.Base/Media/TextFormatting/TextLayout.cs:546` 的内部方法仅依次构造公开的
`GenericTextRunProperties` 和 `GenericTextParagraphProperties`，没有额外内部状态。

可在既有 TextArea 计算中使用相同公开构造函数和全部相同参数，删除反射文件；不新增通用 helper，
不省略 FlowDirection、LineHeight、LetterSpacing、FontFeatures 或 paragraph 标志。
验证固定行数、字体、缩放与换行高度。该替代在 12.1.2 已可用。

### 2.4 ItemsControl._items → Items

`src/AtomUI.Controls/ItemsControl/ItemsControlReflectionExtensions.cs:35` 的 `GetItems` 只在
`src/AtomUI.Desktop.Controls/ListView/ListView.cs:419` 使用。

上游 `src/Avalonia.Controls/ItemsControl.cs:120` 已提供 `public ItemCollection Items => _items`。
可以用 `Items` 替代，删除 `_items` FieldInfo 和 GetItems。此文件的 `WrapFocus` setter 仍需保留，不能整文件删除。

## 3. 可分批精简的其他成员

| 文件/区域 | 可精简内容 | 必须保留或核对的边界 |
| --- | --- | --- |
| `Core/Controls/VisualReflectionExtensions.cs` | 无外部调用的 `GetVisualChildrenList`、Index/Add/Insert helpers，以及 `ClearVisualParentRecursive`；随后移除 `VisualChildren` PropertyInfo | `SetVisualParent` 仍被 IconPresenter、DataGrid flyout 路径使用，不删除整个类。 |
| `Core/Reflection/StyledElementReflectionExtensions.cs` | 无外部调用的 logical children Get/Add/Insert 与 `SetTemplatedParentRecursive`；移除 LogicalChildren PropertyInfo | `SetTemplatedParent` 活跃且 setter 非公开，保留。 |
| `Controls/ItemsControl/ItemsControlReflectionExtensions.cs` | 无调用 `GetWrapFocus` | `SetWrapFocus` 用于 ListView 的 `WrapSelection`；上游为 private protected，不能直接赋值。 |
| `Desktop.Controls/Popup/PopupReflectionExtensions.cs` | 无调用的 `SetIgnoreIsOpenChanged`、`GetIgnoreIsOpenChanged`、`RemoveClosingEventHandler`；可移除 `_ignoreIsOpenChanged` FieldInfo | `Closing` 添加、`SetPopupParent`、`HandlePositionChange` 仍活跃；不把移除未使用的解绑封装当成取消生命周期清理。 |
| `Desktop.Controls/Input/Utils/TextBoxReflectionExtensions.cs` | 无调用的 `GetScrollViewer` | `SetScrollViewer` 仍写同一字段，因此不能同时移除 `_scrollViewer` FieldInfo。 |
| `Core/Utils/TypeHelper.cs` | 无调用的 DisplayName、ReadOnly、DefaultMemberName 和集合元素类型推断入口，可按调用图独立裁剪 | 排序的 nested property/indexer fallback 活跃。`FindGenericType` 被活跃的 IList/IReadOnlyList 索引器路径调用，不能随死入口一起删。 |

表中无调用结论针对内部 API；不能推广到公共 helper 或 protected 扩展点。

## 4. 有价值但需要场景验证的替代

### 4.1 TextBox._presenter：复用已经缓存的模板部件

`AutoCompleteTextAreaBox.cs:35-40` 和 `MentionTextArea.cs:66,90-94` 已在 OnApplyTemplate 中保存 `PART_TextPresenter`。
但它们在 `:55` 和 `:204` 又调用 `GetTextPresenter()`，反射读取基类的 `_presenter`。

这两处可考虑改用既有 `_textPresenter`，完成后移除相应反射 getter 与 FieldInfo。
必须验证模板应用之前、模板缺件、自定义模板和 re-template 时的状态，不通过吞掉异常或默认返回空矩形改变现有契约。

### 4.2 VisualLayerManager._layers：使用直接视觉子节点

`VisualLayerManagerUtils.FindLayer<T>` 只用于查找 `WindowFeedbackLayer` 和 `TourLayer`。
上游 `VisualLayerManager.AddLayer` 同时维护 `_layers`、logical parent、ZIndex、`VisualChildren` 和 attach 通知。

**查询**可考虑用公开的 `GetVisualChildren()`，只检查直接子节点，并排除普通 `Child`，保持仅匹配注册层的语义；
这样可以删除 GetLayers 与 `_layers` FieldInfo。不能把查询优化扩展成自行替代 AddLayer——后者还有布局、资源和生命周期职责。
验证嵌套 manager、普通 Child 与目标同型、rehost、窗口反馈层和 Tour 的既有顺序。

### 4.3 IInputRoot.RootElement：公开 IPresentationSource.RootVisual

目前仅 ToolTipService 的两处输入处理使用 `GetRootElement()`。
上游 `src/Avalonia.Controls/PresentationSource/PresentationSource.cs:14,75` 的 PresentationSource 同时实现
`IInputRoot` 和 `IPresentationSource`，其 RootElement 直接返回 RootVisual；`IPresentationSource.RootVisual` 是公开成员。

对标准 Avalonia 输入根可以经 `IPresentationSource` 访问而删除 RootElement 反射。
但两个接口之间没有继承保证，不能直接假设任意 IInputRoot 都能转换；需锁定 Tooltip 接受的输入根契约，
验证 native/overlay popup、窗口离开与测试注入根。该公开接口不是 12.1.3 新增。

### 4.4 FocusManager.GetFocusManager：公开 TopLevel.FocusManager

Avalonia 自身注释推荐公开入口，但它并非完全等价替换：

- 内部方法对 detached/non-Visual 元素还会回退到 `AvaloniaLocator`。
- `TopLevel.FocusManager` 返回 `IFocusManager`；AutoComplete 使用的按 focus scope 查询需要具体 FocusManager。
- 某些独立 visual root 不一定能通过普通祖先遍历得到 TopLevel。

先验证当前调用的连接状态、focus scope、popup 与测试场景，再决定是否移除。不能无声删除 fallback 语义。

## 5. 框架反射边界盘点

路径缩写均相对 `src/`。本表覆盖专用封装及分散的框架成员访问；构造反射和动态数据另见下节。

| 文件 | 成员/作用 | 建议 |
| --- | --- | --- |
| `AtomUI.Core/Animations/AnimatableReflectionExtensions.cs` | EnableTransitions / DisableTransitions | 保留活跃内部状态操作；AtomUI 封装本身还是 public 扩展契约 |
| `AtomUI.Core/Data/DynamicResourceReflectionExtension.cs` | _anchor | 删除无调用文件 |
| `AtomUI.Core/Controls/FocusManagerReflectionExtensions.cs` | GetFocusManager | 条件替代，见 4.4 |
| `AtomUI.Core/Controls/ItemCollectionReflectionExtensions.cs` | SetItemsSource | 保留；改变 Items 源的语义不是简单 AddRange |
| `AtomUI.Core/Controls/RawPointerEventTypeReflectionExtensions.cs` | InputHitTestResult | 保留；消费上游已计算的命中结果，不能用重新 hit-test 冒充等价 |
| `AtomUI.Core/Controls/VisualReflectionExtensions.cs` | SetVisualParent / VisualChildren | 删除死分支，保留 parent 操作 |
| `AtomUI.Core/Reflection/StyledElementReflectionExtensions.cs` | LogicalChildren / TemplatedParent | 删除死分支，保留 active setter |
| `AtomUI.Core/Input/IInputRootRefectionExtensions.cs` | RootElement | 条件替代，见 4.3 |
| `AtomUI.Core/Media/TextFormatting/TextParagraphPropertiesReflectionExtensions.cs` | LineSpacing | 删除无调用文件 |
| `AtomUI.Core/Utils/AvaloniaPropertyReflectionExtensions.cs` | Notifying | 保留 DataGrid 行单元格通知；缓存查找并审查精确保留 |
| `AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs` | _surfaceProxy → _target → WlSurface / Globals / WlCompositor | 保留；当前矩形输入区域能力无已证实的公开等价入口 |
| `AtomUI.Controls/Buttons/AbstractIconButton.cs` | _isFlyoutOpen | protected 方法虽无仓内调用，也不按内部死代码删除 |
| `AtomUI.Controls/ItemsControl/ItemsControlReflectionExtensions.cs` | _items / WrapFocus | 用 Items，删 getter，保留 WrapFocus setter |
| `AtomUI.Controls/ItemsControl/ItemCollectionReflectionExtensions.cs` | SourceChanged | 保留源切换通知时序；CollectionChanged 不直接等价 |
| `AtomUI.Controls/ItemsControl/ItemsSourceViewReflectionExtensions.cs` | TryGetInitializedSource | 删除无调用文件 |
| `AtomUI.Controls/Primitives/TextSearchReflectionExtensions.cs` | GetEffectiveText | 删除无调用文件 |
| `AtomUI.Controls/Primitives/TopLevelReflectionExtensions.cs` | LastPointerPosition | 保留 Popup 指针定位路径 |
| `AtomUI.Controls/Primitives/VisualLayers/VisualLayerManagerReflectionExtensions.cs` | AddLayer / _layers / PopupOverlayLayer | _layers 查询可优化；保留其余内部能力 |
| `AtomUI.Controls/ScrollViewer/ScrollBarReflectionExtensions.cs` | _timer / IsExpanded setter | 保留：IsExpanded 虽公开可读，但 setter 私有且 DirectProperty 只读 |
| `AtomUI.Toolkits.GalleryBase/Shell/GalleryBrowserShellView.cs` | EnablePopupOverlayLayer / LightDismissOverlayLayer | 保留或集中封装；Browser overlay 初始化不能直接删除 |
| `AtomUI.Desktop.Controls/Popup/PopupReflectionExtensions.cs` | Closing / _ignoreIsOpenChanged / SetPopupParent / HandlePositionChange | 删除死成员，其余保留 |
| `AtomUI.Desktop.Controls/Popup/PopupLightDismissRegistration.cs` | _openState / _cleanup / _state、layer 注册与回调 | 高耦合兼容边界，不能由 OpenedPopups 替代 |
| `AtomUI.Desktop.Controls/Input/Utils/TextBoxReflectionExtensions.cs` | _scrollViewer / _presenter / vertical space / undo / text input | 删无调用 getter；已缓存 presenter 可替代；其余逐项保留 |
| `AtomUI.Desktop.Controls/Input/Utils/TextLayoutReflectionExtensions.cs` | CreateTextParagraphProperties | 改用公开构造函数，删除文件 |
| `AtomUI.Desktop.Controls/Flyouts/PopupFlyoutBaseReflectionExtensions.cs` | _popupLazy、Opened/Closed/Closing/KeyUp 回调 | 保留；方法组和事件订阅也是调用，不能仅搜索括号判断死代码 |
| `AtomUI.Desktop.Controls/ComboBox/ComboBoxReflectionExtensions.cs` | _popup / _inputTextBox | 保留；AtomUI 在 base.OnApplyTemplate 后清理/接管基类行为，并非只是读取部件 |
| `AtomUI.Desktop.Controls/TopLevel/TopLevelReflectionExtensions.cs` | LayoutManager | 改用 UpdateLayout，删除文件 |
| `AtomUI.Desktop.Controls/Window/Utils/WindowDrawnDecorationsReflectionExtensions.cs` | _topLevelHost / _decorations / _resizeGrips / GripThickness | 保留真实 decorations 拓扑与 resize 能力；不以改挂业务层替代 |
| `AtomUI.Desktop.Controls/Menu/ContextMenuReflectionExtensions.cs` | _popup、PopupOpened/Closed/Closing/KeyUp | 保留当前 popup 接管和回调语义 |
| `AtomUI.Desktop.Controls/TabControl/ScrollContentPresenterReflectionExtensions.cs` | SnapOffset | 保留；Tab 主题转发 snap 配置，不能按默认无 snap 推断无用 |
| `AtomUI.Desktop.Controls/TextBlock/TextBlockReflectionExtensions.cs` | GetMaxSizeFromConstraint / HasComplexContent | 直接 protected 方法 + 等价 predicate，删除文件 |

尤其不能误判 TextArea 的 `_scrollViewer` 写入：该部件位于嵌套 TextAreaDecoratedBox 模板，
基类 TextBox 只在自身 NameScope 中查找，当前 setter 用来把实际部件传给基类键盘和自动尺寸逻辑。
直接删除或仅重新查找自身 NameScope 会改变行为。

## 6. 其他反射不是本次升级的清理目标

- `MenuFlyout.cs:81`、`TreeViewFlyout.cs:73` 通过 Activator 创建 ItemCollection；上游构造函数仍 internal。
  用临时 ItemsControl 的 Items 替代会带来多余 owner/订阅，并不是已证实的等价优化。
- `ObjectExtension`、`TypeMemberExtension` 是公共兼容 API，不能因内部某些重载无调用而删除。
- `TypeHelper`、`ListSortDescription`、`ListCollectionView.AddNew`、`DataMemberRuntimeComparerFactory` 的活跃反射
  服务用户动态模型兼容；应保留 generated accessor/comparer、显式 factory 等正常 AOT 路径和风险标注。
- Core `IconProvider<T>.CreateFactory` 是 protected 可扩展 fallback。内置 AntDesignIconProvider 已 override GetIcon，
  调用生成的静态 CreateIcon，不应把 fallback 当成内置图标的常规创建成本。
- `ControlRegistrationRuntime` 只读取已选 TypeMap proxy 上的已知 Attribute，属于现行注册契约；不是程序集扫描遗留。
- AtomUI/Gallery 版本信息读取 AssemblyMetadata 有明确用途，不能把所有 GetCustomAttribute 都当作应删除反射。
- Build.Tasks 的 TaskProcessHost 通过固定白名单分发构建任务，再映射输入输出属性；该工具不作为应用运行时代码部署或裁剪。
  构建期 AssemblyName/PE metadata 与 Roslyn symbol API、JSON GetProperty 均不算 Avalonia 私有属性访问。

## 7. 保留反射的维护优化

1. `AvaloniaPropertyReflectionExtensions.InvokeNotifying` 在每次 DataGrid 单元格通知时重新解析 PropertyInfo。
   可将其缓存到既有边界，随后比较必要时的委托调用；不可删除 notifying 前后通知或改为普通 PropertyChanged。
2. 优先删除失效成员，再评估 `NonPublicMethods/Fields/Properties` 的宽泛保留范围。
   必须连同 `TypeMemberExtension` 的 DAM 要求一起审查，不能只删 DynamicDependency 就声称减少了裁剪根。
3. 常量 Type.GetType 和 GetField/GetMethod 可能被 trimmer 识别；没有显式 DynamicDependency 不自动代表 AOT 失败。
   PopupLightDismissRegistration 的风险来自依赖私有对象图、构造泛型名和 lease 结构，需真实发布验证，不能仅看注解推断结果。
4. 不把反射统一换成 UnsafeAccessor 当作“解决耦合”：私有成员契约仍存在，且还需验证目标 TFM、Browser 和 NativeAOT。
5. `AvaloniaAccessUnstablePrivateApis=true` 只切换到实现程序集引用，不改变 C# 的 internal/private 可访问性。
   `[PrivateApi] public` 可直接使用与真正 internal 成员必须分别判断。

## 8. 建议落地顺序与验收

第一批：删除 4 个无调用文件、处理第 3 节明确死成员；完成 UpdateLayout、TextBlock、TextLayout、Items 的等价替代。
其中前三类迁移完成后再移除对应 3 个文件，共计 **7 个可整文件移除的候选**。先保留需模板/输入根语义验证的替代。

第二批：处理已缓存 presenter、layer 查询、IPresentationSource 输入根和 FocusManager；每项单独锁定生命周期、几何和 fallback 契约。

第三批：对剩余活跃反射统一审查缓存和精确保留。涉及 Popup/Window/Wayland 的内部状态桥接，应以现有行为和上游扩展点为依据，不为降低反射数量重做宿主结构。

实际实现须使用受影响验证入口；关注 ListView 源/选择、TextArea 行高、SelectableTextBlock Inline/选择、
TreeView/NavMenu/Cascader 布局、AutoComplete/Mentions 模板重套、Tour/反馈层及 Tooltip 输入边界。
改动资源或反射保留时补真实 NativeAOT/Browser trim 验证。删除无调用封装不需要为“文件已不存在”编写测试。

以上为实施前审计结论。用户批准后，第一批 7 个文件删除、等价 API 替代、局部死成员删除及 Notifying
PropertyInfo 缓存已实施；TypeHelper 动态数据入口与第二批条件性替代保留。构建、回归、包消费和真实
NativeAOT/Browser 验证及失败诊断见 [升级执行日志](2026-09-28-avalonia-12.1.3-upgrade-log.md)。
未进行性能基准测量，不把减少反射次数直接表述为已测得的性能收益。
