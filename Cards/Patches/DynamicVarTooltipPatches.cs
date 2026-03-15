using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Cards.Patches
{
    public class CardDynamicVarTooltipPatch : IPatchMethod
    {
        public static string PatchId => "card_dynamic_var_tooltips";
        public static string Description => "Append registered dynamic variable tooltips to card hover tips";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(CardModel), "get_HoverTips"),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
            // ReSharper restore InconsistentNaming
        {
            var extraTips = __instance.DynamicVars.Values
                .Select(DynamicVarTooltipRegistry.Create)
                .OfType<IHoverTip>()
                .ToArray();

            if (extraTips.Length == 0)
                return;

            __result = __result.Concat(extraTips).Distinct().ToArray();
        }
    }

    public class DynamicVarTooltipClonePatch : IPatchMethod
    {
        public static string PatchId => "dynamic_var_tooltip_clone";
        public static string Description => "Preserve registered dynamic variable tooltip metadata when cloning";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(DynamicVar), nameof(DynamicVar.Clone), Type.EmptyTypes),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(DynamicVar __instance, DynamicVar __result)
            // ReSharper restore InconsistentNaming
        {
            DynamicVarTooltipRegistry.CopyTo(__instance, __result);
        }
    }
}
