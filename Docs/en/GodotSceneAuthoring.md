# Godot Scene Authoring

This document covers one practical caveat of working with Godot scenes in STS2 mods:

- many scene-facing game types should be wrapped in a mod-local subclass before you bind them in the editor
- your mod assembly's Godot scripts should be registered during initialization

---

## Why This Exists

In the current Godot Mono workflow used by STS2 modding, binding scenes directly to many game-assembly C# types is unreliable in the editor.

In practice, scenes behave more predictably when the script bound in the `.tscn` belongs to your own mod assembly, even if that script is only a thin subclass of a game type.

So the stable rule of thumb is:

- if a scene node needs to behave as a game/editor-facing Godot type, create a mod-local subclass and bind the scene to that subclass

---

## The Wrapper Pattern

Instead of binding a scene directly to a game type like `NEnergyCounter`, create a mod-local script:

```csharp
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MyMod.Scripts
{
    public partial class MyEnergyCounter : NEnergyCounter
    {
    }
}
```

Then bind the scene to `MyEnergyCounter`, not directly to `NEnergyCounter`.

This wrapper can be empty. Its job is often just to give the editor a local script type it can bind reliably.

---

## Typical Targets That Benefit From Local Wrappers

Common examples include scene-facing types such as:

- `NEnergyCounter`
- `NRestSiteCharacter`
- `NCreatureVisuals`
- `NSelectionReticle`
- `MegaLabel`

These are exactly the kinds of classes that often appear as scene roots or scripted child controls.

---

## Generic Example Pattern

A custom energy-counter scene might use bindings like:

- root counter script -> `MyEnergyCounter : NEnergyCounter`
- label child script -> `MyCounterLabel : MegaLabel`

A character visuals scene might use:

- root visuals script -> `MyCreatureVisuals : NCreatureVisuals`

A rest-site scene might use:

- root rest-site script -> `MyRestSiteCharacter : NRestSiteCharacter`

The important point is not the exact class names. The important point is that the bound script belongs to your mod assembly.

---

## Editor Rule

If the Godot editor needs to open, serialize, or rebind the script from your mod scene, prefer a mod-local subclass.

That applies even when:

- you do not add any new logic yet
- the class is only one line of inheritance
- the runtime target already exists in the game assembly

The wrapper is not redundant. It is the compatibility layer between your mod scene and the editor.

---

## Runtime Script Registration

If your mod uses Godot C# scene scripts, call `EnsureGodotScriptsRegistered(...)` during initialization:

```csharp
using System.Reflection;

RitsuLibFramework.EnsureGodotScriptsRegistered(
    Assembly.GetExecutingAssembly(),
    Logger);
```

This asks Godot's script bridge to discover C# scripts from your mod assembly.

A typical mod initializer should do this before content registration so custom scene scripts can be discovered reliably at runtime.

---

## Recommended Workflow

For any custom Godot scene in a mod:

1. identify the game/editor-facing base type you need
2. create a thin mod-local `partial class` inheriting that type
3. bind the `.tscn` to the mod-local script
4. call `EnsureGodotScriptsRegistered(Assembly.GetExecutingAssembly(), Logger)` during mod init

This keeps both editor binding and runtime script lookup much more predictable.

---

## When You Do Not Need This

You usually do not need a wrapper for:

- plain content model classes like cards, relics, powers, or characters
- pure C# helpers not used as Godot scripts
- logic that is never bound into a `.tscn` / Godot scene resource

This guidance is specifically about Godot scene authoring and script binding.

---

## Related Documents

- [Getting Started](GettingStarted.md)
- [Character & Unlock Scaffolding](CharacterAndUnlockScaffolding.md)
- [Asset Profiles & Fallbacks](AssetProfilesAndFallbacks.md)
- [Diagnostics & Compatibility](DiagnosticsAndCompatibility.md)
