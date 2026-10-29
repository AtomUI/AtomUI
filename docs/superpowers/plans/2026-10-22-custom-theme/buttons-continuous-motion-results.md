# CT-11 / CT-12：按钮 loading 连续旋转开关

保持官方Avalonia12.1.4、现有分支与工作目录，仅主agent执行；承接[loading接线结果](buttons-hyper-loading-results.md)。本包是四个owner的loading消费出口，不代表全部按钮、Standalone Icon或完整动效家族通过。

## 原始反例与修复

四项真实Skia合成帧用例全部失败：Button、DropdownButton、HyperLinkButton与IconButton在自身IsMotionEnabled=false后仍有连续旋转。原始关闭后两帧分别变化34/32/29/26像素；此前正常旋转正对照34/35/35/27。不是只读取资源或Animation枚举。第一版编辑脚本在解析阶段失败、无产品修改，随后的四项重复失败保留为过程记录，不计GREEN。

五个AXAML文件内，Button/Dropdown各模板的七处Spin字面值迁至owner的IsMotionEnabled=True style Setter；Hyper现有loading分支同样由owner开关约束；IconButton仅在owner启用时投射其LoadingAnimation，Duration仍动态保留。没有新增公共API、转换器、C#绑定、时钟或动画引擎；普通Icon的可见性与卸载释放实现保持。控件自身loading状态及内容可见性不变。

四项候选实际帧通过：正常旋转34/34/35/27，关闭后全部0，重新启用后30/29/31/24。关闭静态画面已实际查看，loading图标与文字保留。关闭前→后单帧有意差异43/41/43/32像素（最大通道差145/145/151/100），来源是原持续旋转变为零旋转静态图标；这些实时采样不是默认动画的同相位兼容比较。

原生属性优先级仍有效：本地owner IsMotionEnabled覆盖主题默认；清除回到当前owner配置。用户自己给内容Icon设置的LocalValue属于独立内容定制优先级，本包没有改写内容本地值；此边界与其他完整业务分支仍须在家族最终审查时核定，不将当前四项通过扩张为任意用户内容的全局停动保证。

## 默认时序的精确对照

临时验证仅固定真实Compositor的Stopwatch输入，保持实际模板、Spin、1s默认周期及合成器渲染；在finally恢复原Clock状态。代码仅存在于临时测试，不更改Avalonia源码、二进制或正式缓存。先采候选，再只逆向本包五个主题文件取得原模板对照，结束立即恢复候选并核对五文件SHA完全一致；已批准的Hyper loading伪类始终保留。

320×310、scale1，真实Default及Default+Dark scope，四个owner；0/125/300/750ms的八对完整窗口像素差全部0，AA/1DIP/恢复对应前后也全部0。各侧AA0；Light1DIP3333、Dark3399，恢复0；phase0→300的真实输出变化正对照有效。原/候选各两项用例通过。已查看明暗实际动画相位图，无裁剪、对齐、容差或选帧消差。

## 原生与待完成出口

当前Mac NSWindow、scale2，最终managed程序63项通过：Hyper实际明暗与loading/透明度/重模板/重挂；四owner共享开关、Dropdown独立配置隔离、清除、native LocalValue优先级；IconButton由Spin切Pulse后恢复当前Pulse、400ms时长保留。原生程序观察的是实际节点值及状态，不把它等同原生屏幕连续帧或其他平台像素验收。

用户已批准此开关接线、精确视觉差异与内容Icon本地值的既有优先级边界（call_0977f632d3594f458d6de50f7b552c44:0），不含Git提交或完整家族验收。

NativeAOT、强化可见性断言后的重跑、临时产物清理与最终精确模块验证待追加。完整CT11、CT12和CT25继续开放。

NativeAOT实际Mach-O arm64程序与managed同样63PASS，exit0；ILC真实输入为官方包12.1.4/lib/net10.0/Avalonia.Base.dll，NET10/NET8原包SHA均保持FW1之前的数值。没有加载FW1临时候选。临时探针有两条Assembly.Location在single-file中为空的IL3000警告（只用于managed身份日志，AOT跳过空路径分支），本机OpenSSL/Brotli高于macOS12的链接警告也保留；不外推最低OS验证。

补强后的最后10项临时验证全部通过：四个实时帧用例同时断言loading图标仍有效可见；正常旋转32/34/34/25，关闭全部0，恢复30/31/28/22。内容Icon显式Pulse/900ms保持优先，清除回到owner Spin/400ms，再关闭回None。原始帧、批准帧与此次加强帧分别冻结，不覆盖原计数。

清理完成：三份临时测试源码删除，常驻测试净增0；148张PNG及manifest共149文件删除、媒体audit clean；外部Native项目/驱动/managed与AOT产物423文件、1,379,127,857 bytes删除，无外部目录残留。清理后精确Buttons/DropdownButton/IconLoadingAnimation模块132PASS、0FAIL、0Skip（3秒），未运行本地全量或change范围。成员审计1文件/1控件0问题，15份本轮相关Markdown的221个本地链接有效，diff检查通过；作者自审（遵循仅主agent约束，无独立评审）。NET8结果后附。

NET8 Release产品构建通过，0 warning/0 error，18.71秒。本分包出口完成；完整CT11/CT12/CT25继续开放，没有创建提交或启动新的Actions。
