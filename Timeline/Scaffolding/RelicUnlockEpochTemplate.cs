using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    public abstract class RelicUnlockEpochTemplate : EpochModel
    {
        public IReadOnlyList<RelicModel> Relics => RelicTypes
            .Select(type => ModelDb.GetById<RelicModel>(ModelDb.GetId(type)))
            .ToArray();

        public override string UnlockText => CreateRelicUnlockText(Relics.ToList());

        protected abstract IEnumerable<Type> RelicTypes { get; }
        protected virtual IEnumerable<Type> ExpansionEpochTypes => [];

        public override EpochModel[] GetTimelineExpansion()
        {
            return ExpansionEpochTypes.Select(type => Get(GetId(type))).ToArray();
        }

        public override void QueueUnlocks()
        {
            NTimelineScreen.Instance.QueueRelicUnlock(Relics.ToList());

            var expansion = GetTimelineExpansion();
            if (expansion.Length > 0)
                QueueTimelineExpansion(expansion);
        }
    }
}
