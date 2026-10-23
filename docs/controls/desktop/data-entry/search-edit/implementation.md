# SearchEdit 桌面版实现原理

本文档描述 SearchEdit 桌面版的内部模板组合、搜索请求事件流、按钮与输入框布局、状态传递和维护不变量。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [SearchEdit 桌面版架构设计](overview.md)，Control Token 分层见 [AtomUI Control Token 设计规范](../../../../engineering/development/control-token-guidelines.md)，变化记录见 [SearchEdit Changelog](changelog.md)。

## 1. 实现定位

SearchEdit 的实现以 `LineEdit` / `AbstractTextInput` 为文本输入内核，以 `InputControlFrame` 为输入表面边界；AtomUI 在主题和少量内部类中加入搜索按钮、按钮状态传递和一体化布局。实现文档聚焦 SearchEdit 增强层，不重新说明 LineEdit 的文本编辑、清除按钮、Form、CompactSpace 和 TextPresenter 细节。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Input/SearchEdit.cs`：public API、默认 clear icon、模板接入、Enter 处理和搜索请求事件抛出。
- `src/AtomUI.Desktop.Controls/Input/SearchEditDecoratedBox.cs`：内部输入壳体，转接 SearchEdit 属性并订阅搜索按钮 click。
- `src/AtomUI.Desktop.Controls/Input/SearchEditPanel.cs`：搜索输入布局面板，负责左侧 AddOn、搜索按钮和内容框重叠边框排布。
- `src/AtomUI.Desktop.Controls/Input/Themes/SearchEditTheme.axaml`：SearchEdit 根模板、文本区域、内部 action 和 focus/status selector。
- `src/AtomUI.Desktop.Controls/Input/Themes/SearchEditDecoratedBoxTheme.axaml`：搜索按钮和输入框一体化模板。
- `src/AtomUI.Desktop.Controls/Input/Themes/SearchButtonTheme.axaml`：搜索按钮在不同输入表面下的状态视觉。
- `src/AtomUI.Desktop.Controls/Input/Themes/SearchButtonTheme.cs`：SearchButton Semantic Part typed theme。
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteSearchEdit.cs`：AutoComplete 搜索输入入口，复用 SearchEdit 搜索按钮属性。
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteSearchEditBox.cs`：AutoComplete 内部 SearchEdit box，使用 SearchEdit style key。

## 3. 核心类职责

`SearchEdit` 只增加搜索相关契约。它不重写 LineEdit 文本输入算法，也不管理搜索任务。`RaiseSearchRequested()` 是统一内部事件出口，负责在 `IsOperating=false` 时抛出包含查询文本快照和触发来源的 `SearchRequested`。

`SearchEditDecoratedBox` 是 SearchEdit 根模板中的组合布局节点。它继承 `AddOnDecoratedBox`，复用 `InputControlFrame` 的边框、圆角、effective status 和 CompactSpace 计算，并把 `SearchButtonStyle`、`SearchButtonText`、`IsSearchButtonLoading` 传给内部搜索按钮；它不重新计算 variant/status/error/warning/disabled。

搜索按钮直接使用 public `Button`，没有 SearchEdit 专用 Button 子类。Button 保留自己的 identity、Own Token、
loading、icon 和 wave 能力；`SearchEditDecoratedBoxTheme` 根据 owner 的 `StyleVariant` 与 effective status 把组合视觉
投射到 `Button#PART_RightAddOn`。

`SearchEditPanel` 是三段布局面板。它把左侧 AddOn 放在左侧，把搜索按钮放在右侧，把内容框扩展到搜索按钮左边框下方，以形成一体化边框。

## 4. 状态与数据流

搜索按钮属性流：

```text
SearchEdit.SearchButtonStyle / SearchButtonText / SearchButtonTheme / IsOperating
      ↓ TemplateBinding
SearchEditDecoratedBox.SearchButtonStyle / SearchButtonText / SearchButtonTheme / IsSearchButtonLoading
      ↓ TemplateBinding
Button#PART_RightAddOn.ButtonType / Content / Theme / IsLoading
```

输入状态流：

```text
AbstractTextInput.SizeType / StyleVariant / Status / IsEnabled
      ↓ TemplateBinding
SearchEditDecoratedBox
      ↓ frame state binding / selector
Button#PART_RightAddOn + PART_ContentFrame
```

`DataValidationErrors`、`FormStatus` 和 `Status` 在 `AbstractTextInput` 侧接入，`InputControlFrame` 消费这些稳定来源并计算 `EffectiveStatus`；SearchEdit 不维护第二套 error/warning 状态。

搜索事件流：

```text
Button#PART_RightAddOn.Click
      ↓
SearchEditDecoratedBox.HandleSearchButtonClick
      ↓
OwningSearchEdit.RaiseSearchRequested(Button)
      ↓
if !IsOperating raise SearchRequested

SearchEdit.OnKeyUp(Enter) when IsSearchOnEnterEnabled && !Handled
      ↓
mark KeyUp handled
      ↓
SearchEdit.RaiseSearchRequested(EnterKey)
      ↓
if !IsOperating raise SearchRequested
```

