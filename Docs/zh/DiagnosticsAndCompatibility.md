# 诊断与兼容层

本文说明 RitsuLib 在游戏原版之上提供的诊断策略与兼容层。

重点包括：

- 用于定位重复性数据错误的一次性警告
- 面向调试的缺失本地化与无效解锁数据回退
- 原版系统不处理 Mod 内容时使用的窄桥接补丁

---

## 设计意图

RitsuLib 不会试图隐藏所有引擎限制。它遵循以下规则：

- 能尽早暴露真实错误，就尽早暴露
- 原版没有安全扩展点时，框架可以补桥
- 某个回退会掩盖过多行为时，保持系统显式

这层能力是刻意收敛的，只处理边缘问题。

---

## 一次性警告策略

RitsuLib 的部分诊断只会对同一个问题（或同一稳定键）警告一次，包括：

- 缺失资源路径（`AssetPathDiagnostics`）
- **总开关 + LocTable 子项**开启时缺失的 `LocTable` 键（`[Localization][DebugCompat]`）
- **调试总开关 + 建筑师子项**开启时，`THE_ARCHITECT` 无对话注入占位值（`[Ancient]`）
- 其他解锁相关的一次性提示（例如 `ModUnlockMissingRuleWarnings`）

同一稳定键或同一类问题至多记录一次，在可读的日志量下保留定位信息。

---

## 资源路径诊断

显式资源覆写路径由 `AssetPathDiagnostics` 校验。

当资源路径不存在时：

- 输出一次警告（包含宿主类型、模型标识、配置成员名和缺失路径）
- 回退到原始资源路径或原始行为

这对角色资源尤其重要，因为游戏原版对缺失角色资源几乎没有安全兜底。

详见 [资源配置与回退规则](AssetProfilesAndFallbacks.md)。

---

## Debug 兼容模式

可选兼容回退由 `debug_compatibility_mode`（总开关）与设置页中的分项子开关控制。

**默认（总开关关）：** 走原版逻辑。

**总开关开：** 游戏内展开 **兼容回退项**；子项默认**开启**。关闭某一子项时，仅移除对应回退。

| 子项 | 开启时 |
|---|---|
| **LocTable 缺键** | 占位解析 + 一次性 `[Localization][DebugCompat]` 警告 |
| **无效解锁纪元（Epoch）** | 跳过该次授予 + 一次性 `[Unlocks][DebugCompat]` 警告 |
| **建筑师缺对话** | 对 `ModContentRegistry` 角色注入空 `Lines` 条目 + 一次性 `[Ancient]` 警告 |

除 LocTable 缺键处理外，各子项通常只作用于通过 RitsuLib 注册的内容。

**`ModUnlockMissingRuleWarnings`**（例如未注册 Boss 胜场规则）：独立于调试兼容子开关的诊断路径。

**发布内容：** 应提供完整本地化、时间线与对话数据；上表仅用于迭代阶段排障。

Windows 下设置文件路径：

```text
%appdata%\SlayTheSpire2\steam\<user_id>\mod_data\com.ritsukage.sts2-RitsuLib\settings.json
```

---

## 注册冲突诊断

RitsuLib 会显式检查以下冲突：

| 冲突类型 | 常见触发场景 |
|---|---|
| 模型 ID 冲突 | 同 Mod / 同类别下两个已注册模型的 CLR 类型名相同 |
| 纪元 ID 冲突 | 两个纪元解析出同一个 `Id` |
| 故事 ID 冲突 | 两个故事解析出同一个故事标识 |

检测到冲突时抛异常或输出错误日志，不会静默接受模糊身份。

---

## Ancient 对话兼容层

框架在 `AncientDialogueSet.PopulateLocKeys` 之前为已注册 Mod 角色追加基于本地化键的 Ancient 对话条目；作者编写键，框架负责发现与注入，使 Mod 角色复用与原版相同的 Ancient 对话管线。

### `THE_ARCHITECT` 对话兜底

受调试兼容 **总开关 + 建筑师子项** 控制。若原版 `TheArchitect.LoadDialogue` 无结果，RitsuLib 对 `ModContentRegistry` 角色注入空 `Lines` 占位值并记录一次 **`[Ancient]`** 警告。

具体键结构见 [本地化与关键词](LocalizationAndKeywords.md)。

---

## 解锁兼容桥

若干原版进度检查仅针对 vanilla 角色遍历。RitsuLib 以窄补丁将已注册解锁规则挂到相同检查点，使 Mod 角色在同一节点上参与判定：

| 桥接类型 | 说明 |
|---|---|
| 精英胜场 | 精英击杀计数的纪元判定桥接 |
| Boss 胜场 | Boss 击杀计数的纪元判定桥接 |
| 进阶 1 | 进阶 1 的纪元判定桥接 |
| 局后角色解锁 | 局后角色解锁纪元桥接 |
| 进阶显示 | 进阶显示解锁判定桥接 |

桥接补丁会把 RitsuLib 已注册规则转发到原版会跳过 Mod 角色的进度检查点；不引入独立的进度存储。

详见 [时间线与解锁](TimelineAndUnlocks.md)。

---

## Freeze 异常

当内容、时间线或解锁在冻结之后还被注册时，RitsuLib 会直接抛异常。

这是诊断机制：一旦晚注册，往往意味着 ModelDb 缓存已建立、固定身份规则已被使用、解锁过滤已在运行。此时最安全的做法是尽早失败。

---

## 排查要点

1. 警告多表示 Mod 数据或配置问题（路径、键、规则），而非随机引擎故障。
2. 资源与本地化应在数据源补全，而不是长期依赖占位值或兼容回退。
3. 调试兼容回退用于迭代排障；发布构建宜关闭总开关或关闭子项并交付完整数据。
4. 优先使用显式注册 API；兼容回退不宜作为长期架构依赖。

---

## 相关文档

- [资源配置与回退规则](AssetProfilesAndFallbacks.md)
- [本地化与关键词](LocalizationAndKeywords.md)
- [时间线与解锁](TimelineAndUnlocks.md)
- [Godot 场景编写说明](GodotSceneAuthoring.md)
- [框架设计](FrameworkDesign.md)
