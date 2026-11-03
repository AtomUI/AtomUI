# CT15 FloatButton 原四条最终恢复与真实保留绘制输入

范围仅为原四算法的 `main-default → … → main-end-restore`。保留1100×900窗口、原40/35窄宽与完整长文、Group两个子项、180视口/1400内容、500滚动offset、受控Group开关及真实点击、Own/Global/Native、badge、方向、重模板、重挂和show/scroll定时阶段。没有改产品、主题、依赖、像素容差或正常motion；不重做BackTop Target资源修复或原65对批准。

## 精确原历史与当前复现

从本会话2026-10-30原17357 Program及17357/17398/17462/17488/17571/17667/17729/18132/18139/18228驱动修订恢复，fixture来自17089/17103。只提取文本并在外部driver应用已核对的局部字符串改动，没有执行历史命令或写回历史产品代码。原driver每版200帧（168静态/32定时），本轮完整72检查68PASS/4严格恢复FAIL。四最终恢复：L15/max4，D11/max3，C8/max1，DC2/max1。原managed L20/max4及原AOT17/max3、D11/max3、C8/max1、DC2/max1继续留在原结果，不用当前15改写原值。

差异在真实FloatButton与Group trigger的圆角边缘；L bbox(82,102)–(540,509)，D(80,104)–(540,506)，C(82,102)–(285,150)，DC(80,104)–(251,105)。已查看完整Light默认图，原文字换行、Group菜单和滚动条保持。

## 单变量只读观测

保持全部原操作，在默认/最终恢复截图之后只读取实际21个Avalonia Border：背景/边框/厚度/圆角/BoxShadow、Bounds、祖先路径、Opacity/Clip/世界变换，以及 `ElementComposition.GetElementVisual` 对应真实保留DrawList的opcode与资源。没有调用Render、InvalidateVisual、改内部状态或搭近似图元。临时诊断使用JIT反射读取已知字段；不把它放进产品，也不宣称该探针已通过AOT。

首次观测完整72检查仍68PASS/4FAIL，恢复13/11/8/2、max4/3/1/1。L/D/DC全部21份属性、命令原始字节和资源文本完全一致。Compact仅一个透明Frame的原始opcode在偏移14/15/16不同，其余文本一致；需按实际运行时payload字段布局解释，不能称四算法原始字节全部相同。两批168静态中9帧非零，含Light默认8/max3及后续单像素(max4/15)；不能声称观测完全无扰动，也不把它们归入本轮四路径。

最后补一批原完整历史，仅增加Own阶段真实67尺寸的输入正对照和运行时payload布局核对，执行/收尾见下文。未完整定因，不自行转归FW2；非零仍FAIL，32条原业务往返、原115旧新/34跨运行、平台及完整CT15不在本轮修复或批准范围。

## 构建与验证纪律

构建和采样均先非阻塞独占 `.artifacts/verification/run.lock`；锁忙即停止，不干扰其他会话。外部目录起初/var与/private别名造成MSBuild路径错误，改为canonical /private路径；取证工具main参数/返回格式两次调用错误已纠正，均不计产品RED。有效原driver构建0警告/0错误25.19s；只读观测增量4.84s及最终5.35s。未逐批发布AOT，未新增项目内测试源码；已有产品NET8/AOT证据只按原范围复用。

## 最后一批与解释边界

最终仍200帧/72检查68PASS/4FAIL，四恢复L20/max4、D8/max3、C8/max1、DC2/max1。各算法21个Border，L/D/DC全部输入与原始opcode相同；C透明Frame的原始字节偏移14/15/16不同。运行中的真实DrawRectanglePayload由Marshal.OffsetOf给出ServerBrush0/ServerPen4/ClientPen8/Rect16/BoxShadowCount112；含1字节opcode后上述差别仅在payload13–15的字段间填充，所有有效字段与资源相同。没有对图片作任何mask、裁剪、对齐或容差处理；原始opcode不全同的事实保留。

Own阶段从真实40/35宽到67，四算法分别15/21个Border输入变化，真实BackgroundFrame尺寸67×68；不是固定值或空输入的假绿。最终诊断Program SHA256 `f4e4d3b410f78a2057ee033fe1efcb85bf1a390abb547cb6c58ee9229de8650a`。Light默认、DC最终整窗已查看；原布局/长文/滚动/裁剪保持。没有新独立最小反例，不能宣称完整框架定因或与原FW2同根因。

三批重复静态微差及定时时相均不以观测输入相同来豁免。本轮没有产品实现修改，NET8/AOT产品收据复用；原AOT72项68PASS/4FAIL继续按原结果引用，临时反射诊断仅JIT执行，不冒充新AOT证据。620取证及外部工程清理、清理后精确验收见收尾。

清理：官方audit clean/findings=[]，cleanup removed=true/621项（620取证文件及清单）；外部160文件/618351353字节、驱动/build/marker已删除。无项目内临时测试输入；清理后精确FloatButton门禁随后登记。

清理后匹配Debug测试程序集精确 `FullyQualifiedName~AtomUI.Desktop.Controls.Tests.FloatButton`：46PASS/0FAIL/0Skip，2s。无新产品或测试编译输入，沿用同源码NET8零警告20.57s；没有新AOT发布、全量回归或Git提交。限定调查表待用户裁决。
