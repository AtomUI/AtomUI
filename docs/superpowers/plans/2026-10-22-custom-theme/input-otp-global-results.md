# CT-13 OTP Global消费分包记录

本分包将OtpLineEditTheme的6处及CellTheme的39处Shared读取改为OtpLineEdit typed Global；不修改四个Own的名称、公式、编号或V2协议，不修改C#、公开API、模板拓扑及固定几何。根字体、caret/underline/separator几何和光标时间线另在G3-B/C闭合。

## 实际失败与消费

四尺寸六格实际高度、字号仍为旧共享值，frame颜色、内部选区/placeholder、根操作区spacing和EnableMotion配置失效，12项RED1s。接线后12项最初报告62s；同编译产物详细复核总5.4419s，首例892ms，其余50–116ms，未复现持续停滞，不删除首次异常耗时记录。

补动态更新后17项PASS2s：已有六cell身份保持、四尺寸清除恢复、其他LineEdit身份隔离、根BorderBrush/CellBorderBrush/CellWidth原生优先级，以及长寿命父移除后的root与六cell整体GC。motion先进入root，再经既有CellInfo进入cell；本项不声明光标时间线已接通。

## 脚本化视觉与输入

16个OTP实例、96格，四variant/四尺寸，保留480宽、正常motion、空值placeholder、mask、separator与完整六位。两侧完整相同idle/hover/disabled/error/warning/ready/configured/returned轨迹；明暗六个默认阶段及返回均0像素。独立A/A0，1DIP检错11656/11764、回位0。配置使候选改变52731/52878像素；相对旧实现的配置画面改变28859/28943，符合原来未消费字段接通，不作为默认视觉豁免。已查看完整暗色配置图，所有格和clear均在原宽度内。

明暗各16个实例通过真实框架指针、键盘输入654321及clear点击，不调用事件替代输入。输入后不进行静态截图，因此没有关闭原生caret闪烁来消除采样问题；caret时间线仍单独验收。

## 专项与清理出口

JIT及当前Mac arm64 NativeAOT均实际核验96个cell的四尺寸高度、内部文本字号/选区、root与cell motion及清除；四张配置/恢复图JIT与AOT差异0。媒体alpha全为255，默认/恢复阶段另以完整RGBA核验0。没有IL2026/IL3050；已知本机依赖最低OS链接提示不能推断旧macOS。17项临时验证、外部工程427专用文件和49个媒体运行文件已清理，Git媒体审计clean。清理后OTP精确filter65项PASS2s，NET8 Release零警告/错误39.36s，九个本地链接和git diff --check通过。作者自审，无subagent、无提交，不关闭整个CT13或OTP家族。
