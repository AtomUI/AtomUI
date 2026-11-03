# CT15 Spin 执行结果（2026-11-01）

输入5metadata/7Own，保持137/274、既定CT12/13/14批准和CT25/FW1。按已有执行计划、系统排障和临时TDD推进，无分支/worktree/agent/提交/全量。已冻结原10源文件于会话临时目录；第一次14所选临时用例用于实际四算法28默认、Own四SizeType热值/0、独立Indicator与组合Spin的Global反例。尚未完成，不提前标通过。

原首次编译Child修正不计RED。原12所选6PASS/6FAIL中两项Compact预期错误（实际ControlHeightLG35而非32）排除；冻结四算法实际28值后同12用例8PASS/4有效FAIL：Tip字号12非23、Family $Default非Menlo、Tip/独立圆点仍#1677ff非#123456。Own四档热值/0原已通过，不虚报为缺陷。四算法冻结向量：L/D=[15,22,36,4.5,6.75,10.5,1200]；C/DC=[13.25,19.5,32,3.84375,5.8125,9,1200]。向量顺序SM/M/L框、SM/M/L点、时长毫秒。V2仅Global默认公式，无新增Own依赖。

第一轮修正后12PASS（594ms），28默认精确保持。新增四用例时PathIcon命名空间编译错误已修正、不计RED；实际16所选13PASS/3FAIL：template-only自定义仍built-in、组合Indicator未取Spin颜色、自带27×31 glyph被强写67×67。原生字体清除已通过。修复按真实owner在Spin模板投射颜色；自定义模板与Content按同一选择条件；caller-owned PathIcon改用与Result相同的Template优先可释放typed relay，原生本地尺寸优先。内容子树无TemplatedParent，ControlTheme不可等价直达的原因留在代码，不用Part节点定制回退。

补充36所选34PASS/2FAIL：native普通Style被Template投射覆盖为有效新RED；改为Style优先实测glyph普通Style27×31与无值尺寸退休两项通过。一次错误filter发现0用例不计通过，改精确类名后3所选2PASS/1FAIL。另content命中探针：初始built-in可见目标Tag/Margin已命中，隐藏且尚未测量的Presenter未初始化热加样式；不误报SelectorRoute缺陷。改为核对两个marker、每个可见替代目标真实命中，切换custom后两目标均命中；专用AXAML不变、无节点赋值回退。

第二轮36所选全PASS（899ms）。另真实原生TextBox/100高视口/900extent重模板反例2所选1PASS/1FAIL：正文对象/文字/caret/视口均保持，但原焦点为null；后续另一输入焦点原已通过。复用Empty/Statistic的弱引用、模板代际、Loaded队列及后续焦点优先守卫修复，不隐藏/重建原内容、不改视口。

完整所选38PASS/0FAIL/0Skip（22临时+既有16，1s）。成员审查8源/2含注册属性控件，原内部MaskOpacity注册块一项顺序问题纯移动修正后0；Roslyn各成员token指纹完全相同，两个注册调用相对次序不变。最终10源（实际7源功能修改）冻结后进入外部驱动，采样期间不改产品源码。

Skia第一批Light40帧28检查PASS，Dark40帧28中两颜色预期FAIL后脚本主动停止，未运行old/AOT。实际Dark算法对ColorPrimary种子派生，不能把原字面#123456当最终有效Primary。修正驱动只读期望为公开ControlTokenResourceKey.Global(Spin,ColorPrimary)发布的真实Color，并同时断言Tip/组合圆点一致；不改产品、输入历史或容差。第一批80帧排除，后续candidate2/old2/aot2从头四算法完整重跑，保留本次错误期望记录。

第二批candidate2仅Light到global共3帧后驱动类型检查异常：公开资源投射可返回ISolidColorBrush而不是原始Color，不是产品缺失。源已确认ThemeTokenResourceProvider使用资源映射；只读期望支持Color/ISolidColorBrush的实际Color，不使用被测目标作为预期。3帧排除，最终改用candidate3/old3/aot3独立前缀保留所有旧记录。

驱动第四次构建CS0136局部brush重名失败，随后误启动仍为旧driver的candidate3-Light在相同3帧后异常；这3帧也排除，不计有效第三批。已纠正局部名，下一批先核对构建exit0再运行，沿用candidate3前缀前清理此3帧并保留异常文字，不使用残留二进制取证。

末轮补充编译SharedTokenKind命名空间纠正不计RED。50所选49PASS/1有效新FAIL：普通Style初始27×31正确，但IndicatorSize热改31时重建同优先relay导致width被覆盖成31。直接根因为每次尺寸变化重新安装绑定；源observable已经更新，不需要重复配置。最小修正只取消该重建，保留animated center更新；NaN由原生Width自动尺寸语义接收，不跳过首次建立的订阅。补换模板后的同一普通Style优先断言。此前主三版各160帧为pre-hot-style账本保留；最终代码需重新冻结并重跑有效全历史，不用旧批冒充最终产物。

修正后50所选全PASS/0Skip（34临时+既有16，1s），包含普通Style热尺寸/移除/重加/重模板、四算法有效Primary、8组spinning/blur/mask业务组合与Tip显隐。此前主3版480帧只保留为pre-hot-style有效历史，不代替最终产品；新candidate4/old4/aot4各43状态×四算法，并在同一全历史之后用独立新实例、无Controls配置补plain-default/native/restore三态，不额外发布第二套plain产物。