`IsOperating` 同时进入 `Button#PART_RightAddOn.IsLoading` 和 `RaiseSearchRequested()` 的重复请求保护。该状态不参与文本值同步，也不改变 Form value。`SearchRequestedEventArgs.Query` 在抛出事件前读取当前 `Text`，避免事件处理期间文本变化影响本次请求语义。

## 5. 生命周期与模板接入

`SearchEdit.OnInitialized()` 在未设置 `ClearIcon` 时写入默认 `CloseCircleFilled`，与搜索输入常见清除语义保持一致。

`SearchEdit.OnApplyTemplate()` 调用基类 LineEdit 模板接入后，查找 `PART_InputControlFrame` 并设置 `SearchEditDecoratedBox.OwningSearchEdit`。该 owner 引用只用于把搜索按钮点击回调到 SearchEdit；它不改变 frame 的状态归一或视觉 ownership。

`SearchEditDecoratedBox.OnApplyTemplate()` 必须先解除旧 `_searchButton.Click` 订阅，再查找新的 `PART_RightAddOn` 并订阅 click。模板重新应用时不能保留旧按钮事件订阅。

`SearchEditDecoratedBox.NotifyAddOnBorderInfoCalculated()` 把 `RightAddOnBorderThickness` 设置为当前 `BorderThickness`。搜索按钮右侧边框由 decorated box 统一计算，保证搜索按钮与输入框圆角、边框折叠和 CompactSpace 状态一致。

## 6. 交互与事件处理

搜索按钮点击和 Enter `KeyUp` 是 SearchEdit 的两个搜索入口。`IsSearchOnEnterEnabled=true` 且 Enter 事件尚未 handled 时，SearchEdit 先将事件标记为 handled，再进入统一搜索请求管线，避免上层默认按钮重复响应。关闭该属性时 SearchEdit 不消费 Enter 键。

SearchEdit 不会自动提交 Form，也不会在 `TextChanged` 时触发搜索。`IsOperating=true` 时按钮和 Enter 仍保持既有视觉与键盘所有权，但不会再次抛出 `SearchRequested`。

清除按钮、密码 reveal、文本选择、光标移动、复制粘贴和滚动仍由 LineEdit / Avalonia TextBox 路径处理。SearchEdit 不应在搜索按钮事件中直接修改这些状态。

Disabled 状态通过模板传递到 `SearchEditDecoratedBox` 和 `Button#PART_RightAddOn`。搜索按钮不应在 disabled 时成为独立可点击入口。

## 7. 内部算法与关键流程

### 7.1 SearchEditPanel 布局

`SearchEditPanel.ArrangeOverride()` 使用最终尺寸安排三个区域：

1. 左侧 AddOn 使用自身 `DesiredSize.Width`，高度为 `finalSize.Height`。
2. 搜索按钮使用自身 `DesiredSize.Width`，放在右侧，安排高度为 `finalSize.Height`。
3. 内容框宽度为总宽度减去左右 AddOn 宽度，再加回搜索按钮左边框厚度，使输入框右边框与按钮左边框重叠成单线。

该布局要求搜索按钮的可视 frame 高度跟随实际安排高度。Custom 高度下，如果按钮模板仍按 Button 自身默认 `Height` 绘制 frame，会出现按钮边框和输入框边框不齐。

### 7.2 搜索按钮高度同步

`SearchEditDecoratedBoxTheme.axaml` 将 `Button#PART_RightAddOn.Height` 绑定到 `SearchEditDecoratedBox.Bounds.Height`。Button 模板中的 `Frame` 使用 Button `Height` 绘制，因此该绑定使 Custom 高度和内置尺寸下搜索按钮 frame 与输入框 frame 保持同高。

该同步属于 SearchEdit 组合模板职责，不应通过 Gallery 示例、固定 magic height 或 SearchButton 全局主题覆盖来修补。

### 7.3 搜索按钮 style 映射

`SearchButtonStyle=Default` 时，搜索按钮使用 `ButtonType=Default`。Outlined 表面下，按钮 hover/pressed 会提升 z-index，避免按钮边框被内容框遮住。

`SearchButtonStyle=Primary` 时，搜索按钮使用 `ButtonType=Primary`。Outlined 表面下按钮保持激活 z-index。

非 Outlined 表面下，搜索按钮统一映射为 `ButtonType=Text`，让 Filled、Borderless 和 Underlined 输入表面保持轻量视觉。

### 7.4 搜索图标状态

搜索图标由 `SearchEditDecoratedBoxTheme.axaml` 直接创建为带 `skip-status` class 的 `SearchOutlined`。Button 没有
SearchEdit 专用 `OnPropertyChanged()` 路径；按钮基础前景由 Button theme 管理，输入 status 组合视觉由
SearchEdit owner theme 的 selector 管理。

## 8. 资源、性能与 AOT 边界

SearchEdit 不依赖运行时反射发现模板结构。跨模板协作使用固定 template part、`TemplateBinding`、selector 和 owner 引用完成。

资源和生命周期边界：

