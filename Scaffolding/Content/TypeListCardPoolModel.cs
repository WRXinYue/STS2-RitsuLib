using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class TypeListCardPoolModel : CardPoolModel
    {
        protected abstract IEnumerable<Type> CardTypes { get; }

        protected sealed override CardModel[] GenerateAllCards()
        {
            return CardTypes
                .Select(type => ModelDb.GetById<CardModel>(ModelDb.GetId(type)))
                .ToArray();
        }
    }
}
