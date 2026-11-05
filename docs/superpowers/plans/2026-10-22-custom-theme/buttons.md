# CT-12：按钮家族精确主题迁移

最终状态：2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。 早期阶段描述与原始FAIL保留，当前不再待V11/采样/视觉批准。见[完成结果](buttons-box-shadow-finish-results.md)。


2026-10-30状态更新：公开BoxShadow已选定/实现，默认原生managed及AOT各1236检查与52帧/12跨运行0、清理后189项/NET8/成员门禁已完成。当前仅待[最后限定视觉汇总](buttons-box-shadow-final-visual-review.md)，本文件旧V11待选或待采样描述保留为历史，不能作为当前状态。


单agent继续执行，现有feature/custom-theme工作树内修改，不提交、不创建分支、不使用CUA。上游CT-05机制及CT-10/11已验收的共享包可用于本家族；CT-10全owner、CT-11全消费清单和最终平台出口随各家族回填，不能相互等待形成循环，也不能据此提前标完成。

## 范围与入口

目录中本包包括Button、DropdownButton、HyperLinkButton、SplitButton、IconButton、ToggleIconButton、OptionButton、OptionButtonGroup及Common.SimpleButton；对应抽象基类也检查共享实现。五个现有Own终端分别属于Button、DropdownButton、HyperLinkButton、SplitButton、OptionButton。无Own的公开子控件保留独立Global identity，不借用Button配置；不为普通抽象实现增造identity。

主要源码为Desktop.Controls下Buttons、DropdownButton、SplitButton、OptionButtonGroup及对应Themes，Common抽象层在Controls/Commons。规范依据为Own默认求值、Control渲染、消费与视觉验证规范。正式文档更新各控件overview/implementation/token/semantic-part/changelog，不将实现详情只放任务文档。

## B1 默认基线与实际失败

- [ ] 从当前已验证工作树取得实际Window/Popup基线，明暗、Large/Middle/Small/Custom、普通/主色/危险、Outlined/Filled/Text、形状、图标/loading/disabled、组合分隔线与下拉打开分别取证。先A/A与1DIP检错；真实鼠标、键盘及焦点驱动保留事件计数。
- [ ] 临时验证在Desktop.Tests/Temporary/CustomThemeButtons，先证明字体Own→行高→Padding→extra/icon padding的有效上游依赖错误；显式下游、0与清除需独立断言。实际公开控件和生成资源都观察，不止检查Token对象。
- [ ] 静止、悬停、按下、释放、禁用/加载切换及作用域往返持续采帧，保留正常动效。原生显示面的用户闪动问题单独复现、记录时间线及渲染来源；Headless稳定不代替原生验收。新旧Overlay恢复边缘4px/RGB1按原条件追踪，不加容差掩盖。

## B2 Own V2与字段合同

- [x] 五个终端逐字段确定默认输入、有效Own依赖、单位及兼容名称，更新token-contract-review.csv；只迁移本家族。保留默认配色、阴影、尺寸公式与现有字段，包括IconOny历史拼写，不无授权改名。
- [x] 每字段一个ControlTokenDefault静态函数，显式声明Own输入；版本DependencyDefaultsV2/Revision1、sealed partial及IControlTokenDefaults。不从终端整体Calculate推断运行时优先级，移除旧整体evaluator和NaN/-1缺省哨兵的内部判断，缺省由存在位确定。显式非法字体输入的旧兼容语义先查实际合同，不把无效值当成缺省偷偷改变。
- [x] 在同一现有Compiler、scope与缓存上验证A→B→A、兄弟identity、依赖更新/显式下游/清除、惰性与异常。SplitButton组合子按钮的真正owner及独立主题合同单列，不能因组合而把SplitButton Own复制到Button。

## B3 消费与默认优先级

- [x] 逐主题和真实模板节点核对字体、字重、Padding、阴影、描边、圆角、颜色、图标、focus、loading与wave。可等价表达的绑定迁到ControlTheme，Transitions放style Setter；代码兜底写明原因与释放owner。
- [x] 未迁移的按钮Transition集合按M2机制接入，每集合证明默认及作用域。Custom只关闭自身尺寸预设，不接管未拥有的维度；shape/variant/size恢复不留本地值或旧缓存。
- [x] root用普通原生Style；Semantic PART仅使用生成专用Style，断言真实命中、优先级与Setter，禁止code-behind改目标属性回退。

### B3 字重兼容裁定

实际已验证FontWeight=700未消费，同时未配置按钮会继承祖先Bold。保留既有Own默认资源400；TokenResource的WhenExplicit只查询同一effective config的Own存在性，缺省资源为Unset，普通资源读取和默认求值不变。实际原生试验已证明直接把Unset绑定给FontWeight会落回Normal，且double不会自动转enum，不能据此宣称可选Setter已实现。

因此ControlTheme用MultiBinding选择明确的继承回退或已配置字重，并在AXAML承担最终FontWeight Setter。真实InheritanceParent是Avalonia受保护成员，可能不同于逻辑父节点；Button只观察该父对象字体并投射内部只读值，编译绑定使用原生AvaloniaProperty accessor，代码注释说明声明式不可达的具体原因。第三路保留原始资源查找，使局部资源覆盖继续由Avalonia处理；不以数值400猜缺省、不创建来源flag或优先级引擎。显式400/700、清除、祖先实时变化、本地属性/普通Style、局部资源及释放均实际验证。

## B4 出口

- [ ] 默认每个适用状态前后零未解释/未经批准像素差异；同实例主题往返保留focus、输入、checked/open/loading状态。合法原生交互位置变化与原缺陷修复分别记录。
- [x] 关闭/分离/重模板/替换与GC验证；当前Mac NativeAOT实际运行及NET8构建，其他平台准确挂CT-25。
- [x] 删除临时测试、驱动、埋点和全部媒体，之后只跑本包明确focused测试；成员布局audit、文档链接、diff check及作者自审。结果回填总任务和问题处置表，不因一个按钮通过宣告整个家族通过。

## 历史五字段兼容裁定

完整源码搜索确认Button的四ExtraContent字段无模板消费者，RightExtraContent仅为internal未接线属性；GroupBorderColor无当前公开Button.Group等价区域。分别归Reserved与Legacy，保留字段名、类型、资源ID、V2默认函数和配置往返；不创造新区域，不映射到CompactSpace或Dropdown。正式合同及XML摘要已同步，审计CSV精确五行CompatibilityRetained。CT23的字段目录必须显示历史保留且不可编辑。此为兼容分类/注释改动，未改变运行语句，无新增测试；完整按钮loading/持续动效与家族出口继续推进。

2026-10-29总出口：128有效Own逐字段与28业务/Style/GC全部通过；五历史字段仍按兼容裁定。清理后179项及Release NET8通过。该矩阵未覆盖原始V11的未导出预设色阴影入口，见[当前总出口](buttons-family-exit-results.md)。

当前未修改产品的原生矩阵：正常motion、实际NSWindow、指针/按下0及焦点保持，managed/AOT各482检查、52帧指定稳定比较0，跨运行12对0。V11预设色阴影入口仍缺，4项实际探针确认高度/颜色已有Global输入而Blur3/Spread0固定；不能仅核定[两对混合宿主细线](buttons-native-exit-review.md)就关闭家族。V11公开入口当前等待用户选择，落地后按实际改动重做必要门禁。
