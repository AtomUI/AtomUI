# ColorPicker 桌面版实现原理

本文档描述 ColorPicker 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [ColorPicker 桌面版架构设计](overview.md)，变化记录见 [ColorPicker Changelog](changelog.md)。涉及 Control Own Token 的实现应同时阅读 [ColorPicker Token 设计](token.md)。

编辑与提交的职责划分见 [ColorPicker 确认与取消设计](confirmation-design.md)：宿主拥有提交值，面板拥有候选编辑状态，确认操作执行写回，关闭通知负责结束编辑及释放订阅。

Popup 接入边界：`AbstractColorPicker` 负责业务状态和内容准备，color panel Popup 负责实际显示。模板重建或宿主切换时必须释放旧 Popup 事件并接入新的模板绑定与 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 ColorPicker 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls.ColorPicker`：代表文件包括 `AbstractColorPicker.cs`、`AtomUIColorPickerThemesProvider.cs`、`ThemeManagerBuilderExtensions.cs`、`ColorBlock.cs`、`ColorChangedEventArgs.cs` 等。
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider`：10 个文件，代表文件 `AbstractColorPickerSliderTrack.cs`、`AbstractColorSlider.cs`、`ColorPickerSliderTrack.cs`、`ColorSlider.cs`、`ColorSliderPseudoClass.cs` 等。
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorView`：6 个文件，代表文件 `AbstractColorPickerView.cs`、`ColorPickerInput.cs`、`ColorPickerView.cs`、`ColorSpectrum.cs`、`ColorSpectrumPseudoClass.cs` 等。
- `src/AtomUI.Desktop.Controls.ColorPicker/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.Localization`：生成 Catalog descriptor、语言模块注册入口和 `ColorPickerLangResource` 扩展。
- `src/AtomUI.Desktop.Controls.ColorPicker/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.ResourceHost.ScopedResourceHostGenerator`：1 个文件，代表文件 `GenerateScopedResourceHostAttribute.g.cs`。
- `src/AtomUI.Desktop.Controls.ColorPicker/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator`：生成 `GeneratedControlPackageRegistration.g.cs`、`GeneratedThemeSchema.g.cs` 和 `TokenResourceConst.g.cs`。
- `src/AtomUI.Desktop.Controls.ColorPicker/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.ThemeAssetManifestGenerator`：生成独立主题叶子的 `GeneratedControlThemeAssetManifest.g.cs`。
- `src/AtomUI.Desktop.Controls.ColorPicker/Localization`：`ColorPickerLangResourceKind.cs` 定义稳定 Catalog，`en-US.xlf`、`zh-CN.xlf`、`zh-TW.xlf` 提供内置翻译。
- `src/AtomUI.Desktop.Controls.ColorPicker/Properties`：1 个文件，代表文件 `AssemblyInfo.cs`。
- `src/AtomUI.Desktop.Controls.ColorPicker/Themes`：21 个文件，代表文件 `AbstractColorPickerTheme.axaml`、`AbstractColorPickerTheme.cs`、`ColorBlockTheme.axaml`、`ColorPickerPaletteGroupTheme.axaml`、`ColorPickerTheme.axaml` 等。
- `src/AtomUI.Desktop.Controls.ColorPicker/Utils`：5 个文件，代表文件 `ColorPickerHelpers.cs`、`Hsv.cs`、`IncrementAmount.cs`、`Rgb.cs`、`TransparentBgBrushUtils.cs`。

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- `ColorPickerToken` 通过源生成的版本化默认计划计算24个Own字段，四条Own依赖使用合并后的上游值，不能在控件或模板重复推导同一默认公式；明确下游覆盖与0值保留。具体依赖及输入尺寸的兼容回退见[token设计](token.md#默认依赖与明确覆盖)。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractColorPicker`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorPickerSliderTrack`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorPickerSliderTrackTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AbstractColorPickerTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AbstractColorPickerView`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorPickerViewTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AbstractColorSlider`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorSliderTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AtomUIColorPickerThemesProvider`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorBlock`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPicker`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerCollapse`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerInput`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerLangResourceExtension`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerPalette`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerPaletteGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerPseudoClass`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerSliderTrack`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerToken`：ColorPicker Own Token builder，使用该 Control 的 Effective Global Token 计算默认值。
- `ColorPickerTokenResourceExtension`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorSlider`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorSliderThumb`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorSpectrum`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- 其他 11 个内部类型按源码目录分层维护，修改前应先确认所有引用路径。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

ColorPicker 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

