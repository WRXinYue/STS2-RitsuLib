using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class TypeListPotionPoolModel : PotionPoolModel
    {
        protected abstract IEnumerable<Type> PotionTypes { get; }

        protected sealed override IEnumerable<PotionModel> GenerateAllPotions()
        {
            return PotionTypes
                .Select(type => ModelDb.GetById<PotionModel>(ModelDb.GetId(type)))
                .ToArray();
        }
    }
}
