using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Cards.HandGlow
{
    /// <summary>
    ///     Extension methods for <see cref="CardModel" /> hand-highlight conditions; use inside
    ///     <c>ShouldGlowGoldInternal</c> / <c>ShouldGlowRedInternal</c> overrides for concise, readable code.
    /// </summary>
    public static class CardModelHandGlowExtensions
    {
        extension(CardModel card)
        {
            /// <inheritdoc cref="ModCardHandGlowPredicates.OwnerCompanionOstyMissing" />
            public bool ModHandGlowOwnerCompanionOstyMissing()
            {
                return ModCardHandGlowPredicates.OwnerCompanionOstyMissing(card);
            }

            /// <inheritdoc cref="ModCardHandGlowPredicates.AnyOfOwnersCardsExhaustedThisTurn" />
            public bool ModHandGlowAnyOfOwnersCardsExhaustedThisTurn()
            {
                return ModCardHandGlowPredicates.AnyOfOwnersCardsExhaustedThisTurn(card);
            }

            /// <inheritdoc cref="ModCardHandGlowPredicates.ThisCardNotFinishedPlayThisTurn" />
            public bool ModHandGlowThisCardNotFinishedPlayThisTurn()
            {
                return ModCardHandGlowPredicates.ThisCardNotFinishedPlayThisTurn(card);
            }
        }
    }
}