控件的状态入口按以下语义维护：

- 内容与数据：`ColorValue`、`ColorValueBrush`、`DefaultValue`、`EmptyColorText`、`GradientValue`、`IsTextVisible`、`MaxValue`、`MinValue`、`Value`、`ValueSyncStrategy`。
- 选择与集合：`ColorModel`、`IsEmptyColorMode`、`IsPaletteGroupEnabled`。
- 交互与状态：`IsNeedConfirm`、`IsActivated`、`IsAlphaEnabled`、`IsAlphaVisible`、`IsArrowVisible`、`IsClearEnabled`、`IsColorSpectrumSliderVisible`、`IsFormatEnabled`、`IsMotionEnabled`、`IsPerceptive`、`IsPointAtCenter`。
- 视觉与布局：`Color`、`ColorComponent`、`ColorSpectrumComponents`、`HsvColor`、`MarginToAnchor`、`Placement`、`PlacementAnchor`、`PlacementGravity`、`Shape`、`Size` 等 12 项。
- 动效与异步：`MouseEnterDelay`、`MouseLeaveDelay`。
- 其他稳定入口：`ActivatedThumb`、`Components`、`DecreaseButton`、`Format`、`IncreaseButton`、`MaxHue`、`MaxSaturation`、`Maximum`、`MinHue`、`MinSaturation` 等 14 项。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- `IsPickerOpen` 保存业务打开状态，`Popup.IsOpen` 保存物理宿主状态。`IsPopupPinnedOpen=true` 时，`CoerceIsPickerOpen` 拒绝普通关闭请求；只有 `ClosePickerForLifecycle` 持有的 lifecycle scope 可以把业务状态置为 false。物理 Popup 由代码在 template part、placement、pinned 模板绑定与 dismiss 设置完成后打开，不通过模板 TwoWay binding 竞争状态。
- `ColorPicker.Value` / `GradientColorPicker.Value` 是 Form 和绑定的单一提交值 owner，默认 `TwoWay` 并启用 Avalonia 数据验证。确认模式中的面板输入、预设色、滑杆、渐变节点和 clear 路径只更新候选状态；确认后以 `SetCurrentValue` 写回宿主，再由属性变化刷新色块、文本、值事件和 Form 状态。
- 候选空值与用于编辑的默认色分开表达，不能把白色、透明色或空渐变当作 `null` 的提交替代值。渐变候选拥有独立画刷与 stop，禁止通过共享引用修改宿主提交值。
- 确认模式下外部值写入与 Form clear 使旧草稿失效；进入或退出确认模式也重置草稿，不隐式提交。取消只能丢弃当前草稿，不能用打开时的备份覆盖最新外部值。具体转换与事件顺序见确认专项设计。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- `OnApplyTemplate` 在连接 `PART_Popup` 的 pinned 模板绑定、事件和 dismiss 行为后，才把已有的业务打开状态投射到物理 Popup；Popup 外部关闭再回写业务状态。pinned 锚点暂时隐藏时，业务状态保持不变，物理宿主关闭并由共享 Popup 在锚点恢复后重开。
- 开启动画由共享 Popup 管理；`PopupMotionActor` 无论在 `Opened` 前还是后挂载，都必须进入同一开启动画路径。`Closed` 负责取消动画并释放当前 actor，ColorPicker 不保存或补偿 motion actor 状态。
- 首次打开竞态、Popup 直接 Child 转发契约、pinned light-dismiss 全局审计与回归测试范式统一记录在
  [Semantic Part Popup 首次打开生命周期竞态案例](../../../../engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md)，本控件文档只维护 ColorPicker 自身不变量。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- 确认模式下，普通关闭、卸载和模板重建不提交草稿；先结束编辑状态，再释放旧面板输入与操作区订阅。钉住弹层中确认或取消后按最新提交值重置候选，保持业务打开状态。物理 Popup 因无效锚点或宿主失效关闭时同样丢弃旧草稿，恢复显示不复活已失效候选。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_AlphaInput`：承载Alpha百分比编辑；其宽度由输入ControlTheme按明确Own覆盖或历史48 DIP回退解析。按钮宽度由同一主题投射到所有数字输入，详见[token设计](token.md#默认依赖与明确覆盖)。
- `PART_BValueInput`：承载文本输入、过滤、显示或编辑入口。
- `PART_CheckedMark`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ClearColor`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ColorFormatComboBox`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ColorIndicator`：展示指示器、进度、分页或状态反馈。
- `PART_ColorPreview`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ColorPreviewFrame`：承载根视觉、边框、背景或尺寸基线。
- `PART_ColorText`：承载文本输入、过滤、显示或编辑入口。
- `PART_ColorTextPanel`：承载集合项、布局面板或虚拟化内容。
- `PART_DecreaseButton`：承载用户触发入口、导航或关闭动作。
- `PART_EmptyColorFrame`：承载根视觉、边框、背景或尺寸基线。
- `PART_Frame`：承载根视觉、边框、背景或尺寸基线。
- `PART_GValueInput`：承载文本输入、过滤、显示或编辑入口。
- `PART_GradientColorSlider`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_HValueInput`：承载文本输入、过滤、显示或编辑入口。
- `PART_HexValueInput`：承载文本输入、过滤、显示或编辑入口。
- `PART_HsvValuePanel`：承载集合项、布局面板或虚拟化内容。
- `PART_IncreaseButton`：承载用户触发入口、导航或关闭动作。
- `PART_InnerEllipse`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_InputTarget`：承载文本输入、过滤、显示或编辑入口。
- `PART_LayoutRoot`：承载根视觉、边框、背景或尺寸基线。
- `PART_PaletteGroup`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Popup`：承载弹层宿主、打开关闭或候选内容。
- 其他 10 个 part 按相同生命周期规则维护。

