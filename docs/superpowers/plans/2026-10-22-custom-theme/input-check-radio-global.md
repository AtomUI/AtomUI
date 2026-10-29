# CT13 CR3：CheckBox / RadioButton 精确 Global 消费

沿已批准的CT13逐owner有效Global合同执行，主agent、当前目录及分支。承接CR1/CR2，不改字体继承（另包）、Own默认、Legacy RadioBgColor或选择/输入合同。

## 文件与实现边界

- CheckBoxTheme / RadioButtonTheme：把已有Shared颜色、动效/水波开关替换为各自生成的typed TokenResource，同样的Setter和条件，保持原生优先级。
- CheckBoxIndicatorTheme / RadioIndicatorTheme：内部资产的线宽和动效开关用现有OwnerTokenResource，沿实际模板路由归公开CheckBox / RadioButton，不新增身份或绑定helper。现有TokenTransitions会把SharedTokenResourceValue标记的DurationInputKind转换为OwnerTokenResource绑定；时长先验证，不重复改已正常接线。Transitions仍在Style Setter。
- CheckBoxGroupTheme / RadioButtonGroupTheme：只处理已存在的动效开关与SpacingXS，归各自公开Group身份。内部CheckBoxItemsControl的默认间距沿Owner查询，Group的显式模板绑定仍优先。
- 既有公开子CheckBox/RadioButton维持独立身份，不把Group Global强行投射到item。字体和其他未触及维度留在下一切片。

唯一采用的路径复用现有typed资源和最近公开owner路由。给所有公共节点统一使用动态Owner查询会模糊明确身份，新增私有镜像资源或C#属性写回违反现行合同，均不采用。缺失新机制或路由反例时停止扩散替换，先定位真实失败。

## 执行与验收

- [x] CR3-R：临时真实Window检查每个公开owner的root/指示器/组间距，配置后当前失败；同scope兄弟身份不受影响，清除恢复、0/false、显式实例值保持。反例必须在AXAML替换前执行。
- [x] CR3-B：修改前完整明暗默认/输入轨迹、自然AA与直接绘制、1DIP检错；保留CR1已批准差异的精确边界，不自动继承批准到新差异。
- [x] CR3-I：仅替换经反例证明的现有资源入口，保留Setter条件、值类型及原计算，不引入新字段/API或泛化资源引擎。
- [x] CR3-V：同实例配置/清除、实际部件与时长、专用Style覆盖、输入/选择、模板重挂及释放；原/候选默认与配置恢复、新定制目标分别比较，当前Mac managed/AOT及NET8验证。
- [x] CR3-C：临时源码/原生工程/取证媒体清理；清理后精确CheckBox/RadioButton模块及成员/文档/CSV审查。只能回填该消费切片，完整家族及CT25保持开放，无Git授权。

CR3：六AXAML/38个已有Global资源入口接到四个公开owner与内部指示器；8有效RED加修正算法后的2有效RED→15PASS，0/恢复扩展14PASS；Mac managed/AOT各54，清理后原51项、NET8零警告。默认/输入原→新0，精确配置5015/5015及三点恢复3/3已批准；原始失败保留CT25。临时2源、419文件原生工程、108PNG+manifest全部清理，无Git/完整家族批准。