Pre-hot-style账本：candidate3/old3/aot3各160帧（124静态+36持续），候选/AOT各112检查全PASS，旧112中28FAIL；无缺帧。旧新74零/86非零（静态70零/54非零，其中46主有意候选、8独立cal未批准；持续32非零），cross128零/32持续非零，124静态全0。候选/AOT各52主恢复0、A/A4与恢复4各0，1DIP各1210/max233或170。原版同样主恢复/A-A/复位全0。主AOT22293256字节，SHA256 23bb4b2013b5aeddf5407482cce742617ab81261c8ad9128b7b3d3ebb7c01daf；0 IL/CS警告，5条既有Homebrew链接部署警告。此账本不替代最终hot-style修正后的全历史。

50所选通过后立即删除34项临时验证所在3源（18621 bytes），不留永久测试；临时样式已独立复制到外部driver，不依赖测试目录。外部最终完整Skia/AOT仍在重放，取证/会话清理及清理后精确模块与NET8待。

最终冻结candidate4/old4/aot4：各43状态×4算法=172帧（136静态/36持续）。managed/AOT各124检查全PASS；旧124中28FAIL，三版均完整无缺帧。136静态跨运行全0；总cross140零/32持续非零。旧新86零/86非零，其中54静态=46主有意待审+8独立cal非零保留，32持续非零。无Controls配置的独立新实例default/native/restore12静态旧新与cross全0，各4plain恢复0；其他52主恢复、A/A4与校准恢复4各全0，合计各56主恢复。1DIP各1210/max233或170检出。最终AOT22293256 bytes，SHA256 d089e4a2f3a4b8376e92e3459787bb19a600e7bb72651e2bc1420d103dc179fb，0 IL/CS警告，五条既有Homebrew部署链接警告。固定图面、显式低对比/长Tip原宽裁剪/局部重叠继续列明，不宣称原生最终OS面或完整周期端点。


最终成员8源/2控件0问题，四算法静态最小760ms且连续原始RGBA≥12相同帧。34临时3源18621 bytes已删除；1114取证文件（含86错误驱动原帧、480 pre-hot-style帧、516最终帧、4拼图与TSV/manifest）经官方audit/cleanup清理，findings=[]/removed=true；driver420文件与两publish各15文件、原会话共540文件/1200650917 bytes已删除，源码10源冻结逐字一致。最终精确模块/NET8在独立小会话执行，不保留任何媒体或驱动。


清理后精确验收：Debug/net10真实重建，SpinSemanticPartTests/SpinIndicatorCustomSizeTests/RecurringVisualWorkTests.Spin_共16PASS/0FAIL/0Skip、663ms（命令36.54s）；NET8 Release桌面构建覆盖Common，0警告0错误21.80s（命令22.05s）。均先取得共享产物非阻塞独占锁。最终run.py/cleanup.json/module.log/net8.log四文件会话随后清理，正式五文档相对链接0断链、git diff --check通过。仅表内46主静态和列明原生glyph优先修正待限定批准；其他边界与完整CT15不关闭。


限定批准登记：2026-11-01，用户明确回复“批准表内46对及列明原生优先修正，其余边界保留”（request_user_input_async / call_4176ecaa6e314f4da281e28a8657ee2f / item 0）。仅Spin表内46对主静态与列明glyph尺寸/普通Style优先修正获批准；32持续态cross非零、8独立cal旧新非零、平台/GC/完整周期及完整家族/CT15保留，不授权提交。


完整循环后续：[循环结果](spin-cycle-results.md)修正旋转/透明度起点与周期边界，默认/原生/Own/0及仅模板/组合都完成本轮取证和清理后验收；308独立变化等待另行限定批准。原46静态及原生优先批准不扩大，原恢复/正常时相/平台/GC/完整家族仍保留。

2026-11-02仅复用原会话文字核对独立cal来源：原完整Program在主轨迹后新建Spin（420×240、Tip为Independent calibration original、18×18自定义Border）、同一Config(algorithm)与实际背景，再做cal-a/cal-aa/TranslateTransform(1,0)/cal-restore。该Config仍明确选择Global，不能把这些图称为无配置默认。最终candidate4/old4/aot4摘要确证8个旧新cal非零、136静态cross全0和四组自身cal恢复0；逐条1002/max42目前只找回pre-hot-style旧批输出，未找到最终八条逐项值，故不冒充最终数值或扩大46对批准。媒体已按原规则清理，不新建近似场景补图；本分支停于这个明确证据缺口，8对仍未获视觉批准。


限定批准登记：2026-11-01，用户明确回复“批准表内308对，其余边界保留”（request_user_input_async / call_122c5e1eef6b4d7ca6e0c98bd9c0c55b / item 0）。仅Spin循环表内308独立起点/端点/错峰/重新可见变化获批准，51行整帧等价合并及268同帧second复核不扩大产品状态。16真实clock非零、原12FAIL/模型及就绪坏批、其他恢复/平台/GC与完整CT15保留；原46静态/P3-C96批准不扩大，不授权提交。