- 搜索按钮 click 订阅必须在重新套用模板时解绑旧实例。
- `OwningSearchEdit` 只保存当前模板 owner，不创建全局订阅。
- 搜索按钮高度同步使用 XAML binding，不在布局过程中写本地 `Height` 值。
- 搜索按钮状态不创建异步任务；业务异步状态由外部设置 `IsOperating`。
- SearchEdit 有独立 Control identity、没有 Own Token；运行时状态不得进入 Token schema。
- `SearchEditTokenResource` 只读取 SearchEdit 专属组合值；`InputControlFrameTheme` 读取 SharedToken 表达通用输入表面，Button 基础视觉继续显式读取 `ButtonTokenResource`。

AOT 边界：

- SearchEdit identity、强类型 TokenResource 和三个独立主题叶子通过生成 descriptor/asset manifest 注册。
- `AutoCompleteSearchEditBox` 使用显式 `StyleKeyOverride` 复用 SearchEdit 主题。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护，不依赖运行时反射扫描。

## 9. 维护不变量

内部重构必须保持以下不变量：

- 搜索按钮和 Enter 键只能通过 `SearchEdit.RaiseSearchRequested()` 抛出 `SearchRequested`。
- `IsOperating=true` 必须阻止重复搜索请求，并继续驱动按钮 loading。
- `IsSearchOnEnterEnabled=false` 时不得消费 Enter；handled Enter 不得产生搜索请求。
- `SearchRequestedEventArgs.Query` 和 `Trigger` 必须准确反映触发时的文本与输入来源。
- 搜索按钮和内容框的边框必须在同一布局高度下绘制。
- `SearchEditPanel` 的按钮左边框重叠算法不能破坏单线边框视觉。
- 搜索按钮必须接收 SearchEdit 的 `SizeType`、`IsEnabled` 和 loading；`StyleVariant` 与 `EffectiveStatus` 的组合视觉由 shared frame theme 投射。
- 搜索按钮必须保持 public Button 类型；不得重新引入借用 LineEdit 或 Button identity 的 internal SearchButton。
- `SearchButtonTheme` 必须继续作为强类型 Semantic Part Theme，并允许实例级替换；其状态色只能消费 frame 投射的有效状态，不得成为 validation owner。
- 搜索按钮右侧外部 AddOn 位置不可被用户内容替代。
- `InnerRightContent`、clear、reveal 和文本 presenter 的绑定仍由 LineEdit 模板路径维护。
- AutoCompleteSearchEdit 复用 SearchEdit 视觉时不能绕过 SearchEdit 搜索按钮契约。

## 10. 测试与验证

验证范围：

- `SearchEditBehaviorTests` 覆盖默认 Enter 行为、按钮与 Enter 触发来源、查询文本快照和搜索中重复请求抑制。
- `SearchEditLayoutTests` 覆盖 Custom 和内置 SizeType 下搜索按钮 frame 与输入框 frame 高度一致。
- `CustomizableSizeTypeContractTests` 覆盖 SearchEdit 和 AutoCompleteSearchEdit 的 customizable size contract。
- `LineEditShowCasePageTests` 覆盖 LineEdit / SearchEdit Gallery 示例结构、SearchEdit SizeType 示例和 snapshot。
- Gallery 走查 SearchEdit 基础、状态、尺寸、Custom、loading、disabled、内部右侧内容和 AutoComplete 搜索示例。
- 文档改动运行 `git diff --check`，并检查相对链接存在。

## 根表面主题投射

根几何投射保持搜索按钮组合；padding 恢复必须同时恢复输入高度、按钮宽度、焦点和选择区。

几何默认、原生优先级和组合边界见[共享输入表面](../input-control-architecture-design.md#81-根表面几何与预设默认值)。

### 根前景与字体消费

根 Foreground 的默认来自当前 Control 的 Effective Global ColorText，通过框体继承链到达真实文本。根 FontFamily/FontSize/FontWeight/FontStyle 的原生实例输入保留；语义 input 的专用 Style 可声明覆写，禁用及错误状态仍由对应状态样式处理。其他组合 owner 的独立子控件合同在各家族分别定义。

## 精确主题的资源身份

SearchEdit是独立Global owner；自身模板的caret/selection/placeholder、状态文字、间距与EnableMotion从SearchEdit typed资源读取，不能借用LineEdit或直接Shared查找。内部搜索按钮和公共frame仍按各自实际owner与模板关系逐项验收。

## 可选主题排版

SearchEdit根FontFamily和三档Global字号在对应字段显式配置时生效；未配置时保留真实InheritanceParent的字体。Custom采用中档，IsCustomFontSize=true退出主题字号，普通Style/LocalValue优先级保持。选择最终值的有限MultiBinding位于ControlTheme，继承父通过内部只读桥和AOT编译observable读取；不借用LineEdit的Own字段，也不通过代码设置最终字体。搜索按钮仍保留独立Button身份。

内部frame与操作按钮通过[共享输入架构](../input-control-architecture-design.md#内部资产的主题查询路线)消费真实公开owner的Global；Content附加区的逻辑路线与模板路线同属该合同，保留原生样式优先级和状态归一。
