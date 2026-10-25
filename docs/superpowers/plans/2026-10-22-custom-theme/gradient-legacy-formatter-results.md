# CT19 C3：历史formatter入口兼容结果

既有feature/custom-theme及当前checkout；本项未创建分支、worktree或提交。源码基于已获视觉批准的B08，新增差异仅为旧SetPresetColor方法内的强类型委托适配，当前SHA256为e2d97a621f6128fe300088445c42ee64a7eb8f9429a80a8dedc770b20e2fb101。旧B08结果仍对应其原哈希，本项单独资格确认。

## 前后证据与合同

原版运行10项临时检查，9失败、1通过；非空两参数Func在写入三参数Action附加属性时触发ArgumentException。首轮临时驱动将枚举Hsva误写为Hsv，修正编译后才取得上述有效运行前证。

候选10项全部通过，覆盖真实模板的Format／Value重复更新、返回新集合、复用已归属控件、返回同一输出集合、空／null结果、回调异常不由适配器清空原内容、原Action委托及本地null／ClearValue样式优先级。清除后自定义节点脱离面板，默认两个文字节点恢复。

保持公开方法名、参数和二进制签名；ColorTextFormatterProperty和getter继续为原Action类型。先调用Func取得结果，再清除和替换输出；同一输出集合不重复添加。没有反射、DynamicInvoke、代码样式绑定、新订阅或新字段。GenerateValueText原有的回调前节点准备行为不在本包改变。

## 渲染资格与观察

Mac arm64、net10、Avalonia12.1.4、Headless真实Skia、region-dirty=true，900×360、scale1。原Action旧版／候选与候选Func分别重放明暗、main／独立A/A／独立1 DIP，共126帧：默认→安装formatter→格式→禁用→值更新并启用→清除→完整reset。

原始驱动只设置RequestedThemeVariant，检查图片发现产品实际仍是明色；初次标题也有532像素A/A及603像素恢复差异。该126帧判为不合格，计数和媒体哈希保留后清理。补充真实ThemeManager算法切换、Committed／Appearance及Window.ActualThemeVariant断言；同步等待在异步测试上下文下会阻塞动画，取得超时后改用await真实异步采样，覆盖实际Transition时长并确认所有声明属性无动画及连续三帧相等。没有修改产品动效、标题或比较区域。

终版42组A/A全0，42组1 DIP全部检出（1163–4033像素）；旧新Action42对全0，候选Action／Func42对全0，6对同实例默认／reset全0。没有容差、mask、配准或重绘消差。已查看实际暗色禁用及明色默认：暗色背景与色块弱化正确，自定义橙色文字保持本地优先级，默认渐变文字与框体完整。Func旧版原本赋值即失败，候选Func画面与既有正常Action入口比较，不虚构旧Func的可用画面。

## 实际Mac原生宿主

managed及NativeAOT各136项API／模板／父关系检查通过，耗时分别22.506s／25.718s，实际Window scale2，明暗各含Overlay与独立PopupRoot，并验证固定打开、格式／值更新、关闭／重新打开、自定义输出身份、清除及原Action兼容。AOT DynamicCodeSupported=false，exe SHA256为`8de0d6781903b6b3d345632910ebc162eca599795e995af78decefff917330fd`，120065040字节。这里证明真实原生宿主功能，不是系统输入注入或原生最终像素验收。

首次临时App未声明partial产生ATOMUIREG015，改为partial后构建0警告／错误。首次AOT链接未提供本机已安装OpenSSL／Brotli的LIBRARY_PATH，ld找不到ssl；补充仅该命令的现有库路径后发布9.768s通过，源码未变，无IL2026／IL3050。5项本机Homebrew dylib最低OS链接警告保留，不证明macOS12支持。

## 清理与剩余出口

两份临时C#与重放helper已删除，原始和候选两份完整源码备份已删除；无永久测试净新增。原生项目423文件／1,179,192,239字节已删除。两轮各126张媒体均在关键图审查及数值判定后清理，逐帧SHA256、原始计数、失败原因及audit文字保留，媒体审计clean且findings为空。

成员审计1文件／1控件／0问题；清理后ColorPickerPinnedOpenTests／ColorPickerSemanticPartTests共20PASS，实际运行4.717s；NET8 Release构建18.82s，零警告／错误，diff检查通过。作者自审，无独立agent评审；不将本包写成完整ColorPicker、CT19、CT25或CT11–26验收。其余Own／Global／Part／状态与平台最终像素仍独立推进，Git提交须另行审查批准。
