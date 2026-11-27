# 完整注册的137ms首帧成本恢复调查（2026-10-13）

## 范围

保持已批准的完整注册方案，不恢复关键集，不增加平行描述符模型、预热调度器或运行期缓存。
本轮未保留新的产品源码改动；发布策略待用户明确开发/发布优先目标。
进入本轮前已有的Gallery TemporaryStartupTimeline.cs属于外部工作，不覆盖或删除。

## JIT取证

使用当前Release产物，对Desktop完整Collect捕获EventPipe JIT/Loader事件及线程栈采样。
工厂事件的OptimizationTier是QuickJitted。跟踪会放大耗时，以下为定位证据，不是无跟踪性能基线：

| 生成方法 | 方法数 | 配对JIT事件时长合计 |
| --- | ---: | ---: |
| Control descriptor工厂 | 183 | 82.197ms |
| Asset descriptor工厂 | 315 | 82.061ms |
| Semantic descriptor工厂 | 99 | 32.281ms |
| 缓存getter | 912 | 52.188ms |

实测负担集中在首次编译元数据构造代码，不能将其解释为主题派生算法计算慢。

## 实验一：缓存发布并入Add

只改生成器输出，把Volatile.Read/CompareExchange放入原来的片段/资产Add方法，保留原缓存字段、
异常和重试语义，消除独立getter。临时测试先失败，再实际编译生成输出，验证两次收集共享元数据而
不共享builder；Generator Registration及临时测试120项通过。

8组独立进程交错采样：

| 无跟踪完整Collect | 原生成形态 | 合并getter |
| --- | ---: | ---: |
| 首次收集中位数 | 276.650ms | 277.054ms |
| 当前线程分配 | 1,756,600B | 1,756,624B |

**无收益，已撤回。** 删除方法包装层没有消除构造体本身的JIT成本，不把代码变形当作优化。

## 实验二：标准ReadyToRun发布

独立临时程序引用当前Core/Desktop/字体项目，用.NET SDK标准PublishReadyToRun=true，
Release、osx-arm64、framework-dependent发布。它不改变框架源码或运行期初始化协议。
对比普通构建IL输出与同一程序的ReadyToRun输出；未禁用整个CLR的ReadyToRun，以免把BCL变化混入对比。

相同800×500 AtomUI Window、默认主题、en-US、AlibabaSans、Desktop包、空Deferred块和Render哨兵。
两侧各预热一个进程，再作6组交错独立进程采样；从Main中Stopwatch开始计时，不含Main之前的运行时启动，
不是显示器presentation，也不是完整Gallery shell测量。

| 指标（中位数） | 普通IL构建 | ReadyToRun发布 | 缩短 |
| --- | ---: | ---: | ---: |
| UseAtomUI同步段 | 366.49ms | 142.36ms | 224.13ms |
| Window.Opened | 924.83ms | 515.12ms | 409.71ms |
| 首次Render | 971.57ms | 547.51ms | 424.06ms |
| Framework Ready | 1150.12ms | 679.70ms | 470.42ms |

**对于发布产物，已实测覆盖此前约137ms的回退。** 效果包含依赖库与窗口路径的预编译收益，
不是声称只优化了注册的137ms。普通Rider/dotnet run构建不会自动获得这份发布结果。

代价：Core DLL从904,704B增至2,414,080B；Desktop DLL从8,422,400B增至23,901,696B。
另有发布预编译耗时和RID相关产物；本轮只验证osx-arm64，不能外推其他平台。
NativeAOT本来就是预编译发布路径，本轮没有改变或重新测量其性能。

## 处置

- 失败的代码优化已撤回，临时测试删除；最终Generator Registration重新验证。
- 运行期架构与现有未提交重构保持原样，没有自动打开发布开关。
- 同轮原始样本、JIT汇总和DLL哈希位于`.superpowers/sdd/2026-10-13-startup-recovery/`。
- 本轮创建的临时工程、跟踪文件和发布产物在调查结束时清理。
