using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;
using MegaCrit.Sts2.Core.Timeline;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Timeline.Patches
{
    /// <summary>
    ///     Before the timeline queues expansion slots, align <see cref="EpochModel.AllEpochIds" /> with
    ///     <c>EpochModel._epochTypeDictionary</c>. Otherwise <see cref="MegaCrit.Sts2.Core.Saves.ProgressState" />
    ///     <c>FilterAndSortEpochs</c> may strip mod expansion ids (IsValid false) immediately after
    ///     <see cref="MegaCrit.Sts2.Core.Saves.SaveManager.UnlockSlot" />, so the live expansion UI breaks while a cold
    ///     reload still shows slots once the cache matches the dictionary.
    /// </summary>
    public class QueueTimelineExpansionSyncEpochIdListPatch : IPatchMethod
    {
        /// <inheritdoc />
        public static string PatchId => "queue_timeline_expansion_sync_epoch_id_list";

        /// <inheritdoc />
        public static string Description =>
            "Sync EpochModel.AllEpochIds with the epoch type dictionary before QueueTimelineExpansion runs UnlockSlot";

        /// <inheritdoc />
        public static bool IsCritical => false;

        /// <inheritdoc />
        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(EpochModel), nameof(EpochModel.QueueTimelineExpansion), [typeof(EpochModel[])]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Ensures <see cref="EpochModel.IsValid" /> sees every id present in <see cref="EpochModel.Get" />.
        /// </summary>
        public static void Prefix(EpochModel[] epochs)
        {
            ArgumentNullException.ThrowIfNull(epochs);
            ModTimelineRegistry.EnsureAllEpochIdsSyncedWithDictionary();
        }
    }

    /// <summary>
    ///     Vanilla <see cref="NUnlockTimelineScreen.SetUnlocks" /> sorts only by <see cref="EpochSlotData.EraPosition" />,
    ///     which collides across <see cref="EpochEra" /> values — common for mod timelines. Expansion animation then
    ///     feeds <see cref="MegaCrit.Sts2.Core.Nodes.Screens.Timeline.NTimelineScreen.AddEpochSlots" /> in the wrong era
    ///     order.
    /// </summary>
    public class NUnlockTimelineScreenExpansionSlotSortPatch : IPatchMethod
    {
        /// <inheritdoc />
        public static string PatchId => "n_unlock_timeline_screen_expansion_slot_sort";

        /// <inheritdoc />
        public static string Description =>
            "Sort timeline expansion slots by Era then EraPosition for mod-compatible column ordering";

        /// <inheritdoc />
        public static bool IsCritical => false;

        /// <inheritdoc />
        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(NUnlockTimelineScreen), nameof(NUnlockTimelineScreen.SetUnlocks),
                    [typeof(List<EpochSlotData>)]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Replaces the vanilla <c>_erasToUnlock</c> ordering with era-stable sorting.
        /// </summary>
        public static void Postfix(NUnlockTimelineScreen __instance, List<EpochSlotData> eras)
        {
            ArgumentNullException.ThrowIfNull(eras);
            var field = AccessTools.Field(typeof(NUnlockTimelineScreen), "_erasToUnlock");
            if (field == null)
                return;

            var ordered = eras.OrderBy(a => a.Era).ThenBy(a => a.EraPosition).ToList();
            field.SetValue(__instance, ordered);
        }
    }
}
