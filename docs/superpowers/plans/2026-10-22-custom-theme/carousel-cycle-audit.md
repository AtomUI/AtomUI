# CT15 Carousel 正常自动播放周期核对

复用原6metadata/7Own/44静态批准与26+2验证，不重做默认/Part/退役绑定。当前补正常autoplay连续两圈的实际SelectedIndex页序、有限末页不回绕、隐藏停止/显示恢复，保留caller-owned页面/TextBox/120高viewport900extent、normal motion/进度及原内容。

本新定位场景明确为720×480 owner、560×220 carousel；用于时钟/页序正确性，不替代旧完整渲染历史。default3000ms和公开native700ms两组；读取真实选择事件/帧，至少0,1,2,0,1,2,0，两圈后页面原引用仍在。真实采样时间误差与像素容差分开；无clock冻结/替换，不把不同相位帧作RGBA0。临时清理后精确Carousel与既有持续工作门禁，无产品失败前不改代码。

## 临时场景校正与当前证据

首轮3项1PASS/2FAIL，序列0,1,1,2,2,0,0；只读SelectionChanged.Source记录证明每次翻页各有CarouselPagination及Carousel两个冒泡事件。过滤Source==carousel后两种间隔记录完整0,1,2,0,1,2,0。再次重放3项2PASS/1FAIL：native700首次主题准备已消耗间隔，250ms观察点已在page1，说明“开始前一定page0”不是有效前提。改成完成相同原布局后才公开IsAutoPlay=true，非产品修复；随后3项全PASS/0Skip，45.1134s。

默认3000及native700均真实正常Dispatcher/Skia运行两圈；同实例隐藏暂停/重新显示继续、IsAutoPlay=false停止、有限末页保持page2和empty Next安全均通过。保留原caller-owned三页/TextBox/120viewport900extent及normal motion/进度，不冻结clock。不把来自不同时间原点的图片报成零差异。AOT使用同源场景与独立小断言驱动，缺using引发的两次编译错误属于驱动诊断，不作为产品反证；发布/执行与清理后验收继续。


## NativeAOT结果

相同源码三场景AOT38检查0FAIL/exit0。native700页变更时刻704.488/1401.196/2102.167/2802.206/3503.758/4206.419ms，maxGap82.931ms；default3000为3000.852/6001.253/9002.936/12002.072/15002.370/18001.883ms，maxGap93.118ms。managed相应首/末分别705.829/4204.339、3000.659/18001.289，maxGap126.324/62.972ms。间隔误差只按实际采样gap×2+80ms验证时间采样，不作为像素容差。AOT46261816字节，SHA256 74bfc43fee06e2b1ca5325404c1e7ae0aaa08affa7e96f938b7c8b4b1ac77e59；零IL/CS warning，五条既有Homebrew最低OS linker warning。首次外部驱动缺Homebrew linker搜索路径导致ssl未找到，补已安装目录后发布成功，未换官方依赖。

已查看完整720×480正常回到首页帧，560×220原页面与分页/进度仍存在；这不作不同真实相位的整帧零差异声明。无产品修改/新视觉变化，不申请扩大原44对批准。清理后精确模块正在执行。


清理：3临时case/1源5375字节、5取证文件及310会话757588934字节均删除，evidence audit clean。原反例/起点错误/编译与linker失败仅保留上述文字事实；临时驱动、marker、二进制不入库。精确Carousel3项待当前BackTop同会话验证后执行；因新BackTop产品改动，最终NET8会按最终源码实际重建，不能复用更早源码证据。


2026-11-01 Carousel正常周期补证：三临时用例PASS（45.1134s），default3000/native700真实页序均0,1,2,0,1,2,0；同源AOT38检查PASS/exit0。隐藏暂停/恢复/公开停止、有限末页与空集均验证，原caller-owned内容/120viewport900extent和normal motion保持。3临时/1源5375字节、5取证和310会话757588934字节已清理audit clean，清理后Carousel3项PASS（与Float46共49项/6.9191s）。无Carousel产品源修改/新视觉批准，原16旧新/4cross持续态、平台/极值与完整CT15保留，见carousel-cycle-audit.md。


清理后最终门禁：Float46与Carousel3精确共49PASS/0Skip/6.9191s；Desktop NET8 Release最终源码零警告零错误28.37s。全程共享run.lock非阻塞独占，无全量。成员1源/2控件0问题，确切原未提交快照到当前仅一个条件替换；注册/初始化/API/Token/布局均无增量。作者另步自审，非独立审查；订阅获取/退出与再次挂载配对，未改原动效模型或已批准家族。两条具体泄漏已修正，其他保留项不倒写。最终门禁helper/marker随读取清理，不自动提交。
