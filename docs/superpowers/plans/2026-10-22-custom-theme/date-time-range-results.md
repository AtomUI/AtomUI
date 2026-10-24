# CT-16 D3-A 范围时间 Own 与指示条几何

此包核定TimePicker的两个历史范围字段，修复实际范围输入的动态指示条几何。完整日期家族尚未通过；重套模板的既有释放问题仍在CT-25处理中。

## 合同与实现

RangePickerArrowMargin和RangePickerIndicatorThickness在RangeTimePicker上作为OptionalOverride：TimePicker Own明确配置优先，缺省／清除恢复原InfoPickerInput有效Own。保留默认水平margin8、厚度2、字段名称／类型／ID／slot；不新增RangeTimePicker Own身份。0值有效，负Thickness保留原合同。单值TimePicker与RangeDatePicker不读取这两项覆盖。

派生ControlTheme以有限三输入转换器选择值。共享RangeInfoPickerInput只发布实际输入宽度、水平位置及框体底边，Rectangle最终Width、Canvas.Left和Canvas.Top在ControlTheme声明；Top由框体底边减当前Height。运行期布局输出无法用静态主题推算，代码不直接设置叶子样式。保留原边框／裁剪、尺寸档位、Start／End切换、普通motion与已提交值。

成员先纯移动再修改：两个移动块的哈希及非空行集合一致，记录于date-time-range/member-moves.json；当前整个RangeInfoPickerInput成员审计无问题。

## 失败与聚焦验证

- 两字段初始9项验证，原版7失败／2正对照通过；另有实际Start／End热更新2项失败、厚度单独更新2项失败。无效夹具编译和错误默认假设不算产品RED。
- 候选17项Own／几何／scope／移除释放，加原有TimePicker／DatePicker133项，共150项通过，11秒。覆盖0／负margin、通用回退、明确覆盖及清除、日期兄弟隔离、原根尺寸不变时的输入缩放、实际底边与reparent。
- 移除整个RangeTimePicker后，在provider仍存活且真实renderer tick之后，根／箭头／指示条三项弱引用均释放。它不代替重套模板释放结论。

## 实际 Skia

实际AtomUI Window 1000×800、scale1；四个RangeTimePicker分别Small／Middle／Large／Custom，固定10:15:30→12:45，另有RangeDatePicker兄弟。鼠标真实激活Start／End，pin弹层后把焦点移至Action以稳定光标，保留真实overlay与正常motion。

同一驱动在原版和候选各执行明暗15阶段，旧版各14项失败、候选各0。16帧兼容状态逐像素差异0；14帧变化为已复现的宽度／坐标／底边修复及新接通Own配置。旧版margin8→30后输入174→152而指示条仍174；厚度2→9仍沿旧Y30绘制至39，实际框体底边32。候选宽度152、Y23，高9，底边32；清除回默认后End实际X216、宽174，修复旧X238／宽152。

A/A差异0；单变量1DIP检错明4643／暗4646像素，恢复后0。普通Generic配置往返和关闭恢复均0。8个默认资源的明暗16行名称／ID／slot／值完全一致。End过渡采到中间位置、单调抵达目标，无重新启动；四组时间值全程不变。无mask、容差白名单或图像对齐。

## 生命周期未决与后续

重套模板后旧箭头／指示条在原版与候选都被保留（相同Generic13／9、实际创建不同新部件、渲染tick与GC）。当前树引用正确、焦点为空、旧节点脱离树且Parent／TemplatedParent为空。堆证据另发现复用时钟图标的TemplatedParent仍指向旧PickerClearUpButton；仅修正该指针仍有3条非栈全局引用，经当前Classes、旧StyleClassActivator和StyleInstance保留旧部件。因此没有仓促修改共享IconPresenter或加入范围专用绕路。此失败明确继续CT-25，不能声明完整生命周期通过。

## NativeAOT、清理与审查

当前Mac NativeAOT实际执行相同明暗30阶段，所有行为断言通过，与JIT30帧逐像素差异全部0；没有IL警告，5条本机OpenSSL／Brotli部署目标版本警告已按实际来源记录。此证据不代替原生最终显示面、其他OS或Browser验证。

17项已完成临时测试删除；另1项未解决的重套模板失败迁入Temporary/CT25RangeLifetime，继续跟踪而非转成常驻测试。423个驱动及构建文件、两个含9份源字节的备份、媒体run全部清理，审计无二进制残留。额外堆诊断8文件／514402865字节也已删除；保留数值、引用路径与测试处置文字，不保留dump或图片。

正式TimePicker Token／实现／Changelog、共享输入架构及两行Own目录同步。Own目录仍1221项／1221唯一键，Control目录721项、历史V2终端13／175字段均不变。4个C#文件审计无问题，49条相对链接有效，暂存内容为空。作者另步审查确认最终Setter均在ControlTheme、可选存在位保留0、通用回退未换源、实际过渡与提交状态有证据；释放未决没有填成通过。清理后最终TimePicker／DatePicker精确133项通过（8秒），NET8 Release构建零警告／错误；误用Debug仅net10依赖的首次net8命令不算产品失败或验证通过，最终命令已按仓库Release多目标配置执行。此包不是完整CT-16或CT-25完成结论。
