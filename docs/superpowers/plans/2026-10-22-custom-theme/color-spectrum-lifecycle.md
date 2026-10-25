# CT19 C7：色谱异步结果与输入映射一致性

沿现有feature/custom-theme及checkout，由主agent独立执行，不创建分支／worktree，不自动提交。第十一批13文件已另获提交／推送／Actions授权，本项不在该快照中。继续既有systematic-debugging、TDD、控件优化、资源生命周期及脚本化视觉验收；不提出产品性能提升主张。

## 修改前审计

- 类型：narrow edge-case correctness／lifecycle；目标ColorSpectrum，1755行，不因行数拆partial。源码职责包括公开给同程序集的属性合同、模板输入、选择椭圆、后台像素／HSV映射及实时画刷。
- 状态owner：UI线程上的ColorSpectrum拥有当前输入、模板部件及已应用的位图／映射；后台任务只能计算启动时快照，不能拥有当前控件状态。
- 入口：OnApplyTemplate中的layoutRoot Bounds订阅、范围／Components／Shape属性变化发起CreateBitmapsAndColorMapAsync。OnAttached只Post UpdateEllipse，模板重建解除旧部件订阅；没有对应的任务代次／detach失效边界。
- 当前风险：后台已捕获components／范围／尺寸，UI完成阶段却按当前Components分配旧像素，并把当前范围写成旧map的元数据；旧请求晚结束可能覆盖新请求或同步fast路径。该段尚无受控完成顺序反例，不先称已修复。
- 可达合同：公共ColorPickerView的Min/Max范围通过TemplateBinding进入实际Spectrum，非完整范围进入后台像素路径。主题固定SaturationValue，而AbstractColorPickerView.ColorSpectrumComponents属性未接线的现状另记CT19兼容合同，不能借本次切换默认面板或私改API。
- 比较：同包ColorSlider的已验证请求版本／attach路径；ColorPickerInput的权威输入与草稿边界；AbstractColorPickerView模板参数。现有PinnedOpen与SemanticPart测试没有受控位图顺序／映射断言。
- 结构审计：1文件／1控件／24问题，均为属性／事件region及ThirdComponent的DirectProperty backing位置；纯布局先保存逐成员token、imports及初始化顺序证据，与语义修复分开，不把布局整理当行为修复。
- 现有同步旗标_updatingColor／_updatingHsvColor与输入循环相关，本项不新增ignore／suppress。空地图fast模式、六分量、范围归一、指针／键盘、模板与挂载、Bitmap和PooledList释放均需按影响复核；旧minBitmap在部分分量下的用途／释放作为资源审计事实核对，不能只证明版本守卫而声称无泄漏。

## 合同与步骤

1. 在所属Desktop测试Temporary/CT19SpectrumLifecycle，使用真实公开View、现有Headless/Skia及原尺寸／滑轨／输入区，以临时worker门闩控制旧非完整范围请求和新范围／尺寸请求的完成顺序。检查实际Bitmap像素、尺寸及真实输入map，保留默认fast正对照。证明旧实现有效RED；挂钩最终全部删除。
2. 先完成纯成员布局，保持属性／事件／模板／Token／资源编号及所有成员实现token；不拆文件或迁移公开合同。行为候选只处理请求的线性顺序和所属宿主／模板有效性，使用启动时已捕获输入写入一致的已应用元数据。同步fast路径同样使旧任务过期，旧任务不能替换或dispose新图。
3. 按真实反例决定attach／detach／retemplate的失效与重新生成边界，不凭静态猜测新增回调；验证旧模板和宿主释放、输入保持及后台池缓冲归还。ControlTheme不能生成运行时像素，固定模板绑定继续优先AXAML。
4. 删除测试挂钩后，公开范围／尺寸往返、默认／明暗／恢复同轨迹A/A与1DIP、真实Mac managed／NativeAOT及指针／键盘。新预期视觉差异单独审查，不扩大C1–C6限定。其他OS与最终原生像素单列CT25。
5. 删除临时源、驱动、工程和媒体（待审媒体按规范暂留）；清理后精确模块、NET8 Release、成员审计与diff检查。正式实现／changelog和本结果按已证事实更新；没有完整家族验收证据前不关闭CT19。

