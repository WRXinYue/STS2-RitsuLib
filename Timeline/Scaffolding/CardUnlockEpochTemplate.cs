using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    public abstract class CardUnlockEpochTemplate : EpochModel
    {
        public IReadOnlyList<CardModel> Cards => CardTypes
            .Select(type => ModelDb.GetById<CardModel>(ModelDb.GetId(type)))
            .ToArray();

        public override string UnlockText => CreateCardUnlockText(Cards.ToList());

        protected abstract IEnumerable<Type> CardTypes { get; }
        protected virtual IEnumerable<Type> ExpansionEpochTypes => [];

        public override EpochModel[] GetTimelineExpansion()
        {
            return ExpansionEpochTypes.Select(type => Get(GetId(type))).ToArray();
        }

        public override void QueueUnlocks()
        {
            NTimelineScreen.Instance.QueueCardUnlock(Cards);

            var expansion = GetTimelineExpansion();
            if (expansion.Length > 0)
                QueueTimelineExpansion(expansion);
        }
    }
}
