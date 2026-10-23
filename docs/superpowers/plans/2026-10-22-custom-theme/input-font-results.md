# CT-13 LineEdit字体消费分包结果

LineEdit三档Own字号自身未消费，配置后仍父级23；控件Global字号24也仍14。实际5项RED769ms。相应Own/Global显式配置才激活Own最终资源，未配置保留真实InheritanceParent，Custom用中号，IsCustomFontSize退出。字体族使用可选Global，raw资源在激活配置后参与最终取值。未修改Select过滤器借用LineEdit身份，仍归CT14。

## 实现与验证

Core内部泛型继承观察器复用已验收字体族生命周期，增加字号观察；AbstractTextInput提供protected父的内部只读桥。最终值由LineEditTheme根级Setter持有，两个状态用CompiledBindingExtension $self，无反射/代码改字体。最初尺寸条件Style会压过普通用户Style29（实际21）；该项失败后改根级有限选择器，最终9项PASS1s：三档/Custom、对应Global、Own优先、raw/local/普通Style、父级实时变化、跨Provider移除重挂载、IsCustomFontSize切换、长寿命父移除后的GC。成员1文件1控件0问题。

## 脚本化视觉与AOT

12实例/四尺寸，普通继承、父18及字体族列，保留clear/count与正常motion。明暗idle/hover/disabled/ready旧新全部0；A/A0，1DIP检错15390/15379。字号及字体配置改变27928/27920，恢复0；独立仅Courier New改变15678/15671，恢复0，已查看两个暗色配置画面。真实鼠标/键盘输入PASS，默认仍14/18/14各列。

当前Mac arm64 NativeAOT实际运行验证Compiled父字体、三档Own、自定义字体族和清除继承PASS，不推断其他平台。未把整个输入家族或性能/原生显示面最终出口记为通过。

## 清理及出口

9项临时测试及外部工程/专用产物已删除，41媒体运行文件清理、Git媒体审计clean。清理后Core27项236ms、Desktop51项3s通过；NET8 Release零警告/错误38.47s，成员/本地链接/git diff --check通过。作者自审确认普通Style优先级失败已解决、字体最终值仍由AXAML持有、父订阅分离释放。F1关闭不代表F/G/V或CT13全部完成；无提交，作者自审独立记录，不冒充外部评审。
