using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    public abstract class PotionUnlockEpochTemplate : EpochModel
    {
        public IReadOnlyList<PotionModel> Potions => PotionTypes
            .Select(type => ModelDb.GetById<PotionModel>(ModelDb.GetId(type)))
            .ToArray();

        public override string UnlockText => CreatePotionUnlockText(Potions.ToList());

        protected abstract IEnumerable<Type> PotionTypes { get; }
        protected virtual IEnumerable<Type> ExpansionEpochTypes => [];

        public override EpochModel[] GetTimelineExpansion()
        {
            return ExpansionEpochTypes.Select(type => Get(GetId(type))).ToArray();
        }

        public override void QueueUnlocks()
        {
            NTimelineScreen.Instance.QueuePotionUnlock(Potions.ToList());

            var expansion = GetTimelineExpansion();
            if (expansion.Length > 0)
                QueueTimelineExpansion(expansion);
        }
    }
}
