using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace STS2RitsuLib.Cards.DynamicVars
{
    public sealed class ComputedDynamicVar : DynamicVar
    {
        private readonly Func<CardModel?, decimal> _currentValueFactory;
        private readonly Func<CardModel?, CardPreviewMode, Creature?, bool, decimal>? _previewValueFactory;

        public ComputedDynamicVar(
            string name,
            decimal baseValue,
            Func<CardModel?, decimal> currentValueFactory,
            Func<CardModel?, CardPreviewMode, Creature?, bool, decimal>? previewValueFactory = null)
            : base(name, baseValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(currentValueFactory);

            _currentValueFactory = currentValueFactory;
            _previewValueFactory = previewValueFactory;
        }

        public override void UpdateCardPreview(
            CardModel card,
            CardPreviewMode previewMode,
            Creature? target,
            bool runGlobalHooks)
        {
            PreviewValue = _previewValueFactory?.Invoke(card, previewMode, target, runGlobalHooks)
                           ?? _currentValueFactory(card);
        }

        protected override decimal GetBaseValueForIConvertible()
        {
            return _currentValueFactory(_owner as CardModel);
        }
    }
}
