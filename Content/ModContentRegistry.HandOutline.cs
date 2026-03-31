using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;

namespace STS2RitsuLib.Content
{
    public sealed partial class ModContentRegistry
    {
        /// <summary>
        ///     Registers a custom in-hand outline / highlight tint (<see cref="MegaCrit.Sts2.Core.Nodes.Cards.NCardHighlight" />
        ///     <c>Modulate</c>) for <typeparamref name="TCard" />. Multiple rules may be registered; the highest
        ///     <see cref="ModCardHandOutlineRule.Priority" /> among matching predicates wins.
        /// </summary>
        public void RegisterCardHandOutline<TCard>(ModCardHandOutlineRule rule) where TCard : CardModel
        {
            EnsureMutable("register card hand outline rule");
            ModCardHandOutlineRegistry.Register<TCard>(rule);
        }

        /// <summary>
        ///     Registers several <see cref="ModCardHandOutlineRule" /> values for the same card type (e.g. different
        ///     priorities / conditions).
        /// </summary>
        public void RegisterCardHandOutline<TCard>(params ModCardHandOutlineRule[] rules) where TCard : CardModel
        {
            ArgumentNullException.ThrowIfNull(rules);
            EnsureMutable("register card hand outline rules");
            foreach (var rule in rules)
                ModCardHandOutlineRegistry.Register<TCard>(rule);
        }
    }
}
