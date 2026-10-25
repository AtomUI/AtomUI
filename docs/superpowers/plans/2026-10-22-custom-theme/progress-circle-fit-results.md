# CT17 P1-B3 圆形信息适配与状态恢复

## 合同与候选

Circle／Dashboard按当前Percentage和ProgressTextFormat测量信息，保留实际96尺寸及原轨道／裁剪；可以放入内圆的短串显示，放不下的长串或图标隐藏。字体变化及时重算，异常期间Value更新后切回Normal应显示当前百分比。来源为ProgressBar overview的信息替代和内圆适配合同，以及前批B1／B2的尺寸／状态邻接风险。

候选改三文件：AbstractCircleProgress使用实际Percentage及FontStyle／FontWeight，字体变化刷新；Status变化刷新当前标签内容。只检查当前激活的状态图标，CircleProgressIndicatorPanel在测量前读取模板／专用Style实际宽高更新fit。ControlTheme在原状态分支声明式消费IsStatusIconVisible；没有代码样式回退、新公共API或Token字段，保留原状态互斥和disabled规则。2项C#整文件成员审计0问题。

## RED／GREEN

实际明暗算法、Appearance与ActualThemeVariant合格的临时验证：当前短1%宽57.8125小于内圆88.6却被旧固定100%测量隐藏；高精度12.3456789%宽125.9162却显示；Own图标200的fit为false仍显示。扩展Exception50／100、Success50／100、Normal100、Active100，检查Own24同实例恢复。

新增异常25后回Normal仍显示旧50%的反例；动态字体用Arial的099% iii，实测Normal／Bold宽度跨越内圆边界后再断言。最初数字／WWW两字重advance相同的4项只是驱动资格失败，不计产品RED。模板逗号组合selector使TemplateBinding无法推断scope的AVLN3000也单独记录，拆独立selector后消除。

最终同一组44项在旧版全部失败，56.348s；候选44项全部通过，另合并一项CI Masonry精确反例后45PASS、10.778s（已构建）。Masonry在Mac未复现不表示Linux已修复。4项编译专用CircleProgressIndicatorStyle／DashboardProgressIndicatorStyle验证Exception200／Success24、active不受inactive影响、Style优先Own及退出／清除恢复；实际Tag命中、专用类及关键Setter形态和无Name／Loaded／Unloaded代码回退均检查。

旧新只切换三份对应源码，采用当前字节备份并finally恢复，source hash校验一致。现有DataGrid G1及其他工作不参与版本切换。

## 真实 Skia 对照

产品Window720×360、scale1，六个正常自动尺寸owner及两个96定制Circle／Dashboard。公开ThemeManager同时设置实际Application／Window与局部Own的明暗；正常motion保留，采样至少500ms或最长声明Transition、连续3帧一致、Value／画刷动画结束，8s上限。main／AA／1DIP为独立实例并有相同完整操作历史，扰动仅根Margin左+1右-1。

首轮图片已捕获但JSON不能保存模板默认NaN尺寸，导致两项取证中断；改为文本记录尺寸，并保证日志失败仍关闭窗口／恢复主题。该轮不是产品RED，23文件在查看关键图后清理、audit clean。合格候选2项86.852s，旧版2项71.742s（同一构建无重新编译），共132个compositor帧。两版合计44组AA全部0、44组1DIP全部检出（最小9,071像素）；默认→reset两版共12组为0，候选字体恢复6组为0。

旧新版完整66对中，默认、图标恢复、完成成功、普通字重、最终重置共30对为0。其余实际变化见表；相同变化在main／AA／DIP均可重复。无缩放、裁剪、遮罩、配准或通用容差。

