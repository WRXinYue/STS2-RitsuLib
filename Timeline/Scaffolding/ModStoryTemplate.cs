using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    /// <summary>
    ///     Base <see cref="StoryModel" /> that derives its id from <see cref="StoryKey" />. Epoch order comes from
    ///     <see cref="ModTimelineRegistry.RegisterStoryEpoch{TStory, TEpoch}" /> (or
    ///     <see cref="StoryEpochPackEntry{TStory,TEpoch}" />),
    ///     not from an overridden type list.
    /// </summary>
    public abstract class ModStoryTemplate : StoryModel
    {
        /// <inheritdoc />
        protected sealed override string Id => StringHelper.Slugify(StoryKey);

        /// <inheritdoc />
        public sealed override EpochModel[] Epochs => ModStoryEpochBindings
            .GetOrderedEpochTypes(GetType())
            .Select(ResolveEpoch)
            .ToArray();

        /// <summary>
        ///     Human-readable story key slugified into the model id.
        /// </summary>
        protected abstract string StoryKey { get; }

        private static EpochModel ResolveEpoch(Type epochType)
        {
            ArgumentNullException.ThrowIfNull(epochType);
            return EpochModel.Get(EpochModel.GetId(epochType));
        }
    }
}
