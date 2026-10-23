# Token资源键的稳定数值合同

公开SharedTokenKind、各Control TokenKind及Scoped TokenKey是可被旧程序集内联的资源键。内部Schema slot可以按名称组织，公开资源ID必须独立维护；新增字段不能让旧整数指向另一字段。

## 内置冻结表

仓库构建通过build/AtomUI.Repository.props向生成器提供build/metadata/token-resource-ids.tsv。单文件最多4194304字符、32768条数据记录，各文本标识最多512字符。版本1首行为`AtomUI.TokenResourceIds<TAB>1`，其后四列为程序集、Token定义完整类型名（Shared使用$shared）、字段名、非负资源ID。95个Own终端和Shared共96范围，现有1478条记录。原1195个Own和255个Shared ID保持；本轮Dropdown23、Rate1、Shared4新字段追加到各自范围末尾。

每范围ID必须从0开始、唯一且连续；旧字段不删除、不改ID，不按字母重排。新增字段只追加新ID。提供合同后，每个实际生成范围均需登记，遗漏、空合同、非法版本、重复、空洞及删除均报告ATOMUIGEN045；附加文件错误定位到对应行。整范围移除也要求显式API迁移，不以缺失声明静默退化。

未提供合同的外部源生成消费保留原名称排序路径；外部公共组件包需要稳定数值时可提供同名附加文件并遵守同一追加规则。该文件是编译输入，不是运行期配置、另一套Token表或新的主题格式。

## 生成与运行边界

生成器输出显式TokenKind值，Scoped Own值由同一个资源ID编码为-1-ID，Global值引用SharedTokenKind成员。Own/Global Schema descriptor及默认依赖仍按自身slot规则组织，不用公开枚举整数充当存储索引。

Core Registry用已有有界数组按Shared资源ID索引descriptor；GetGlobal/GetEffectiveGlobal再读取其真实slot。资源字典和Own查找仍使用原枚举键；别名保持独立历史资源ID并投影同一规范值。没有反射解析或运行期字符串合并。

XML v1与compiled ABI v1不变。该合同保证公开键数值，不承诺绕过已有编译资产schema资格检查；旧资产仍须满足其独立版本/Schema合同。验证必须包括旧literal真实资源值、独立旧参照程序集和当前NativeAOT，不能只比生成源码文本。
