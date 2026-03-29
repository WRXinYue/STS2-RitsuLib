using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace STS2RitsuLib.Relics
{
    /// <summary>
    ///     Holds mod-supplied mappings for <see cref="ArchaicTooth" /> transcendence and
    ///     <see cref="TouchOfOrobas" /> refinement, applied via framework Harmony patches.
    /// </summary>
    internal static class OrobasAncientUpgradeRegistry
    {
        private static readonly Lock Sync = new();

        private static readonly Dictionary<ModelId, CardModel> TranscendenceByStarter = [];

        private static readonly Dictionary<ModelId, RelicModel> RefinementByStarter = [];

        internal static bool TryGetTranscendenceAncient(ModelId starterCardId,
            [NotNullWhen(true)] out CardModel? ancientTemplate)
        {
            lock (Sync)
            {
                return TranscendenceByStarter.TryGetValue(starterCardId, out ancientTemplate);
            }
        }

        internal static bool TryGetRefinementUpgrade(ModelId starterRelicId,
            [NotNullWhen(true)] out RelicModel? upgradedTemplate)
        {
            lock (Sync)
            {
                return RefinementByStarter.TryGetValue(starterRelicId, out upgradedTemplate);
            }
        }

        internal static bool HasTranscendenceStarter(ModelId starterCardId)
        {
            lock (Sync)
            {
                return TranscendenceByStarter.ContainsKey(starterCardId);
            }
        }

        /// <summary>
        ///     Distinct ancient card templates registered by mods (for <see cref="ArchaicTooth.TranscendenceCards" />).
        /// </summary>
        internal static IReadOnlyList<CardModel> GetRegisteredTranscendenceAncientTemplates()
        {
            lock (Sync)
            {
                var seen = new HashSet<ModelId>();

                return TranscendenceByStarter.Values.Where(card => seen.Add(card.Id)).ToList();
            }
        }

        internal static void RegisterTranscendence(ModelId starterCardId, CardModel ancientTemplate,
            string? modIdForLog)
        {
            ArgumentNullException.ThrowIfNull(ancientTemplate);

            lock (Sync)
            {
                if (TranscendenceByStarter.TryGetValue(starterCardId, out var previous) &&
                    !ReferenceEquals(previous, ancientTemplate))
                    RitsuLibFramework.Logger.Warn(
                        $"[OrobasAncientUpgrades] Transcendence mapping for starter card {starterCardId} " +
                        $"was replaced{(string.IsNullOrEmpty(modIdForLog) ? "" : $" (mod {modIdForLog})")}.");

                TranscendenceByStarter[starterCardId] = ancientTemplate;
            }
        }

        internal static void RegisterRefinement(ModelId starterRelicId, RelicModel upgradedTemplate,
            string? modIdForLog)
        {
            ArgumentNullException.ThrowIfNull(upgradedTemplate);

            lock (Sync)
            {
                if (RefinementByStarter.TryGetValue(starterRelicId, out var previous) &&
                    !ReferenceEquals(previous, upgradedTemplate))
                    RitsuLibFramework.Logger.Warn(
                        $"[OrobasAncientUpgrades] Refinement mapping for starter relic {starterRelicId} " +
                        $"was replaced{(string.IsNullOrEmpty(modIdForLog) ? "" : $" (mod {modIdForLog})")}.");

                RefinementByStarter[starterRelicId] = upgradedTemplate;
            }
        }
    }
}
