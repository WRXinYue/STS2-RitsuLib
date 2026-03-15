# Card Dynamic Var Toolkit

RitsuLib provides generic helpers for custom dynamic variables and tooltip wiring.

## Included helpers

Files:

- [Cards/DynamicVars/DynamicVarExtensions.cs](../Cards/DynamicVars/DynamicVarExtensions.cs)
- [Cards/DynamicVars/ModCardVars.cs](../Cards/DynamicVars/ModCardVars.cs)
- [Cards/DynamicVars/DynamicVarTooltipRegistry.cs](../Cards/DynamicVars/DynamicVarTooltipRegistry.cs)

## Creating vars

### Integer var

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    ModCardVars.Int("Hits", 2)
];
```

### String var

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    ModCardVars.String("Stance", "Aggressive")
];
```

RitsuLib does not attach any gameplay behavior to these vars. Their meaning is defined entirely by the content author.

## Tooltip helpers

Any `DynamicVar` can be given a tooltip.

### Using a shared localization entry

```csharp
new IntVar("Hits", 2)
    .WithSharedTooltip("MYMOD-HITS");
```

Expected localization keys in `static_hover_tips.json`:

```json
{
  "MYMOD-HITS.title": "Hits",
  "MYMOD-HITS.description": "This card hits [blue]{Hits}[/blue] times."
}
```

### Using explicit tables and keys

```csharp
new IntVar("Burst", 4)
    .WithTooltip("card_keywords", "burst.title", "card_keywords", "burst.description");
```

### Using a custom factory

```csharp
new IntVar("Drain", 3)
    .WithTooltip(var => new HoverTip(
        new LocString("static_hover_tips", "MYMOD-DRAIN.title"),
        new LocString("static_hover_tips", "MYMOD-DRAIN.description")));
```

## Notes

- tooltip metadata is preserved across `DynamicVar.Clone()`
- card hover tips now automatically include registered dynamic variable tooltips
- RitsuLib does not ship any built-in dynamic var gameplay semantics or localization entries
