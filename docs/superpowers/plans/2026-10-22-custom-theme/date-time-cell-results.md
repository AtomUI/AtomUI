# CT-16 D2-B 公开 TimeViewCell 消费结果

本包沿D1／D2-A清理后的feature/custom-theme工作区推进，单agent，不建分支／worktree、不提交。实现与当前Mac专项已验证，完整家族及原生最终显示面继续 [CT-16／CT-25](tasks.md)。[执行计划](date-time-consumers.md) 保留D2内部TimeView Global／非法行高及D3范围、日期家族边界。

## 接线与兼容选择

公开Cell通过内部只读InheritanceParent桥接真实字体；FontFamily／FontSize的最终选择仍在AXAML。缺省保留祖先字型，明确配置后取自身有效Global，LocalValue与原生Style优先级保持，无新公开属性或字号预设。圆角取自身BorderRadiusSM；行高／padding仍是TimePicker Own角色。

普通、hover、selected的画刷保留原ListView角色回退，仅Cell本身明确配置相应Global时切换。Core内部ControlOnly存在位区分根Global与Cell直接输入，避免根Global绕过旧ListView Own／control Global；公开WhenExplicit原语义保持，没有新增public API、Own字段或资源ID。

原PART_ContentPresenter禁用selector未命中真实ContentPresenter。新disabled色在Cell owner上声明并经原TemplateBinding传递；只在Cell明确配置ColorTextDisabled时激活。曾试用“值相同的回退Setter”，实际仍改变未配置时普通应用Style的优先级，现以内部bool和有限IBrush存在性转换门控整个Style。透明色是明确值，清除后Style完全退出，不擅自改变原默认禁用色。

## 失败、优先级与生命周期

有效初始RED **8 FAIL／2正对照PASS，425ms**：字体族／字号、普通／selected／真实hover、两种disabled、旧ListView与新Cell配置组合均未正确消费。原继承Courier19及本地Arial21／Orange等两个正对照通过。ListView名字空间歧义和Shouldly表达式树pattern两次fixture编译错误不计产品失败。

后续验证涵盖根Global与旧ListView Own／control Global组合、新Cell输入与清除、本地disabled优先级、普通公开Cell Style、专用AXAML声明、reparent／兄弟及旧provider更新隔离、ControlOnly与旧存在位并存、透明disabled色与圆角0。最终 **154 PASS，9s**（21临时＋133既有时间／日期）；Core资源原合同 **13 PASS，171ms**。

最初GC检查在布局后立即收集，三个对象仍存活。单变量处理一帧后释放；确切旧Cell／旧Theme也同样是layout-only FAIL、rendered PASS（328ms，新版367ms），证明是尚未释放的renderer旧场景，不能称为新资源泄漏。最终保留实际帧后的Cell／Presenter／Text三WeakReference释放检查，provider仍存活，且分离前断言23／Red，临时纯探针删除。

## 实际渲染与专用 Style

真实Skia、UseHeadlessDrawing=false、正常motion，AtomUI Window1000×800／scale1。六个公开Cell覆盖普通／selected／disabled／两者／本地覆盖／hover，父Courier19；左中右三点走真实pointer路径。实际两个12时TimePicker保持10:15:30；plain弹层解钉不等于关闭，因此后续styled阶段保留两个实际弹层。它们共享同一provider并在配置往返中保留实例。动作按钮真实点击一次，配置不提交时间。

专用TimePickerPopupItemStyle在编译AXAML中声明Foreground#722ED1、CornerRadius11、Arial21，实际134个styled行逐一命中，优先于Cell Global。测试锁定专用类与四个Setter，确认不存在Name／Loaded／Unloaded或FindControl样式回退。Part查找只读，公开根属性用于状态准备；原8行／滚动／裁剪和motion保留。

| 对照 | 完整RGBA结果 |
| --- | --- |
| 同进程A/A、1DIP恢复、旧ListView定制恢复、主场景配置清除 | Light／Dark均0 |
| 1DIP检错 | Light3,185／Dark1,631像素改变 |
| 38帧相同完整轨迹旧新重放 | 30个兼容帧中22为0；8个新配置帧为意图变化；其余边缘差异如下 |
| 当前Mac osx-arm64 NativeAOT对JIT | 38帧中34为0；4帧plain-popup／AA各3像素，均位于触发框圆角，另与最初旧版对应整帧完全相同 |

兼容帧中的plain-return／AA为Light7／Dark6，styled-return／AA为Light31／Dark23像素。旧新全部666／1169行实际几何、字体与绘制输入逐字一致，同一进程对应AA均0。旧版跨进程plain触发框本身有3像素变化；不以此给所有差异设置容差。

为证明styled选中行的变化，确切旧五文件仅使用旧版已支持的ListViewOwn ItemSelectedBgColor绿→清除，其他字型／尺寸／motion不变。1169行绘制输入恢复相同，旧版也产生选中行圆角残留；其结果与新版比较只剩Light6／Dark10个弹层外框边缘像素。记录保留全部坐标和颜色，没有遮罩、对齐、图像缩放或通用白名单。证据支持既有背景往返／圆角渲染状态的判断，外框及具体native函数归因继续CT-25，不宣称38帧全零或全家族视觉通过。

AOT与JIT的DRAW文字记录另有隐藏、空header的FontWeight.ToString别名差别（SemiBold／DemiBold）；本地Avalonia12.1.4二进制核查两者均600且相等，原始记录保留，未改写日志成逐字相同。专用数值probe运行后删除。最终无IL警告，5条已知Homebrew dylib最低OS linker警告单列。已查看旧ListView定制、新配置、暗色styled及亮色AOT画面。

## 清理与最终检查

删除21临时case所在1文件、driver429文件、三份源码备份JSON（5＋5＋2文件）、221份媒体／专用产物；媒体Git审计无发现，净增永久case为0。清理后Core **13 PASS，187ms**、TimePicker／DatePicker **133 PASS，9s**；NET8 Desktop Release（含Core／Common）**17.90s，0警告／0错误**。5文件／1控件成员审计0问题，作者自审，不冒充独立评审。

目录721、Own1221、历史V2的13终端175字段均保持。Cell无Own字段，本包不将它计入终端迁移数量。RangeTimePicker专用Style、其他日期宿主、全部Global／算法／极端值、持续动效与原生平台仍需后续逐项验收，完整家族计数仍0。
