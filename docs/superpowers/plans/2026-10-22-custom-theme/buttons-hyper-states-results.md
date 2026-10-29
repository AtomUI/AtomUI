# CT-12：HyperLinkButton icon-only / danger 状态消费

承接已批准的[loading接线](buttons-hyper-loading-results.md)与[动效开关](buttons-continuous-motion-results.md)，保持官方Avalonia12.1.4；无Git提交授权。

## 原始失败及最小修复

六项有效原始用例全部失败：Large/Middle/Small/Custom的`:icononly`均未设置，IsDanger在首次挂载前及挂载后设置均没有`:danger`。已有元数据声明四个伪类，AXAML已有icon-only指标、danger idle/hover/pressed及disabled规则；基类原同步方法只设置visited和前一分包补齐的loading，也没有监听Icon与IsDanger变化。

补上IconProperty/IsDangerProperty状态同步触发，按同包Button既有定义把`Icon != null && Content == null`同步为icon-only，并同步IsDanger。四行新增消费接线，不新增API、Token、默认公式、主题选择器、绑定、订阅或缓存；不把IsGhost改成另一套状态。

候选六项通过：四档OnlyIconSize与IconOnyPadding真实到达root及Frame，内容切换与Icon清除/替换更新资格，loading替代节点读取同尺寸和零margin，显式0有效。真实Headless指针验证danger idle/hover/pressed，disabled优先；配置/清除、兄弟普通链接颜色及主题更新保留焦点通过。两个临时夹具编译问题（Core.Tests私有扩展与Grid命名空间冲突）已纠正，不计产品RED。

## 原始视觉恢复失败及定位

同步等待夹具的第一轮原始结果完整保留：Light退出状态/配置后2654像素非零，部分颜色为中间色，不能按预期差异验收。追加只读观测显示目标/base颜色已正确，当前动画优先级颜色仍是过渡值；真实Duration200ms。相同等待时长下，仅把Thread.Sleep替换为异步Task.Delay让UI事件循环推进，明暗的配置恢复和退出均回到0，当前颜色与目标一致。没有修改产品Transition、加长等待或关闭动效消差。

最终夹具前后均用相同异步历史，每次采样前硬断言当前颜色已到达base目标；保持normal→AA→1DIP→恢复→icon-only/danger→真实hover/pressed/release→配置B→配置恢复→正常状态恢复。为清楚呈现暗色，夹具自身的行标签按scope ColorText绑定；产品被测节点没有额外样式。仅逆向HL2四行取得旧源基线，已批准loading和motion始终保留；结束恢复候选并核对SHA一致。

最终精确像素矩阵、原生/AOT及清理结果待实际完成后追加。原始同步轮、诊断轮、异步验证轮、最终轮的帧和SHA分别保存，不覆盖原始失败计数。

最终临时7PASS（6功能+1完整明暗视觉轨迹），每次采样的当前颜色到达base目标硬断言通过。760×300、scale1；明暗A/A均0，1DIP检错Light7794/Dark7791，两侧恢复0；normal及退出正常状态前后均0，配置往返恢复也均0。原版在启用无激活者的配置B时变化0；候选B真实变化Light2936/Dark2924，清除回0。

| 前→后完整窗口 | Light | Dark |
| --- | ---: | ---: |
| normal / AA / 1DIP恢复 / 正常状态恢复 | 全部0 | 全部0 |
| icon-only+danger idle | 2944 | 2944 |
| hover / pressed / released | 各2944 | 各2944 |
| 配置B（现有尺寸/Padding/Error色入口） | 2976 | 2976 |
| 配置恢复 | 2944 | 2944 |

有意变化为现有icon-only图标尺寸、Padding与零margin规则生效，以及IsDanger=true的颜色规则生效；配置B使用原有字段。最大通道差Light233、Dark210；全部精确计数与SHA已冻结，无通用容差、裁剪或对齐。已查看最终明暗状态、配置及正常恢复全图。真实Headless输入每侧Click=1，配置更新后同实例焦点保留。

## 仍开放

本状态修复的视觉审查、当前Mac NativeAOT、临时源码与媒体清理及最终精确模块验证尚未完成。CT12完整家族、其他平台和既有原生像素差异仍开放。只用主agent，不创建分支/worktree/clone，不发布NuGet。

用户已批准本状态接线与上述精确差异（call_6fca597505d04243ac7024d14bb41f5c:0），不含Git提交、其他平台或完整家族验收。当前Mac实际NSWindow scale2、360×310，managed129PASS：四档真实节点指标与颜色、内容/图标变化、loading替代节点、同实例主题更新/恢复、原生本地值优先级、焦点、实际模板替换及分离重挂。原生输入颜色的hover/pressed证据仍来自Headless真实输入，不把原生属性检查扩张为Native屏幕逐帧验收。

NativeAOT实际Mach-O arm64程序同样129PASS，exit0；真实ILC输入为官方12.1.4/lib/net10.0/Avalonia.Base.dll。没有FW1候选或同版本缓存替换，未发布NuGet；新探针无Assembly.Location裁剪诊断，保留本机OpenSSL/Brotli高于macOS12的五条链接警告，不外推最低OS。成员审计1文件/1控件0问题。

清理完成：两份临时测试源码删除，常驻测试净增0；132PNG+manifest共133文件删除、媒体audit clean；外部项目及managed/AOT产物423文件、1,379,232,346 bytes删除。清理后Buttons/Dropdown/IconLoading精确模块132PASS、0FAIL、0Skip（3秒）；NET8 Release构建0 warning/0 error，18.30秒。成员1文件/1控件0问题，diff检查通过，按仅主agent约束完成作者自审，无独立评审。此状态接线分包完成，完整CT12/CT25不关闭，未提交或调度Actions。