当前在原算法上临时增加worker门闩和实际Task观测，未实施行为候选。公开View范围25→60、让旧请求晚完成，实际红色位图左上像素应为#FF6666，却被旧结果#FFBFBF覆盖，已建立有效RED。原View内部横向StackPanel有固定的实际期望宽度，改变外层View.Width没有触发Spectrum Bounds，首次尺寸用例只是观测等待超时，不计产品失败；独立Spectrum本身的正常Width合同另用单独宿主验证，不删除或减弱原View约束。最初临时探针的命名空间／编码器编译准备错误也不算产品RED。所有失败和准备日志分别保留。

## 进行中结果

修正尺寸fixture后，两项原算法反例3.8054秒全部失败：公开View颜色被旧#FFBFBF覆盖，独立Spectrum当前420×160被旧300×160覆盖。纯成员布局的99个成员token／全部imports和header相同，结构审计24→0，静态初始化顺序不变，常量DirectProperty backing移位独立复核；纯布局patch单独保存。

实际范围还有同源一致性错误：像素生成已把min>=max归一为min，但旧缓存元数据仍保留倒置范围，选择环可能Math.Clamp异常或0/0；8项矩阵原1通过7失败（包含上述顺序2项）。候选增加UI线程拥有的请求代次，fast路径也使旧任务过期；UI提交只接受最新代次，使用该请求的分量与归一范围更新像素／map／元数据，并对零跨度百分比取确定的0。8项4.5049秒通过，未新增ignore／suppress旗标、订阅、计时器或宿主回调。

扩展前，普通范围8项仍通过，而单像素轴2项产生非有限map、键盘共享helper的3项倒置范围返回max而非min，13项矩阵8通过5失败。像素坐标沿单像素轴与既有指针计算一样取0；IncrementColorComponent在min>=max时直接返回该分量min，保持另外两分量，不进入算术或英文命名色搜索。20项5.2447秒通过，包括英文small／large、相等／倒置、普通增量与wrap正对照。扩大理论参数时一次internal enum公开签名编译准备失败已单列，不算产品RED。

独立使用框架Headless SetRenderScaling实测同一窗口1→2和同一公共View迁往scale2窗口，逻辑色谱Size保持，实际Bitmap与完整HSV map均按当前scale2生成；两项4.1443秒通过。该输入在当前代码已满足合同，因此未凭静态猜测新增ScalingChanged或attach／detach回调，亦不称本机模拟为原生平台DPI验收。下一步核对重叠任务、模板／释放、真实输入及原生／视觉；临时挂钩和测试尚在，不能结案。

## C7 原生与视觉验收合同

- AC7-V1（原异步覆盖反例／公开范围合同）：800×620、Skia scale1，公开普通／渐变View保留实际模板、输入、滑轨和动效。固定明／暗、默认→受限S25/V80→倒置S60/20→等值V60/60→倒置H120/60→连续S25/40/60→reset；每步先恢复合法范围和红色输入，渐变用实际首个stop指针输入选择。位图／selector读取只读，旧算法异常按失败保留，不能转为通过。
- AC7-V2（主题兼容／检错）：每版本、主题各main／独立AA／独立1DIP角色，完整操作历史相同；1DIP只把根左边距24改25。比较整幅精确RGBA，无缩放、遮罩、通用容差；默认／reset同实例往返单列。采样覆盖实际Transition及至少650ms并等待连续真实帧稳定，不能关闭动效。
- AC7-N1（真实宿主）：同一公共View及同一范围往返，Mac实际NSWindow managed／NativeAOT分别记录句柄、scale、位图尺寸、顶点像素和选择环坐标；两帧真实渲染回执后发送框架原始指针，验证命中、事件和当前颜色。只读观测不修改内部节点样式。
- AC7-N2（共享helper邻接输入）：真实原始键盘必须记录焦点、事件到达和实际颜色变化；不得把Spectrum内部Focusable=false绕过为已经支持键盘。本次不接线尚未生效的公开Components属性或新API。原生初探证明Hue滑轨获得焦点且方向键事件到达，但没有颜色通知；源码确认AbstractColorSlider／ColorSlider未处理键盘，不能将本来不存在的路径认作C7原生键盘通过。该既有可达性缺口另列C8；C7的helper键盘边界仅有临时功能检查证据，原生脚本保留无处理的现状正对照。
- AC7-L（清理与边界）：所有临时驱动、工程、截图和二进制在项目外；视觉审查及用户所需批准完成后清理，只保留数值、SHA与文字。原生检查不等于原生最终像素或其他OS通过。

