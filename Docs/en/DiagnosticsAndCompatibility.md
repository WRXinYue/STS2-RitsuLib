# Diagnostics & Compatibility

This document describes the diagnostic policy and compatibility layers that RitsuLib adds on top of the base game.

It focuses on:

- one-time warnings for recurring authoring errors
- debug-oriented fallbacks for missing localization and invalid unlock data
- narrow bridge patches where vanilla systems do not process mod content

---

## Design Intent

RitsuLib does not try to hide every engine limitation. It follows these rules:

- Surface real errors as early as possible
- where vanilla offers no safe extension point, the framework may add a bridge
- if a fallback would conceal too much behavior, keep the system explicit

This layer is deliberately narrow and only handles edge cases.

---

## One-Time Warning Policy

Some RitsuLib diagnostics warn only once per issue (or once per stable key), including:

- Missing resource paths (`AssetPathDiagnostics`)
- Missing `LocTable` keys when the master toggle and the **LocTable missing keys** toggle are enabled (`[Localization][DebugCompat]`)
- `THE_ARCHITECT` empty-`Lines` fallback when the debug compatibility master toggle and the **THE_ARCHITECT missing dialogue** toggle are enabled (`[Ancient]`)
- Other unlock-related one-shots (for example `ModUnlockMissingRuleWarnings`)

Each stable key or issue class logs at most once so traces stay readable.

---

## Asset Path Diagnostics

Explicit asset override paths are validated by `AssetPathDiagnostics`.

When a path is missing:

- A one-time warning is logged (host type, model id, member name, missing path)
- Behavior falls back to the original asset path or original behavior

This matters especially for character assets, where vanilla has almost no safe fallback.

See [Asset Profiles & Fallbacks](AssetProfilesAndFallbacks.md).

---

## Debug Compatibility Mode

Optional compatibility fallbacks are grouped under `debug_compatibility_mode` and per-area toggles in mod settings.

**Default (master toggle off):** vanilla behavior for the patched systems described here.

**Master toggle on:** the settings UI shows a **Compatibility fallbacks** section. Per-feature toggles default to **on**. Turning a toggle **off** removes only that fallback.

| Toggle | Effect when enabled |
|---|---|
| **LocTable missing keys** | Placeholder resolution + one-time `[Localization][DebugCompat]` warnings |
| **Invalid unlock epochs** | Skip the grant + one-time `[Unlocks][DebugCompat]` warnings |
| **THE_ARCHITECT missing dialogue** | Inject empty `Lines` entries for `ModContentRegistry` characters + one-time `[Ancient]` warning |

Except for LocTable missing-key handling, each toggle typically applies only to content registered through RitsuLib.

**`ModUnlockMissingRuleWarnings`** (e.g. missing boss-win rule registration): separate diagnostic path from the debug compatibility toggles.

**Released content:** ship complete localization, timeline data, and dialogue. Treat the table above as an iteration aid.

Windows settings path:

```text
%appdata%\SlayTheSpire2\steam\<user_id>\mod_data\com.ritsukage.sts2-RitsuLib\settings.json
```

---

## Registration Conflict Diagnostics

RitsuLib checks these conflicts explicitly:

| Conflict | Typical cause |
|---|---|
| Model id collision | Two registered models in the same mod/category share the same CLR type name |
| Epoch id collision | Two epochs resolve to the same `Id` |
| Story id collision | Two stories resolve to the same story identity |

When detected, the framework throws or logs errors — it does not accept ambiguous identity silently.

---

## Ancient Dialogue Compatibility Layer

Before `AncientDialogueSet.PopulateLocKeys`, the framework appends localization-defined ancient dialogue rows for registered mod characters. Authors own the keys; the framework discovers and injects them so mod characters use the same ancient-dialogue pipeline as vanilla.

### `THE_ARCHITECT` dialogue fallback

Gated on the debug compatibility master toggle and the **THE_ARCHITECT missing dialogue** toggle. If vanilla `TheArchitect.LoadDialogue` yields no dialogue, RitsuLib injects empty `Lines` entries for `ModContentRegistry` characters and logs **`[Ancient]`** once.

For key format, see [Localization & Keywords](LocalizationAndKeywords.md).

---

## Unlock Compatibility Bridges

Several vanilla progression checks only iterate vanilla characters. RitsuLib applies narrow patches so registered unlock rules participate at the same checkpoints for mod characters:

| Bridge | Description |
|---|---|
| Elite wins | Elite kill count → epoch checks |
| Boss wins | Boss kill count → epoch checks |
| Ascension 1 | Ascension 1 → epoch checks |
| Post-run character unlock | Post-run character-unlock epochs |
| Ascension reveal | Ascension reveal unlock checks |

Bridge patches forward RitsuLib-registered rules into vanilla progression checkpoints that otherwise skip mod characters. They do not introduce a separate progression store.

See [Timeline & Unlocks](TimelineAndUnlocks.md).

---

## Freeze Errors

If content, timeline, or unlock registration runs after freeze, RitsuLib throws.

That is intentional: late registration often means ModelDb caches are already built, fixed identity rules are in use, and unlock filters are active. Failing fast is the safe choice.

---

## Troubleshooting notes

1. Warnings usually point to mod data or configuration (paths, keys, rules), not random engine failure.
2. Fix missing assets and localization in source data rather than relying on placeholders long term.
3. Debug compatibility fallbacks are for iteration; release builds should ship with the master toggle off, or with per-feature toggles disabled and complete data.
4. Prefer explicit registration APIs; compatibility fallbacks are not a long-term architecture substitute.

---

## Related Documents

- [Asset Profiles & Fallbacks](AssetProfilesAndFallbacks.md)
- [Localization & Keywords](LocalizationAndKeywords.md)
- [Timeline & Unlocks](TimelineAndUnlocks.md)
- [Godot Scene Authoring](GodotSceneAuthoring.md)
- [Framework Design](FrameworkDesign.md)
