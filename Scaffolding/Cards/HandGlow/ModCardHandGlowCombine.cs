using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Scaffolding.Cards.HandGlow
{
    /// <summary>
    ///     Combinators for <see cref="ModCardHandGlowRules" /> predicates (<c>Func&lt;CardModel, bool&gt;</c>).
    /// </summary>
    public static class ModCardHandGlowCombine
    {
        /// <summary>Logical OR of any non-null predicates.</summary>
        public static Func<CardModel, bool> Or(params Func<CardModel, bool>?[] parts)
        {
            return card => { return parts.OfType<Func<CardModel, bool>>().Any(p => p(card)); };
        }

        /// <summary>Logical AND of any non-null predicates; if all parts are null, returns <c>_ => true</c>.</summary>
        public static Func<CardModel, bool> And(params Func<CardModel, bool>?[] parts)
        {
            var filtered = parts.Where(static p => p != null).Cast<Func<CardModel, bool>>().ToArray();
            if (filtered.Length == 0)
                return static _ => true;

            return card => filtered.All(p => p(card));
        }
    }
}
