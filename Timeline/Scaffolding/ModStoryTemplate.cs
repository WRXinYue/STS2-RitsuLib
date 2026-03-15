using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    public abstract class ModStoryTemplate : StoryModel
    {
        protected sealed override string Id => StringHelper.Slugify(StoryKey);

        public sealed override EpochModel[] Epochs => EpochTypes
            .Select(ResolveEpoch)
            .ToArray();

        protected abstract string StoryKey { get; }
        protected abstract IEnumerable<Type> EpochTypes { get; }

        private static EpochModel ResolveEpoch(Type epochType)
        {
            ArgumentNullException.ThrowIfNull(epochType);
            return EpochModel.Get(EpochModel.GetId(epochType));
        }
    }
}
