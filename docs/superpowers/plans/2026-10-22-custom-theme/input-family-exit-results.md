# CT13 输入与选择家族出口结果

2026-10-30。所有剩余生产实现、临时清理与清理后精确验收已完成。用户已明确批准AM2/AM3/AM4/FU3/FU4五表限定视觉，既有其他批准保留；研发、清理、精确验收及视觉批准共同满足既定出口，CT13已完成并关闭。CT25/FW1仍按既定边界开放。

## 最后实现与真实反例

- FU3七公开字体族：7实际文字RED；可选字体/字号使用ControlTheme和既有只读真实继承父桥，原语义字号保持。见[字体结果](input-form-upload-typography-results.md)。
- FU4七根显式Global.FontSize原23未消费27的7项、四列表普通/失败文字8项真实RED；修复后实际字体/颜色、零圆角、描边3、11内部主题69处有效Global源均接到公开Upload。内部公开IconButton/ProgressBar的基础identity保留。
- Decorator/DropZone暗色正文旧#E0000000，现有效ColorText=#D8FFFFFF；表单冒号同根前景。最初夹具错误预期D9已纠正，随后单变量撤销两根接线验证正确D8预期的原版失败并精确恢复，不能把准备错误当产品证据。
- 原圆形项首次接有效资源后，配置往返变方，出现约2800像素差异；独立Circle失败为应102实际0。移除AbstractUploadPictureContent及Trigger各三处SetCurrentValue几何方法，ControlTheme按Bounds声明圆形默认圆角。Circle→Card→Circle、配置及清除、真实frame与mask恢复；默认Circle仍102，Card/Picture可消费零圆角。
- 六个Single/Multiple真实过滤输入光标/选区RED：Select/TreeSelect/Cascader共享私有过滤输入将正文、光标、选区、选中文字及禁用色读取各自公开owner有效Global。字体继承实测已经正确，未重新改造或增加Own。
- TextArea的三条Shared nominal padding曾被当成缺口；真实绘制输入早已由公开Padding以Template优先级投射。前后20对RGBA0，三条生产修改已按hash精确撤回，仅保留实际输入正对照，不拿内部getter失败冒充缺陷。

## 状态、Part与生命周期

最终52临时专项通过（29s）：字体继承/Style/Local/清除/换父、真正重建根模板、四列表、Pending/Uploading/Success/Failed/Cancelled/Success、真实图片预览替代节点、mask颜色与中心hover、动态配置及圆形往返。专用UploadListStyle/UploadItemStyle在编译AXAML声明Opacity0.81/0.73，所有状态容器命中，并通过原生selector资格切换恢复1；没有找节点写样式回退。两组14对象，共28个目标弱引用可回收。

曾直接Styles.Clear而保留应用scope的测试仍失败，已保留原日志。此为已独立验证的官方12.1.4样式Frame退役缺陷，见[CT25-FW1](framework-restore-validation-results.md)；用户已选择继续官方12.1.4。资格切换恢复不等于直接移除恢复，未修改框架/缓存程序集，也未把该原始FAIL改成PASS。CT25/FW1保持原边界。

## 渲染与原生

100对有明确同历史关系的当前切片图：FU3字体18默认/恢复0+2字体配置变化；FU4公开表面9零/11变化（含暗色可读性），Upload18零/2配置、过滤输入18零/2配置、已撤回TextArea审计20零。合计83零/17明确变化，无遮罩/裁剪/像素容差。Upload先前变方的失败帧独立保留，修复后18非配置/清除对全部0。详见[最后消费视觉](input-family-exit-visual-review.md)。

真实NSWindow最终managed与arm64 NativeAOT各196字体/字号+614出口=810检查，exit0；compiled专用Style、四列表全部状态、字体/选择颜色和清除均实际运行。使用AOT发布对应的同一份IL产物复核managed。没有将属性/布局检查称作原生截图或原始系统输入。AOT无IL2026/IL3050，五条OpenSSL/Brotli macOS26库与最低链接12版本提示保留；其他平台未验证。NET8 Release0警告/错误20.62s。

