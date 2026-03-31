using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Cards.HandGlow;

namespace STS2RitsuLib.Content
{
    public sealed partial class ModContentRegistry
    {
        /// <summary>
        ///     Registers hand-highlight rules for <typeparamref name="TCard" /> (gold / red borders in hand during combat
        ///     play phase). Same semantics as overriding <see cref="CardModel.ShouldGlowGoldInternal" /> and
        ///     <see cref="CardModel.ShouldGlowRedInternal" />, but keeps logic in data/registration. Multiple registrations for
        ///     the same type OR-merge.
        /// </summary>
        public void RegisterCardHandGlow<TCard>(ModCardHandGlowRules rules) where TCard : CardModel
        {
            EnsureMutable("register card hand glow rules");
            ModCardHandGlowRegistry.Register<TCard>(rules);
        }
    }
}
