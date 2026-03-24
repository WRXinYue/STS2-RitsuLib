# 时间线与解锁

本文是时间线注册与解锁语义的参考文档。

RitsuLib 把时间线注册和解锁规则拆成两个系统，但这两个系统本来就是配合使用的。

本文会说明：

- `Story` / `Epoch` 是怎么注册的
- 脚手架模板到底帮你做了什么
- 解锁规则是怎样被判定的
- 为什么需要兼容补丁去桥接原版对 Mod 角色不完整的进度逻辑

---

## 两个注册器

这里涉及两个相关注册器：

- `ModTimelineRegistry`：注册 `StoryModel` 和 `EpochModel`
- `ModUnlockRegistry`：定义内容或纪元什么时候解锁

在链式构建器里，它们分别对应：

- `.Story<TStory>()`
- `.Epoch<TEpoch>()`
- `.RequireEpoch<TModel, TEpoch>()`
- `.UnlockEpochAfter...()`

核心区别是：

- 时间线注册回答“这个东西是否存在”
- 解锁注册回答“它什么时候可用”

---

## `Story` 注册

`ModTimelineRegistry.RegisterStory<TStory>()` 用于注册具体的 `StoryModel` 类型。

推荐直接使用 `ModStoryTemplate`：

```csharp
public class MyStory : ModStoryTemplate
{
    protected override string StoryKey => "my-story";

    protected override IEnumerable<Type> EpochTypes =>
    [
        typeof(MyCharacterEpoch),
        typeof(MyCardEpoch),
    ];
}
```

`ModStoryTemplate` 帮你做了两件事：

- 通过 `StoryKey` 自动生成规范化的故事标识
- 把 `EpochTypes` 解析成游戏需要的 `Epochs` 数组

所以它本质上是一个“从类型列表到 `StoryModel`”的轻量桥接层。

---

## `Epoch` 注册

`ModTimelineRegistry.RegisterEpoch<TEpoch>()` 用于注册具体的 `EpochModel` 类型。

你当然可以直接写原生 `EpochModel` 子类，但 RitsuLib 也提供了几种常用脚手架：

- `CharacterUnlockEpochTemplate<TCharacter>`
- `CardUnlockEpochTemplate`
- `RelicUnlockEpochTemplate`
- `PotionUnlockEpochTemplate`

这些模板主要负责两件事：

- 生成时间线界面的解锁入队逻辑
- 通过 `ExpansionEpochTypes` 支持后续纪元展开

---

## 角色解锁纪元模板

`CharacterUnlockEpochTemplate<TCharacter>` 用于“这个纪元本身就是角色解锁展示节点”的场景。

它的内置行为包括：

- 向 `NTimelineScreen` 队列一个角色解锁
- 把待解锁角色写入进度存档
- 若配置了 `ExpansionEpochTypes`，继续把后续纪元加入时间线展开

如果你的某个纪元本身就是角色解锁步骤，用它最合适。

---

## 卡牌 / 遗物 / 药水纪元模板

`CardUnlockEpochTemplate`、`RelicUnlockEpochTemplate`、`PotionUnlockEpochTemplate` 的工作方式相似：

- 你声明解锁的模型类型
- 模板通过 `ModelDb` 解析这些类型
- `UnlockText` 自动根据解析结果生成
- `QueueUnlocks()` 自动把对应的解锁内容推入时间线界面

这让你可以直接用“类型列表”描述 Timeline 解锁，而不用自己手搓队列逻辑。

---

## Expansion Epochs

所有这些解锁纪元模板都支持：

```csharp
protected virtual IEnumerable<Type> ExpansionEpochTypes => [];
```

只要这里返回纪元类型，当前纪元完成时就会自动把这些纪元作为时间线扩展加入。

这是组织这种解锁链的主要方式：

- 先解锁角色
- 再展开卡牌解锁
- 再展开遗物解锁

而不需要手动去写 UI 层的衔接代码。

---

## 注册时机与冻结

时间线和解锁两个注册器都会在早期初始化后被冻结。

原因是：

- 故事标识必须稳定
- 纪元标识必须稳定
- 解锁过滤与兼容补丁需要面对一套最终确定的规则表

因此，`Story`、`Epoch` 和解锁规则都应该在初始化入口中注册。
不要拖到更晚的运行期逻辑里再做。

---

## 为内容设置 Epoch 门槛

当一个模型本来就存在，但应该在某个纪元解锁后才出现时，使用 `RequireEpoch<TModel, TEpoch>()`。

常见用途：

- 某些后期卡牌在进度达成前不应进入牌池
- 某些遗物只在特定故事分支后开放
- 某些共享 `Ancient` / 事件需要时间线进度门槛

