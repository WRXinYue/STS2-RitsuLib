using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace STS2RitsuLib.Cards.DynamicVars
{
    public static class DynamicVarExtensions
    {
        extension(DynamicVar dynamicVar)
        {
            public DynamicVar WithTooltip(Func<DynamicVar, IHoverTip> tooltipFactory)
            {
                ArgumentNullException.ThrowIfNull(dynamicVar);
                ArgumentNullException.ThrowIfNull(tooltipFactory);
                DynamicVarTooltipRegistry.Set(dynamicVar, tooltipFactory);
                return dynamicVar;
            }

            public DynamicVar WithTooltip(string titleTable,
                string titleKey,
                string? descriptionTable = null,
                string? descriptionKey = null,
                string? iconPath = null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(titleTable);
                ArgumentException.ThrowIfNullOrWhiteSpace(titleKey);

                var resolvedDescriptionTable = descriptionTable ?? titleTable;
                var resolvedDescriptionKey =
                    descriptionKey ?? titleKey.Replace(".title", ".description", StringComparison.Ordinal);

                return dynamicVar.WithTooltip(var =>
                {
                    var title = new LocString(titleTable, titleKey);
                    var description = new LocString(resolvedDescriptionTable, resolvedDescriptionKey);
                    title.Add(var);
                    description.Add(var);

                    Texture2D? icon = null;
                    if (!string.IsNullOrWhiteSpace(iconPath) && ResourceLoader.Exists(iconPath))
                        icon = ResourceLoader.Load<Texture2D>(iconPath);

                    return new HoverTip(title, description, icon);
                });
            }

            public DynamicVar WithSharedTooltip(string entryPrefix,
                string? iconPath = null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(entryPrefix);
                return dynamicVar.WithTooltip("static_hover_tips", $"{entryPrefix}.title", "static_hover_tips",
                    $"{entryPrefix}.description", iconPath);
            }

            public IHoverTip? CreateHoverTip()
            {
                return DynamicVarTooltipRegistry.Create(dynamicVar);
            }
        }

        extension(DynamicVarSet dynamicVars)
        {
            public int GetIntOrDefault(string key, int defaultValue = 0)
            {
                ArgumentNullException.ThrowIfNull(dynamicVars);
                ArgumentException.ThrowIfNullOrWhiteSpace(key);
                return dynamicVars.TryGetValue(key, out var value) ? value.IntValue : defaultValue;
            }

            public decimal GetValueOrDefault(string key, decimal defaultValue = 0m)
            {
                ArgumentNullException.ThrowIfNull(dynamicVars);
                ArgumentException.ThrowIfNullOrWhiteSpace(key);
                return dynamicVars.TryGetValue(key, out var value) ? value.BaseValue : defaultValue;
            }

            public bool HasPositiveValue(string key)
            {
                return dynamicVars.GetValueOrDefault(key) > 0m;
            }
        }
    }
}
