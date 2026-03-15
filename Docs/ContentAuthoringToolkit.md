# Content Authoring Toolkit

## Goals

This toolkit is intended to reduce boilerplate for mod authors who need to:

- register content, keywords, stories, epochs, and unlock rules together
- configure assets from predictable paths instead of overriding many getters
- build cards / relics / powers / orbs / potions on top of shared templates

## New building blocks

### 1. Fluent content pack builder

Use [Scaffolding/Content/ModContentPackBuilder.cs](../Scaffolding/Content/ModContentPackBuilder.cs) through `RitsuLibFramework.CreateContentPack(modId)`:

```csharp
RitsuLibFramework.CreateContentPack(MyMod.Id)
    .Character<MyCharacter>()
    .Card<MyCardPool, MyStrike>()
    .Card<MyCardPool, MyDefend>()
    .Relic<MyRelicPool, MyStarterRelic>()
    .Potion<MyPotionPool, MyPotion>()
    .Power<MyPower>()
    .Orb<MyOrb>()
    .CardKeyword("digging", iconPath: "res://images/ui/keywords/digging.png")
    .Epoch<MyCharacter1Epoch>()
    .Story<MyCharacterStory>()
    .RequireEpoch<MyStrike, MyCharacter1Epoch>()
    .UnlockEpochAfterRunAs<Ironclad, MyCharacter1Epoch>()
    .Apply();
```

### 2. Asset profiles

Use [Scaffolding/Content/ContentAssetProfiles.cs](../Scaffolding/Content/ContentAssetProfiles.cs) to opt into convention-based assets.

#### Card example

```csharp
public sealed class MyStrike : ModCardTemplate
{
    public MyStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.Enemy)
    {
    }

    public override CardAssetProfile AssetProfile =>
        ContentAssetProfiles.Card("winefox", "winefox_strike") with
        {
            FramePath = "res://images/ui/cards/winefox_attack_frame.png",
            PortraitBorderPath = "res://images/ui/cards/winefox_border.png"
        };
}
```

#### Relic example

```csharp
public sealed class MyRelic : ModRelicTemplate
{
    public override RelicAssetProfile AssetProfile =>
        ContentAssetProfiles.Relic("hand_crank");
}
```

#### Power example

```csharp
public sealed class MyPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile =>
        ContentAssetProfiles.Power("stress_power");
}
```

#### Orb example

```csharp
public sealed class MyOrb : ModOrbTemplate
{
    public override OrbAssetProfile AssetProfile =>
        ContentAssetProfiles.Orb("glass") with
        {
            VisualsScenePath = "res://scenes/orbs/custom_glass_orb.tscn"
        };
}
```

#### Potion example

```csharp
public sealed class MyPotion : ModPotionTemplate
{
    public override PotionAssetProfile AssetProfile =>
        ContentAssetProfiles.Potion("my_potion");
}
```

### 3. Card dynamic var toolkit

See [CardDynamicVarToolkit.md](CardDynamicVarToolkit.md).

RitsuLib only provides generic dynamic var creation helpers plus tooltip registration and cloning support.

## Runtime support now included

RitsuLib now patches the game to allow template-based overrides for:

- card portrait / beta portrait / frame / border / energy icon / frame material
- relic icon / outline / big icon
- power icon / big icon
- orb icon / visuals scene
- potion image / outline

## Recommended author flow

1. create pools using `TypeListCardPoolModel`, `TypeListRelicPoolModel`, `TypeListPotionPoolModel`
2. derive content from the `Mod*Template` base classes
3. configure predictable resources via `ContentAssetProfiles`
4. register everything in one chain with `CreateContentPack(modId)`
5. add unlock requirements and stories in the same builder chain

## Notes

- asset overrides only activate when the implementing type derives from the corresponding `Mod*Template` or otherwise implements the matching `IMod*AssetOverrides` interface
- all registration still must happen before content registration is frozen
- the builder is only a convenience layer; direct registry access remains supported
