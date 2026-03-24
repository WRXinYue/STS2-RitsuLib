# Timeline & Unlocks

This is the reference document for timeline registration and unlock semantics.

RitsuLib separates timeline registration from unlock rules, but the two systems are designed to work together.

This document explains:

- how stories and epochs are registered
- what the scaffold templates actually do
- how unlock rules are evaluated
- where compatibility patches bridge vanilla progression logic for mod characters

---

## The Two Registries

There are two related registries:

- `ModTimelineRegistry`: registers `StoryModel` and `EpochModel` types
- `ModUnlockRegistry`: defines when content or epochs become available

In the fluent builder they are exposed through:

- `.Story<TStory>()`
- `.Epoch<TEpoch>()`
- `.RequireEpoch<TModel, TEpoch>()`
- `.UnlockEpochAfter...()`

The important distinction is:

- timeline registration says what exists
- unlock registration says when it becomes available

---

## Story Registration

`ModTimelineRegistry.RegisterStory<TStory>()` registers a concrete `StoryModel` type.

The recommended base class is `ModStoryTemplate`:

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

What `ModStoryTemplate` does for you:

- derives the story id by slugifying `StoryKey`
- resolves `EpochTypes` into the `Epochs` array expected by the game

So the story template is really a thin bridge from a type list to the game's story model.

---

## Epoch Registration

`ModTimelineRegistry.RegisterEpoch<TEpoch>()` registers a concrete `EpochModel` type.

You can always write raw `EpochModel` subclasses, but RitsuLib also provides several scaffold bases:

- `CharacterUnlockEpochTemplate<TCharacter>`
- `CardUnlockEpochTemplate`
- `RelicUnlockEpochTemplate`
- `PotionUnlockEpochTemplate`

These templates do two jobs:

- generate the unlock queue logic for the timeline screen
- optionally expose follow-up expansions through `ExpansionEpochTypes`

---

## Character Epoch Template

`CharacterUnlockEpochTemplate<TCharacter>` is used when an epoch unlocks a character.

Its built-in behavior:

- queues a character unlock in `NTimelineScreen`
- writes the pending character unlock to save progress
- expands into any epochs returned by `ExpansionEpochTypes`

Use it when the epoch itself is the presentation step for unlocking the character.

---

## Card / Relic / Potion Epoch Templates

`CardUnlockEpochTemplate`, `RelicUnlockEpochTemplate`, and `PotionUnlockEpochTemplate` work similarly:

- you declare the unlocked model types
- the template resolves them from `ModelDb`
- `UnlockText` is generated from the resolved models
- `QueueUnlocks()` pushes the right unlock payload to the timeline screen

This lets you define timeline unlocks in terms of model types instead of hand-building the queue logic.

---

## Expansion Epochs

All unlock epoch templates support:

```csharp
protected virtual IEnumerable<Type> ExpansionEpochTypes => [];
```

If you return epoch types here, they are queued as timeline expansion once the current epoch resolves.

That is the main mechanism for building a sequence like:

- unlock character
- then reveal card unlocks
- then reveal relic unlocks

without manually wiring every transition in UI code.

---

## Registration Timing And Freeze

Both timeline and unlock registries freeze after early initialization.

Why:

- story ids must remain stable
- epoch ids must remain stable
- unlock filters and compatibility patches need a finalized rule set

So register stories, epochs, and unlock rules from your mod initializer.
Do not wait until later gameplay hooks.

---

## Requiring An Epoch For Content

Use `RequireEpoch<TModel, TEpoch>()` when a model exists, but should only appear after an epoch is obtained or revealed.

Typical uses:

- cards that should stay hidden before a progression milestone
- relics tied to a specific narrative branch
- shared ancients or events gated behind timeline progress

RitsuLib applies unlock filtering to multiple content access paths, including:

- `UnlockState.Characters`
- unlocked card/relic/potion pool queries
- shared ancient lists
- generated act events

So the rule is not just cosmetic; it affects what the game actually offers.

---

## Post-Run Epoch Rules

`ModUnlockRegistry` supports several post-run convenience APIs:

- `UnlockEpochAfterRunAs<TCharacter, TEpoch>()`
- `UnlockEpochAfterWinAs<TCharacter, TEpoch>()`
- `UnlockEpochAfterAscensionWin<TCharacter, TEpoch>(level)`
- `UnlockEpochAfterRunCount<TEpoch>(requiredRuns, requireVictory)`

These all compile down to `PostRunEpochUnlockRule`.

If you need more control, you can register a custom rule directly:

```csharp
registry.RegisterPostRunRule(
    PostRunEpochUnlockRule.Create(
        epochId: new MyEpoch().Id,
        description: "Unlock after any abandoned ascension-5 run",
        shouldUnlock: ctx => ctx.IsAbandoned && ctx.AscensionLevel >= 5));
```

The rule receives a `PostRunUnlockContext` with run result, character id, total run count, total wins, and ascension level.

---

## Counted Progression Rules

RitsuLib also supports progression rules backed by cumulative stats:

- `UnlockEpochAfterEliteVictories<TCharacter, TEpoch>(requiredEliteWins)`
- `UnlockEpochAfterBossVictories<TCharacter, TEpoch>(requiredBossWins)`
- `UnlockEpochAfterAscensionOneWin<TCharacter, TEpoch>()`
- `RevealAscensionAfterEpoch<TCharacter, TEpoch>()`
- `UnlockCharacterAfterRunAs<TCharacter, TEpoch>()`

These rules exist because vanilla progression checks are character-specific and do not naturally understand mod characters.

RitsuLib bridges that with compatibility patches that read the registered mod rules and apply equivalent progression behavior.

---

## Compatibility Patches

The unlock system relies on a few focused compatibility patches:

- elite-win epoch check bridge
- boss-win epoch check bridge
- ascension-one epoch bridge
- post-run character-unlock epoch bridge
- ascension reveal bridge

These are intentionally narrow.

RitsuLib does not try to replace all progression logic; it only intercepts the vanilla locations that would otherwise skip mod characters entirely.

This is why the registry stores rules in explicit maps keyed by `ModelId` rather than trying to infer progression from the timeline graph alone.

---

## Recommended Pattern

For a story-driven character mod, a good pattern is:

1. Register the character, pools, epochs, and story in one content pack
2. Use `CharacterUnlockEpochTemplate<TCharacter>` for the character unlock epoch
3. Use follow-up card/relic/potion epoch templates for content reveals
4. Gate late content with `RequireEpoch<TModel, TEpoch>()`
5. Register one or two clear progression rules instead of many overlapping ones

This keeps the timeline readable and the unlock logic easy to explain.

---

## Example Builder Flow

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

## Common Mistakes

- registering epochs but forgetting to register the story that exposes them
- registering stories after timeline freeze
- using `RequireEpoch` for content but never registering a rule that can actually obtain that epoch
- stacking many overlapping unlock rules for the same epoch without a clear reason
- assuming vanilla counted unlock logic will automatically work for mod characters without RitsuLib rule registration

---

## Related Documents

- [Character & Unlock Scaffolding](CharacterAndUnlockScaffolding.md)
- [Content Packs & Registries](ContentPacksAndRegistries.md)
- [Diagnostics & Compatibility](DiagnosticsAndCompatibility.md)
- [Framework Design](FrameworkDesign.md)
