# CT-25 公共Token资源键数值兼容分包

按钮收尾审计发现：生成公开TokenKind按名称隐式编号，新增Dropdown23字段使12个既有值偏移，旧ExtraContentMargin=11现在变成DangerColor。Rate新增1字段及Shared新增4字段同样须对照。先解决该交付阻断，再继续其余家族；不创建新工作树，不提交或委派。

- [x] 以源码审计基线85407138f和原1195字段冻结已存在公开Own枚举值，Shared旧255个值同理；用当前实际资源访问证明旧literal误读，而非只比字符串。
- [x] 增加版本化、受控的构建输入build/metadata/token-resource-ids.tsv，冻结每个内置token终端及Shared公开资源ID；保留原值，新增字段只追加。所有96scope有可审查记录，缺字段/重复ID/空洞/删除/未登记新字段有生成诊断。外部无manifest的旧生成路径保持，可显式提供自己的合同。
- [x] 生成器按资源ID输出显式TokenKind值和对应Scoped TokenKey值，Schema存储slot仍独立按原规则，不混用。共享枚举ID到真实schema slot通过registry映射，不能继续将(int)kind直接当数组索引；不改XML v1/compiled ABI v1格式或另造运行期配置引擎。
- [x] 测试先证明旧Dropdown/Rate/Shared整数指错实际字段，再验证全部冻结键。构造仅含旧公开枚举定义的临时reference程序集，编译独立producer后在当前runtime/真实NativeAOT消费其旧literal，证明资源类型/意义正确；旧参照不是研发副本或checkout。
- [x] 生成诊断专项、实际配置/别名/Global和scope、默认明暗像素、NET8、当前Mac NativeAOT；临时源/旧reference/driver/media全清理后聚焦Core/Generator/Desktop、文档/成员/diff及作者自审。未验其他平台不填通过。

取舍：新冻结表是构建合同，更新必须追加ID，不按字母重排；Schema slot与资源ID分离，避免对外整数资源键绑到内部排序。已发布字段不删不改型；本轮未发布新字段分配追加ID。最终仍由CT-26汇总兼容与迁移说明。
