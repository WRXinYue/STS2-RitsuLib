using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Timeline.Scaffolding;

namespace STS2RitsuLib.Timeline.Patches
{
    /// <summary>
    ///     Vanilla <see cref="NTimelineScreen.InitScreen" /> only passes epochs that already exist in
    ///     <see cref="MegaCrit.Sts2.Core.Saves.ProgressState.Epochs" /> into <see cref="NTimelineScreen.AddEpochSlots" />.
    ///     Mod story lines are not inserted into the save until gameplay or expansion runs
    ///     <see cref="MegaCrit.Sts2.Core.Saves.ProgressState.UnlockSlot" />, so mod columns would stay missing while the
    ///     underlying unlock flow stays correct. This prefix only augments the in-memory list for the non-animated
    ///     <see cref="NTimelineScreen.AddEpochSlots" /> call used by <c>InitScreen</c> (the only <c>isAnimated: false</c>
    ///     caller), without writing placeholder rows into progress — so <see cref="EpochModel.QueueTimelineExpansion" />
    ///     and <see cref="MegaCrit.Sts2.Core.Saves.ProgressState.UnlockSlot" /> behave like vanilla.
    /// </summary>
    public sealed class NTimelineScreenAddEpochSlotsMergeModTemplatesPatch : IPatchMethod
    {
        /// <inheritdoc />
        public static string PatchId => "n_timeline_screen_add_epoch_slots_merge_mod_templates";

        /// <inheritdoc />
        public static string Description =>
            "Merge ModEpochTemplate slots into InitScreen AddEpochSlots list without mutating Progress.Epochs";

        /// <inheritdoc />
        public static bool IsCritical => false;

        /// <inheritdoc />
        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(NTimelineScreen), nameof(NTimelineScreen.AddEpochSlots),
                    [typeof(List<EpochSlotData>), typeof(bool)]),
            ];
        }

        /// <summary>
        ///     When <paramref name="isAnimated" /> is false (timeline init), append missing mod template slots and re-sort
        ///     like vanilla <c>InitScreen</c> (<c>EraPosition</c> only).
        /// </summary>
        public static void Prefix(List<EpochSlotData> slotsToAdd, bool isAnimated)
        {
            if (isAnimated)
                return;

            var progress = SaveManager.Instance?.Progress;

            var existing = new HashSet<string>(slotsToAdd.Count);
            foreach (var s in slotsToAdd)
                existing.Add(s.Model.Id);

            foreach (var id in EpochModel.AllEpochIds)
            {
                if (existing.Contains(id))
                    continue;

                EpochModel model;
                try
                {
                    model = EpochModel.Get(id);
                }
                catch
                {
                    continue;
                }

                if (model is not ModEpochTemplate)
                    continue;

                slotsToAdd.Add(new(model, ResolveMergedModSlotState(id, progress)));
                existing.Add(id);
            }

            slotsToAdd.Sort((a, b) => a.EraPosition.CompareTo(b.EraPosition));
        }

        private static EpochSlotState ResolveMergedModSlotState(string id, ProgressState? progress)
        {
            if (progress == null || !progress.HasEpoch(id))
                return EpochSlotState.NotObtained;

            var row = progress.Epochs.FirstOrDefault(e => e.Id == id);
            if (row == null)
                return EpochSlotState.NotObtained;

            return row.State switch
            {
                EpochState.Revealed => EpochSlotState.Complete,
                EpochState.Obtained => EpochSlotState.Obtained,
                EpochState.ObtainedNoSlot => EpochSlotState.Obtained,
                _ => EpochSlotState.NotObtained,
            };
        }
    }
}
