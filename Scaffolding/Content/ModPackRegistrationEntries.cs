using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;
using STS2RitsuLib.Unlocks;

namespace STS2RitsuLib.Scaffolding.Content
{
    /// <summary>
    ///     Registers <typeparamref name="TEpoch" /> and appends it to <typeparamref name="TStory" />'s ordered column.
    /// </summary>
    public sealed class StoryEpochPackEntry<TStory, TEpoch> : IModContentPackEntry
        where TStory : StoryModel, new()
        where TEpoch : EpochModel, new()
    {
        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Timeline.RegisterStoryEpoch<TStory, TEpoch>();
        }
    }

    /// <summary>
    ///     Registers a <see cref="StoryModel" /> type into vanilla story discovery.
    /// </summary>
    public sealed class StoryPackEntry<TStory> : IModContentPackEntry
        where TStory : StoryModel, new()
    {
        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Timeline.RegisterStory<TStory>();
        }
    }

    /// <summary>
    ///     <see cref="ModUnlockRegistry.RequireEpoch{TModel, TEpoch}" />.
    /// </summary>
    public sealed class RequireEpochPackEntry<TModel, TEpoch> : IModContentPackEntry
        where TModel : AbstractModel
        where TEpoch : EpochModel, new()
    {
        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Unlocks.RequireEpoch<TModel, TEpoch>();
        }
    }

    /// <summary>
    ///     <see cref="ModUnlockRegistry.UnlockEpochAfterWinAs{TCharacter, TEpoch}" />.
    /// </summary>
    public sealed class UnlockEpochAfterWinAsPackEntry<TCharacter, TEpoch> : IModContentPackEntry
        where TCharacter : CharacterModel
        where TEpoch : EpochModel, new()
    {
        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Unlocks.UnlockEpochAfterWinAs<TCharacter, TEpoch>();
        }
    }

    /// <summary>
    ///     <see cref="ModUnlockRegistry.UnlockEpochAfterEliteVictories{TCharacter, TEpoch}" />.
    /// </summary>
    public sealed class UnlockEpochAfterEliteVictoriesPackEntry<TCharacter, TEpoch> : IModContentPackEntry
        where TCharacter : CharacterModel
        where TEpoch : EpochModel, new()
    {
        private readonly int _requiredEliteWins;

        /// <summary>
        ///     Creates a rule with the given elite-win threshold (default 15).
        /// </summary>
        public UnlockEpochAfterEliteVictoriesPackEntry(int requiredEliteWins = 15)
        {
            _requiredEliteWins = requiredEliteWins;
        }

        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Unlocks.UnlockEpochAfterEliteVictories<TCharacter, TEpoch>(_requiredEliteWins);
        }
    }

    /// <summary>
    ///     <see cref="ModUnlockRegistry.UnlockEpochAfterBossVictories{TCharacter, TEpoch}" />.
    /// </summary>
    public sealed class UnlockEpochAfterBossVictoriesPackEntry<TCharacter, TEpoch> : IModContentPackEntry
        where TCharacter : CharacterModel
        where TEpoch : EpochModel, new()
    {
        private readonly int _requiredBossWins;

        /// <summary>
        ///     Creates a rule with the given boss-win threshold (default 15).
        /// </summary>
        public UnlockEpochAfterBossVictoriesPackEntry(int requiredBossWins = 15)
        {
            _requiredBossWins = requiredBossWins;
        }

        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Unlocks.UnlockEpochAfterBossVictories<TCharacter, TEpoch>(_requiredBossWins);
        }
    }

    /// <summary>
    ///     <see cref="ModUnlockRegistry.UnlockEpochAfterAscensionOneWin{TCharacter, TEpoch}" />.
    /// </summary>
    public sealed class UnlockEpochAfterAscensionOneWinPackEntry<TCharacter, TEpoch> : IModContentPackEntry
        where TCharacter : CharacterModel
        where TEpoch : EpochModel, new()
    {
        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Unlocks.UnlockEpochAfterAscensionOneWin<TCharacter, TEpoch>();
        }
    }

    /// <summary>
    ///     <see cref="ModUnlockRegistry.RevealAscensionAfterEpoch{TCharacter, TEpoch}" />.
    /// </summary>
    public sealed class RevealAscensionAfterEpochPackEntry<TCharacter, TEpoch> : IModContentPackEntry
        where TCharacter : CharacterModel
        where TEpoch : EpochModel, new()
    {
        /// <inheritdoc />
        public void Apply(ModContentPackContext context)
        {
            context.Unlocks.RevealAscensionAfterEpoch<TCharacter, TEpoch>();
        }
    }
}
