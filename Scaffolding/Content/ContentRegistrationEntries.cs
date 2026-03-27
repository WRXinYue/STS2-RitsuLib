using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Content;

namespace STS2RitsuLib.Scaffolding.Content
{
    public interface IContentRegistrationEntry
    {
        void Register(ModContentRegistry registry);
    }

    public sealed class CharacterRegistrationEntry<TCharacter> : IContentRegistrationEntry
        where TCharacter : CharacterModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterCharacter<TCharacter>();
        }
    }

    public sealed class ActRegistrationEntry<TAct> : IContentRegistrationEntry
        where TAct : ActModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterAct<TAct>();
        }
    }

    public sealed class CardRegistrationEntry<TPool, TCard>(ModelPublicEntryOptions publicEntry = default)
        : IContentRegistrationEntry
        where TPool : CardPoolModel
        where TCard : CardModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterCard<TPool, TCard>(publicEntry);
        }
    }

    public sealed class RelicRegistrationEntry<TPool, TRelic>(ModelPublicEntryOptions publicEntry = default)
        : IContentRegistrationEntry
        where TPool : RelicPoolModel
        where TRelic : RelicModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterRelic<TPool, TRelic>(publicEntry);
        }
    }

    public sealed class PotionRegistrationEntry<TPool, TPotion>(ModelPublicEntryOptions publicEntry = default)
        : IContentRegistrationEntry
        where TPool : PotionPoolModel
        where TPotion : PotionModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterPotion<TPool, TPotion>(publicEntry);
        }
    }

    public sealed class PowerRegistrationEntry<TPower> : IContentRegistrationEntry
        where TPower : PowerModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterPower<TPower>();
        }
    }

    public sealed class SharedCardPoolRegistrationEntry<TPool> : IContentRegistrationEntry
        where TPool : CardPoolModel
    {
        public void Register(ModContentRegistry registry)
        {
            registry.RegisterSharedCardPool<TPool>();
        }
    }
}
