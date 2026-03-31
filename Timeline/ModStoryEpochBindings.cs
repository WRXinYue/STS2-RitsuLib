using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Timeline
{
    /// <summary>
    ///     Collects ordered epoch CLR types per concrete <see cref="MegaCrit.Sts2.Core.Timeline.StoryModel" /> type, filled by
    ///     <see cref="ModTimelineRegistry.RegisterStoryEpoch{TStory, TEpoch}" />. <see cref="Scaffolding.ModStoryTemplate" />
    ///     reads this list instead of a hard-coded <c>EpochTypes</c> override.
    /// </summary>
    public static class ModStoryEpochBindings
    {
        private static readonly Lock Sync = new();

        private static readonly Dictionary<Type, List<Type>> StoryToEpochs = [];

        private static readonly Dictionary<Type, Type> EpochToStory = [];

        private static bool _frozen;

        /// <summary>
        ///     Appends <paramref name="epochType" /> to <paramref name="storyType" />'s column order (registration order).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Frozen, duplicate epoch in the same story, or epoch already bound to
        ///     another story.
        /// </exception>
        public static void Append(Type storyType, Type epochType)
        {
            ArgumentNullException.ThrowIfNull(storyType);
            ArgumentNullException.ThrowIfNull(epochType);

            if (!typeof(StoryModel).IsAssignableFrom(storyType) ||
                storyType.IsAbstract ||
                storyType.IsInterface)
                throw new ArgumentException($"Type '{storyType.FullName}' must be a concrete StoryModel subtype.",
                    nameof(storyType));

            if (!typeof(EpochModel).IsAssignableFrom(epochType) ||
                epochType.IsAbstract ||
                epochType.IsInterface)
                throw new ArgumentException($"Type '{epochType.FullName}' must be a concrete EpochModel subtype.",
                    nameof(epochType));

            lock (Sync)
            {
                ThrowIfFrozen();

                if (EpochToStory.TryGetValue(epochType, out var owner) && owner != storyType)
                    throw new InvalidOperationException(
                        $"Epoch type '{epochType.Name}' is already bound to story '{owner.Name}'; cannot bind to '{storyType.Name}'.");

                EpochToStory[epochType] = storyType;

                if (!StoryToEpochs.TryGetValue(storyType, out var list))
                {
                    list = [];
                    StoryToEpochs[storyType] = list;
                }

                if (list.Contains(epochType))
                    throw new InvalidOperationException(
                        $"Epoch type '{epochType.Name}' is already listed for story '{storyType.Name}'.");

                list.Add(epochType);
            }
        }

        /// <summary>
        ///     Ordered epoch types for a concrete story type, or empty when none were bound.
        /// </summary>
        public static IReadOnlyList<Type> GetOrderedEpochTypes(Type storyConcreteType)
        {
            ArgumentNullException.ThrowIfNull(storyConcreteType);

            lock (Sync)
            {
                return StoryToEpochs.TryGetValue(storyConcreteType, out var list)
                    ? list.ToArray()
                    : [];
            }
        }

        internal static void Freeze()
        {
            lock (Sync)
            {
                _frozen = true;
            }
        }

        private static void ThrowIfFrozen()
        {
            if (_frozen)
                throw new InvalidOperationException(
                    "Story–epoch bindings are frozen; register before model initialization.");
        }
    }
}
