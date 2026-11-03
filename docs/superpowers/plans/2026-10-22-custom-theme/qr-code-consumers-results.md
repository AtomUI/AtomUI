# CT15 QRCode 执行结果（2026-11-01）

原目录feature/custom-theme单agent执行，未创建分支/worktree、未提交、未全量；此前未提交改动、限定批准与CT12/13/14、CT25/FW1边界保持。

输入2metadata/2Own；原代码在替换生成Bitmap时已Dispose旧实例，未把既有释放路径误报为缺失，也不重做Skia二维码算法。原quietZoneSize=0、Size160、EccLevel.M及图标中心透明挖空保留。四算法8默认/type/key/slot已冻结，普通Mask #f4ffffff、Text #e0000000；默认颜色alpha是实际合同，不转换为完全不透明默认。

首轮19所选（11临时+8既有）13PASS/6FAIL。五项有效产品RED：局部Global Family/Size/Border/Padding四项未进入实际Root，清空Value后Bitmap仍旧。另一个Own探针错误地比较alpha原值，将默认224→指定255的预期颜色变化计为模块形状变化；该失败不计产品RED，已改为仅比较编码模块非透明占位（单独语义探针），原源码独立重跑该Own用例PASS。实际UI视觉验收仍完整RGBA，无裁剪/掩码/容差，不将此编码占位比较用于视觉通过。

官方外部取证目录已登记四张原始160×160透明Bitmap。现有本地OpenCV解码探针在内存以alpha占位规范为黑白、最近邻4倍与32px白边，仅服务原quietZone0编码语义验证，不改源Bitmap/实际UI。四算法都精确解出原含中文Payload：`https://atomui.test/ct15?message=你好&n=12345`。OpenCV四次ECI提示“not supported properly”原样保留；本样本逐字匹配不外推为全部ECI/Unicode/纠错或真实OS摄像头通过。

当前只完成RED与基线，成员审查、V2/真实Global、状态/模板/原生内容/刷新/资源退休、正常Skia旧新/AOT、临时清理与最终模块/NET8尚待；完整CT15不关闭。

成员6源/1含注册属性类型1问题→0，只将EccLevel/Size的public wrapper按原注册序列归位；纯整理token指纹、注册与全部static/instance字段序列精确同。V2R1保持两字段原ColorText及alpha244的ColorBgContainer RGB公式，Root复用真实InheritanceParent可选字体，原border/radius/padding转Owner；Value空时清空并释放旧Bitmap。首编译局部oldBitmap同名CS0136已更名，不计产品RED。

更正模块占位探针后原Own用例独立PASS（2s）；实际颜色alpha变化不被误判为编码几何。首修19所选18PASS/1FAIL，四算法8默认精确同；剩余border探针在Disabled算法下只输入LineWidth3却期待BorderThickness3，输入不成立，已更正为实际BorderThickness3。持锁只回放原QRCodeTheme、finally恢复候选后，Family/Size/Border/Padding及新增Caption五用例全FAIL（3s），确立正确Global输入的有效RED；不将前一探针错误当产品缺陷。

增强24所选22PASS/2FAIL（16临时+8既有，12s）：Global与默认caption的实际字体、原生Courier17/Orange、两个Own、刷新旧模板订阅退休及空Value均通过。剩余为detach后仍持有生成Bitmap、原生Expired输入重模板检查。后者已拆开身份/后代/focus/键盘断言定位，未先把一行Shouldly表达式误报为内容值丢失。正在沿生命周期退休Bitmap和刷新按钮订阅，保持原编码/quietZone/图标挖空与旧bitmap替换Dispose机制；焦点实际阶段另证。

生命周期首修的精确两例1PASS/1FAIL（3s）：detach后清空/释放生成Bitmap，离树Value更新不分配bitmap，重新挂载以当前输入重建通过；剩余明确是focus after template为null，内容身份/后代已通过。随后复用已验证的weak focus/generation/同TopLevel/后代/current focus空守卫，detach取消；未修改调用方ExpiredContent。24所选全PASS（13s），8默认精确保持。刷新按钮只在挂载时订阅，模板替换/卸载解绑，重挂重新接一次；生成Bitmap与编码流都在明确owner/作用域释放，不释放调用方Icon。

正在扩充唯一专用QRCodeCoverStyle命中/clear和静态形态、四纠错级别、原生Size1/67/511、三原生状态内容、后来用户focus优先、卸载幽灵刷新、真实Space键及旧Bitmap/调用方Icon所有权。首增强编译漏Headless键事件的必需RawInputModifiers参数已补None，不计产品RED；这些新增用例尚未计为通过。正常Skia/AOT、清理与最终验收仍待。

