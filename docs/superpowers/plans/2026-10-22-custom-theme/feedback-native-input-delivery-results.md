# CT15 原生输入投递前置条件

仅收敛[Modal原生输入历史](modal-native-input.md)中的输入/焦点缺口。原261失败、旧输入失败和原观察者shutdown异常保留；本记录不宣称通过一次新绿证明所有旧偶发根因已修复，不修改Dialog焦点代码。

## 原历史恢复

恢复36384初Program及36405/36480/36569/36728/36797/36884文本修订，保留原四场景、Dialog/MessageBox×Overlay/Window、960×740、520×420 Surface、140viewport/24行、完整内容、正常motion、async veto/Escape/同controller重开Enter及原四捕获顺序。原阶段源13661字符，SHA `5b1d8e677d9e4cf845c1f6f1a68c6266436b7e9cd70b8fa8f26249f4119a3d48`。不执行历史产品替换命令，单一外部工程使用当前正式compat引用。

当前原阶段261项258PASS/3FAIL、exit2。前三个false为Dialog Overlay第一次Escape前的`native-key-target-active`、`native-keyWindow-matches-target`，以及其后`async-close-veto`；捕获时active=false、点击后焦点null，首次Escape没有CloseRequest，下一次目标有效后才收到Rejected。该业务失败缺少有效输入前提，不能据此修改产品关闭或焦点逻辑。其他原编辑、Tab、关闭、重开及四焦点返回本批通过。没有将exit2或这三条改记PASS。

## 单变量与有效投递

只在原click/key之前增加NSApplication公开激活请求，仍保留150ms及原其余历史：261/261 PASS、exit0，32次目标IsActive及keyWindow检查均真、四焦点返回通过。实际官方12.1.4提交2d79ba55的WindowBase.Activate→WindowImplBase.Activate→WindowBaseImpl::Activate本身已调用ActivateApplication；故额外请求的绿不能证明旧驱动完全没请求应用激活，也不能证明所有历史输入失败同根因。

最终驱动使用原Window.Activate后，实际观测应用isActive、窗口IsActive和AppKit keyWindow均匹配才投递。就绪轮询上限1500ms；超时直接返回“输入未投递”，不继续生成veto/编辑等伪业务FAIL。不增加固定截图等待、不修改坐标或产品UX、不停止motion、不用部件赋值制造成功。

最终原四场景261/261 PASS、exit0。52个click/key就绪观测均在0ms满足三条件，实际没有靠新增延迟获得通过；32次原key目标/窗口检查及四次原焦点返回全部真，完整veto、关闭一次、重开Enter和24行/140视口保持。运行目录管理Skia字节与正式compat输出全同。已查看当前MessageBox Window整图，原选择文本、完整业务内容/标题、滚动条、两按钮与520×420 Surface保持。

这是验证驱动前置条件纠正，非产品焦点修复。旧缺乏完整激活状态的历史仍未完整定因，不凭一次有效批覆盖；真实外部物理输入/IME/其他OS不在本次新增门槛内。后续查看、清理和精确Dialog/MessageBox验收仍必需；不全量、不提交。

Cleanup: official audit clean/findings=[], removed 16 evidence/manifest files; final helper/driver/log receipt directory 155 files/613817540 bytes removed. No product code or permanent tests changed. Final focused Dialog/MessageBox gate follows.

清理后精确Dialog/MessageBox：276PASS/0FAIL/0Skip、17s（命令29.00s），真实重建并使用正式compat引用。生产源未变，NET8零警告及既有AOT业务证据按其原范围复用；此次两次261成功批均为managed native，不写成新AOT。最后三个外部门禁脚本/日志/JSON在登记后删除。当前有效输入合同通过，旧不满足前置条件的三FAIL与未完整定因的其他旧批继续保留历史，不作为已修复产品bug或PASS计数；不把历史红灯自动新增为当前研发门槛。
