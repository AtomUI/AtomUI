# CT13 FU3：Form/Upload 字体结果

状态更新：最后五表已于2026-10-30获用户明确限定批准，CT13既定范围已完成；下文原始执行/失败记录保留，CT25/FW1继续独立。见[批准汇总](input-final-visual-review.md)。


2026-10-30。七个公开 owner 的 FontFamily 七项真实 RED→GREEN；扩展普通 Style、Local、清除、换父、真正重建根模板及 GC 后15项通过。字体族与可选字号均由根 ControlTheme 选择，只读继承父接口无状态字段、代码字体绑定或每实例资源字典。

原父 Courier New/23：Form/FormItem 的 Label14、Decorator/Upload/Trigger/DropZone 正文23、DropArea Header16 及 SubHeader14 保持。显式字体 Times New Roman 进入实际文字；普通 Style/Local 仍优先。FU4再独立接可选Global.FontSize和正文颜色，不用根 getter 代替实际文字。

触及七个 C# 文件时整理了22个存量成员问题。最初脚本漏末尾类型、重复闭括号及跨类型初始化次序均被编译/测试拦截并纠正；FormActionsItem与DisplayOnlyUploadFileSource原成员保留。FormItem的15个Form.AddOwner初始化移到静态构造器事件字段就绪后；两种构造入口实跑。原531成员中512指纹保持，16个FormItem字段/静态构造器因上述初始化调整改变，Trigger另3个旧几何方法由FU4主题规则替代；没有把这些差异写成全体成员相等。

明暗完整同历史各10阶段：AA/1DIP检错复位、默认、配置、清除、换父与返回、Local及清除。20对中18非配置对逐RGBA严格0；两配置对11859/11821像素，已查看关键原图。父23下102宽触发器原有长文字裁剪保留，没有缩短文字或放大卡片规避。

最终联合Mac真实NSWindow managed与arm64 NativeAOT各196字体/字号检查，另614家族出口检查；无反射成员扫描或IL抑制。NET8 Release零警告/错误20.62s；最终成员9文件/8Control/0问题。联合52临时专项、28对象弱引用及清理后精确验收见[家族出口](input-family-exit-results.md)。限定视觉见[审查表](input-form-upload-typography-visual-review.md)，未经批准不代替完整CT13。