按仓库现有用法补齐Space键的RawInputModifiers/PhysicalKey/null完整签名后，40所选全PASS（32临时+8既有，30s），前两次编译参数不全不计产品失败。旧生成Bitmap在颜色替换、Value清空和卸载后三条路径均不可再使用，调用方Icon在卸载/窗口关闭后仍可保存；实际键盘刷新一次、旧模板/卸载button无幽灵订阅/重挂一次、原生内容与后来用户焦点、专用Style/clear及原生1/67/511全部通过。

补充透明Own两例与跨scope热迁移一例，连同仅为导出媒体所需的4默认/4纠错，精确11例PASS（11s）。总不同用例43（35临时+8既有），不冒称一次整批43。透明颜色按明确alpha0使用并clear；同一owner跨左右scope字体/颜色切换正确，旧scope后续更新不污染当前，payload与模块占位保持。

候选四默认原始Bitmap全部RGBA通道与旧基线精确同，四默认及四Ecc的8图都逐字解出原含中文Payload；ECI提示仍保留，不推导全部解码器或真实摄像头支持。编码探针的黑白/白边仅在内存，UI RGBA验收不做这些处理。最终成员6源/1含注册属性类型0问题，冻结7文件、实际4文件diff；驱动Release构建0警告/0错误（22.18s），正在正常Skia Light完整轨迹，其他门槛待。

候选正常Skia四算法各60帧/34检查全PASS，共240帧/136检查。每算法44静态（40主状态+4独立cal）与16持续态（6原生输入、3Space按钮、7默认Loading Spin）分账，默认Spin未停止取静态。主轨迹保留原Value/160边框、Own/透明/Global/Native/专用Cover及clear、真实彩色Icon、67/511、四状态/纠错/空Value/RTL/模板/显隐/重挂/双scope迁移。

Light默认、GlobalExpired、IconOwn与空Value全图已查看；原160槽不加宽，Global23过期原文沿原槽裁剪，刷新保持原按钮角色。明确Global #102030遮罩/#123456文字在亮暗均低对比；原Cover为方角，Root圆角17不自动改其矩形拓扑，暗覆层上角方角已列待审，不能以裁剪/改色掩盖。原生511仍沿原720×520窗口裁剪，完整Bitmap另证，不据局部图宣称所有极值可扫描。

候选60主恢复中56零/4非零：L/C的parts-clear与icon-clear各1像素/max2；D/DC全部零。独立cal与默认、持续态和旧新/AOT继续分账；这四恢复不转通过、不放宽容差、不直接归因FW1。正在旧源码完整回放，其他最终门槛待。

原第一批旧新各240帧/136检查，旧版各11FAIL，共44FAIL、无缺帧；原RGBA240对174零/66非零，176静态只有14非零（Global/GlobalExpired/NativeGlobal各4、ValueEmpty仅L/C两对）。这些数值保留为无效背景批次，不作为最终视觉结论。

反查暗色ValueEmpty为何零时，原图确认主场景Grid绑定背景在ThemeConfigProvider外：D/DC白码白底，连默认码也不可见。主轨迹这80暗色帧不满足正确视觉条件，完整历史受该驱动版本影响，原旧新480帧全部留账不作为最终验收；不能以暗码不可见得出空Value无差异。独立cal/输入窗口和原始Bitmap解码探针位于正确scope，但不拿其通过替换原主轨迹。正在发布的旧背景驱动AOT不运行、不采样、不接受为证据。

修正仅在主host外包同算法ThemeConfigProvider，使背景与组件属于同一scope；不改产品7冻结源、Root.Size160、内容/字体/裁剪、官方依赖或像素容差。等当前独占发布结束后再改驱动，使用fresh进程/完整旧新/AOT历史的第二批重采，不拼接替换缺失或错误帧。此前43不同功能用例、8默认、12编码图和6-1-0成员证据不受错误主背景影响；四1像素恢复也仍留账，待有效批复核。

第一发布正常结束后才修驱动，保留其输出publish目录为无效背景二进制，未运行或采样。修正驱动零警告构建（18.49s），主host外层同算法scope与明确背景断言已加入；candidate2/D fresh完整60帧/35检查全PASS。默认暗色完整原图已查看，白码在#141414背景清晰可见；不再使用第一批的白码白底默认作为通过。正在candidate2剩余三算法，随后old2与确切publish-final二进制，不修改产品源或拼接两批。

有效candidate2四算法均60帧/35检查PASS，共240帧/140检查，176静态与64持续态分账。背景断言全部通过；60主恢复52零/8非零：四算法parts-clear/icon-clear两状态，L/C各1/max2，D/DC各12/max1。暗色正确背景显出原先被白底掩盖的12像素，不能把第一批暗恢复0沿用为通过；全部8对保持未通过，等old2/AOT2同历史比较。正式文档7篇相对链接检查无缺失；此检查不替代视觉/清理门槛。


