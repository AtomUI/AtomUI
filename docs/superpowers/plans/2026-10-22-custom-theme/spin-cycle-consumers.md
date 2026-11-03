# CT15 Spin 完整循环后续（2026-11-01）

此前46静态与原生优先修正已限定批准，不重复研发或改写边界。本轮只核对内置/自定义旋转整周期、四点初始透明度、Alternate透明度的2P周期和原0/P/3错峰延时。旋转P不等于整体透明度周期；不错误要求startup帧与P帧全图相同。

复用P3-C已验证的临时真实Compositor clock输入与官方12.1.4零长度区间因果证据，不修改依赖、容差、产品时钟或新增业务状态。先独立RED，再仅替换临时模型验证可保留曲线/延时的最小消费者修正，随后再改产品。

原12所选全FAIL（编译Path同名修正不计RED）：四算法内置/自定义旋转在P/2P/3P/4P停上一4.712389；内置初始opacity=[0,0.3,0.3,0.3]，而声明起点为0.3。完整204帧与12观察文本已收齐，不因旧静态批准覆盖这些失败。

临时对照模型：旋转明确0隐式起点+唯一终点；opacity先保持声明0.3，保留SetInitialValueBeforeDelay、P×index/3延时、Alternate/Forever与同easing，仅将首key放在最小正float进度，避免与隐式0构成零长度区间。该进度小于TimeSpan任意合法周期的单tick非零进度；不使用像素容差或可见时间延迟。该模型尚待8项实际检查，未据代码推断先改产品。

后续须完整正常clock/受控周期、Own热duration/可见性/换模板/退出资源、managed/AOT、临时清理与精确模块/NET8；有意端点/初始点变化提交具体限定审查。P3-C的96对已获独立限定批准；Part恢复max1/其他资源失败与完整CT15保留，不提交/新分支/worktree/子agent/全量。

最小次正规float首key的临时模型8项7PASS/1FAIL（Compact首点仍0），原批保留。改用最小正规float 1.17549435E-38f，8项全PASS（1s）；其非零位置仍低于任何合法TimeSpan单tick进度，保留原延时前0.3与全部可表示非零曲线。当前产品最小修正旋转隐式0起点/唯一终点，dot明确0.3起点并保留原Alternate/Forever/Delay与easing，仅首key移至该最小正规位置；完整所选验证中，不提前标通过。

当前实际消费者修正后36所选全PASS（20临时+既有16，3s），四算法内置初始四点均0.3，旋转所有整周期回0，P与3P稳态完整帧一致。原12FAIL及最小次正规模型7PASS/1FAIL不改写。追加热500ms时长/CubicEaseInOut曲线、显隐、换模板和detach/reattach八项验证正在执行，尚未AOT/清理/最终门禁，不把36局部通过当作完整Spin/CT15完成。

功能验证处置：36+8不同用例通过，原12FAIL与次正规模型1FAIL保留。28临时1源已删除，外部driver引用当前源码，原产品快照/候选快照均为本轮1源，回放持有共享输出独占锁并finally精确恢复。新模型位置使用最小正规正float；后续AOT必须验证，不依据JIT通过外推。


已查看四算法完整图面与首批TextBox差异区域，初值/端点/延时与组合原内容分开判断。28临时1源10258字节已删除；9375取证文件（原12FAIL/两个模型/候选临时/1160就绪不足帧/最终7116主帧/四拼图/TSV/manifest）经官方audit/cleanup删除，findings=[]、removed=true。外部driver412文件/publish15文件、原469会话共1082498989字节已删除；源码1源冻结逐字一致。清理后精确模块/NET8在独立四文件会话执行，不保留驱动、时钟访问器或媒体。


清理后真实重建精确Spin：SpinSemanticPartTests/SpinIndicatorCustomSizeTests/RecurringVisualWorkTests.Spin_共16PASS/0FAIL/0Skip、581ms（命令31.29s）；Desktop NET8 Release覆盖Common零警告零错误21.04s（命令21.27s）。均先取得共享输出非阻塞独占锁，最终四文件门禁会话清理。成员1源/1控件0、正式文档相对链接无断链、git diff --check通过；无全量/提交/新分支/worktree/子agent。仅308对独立起点/端点变化待限定批准，268个second为相同图像复核，16正常clock非零/原FAIL/其他恢复/平台/GC/完整Spin与CT15保留。


限定批准登记：2026-11-01，用户明确回复“批准表内308对，其余边界保留”（request_user_input_async / call_122c5e1eef6b4d7ca6e0c98bd9c0c55b / item 0）。仅Spin循环表内308独立起点/端点/错峰/重新可见变化获批准，51行整帧等价合并及268同帧second复核不扩大产品状态。16真实clock非零、原12FAIL/模型及就绪坏批、其他恢复/平台/GC与完整CT15保留；原46静态/P3-C96批准不扩大，不授权提交。
