namespace AtomUI.Theme.Schema;

/// <summary>
/// 一个注册包的编译期 schema 指纹（生成器 emit 的常量）。仅参与启动 Revision 的组合计算；
/// 值不要求与全量遍历算法跨域可比，只需编译期确定性与内容区分度。
/// </summary>
internal readonly record struct PackageSchemaFingerprint(string PackageId, ulong Fingerprint);
