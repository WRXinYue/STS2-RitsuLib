using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    public abstract class CharacterUnlockEpochTemplate<TCharacter> : EpochModel
        where TCharacter : CharacterModel
    {
        public override bool IsArtPlaceholder => false;

        protected virtual IEnumerable<Type> ExpansionEpochTypes => [];

        public override EpochModel[] GetTimelineExpansion()
        {
            return ExpansionEpochTypes.Select(type => Get(GetId(type))).ToArray();
        }

        public override void QueueUnlocks()
        {
            NTimelineScreen.Instance.QueueCharacterUnlock<TCharacter>(this);
            SaveManager.Instance.Progress.PendingCharacterUnlock = ModelDb.GetId<TCharacter>();

            var expansion = GetTimelineExpansion();
            if (expansion.Length > 0)
                QueueTimelineExpansion(expansion);
        }
    }
}
