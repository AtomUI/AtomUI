# CT15 Dialog/MessageBox 限定视觉审查（2026-10-31）

**已获用户限定批准：仅下表16对明确Global配置变化。** 工程/资源释放/独立校准和清理后验收已完成，见[结果](modal-consumers-results.md)；25临时已删除，清理后276PASS、NET8零警告、成员4-2-0，媒体已清理。完整Modal/CT15不关闭，不授权提交。

## 已确认的16对Global变化

四算法×Dialog/MessageBox×Overlay/Headless Window，每格一对，格式changedPixels/maxChannelDelta。旧新完整操作历史相同；176对主帧160零，余16均为下表Global阶段；176对JIT/AOT全部精确0。

| owner/Host | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| dialog/overlay | 20710/224 | 20673/224 | 20706/199 | 20638/199 |
| message/overlay | 21336/250 | 21074/250 | 21332/233 | 21066/233 |
| dialog/window | 16527/224 | 16261/224 | 16575/199 | 16304/199 |
| message/window | 17140/250 | 16647/250 | 17187/233 | 16691/233 |

明确公共终端Global：FontFamily=Menlo、FontSize=23、ColorText=#802080、BorderRadiusLG=17、SpacingSM=23、FontWeightStrong=600、ColorWarning=#008000。正文接受终端字体/前景；标题保留HeaderFontSize/HeaderColor角色但使用对应字体族/字重，MessageBox警告glyph及真实间距接受同一终端配置，Overlay圆角接受Global。Window原生chrome及独立公开按钮不由本表定制。

默认、Own、专用Part、scroll、六类恢复等其余160对精确0；Host请求520×420、owner960×740、ScrollViewer高度140/可见竖条/24行原文与两个标准按钮保持。Window为当前Mac上Headless平台Window宿主路径，不能据此声称真实NSWindow chrome/IME/原生输入已验收。

完整Dialog业务/全部状态、持续loading/确认loading、RTL与自然极值出口还需逐合同补齐；已关闭CT12/13/14、CT25/FW1原始问题和官方Avalonia12.1.4保持。本文不申请非零容差，不授权Git提交。

旧/候选/AOT各16组A/A与1DIP恢复全部精确0，1DIP检测9341–11965像素/max198–235；相同场景三版本检测数字相同。首轮误改Surface定位变换的8对失败仍记录在结果中，校准修正不改产品、不扩大FW1。

所需结论仅为“批准Modal表内16对Global变化，其余未通过与完整CT15保留”。


限定批准登记：2026-10-31，用户明确回复“批准表内16对Global变化，其余未通过与完整CT15保留”（request_user_input_async / call_62f69b5d81a14a9ab06d7d6f5bb64392 / item 0）。仅Modal表内16对Global获限定批准，真实OS输入及完整Modal/CT15、其他未通过保留，不授权Git提交。
