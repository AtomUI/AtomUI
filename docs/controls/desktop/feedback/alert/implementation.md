# Alert 桌面版实现原理

本文档描述 Alert 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Alert 桌面版架构设计](overview.md)，完整 Semantic Part 公共契约见 [Alert Semantic Part 契约](semantic-part.md)，变化记录见 [Alert Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Alert Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Alert 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Alert/Alert.cs`
- `src/AtomUI.Desktop.Controls/Alert/Alert.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Alert/AlertToken.cs`
- `src/AtomUI.Desktop.Controls/Alert/Themes/AlertTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- SemanticParts 文件只声明 descriptor 元数据；生成器产生 descriptor、名称常量和六个 public Style Type。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Alert`：public API 和运行时状态 owner，维护伪类、close button 订阅和 `CloseRequest`。
- `AlertToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Alert 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> PixelAlignedBorder / static presenter / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容：`Message`、`Description`、`ExtraAction`、`CloseIcon`。
- root 表面：`StrokeDashArray` 与继承的 Background、Border、CornerRadius、Padding 属性通过 TemplateBinding 投影到
  `PixelAlignedBorder`。
- 视觉状态：`Type`、`IsShowIcon`、`IsClosable`、`IsMessageMarqueeEnabled`。
- 交互输出：当前模板的 close button 点击后触发 `CloseRequest`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- `OnApplyTemplate` 在查找新 `PART_CloseBtn` 前解除旧按钮的 `Click` 订阅，再为新按钮订阅。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_CloseBtn`：稳定模板协作入口，重命名前必须同步主题和实现。

Semantic descriptor 到真实节点的映射：

| Part | Marker | 真实节点 | 创建方式 | 生命周期 |
| --- | --- | --- | --- | --- |
| `root` | 无 | Alert owner；表面属性投影到 `PixelAlignedBorder` | 用户或宿主创建 | owner 生命周期 |
| `icon` | `.semantic-icon` | 四个状态 `Icon` | ControlTemplate 静态创建 | 每次模板实例 4 个 |
| `section` | `.semantic-section` | 内容 `StackPanel` | ControlTemplate 静态创建 | 每次模板实例 1 个 |
| `title` | `.semantic-title` | `MessageLabel`、`MarqueeLabel` | ControlTemplate 静态创建 | 每次模板实例 2 个 |
| `description` | `.semantic-description` | `DescriptionLabel` | ControlTemplate 静态创建 | 每次模板实例 1 个 |
| `actions` | `.semantic-actions` | `ExtraActionPresenter` | ControlTemplate 静态创建 | 每次模板实例 1 个 |
| `close` | `.semantic-close` | `PART_CloseBtn` | ControlTemplate 静态创建 | 每次模板实例 1 个；Click 订阅随 re-template 更换 |

所有 marker 都由 AXAML 静态声明。隐藏、Type 切换与内容替换不创建或重新标记节点；模板重套用会整体替换 target 集合。

## 6. 交互与事件处理

Alert 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

`CloseRequest` 是唯一控件专属 public 事件。`HandleCloseBtnClick` 只转发事件，不修改 `IsVisible`、不从父级移除 Alert，也不启动
motion。业务层负责事件后的生命周期决策。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

1. `Type` 由主题 selector 映射到 root 背景、边框和四个 icon 的可见性。
2. `Description` 与 `ExtraAction` 通过 TemplateBinding 投影到静态 presenter；`UpdatePseudoClasses` 在模板应用时初始化两个伪类，
   任一属性变化时重新计算它们，使 `:has-description`、`:has-extra-action` 与 presenter 状态保持同源同步。
3. `IsMessageMarqueeEnabled` 在两个静态 title target 之间切换；两个节点共享 `Message` 数据源。
4. `IsClosable` 只控制静态 close button 可见性，`CloseIcon` 为 null 时在 Template priority 提供默认 `CloseOutlined`。
5. re-template 先释放旧 close button 的事件订阅，再为新 target 建立订阅并回放伪类与默认图标。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

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
- Semantic Part 使用生成期 descriptor 和静态 marker，不增加运行时 VisualTree 搜索、反射或 selector 字符串组装。

## 9. 维护不变量

维护 Alert 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- `root`、`icon`、`section`、`title`、`description`、`actions`、`close` 的名称、selector、ContractType、cardinality 和 marker 身份。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 `PART_CloseBtn` 的 Click 订阅释放路径与新 part 的重新订阅顺序。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

Alert 专项测试必须覆盖 descriptor、十个静态 marker target、六个生成 Style 的实际命中、四种 Type、可选区域显隐、跑马灯切换、
close button re-template 订阅和默认主题不消费 `.semantic-*`。Gallery 测试必须覆盖七项 Part 描述、owner-scoped 样式示例和
Semantic Preview 的延迟实例化。
