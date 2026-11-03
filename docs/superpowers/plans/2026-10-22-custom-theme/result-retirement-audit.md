# CT15 Result 退出与保留判据核对

现有目录/feature/custom-theme，单agent，无分支/worktree/提交/全量。原Result分包保留完整当前Presenter却期望owner回收的探针仍为INCONCLUSIVE，不倒写为产品泄漏或已修复；本轮明确区分真实membership、调用方保留当前子树和已经替换的caller glyph。

## 独立预期

- 原Window/provider/shell长期保留，移除Result且不保留其子图，Result/glyph/Presenter/原生TextBox/ScrollViewer五弱引用应退出。
- 调用方仍持当前模板Presenter时，现有TemplatedParent/父链使Result存活是正对照；释放调用方引用后五弱引用应退出。
- 调用方持已替换旧glyph时，旧glyph的visual/logical parent应为空，Result/当前Presenter/原内容应退出；旧glyph原生27×31仍保持，释放调用方引用后旧glyph也退出。
- 缺省与实际Result scope两组：scope FontSize23/StatusIconSize93必须命中，原生glyph27×31优先。原520宽/完整标题描述/按钮/120viewport900extent/可见滚动条/完整原生输入保留；不通过Close整个Window或释放provider替代移除Result。

## 执行与判据修正

临时位置Temporary/CT15ResultExit，严格NoInlining建图/释放调用方引用；观察只读Parent/弱引用，不清composition私有字段、不改产品。初次缺ResultStatus命名空间的编译错误不记产品RED。

原四项Debug首批2PASS/2FAIL，详细重放持当前Presenter两项仍FAIL在释放后的回收断言；直接ShouldNotBeNull接收Presenter使Debug JIT暂存引用。只把该观测放NoInlining helper，四项Debug全PASS（14.21s命令/用例各约2s），不改建图、移除、窗口/provider保留、GC次数或产品。该观测反例保留，不当成新修复前失败。

增强真实scope断言并加入保留已替换glyph两项后，最终Release六项全PASS（19.79s命令）；普通退出/当前子树保留与释放/已替换glyph保留均符合独立预期。最终Debug六项同样全PASS（用例16s/0Skip），原scope数值93/23与glyph本地27×31、120viewport/900extent均通过。六临时用例所在单源6584字节已删除，无新增永久测试/媒体或专用产物；清理后实际重建精确ResultSemanticPartTests九项全PASS/0Skip（745ms）；无产品变化，复用相同最终源码NET8零警告证据。没有产品源修改，不新增永久测试、不请求新视觉批准。

原Result384帧/18失败和最终60对限定批准不变；新证据只补上述可解释的退出路径，不代表其他资源、平台、极值或完整家族/CT15通过。