成员最终9文件/8Control/0问题；15项跨owner初始化移位及3旧几何方法删除有明确指纹差异，额外顶层类型保留。只完成作者另步自审，无独立agent评审。

## 家族范围与记录

目录91项=29公开类型、55内部目标、7抽象契约；29包含无独立Token的内容适配/基类/别名，不能写成29个独立主题identity。Own93条=88 Active、1Derived、4Legacy（RadioBgColor、SelectAffixPadding、FixedItemMargin、VerticalLabelPadding）；Legacy保留配置/key/ID/slot，不虚构内置消费者。每条映射见[家族核对矩阵](input-family-exit-matrix.md)。CT12 Submit/Reset继承仅核查既有Form行为及当前Button主题，没有修改其生产代码/进度或六份临时测试。

剩余四处Shared普通资源：CheckBox内部items的两个spacing默认被公开Group的TemplateBinding覆盖；SelectHandle图标fallback由真实IconPresenter的继承IconBrush覆盖；SelectTagAwareTextBox禁用foreground不替代各Tag及过滤输入明确来源。保留真实fallback与独立子Control身份，不按字符串搜索零命中判完成。TokenTransitions的SharedTokenResourceValue是已有owner时长描述符，非静态Shared消费。

## 清理与最后验收

本轮16份临时验证源（59286 bytes）、42份源副本/含源码inventory（1108745 bytes）删除；外部原生工程427文件/1,288,368,093 bytes删除。FU3 40PNG+manifest=41文件、FU4 181PNG+manifest=182文件，共223媒体文件全部清理，两次audit均clean；保留文字记录与hash，未链接已删除媒体。CT12原有5个C#加1个AXAML共6文件、31个受保护源码/资产hash全部保持。常驻测试净增0。

清理后首次精确家族719项为718PASS/1旧源码断言FAIL（要求已移除的ConfigureEffectiveCornerRadius）；更新既有断言以锁定Bounds声明式圆形和无代码CornerRadius写入，原视觉外壳/行为检查保持。精确该类9PASS74ms，最后家族719PASS/0Skip21s；Core直接消费者12PASS97ms、Generator默认合同1PASS1s、输入相关Gallery8个精确类23PASS9s，共755项清理后精确测试通过。不是全量回归。

最后成员门禁9文件/8Control/0问题，新增例外注释前后成员token指纹相等；git diff --check通过。ThemeTokenResourceProvider拒绝原型已恢复原SHA bddd0d12db97a20f703aa4d80c065d9bd3a8ea94a5c46c455c84f3b3363fd249。所有短暂源码撤销的mustRestore均为空。未新建分支/worktree、未使用agent、未提交或改写Git历史，未改CT12进度。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --no-build --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Input.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TreeSelectControl.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.CheckBox.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.RadioButton.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.AutoComplete.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Mention.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.NumericUpDown.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ButtonSpinnerTests.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.OtpLineEdit.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Form.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Upload.'
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~InputFoundationGlobalTokenTests|FullyQualifiedName~TokenResourceExtensionTests|FullyQualifiedName~ControlTokenIdentityOwnershipTests'
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ControlTokenDefaultContractTests'
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ComboBoxShowCasePageTests|FullyQualifiedName~ComboBoxSemanticPartHighlightTests|FullyQualifiedName~FormShowCasePageTests|FullyQualifiedName~FormShowCaseSemanticTabTests|FullyQualifiedName~UploadShowCasePageTests|FullyQualifiedName~UploadSemanticPartHighlightTests|FullyQualifiedName~CheckBoxShowCasePageTests|FullyQualifiedName~RadioButtonShowCasePageTests'
```

## 最终批准与关闭

2026-10-30，用户明确回复“批准五表限定范围，保留 CT25/FW1”（request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0）。已同步五张审查表、91目录项/93Own记录、tasks、inputs及本轮ledger。未新增代码或测试运行；沿用已完成的工程验证记录，原始失败不改写成通过，无提交授权。
