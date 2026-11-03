# CT15 Card 操作区安排几何更新

2026-11-02，原Card7metadata/22Own范围内的实际动态绘制缺陷。复用既有字体/Own/Global/专用Style，不迁移默认，不改变接口或动作布局。先保留原Card32对批准和原恢复/FW2边界。

## 反例与因果

真实Card400×240/完整标题正文与两个原生Action按钮，生成CardActionsStyle只声明蓝色描边/1宽与命中Tag；没有取部件设置样式。子项宽120不变、操作区Bounds不变，只由HorizontalAlignment.Left改Right，实际位置0→78，但新边界像素仍白而没有分隔线。有效1FAIL/2s。

独立单变量只使同一操作区InvalidateVisual，其他操作相同，诊断PASS；原例仍FAIL，合计1PASS/1FAIL、5s。先前20→30高度尝试被Button原32最小高度约束，属于夹具未触发；宽120→160真实路径通过，不作为产品RED。夹具误用Card.Title和遗漏Input扩展using的编译错误分开保留。

## 最小实现

CardActionPanel原绘制从运行时Action的TranslatePoint/Bounds计算线段，集合改变会重绘，子项只安排不改变测量/操作区Bounds时未使自绘失效。缓存最后实际绘制的值类型Rect?，观察自己拥有的LayoutUpdated；仅实际子项几何与最后绘制不同才失效。原线段、描边、零尺寸/null映射和绘制次序保持。

初稿按模板grid接线已取得22PASS；作者简化为框架原生自拥有事件，删除模板观察者与三生命周期覆写，再次22PASS。官方Avalonia12.1.4 Layoutable.cs的LayoutUpdated及OnAttached/DetachedFromVisualTreeCore负责外部layout-manager接线配对；自身事件不持有模板/Action对象。没有新的通用观察框架、代码样式绑定、强引用Action缓存或定时器，C#注释说明ControlTheme不能表达从运行时子项安排变化失效自绘。

当前三档×RTL/LTR、重模板、命中与长寿命Window移除后的回收扩展验证进行中，不提前记通过。既有成员审计两源/两控件0问题；当前临时3源、会话和旧Release Desktop程序集仍在，最终渲染/AOT/NET8/清理后Card门禁尚待。未关闭Card或CT15，不提交。

扩展结果：三档×LTR/RTL、同实例Left→Right→Left、公开Card重模板后再次Right、原生命中，以及活Window保留下移除Card/fixture/panel三个WeakReference释放，加独立重录诊断及原20项共28PASS/0FAIL/0Skip、40s。RTL原整点取样落在子按钮覆盖的半个描边上；以真实local坐标+0.25采笔画外侧，随实际镜像变换选物理点，没有改产品公式、对齐图像或放宽像素比较。原3个RTL取样失败单列，不计产品RED。

8临时不同条件/3源已删除；一个冻结参数驱动集中24条件（四算法×三档×LTR/RTL）的旧/managed/AOT，使用外部原Release Desktop DLL，SHA256 `d0597faefe381360757b41ab3769fb053053d037d454ed35fb5cc79b68d0d25f`。无配置默认独立实例与原动态历史分开；集中采样进行中，不提前记整帧、AOT或最终源码NET8通过。


集中managed/旧行为结果：各120帧、240检查；候选240PASS，旧216PASS/24绘制FAIL。24个独立无配置默认与24个初始left旧新全0；right与template-right各24对每对40–46像素，最大120。24个left-restore旧新14–22像素非零；候选自身恢复9–18/max1–24，旧自身16–22/max2–24均保留，不算修复或归框架。已查看普通middle原/新right全图，缺线与正确线实际可见，原按钮/完整正文/尺寸保持。

当前最终两源成员审计2文件/2控件/0问题。冻结源码哈希与AOT实际身份已记录，AOT25838112字节，SHA256 `5df12919e8123c831615bad7b2356966d13183bff78d6f21fdfd90dd1f3be62a`；发布成功，0 IL/CS warning，五条既有Homebrew最低OS linker warning。实际AOT同历史执行中，清理后验收及新源码NET8仍待，不预记通过。


最终AOT240PASS/exit0，120完整cross全0。8临时/3源已删除，361取证经官方audit/cleanup clean/findings=[]后删除。共享产物独占锁下清理后真实重建Card20PASS/0FAIL/0Skip、990ms；最终源码Desktop NET8 Release零警告零错误20.57s。成员审计2源/2控件/0问题，源码冻结哈希一致。正式changelog/implementation、CSV受影响2metadata及矩阵/总表引用同步，Own22及全局137/274/决策分类不变。

作者另步自审：缓存只含值类型矩形、不保存Action/模板Grid；自拥有LayoutUpdated借用框架已配对的挂载机制，活Window回收验证通过。每次布局只比较当前实际分隔几何，不对所有布局无条件重绘；原Style/Native输入、坐标/线段公式、零尺寸/null映射、内容及命中保留。没有新绑定/反射/通用框架/公开API或注册/字段顺序重排；原未提交改动保留。只将48对解释明确的变化提交[限定审查](card-action-geometry-visual-review.md)，24往返非零、原FW2及完整CT15不扩大。外部会话及marker在最后记录后清理。

本表48对修正已获限定批准；原话、请求ID与完整排除范围集中见[视觉表批准登记](card-action-geometry-visual-review.md#限定批准登记)。未关闭整个Card或CT15。