有效old2各60帧/35检查11FAIL，共240帧/140检查96PASS44FAIL，无缺帧，背景断言均通过。失败名为global-font/global-geometry/global-caption/value-empty/detached-bitmap-retired/current-scope/input-focus-retained/input-keyboard/input-content/detached-refresh-retired/current-refresh-once，保留而不以candidate替补。有效旧新240对178零/62非零，176静态160零/16明确变化，其余46持续态非零分账；四default均0。两版各60主恢复同样52零/8微差，独立A/A/restore各4零，1DIP5419/5419/5531/5531可检出（max224/199/224/199）。正确暗背景空Value旧码可见、候选空框的全图已查看，四算法这一修复均进入限定16对。publish-final仍在发布，未提前宣告跨运行/清理通过。

修正后的publish-final实际二进制46625808字节，SHA256 30eb8a268d6f3b17673636b3b327b3d8d149941b600cf287ab4dc211d162869f。官方Avalonia12.1.4未改；此发布0 IL/CS/注册诊断，只有5条既有Homebrew/macOS26 dylib与target12链接警告。aot2按该路径运行四fresh进程完整历史，原publish错误驱动从未运行。有效四组contact sheet和关键暗色空Value全图已查看；AOT数值与清理门槛仍待，不提前补通过。


有效最终aot2四算法各60帧/35检查PASS，共240帧/140检查，背景断言全部通过；176静态cross全0，表内16对也全0。完整240对199零/41持续态非零，64持续态23零/41非零，不停止Spin/caret/按钮或放宽容差。三版各60主恢复52零/8相同微差：L/C两清除各1/max2，D/DC两清除各12/max1；独立A/A/restore各4全0，1DIP全检出。三版176静态最短760/758/758ms，至少12次raw连续相同。

保留cross持续态 L：input-cycle 324/max32；input-keyboard 293/max1；input-template 285/max1；key-down 320/max1；key-up 234/max1；live-125 114/max24；live-425 113/max36；live-initial 107/max19；live-reattach 82/max17；live-shown 27/max14；live-template 102/max16。

保留cross持续态 D：input-cycle 315/max5；input-keyboard 323/max23；input-template 306/max2；live-125 91/max27；live-425 154/max53；live-initial 332/max26；live-reattach 112/max19；live-shown 129/max42；live-template 118/max18。

保留cross持续态 C：input-cycle 304/max4；input-keyboard 316/max19；key-down 350/max3；key-up 303/max1；live-125 115/max60；live-425 149/max146；live-initial 337/max14；live-reattach 86/max16；live-shown 89/max23；live-template 93/max22。

保留cross持续态 DC：input-cycle 316/max18；input-keyboard 316/max26；input-template 281/max1；key-down 340/max3；key-up 235/max1；live-125 86/max20；live-425 145/max107；live-initial 435/max32；live-reattach 96/max23；live-shown 6/max4；live-template 101/max24。

中途一次只读对比碰到aot2/DC正在写入的cal-dip PNG而失败，未接受该对比输出；待sampler完整结束后重读全部240对并断言数量，没有缺帧、拼图或损坏文件。AOT正确暗默认/空Value、Compact可见输入和Global过期完整原图已查看。七冻结源逐字保持；官方evidence audit clean。第一批480错误背景帧、有效三版720帧、12编码图、8contact sheets与manifest共1221取证待清理，随后清理后精确8项/NET8，不预记门槛通过。


判断后清理完成：35临时用例、3源24025字节删除；官方audit clean后cleanup删除1221取证（480错误背景帧+720有效三版帧+12编码图+8contact sheets+manifest）。driver415/错误publish15/有效publish-final15文件及整个原会话566文件/1600272817字节删除，原marker清理。默认零新增永久测试；最终8项模块/NET8在独立小会话完成后同样删除。


清理后最终验收：共享锁独占下精确QRCodeSemanticPartTests（Debug/net10）8PASS/0FAIL/0Skip（706ms，命令29.01s）；NET8 Desktop Release 0警告/0错误（19.04s，命令19.31s）。35临时3源24025字节、1221取证、566原会话已删除，独立最终会话4文件与marker记录后删除；零新增永久测试。正式五文档、CSV2metadata/2Own、四总表及作者自审同步，16明确静态待限定视觉批准；旧44FAIL、46旧新/41cross持续态、8恢复微差及错误背景480帧、ECI/平台/GC/极值与完整CT15保持，未提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。


最终结果：六不同场景Debug均PASS（原四12.4786s/新增二8.5300s），Release均PASS（四13.0868s/二7.4010s）；没有产品反例或源码修改，不把重复运行计为更多用例。6临时case/1源5368字节与7会话文件25204字节已删除，无媒体。清理后实际重建精确Statistic5+QRCode8+Empty13+Spin16+Marquee1共43PASS/0Skip/3.8264s；最终产品源码未变，复用Drawer最终NET8零警告零错误16.76s。git diff --check通过。作者另步自审确认provider/window保持存活与NoInlining观测，没有强持有被测对象；只补本六条对象退出，不关闭其他恢复/持续态/平台/极值或完整CT15。
