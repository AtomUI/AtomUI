# CT-25 启动索引主题查询分配整改

本分包属于CT13-G2性能闭环。实际App启动索引下，typed与旧无owner-type身份各10000次标量Control Global查询都分配560000B；伪造owner拒绝正对照通过。已有零分配测试使用手工registry，未覆盖生产StartupPackageIndex。不能据此自动解释全部热切换分配增长。

## 方案与边界

StartupPackageIndex保留字符串身份表供编译清单及有序链接；另以元数据现有Catalog/Id二元值建立应用自有的有限查询表。只保存既有entry，不构造descriptor、Token或Visual，不新增静态缓存、资源键、slot快捷路径或身份规则。

二元键不调用ControlTokenIdentity的校验构造器，避免把惰性校验提前；字符串原表仍保持相同重复身份错误。TryGetControl、语义查询及owner需求匹配复用该表，后续真实CLR owner校验和禁用包诊断保持原位置。追加索引按原metadata重建，沿用旧entry/slot，失败回滚行为不变。

## 执行与验收

1. 先实际失败分配及伪造owner对照，再采17消费者明暗相同轨迹基线、独立A/A和1DIP检错、16输入相同全局更新五轮性能。
2. 最小整改后两种实际启动身份标量热查询为0B；补既有owner/资源覆盖、惰性与追加契约，避免绕过校验换取低分配。
3. 同轨迹所有默认、配置和返回画面新旧零差异；公开原生资源路径单独测量，实际当前Mac NativeAOT验证零分配与动态配置/清除。头部、字体、状态、尺寸及资源键保持。
4. 保存真实耗时/分配，不以范围重叠声称无回归；删除临时测试、外部工程/专用产物与媒体，清理后Core启动/资源及受影响桌面输入focused、NET8及文档检查。作者自审，无subagent、无提交。

该包不代替原生最终显示面的边缘差异、持续动效或所有Host/平台验收。

同源补查确认原生Own枚举及scoped Own各10000次还会分配240000B，来自Convert.ToInt64对基础整数装箱。生成式Own key固定Int32，采用经GetTypeCode确认的直接拆箱；其他基础类型保留原Convert及溢出语义。补负数/超界/跨owner拒绝，实际公开Global+Own+scoped Own三组原生查询原来合计1600000B。新增默认视觉不变合同仍与原始基线比较。独立关闭motion的性能配方只隔离查询/绑定成本，不能替代正常motion视觉及时间线验收。
