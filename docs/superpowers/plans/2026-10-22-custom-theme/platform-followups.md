# CT-25 首轮 CI 后续执行包

用户明确批准了241文件快照提交／远端分支／推送及验证。该快照为cfb1d6668，已推送feature/custom-theme；[首轮 Actions](https://github.com/AtomUI/AtomUI/actions/runs/40053351877)完成，三个真实OS NativeAOT与两个Browser模式成功，三个选测任务失败。未调用发布流程。后续修复仍在当前目录；原快照授权不扩大为任意后续提交。

## W1 文本编码

Windows在测试执行前，因系统默认编码误读UTF-8 BOM项目而XML解析失败。本机C locale／禁用Python UTF-8模式复现；同时实际项目、带BOM XML、Unicode源码与失败进程日志四项失败，ASCII正对照通过。四个验证器文件固定UTF-8／可带BOM读取，JSON和日志固定UTF-8写入，保持选择、命令和门禁。五项临时验证通过并删除；清理后同非UTF-8环境的tooling iterate通过68.993秒。新增常驻测试0。

## W2 关闭窗口的上下文

Mac CI与本地既有WindowThemeContextLease测试均复现：关闭后桥已删除，但ContextProperty又变成根上下文。ThemeScope.ResolveContext在空值回调中会回填TopLevel，需验证其在关闭过程重新发布的具体轨迹。精确本地Avalonia12.1.4源码证实PlatformImpl在OnClosed前置空，未显示／隐藏时仍存在；拟在该fallback的存活边界阻止关闭根回填，不按IsVisible判断，不改变预显示准备、隐藏后重开或活动owner的解析。

合同保持：Opened之前同一上下文与正确明暗已就绪；显式背景、owned窗口继承和失败Show保持；Close后桥和Context同时清除，后续查询不复活。优先用既有失败方法及同类五项正反合同验证，再核对Core相关测试、NET8与真实NativeAOT。Native产品夹具在同一次真实窗口Close后追加合同断言，先在原版验证、再验证修复。该断言不增加测试进程或全局用例，独立价值为真实OS／AOT关闭边界，额外代价常数级；不将Headless通过代替该入口。旧range模板保留失败另行保留，不混称已释放。

## W3 NavMenu 连续动效

Mac CI展开阶段未观察到中间宽度，本机两方法聚焦中该方法通过。保留Width300、正常motion、原折叠与展开全轨迹；核查Task.Delay／线程调度、宽度属性通知、实际layout帧和配置来源。当前手工循环使用墙钟与应用Shared时长，owner／scope消费亦需后续核定。先证明CI触发原因，再决定产品或测试时钟修复；禁止延长时长、放松区间或删除连续动效断言来抹去失败。

## W4 Linux DataGrid 交互与几何

Linux模块309项中18失败，Mac CI309项通过，本机六精确类45项通过。原验证器错误尾部只有16KB，首轮有7个失败方法的完整名字不可恢复，不能宣称已有完整逐项清单。需在Linux保持原模块／测试条件补全记录。Docker宿主已可用；只挂载当前checkout及只读Git元数据执行Linux测试，不新建分支、worktree、clone或研发源码副本。构建串行，核对原始触发、屏幕与控件坐标、实际hit、全部祖先bounds／clip／overlay、焦点与pointer capture，再做单变量对照。保留排序、列调整、选择、输入焦点、滚动条、details展开与滚动边界，未证明前不改事件处理器。

全部变更做完、临时验证清理、聚焦校验及作者自审后再提交用户审查；跨平台测试入口只证明各自合同，像素、持续动效、性能及完整家族仍按原任务开放。

## 已取得的后续证据

W2已实现：ResolveContext只在具有PlatformImpl的存活TopLevel回填根上下文，避免OnClosed清空时资源回调将其复活。既有窗口五项、Core三个精确类14项通过。实际OS NativeAOT在原逻辑下以关闭后Context非空失败，候选26断言／1.360秒通过；新增的是既有原生夹具的关闭合同断言，不增加全局测试进程。该次发布17文件／136,664,272字节已删除。独立的Range旧模板保留仍开放。

W3有效反例保持Width300、默认正常motion、先完整折叠再展开；UI持续处理任务时，占满后台worker后原Task.Delay循环没有中间布局帧。普通worker正对照通过，同驱动复验仍为1失败／1通过。候选使用DispatcherTimer在UI推进，时长、16ms间隔、墙钟进度、CubicEaseOut和宽度恢复保持；CTS仍是唯一取消身份，取消时停止timer。运行时测量／中断轨迹不能由ControlTheme等价计算，代码已有具体说明。现有Headless等待helper增加中性UI心跳，因RunJobs在空队列不会推进dispatcher timer；未延长时长或放宽宽度判据。310项聚焦通过，另三项临时验证覆盖繁忙／普通worker、改向连续性、停用、卸载500ms无残留写入及GC释放。首轮CI没有线程池遥测，控制反例证明同症状的产品缺陷；是否消除该CI失败仍由复跑判断。

NavMenu另先完成独立布局整理：152成员token及文件头相同，31项成员组织问题降为0；此步309项既有测试通过，原线程池反例仍失败。不是用布局变更消除缺陷。行为修改单独保存证据，提交时与布局整理分开审查。

W3真实Skia使用正常motion与同一完整操作历史；最初只指定RequestedThemeVariant的驱动被Window根上下文覆盖，不能算暗色证据，已改为公开IThemeManager应用配置并断言实际ThemeVariant后重跑双方。最终明暗18组旧新终态、12组独立A/A、四组同实例返回均0 RGBA差异；12组1 DIP检错分别142／987／988像素。已查看明暗正常、折叠与中间帧，文字／标题与折叠首字母位置一致。实际原生AOT线程池繁忙专项继续，未以Skia代替最终显示面或完整家族。

W4 Docker Linux arm64／SDK10.0.402复现原模块291通过／18失败，完整TRX已补齐首轮丢失的七项方法，未更换测试隔离模型。四项只读几何探针保留原窗口／输入尺寸与点击路线：目标位置稳定、原绘图stub下实际命中根Panel；仅换为真实Skia后四项均通过。精确Avalonia12.1.4源码的HeadlessStreamingGeometryStub只检查相邻三顶点构成的三角形，圆角多边形中心不被包含；Linux模板具有该StreamGeometry裁剪，Mac非CSD路径没有。DataGrid测试宿主因此改用真实Skia，不删除裁剪、不改产品事件处理器。

真实Skia后原模块300通过／9失败。其余失败为三组DataGrid组件测试中Window名称解析为AtomUI窗口引入平台chrome：Linux 520×280窗口内容区448×168、固定480×240表格原点(20,31)，排序点(279.5,56)在内容Clip起点y67外，实际命中标题栏Frame；420×260窗口内容仅348×148，RowsPresenter高97只能实现两行。此为测试对可用内容区的错误假设，不是排序／裁剪产品缺陷。三组组件测试显式用AvaloniaWindow；窗口最小模板在编译AXAML内声明PART_VisualLayerManager及内容绑定，让TopLevel启用实际PopupOverlayLayer，不禁用排序Tooltip。测试宿主不增加测试用例／进程，独立用途是让现有组件断言使用一致的可用内容区及真实弹层能力。不改表格宽高、数据、输入动作、排序断言和产品窗口。其余产品Window宿主与原窗口四项输入探针保留；不能将这项测试宿主校正描述成修复了产品Window输入bug。

## 清理后的检查与自审

真实Mac NativeAOT同一临时驱动得到旧版88次采样／0中间宽度而失败，新版88次采样／36中间宽度、最终300而通过，时长均200ms。新版27项产品断言及PRODUCT_PASS selected完成，3.142秒；未声称取到了最终屏幕像素，五项既有Homebrew链接警告保持。临时驱动已从共享入口撤回，既有关闭上下文断言继续保留。

13项临时验证（3个C#文件）、原生NavMenu发布、源备份／插桩helper共32文件／139,675,691字节已清理；73个登记媒体／trace及清理元数据文件已删除，媒体审计无发现。此前Windows五项临时验证已独立清理。净新增常驻测试用例0；既有CT25RangeLifetime失败仍保留，不包含在提交中。

清理后Linux SDK10.0.402／arm64：DataGrid309通过43.788秒，NavMenuInlineCollapsed及窗口合同23通过6.578秒；Mac SDK10.0.300：NavMenu与窗口313通过15.766秒、DataGrid309通过30.782秒、Core三个精确类14通过、标题栏Skia渲染1通过。NET8 Release零警告／错误。非UTF-8 locale且禁用Python UTF-8模式的四路径tooling iterate通过47.941秒。10个C#文件全文件成员／控制流审计（两控件式声明）问题0。当前Docker容器及匿名产物卷均已删除，没有研发源码副本。

作者自审按时钟、取消／回收、关闭重入、编码边界、测试宿主的可用内容区／弹层及文档逐项检查。已同步NavMenu实现与changelog，无公共API／Token／Gallery演示变化。默认原生显示面、其他CT25旧失败及完整控件家族仍开放。后续修复尚未提交，首轮CI结论仍为失败；需用户审查本批快照后再推送已有分支并复跑验证，不创建新分支或worktree，不发布NuGet。

## 批准后的执行状态

以上为提交前审查记录。用户随后明确批准该20文件快照的两个提交、推送与验证；已创建938f6e3c9（成员布局）和1fe3358ec（行为及验证宿主），聚合patch与批准快照一致，并推送既有分支。[第二轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40061147545)五个运行时任务通过，Mac原两个失败与Linux原DataGrid失败均消失；三个选测任务在后续位置失败，继续按[第二轮后续包](platform-second-followups.md)处理。该次提交批准已执行完毕，后续提交需另行审查。
