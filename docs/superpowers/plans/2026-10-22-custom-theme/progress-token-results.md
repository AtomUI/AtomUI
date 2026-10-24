# CT-17 P1-A：Progress Token 接线调查结果

状态：研发中，尚未验收或申请本包Git提交。此包与待审查的34文件快照分离，后者逐文件哈希持续保持原值。CT-24继续暂缓。

## 实现与字段兼容

ProgressBarToken改用DependencyDefaultsV2／revision1，保留既有13字段、类型、slot和resource key。两主题实际运行快照逐字段比较，26项名称／类型／slot／key／格式化值全部与旧版相同，没有增加Own身份或资源编号。三个未启用步骤／动效槽位保留原0默认，不新增其运行语义，完整处置仍归后续分包。

四个公开owner保持各自identity，Own继续使用既有ProgressBar家族scope；本包不另造Circle／Dashboard／Steps的Own字典。CircleTextColor明确配置时投射到Circle／Dashboard的Foreground，使用既有三值converter显式选择原ColorTextLabel回退；disabled和调用方本地Foreground仍由原生优先级执行。普通line在Round分支用明确Own LineBorderRadius，否则由小型模板converter保留原1000胶囊值，未把schema默认100直接灌入旧模板。rail／track／success都通过AXAML应用，专用生成Style仍可覆盖，无Part属性代码回退。

## 功能反例

在当前真实Skia/Headless宿主、原360宽line、96×96 Circle／Dashboard、同一实例上，首轮6项全部有效失败：两主题Own文字／圆角未接线、专用Style退出后不能返回Own、evaluator仍V1。修改后6项通过；追加实际明暗、13项明确值（含0及负Margin）、异常／完成与恢复后，8项功能加2项视觉驱动全部通过。初次补异常状态验证漏了临时文件using，编译失败已修正，不作为产品红例。

功能检查覆盖：Own配置／重置、本地Foreground／ClearValue、disabled往返、Exception图标／completed图标、Round／Flat、专用Rail／Track Style退出／重入、13字段显式值及原类型保持。其余8个有视觉语义字段的动态失效不冒充已通过；Circle的最小字号／图标只随尺寸更新、普通／步骤间距失效等继续P1-B。

## 原始宿主视觉检查

两主题，720×520窗口、640宽StackPanel、原四形态及24间距，正常motion；每版独立main／A/A／1 DIP实例，同实例默认→两项Own→重置，捕获前确认实际scope、值、非动画及有效布局，再等待实际compositor commit。36帧完整不透明RGBA，无mask、对齐或容差。

默认旧新明暗均0；Own配置旧新明／暗952／974像素，范围(40,25)至(364,277)。原始宿主重置后候选有168／190像素变化，范围(40,183)至(136,277)，而旧版自身重置为0。12组独立A/A均0，12组1 DIP全部检出3,104至3,177像素；默认／重置的只读几何完全相同。实际查看旧／新明色Own和重置差异图，预期文字紫色及圆角之外，存在圆环边缘重绘差异，不能按预期配置变化直接放行。

## 脏矩形裁剪单变量诊断

同候选54帧对照：compositor重置仍168／190，直接Skia RenderTargetBitmap重绘为0，仅令根背景重绘后的compositor也为0；独立A/A全部0。数据、Shape.Data及Bounds、Stroke／厚度／cap、节点几何、变换、opacity和clip记录都相同。根重绘只作诊断，没有写入产品或正式验收驱动。

