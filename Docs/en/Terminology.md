# Terminology

This document defines the canonical terms used across the RitsuLib documentation.

---

## Core Terms

| Term | Preferred usage | Notes |
|---|---|---|
| settings UI | settings UI | Use for the mod configuration interface as a whole. |
| settings page | page | A single registered page in the settings UI. |
| section | section | A structured group within a page. |
| entry | entry | One visible row or control within a section. |
| binding | binding | The read/write link between UI and stored or in-memory state. |
| persistence | persistence | The storage layer and save lifecycle. |
| persisted | persisted | Use for values written through the persistence layer. |
| preview-only | preview-only | Use for controls or bindings that never persist data. |
| fallback | fallback | Preferred over `shim` for compatibility behavior. |
| compatibility fallback | compatibility fallback | A narrowly scoped behavior used when vanilla data or APIs are incomplete. |
| bridge patch | bridge patch | A patch that forwards mod content into vanilla logic that would otherwise skip it. |
| registry | registry | The runtime registration container for a content type. |
| content pack | content pack | The convenience entry point that writes into multiple registries. |
| builder | builder | Use for fluent page, section, or content construction APIs. |
| override | override | Use for replacing an asset path, behavior, or value source. |
| placeholder | placeholder | A temporary fallback value used when data is missing. |
| scope | scope | The storage scope of a persisted value. |
| profile | profile | Per-profile save scope. |
| global | global | Cross-profile save scope. |
| epoch | epoch | Keep the game term `epoch` in English. |
| story | story | Keep the game term `story` in English. |
| Ancient dialogue | Ancient dialogue | Use this spelling for the game system and related keys. |

---

## Related Documents

- [Framework Design](FrameworkDesign.md)
- [Mod Settings](ModSettings.md)
- [Diagnostics & Compatibility](DiagnosticsAndCompatibility.md)
- [Localization & Keywords](LocalizationAndKeywords.md)
