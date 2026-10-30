# DateViewer 桌面版实现原理

本文描述当前公共日期面板的源码职责、状态流和真实模板结构。公共契约见 [架构设计](overview.md)，
家族 ownership 见 [共享设计](shared-panel-design.md)，资源见 [Token 设计](token.md)，定制见
[Semantic Part 契约](semantic-part.md)，变化见 [Changelog](changelog.md)。

## 1. 实现定位

DateViewer/RangeDateViewer 是 public 控件，内部 DatePanel、DatePanelSession、DateViewerHeader、
DateViewerCell 与纯日期算法实现浏览和激活。托管时宿主提供投影、接收意图，选择 owner 不转移到 Cell。

## 2. 源码文件结构

源码根为 [DateViewer](../../../../../src/AtomUI.Desktop.Controls/DateViewer)。

| 文件 / 子域 | 职责 |
| --- | --- |
| DateViewer.cs、RangeDateViewer.cs | 公共属性、事件、独立提交、internal 宿主接入与语言生命周期。 |
| DateViewerRange.cs、DateViewerEnums.cs、DateViewerEventArgs.cs | 原子值、公开枚举与强类型通知。 |
| DateViewerCellContext.cs、DateViewerHeaderContext.cs | 强类型内容上下文与 Header 命令。 |
| DateViewer.SemanticParts.cs、RangeDateViewer.SemanticParts.cs | 显式公开 descriptor 与 owner-relative route。 |
| Internal/IDatePanelHost.cs、DatePanelModels.cs | 宿主投影、动作、纯快照和模型。 |
| Internal/DatePanelAlgorithms.cs | 单位周期、网格、边界和状态投影。 |
| Internal/DatePanelSession.cs | 交互 cursor、拓扑缓存、状态刷新与激活防线。 |
| Internal/DatePanel.cs | 模板 Grid、容器池、Pointer/键盘和 arrange 几何。 |
| Internal/DateViewerCell.cs、DateViewerHeader.cs | Cell 激活/内容与 Header 上下文投影。 |
| Internal/*AutomationPeer.cs | Grid 与单元格自动化语义。 |
| Themes/*Theme.axaml、*Theme.cs | 五个独立编译主题资产和样式。 |
| DateViewerToken.cs、Localization/ | 面板资源与 XLIFF Catalog。 |

## 3. 核心类职责

公共 owner 把外部属性转成 DatePanelInput；DatePanelSession 合并焦点、Hover 与宿主投影。
DatePanelAlgorithms 生成 DatePanelModel/DateViewerCellModel，DatePanel 将模型映射到有界容器池。
DateViewerCell 只保留对应模型与内容上下文，激活统一交给 session。

RangeDateViewer 的两个 DatePanel 共用一份 session；单端点、活动端点与完整原子范围由公共 owner 管理。
内部协作对象不成为第二个可写 Value，也不成为用户注册入口。

## 4. 状态与数据流

外部属性或用户动作 → owner 输入 → session → 模型 → 池化 Cell → 状态通知。
拓扑 key 去掉选择、焦点和 Hover；锚点、面板、周规则或约束改变时重建拓扑，其余变化投影状态。
可闭包捕获可变业务数据的 DisabledDate 在激活前重新验证，避免使用陈旧可选结果。

交互动作区分 Navigate、ChangePanel、Focus、MoveFocus、Hover、Activate、ActivateFocused。
程序 Value 赋值刷新属性与模型；独立用户提交更新 Value/DisplayDate，再按提交语义通知。
内部绑定使用 SetCurrentValue 保留调用方绑定。

## 5. 组合结构模型

角色树来自实际 [DateViewerTheme.axaml](../../../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerTheme.axaml)、
[RangeDateViewerTheme.axaml](../../../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/RangeDateViewerTheme.axaml)、
[DatePanelTheme.axaml](../../../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DatePanelTheme.axaml)和
[DateViewerCellTheme.axaml](../../../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerCellTheme.axaml)。

```text
DateViewer
└── Border
    └── DockPanel
        ├── DateViewerHeader (.semantic-header)
        ├── 默认 Today 页脚 (.semantic-footer，仅独立 Date 单位可见)
        └── Panel (.semantic-body)
            └── DatePanel#PART_PrimaryPanel (.semantic-content)

RangeDateViewer
└── Border
    └── DockPanel
        ├── StackPanel (水平 Header 行)
        │   ├── DateViewerHeader (.semantic-header)
        │   └── DateViewerHeader (.semantic-header，按第二面板可见性显示)
        └── StackPanel (.semantic-body)
            ├── DatePanel#PART_PrimaryPanel (.semantic-content)
            └── DatePanel#PART_SecondaryPanel (.semantic-content，条件可见)

DatePanel
└── DockPanel (.semantic-scope-panel)
    ├── Grid#PART_WeekHeader
    └── Grid#PART_CellHost (.semantic-scope-cells)
        └── DateViewerCell × N (运行时创建，.semantic-cell)

DateViewerCell
└── Border#PART_CellRoot
    └── Grid#PART_Layout
        ├── Border#PART_CellSurface（内缩状态表面）
        ├── Grid（完整列宽的范围/周背景）
        │   ├── Border#PART_RangeMiddle
        │   ├── Border#PART_RangeStart
        │   ├── Border#PART_RangeEnd
        │   └── Border#PART_WeekSelection
        ├── Border#PART_ValueFrame（前景）
        │   └── TextBlock#PART_Value
        └── ContentControl#PART_CellContent (.semantic-cell-content，前景)
```

| 节点 | 生命周期 owner | 稳定性 / 使用边界 |
| --- | --- | --- |
| DateViewer / RangeDateViewer | 应用 | public；值、模板、事件和 descriptor 可依赖。 |
| DateViewerHeader | 公共 owner 模板 | internal-observable；只通过 HeaderTemplate 与 header Part 定制。 |
| DatePanel | 公共 owner 模板 | internal-observable；content Part 不公开内部类型。 |
| DateViewerCell | DatePanel 容器池 | internal-observable；只使用 cell/cellContent Part 和强类型内容上下文。 |
| PART_* Grid / Border | 对应叶子主题 | 模板维护边界；应用示例不得依赖 Name 或自行穿透子模板。 |

Header 的独立叶子主题通过 Panel 组合自定义 ContentControl 和默认带底边框的 Grid；默认 Header 内容以 HeaderHorizontalPadding 保留左右导航间距，边框仍覆盖整行。默认导航按钮消费
DateViewerHeaderContext 的命令与参数。默认 Header 使用四个导航箭头和居中周期标题；范围两个 Header 与两个面板共享同一次浏览转换。
独立 DateViewer 的 Today 命令先回到当天所在日期面板，再经 DatePanelSession 的 Cell 激活防线提交；
托管状态隐藏该页脚，DatePicker 的 Footer 仍由 Presenter 拥有。

Content 单元格以外层透明 Border 保持整列命中；Grid 中的状态表面、范围/周背景和前景内容按绘制顺序分层。
范围/周背景直接占完整列宽，状态表面单独内缩并承载顶部边界与状态背景；前景日期和业务内容用共享 Token 推导的留白对齐，不使用负 Margin 补偿列宽。
普通 Today 日期的主色顶边、浅主色表面与主色文字不依赖可选择状态；边界或 DisabledDate 禁用当天时仍保留这些视觉标识，但 Cell 不接收激活。
Content 的日期值框将边框画刷覆写为透明，避免在日期文字下方出现额外边线；Week 与范围状态继续由自身的视觉规则负责。
范围与整周背景延伸到外层 Cell 边界，跨列连接不随内层视觉表面的留白断开；范围端点的值框保持列内居中，与左右半格背景相接。
DatePanel 的星期标题在模板应用后按模型创建，节点没有 TemplatedParent；主题将对齐、行高与内边距赋给
DatePanel 的内部属性，DatePanel 在生成或更新标题时应用这些值。

## 6. 生命周期与模板接入

DatePanel 模板替换先释放旧 Cell 和旧 session 订阅，再取得两个 Grid，订阅当前 session 并投影。
attach 恢复订阅，detach 解除 Changed/FocusRequested，释放 Cell 和模型引用。
Cell 回收清空 session、model、context、CellTemplate、FullCellTemplate 与默认内容投影。

公共 owner attach 获取有效语言并刷新快照，detach 解除 LanguageChanged。
DateViewer 对主 DatePanel.GeometryChanged 的订阅在 re-template/detach 前解除，
arrange 后把 Cell bounds 变换到 owner 坐标供 Calendar overlay 使用。

## 7. 交互与事件处理

Cell 的 Pointer 按下、捕获、释放/取消与键盘激活使用同一 session 提交入口；
Automation 同样通过有效模型与激活防线。Hover 从 Panel 的实际格位置投影，
范围预览不写入 Value。内容模板可替换内容，不能移除交互容器或绕过禁用。

Header NavigateCommand 使用 int，ChangePanelCommand 使用 DateViewerPanelKind；
公开命令不暴露内部 action 类型。Range Header 的上下文以各自显示锚点生成。

## 8. 内部算法与关键流程

Date、Month、Quarter、Year 四种网格的周期、槽位和下钻规则由 DatePanelAlgorithms 提供。
日期规范化、有效区间和约束共同决定可用状态，边界运算不能使 DateTime 溢出。
选择、范围端点/中段、Preview 与周行状态由模型产生，不能反向读取伪类作为算法输入。
Preview 能组成完整范围时，模型先按单位规范化两个视觉端点，再抑制 committed selected/range 状态；
不能组成范围时保留 pending selection。Cell 使用单一 visual-endpoint 投影主色，使 selected 与 Preview
状态切换时不会暴露中间空白帧。

键盘按当前列数移动，跳过不可焦点项；session 的范围共享 cursor 保证两个面板一致。
Calendar 的业务条消费实际 arrange 几何，不复制日期起点算法。

## 9. 资源、性能与 AOT 边界

主题声明默认资源、模板结构和伪类 Setter；公共面板默认不依赖 Picker 或 Calendar Token。
值框 `PART_ValueFrame` 的背景变化使用 MotionDurationMid；Week 的 `PART_WeekSelection` 保持可见，
在透明色、选中/悬停主色和范围中段背景色之间使用同一时长过渡。其它范围中段的背景切换不添加额外动画。
Content 的普通日期由 `PART_CellSurface` 绘制 Hover 与选中背景；该表面以透明画刷为静止基线，
背景和 `PART_Value` 的文字颜色使用 MotionDurationSlow 过渡。动效关闭时不安装这些过渡，状态颜色立即更新。
动效由共享 EnableMotion Token 控制，Transitions 声明在 ControlTheme 的 Style Setter 中。
DatePanel 保持有界容器池，状态改变复用拓扑；内容 revision 独立于拓扑，避免业务内容失效重建所有日期模型。
原始指针事件仅在 session 已有 Hover 时检查跨面板离开边界；无 Hover 时直接返回。有 Hover 时只检查该 session 已连接的面板，面板注册与 session 订阅同步建立和解除，避免滚动时遍历整个 TopLevel。

主题、Token、Semantic descriptor 与 XLIFF 使用生成器注册；Gallery 通过静态路由和
GeneratedLanguageModuleRegistration 接入，无反射控件发现。真实 NativeAOT、裁剪后冷消费
与性能收益需要专项证据，源码结构或普通构建不能替代验证。

## 10. 维护不变量

- 每个选择只有一个 owner；范围始终通过一个 DateViewerRange 提交。
- 内部宿主入口不公开，Cell 不写 Value。
- 默认结构归 AXAML，子主题维护自己模板；专用 Semantic Style 使用完整生成 route。
- 回收与 re-template 释放旧订阅和上下文；语言变化使用当前有效文化。
- 宿主业务提交、时间、Form、Footer 与 overlay 不进入日期算法。
- 保持原触发条件验证输入几何、滚动、裁剪和交互，不用删除约束代替修复。

## 11. 测试与验证

定向验证分为纯模型、公共面板、主题/Style、输入/Automation、生命周期和实际宿主。
Gallery 使用独立 DateViewer 页面验证用户消费，既有 catalog 和路由检查保护生成入口。
新增验收测试临时存放，验证后删除，遵循 [共享验收](shared-panel-design.md#10-重建边界与等价验收)。

公共面板源码和主题已落地，Calendar 接入与 Picker 编辑会话正在家族迁移范围内；
不能将此文档当作全部 Picker Presenter 已替换、旧 Gallery 像素等价或 NativeAOT 已验收的声明。
