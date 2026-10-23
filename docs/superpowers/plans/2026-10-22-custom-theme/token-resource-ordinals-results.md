# CT-25 公共Token资源编号兼容结果

本分包完成公开资源ID与内部Schema slot分离。build/metadata/token-resource-ids.tsv冻结95个Own终端及Shared共96范围、1478个ID；原1195 Own、255 Shared保留，Dropdown23、Rate1及Shared4新字段追加。所有实际生成范围逐字段与合同一致。新增字段不再让旧程序集内联整数读取另一资源。正式合同见[生成模块文档](../../../modules/generator/token-resource-keys.md)。

## 失败与修复证据

原生成路径5项实际失败：Dropdown旧ExtraContentMargin整数指向Color，Shared旧WaveStartOpacity整数指向Easing，旧枚举集合偏移。冻结显式ID后，Shared访问仍错误，证明不能把公开ID直接当Schema slot；Registry descriptor映射修复后通过。独立按旧枚举参照编译的producer在禁用manifest输入的同worktree重放6项全部失败；恢复输入后6项全部通过477ms。props按字节恢复，不另建checkout。

生成器11项临时检查通过1s：版本、空合同、重复字段/ID、空洞、负值、字段遗漏/删除、遗漏生成范围、整范围删除行定位、外部无合同兼容及实际生成编译。合同输入有界，错误ATOMUIGEN045；不改变XML v1或compiled ABI v1，旧资产仍受独立schema资格校验。

## 视觉与运行验收

12个实际Button/Dropdown/Rate实例，明暗、四尺寸、鼠标Click计数1与Tab焦点；五种默认状态旧新逐像素均0。A/A为0，1DIP检错13497/13855像素。真实Own margin/StarSize配置在相同键盘状态变化10182/10334像素，清除后均0。最初误把有键盘焦点的恢复截图与idle比较得到918/916；同状态比较0，没有裁剪/掩码/容差。已查看明色默认及暗色配置图。

当前macOS26/arm64 NativeAOT实际消费独立旧producer的Dropdown Thickness、Rate尺寸、Shared opacity/ZIndex及effective Global，运行PASS。临时producer先误用运行时CoreLib编译导致AOT引用失败，改用SDK reference程序集后发布/运行通过；这不是产品修复。GalleryBase Release构建0错误22.15s。未验证其他平台，不声明全平台通过。

## 清理及作者自审

删除17项临时测试、旧参照/视觉/AOT三外部驱动、29个媒体运行文件；cleanup内置Git媒体审计clean。清理后Core/Generator/Desktop明确filter聚焦通过，NET8 Release构建通过，Core36项247ms、Generator74项6s、Desktop Buttons/Rate128项2s；NET8零警告/错误19.97s。没有新增永久测试或提交。作者另步检查公开ID、slot映射、源生成诊断、旧literal实际值、内存边界及格式兼容；这是作者自审，不是独立审查。CT-25整体和其余控件家族仍开放。
