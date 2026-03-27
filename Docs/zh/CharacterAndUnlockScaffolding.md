# 角色与解锁脚手架

本文是角色 Mod 的实践搭建指南：角色模板、内容池定义、纪元模板与解锁注册，并附完整示例。

更细的回退规则见 [资源配置与回退规则](AssetProfilesAndFallbacks.md)，更细的时间线与进度语义见 [时间线与解锁](TimelineAndUnlocks.md)。涉及角色视觉场景、休息点、能量球等场景脚本包装时，请继续看 [Godot 场景编写说明](GodotSceneAuthoring.md)。

---

## 概览

一个完整的角色 Mod 通常包含以下部分：

| 内容 | 基类 | 示例 |
|---|---|---|
| 卡池 | `TypeListCardPoolModel` | `MyCardPool` |
| 遗物池 | `TypeListRelicPoolModel` | `MyRelicPool` |
| 药水池 | `TypeListPotionPoolModel` | `MyPotionPool` |
| 角色 | `ModCharacterTemplate<TCard, TRelic, TPotion>` | `MyCharacter` |
| 故事 | `ModStoryTemplate` | `MyStory` |
| 纪元 | `CharacterUnlockEpochTemplate<T>` 或自定义 | `MyEpoch2` |

---

## 内容池定义

- **卡池**：`TypeListCardPoolModel` 的池成员在 `CreateContentPack` / Manifest 中通过 `.Card<卡池, 卡牌>()` / `CardRegistrationEntry` 登记；基类已提供默认空的 `CardTypes`（`[Obsolete]`），**无需覆写**。
- **遗物池 / 药水池**：仍通过 `TypeListRelicPoolModel` / `TypeListPotionPoolModel` 的 `RelicTypes` / `PotionTypes` 列举（或配合内容包 `RegisterRelic` / `RegisterPotion`，勿对同一模型重复两种来源）。

```csharp
using Godot;

public class MyCardPool : TypeListCardPoolModel
{
    public override string Title => "My Pool";
    public override string EnergyColorName => "orange";
    public override string CardFrameMaterialPath => "card_frame_orange";
    public override Color DeckEntryCardColor => new("d2a15a");
    public override bool IsColorless => false;
}

public class MyRelicPool : TypeListRelicPoolModel
{
    protected override IEnumerable<Type> RelicTypes =>
    [
        typeof(MyStarterRelic),
    ];
}

public class MyPotionPool : TypeListPotionPoolModel
{
    protected override IEnumerable<Type> PotionTypes => [];
}
```

**卡牌与 `CardTypes`（过时钩子）：** 不要再覆写 `CardTypes` 来列牌；若旧代码仍覆写并列举类型，会得到 **CS0618**，且与内容包注册叠用时 `AllCards` 仍会重复。迁移方式为删除覆写、仅保留内容包注册。遗物池、药水池仍避免「`RelicTypes`/`PotionTypes` + 同一模型的 `RegisterRelic`/`RegisterPotion`」双登记。

### 配置卡牌边框颜色（HSV）

`TypeListCardPoolModel` 支持直接覆盖 `PoolFrameMaterial`。当该属性返回非空材质时，会优先使用这个材质渲染卡牌边框，不再依赖 `CardFrameMaterialPath`。

```csharp
using Godot;
using STS2RitsuLib.Utils;

public class MyCardPool : TypeListCardPoolModel
{
    // 卡牌在 CreateContentPack / Manifest 中注册；勿覆写 CardTypes

    // 直接用 HSV 生成边框材质：H=0.55, S=0.45, V=0.95
    public override Material? PoolFrameMaterial =>
        MaterialUtils.CreateHsvShaderMaterial(0.55f, 0.45f, 0.95f);
}
```

若你希望继续走资源路径模式，也可以不覆盖 `PoolFrameMaterial`，仅覆盖 `CardFrameMaterialPath`。

### 示例：配置池能量图标

`TypeList*PoolModel` 现在也支持统一配置能量图标：

- `BigEnergyIconPath`：通过 `EnergyIconHelper` 解析的大图标
- `TextEnergyIconPath`：卡牌描述富文本里使用的小图标

```csharp
public class MyCardPool : TypeListCardPoolModel
{
    public override string? BigEnergyIconPath => "res://MyMod/ui/energy/my_energy_big.png";
    public override string? TextEnergyIconPath => "res://MyMod/ui/energy/my_energy_text.png";
}
```

---

## 角色模板

继承 `ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>`，指定三个池类型参数，并声明初始牌组以及你真正想替换的资源即可。

未填写的角色资源会自动回退到 `PlaceholderCharacterId`，默认值为 `ironclad`。

