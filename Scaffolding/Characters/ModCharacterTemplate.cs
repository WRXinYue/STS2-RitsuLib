using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Characters
{
    public abstract class ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool> : CharacterModel
        where TCardPool : CardPoolModel
        where TRelicPool : RelicPoolModel
        where TPotionPool : PotionPoolModel
    {
        public sealed override CardPoolModel CardPool =>
            ModelDb.GetById<CardPoolModel>(ModelDb.GetId<TCardPool>());

        public sealed override RelicPoolModel RelicPool =>
            ModelDb.GetById<RelicPoolModel>(ModelDb.GetId<TRelicPool>());

        public sealed override PotionPoolModel PotionPool =>
            ModelDb.GetById<PotionPoolModel>(ModelDb.GetId<TPotionPool>());

        public sealed override IEnumerable<CardModel> StartingDeck => ResolveModels<CardModel>(StartingDeckTypes);

        public sealed override IReadOnlyList<RelicModel> StartingRelics =>
            ResolveModels<RelicModel>(StartingRelicTypes).ToArray();

        public sealed override IReadOnlyList<PotionModel> StartingPotions =>
            ResolveModels<PotionModel>(StartingPotionTypes).ToArray();

        protected sealed override CharacterModel? UnlocksAfterRunAs => UnlocksAfterRunAsType == null
            ? null
            : ModelDb.GetById<CharacterModel>(ModelDb.GetId(UnlocksAfterRunAsType));

        protected abstract IEnumerable<Type> StartingDeckTypes { get; }
        protected abstract IEnumerable<Type> StartingRelicTypes { get; }
        protected virtual IEnumerable<Type> StartingPotionTypes => [];
        protected virtual Type? UnlocksAfterRunAsType => null;

        protected static IEnumerable<TModel> ResolveModels<TModel>(IEnumerable<Type> types)
            where TModel : AbstractModel
        {
            return types
                .Select(type => ModelDb.GetById<TModel>(ModelDb.GetId(type)))
                .ToArray();
        }
    }
}
