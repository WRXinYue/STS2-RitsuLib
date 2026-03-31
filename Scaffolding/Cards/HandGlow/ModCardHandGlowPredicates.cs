using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Cards.HandGlow
{
    /// <summary>
    ///     Reusable condition functions matching common vanilla card patterns (Evil Eye, Fetch, Osty attacks). Prefer
    ///     <see cref="CardModelHandGlowExtensions" /> on <see cref="CardModel" /> for override bodies.
    /// </summary>
    public static class ModCardHandGlowPredicates
    {
        /// <summary>
        ///     Same idea as vanilla Osty attack cards: red when the owner’s companion is not present.
        /// </summary>
        public static bool OwnerCompanionOstyMissing(CardModel card)
        {
            return card.Owner?.IsOstyMissing == true;
        }

        /// <summary>
        ///     Same history shape as <see cref="MegaCrit.Sts2.Core.Models.Cards.EvilEye" />: any of the owner’s cards was
        ///     exhausted this turn (often drives gold while a stronger effect line is active).
        /// </summary>
        public static bool AnyOfOwnersCardsExhaustedThisTurn(CardModel card)
        {
            var owner = card.Owner;
            var combat = card.CombatState;
            var history = CombatManager.Instance?.History;
            if (owner is null || combat is null || history is null)
                return false;

            return history.Entries.OfType<CardExhaustedEntry>()
                .Any(e => e.HappenedThisTurn(combat) && e.Card.Owner == owner);
        }

        /// <summary>
        ///     Same history shape as <see cref="MegaCrit.Sts2.Core.Models.Cards.Fetch" /> gold: this card has not finished a
        ///     play this turn.
        /// </summary>
        public static bool ThisCardNotFinishedPlayThisTurn(CardModel card)
        {
            var combat = card.CombatState;
            var history = CombatManager.Instance?.History;
            if (combat is null || history is null)
                return false;

            return !history.CardPlaysFinished.Any(e =>
                e.CardPlay.Card == card && e.HappenedThisTurn(combat));
        }
    }
}