| 输入／阶段 | 明色全图变化像素 | 暗色全图变化像素 | 结果 |
| --- | ---: | ---: | --- |
| Value1／Font40 | 1,376 | 1,364 | 显示能放入的1% |
| 高精度12.3456789% | 1,666 | 1,646 | 隐藏越界标签，轨道保留 |
| Exception50／Own最小图标200 | 8,009 | 8,011 | 隐藏越界状态图标，96 owner与轨道保留 |
| Exception25后Normal | 1,156 | 1,149 | 旧100%刷新为当前25%（前序完整历史曾到Success100） |
| Arial／099% iii／Bold | 2,234 | 2,230 | 实际粗体宽度超过内圆时隐藏 |
| Bold回Normal后的附带圆弧重绘 | 117 | 126 | 候选回到自身普通字重画面0；旧compositor自身恢复非0，定位见下 |

已逐对查看旧新短串、暗色长串、超大异常、状态恢复和粗体关键图；显示／隐藏及轨道行为与合同一致，窗口、owner尺寸和布局没有以去掉约束规避。默认、最终重置与恢复图已查看。用户随后明确回复“允许圆形适配与状态恢复修复（推荐）”，批准本表所示预期变化及已定位的附带圆弧重绘；仅限本包视觉，不包含Git提交、原生最终像素或完整家族验收。

## 附带圆弧像素的定位

严格比较最初178项中有12项失败记录：旧版自身字体往返6项及旧新版恢复6项，均为117／126像素，未删除、覆盖基线或改为容差通过。差异在两只定制圆形的弧边区域，整图bbox(25,244)–(234,268)，不在文字度量或状态内容差异区域。

增加只读诊断，在相同完整历史下同时采compositor与RenderTargetBitmap直接Skia重绘：两版本各2项通过，合计88帧。新增直接采样前后，原compositor全部44帧精确相同，证明诊断没有改变原轨迹或消除原失败；四种版本／主题的字体前后模型（布局、DesiredSize、位置、矩阵、路径、画刷、线宽和内容）全部相同。

直接Skia中，旧版字体恢复、候选字体恢复及旧新版恢复在两主题均为0；旧compositor仍117／126，候选compositor为0。现象因而被限定在旧版compositor增量重绘路径；信息可见性变化改变重绘区域后，候选回到普通字体基线。该证据不是框架增量裁剪实现的修复声明，也不扩用此前7像素许可。本包保留全部真实计数，所示圆弧附带变化随本包已获明确视觉批准；比较器继续保留真实计数，不加容差。

## 原生／AOT与当前边界

Mac实际Native／默认Metal窗口720×440、scale2，managed程序130项43.122s通过；构建29.042s、0warnings／0errors。覆盖两主题、六种结果状态、Own200／24、专用编译Style的active／inactive与优先级、异常退出、真实字体边界及同实例恢复。NativeAOT发布69.721s成功，实际Mach-O arm64运行同130项42.901s通过，SHA256为176cbd8da1bf8e00b312b8d7406ad68ab5b15ffeb08ed698b7e7f03503605c26。发布保留5项本机Homebrew dylib最低系统版本链接警告，不证明macOS12兼容。原生最终像素、其他OS和完整Progress家族仍开放。

## 批准、清理与最终检查

用户明确允许本表信息适配与状态恢复修复，范围包含所示附带圆弧重绘；不是Git提交、原生最终像素或完整家族批准。正式implementation与changelog已同步；未新增公共API／Token，不改Gallery示例合同。

合格主录制157文件及直接Skia诊断93文件已按marker清理，媒体audit无发现；首轮无效JSON的23文件此前已清理。删除4临时源码／AXAML及8驱动／旧源码备份，14份TRX压缩为计数、有效失败与SHA256文本后删除。专用原生工程整体删除431文件、1,090,566,112字节，保留源码与实际二进制的文本指纹，净新增常驻case0。另案Range生命周期失败反例保留，不混入本包。

最终清理树ProgressBarSemanticPartTests／ProgressBarBehaviorTests精确42项通过，50.549s；NET8 Release构建24.111s、0警告／0错误，两C#整文件成员审计0问题，diff检查通过。自审未发现当前分包阻断项；这是P1-B3切片出口，完整CT17及原生最终像素矩阵继续。未取得本包Git提交授权，不提交／推送。
