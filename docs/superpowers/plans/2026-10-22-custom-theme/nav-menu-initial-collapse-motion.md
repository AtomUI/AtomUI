# CT14：NavMenu 初始折叠首次展开短设计（设计已批准）

本片源于容器 N2/N4 的实际动效核查，独立于[第十七批冻结快照](platform-seventeenth-followups.md)。用户已于2026-10-26明确批准本片推荐短设计；批准登记时产品源码尚未修改。设计正文保留实施前证据，不表示视觉或Git批准。

## 已复现事实

实际420×240 Window，NavMenu Mode=Inline、Width=300、IsMotionEnabled=true，初始IsInlineCollapsed=true，默认折叠宽度48。加载后首次展开只从48跳到300，没有中间Bounds；对照先正常展开、收起后再展开，产生121、174等实际中间宽度并到300。两条轨迹最终base Width都为300，没有更改几何或关闭动效。

切换前独立记录的目标输入：初始折叠路径中GetBaseValue(WidthProperty)=48、ResolveExpandedWidthForInlineCollapsedMotion返回48、最近展开宽度为0；实际Bounds也为48，于是Prepare判断起终点相同并跳过motion。解除折叠后底层Width恢复300。对照路径GetBaseValue及目标均为300。两项临时探针1 FAIL／1 PASS，公开输入和实际Bounds、临时内部宽度及motion状态均已记录。

本地参考源码按规定顺序定位到 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`，commit `8eeda4f6f546165b3f72e63c9f42247abb306905`。其PropertyStore的GetBaseValue返回已coerce的_baseValue，coerce回调入口仍收到请求输入；参考源码只解释机制，AtomUI当前运行的48／300轨迹才是本片依据，不以本地参考版本代替运行证明。

## 建议方案与边界

建议在现有Width coercion入口保留请求宽度的一个实例级double输入，展开目标使用请求值，不把已经压成48的有效值当作用户展开宽度；Width=Auto/NaN仍使用现有最近实际展开宽度回退。该数值是宽度状态机的输入，随当前属性求值更新，不引用Visual、不增加订阅或全局缓存。实施时须证明绑定、style/local/animation优先级、明确0、NaN/Auto及折叠期间更改宽度均保持；不能以最后一次回调值的假设代替验证。若输入优先级不能保持，先修订设计，不提交未经验证的镜像字段。

保留Prepare在effective-mode切换前固定实际起点的顺序、默认duration/easing、base Width/绑定、Min/Max、停用动效、改向和卸载取消。根宽度仍由现有coercion和内部布局宽度协作；不引入Animation优先级relay、延迟刷新、反射访问PropertyStore或初始化特判。

先切换effective-mode再读取Width的方案会暴露300→48的临时属性变化，并可能让同步布局回调提前跳到终点，因此不采用。读取框架私有_uncoerced字段引入反射/AOT边界，也不采用。

## 合同与实施出口

API审查等级L2：初始折叠首次展开从跳变恢复为已有默认时长的动画，属于可观察动效变化。没有新公开签名、Token、Part、伪类或Style入口；不改变稳态默认画面、普通收起／展开和用户宽度优先级。

批准后先补明确原始失败及优先级／中断／约束反例，再实现最小修复；实际Light/Dark自然帧核对两条历史、首帧／中间帧／终帧、默认及恢复，正常输入与原生/AOT分别验证。临时源、驱动和媒体清理后只跑NavMenu精确模块与NET8，完整成员／文档自审。预期动效变化若需主题视觉单独批准，仍按实际证据审查；本次短设计不代表视觉出口、Git提交或完整家族完成。
