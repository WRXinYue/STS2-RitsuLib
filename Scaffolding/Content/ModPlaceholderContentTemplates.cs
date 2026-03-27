using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Content;

namespace STS2RitsuLib.Scaffolding.Content
{
    /// <summary>
    ///     Base implementation for generated placeholder cards (see
    ///     <see cref="ModContentRegistry.RegisterPlaceholderCard{TPool}(string, PlaceholderCardDescriptor)" />). Mods
    ///     normally do not subclass this; only subclass if you need a hand-written type with the same no-op behavior.
    /// </summary>
    public abstract class ModPlaceholderCardTemplate(
        int baseCost,
        CardType type,
        CardRarity rarity,
        TargetType target,
        bool showInCardLibrary = false)
        : ModCardTemplate(baseCost, type, rarity, target, showInCardLibrary)
    {
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    ///     Base for emitted placeholder relics; prefer
    ///     <see cref="ModContentRegistry.RegisterPlaceholderRelic{TPool}(string, PlaceholderRelicDescriptor)" /> instead of
    ///     subclassing.
    /// </summary>
    public abstract class ModPlaceholderRelicTemplate(
        RelicRarity rarity,
        bool isUsedUp = false,
        bool hasUponPickupEffect = false,
        bool spawnsPets = false,
        bool isStackable = false,
        bool addsPet = false,
        bool showCounter = false,
        int displayAmount = 0,
        bool includeEnergyHoverTip = false,
        int merchantCostOverride = -1,
        bool alwaysAllowedInRun = true,
        string flashSfx = "event:/sfx/ui/relic_activate_general",
        bool shouldFlashOnPlayer = true)
        : ModRelicTemplate
    {
        public override RelicRarity Rarity => rarity;

        public override bool IsUsedUp => isUsedUp;

        public override bool HasUponPickupEffect => hasUponPickupEffect;

        public override bool SpawnsPets => spawnsPets;

        public override bool IsStackable => isStackable;

        public override bool AddsPet => addsPet;

        public override bool ShowCounter => showCounter;

        public override int DisplayAmount => displayAmount;

        protected override bool IncludeEnergyHoverTip => includeEnergyHoverTip;

        public override int MerchantCost => merchantCostOverride >= 0 ? merchantCostOverride : base.MerchantCost;

        public override string FlashSfx => flashSfx;

        public override bool ShouldFlashOnPlayer => shouldFlashOnPlayer;

        public override bool IsAllowed(IRunState runState)
        {
            return alwaysAllowedInRun;
        }
    }

    /// <summary>
    ///     Base for emitted placeholder potions; prefer
    ///     <see cref="ModContentRegistry.RegisterPlaceholderPotion{TPool}(string, PlaceholderPotionDescriptor)" /> instead of
    ///     subclassing.
    /// </summary>
    public abstract class ModPlaceholderPotionTemplate(
        PotionRarity rarity,
        PotionUsage usage,
        TargetType targetType,
        bool canBeGeneratedInCombat = true,
        bool passesCustomUsabilityCheck = true)
        : ModPotionTemplate
    {
        public override PotionRarity Rarity => rarity;

        public override PotionUsage Usage => usage;

        public override TargetType TargetType => targetType;

        public override bool CanBeGeneratedInCombat => canBeGeneratedInCombat;

        public override bool PassesCustomUsabilityCheck => passesCustomUsabilityCheck;

        protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            return Task.CompletedTask;
        }
    }
}
