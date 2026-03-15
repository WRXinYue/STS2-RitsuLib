# Character and Unlock Scaffolding

## Character content registration

1. Register your pools, character, story, and epochs from your mod initializer.
2. Register epoch gates for cards / relics / potions / characters.
3. Optionally register post-run unlock rules for new epochs.

## Example outline

- Derive card / relic / potion pools from:
  - `TypeListCardPoolModel`
  - `TypeListRelicPoolModel`
  - `TypeListPotionPoolModel`
- Derive your character from:
  - `ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>`
- Derive your story from:
  - `ModStoryTemplate`
- Derive unlock epochs from:
  - `CharacterUnlockEpochTemplate<TCharacter>`
  - `CardUnlockEpochTemplate`
  - `RelicUnlockEpochTemplate`
  - `PotionUnlockEpochTemplate`

## Typical initializer flow

```csharp
var content = RitsuLibFramework.GetContentRegistry(MyMod.Id);
var timeline = RitsuLibFramework.GetTimelineRegistry(MyMod.Id);
var unlocks = RitsuLibFramework.GetUnlockRegistry(MyMod.Id);

content.RegisterCharacter<MyCharacter>();
content.RegisterCard<MyCharacterCardPool, MyUnlockedCard>();
content.RegisterRelic<MyCharacterRelicPool, MyUnlockedRelic>();
content.RegisterPotion<MyCharacterPotionPool, MyUnlockedPotion>();

timeline.RegisterStory<MyCharacterStory>();
timeline.RegisterEpoch<MyCharacter1Epoch>();
timeline.RegisterEpoch<MyCharacter2Epoch>();
timeline.RegisterEpoch<MyCharacter3Epoch>();
timeline.RegisterEpoch<MyCharacter4Epoch>();

unlocks.RequireEpoch<MyCharacter, MyCharacter1Epoch>();
unlocks.RequireEpoch<MyUnlockedCard, MyCharacter2Epoch>();
unlocks.RequireEpoch<MyUnlockedRelic, MyCharacter3Epoch>();
unlocks.RequireEpoch<MyUnlockedPotion, MyCharacter4Epoch>();

unlocks.UnlockEpochAfterRunAs<Ironclad, MyCharacter1Epoch>();
unlocks.UnlockEpochAfterWinAs<MyCharacter, MyCharacter2Epoch>();
unlocks.UnlockEpochAfterAscensionWin<MyCharacter, MyCharacter3Epoch>(1);
```

## Collision note

`ModelDb` derives `ModelId` from the model category plus the slugified CLR type name. Two model classes with the same type name in the same model category will collide, even if they live in different namespaces or different mods.
