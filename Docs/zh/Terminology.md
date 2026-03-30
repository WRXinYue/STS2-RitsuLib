# 术语表

本文定义 RitsuLib 文档中统一使用的核心术语及推荐译法。

---

## 核心术语

| 英文术语 | 推荐中文 | 说明 |
|---|---|---|
| settings UI | 设置界面 | 指整体玩家配置界面。 |
| page | 页面 | 设置界面中的单个已注册页面。 |
| section | 分区 | 页面中的结构化分组。 |
| entry | 条目 | 分区中的单行可见控件或文本项。 |
| binding | 绑定 | UI 与存储值或内存状态之间的读写连接。 |
| persistence | 持久化 | 存储层与保存生命周期。 |
| persisted | 已持久化 / 会持久化 | 用于描述会写入持久化层的值。 |
| preview-only | 仅预览 | 指不会写入持久化层的控件或绑定。 |
| fallback | 回退 | 兼容或缺失数据场景下的回退行为。 |
| compatibility fallback | 兼容回退 | 优先使用该术语，避免使用“垫片”。 |
| bridge patch | 桥接补丁 | 将 Mod 内容转发到原版逻辑检查点的补丁。 |
| registry | 注册器 | 某类内容的运行时注册容器。 |
| content pack | 内容包 | 向多个注册器写入内容的便捷入口。 |
| builder | 构建器 | 用于链式构造页面、分区或内容的 API。 |
| override | 覆写 | 对资源路径、行为或值来源进行替换。 |
| placeholder | 占位值 | 数据缺失时使用的临时值。 |
| scope | 作用域 | 持久化值的存储范围。 |
| profile | 档位 | 按玩家档位区分的保存范围。 |
| global | 全局 | 跨档位共享的保存范围。 |
| epoch | 纪元（Epoch） | 中文文档首次出现可带英文，后续可简称“纪元”。 |
| story | 故事（Story） | 中文文档首次出现可带英文，后续可简称“故事”。 |
| Ancient dialogue | Ancient 对话 | 与游戏系统保持一致，不改写为其他称呼。 |

---

## 相关文档

- [框架设计](FrameworkDesign.md)
- [Mod 设置界面](ModSettings.md)
- [诊断与兼容层](DiagnosticsAndCompatibility.md)
- [本地化与关键词](LocalizationAndKeywords.md)
