# CT-03–CT-05机制实施记录

分支`feature/custom-theme`，基线`85407138f`，本批未提交。执行步骤见[Own计划](own-defaults.md)，统一状态见[任务进度](tasks.md#0-执行进度)。本记录区分机制实现、验证与产品迁移；95个产品Token没有改成V2，真实Button B02等消费问题没有因此自动修复。

## 实现

- 保留WholeEvaluatorV1构造／整体默认后覆盖语义；新增明确的DependencyDefaultsV2、逐层revision及默认声明。终端静态接口由生成器实现，旧生成器漏生成时C#编译失败。
- 默认上下文只提供已解析appearance和完整Global只读投影。Global属性与当前schema同名同类型，可变支持值经原typed codec复制；实测BoxShadows尾数组不共享。
- Roslyn验证同／跨程序集、默认重定义、可访问性、参数／返回类型及依赖，生成按名称slot与独立拓扑顺序；运行期直接调用，没有依赖图或逐字段闭包。新增诊断040–043，metadata诊断包含可用声明的完整身份。
- Compiler在V2分支先写显式Own值并设置独立位集，再求其余默认。空显式集不分配位集；小位集在栈上，大位集借用ArrayPool并在finally清理归还。新旧identity可同registry共存，仍按Control槽惰性计算。
- 缓存键保留CompilationIdentity并纳入版本／evaluator revision；key估算64→80 bytes对应新增字段。Own slot与Token schema不因默认语义修订重新排序；XML v1／编译ABI v1不增加字段。

## RED与GREEN

| 层 | 实际前证 | 实际验证 |
| --- | --- | --- |
| 分派合同 | 类型缺失的编译失败仅证明新合同未存在，不当作旧产品Bug | 新／旧入口隔离、非法位集／revision及Bind：2/2；完整Global投影及数组ownership后为3/3 |
| 生成 | 缺入口／Global投影和声明诊断导致7 FAIL，静态接口无生成器对照1 PASS | 逐步扩展同／跨包、重定义、不可访问、循环、确定性与revision；最终临时22例＋保留的运行契约1例，共23/23 PASS，零skip |
| Compiler | 正确配置的5例均因尚走旧入口而FAIL；先前探针的不存在Global名称、重复资源key及算法mode错误不算产品前证 | 5/5 GREEN；扩展缓存／scope／大位集和既有模块后60/60 PASS，零skip（536ms） |
| 诊断补验 | metadata身份缺失，以及显式静态接口手写／vararg未拒绝，均取得相应断言失败 | metadata声明身份与这两种非法声明均通过；private metadata方法在默认导入下不可见时，诊断指向实际可见的完整属性声明 |

Compiler实际数值：根ControlHeight50/LineWidth2，显式行高10→padding18；显式padding7保持7；显式行高0→padding23；合法动态codec的显式null保持null；清除下游覆盖恢复派生。父scope padding7与子行高10合并后仍7，重置继承后18；局部ControlHeight70时padding28。65／4097字段的最后slot显式0均保留。

失败处理核对发现最初文档／临时测试误写为“保存异常”。现行源码及既有架构是失败不缓存成功值、保留输入允许重试；已纠正文档和测试，未改变产品失败语义。临时场景前两次失败、第三次成功后复用，调用次数3。不同revision得到不同结果、相同配置命中缓存；既有CompilationIdentity原本就提供隔离，不将该对照虚报成旧缓存Bug。

null验证使用既有动态descriptor的合法codec，将`none`解析为null；本批没有暗改全部生成codec或XML/ABI的null字面量语法。逐字段单位、范围、消费、视觉与历史能力仍按家族合同核定。

## 构建与AOT

- `dotnet build src/AtomUI.Core/AtomUI.Core.csproj -c Release -f net8.0 --no-restore`通过，0 warning／0 error，8.86s；这是.NET8编译证据，不是Native8运行证据。
- 系统临时目录内Base／Consumer两个项目引用当前Core及Generator。Base提供公开抽象层／protected静态默认，Consumer终端由生成器接线；没有另行clone仓库。
- `dotnet publish <Consumer/Probe.csproj> -c Release -r osx-arm64 -p:PublishAot=true -o <run>/publish`及实际可执行文件运行退出0。生成入口输出`20,40,3,7,0,23,14,28,9`；实际生成descriptor→Normalize→Compiler→惰性槽得到padding3；已注册未查询终端构造次数0。
- 首次链接缺少本机Homebrew库搜索路径；仅在临时Consumer导入仓库已有`build/MacOSHomebrewNativeAot.targets`后通过，没有改产品平台脚本或安装依赖。链接警告指出本机库面向macOS26，本次只证明当前macOS26/osx-arm64运行，不证明macOS12或其他平台。

## 测试处置及最终检查

保留两项共享契约保障：Core在既有DenseThemeCompilerTests中新增一个行为用例，Generator复用已有真实引用／编译fixture新增一个Emit后实际执行的用例。编译器、ApiCompat只能保护形状，不能发现默认顺序或显式存在状态静默算错；专项AOT成本不适合每次运行。这两种错误会影响全部V2配置，须作为最小共享保障进入全局验证，不保留逐控件矩阵。Core用例118ms，Generator单独进程约1s（控制台取整，含首次Roslyn／Core加载）；成本已复核，纯模型或字符串检查不能提供等价运行保障，没有新增永久fixture／项目。

临时负例／边界探针3个源码文件及专属目录、外部Base／Consumer／generated／publish整个工程已删除；本批未生成截图或录屏。清理后Core精确47/47 PASS（257ms），Generator精确63/63 PASS（6s），零skip；.NET8 Release重新构建0 warning／0 error（7.11s）。既有Skia ButtonFocus 1/1 PASS（576ms，仅有该原文件已有xUnit1051警告），只证明该已有焦点绘制对照，不代替逐Control前后视觉验收。验证静态audit退出0，git diff --check通过；未执行全量或commit。当前本批独立最终审查中，未把产品迁移、品牌或原生无闪烁门槛改为通过。

## 最终审查

CT-03–CT-05只读最终审查未发现Critical、Important或Minor问题，接受进入CT-06。结论仅覆盖共享机制；95个产品Token迁移、DataGrid别名及消费、完整视觉与原生无闪烁、其他平台和发布验证仍由后续任务负责。审查未重新运行已删除的专项工程；验证结论依据上述实际输出。

用户随后明确要求后续全部工作由主agent完成，不使用subagent；后续按独立步骤自审，不再派发实现或评审。
