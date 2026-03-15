using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class TypeListRelicPoolModel : RelicPoolModel
    {
        protected abstract IEnumerable<Type> RelicTypes { get; }

        protected sealed override IEnumerable<RelicModel> GenerateAllRelics()
        {
            return RelicTypes
                .Select(type => ModelDb.GetById<RelicModel>(ModelDb.GetId(type)))
                .ToArray();
        }
    }
}
