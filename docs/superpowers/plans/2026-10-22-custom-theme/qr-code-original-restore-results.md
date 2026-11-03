# CT15 QRCode 原八条恢复与真实框体/码图输入

范围仅原四算法parts-clear/icon-clear，复用2metadata/2Own、16视觉批准、原35功能/8默认/编码及退出证据，不重新研发。原有效old2/managed/AOT三版八恢复均L/C各1/max2、D/DC各12/max1，见[原结果](qr-code-consumers-results.md)。480错误背景帧继续排除，不以白码白底的原暗0覆盖有效失败。

## 原完整历史与方法

从原28565专用QRCodeCoverStyle夹具、28708完整Program和28910正确scope背景修订恢复，720×520产品Window、Size160、含中文Payload/原quiet-zone、各状态/Custom内容、Own/Global/Native/Part/Icon/尺寸/ECC/Value/方向/重模板/显隐/跨scope/真实键盘/持续Loading全部60阶段保持。仅提取原文本，不执行历史旧源替换或Git命令。外部驱动原Main无条件return0被收紧为 `_failures==0?0:1`，只改变失败退出、不改变操作；本轮没有失败被吞掉。

共享锁独占构建/采样，四算法各60图/35检查PASS，共240帧/140检查。外部增量构建0warning/0error、5.01s；无产品/测试源码改变、新AOT、全量回归或提交，已有匹配NET8/AOT业务收据仅按原范围复用。诊断为临时JIT反射读取，不声称新探针已经AOT验证。

原默认、Own、parts-clear、icon-clear截图之后，只读取真实PixelAlignedBorder/DashedBorder/Avalonia Border的有效值、几何缓存、笔、祖先Bounds/Opacity/Clip/世界变换、实际ElementComposition保留命令原始字节及资源；不调用Render/Invalidate或改状态。直接读取当前QRCode.Bitmap RGBA SHA256/尺寸、Color/Value/Status/Size；不重新编码或搭近似图元。复用Alert已核对的同一参数化只读探针，SHA256 `d2124154be1235b4286c66971bdcecd3fb9b6edf0af756b936bf5da31097b20c`。

## 结果与限制

八条原失败精确复现：L/C parts-clear、icon-clear各1/max2，点(437,357)；D/DC各12/max1、bbox(281,202)–(439,359)。这些位于真实Frame圆角；该PixelAlignedBorder位于(280,200)、160×160，四算法/两恢复的全部上述输入与默认逐字相同，含opcode原始字节。当前160×160码图RGBA SHA256及Color/Value/Status/Size也相同；Own阶段四码图哈希均改变，保证读到了真实输入。

没有声称整树相同：默认仅10个框体，恢复15个。隐藏Cover和Expired按钮在激活后保留其布局/绘制缓存（初0、后158等），已披露，未将其清零或强制销毁规避原历史。它们的祖先Cover/ExpiredLayout为不可见；真实差异点落在外框圆角，不能由此宣称框架根因完全定位或排除全部不可见节点依赖。

独立原校准A/A/位移恢复四算法各0；1DIP5419/5419/5531/5531，max224/199/224/199。D默认和L parts-clear完整图已查看：暗色码与正确暗背景可见、原内容/窗口标题/尺寸保留。没有做像素mask/裁剪/对齐/容差。Program SHA256 `31a3bf99bcc35e929b87b510db7a4c9abc60788e30c57a3fcf88d59a03c4375d`。

没有无观测新副批，因此不宣称本轮观测完全无扰动；只说明本轮八条数值与有效原三版精确相同，140业务性质通过。真实输入一致不代替屏幕恢复通过；八条仍严格FAIL。仅提议八条交CT25/FW2独立子记录，其他持续态/跨运行、ECI/平台及完整CT15排除；不认定同根因、不改官方依赖/FW1/容差，须用户裁决。清理及精确门禁随后登记。

清理：官方audit clean/findings=[]、cleanup removed=true/257项（256取证及清单）；外部156文件/618261776字节、driver/build/marker删除。无项目内临时测试编译输入，清理后精确QRCode验收随后登记。

清理后精确QRCodeSemanticPartTests：8PASS/0FAIL/0Skip；产品和测试编译输入未变，匹配NET8零警告20.57s与原有效三版AOT证据按原范围复用，没有重复发布。八路径有限调查待裁决，无全量/提交。

正式采用后原路径重放：当前严格结果集中于[采用结果](feedback-region-sdk-adoption-results.md#原恢复合批结果正式采用后)，旧FAIL保留历史，本轮未覆盖不自动转通过；清理与最终精确门禁集中执行。
