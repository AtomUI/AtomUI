# CT14 Tab：V2默认依赖与添加按钮恢复的本机结果

沿278429b50和现有checkout，由主agent执行；没有创建分支/worktree/clone、提交、推送或发布NuGet。本片是默认机制及已批准按钮尺寸修复的本机切片，不是完整Tab家族验收。

## 默认与资源身份

`TabControlToken`的34项默认接入`DependencyDefaultsV2`、Revision1；只增加`CardPadding`到有效Own `CardSize`的现行公式依赖。未新增TitleFontSize依赖、padding钳制、Token、Part或公开API。

原四项临时检查3 FAIL / 1 PASS，候选4 PASS。原明暗两项消费者反例共12处断链；408条metadata/key/slot记录及68条默认保持，仅4条CardPadding阶段值按预期变化：size56为16,16，size0为16,-12。明确下游3,4,5,6继续优先，其他404条值相同。`HorizontalMargin`的Undecided分类仍未裁决，没有借默认迁移关闭实际消费者缺口。

四个公开owner的Global身份独立，模板显式复用TabControl Own；不按CLR继承推定Own复制。

## 按钮尺寸与失败保留

两类Card、Top/Bottom、310×190、12item、SelectedIndex1、加号开启的同版本历史反例四项均失败：最终presenter646×29、item29一致，旧按钮却保留35/51。早期“两个Height都为0”的伪GREEN已经否决，之后明确锁定29，原日志保留。

尺寸关系从C# Arrange/回调回写移入主题。水平Height读取同模板ItemsPresenter.Bounds.Height，垂直保持CardSize正方形及原方向对齐。DesiredSize不提供路径更新通知；模板属性上的名称绑定又只弱引用NameScope，专用Style暂停绑定后NameScope无法恢复，原Height=NaN反例重新复现为1 FAIL。

最终使用模板内Style Setter保存编译绑定路径的NameScope。原29断言转GREEN，专用`CardTabControlAddStyle`/`CardTabStripAddStyle`的91×37覆盖和撤销class、三尺寸、四方向及返回Top、CardSize0、重挂与模板替换通过。GC后绑定恢复及关闭图释放也通过；释放探针必须推进Headless渲染队列，最初只RunJobs/GC的失败保留，不当作已证明的产品泄漏。

最终临时功能/历史/扩展检查11 PASS、0Skip，其中包含两种实际明暗主题、每主题30次目标按钮点击，共60次Headless输入；每次验证实际hit和AddTabRequest仅增加1，12item及选择保持。所有定制经生成专用Style的AXAML，没有找部件写尺寸的回退。

**独立未闭合失败：** 直接从原生`Styles`集合移除仍活动的规则，Width保留91而期望56；该失败与本片Height=NaN的绑定恢复不同。它与CT25已记录的原生样式移除遍历问题一起继续调查，未删除断言、未宣布修复或写成专用Style“不支持”。批准仅覆盖状态规则失活后的恢复及所示布局，不豁免直接Remove。

## 视觉审查

原自然120帧的原始A/A、恢复与5320/5259等失败计数保留，媒体已清理。最终自然候选60帧：20组A/A中4组非零；20组1 DIP全部检出；6组恢复非零。旧新60对中24对是size56、explicit、size0、scroll历史的预期布局变化，兼容目标36对中30对零差异，余下6对reset计入限定审查。

独立直接绘制120帧：候选20组A/A、6组恢复均0，20组1 DIP全部检出，36对兼容目标旧新均0；另外24对保留预期布局变化。明暗各160条A/A与48条恢复的公开资源/几何记录相同。直接绘制不代替自然合成器，也不证明每次历史差异的内部原因。

用户2026-10-27批准所示布局及[N01–N16精确限定差异](tabs-layout-noise-review.md)。该表39–270点、最大通道1–3，位于文字及部分卡片圆角边缘；帧、点数、坐标和SHA限定原对象，原始FAIL保持，无通用容差、遮罩、缩放或产品重绘补丁。

## 当前Mac原生与AOT

实际NSWindow、scale2、真实明暗、12条目及四方向：managed1062、NativeAOT1063检查通过，408条资源记录逐条相同。NativeAOT额外验证动态代码禁用。该专项证明资源、消费者和编译专用Style恢复，不声称原生最终像素或原生raw输入验收。

外部工程规范化为`/private/var`路径后构建；初次路径解析、两个别名编译错误、把Color投射Brush误当同类型的探针失败，以及首次缺少ssl链接目录的失败均保留。修正驱动和使用现有openssl/brotli目录后真实发布运行成功。macOS26 dylib与最低链接12的警告保留，不宣称macOS12兼容。

## 清理及最终输入

- 8份临时源码（Tab6、Nav调查2）、专用诊断目录均删除，常驻测试净增0。
- 302张PNG（300采样、2审查叠加图）及manifest共303文件已清理，媒体审计clean；保留文本计数和SHA，不保留图片、编码图或媒体链接。
- 原生工程423文件、1,106,313,464字节已删除，两次宿主均退出。
- 清理临时源码和原生工程后，精确Tab原283/283、0Skip通过，命令总42.77秒（测试10秒）；未修改的NavMenu原307/307通过，总12.89秒（测试10秒）。这307项不证明首次展开已修复，调查结论见[NavMenu结果](nav-menu-initial-collapse-motion-results.md)。
- Desktop Controls NET8 Release通过，18.79秒、0 warning/0 error。外部媒体清理没有改变编译输入，沿用相同最终产品输入的模块证据。
- 三份完整产品C#（两控件、一Token）成员审计无问题；默认函数、绑定优先级、生命周期、生成注册、正式九文档及Gallery API/示例边界由作者另步自审。按用户要求未使用评审agent，不称独立评审。

聚焦命令：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TabControl.`；NET8为该产品项目`dotnet build -c Release --framework net8.0 --no-restore -m:1`。完整短日志、命令和清理receipt位于`.superpowers/sdd/container-tabs-audit/`。

本机默认依赖及状态规则退出恢复切片已取得批准；直接原生Style移除、HorizontalMargin语义、其余Global/输入/持续动效、其他Host、原生最终显示面及完整CT14/CT25仍开放。NavMenu不正确候选已撤回，未纳入产品改动。Git须另审具体快照。