```csharp
public class MyCharacter : ModCharacterTemplate<MyCardPool, MyRelicPool, MyPotionPool>
{
    // 初始牌组（框架自动按类型解析为 ModelId）
    protected override IEnumerable<Type> StartingDeckTypes =>
    [
        typeof(MyStrike), typeof(MyStrike), typeof(MyStrike),
        typeof(MyDefend), typeof(MyDefend),
    ];

    // 起始遗物
    protected override IEnumerable<Type> StartingRelicTypes =>
    [
        typeof(MyStarterRelic),
    ];

    public override string? PlaceholderCharacterId => "ironclad";

    // 资源路径（使用 AssetProfile 统一配置）
    public override CharacterAssetProfile AssetProfile => new(
        Spine: new(
            CombatSkeletonDataPath: "res://MyMod/spine/my_character.tres"),
        Ui: new(
            IconTexturePath: "res://MyMod/art/icon.png",
            CharacterSelectBgPath: "res://MyMod/art/select_bg.tscn"),
        Scenes: new(
            RestSiteAnimPath: "res://MyMod/scenes/rest_site/my_character_rest_site.tscn"));
}
```

如果你更想继承 `silent`、`defect` 等角色的商人 / 休息点 / 小地图 / 默认音效风格，可以改写 `PlaceholderCharacterId`。若你想关闭这层兜底，可返回 `null`。

---

## 故事模板

继承 `ModStoryTemplate`，定义一个故事节点以及它在时间线中展开的纪元序列：

```csharp
public class MyStory : ModStoryTemplate
{
    protected override string StoryKey => "my-character";

    protected override IEnumerable<Type> EpochTypes =>
    [
        typeof(MyCharacterEpoch),
        typeof(MyEpoch2),
    ];
}
```

### Ancient 对话本地化

RitsuLib 会在游戏原版 `AncientDialogueSet.PopulateLocKeys` 之前，自动为已注册的 Mod 角色追加基于本地化定义的 Ancient 对话。

键格式与原版保持一致：

| 键组件 | 说明 |
|---|---|
| `<ancientEntry>.talk.<characterEntry>.<dialogueIndex>-<lineIndex>.ancient` | Ancient 台词 |
| `<ancientEntry>.talk.<characterEntry>.<dialogueIndex>-<lineIndex>.char` | 角色台词 |
| 可选后缀 `.sfx` | 音效 |
| 可选后缀 `-visit` | 访问覆盖 |
| 可选后缀 `-attack` | Architect 专用攻击者覆盖 |
| 可选后缀 `r` | 重复对话 |

如果需要直接操作工具方法，可使用 `STS2RitsuLib.Localization.AncientDialogueLocalization`。

---

## 纪元模板

RitsuLib 提供预置的纪元模板，用于常见解锁目标：

| 模板 | 说明 |
|---|---|
| `CharacterUnlockEpochTemplate<TCharacter>` | 解锁角色本身的纪元 |
| `CardUnlockEpochTemplate` | 解锁额外卡牌的纪元 |
| `RelicUnlockEpochTemplate` | 解锁额外遗物的纪元 |
| `PotionUnlockEpochTemplate` | 解锁额外药水的纪元 |

```csharp
public class MyCharacterEpoch : CharacterUnlockEpochTemplate<MyCharacter>
{
}

public class MyEpoch2 : CardUnlockEpochTemplate
{
    protected override IEnumerable<Type> CardTypes =>
    [
        typeof(MyAdvancedCard),
    ];
}
```

---

## 完整注册示例

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    // 卡牌（指定所属池）
    .Card<MyCardPool, MyStrike>()
    .Card<MyCardPool, MyDefend>()
    .Card<MyCardPool, MySignatureCard>()
    .Card<MyCardPool, MyAdvancedCard>()

    // 遗物
    .Relic<MyRelicPool, MyStarterRelic>()

    // 角色
    .Character<MyCharacter>()

    // 故事与纪元
    .Story<MyStory>()
    .Epoch<MyCharacterEpoch>()
    .Epoch<MyEpoch2>()

    // 解锁规则
    .RequireEpoch<MyAdvancedCard, MyEpoch2>()       // 纪元 2 才显示该卡
    .UnlockEpochAfterRunAs<MyCharacter, MyEpoch2>() // 完成一局后解锁纪元 2

    .Apply();
```

---

## 模型 ID 与本地化

通过 RitsuLib 注册的角色模型遵循与其他内容相同的 `ModelId.Entry` 规则（参见 [内容注册规则](ContentAuthoringToolkit.md)）。

示例（Mod id `MyMod`，类型 `MyCharacter`）：
- `ModelId.Entry` → `MY_MOD_CHARACTER_MY_CHARACTER`
- 本地化 Key → `MY_MOD_CHARACTER_MY_CHARACTER.title`

> 重命名 CLR 类型会改变其推导出的 Entry，影响存档兼容性。发布后请勿随意重命名。

---

## 依赖规则

- 卡牌/遗物/药水类型必须在运行时模型查找发生前完成注册
- 角色引用的池类型必须已经注册
- 所有模型（包括受解锁条件限制的内容）均必须完成注册，解锁规则**不**替代注册

---

## 相关文档

- [内容注册规则](ContentAuthoringToolkit.md)
- [快速入门](GettingStarted.md)
- [时间线与解锁](TimelineAndUnlocks.md)
- [资源配置与回退规则](AssetProfilesAndFallbacks.md)
- [Godot 场景编写说明](GodotSceneAuthoring.md)
