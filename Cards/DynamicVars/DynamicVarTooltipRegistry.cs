using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Utils;

namespace STS2RitsuLib.Cards.DynamicVars
{
    public static class DynamicVarTooltipRegistry
    {
        private static readonly AttachedState<DynamicVar, Func<DynamicVar, IHoverTip>?> TooltipFactories =
            new(() => null);

        public static void Set(DynamicVar dynamicVar, Func<DynamicVar, IHoverTip> tooltipFactory)
        {
            ArgumentNullException.ThrowIfNull(dynamicVar);
            ArgumentNullException.ThrowIfNull(tooltipFactory);
            TooltipFactories[dynamicVar] = tooltipFactory;
        }

        public static Func<DynamicVar, IHoverTip>? Get(DynamicVar dynamicVar)
        {
            ArgumentNullException.ThrowIfNull(dynamicVar);
            return TooltipFactories[dynamicVar];
        }

        public static IHoverTip? Create(DynamicVar dynamicVar)
        {
            ArgumentNullException.ThrowIfNull(dynamicVar);
            var factory = Get(dynamicVar);
            return factory?.Invoke(dynamicVar);
        }

        public static void CopyTo(DynamicVar source, DynamicVar destination)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destination);

            var factory = Get(source);
            if (factory != null)
                TooltipFactories[destination] = factory;
        }
    }
}
