using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Relics;

namespace STS2RitsuLib
{
    public static partial class RitsuLibFramework
    {
        /// <summary>
        ///     Registers an <see cref="ArchaicTooth" /> transcendence pair: when the player’s deck contains
        ///     <typeparamref name="TStarterCard" />, obtaining the relic transforms it into <typeparamref name="TAncientCard" />
        ///     (preserving upgrade state and enchantments, same as vanilla starters).
        /// </summary>
        /// <param name="registeringModId">Optional mod id for log messages when mappings are replaced.</param>
        public static void RegisterArchaicToothTranscendenceMapping<TStarterCard, TAncientCard>(
            string? registeringModId = null)
            where TStarterCard : CardModel
            where TAncientCard : CardModel
        {
            RegisterArchaicToothTranscendenceMapping(
                ModelDb.Card<TStarterCard>().Id,
                ModelDb.Card<TAncientCard>(),
                registeringModId);
        }

        /// <summary>
        ///     Registers an <see cref="ArchaicTooth" /> transcendence mapping using explicit ids/templates.
        /// </summary>
        /// <param name="starterCardId">Deck card model id to match.</param>
        /// <param name="ancientCardTemplate">
        ///     Target card prototype from <see cref="ModelDb.Card{T}" /> (same usage as vanilla’s transcendence table values).
        /// </param>
        /// <param name="registeringModId">Optional mod id for log messages when mappings are replaced.</param>
        public static void RegisterArchaicToothTranscendenceMapping(ModelId starterCardId,
            CardModel ancientCardTemplate,
            string? registeringModId = null)
        {
            OrobasAncientUpgradeRegistry.RegisterTranscendence(starterCardId, ancientCardTemplate, registeringModId);
        }

        /// <summary>
        ///     Registers a <see cref="TouchOfOrobas" /> refinement pair: when the player’s starter relic is
        ///     <typeparamref name="TStarterRelic" />, the blessing replaces it with <typeparamref name="TUpgradedRelic" />.
        /// </summary>
        /// <param name="registeringModId">Optional mod id for log messages when mappings are replaced.</param>
        public static void RegisterTouchOfOrobasRefinementMapping<TStarterRelic, TUpgradedRelic>(
            string? registeringModId = null)
            where TStarterRelic : RelicModel
            where TUpgradedRelic : RelicModel
        {
            RegisterTouchOfOrobasRefinementMapping(
                ModelDb.Relic<TStarterRelic>().Id,
                ModelDb.Relic<TUpgradedRelic>(),
                registeringModId);
        }

        /// <summary>
        ///     Registers a <see cref="TouchOfOrobas" /> refinement mapping using explicit ids/templates.
        /// </summary>
        /// <param name="starterRelicId">Starter relic instance id to match.</param>
        /// <param name="upgradedRelicTemplate">
        ///     Replacement relic prototype from <see cref="ModelDb.Relic{T}" /> (same shape as vanilla refinement table values).
        /// </param>
        /// <param name="registeringModId">Optional mod id for log messages when mappings are replaced.</param>
        public static void RegisterTouchOfOrobasRefinementMapping(ModelId starterRelicId,
            RelicModel upgradedRelicTemplate,
            string? registeringModId = null)
        {
            OrobasAncientUpgradeRegistry.RegisterRefinement(starterRelicId, upgradedRelicTemplate, registeringModId);
        }
    }
}
