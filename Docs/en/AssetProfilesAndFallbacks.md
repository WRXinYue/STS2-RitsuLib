# Asset Profiles & Fallbacks

This is the reference document for asset-profile structure, placeholder fallback, and asset-path diagnostics.

RitsuLib uses asset profiles to describe overrideable art, scenes, materials, and related resources.

This document explains the structure behind those profiles and the fallback rules that make them safe to use.

---

## Why Asset Profiles Exist

Asset overrides could have been exposed as a long flat list of virtual properties.

RitsuLib instead groups them into profile records because that scales better:

- related assets stay together
- partial overrides remain readable
- fallback merging stays explicit
- migration from placeholder-based systems is possible without abandoning structure

For characters, this is especially important because character assets span scenes, UI, VFX, audio, Spine, and multiplayer-specific textures.

---

## Character Asset Profile Structure

`CharacterAssetProfile` is split into several nested record groups:

- `CharacterSceneAssetSet`
- `CharacterUiAssetSet`
- `CharacterVfxAssetSet`
- `CharacterSpineAssetSet`
- `CharacterAudioAssetSet`
- `CharacterMultiplayerAssetSet`

This lets you override only one category without turning the other categories into noise.

Example:

```csharp
public override CharacterAssetProfile AssetProfile => new(
    Scenes: new(
        VisualsPath: "res://MyMod/scenes/character/my_character.tscn",
        EnergyCounterPath: "res://MyMod/ui/energy/my_energy_counter.tscn"),
    Ui: new(
        IconTexturePath: "res://MyMod/ui/top_panel/icon.png",
        MapMarkerPath: "res://MyMod/map/map_marker.png"),
    Audio: new(
        AttackSfx: "event:/sfx/characters/my_character/attack"));
```

---

## Placeholder Character Fallback

`ModCharacterTemplate` now exposes:

```csharp
public virtual string? PlaceholderCharacterId => "ironclad";
```

Behavior:

- your explicit `AssetProfile` is read first
- missing fields are filled from `CharacterAssetProfiles.FromCharacterId(PlaceholderCharacterId)`
- if `PlaceholderCharacterId` is `null`, fallback is disabled entirely

This gives you BaseLib-style migration convenience without flattening the whole character API.

---

## How Character Profile Merging Works

RitsuLib merges character profiles category-by-category and field-by-field.

That means:

- providing a custom `Scenes` record does not erase `Ui`
- providing only `RestSiteAnimPath` does not erase `MerchantAnimPath`
- providing only `AttackSfx` does not erase the other default SFX entries

This is important because character assets are rarely replaced all at once.

---

## Character Asset Profile Helpers

`CharacterAssetProfiles` provides several helper APIs:

- `FromCharacterId(string)`
- `Ironclad()` / `Silent()` / `Defect()` / `Regent()` / `Necrobinder()`
- `Resolve(profile, placeholderCharacterId)`
- `Merge(fallback, profile)`
- `FillMissingFrom(...)`
- `WithPlaceholder(...)`
- `WithScenes(...)`, `WithUi(...)`, `WithVfx(...)`, `WithSpine(...)`, `WithAudio(...)`, `WithMultiplayer(...)`

These helpers exist for two main use cases:

- partial authoring of new characters
- migration from frameworks that assumed a placeholder character from the start

---

## Content Asset Profiles

RitsuLib also provides profile records for other content:

- `CardAssetProfile`
- `RelicAssetProfile`
- `PowerAssetProfile`
- `OrbAssetProfile`
- `PotionAssetProfile`
- `AfflictionAssetProfile`
- `EnchantmentAssetProfile`
- `ActAssetProfile`

These are intentionally much smaller because their asset surfaces are smaller.

---

## Path Builder Helpers

For common vanilla-style asset conventions, there are helper factories:

- `CharacterAssetProfiles.FromCharacterId(...)`
- `ContentAssetProfiles.Card(...)`
- `ContentAssetProfiles.Relic(...)`
- `ContentAssetProfiles.Power(...)`
- `ContentAssetProfiles.Orb(...)`
- `ContentAssetProfiles.Potion(...)`
- `ContentAssetProfiles.Affliction(...)`
- `ContentAssetProfiles.Enchantment(...)`
- `ContentAssetProfiles.Act(...)`

There is also `CharacterAssetPathHelper` for deriving character-related default asset paths such as visuals, energy counter, select background, and map marker.

These helpers are most useful when your assets intentionally follow a conventional naming layout.

If those assets are backed by custom Godot scenes, remember that scene roots and scripted child nodes often need mod-local wrapper classes for stable editor binding. See [Godot Scene Authoring](GodotSceneAuthoring.md).

---

## Energy Counter vs Big Energy Icon vs Text Icon

RitsuLib treats these as separate concerns:

- `CustomEnergyCounterPath`: full combat UI counter scene
- `BigEnergyIconPath`: large pool-linked icon resolved through `EnergyIconHelper`
- `TextEnergyIconPath`: small icon used inside rich text

Why this matters:

- a scene replacement is the right abstraction for a custom counter
- a texture path is the right abstraction for a pool icon
- keeping them separate avoids overloading one API with three unrelated jobs

---

## Missing Path Diagnostics

RitsuLib now validates asset-path overrides through `AssetPathDiagnostics`.

Current behavior:

- empty path -> ignore override
- existing path -> use override
- missing path -> log a one-time warning and fall back to the base asset

The warning includes:

- the owner type
- the model entry when available
- the specific profile member name
- the missing path

This makes broken resource wiring much easier to debug.

---

## What Gets Path Validation

Path validation covers resource-like overrides such as:

- card textures, materials, overlays, and banners
- relic / power / orb / potion icons
- act backgrounds
- character visuals, energy counters, map assets, trail scenes, and Spine data
- pool energy icon paths

It does not validate non-resource strings such as audio event ids.

So character SFX override fields are still treated as plain values, not `ResourceLoader` paths.

---

## Recommended Character Authoring Pattern

For most custom characters, this pattern works well:

1. leave `PlaceholderCharacterId` at `ironclad` or switch it to the base character you want to inherit from
2. override only the assets that are truly custom
3. use pool-level `BigEnergyIconPath` / `TextEnergyIconPath` for energy icon concerns
4. use `CustomEnergyCounterPath` only when you need a real counter scene replacement

This keeps the authoring surface small while preserving safe fallback behavior.

---

## Recommended Content Authoring Pattern

For cards and other content:

- use `AssetProfile` when several asset fields belong together
- use a direct `Custom...Path` override only for one-off exceptions
- prefer helper factories like `ContentAssetProfiles.Card(...)` when your resource layout matches the helper's expectations

The profile approach is especially good for keeping portrait, frame, overlay, and banner decisions in one place.

---

## Related Documents

- [Character & Unlock Templates](CharacterAndUnlockScaffolding.md)
- [Content Authoring Toolkit](ContentAuthoringToolkit.md)
- [Godot Scene Authoring](GodotSceneAuthoring.md)
- [Diagnostics & Compatibility](DiagnosticsAndCompatibility.md)
- [Framework Design](FrameworkDesign.md)
