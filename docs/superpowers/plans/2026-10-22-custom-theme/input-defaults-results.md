# CT-13 四个输入Own的V2默认求值结果

TextBox3、LineEdit3、TextArea6、OtpLineEdit4共16字段迁移DependencyDefaultsV2 Revision1；全部名称、类型、公共资源ID、默认公式和单位保持。四个实际注册描述符原为V1，4项RED1s；迁移后10项PASS1s，包含6组明暗/控件Global/显式Own快照。96个格式化值逐项相等。默认工厂仅依赖有效Global，没有Own之间的读取；删除旧整表override和空构造。现有9个V2终端共149字段，不能据此宣布全部1219个Own或家族消费完成。

## 公式与实际消费

TextBox中/小档Round、大档Ceiling和运算顺序保持；LineEdit三个对应Global字号保持；TextArea颜色/尺寸/padding/margin和OTP宽度/间距保持。新协议不自动改变现有控件范围或把未消费字段标成已消费。

当前Mac arm64 NativeAOT实际四owner验证：padding资源、LineEdit字号、TextArea实际handle20、OTP实际cell46与gap资源11及清除恢复PASS。其他平台未验。

## 视觉与脚本自审

16控件/四owner/四尺寸明暗，固定多行高度/原内容，真实键盘输入。A/A0，1DIP检错18946/18914。首轮hover1/7px差异来自脚本仅旧侧先做过1DIP扰动：同V2单变量复现完全相同1/7。统一完整动作历史后，V1/V2 idle/hover/disabled/ready/configured/returned全部0；各自Own配置清除也0，无容差/遮罩/裁剪。源码四文件SHA256确认精确恢复。

原OTP样本宽320而配置6×46+5×11=331，最后一格裁剪11px；保留原证据后独立fit样本改请求宽度44（合计319），不改变产品或固定宽度。两版本fit配置画面相同；相对原默认变化15013/15073，恢复0，查看了完整暗色六格。AOT46只证明值和实际宽度消费，不把超宽画面当成适配通过。

此前验证脚本两侧动作不一致是作者错误，已修订个人scripted-visual-acceptance skill及AC模板：相同完整轨迹、检错隔离或已证零复位；不得删除用户原始触发或忽略产品往返失败。skill quick_validate通过，三情景作者自审；遵守用户无subagent要求，非独立压力评审。G1原生绘制/往返问题和默认文字例外待裁决仍独立开放，不据本次结果消除。

## 清理与最终验证

10项临时测试与外部工程427专用文件已删除，65个媒体运行文件清理，Git媒体审计clean。清理后Core33项253ms、Generator默认契约1项1s、Input/OTP198项5s通过；NET8 Release零警告/错误18.89s（进程20.78s），11个本地文档链接及diff检查通过。作者复核16工厂公式/运算顺序、冻结资源编号、平坦依赖及显式覆盖优先；没有新增常驻测试。作者自审，不提交；CT13其余字体/共享frame/OTP消费、选择控件和全家族出口继续推进。
