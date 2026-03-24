# Character & Unlock Scaffolding

This document is the practical assembly guide for a character mod: character templates, content pools, epoch templates, and unlock registration with full examples.

Detailed fallback semantics live in [Asset Profiles & Fallbacks](AssetProfilesAndFallbacks.md). Detailed timeline and progression semantics live in [Timeline & Unlocks](TimelineAndUnlocks.md). For scene-script wrappers used by visuals, rest sites, and counters, see [Godot Scene Authoring](GodotSceneAuthoring.md).

---

## Overview

A full character mod typically includes:

| Content | Base Type | Example |
|---|---|---|
| Card pool | `TypeListCardPoolModel` | `MyCardPool` |
| Relic pool | `TypeListRelicPoolModel` | `MyRelicPool` |
| Potion pool | `TypeListPotionPoolModel` | `MyPotionPool` |
| Character | `ModCharacterTemplate<TCard, TRelic, TPotion>` | `MyCharacter` |
| Story | `ModStoryTemplate` | `MyStory` |
| Epoch | `CharacterUnlockEpochTemplate<T>` or custom | `MyEpoch2` |

---

## Pools

Use `TypeList*PoolModel` to declare pool contents by type — no manual `ModelId` handling required:

```csharp
public class MyCardPool : TypeListCardPoolModel
{
    protected override IEnumerable<Type> CardTypes =>
    [
        typeof(MyStrike),
        typeof(MyDefend),
        typeof(MySignatureCard),
    ];
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
    // Leave empty if the character has no exclusive potions
    protected override IEnumerable<Type> PotionTypes => [];
}
```

### Configure Card Frame Color (HSV)

`TypeListCardPoolModel` supports directly overriding `PoolFrameMaterial`. When this property returns a non-null material, that material is used for card frame rendering and `CardFrameMaterialPath` is no longer required.

```csharp
using Godot;
using STS2RitsuLib.Utils;

public class MyCardPool : TypeListCardPoolModel
{
    protected override IEnumerable<Type> CardTypes =>
    [
        typeof(MyStrike),
        typeof(MyDefend),
    ];

    // Generate a frame material from HSV: H=0.55, S=0.45, V=0.95
    public override Material? PoolFrameMaterial =>
        MaterialUtils.CreateHsvShaderMaterial(0.55f, 0.45f, 0.95f);
}
```

If you prefer path-based configuration, simply leave `PoolFrameMaterial` as `null` and override `CardFrameMaterialPath` instead.

### Example: Configure Pool Energy Icons

`TypeList*PoolModel` also exposes pooled energy icon hooks:

- `BigEnergyIconPath`: the large icon resolved through `EnergyIconHelper`
- `TextEnergyIconPath`: the small inline icon used in rich-text card descriptions

```csharp
public class MyCardPool : TypeListCardPoolModel
{
    protected override IEnumerable<Type> CardTypes =>
    [
        typeof(MyStrike),
        typeof(MyDefend),
    ];

    public override string? BigEnergyIconPath => "res://MyMod/ui/energy/my_energy_big.png";
    public override string? TextEnergyIconPath => "res://MyMod/ui/energy/my_energy_text.png";
}
```

---

## Character Template

Inherit `ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>` and provide the starting deck plus any custom assets you actually want to replace.

Unspecified character assets automatically fall back to `PlaceholderCharacterId`, which defaults to `ironclad`.

```csharp
public class MyCharacter : ModCharacterTemplate<MyCardPool, MyRelicPool, MyPotionPool>
{
    protected override IEnumerable<Type> StartingDeckTypes =>
    [
        typeof(MyStrike), typeof(MyStrike), typeof(MyStrike),
        typeof(MyDefend), typeof(MyDefend),
    ];

    protected override IEnumerable<Type> StartingRelicTypes =>
    [
        typeof(MyStarterRelic),
    ];

    public override string? PlaceholderCharacterId => "ironclad";

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

Override `PlaceholderCharacterId` with another base character such as `silent` or `defect` if you want their merchant / rest-site / map marker / default SFX alignment instead. Return `null` if you want strict no-fallback behavior.

---

## Story Template

Inherit `ModStoryTemplate` to define a story node and the epoch sequence it exposes on the timeline:

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

### Ancient Dialogue Localization

RitsuLib now auto-appends localization-defined ancient dialogues for registered mod characters before `AncientDialogueSet.PopulateLocKeys` runs.

Use the same key pattern as the base game:

- dialogue lines: `<ancientEntry>.talk.<characterEntry>.<dialogueIndex>-<lineIndex>[r].ancient|char`
- optional SFX: append `.sfx`
- optional visit override: append `-visit`
- architect-only attack override: append `-attack`

If you need the helpers directly, see `STS2RitsuLib.Localization.AncientDialogueLocalization`.

---

## Epoch Templates

RitsuLib provides pre-built epoch templates for common unlock targets:

| Template | Purpose |
|---|---|
| `CharacterUnlockEpochTemplate<TCharacter>` | Epoch that unlocks the character itself |
| `CardUnlockEpochTemplate` | Epoch that unlocks additional cards |
| `RelicUnlockEpochTemplate` | Epoch that unlocks additional relics |
| `PotionUnlockEpochTemplate` | Epoch that unlocks additional potions |

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

## Full Registration Example

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    // Cards (specify the owning pool)
    .Card<MyCardPool, MyStrike>()
    .Card<MyCardPool, MyDefend>()
    .Card<MyCardPool, MySignatureCard>()
    .Card<MyCardPool, MyAdvancedCard>()

    // Relics
    .Relic<MyRelicPool, MyStarterRelic>()

    // Character
    .Character<MyCharacter>()

    // Story and epoch
    .Story<MyStory>()
    .Epoch<MyCharacterEpoch>()
    .Epoch<MyEpoch2>()

    // Unlock rules
    .RequireEpoch<MyAdvancedCard, MyEpoch2>()       // hide card until epoch 2
    .UnlockEpochAfterRunAs<MyCharacter, MyEpoch2>() // unlock epoch 2 after one completed run

    .Apply();
```

---

## Model ID and Localization

Character models follow the same fixed `ModelId.Entry` rule as all other content (see [Content Authoring Toolkit](ContentAuthoringToolkit.md)).

Example — mod id `MyMod`, type `MyCharacter`:
- `ModelId.Entry` → `MY_MOD_CHARACTER_MY_CHARACTER`
- Localization key → `MY_MOD_CHARACTER_MY_CHARACTER.title`

> Renaming a CLR type changes its derived entry. Avoid renaming types after they have been published.

---

## Dependency Rules

- All card / relic / potion types referenced by a pool must be registered before runtime model lookup occurs.
- A character's referenced pool types must all be registered.
- Every model — including epoch-gated content — must still be registered. Unlock rules do not replace registration.

---

## Related Documents

- [Content Authoring Toolkit](ContentAuthoringToolkit.md)
- [Getting Started](GettingStarted.md)
- [Timeline & Unlocks](TimelineAndUnlocks.md)
- [Asset Profiles & Fallbacks](AssetProfilesAndFallbacks.md)
- [Godot Scene Authoring](GodotSceneAuthoring.md)
