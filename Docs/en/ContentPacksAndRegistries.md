# Content Packs & Registries

This document is the reference for how RitsuLib registration is organized.

It covers:

- the relationship between `CreateContentPack(...)` and the underlying registries
- what `Apply()` actually does
- when to use builder steps, manifests, or direct registry access
- how fixed model identity and ModelDb integration relate to registration

---

## Registry Map

RitsuLib keeps registration responsibilities split by concern:

| Registry | Purpose |
|---|---|
| `ModContentRegistry` | Register models such as characters, cards, relics, potions, powers, orbs, acts, events, ancients |
| `ModKeywordRegistry` | Register reusable keyword definitions |
| `ModTimelineRegistry` | Register stories and epochs |
| `ModUnlockRegistry` | Register epoch requirements and progression rules |

`CreateContentPack(modId)` is the convenience layer that coordinates all four.

---

## `CreateContentPack(...)`

The fluent builder is the recommended entry point:

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

What the builder does not do:

- it does not auto-discover content by reflection
- it does not reorder your steps for you
- it does not replace the underlying registries

It simply records registration steps and runs them in insertion order when `Apply()` is called.

---

## `ModContentPackContext`

`Apply()` returns a `ModContentPackContext` containing:

- `Content`
- `Keywords`
- `Timeline`
- `Unlocks`

That means the fluent builder can be your main registration path, while still letting you access the raw registries afterward.

---

## Step Ordering

Builder steps execute in the order you add them.

That matters when:

- your custom step expects a registry entry to already exist
- you mix builder calls with `Custom(ctx => ...)`
- you want logs to reflect a specific setup flow

`CreateContentPack` is intentionally explicit here. It is a sequenced registration script, not a dependency solver.

---

## Builder Surface

The builder supports several kinds of steps:

- content model registration
- keyword registration
- timeline registration
- unlock registration
- manifest-driven registration
- arbitrary custom callbacks

Less obvious helpers that are still useful:

- `Entry(IContentRegistrationEntry)`
- `Entries(IEnumerable<IContentRegistrationEntry>)`
- `Keyword(KeywordRegistrationEntry)`
- `Keywords(IEnumerable<KeywordRegistrationEntry>)`
- `Manifest(contentEntries, keywordEntries)`
- `Custom(Action<ModContentPackContext>)`

These are useful when you want registration declared as data instead of written inline in one long chain.

---

## When To Use The Raw Registries

Use `CreateContentPack(...)` by default.

Use raw registries directly when:

- registration is split across several modules
- you want to expose registration helpers from your own library layer
- you need registry access without committing to a single fluent chain
- you are generating registration entries programmatically

Typical direct access looks like:

```csharp
var content = RitsuLibFramework.GetContentRegistry("MyMod");
content.RegisterCharacter<MyCharacter>();

var timeline = RitsuLibFramework.GetTimelineRegistry("MyMod");
timeline.RegisterEpoch<MyEpoch>();
```

The registries are first-class APIs, not implementation details.

---

## What The Content Registry Owns

`ModContentRegistry` is responsible for:

- recording which model types belong to which mod
- validating ownership and duplicate registration
- feeding ModelDb integration: global accessors such as `AllCharacters`, acts, powers, orbs, shared events, ancients, and **shared card pool types** are appended via patches; **per-pool** cards/relics/potions are merged through `ModHelper.AddModelToPool` when each pool expands `AllCards` / `AllRelics` / `AllPotions` (a different code path than those global appenders)
- generating fixed public `ModelId.Entry` values for registered types

That owner tracking is what lets RitsuLib safely answer questions like:

- which mod registered this type?
- what should its fixed public entry be?
- should vanilla progression/compatibility logic treat this as modded content?

---

## Fixed Public Identity

For RitsuLib-registered models, public `ModelId.Entry` is forced into a stable format:

```text
<MODID>_<CATEGORY>_<TYPENAME>
```

This is applied through the ModelDb identity patch, not by changing your CLR type names at source.

Why it matters:

- localization keys become deterministic
- default asset conventions become predictable
- model ownership remains clear across patches and saves

The identity rule applies only to types explicitly registered through RitsuLib.

---

## ModelDb Integration

Registration alone is not enough; the game still needs to see the content.

RitsuLib patches ModelDb and related model access points to:

- append registered characters, acts, powers, orbs, events, ancients, and shared card pools where applicable
- attach registered cards/relics/potions to their **target pools** via `ModHelper.AddModelToPool` (concatenated when each pool materializes its `All*` sequence)
- force fixed public entries for registered model types
- bootstrap dynamic act-content patching before caches lock in

This is why registration must happen before the framework freeze points.

---

## Freeze Behavior

The relevant registries freeze after early initialization:

- content registration freeze
- timeline registration freeze
- unlock registration freeze

Once frozen, later registration attempts throw.

This is intentional because the framework wants:

- stable identity
- stable model lists
- deterministic unlock/filter behavior

If a mod registers content late, the safest outcome is to fail early rather than let the game build partial caches.

---

## Manifests And Entry Objects

If you want registration to be declared as data, you can package it into entry objects:

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

This is useful when you want a declarative registration list or want to share registration bundles across modules.

---

## Recommended Registration Pattern

For most mods:

1. create one content pack in the mod initializer
2. register all content, keywords, timeline nodes, and unlock rules there
3. keep `Custom(...)` steps small and explicit
4. avoid late registration from gameplay hooks
5. with `TypeListCardPoolModel`, register pool cards via `.Card<Pool, Card>()` or `CardRegistrationEntry`; **do not** override the obsolete `CardTypes` hook (the base already defaults to empty—see [Getting Started](GettingStarted.md))

If the mod grows large, keep the builder at the top level and feed it entry objects or helper methods from submodules.

---

## Related Documents

- [Content Authoring Toolkit](ContentAuthoringToolkit.md)
- [Timeline & Unlocks](TimelineAndUnlocks.md)
- [Framework Design](FrameworkDesign.md)
