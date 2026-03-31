using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Cards.HandGlow
{
    /// <summary>
    ///     Declarative hand-highlight rules that mirror vanilla <see cref="CardModel.ShouldGlowGold" /> (gold border when a
    ///     bonus / stronger line is active while the card is playable) and <see cref="CardModel.ShouldGlowRed" /> (red border
    ///     for warning states such as companion missing). Applied either by overriding the protected
    ///     <c>ShouldGlow*Internal</c>
    ///     members, or by registering with <see cref="ModCardHandGlowRegistry" /> /
    ///     <see cref="STS2RitsuLib.Content.ModContentRegistry" /> (see
    ///     <c>RegisterCardHandGlow&lt;TCard&gt;</c> on the content registry).
    /// </summary>
    public readonly record struct ModCardHandGlowRules
    {
        /// <summary>
        ///     When this returns true for a card instance, the hand UI may show the gold highlight (same channel as
        ///     <see cref="CardModel.ShouldGlowGoldInternal" />).
        /// </summary>
        public Func<CardModel, bool>? GoldWhenBonusActive { get; init; }

        /// <summary>
        ///     When this returns true, the hand UI may show the red highlight (same channel as
        ///     <see cref="CardModel.ShouldGlowRedInternal" />).
        /// </summary>
        public Func<CardModel, bool>? RedWhenHandWarning { get; init; }

        /// <summary>
        ///     Gold only (e.g. Evil Eye–style “stronger effect active”).
        /// </summary>
        public static ModCardHandGlowRules Gold(Func<CardModel, bool> whenBonusActive)
        {
            return new() { GoldWhenBonusActive = whenBonusActive };
        }

        /// <summary>
        ///     Red only (e.g. Osty-missing attack cards).
        /// </summary>
        public static ModCardHandGlowRules Red(Func<CardModel, bool> whenHandWarning)
        {
            return new() { RedWhenHandWarning = whenHandWarning };
        }

        /// <summary>
        ///     Both channels in one rule set.
        /// </summary>
        public static ModCardHandGlowRules GoldAndRed(
            Func<CardModel, bool>? goldWhenBonusActive,
            Func<CardModel, bool>? redWhenHandWarning)
        {
            return new()
            {
                GoldWhenBonusActive = goldWhenBonusActive,
                RedWhenHandWarning = redWhenHandWarning,
            };
        }

        /// <summary>
        ///     Merges with <paramref name="other" /> by OR-ing each channel (useful when multiple mods register the same
        ///     card type, or you split rules across calls).
        /// </summary>
        public ModCardHandGlowRules Or(ModCardHandGlowRules other)
        {
            return new()
            {
                GoldWhenBonusActive = CombineOr(GoldWhenBonusActive, other.GoldWhenBonusActive),
                RedWhenHandWarning = CombineOr(RedWhenHandWarning, other.RedWhenHandWarning),
            };
        }

        private static Func<CardModel, bool>? CombineOr(Func<CardModel, bool>? a, Func<CardModel, bool>? b)
        {
            if (a == null)
                return b;
            return b == null ? a : c => a(c) || b(c);
        }
    }
}
