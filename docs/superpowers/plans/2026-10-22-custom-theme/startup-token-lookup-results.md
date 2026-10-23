# CT-25 启动主题热查询分配结果

生产App根索引确认为StartupPackageIndex。typed和旧无owner-type身份的Global/语义查询各10000次分配560000B，四项RED174ms，伪造owner拒绝对照通过；现有零分配测试仅使用手工registry，未覆盖这一路径。新增有限Catalog/Id元数据二元索引，保留字符串清单表和原校验后，五项PASS100ms。

同源补查原生Own及scoped Own各10000次又分配240000B，两项RED163ms。原因是Convert.ToInt64把生成式Int32枚举基础整数装箱；GetTypeCode确认后直接拆箱，其他基础类型保留旧转换和溢出语义。最终八项PASS186ms；追加运行期包索引兼容检查后九项PASS161ms，旧slot保持、新身份按末尾slot可查询、未创建Control实例，涵盖上述六种热查询、伪造owner及Own负数/超界/跨owner拒绝。没有更改公开资源键、字段ID、slot或descriptor惰性物化。

## 视觉与性能

17消费者：四类文本输入的四variant/四尺寸，加完整六格OTP。原始基线与两处最终修复的明暗idle/hover/disabled/error/ready/configured/returned均RGBA 0像素。独立A/A0，1DIP检错15334/15319，回位0/1；检错实例不参与主基线。查看了完整暗色配置图，保留原多行高度、滚动、所有格和操作区。原生最终显示面的旧边缘问题不据此豁免。

正常motion五轮原始数据保留：热切换分配中位40232864→39645144B，时间593.48→981.94ms，样本波动较大，不作更快或无回归结论。另以独立显式关闭motion配方隔离查询/绑定成本，五轮冷窗口中位725.24→686.70ms、22996304→22908624B，十轮切换447.78→453.08ms、17895624→17482016B；三组原生Global/Own/scoped Own查询30000次从1600000B降到0B，查询时间15.00→16.98ms。改善是分配，不能把这批数据描述成端到端加速；G2完整性能出口仍开放。

## 未完成出口

实际当前Mac arm64 NativeAOT公开Global/Own/scoped Own混合30000次标量查询为0B，明暗动态配置/清除通过；相同场景操作体SHA确认不变，JIT与AOT四图RGBA 0。无IL2026/IL3050；已知本机依赖最低OS链接提示不推断旧macOS支持。九项临时验证、外部工程427专用文件和59个媒体运行文件已删除，Git媒体审计clean。清理后Core注册/启动/身份/资源64项516ms、受影响桌面525项14s、DataGrid Token对齐1项27ms通过；NET8 Core Release零警告/错误3.31s，九个本地链接和git diff --check通过。此为作者自审，无subagent或提交，不关闭CT25全平台出口。