RitsuLib 会把这类门槛应用到多个访问入口，包括：

- `UnlockState.Characters`
- 卡牌 / 遗物 / 药水的已解锁池查询
- 共享 `Ancient` 列表
- Act 生成出来的事件列表

也就是说，这不是单纯 UI 过滤，而是真正影响游戏可提供内容的规则。

---

## 局后 `Epoch` 规则

`ModUnlockRegistry` 提供了一组常用的局后便捷 API：

- `UnlockEpochAfterRunAs<TCharacter, TEpoch>()`
- `UnlockEpochAfterWinAs<TCharacter, TEpoch>()`
- `UnlockEpochAfterAscensionWin<TCharacter, TEpoch>(level)`
- `UnlockEpochAfterRunCount<TEpoch>(requiredRuns, requireVictory)`

这些最终都会转成 `PostRunEpochUnlockRule`。

如果你需要更复杂的条件，也可以直接注册自定义规则：

```csharp
unlocks.RegisterPostRunRule(
    PostRunEpochUnlockRule.Create(
        epochId: new MyEpoch().Id,
        description: "在任意一次被放弃的 5 层进阶局后解锁",
        shouldUnlock: ctx => ctx.IsAbandoned && ctx.AscensionLevel >= 5));
```

`PostRunUnlockContext` 会提供对局结果、角色标识、总跑图次数、总胜场和进阶层数等信息。

---

## 累计进度型规则

RitsuLib 还支持一组基于累计统计的规则：

- `UnlockEpochAfterEliteVictories<TCharacter, TEpoch>(requiredEliteWins)`
- `UnlockEpochAfterBossVictories<TCharacter, TEpoch>(requiredBossWins)`
- `UnlockEpochAfterAscensionOneWin<TCharacter, TEpoch>()`
- `RevealAscensionAfterEpoch<TCharacter, TEpoch>()`
- `UnlockCharacterAfterRunAs<TCharacter, TEpoch>()`

这些规则存在的原因是：原版的相关进度检查是按原生角色设计的，并不会自然支持 Mod 角色。

RitsuLib 通过兼容补丁读取你注册的规则，再补上一套等价的进度判定逻辑。

---

## 兼容补丁

当前解锁系统依赖几类较窄的兼容补丁：

- 精英击杀计数的纪元判定桥接
- Boss 击杀计数的纪元判定桥接
- 进阶 1 的纪元判定桥接
- 局后角色解锁纪元桥接
- 进阶显示解锁判定桥接

这些补丁是刻意收敛的。

RitsuLib 并不想重写整套原版进度系统，只是在原版会彻底跳过 Mod 角色的节点上补一层桥。

这也是为什么解锁注册器会显式地按 `ModelId` 保存规则，而不是试图仅从时间线图结构里推断全部进度逻辑。

---

## 推荐模式

对一个故事驱动型角色 Mod，比较推荐这样组织：

1. 在一个内容包里注册角色、池、纪元和故事
2. 用 `CharacterUnlockEpochTemplate<TCharacter>` 作为角色解锁纪元
3. 用卡牌 / 遗物 / 药水纪元模板做后续内容展开
4. 用 `RequireEpoch<TModel, TEpoch>()` 给后期内容加门槛
5. 用少量清晰的进度规则，而不是堆很多重叠规则

这样时间线更好读，解锁逻辑也更容易说明。

---

## 构建器示例

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    .Character<MyCharacter>()
    .Card<MyCardPool, MyLateCard>()
    .Relic<MyRelicPool, MyLateRelic>()
    .Epoch<MyCharacterEpoch>()
    .Epoch<MyLateContentEpoch>()
    .Story<MyStory>()
    .RequireEpoch<MyLateCard, MyLateContentEpoch>()
    .RequireEpoch<MyLateRelic, MyLateContentEpoch>()
    .UnlockEpochAfterWinAs<MyCharacter, MyCharacterEpoch>()
    .UnlockEpochAfterAscensionWin<MyCharacter, MyLateContentEpoch>(10)
    .Apply();
```

---

## 常见错误

- 注册了纪元，却忘了注册暴露这些纪元的故事
- 在时间线冻结之后才注册故事 / 纪元
- 给内容设置了 `RequireEpoch`，却没有任何规则能真正获得该纪元
- 对同一个纪元叠很多重叠解锁规则，却没有明确设计理由
- 误以为原版累计进度逻辑会自动兼容 Mod 角色，而没有注册 RitsuLib 解锁规则

---

## 相关文档

- [角色与解锁脚手架](CharacterAndUnlockScaffolding.md)
- [内容包与注册器](ContentPacksAndRegistries.md)
- [诊断与兼容层](DiagnosticsAndCompatibility.md)
- [框架设计](FrameworkDesign.md)
