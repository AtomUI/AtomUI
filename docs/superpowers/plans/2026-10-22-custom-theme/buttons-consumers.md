# CT-12 按钮 Global 与声明式消费分包

延续[家族计划](buttons.md)，在现有worktree独立执行，不提交、不委派。本包处理 Button、DropdownButton、HyperLinkButton、SplitButton、IconButton、ToggleIconButton 的真实Global与动效消费，默认兼容先采样。Option19另见[结果](option-buttons-results.md)。

- [x] 用各公开控件的有效实例证明高度/圆角/图标/边框等Global未消费，及剩余Transition时长固化；分开配置兄弟identity，并保留现有实例与状态。
- [ ] 建立同环境明暗、三档/Custom、普通/主色/危险、加载/禁用/图标、鼠标/键盘及组合场景默认基线，A/A0和1DIP检错；同状态旧新0未解释差异。
- [x] 各专属ControlTheme用自身类型化TokenResource消费Global；不能把无Own的公开Control借用Button identity。剩余Transition集合复用TokenTransitions。模板内的Global属于模板owner而非内部视觉节点，保留原生属性/Style覆盖。
- [x] HyperLinkButton加载图标margin的代码绑定审查：先证明现有契约，包括自定义模板与动态margin，再等价迁到ControlTheme；编译声明式不可达时记录具体原因与释放。
- [x] Button派生色表Capture<Button>跨identity行为单列实际测试，定义Own组合边界后整改，不能只换Globalaccessor而宣告Own隔离。
- [ ] 继承字体与显式FontFamily/FontStyle/FontWeight均按原生优先级逐项核对；没有前后证据不能无条件新增字体Setter。
- [ ] 同实例scope往返、原生优先级、关闭/重模板/释放、实际NativeAOT、NET8；清理临时源/媒体后focused、成员audit、正式文档和CSV/总表同步。原生用户闪动继续单独验收。

## Dropdown 颜色身份补全裁定

已确认 Dropdown 继承 Button 的渲染算法，但23字段终端只有排版/边距/禁用描边，算法还读取另一个 Button identity 的23个颜色/阴影Own。仅把Global accessor改成Dropdown仍会残留Button Own泄漏，且无法精确覆盖Dropdown自身的这些视觉维度。

为这些已有渲染语义在Dropdown增加23个同名Color/BoxShadows字段（终端23→46），各字段静态默认读取Dropdown有效Global，保留Button旧字段和类型；不引入字体/尺寸的跨identity继承，不共享有效Own表。渲染算法保留，reader按已知Button/Dropdown身份捕获，再以有限静态switch选择对应Own key；不使用Enum.Parse、字符串反射或运行时字典合并。未注册的应用派生类型继续按其已知产品基类契约回退。

未配置默认值与同状态像素必须和现有基线一致；Button.DefaultBg显式值不再渗透Dropdown，Dropdown的Global与Own均可独立配置。该行为是身份隔离修复，兼容说明记录此前意外共享；历史1195字段统计不改写，新增23字段另计。成本是新增公开生成字段及相应合同/导出记录，不能伪装成原有23字段已包含能力。

## ToggleIconButton 既有状态画刷入口

源码确认NormalIconBrush、ActiveIconBrush、SelectedIconBrush、DisabledIconBrush仅声明而未消费，也无既有调用者。保留四个API；ControlTheme为Normal提供ColorIcon、Active提供ColorText、Disabled提供ColorTextDisabled，Selected缺省跟随Normal。最终IconBrush在普通/checked/pressed/disabled分别引用对应入口，hover保持ColorIconHover，优先顺序disabled→pressed→hover→checked→normal，原生IconBrush本地值仍优先。全部在AXAML编译Self绑定表达，不新增事件或控件状态镜像。默认checked仍与normal同色，必须与旧基线逐像素比较；配置四画刷后的实际Skia输入和切换单独验证。

[当前分包结果](button-consumers-results.md)已通过193项清理后聚焦/NET8和当前Mac AOT。循环loading、全部字体/字段与原生显示面仍为家族出口，未勾选整体完成。
