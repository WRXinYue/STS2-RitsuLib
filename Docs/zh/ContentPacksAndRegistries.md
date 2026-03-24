# 内容包与注册器

本文是 RitsuLib 注册体系的参考文档。

它主要解释：

- `CreateContentPack(...)` 与底层各个注册器的关系
- `Apply()` 到底做了什么
- 什么时候该用链式构建器，什么时候该直接用注册器
- 固定模型身份与 ModelDb 集成是怎样建立在注册之上的

---

## 注册器总览

RitsuLib 按职责拆分了几类注册器：

| 注册器 | 作用 |
|---|---|
| `ModContentRegistry` | 注册角色、卡牌、遗物、药水、能力、球体、Act、事件、Ancient 等模型 |
| `ModKeywordRegistry` | 注册可复用关键词定义 |
| `ModTimelineRegistry` | 注册 `Story` 与 `Epoch` |
| `ModUnlockRegistry` | 注册纪元门槛与进度解锁规则 |

`CreateContentPack(modId)` 就是把这四类能力打包成一个更顺手的入口。

---

## `CreateContentPack(...)`

推荐默认使用链式构建器：

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    .Character<MyCharacter>()
    .Card<MyCardPool, MyCard>()
    .Relic<MyRelicPool, MyRelic>()
    .CardKeyword("brew", locKeyPrefix: "my_mod_brew")
    .Epoch<MyCharacterEpoch>()
    .Story<MyStory>()
    .RequireEpoch<MyLateCard, MyCharacterEpoch>()
    .Apply();
```

但需要明确的是，它不会：

- 自动反射扫描内容
- 自动替你重排注册顺序
- 取代底层注册器的存在

它只是把一系列注册步骤按加入顺序记录下来，并在 `Apply()` 时顺序执行。

---

## `ModContentPackContext`

`Apply()` 返回 `ModContentPackContext`，里面包含：

- `Content`
- `Keywords`
- `Timeline`
- `Unlocks`

也就是说，构建器可以作为主要入口，同时你在需要时仍然可以拿到原始注册器继续操作。

---

## 步骤顺序

构建器中的步骤严格按添加顺序执行。

这点在以下场景会很重要：

- 某个 `Custom(ctx => ...)` 依赖前面已经注册的内容
- 你希望日志顺序能准确反映初始化流程
- 你在同一个 chain 中混合内容注册与自定义逻辑

`CreateContentPack` 故意保持显式，它是“顺序执行的注册脚本”，而不是“自动推断依赖关系的求解器”。

---

## 构建器能做什么

构建器支持的步骤大致包括：

- 内容模型注册
- 关键词注册
- 时间线注册
- 解锁注册
- 清单式注册
- 任意自定义回调

一些不那么显眼，但很实用的入口包括：

- `Entry(IContentRegistrationEntry)`
- `Entries(IEnumerable<IContentRegistrationEntry>)`
- `Keyword(KeywordRegistrationEntry)`
- `Keywords(IEnumerable<KeywordRegistrationEntry>)`
- `Manifest(contentEntries, keywordEntries)`
- `Custom(Action<ModContentPackContext>)`

如果你希望“注册声明本身也是数据”，这些入口会很好用。

---

## 什么时候直接使用注册器

默认优先使用 `CreateContentPack(...)`。

但以下情况直接使用注册器更合适：

- 注册逻辑拆分在多个模块里
- 你希望在自己的前置库里再包装一层 API
- 你不想把所有注册都塞进一条长链
- 你要程序化生成注册项

典型写法如下：

```csharp
var content = RitsuLibFramework.GetContentRegistry("MyMod");
content.RegisterCharacter<MyCharacter>();

var timeline = RitsuLibFramework.GetTimelineRegistry("MyMod");
timeline.RegisterEpoch<MyEpoch>();
```

这些注册器是一等公民 API，不是构建器背后的私有实现细节。

---

## 内容注册器的职责

`ModContentRegistry` 主要负责：

- 记录某个模型类型归属于哪个 Mod
- 校验重复注册与冲突
- 提供给 ModelDb 补丁使用的追加模型序列
- 为已注册类型生成固定公开 `ModelId.Entry`

这套归属跟踪很关键，因为它让 RitsuLib 可以安全回答这些问题：

- 某个类型是谁注册的？
- 它的固定公开条目标识应该是什么？
- 某些兼容逻辑是否应该把它当作 Mod 内容处理？

---

## 固定公开身份

对于通过 RitsuLib 注册的模型，公开 `ModelId.Entry` 会被强制成稳定格式：

```text
<MODID>_<CATEGORY>_<TYPENAME>
```

这不是靠改你源码里的类型名实现的，而是通过 ModelDb 身份补丁在公开入口上统一的。

这么做的意义在于：

- 本地化 Key 可预测
- 默认资源路径约定更稳定
- 补丁、存档、兼容逻辑里都更容易识别内容归属

这条规则只作用于显式通过 RitsuLib 注册的类型。

---

## ModelDb 集成

仅仅完成注册还不够，游戏本身还必须“看得到”这些内容。

RitsuLib 通过对 ModelDb 及相关访问点打补丁来完成这件事，包括：

- 追加已注册的角色、Act、能力、球体、事件、Ancient
- 对已注册模型类型强制固定公开条目标识
- 在缓存锁定前引导动态 Act 内容补丁

这也是为什么注册必须发生在框架冻结之前。

---

## Freeze 行为

几个关键注册器都会在早期初始化后冻结：

- 内容注册冻结
- 时间线注册冻结
- 解锁注册冻结

冻结之后再注册会直接抛异常。

这是有意为之，因为框架追求的是：

- 身份稳定
- 模型列表稳定
- 解锁/过滤行为稳定

如果某个 Mod 在太晚的时候才注册内容，最安全的结果就是尽早失败，而不是让游戏带着半成品缓存继续跑下去。

---

## Manifest 与 Entry 对象

如果你希望把注册描述成数据，可以使用注册条目对象：

```csharp
var contentEntries = new IContentRegistrationEntry[]
{
    new CharacterRegistrationEntry<MyCharacter>(),
    new CardRegistrationEntry<MyCardPool, MyCard>(),
};

var keywordEntries = new[]
{
    KeywordRegistrationEntry.Card("brew", "my_mod_brew"),
};

RitsuLibFramework.CreateContentPack("MyMod")
    .Manifest(contentEntries, keywordEntries)
    .Apply();
```

这对“声明式注册列表”或“跨模块复用注册清单”的场景会很方便。

---

## 推荐注册模式

对大多数 Mod，建议这样组织：

1. 在初始化入口中创建一个内容包
2. 在其中注册所有内容、关键词、时间线节点与解锁规则
3. `Custom(...)` 保持小而显式
4. 不要把注册拖到运行期 hook 再做

如果 Mod 很大，可以保留一个顶层构建器，再从子模块喂注册条目对象或辅助方法进去。

---

## 相关文档

- [内容注册规则](ContentAuthoringToolkit.md)
- [时间线与解锁](TimelineAndUnlocks.md)
- [框架设计](FrameworkDesign.md)