功能阶段最终26项8.0734秒通过，覆盖fast替代pending、模板替换及所有位图请求完成后的释放。一次GC探针在请求完成但渲染未确认时仍观察到保留；加入两个实际渲染帧回执后通过，未为此改产品生命周期，也不声称已证明此前保留的框架内部原因。所有临时测试与产品挂钩已删除；清理后23项5.3630秒、NET8 Release零警告／错误及2文件成员审计零问题通过。

原生准备中一次初始渐变stop命中检查失败，另两次方向键收到但无颜色通知的期待失败均保留，不改为产品RED。首版基础Avalonia Window的暗色文字／背景继承缺失，因此改用产品AtomUI Window（仍800×620、隐藏title bar）；输入前明确UpdateLayout并取得真实帧回执。该宿主110项功能检查通过，但后续看图发现只设置RequestedThemeVariant没有切换AtomUI有效主题，两个标称模式实际均为Light，因此撤回明暗覆盖结论，只记两轮宿主功能证据。键盘无处理仍作为明确覆盖缺口记录，不属于新增功能。未在产品中改变输入、布局或动效以使驱动通过。

首版基础宿主42帧功能300项通过；14对A/A与6对往返中10对保留3–8像素滑轨thumb边界差异，14组1DIP全检出。已查看明色默认与暗色倒置饱和度画面，确认两面板、输入与滑轨保留；该组因宿主继承不足只作准备证据，未声称正式视觉通过，数字／SHA和失败记录保留，42帧媒体已审计清理。正式产品宿主同时采集自然合成帧及独立历史的直接Skia诊断，二者不能混作同一基线，也不扩大此前C1–C6容差。

产品宿主第二组70帧（自然42、独立直接28）虽有500项功能检查通过、18对直接Skia A/A／恢复为零，但看图暴露其暗色输入无效，不能记作正式明暗视觉证据。自然比较11对3–6像素失败照常保留；数字、SHA和失败日志保留，已查看所标dark默认图实际为Light，媒体审计后清理。下一组先通过公开ThemeManager.ApplyThemeAsync提交Dark算法／Light配置，断言事务状态、CurrentTheme.Appearance、Window.ActualThemeVariant及实际背景，再采样；不只靠文件名或请求值证明主题。

## 最终切片出口

正式有效亮暗候选514项、26项受控功能和真实Mac managed／NativeAOT各184项通过；140帧、28组1DIP和原版同轨迹的真实失败完整保留。用户明确批准所示修复及[结果表](color-spectrum-lifecycle-results.md)内41对限定合成差异，不扩大原批准或比较器容差。全部临时驱动／工程434文件、正式媒体145文件及两组准备媒体清理完成，审计无发现；清理后Release聚焦23项、NET8零警告／错误和2文件成员审计通过。作者自审按请求owner／提交快照／资源缓冲和默认API复核，未修改公开Components默认、模板接线或键盘可达性。C7切片研发／验证完成，Git尚需单独批准；CT19完整家族及C8继续。