Semantic Part marker 的维护边界：`ColorPickerTheme.axaml` 与 `GradientColorPickerTheme.axaml` 各自承载 `semantic-body`（`PART_ColorIndicator`）、`semantic-description`（`PART_ColorText` / `PART_ColorTextPanel`）与 `semantic-popup-root`（`PART_Popup` 直接子节点 `ColorPickerPopupRootFrame`——`Border` 子类，向内容层 `ArrowDecoratedBox` 转发 `IArrowAwareShadowMaskInfoProvider`，共享 Popup 的箭头布局与阴影遮罩机制只识别直接 Child；语义边框贴合弹层外沿，对齐上游 popup root）三个静态 marker；共享 `ColorBlockTheme.axaml` 承载 `semantic-content` marker（`PART_ColorPreview`）。`content` 部件声明 `CrossNestedOwners=true`，route 以 `.semantic-body` 为锚点经第二个 `/template/` 跨入 ColorBlock 自身模板，弹层内的 ColorBlock 实例（清除示例、颜色预览）不带 `semantic-body`，不会被误命中。`popup.root` 不落在弹层 View 模板内的原因：View 由 `CreatePresenter()` 在打开时动态创建，其模板节点的 `TemplatedParent` 链断在 View 上，owner 的 `/template/` route 在 overlay 弹层下不可达；静态包裹 Border 经 TemplatedParent 传播保持可达且默认零视觉影响。

## 6. 交互与事件处理

ColorPicker 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

稳定事件路径包括 `ClearRequest`、`GradientActiveStopChanged`。事件参数和触发时机属于兼容边界。

纯色和渐变View的属性回调必须调用基类，确保公开Alpha可见性／可用性更新到实际输入与滑块；画刷刷新由基类统一处理。隐藏格式选择器保留当前格式，不能向Format枚举属性写入布尔值。

格式选择在ColorPickerInput的ControlTheme中声明：编译型SelectedValueBinding读取ComboBoxItem.Tag，SelectedValue与输入区Format使用原生双向TemplateBinding，再由既有双向模板绑定连接公开面板Format。格式映射不再由控件代码维护SelectedIndex或SelectionChanged订阅；重模板时由原生模板生命周期释放旧绑定。

输入区离树期间记录外部ColorValue／Format是否发生变化，回树时只在有此类变化时按当前值刷新，并清除待回放状态。即使离树A→B→A也代表外部覆盖；没有外部变化时保留无效／有效草稿、校验及光标，不能无条件ResetInput。字段可能包含无效或尚未提交的编辑文本，因此字段值的条件同步属于代码边界，不能以无条件展示绑定覆盖；格式选择本身仍由ControlTheme声明。重新应用模板或显式ResetInput同样清除待回放状态。