按本地优先顺序命中 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`，本地12.1.3／8eeda4f6f5；实际包是12.1.4，其公开XML也声明CompositionOptions。参考源码显示SingleDirtyRectTracker合并并扩大矩形后裁剪绘制，region模式保留分区；只作实现参考，不把本地版本当实际二进制源码。

单独UseSaveLayerRootClip=true未改善168／190；UseRegionDirtyRectClipping=true后同历史重置明暗均0、6组A/A为0、6组1 DIP检错通过。region+SaveLayer亦为0。临时反射只读确认实际ServerCompositor拿到所设options；没有加入产品反射。每次只改变宿主选项，未改原尺寸、内容、输入、motion或控件代码。所有正式宿主源码在finally恢复，不预先把渲染配置改为产品默认。当前证据将差异定位到此宿主脏矩形路径，不声称已经确定Skia内部具体Bug行。

此前P0七像素的原七控件／字体／范围完整历史正在按默认与region两个配置复核；在取到有效结果前，其单独待审查项仍开放。P1还需以确定的宿主配置重做旧新完整对照，审查预期Own变化、确认测试宿主与生产配置边界、清理临时源／媒体和最终聚焦出口。整个CT-17及原生最终显示面仍未验收。

## 确定分区宿主后的完整重录

测试宿主正式加入UseRegionDirtyRectClipping=true，范围仅Desktop测试App，生产Native／Metal配置不变。父容器禁用补查曾得到2失败／6通过：Own颜色的IsEnabled=True选择器仍会在祖先禁用时命中；改用:not(:disabled)后8项通过，调用方本地前景优先级保留。

在该确定宿主下，从两个版本重新跑完四形态完整历史，旧版2项捕获／候选8项功能＋2项捕获通过，36帧均不透明。默认及重置明暗旧新均0，同版本默认→重置四组均0；12组A/A均0，12组1 DIP全部检出3,112至3,177像素。Own输入CircleTextColor=Purple、LineBorderRadius=3时，两主题旧新各784像素，范围(40,25)至(364,275)，仅两处圆形百分比和三处line圆角；实际查看候选明暗及明色差异图，圆环边缘无额外重排或残留。完整几何／Shape记录默认与重置逐项相同，比较没有容差或遮罩。

七控件复核最初另两实例配置与原记录不同（异常line应为Flat、纵向inner应为Center），已纠正后重新跑两版宿主。原七控件公共数据、字体、Bounds／DesiredSize、全部命名部件矩形、圆角和到窗口matrix与原记录逐项零差异。默认宿主42对A/A中6对各7像素，同原x42–46／y33–35、最大单通道16；只改region选项后42对全部0，6组1 DIP全部检出2,652至4,722像素。原条件未被删减；两主题保留正常800ms Value transition及200ms提交余量和实际终值／布局检查。此前七像素容差申请无需继续批准，不在比较器中放行任何像素。原始single-rectangle环境仍记录其失败，不能说生产后端已经修复。

第二次独立只读审查覆盖五份P1源文件，未发现新增Critical／Important问题；认为该更改可作为限定测试宿主修正，同时要求保持Host qualification，不声称Native默认像素等价。13字段默认／显式值、条件回退、祖先disabled、本地值及专用Style均已审查。后续仍需原生Own行为、预期配置视觉审查、临时产物清理和最终精确验证；P1-A不提前完成。

## 真实 Mac Own 行为与 NativeAOT

独立系统临时工程直接引用当前源码并采用生产UseAtomUIPlatformDetect／WithAtomUIDefaultOptions，未开启测试宿主region选项。实际macOS窗口720×440、RenderScaling=2，managed执行218检查、4.355秒；发布所得Mach-O arm64 NativeAOT执行212检查、4.143秒，均exit0。覆盖明暗同scope／原四公开owner的配置与重置、CircleTextColor与LineBorderRadius实际值、Steps隔离、本地Foreground／ClearValue、父容器disabled往返、Round／Flat及编译专用Track Style退出重入；不同检查数来自有界稳定循环次数，功能合同相同。此证据证明真实原生宿主和AOT中的值／布局／优先级行为，不替代原生最终显示面的像素验收。

managed构建0警告／0错误；NativeAOT发布成功，保留5条本机Homebrew dylib最低macOS版本链接警告。专用工程已校验owner marker后整体删除，430文件、1,089,286,078字节；没有引入新分支、worktree或源码副本。9组原始／诊断／七控件媒体373文件已按marker清理，每组Git媒体审计均0发现；本次新旧Own候选画面仍临时保留待用户审查，未存入仓库。剩余P0新旧完整重录和清理后聚焦验证继续。

## P0 七控件的分区宿主补录

在相同确定宿主及正常motion下，只切换AbstractGeneralProgressBar旧源码847c441c1／待审候选，先备份当前字节并在finally恢复，34文件快照哈希保持不变。首次36帧完整字体／范围／恢复：12组A/A全部0、12组1 DIP全部检出2,651至4,722像素。再增加初始帧与直接Skia重绘作诊断，52帧中16组A/A全部0、16组1 DIP全部非0；两次录制共同阶段数值一致。

默认旧新差异仍为已批准的695／696像素；17.3/Bold/Italic为41,709／41,798，长格式为43,546／43,698，恢复合成帧为707／709。直接重绘的新旧恢复仅695／696，说明额外12／13来自旧版恢复合成路径：逐像素差值核对候选相同位置全部为0，旧版最大通道差55／43，并非全部RGB1。较早只凭部分坐标把恢复差异统称RGB1的结论不完整，现以本次全量坐标记录修正；没有据此加入像素容差。两版另共有72个状态图标的合成／直接绘制差异，位置626–639／86–99、逐像素差值完全相同；不同绘制Host不强行判作零差异，也不拿直接重绘替换正式合成截图。

本轨迹保持SuccessThreshold=30且范围改为-100至900，成功段由30%变成13%，所以候选初始→恢复另有780个成功段像素是输入状态不同的预期；它不构成相同状态恢复失败，也不能当作零差异恢复证据。旧版同项是792／793，恰多上述12／13个历史像素。此补录说明已批准布局变化之外的差异来源，原生最终显示面和全家族兼容审查仍开放。已实际查看长格式候选、恢复明暗及差异图；不把这份诊断转换为完整Progress家族验收。

## P1-A 清理后验证

删除本包5个临时源／编译AXAML，净新增常驻case0；独立原生工程已整体清理。仅保留另案Range生命周期的既有失败反例。最终精确11类（Progress行为／Semantic、TypedPackageEntry及测试宿主所涉第三轮10类并集）204通过、0失败、25.521秒。新增Token／模板converter／测试App三文件成员布局审计0问题，git diff --check通过。NET8最终构建仍在运行；视觉批准和正式文档同步未提前完成，当前不申请本包Git提交。

最终NET8 Desktop Release构建exit0，18.841秒，0警告／0错误。当前候选V2终端数18（新增Progress一终端13历史字段），历史迁移字段231、加前序Dropdown23和OTP2后的声明256；规范目录的34文件审查快照未改字节，待其提交授权解决后再同步计数与正式implementation／changelog。正式Progress token.md已说明两项明确Own的回退、适用形态及V2边界；视觉审批仍待用户，不将本包标为完整家族完成。

## P1-A 审查结论

用户明确允许CircleTextColor=Purple／LineBorderRadius=3的Own接线视觉修复。所示明暗各784像素、默认及重置0、12组A/A为0与12组1 DIP检错结论纳入本分包；临时候选38个PNG及marker按审计清理，无媒体进入仓库。P1-A的13字段V2及两项接线分包完成，净新增常驻case0；没有授予本包Git提交权限，也没有把当前研发中的P1-B或整个Progress家族标为完成。

同条回复批准此前34文件快照的两个提交、推送和Actions，已分别形成fc9896fb6／e9bc9e707；P1-A源与测试App不包含在内。第四轮CI为[40089581608](https://github.com/AtomUI/AtomUI/actions/runs/40089581608)，仍以其提交内容为验证对象。用户另接受历史7像素限定差异；现有确定分区测试宿主42对A/A已为0，不在比较器中消费该容差。
