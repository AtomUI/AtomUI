# CT-13 G2 / CT-25：真实 Startup 查询分配核对

旧G2进度提出“成功查询每次ToString”的候选已过时。当前StartupPackageIndex的`_byTokenIdentity`使用metadata `(Catalog,Id)`，5381fb6c9已提交这项优化；本轮先核对代码及history，没有重复修改产品。

临时探针使用真实Desktop App启动后的ThemeScope/ThemeSchemaRegistry，硬断言UsesStartupIndex=true；未使用手工registry。typed及legacy身份返回同一descriptor，伪造Border owner的Button身份被拒绝。

各类先预热2000次，再执行10000次，当前线程实际分配：

| 真实生产路径 | 分配bytes |
| --- | ---: |
| typed identity查询 | 0 |
| legacy identity查询 | 0 |
| typed Global标量资源 | 0 |
| legacy Global标量资源 | 0 |
| Own标量资源 | 0 |

本轮1项临时验证通过3.5858秒；首次夹具错误调用三参数构造器，纠正为ForControl，不计产品失败。没有新产品修复，不要求伪造RED；本次是确认既有优化与清理过期候选。临时源码已删除，常驻测试净增0，未生成媒体或外部项目；清理后精确Core/Desktop结果待追加。

原G2整场景热分配31.949→40.230MB、延迟与往返合成差异仍原样保留。这个局部0分配不能解释或关闭整项26%增长，CT25继续按真实同工作负载核查。查询类型与资源键、惰性descriptor、追加事务和优先级没有产品修改，不新增字典或缓存。

清理后Core ThemeResourceLookupPerformanceTests 2PASS（155ms）、Desktop ButtonThemeScopeTests 6PASS（660ms），临时源码已删除，未生成媒体或外部专用产物。作者自审确认本轮没有StartupPackageIndex产品差异；原G2候选回填完成，整场景分配/像素出口仍未关闭。
