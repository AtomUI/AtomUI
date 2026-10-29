# NumericUpDown 主题输入设计

本文定义 NumericUpDown 的有效 Global、显式复用的 ButtonSpinner Own、实际模板及兼容边界。公共状态和 API 见[总体设计](overview.md)，组合与更新流程见[实现原理](implementation.md)，变化记录见[changelog](changelog.md)。共享输入表面见[输入架构](../input-control-architecture-design.md)，通用 Token 合同见[全局规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 身份与职责

当前源码没有独立的 NumericUpDownToken Own 定义，也没有继承 ButtonSpinnerToken 的 NumericUpDown Own 终端。NumericUpDownTokens.Identity 及 NumericUpDownTokenResource 是该公开控件的有效 Global 入口，不能将其描述成一套可覆盖 HandleBg 等字段的独立 Own。

根排版、前景、占位、动效与几何读取 NumericUpDown 有效 Global。内部 NumericUpDownSpinner 未独立注册，共享输入表面沿真实树解析 NumericUpDown owner；手柄相关模板中显式声明的 ButtonSpinnerTokenResource 仍读取 ButtonSpinner Own 身份。这两条路径不互为隐式别名。

Value、Text、StringValue、Minimum、Maximum、Increment、校验、焦点、hover、Spin 与内容避让属于实例状态，不作为 Token。

## 2. 根与组合几何

根 FontFamily、FontSize/SM/LG 使用既有可选显式输入和继承规则；不虚构 InputFontSize 三项 Numeric Own。Padding、BorderThickness、CornerRadius 进入现有输入框体，状态色和 focus/hover shadow 由共享 frame 的当前合同处理。

Mode=Spinner 的左右按钮宽度跟随实际高度形成方形，分隔线在按钮之外占位；中间文本区独立消费根 padding。该模式加减图标宽高明确读取 NumericUpDown 有效 Global.IconSize，默认14，0与清除按原生绑定更新。尺寸属于父组合职责，不改成目标 IconButton 的尺寸默认。公开 IconButton 的颜色 identity 和状态规则保留。

两条inline分隔边读取传入的公开BorderThickness：减号侧取Right，加号侧取Left，Top/Bottom不进入竖边；默认1，0隐藏，非均匀及本地值按对应侧更新。它与内部浮动手柄绘制取Left的既有算法分别解释，不新增Own字段或LineWidth别名。

Mode=Input 的上下箭头继续读取 ButtonSpinner Own.HandleIconSize，默认来自 ButtonSpinner 有效 Global.FontSize/2，通常7；它不等同于加减图标，也不以 HandleWidth 决定方形加减按钮宽度。Custom 及其他预设按根现有几何/排版合同处理，不新增第四套 Own。

## 3. 显式复用的 ButtonSpinner Own

- HandleWidth 控制浮动上下手柄宽度，并参与偏移与内容避让。
- HandleIconSize 控制上下箭头，不控制 Spinner 模式的加减图标。
- HandleBg / FilledHandleBg 控制对应普通/Filled手柄背景。
- HandleActiveBg、HandleHoverColor、HandleBorderColor 分别进入已有按下背景、hover图标和分隔描边入口；以各模式实际模板为准。

这些字段属于 ButtonSpinnerTokens.Identity。ButtonSpinner 八项默认采用 DependencyDefaultsV2；FilledHandleBg 将有效 Global.ColorFillSecondary 叠加到有效 Own.HandleBg，显式 Filled 优先。详见[ButtonSpinner Own](../../navigation/button-spinner/token.md)。

ControlWidth 是 ButtonSpinner 的 Legacy兼容资源，保留90默认和资源/config读写，内置布局不读取；它不是 NumericUpDown 的默认 Width 设置，也不别名到实例 Width。

## 4. 消费边界与兼容

独立 ButtonSpinner、NumericUpDown 内部包装及公开 IconButton 各自的 Global 身份按实际职责区分。不得因类型继承或同字段名自动合并配置。禁用、错误/警告、动效、分隔线及其他视觉分支按对应主题实现核定，默认资源存在不等于全部状态已消费。

不删除或重命名既有公开身份、resource key、Own 字段、实例属性和 Part。纠正文档中不存在的 Numeric Own 类型/继承链不构成运行时 API 删除；新增独立 Own 必须另有设计和兼容决策。数值解析、精度、formatter、string mode 与草稿继续归控件状态模型。

## 5. 家族与验证

直接影响范围为 NumericUpDownTheme、NumericUpDownSpinnerTheme 及显式复用的 ButtonSpinner 主题；ComboBox 当前直接继承 Avalonia ComboBox，不因名称相似被当成 ButtonSpinner 派生者。

验证实际 owner、四尺寸两模式、图标/手柄/根输入、0/清除、显式优先级、数值/焦点、CompactSpace、原方形命中与布局；新视觉变化单独审查。按影响运行精确模块、Skia/当前平台 NativeAOT及NET8，保留未覆盖 Host 与完整家族出口，不把默认分包当作全家族通过。