确认模式的输入事件收敛为候选编辑、确认、取消三类操作。候选编辑不通知 Form；确认先同步有效输入，再写回宿主，随后对非空值发出一次 `ValueSelected`，最后请求关闭。关闭回调不能重复提交或再次发出选择事件。默认模式仍按原有 `ValueSyncStrategy` 与关闭事件处理，完整矩阵见 [确认与取消设计](confirmation-design.md)。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 色谱内部`PART_LayoutRoot`使用Canvas提供测量边界；按实际绘制尺寸更新的子层不能以旧位图尺寸撑住下一轮自然测量。原有SizingPanel、矩形与透明输入Canvas保留，宽度缩小和加宽配置撤回均须检查同实例尺寸与指针命中。
- `ColorSlider`只提交最新颜色／布局请求的后台位图，较早请求晚完成不得覆盖当前内容；脱离视觉树时的结果不能写回，重新挂载时即使Bounds和颜色没有再次通知也要按当前值重算。过期任务继续释放自身像素缓冲，不处置较新位图。运行时像素生成属于控件绘制边界，不能由ControlTheme绑定替代。
- `ColorSpectrum`的UI线程请求代次同时覆盖后台生成与同步fast路径。后台完成时先核对代次，再创建当前位图；分量、尺寸及范围元数据来自同一启动快照，不能按完成时的当前属性解释旧像素。旧请求失效后仍释放自己的临时缓冲。
- 色谱范围沿用`min >= max`时min为唯一值的约定；生成像素和已应用map元数据必须使用同一归一范围。零跨度的选择环比例与单像素轴比例取0，避免NaN；共享键盘增量helper在等值／倒置范围直接返回该分量min，保留其余分量。此处不改变公开View的Components默认值或模板接线，也不表示现有键盘焦点路径已接通。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- `ColorBlock` 的空颜色斜线使用最终 Bounds 定位端点，不计入外部 Margin，也不使用测量阶段的尺寸代替绘制区域。
- 渐变触发文字的正常前景由 owner `Foreground` 投射到 `PART_ColorTextPanel`，默认读取有效 `ColorText`。当前活动节点沿用 `ColorTextDisabled` 的弱化语义；禁用时文本区统一读取有效禁用色。文字生成及选择变化只同步内部节点标记，颜色由主题声明，不能把当前画刷写成本地快照。
- 渐变文字由 formatter 运行时生成，未加入 owner 的 `TemplatedParent` 链；节点状态样式声明在模板中 `WrapPanel.Styles` 的逻辑子树作用域。`description` 专用 Style 作用于面板，正常文字继承其前景；formatter 明确设置的本地前景继续遵循 Avalonia 原生样式优先级。
- 颜色候选与提交值的隔离、输入有效性检查、渐变深复制、外部值更新及 pinned 操作转换统一遵循 [确认与取消设计](confirmation-design.md)，不套用日期专用编辑会话。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

### 自绘空值标记与选择环

ColorBlock保持原空值斜线坐标和默认红色2 DIP。ControlTheme将所属登记owner的有效LineWidthBold投射到内部绘制线宽，明确0时不生成斜线；有效ColorError只在配置明确提供该角色时投射，未明确提供及撤回配置保留历史红色。ColorBlock按实际画刷与线宽惰性创建自身Pen，输入变化仅清理该缓存并使绘制失效，配置状态由原资源宿主绑定管理。

Spectrum选择环在自身ControlTheme的style setter消费LineWidthBold，默认2 DIP。Popup View是独立公开主题owner，宿主Picker的专属Global不会跨越该身份替代View的配置；general Global、View专属Global和scope重挂载沿既有解析链更新。角色颜色使用算法之后的有效值，不能将原始输入色当作暗色算法输出。此接线不增加公开API、Own Token或Semantic Part。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 ColorPicker 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- 确认与取消能力按 [专项验证矩阵](confirmation-design.md#10-验证要求) 验证默认兼容、候选隔离、清空确认、取消与所有关闭路径；临时验证代码的去留遵循项目测试生命周期规范。
- Semantic Part 契约与 marker 变更运行 `tests/AtomUI.Desktop.Controls.Tests/ColorPicker/ColorPickerSemanticPartTests.cs`（descriptor 五部件、模板静态 marker 清单、默认主题不消费 semantic selector、`IsPopupPinnedOpen` 公共 API、overlay 弹层下生成 Style 命中触发区与 popup.root 目标）。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

### 历史渐变 formatter 委托兼容

历史SetPresetColor入口接受两参数Func，注册的ColorTextFormatterProperty为三参数Action。入口使用强类型适配：先调用Func取得集合，再清除和替换输出，避免复用控件仍属于旧面板时直接添加；返回同一输出集合时不重复添加。回调失败时适配器不清空原输出；既有GenerateValueText准备阶段仍遵循原合同。适配不使用反射、DynamicInvoke或新的订阅，保持原公开签名和附加属性类型。
