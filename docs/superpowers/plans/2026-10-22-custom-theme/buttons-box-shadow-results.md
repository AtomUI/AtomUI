# CT12 V11：公开根阴影候选结果

最终状态：2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。 早期阶段描述与原始FAIL保留，当前不再待V11/采样/视觉批准。见[完成结果](buttons-box-shadow-finish-results.md)。


2026-10-29，用户明确选择公开BoxShadow属性，133Own保持。公开入口工程/当前默认原生验证/清理与最终模块已完成；限定视觉尚待核定，不提前关闭CT12，不提交/推送。

## 合同与实现

新增Button.BoxShadow/BoxShadowProperty，类型Avalonia.Media.BoxShadows，以Border.BoxShadowProperty.AddOwner注册。Dropdown继承，复用Button主题的Submit/Reset/DialogButton也沿该入口消费。ControlTheme默认用编译Self绑定读取既有VariantShadow，既有阴影层改为TemplateBinding根BoxShadow；没有代码绑定、新节点、存在位flag、Own或Semantic Part。

Default/Primary/DangerShadow及preset默认计算均保留；preset的Blur3/Spread0是未定制默认，不再阻止应用用公开根属性配置。原生Style/local及显式空集合可覆盖，清除恢复当前Style或当前主题默认，不以空值猜缺省。已按全文件审查Button.cs的原代码绑定：真实InheritanceParent和模板外Flyout均有声明式不可达原因，不新增迁移债务。完整成员audit修改前后均1文件/1控件/0问题，无纯重排。

## 有效RED与GREEN

- 修改前，Button/Dropdown两项实际运行在注册属性查询处失败：根BoxShadow不存在。没有靠编译错误制造产品RED。
- 最小候选后，同两项通过（3.2206秒）：实际ShadowsFrame消费本地值，scope与颜色/variant变化不压过它；显式空值、原生条件Style、清除、重模板和分离重挂均通过。
- 补充18项通过（3.7779秒）：五种实际Button类×Default/Primary/DangerOwn共15项；编译AXAML根Style及兄弟隔离1项；provider仍存活时两owner释放2项。未改Own/default算法。
- Gallery现有语义例增加显式BoxShadow=DefaultShadow根Style，保留现有Effect与可见结果；现有运行契约断言实际绘制层跟随根属性。当前Gallery没有独立API/Token表，未另建一套；正式公开API与源码XML说明同步。

## 像素与输入

33个实例，Button/Dropdown三模板/颜色/变体/shape/四档及继承类，真实Skia、正常motion。修改前与候选采用同操作历史，明暗各9对（独立A/A/1DIP/恢复探针和idle/hover/pressed/released/disabled），18对完整RGBA差异均0；A/A0，检错有效，独立探针恢复0。真实鼠标计数和pressed/hover状态有硬断言。

编译AXAML两个根Style的显式阴影改变5390像素（最大通道差231），尺寸保持、真实点击1次。清除后原同实例得到24像素、最大通道差1的边缘残留；与同完整输入历史的无覆盖对照相比仍24，因此没有将它解释成单纯焦点不一致。原失败图片与数据保留。

单变量绘制定位：临时宿主在所有分组保持同样树和几何，仅full-damage组在采样前InvalidateVisual；无覆盖恢复0、自然覆盖恢复24、full-damage覆盖恢复0。它支持合成damage历史相关的判断，不是产品已修复的证据。产品与最终原生采样不包含该宿主或强制重绘；24像素仍须具体处置，不转为通用RGB1容差。

## 原生/AOT状态

托管与NativeAOT均已构建；AOT只保留既有五条OpenSSL/Brotli最低OS26对目标12的链接警告。临时原生场景实际30个Button/Dropdown/Submit/Reset/Dialog实例，编译根Style覆盖全部颜色，验证本地空值与清除和实际阴影层。只有原生采样完成后才填通过数；不能把修改前52帧冒充本次产物。

第一次采样在首阶段身份校验处失败、未截图：后续测试构建重写共享Debug输出，与较早copy-local副本差69字节。PE字段解析确认69字节全部在时间戳、MVID、CodeView GUID和PDB checksum，IL/资源内容无差异；串行刷新Native副本后完整SHA256恢复一致，原身份与像素断言未放宽。失败轮次保留，不计产品RED。当前等待用户静止时间重采，此前不再构建其他项目。

## 待完成

- 当前产物的managed/AOT实际原生绘制与指定像素比较。
- 删除本包临时源、外部工程/preimage/媒体，清理后精确模块和Release NET8；链接/diff/成员audit及作者另步自审。
- 回填V11与CT12。新显式定制、24像素历史差异及先前两对3968像素混合宿主细线按具体审查核定；不扩大CT11批准，不关闭CT25的旧全局切换调查。

## 2026-10-30 接手后的最终工程出口

用户明确转由本会话完成。旧驱动非默认region选项已移除，当前真实NSWindow managed/AOT各1236检查、52帧全部阶段内/AA/扰动恢复/清除0、跨运行12对0，正常motion/输入恒定/按下0。当前Skia3条复核保持5390配置及24/max1自然边缘，原始失败不改写；六临时源/441外部文件/121媒体删除，最后精确189PASS、NET8零及成员1/1/0通过。V11无工程未完项；当前仅待[具体视觉核定](buttons-box-shadow-final-visual-review.md)，见[结果](buttons-box-shadow-finish-results.md)。本段更新旧“等待静止重采”状态，保留上文历史过程。
