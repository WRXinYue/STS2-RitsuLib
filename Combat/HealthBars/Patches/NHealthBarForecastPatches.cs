using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Utils;

namespace STS2RitsuLib.Combat.HealthBars.Patches
{
    internal static class NHealthBarForecastPatchHelper
    {
        private static readonly AttachedState<NHealthBar, HealthBarForecastUiState?> UiStates = new(() => null);

        private static readonly AccessTools.FieldRef<NHealthBar, Control> HpForegroundContainerRef =
            AccessTools.FieldRefAccess<NHealthBar, Control>("_hpForegroundContainer");

        private static readonly AccessTools.FieldRef<NHealthBar, Control> HpForegroundRef =
            AccessTools.FieldRefAccess<NHealthBar, Control>("_hpForeground");

        private static readonly AccessTools.FieldRef<NHealthBar, Control> PoisonForegroundRef =
            AccessTools.FieldRefAccess<NHealthBar, Control>("_poisonForeground");

        private static readonly AccessTools.FieldRef<NHealthBar, Control> DoomForegroundRef =
            AccessTools.FieldRefAccess<NHealthBar, Control>("_doomForeground");

        private static readonly AccessTools.FieldRef<NHealthBar, Control> HpMiddlegroundRef =
            AccessTools.FieldRefAccess<NHealthBar, Control>("_hpMiddleground");

        private static readonly AccessTools.FieldRef<NHealthBar, MegaLabel> HpLabelRef =
            AccessTools.FieldRefAccess<NHealthBar, MegaLabel>("_hpLabel");

        private static readonly AccessTools.FieldRef<NHealthBar, TextureRect> InfinityTexRef =
            AccessTools.FieldRefAccess<NHealthBar, TextureRect>("_infinityTex");

        private static readonly AccessTools.FieldRef<NHealthBar, LocString> HealthBarDeadRef =
            AccessTools.FieldRefAccess<NHealthBar, LocString>("_healthBarDead");

        private static readonly AccessTools.FieldRef<NHealthBar, Creature> CreatureRef =
            AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");

        private static readonly AccessTools.FieldRef<NHealthBar, Creature?> BlockTrackingCreatureRef =
            AccessTools.FieldRefAccess<NHealthBar, Creature?>("_blockTrackingCreature");

        private static readonly AccessTools.FieldRef<NHealthBar, int> CurrentHpOnLastRefreshRef =
            AccessTools.FieldRefAccess<NHealthBar, int>("_currentHpOnLastRefresh");

        private static readonly AccessTools.FieldRef<NHealthBar, int> MaxHpOnLastRefreshRef =
            AccessTools.FieldRefAccess<NHealthBar, int>("_maxHpOnLastRefresh");

        private static readonly AccessTools.FieldRef<NHealthBar, Tween?> MiddlegroundTweenRef =
            AccessTools.FieldRefAccess<NHealthBar, Tween?>("_middlegroundTween");

        private static readonly AccessTools.FieldRef<NHealthBar, float> ExpectedMaxFgWidthRef =
            AccessTools.FieldRefAccess<NHealthBar, float>("_expectedMaxFgWidth");

        private static readonly Color DefaultFontColor = StsColors.cream;
        private static readonly Color DefaultFontOutlineColor = new("900000");
        private static readonly Color BlockOutlineColor = new("1B3045");
        private static readonly Color PoisonColor = new("79C03C");
        private static readonly Color DoomColor = new("5A42A5");

        public static void EnsureUiState(NHealthBar healthBar)
        {
            if (UiStates[healthBar] != null)
                return;

            var poisonForeground = (NinePatchRect)PoisonForegroundRef(healthBar);
            var hpForeground = HpForegroundRef(healthBar);
            var mask = poisonForeground.GetParent<Control>();

            var rightContainer = CreateContainer("RitsuForecastRightContainer");
            var leftContainer = CreateContainer("RitsuForecastLeftContainer");

            mask.AddChild(rightContainer);
            mask.MoveChild(rightContainer, hpForeground.GetIndex());

            mask.AddChild(leftContainer);
            mask.MoveChild(leftContainer, hpForeground.GetIndex() + 1);

            UiStates[healthBar] = new(
                rightContainer,
                leftContainer,
                CreateSegmentTemplate(poisonForeground),
                []);
        }

        public static void RefreshForeground(NHealthBar healthBar)
        {
            EnsureUiState(healthBar);

            var state = UiStates[healthBar] ??
                        throw new InvalidOperationException("Missing health bar forecast UI state.");
            var creature = CreatureRef(healthBar);
            var hpForeground = HpForegroundRef(healthBar);
            var poisonForeground = PoisonForegroundRef(healthBar);
            var doomForeground = DoomForegroundRef(healthBar);

            poisonForeground.Visible = false;
            doomForeground.Visible = false;
            HideSegments(state.RightSegments);
            HideSegments(state.LeftSegments);
            state.LastRender = HealthBarForecastRenderResult.Empty;

            if (creature.CurrentHp <= 0)
            {
                hpForeground.Visible = false;
                return;
            }

            hpForeground.Visible = true;
            var currentOffsetRight = GetFgWidth(healthBar, creature.CurrentHp) - GetMaxFgWidth(healthBar);
            hpForeground.OffsetRight = currentOffsetRight;

            if (creature.ShowsInfiniteHp)
            {
                hpForeground.SelfModulate = new("C5BBED");
                return;
            }

            var registeredSegments = HealthBarForecastRegistry.GetSegments(creature).ToArray();

            var rightSegments = BuildRightSegments(creature, registeredSegments)
                .OrderBy(segment => segment.Order)
                .ThenBy(segment => segment.SequenceOrder)
                .ToArray();

            var rightEdgeOffsetRight = currentOffsetRight;
            var remainingHp = creature.CurrentHp;
            Color? lethalRightColor = null;
            var rightIndex = 0;

            foreach (var segment in rightSegments)
            {
                if (remainingHp <= 0)
                    break;

                var visibleAmount = Math.Min(segment.Amount, remainingHp);
                if (visibleAmount <= 0)
                    continue;

                EnsureSegmentCount(state.RightSegments, state.RightContainer, rightIndex + 1, state.Template);
                var node = state.RightSegments[rightIndex];
                var previousHp = remainingHp;
                remainingHp -= visibleAmount;

                var leftWidth = GetFgWidth(healthBar, remainingHp);
                var rightWidth = GetFgWidth(healthBar, previousHp);
                var patchMarginLeft = node.PatchMarginLeft;

                node.Visible = true;
                node.SelfModulate = segment.Color;
                node.OffsetLeft = remainingHp > 0 ? Math.Max(0f, leftWidth - patchMarginLeft) : 0f;
                node.OffsetRight = rightWidth - GetMaxFgWidth(healthBar);

                if (rightIndex == 0)
                    rightEdgeOffsetRight = node.OffsetRight;

                if (remainingHp <= 0)
                    lethalRightColor = segment.Color;

                rightIndex++;
            }

            HideSegments(state.RightSegments, rightIndex);

            hpForeground.OffsetRight = GetFgWidth(healthBar, remainingHp) - GetMaxFgWidth(healthBar);
            hpForeground.Visible = remainingHp > 0;

            var leftSegments = BuildLeftSegments(creature, registeredSegments)
                .OrderBy(segment => segment.Order)
                .ThenBy(segment => segment.SequenceOrder)
                .ToArray();

            var leftAccumulated = 0;
            Color? lethalLeftColor = null;
            var leftIndex = 0;

            foreach (var segment in leftSegments)
            {
                if (leftAccumulated >= creature.MaxHp)
                    break;

                var segmentStart = leftAccumulated;
                leftAccumulated = Math.Min(creature.MaxHp, leftAccumulated + segment.Amount);
                if (leftAccumulated <= segmentStart)
                    continue;

                EnsureSegmentCount(state.LeftSegments, state.LeftContainer, leftIndex + 1, state.Template);
                var node = state.LeftSegments[leftIndex];
                var startWidth = GetFgWidth(healthBar, segmentStart);
                var endWidth = GetFgWidth(healthBar, leftAccumulated);

                node.Visible = true;
                node.SelfModulate = segment.Color;
                node.OffsetLeft = segmentStart > 0 ? Math.Max(0f, startWidth - node.PatchMarginLeft) : 0f;
                node.OffsetRight = Math.Min(0f, endWidth - GetMaxFgWidth(healthBar) + node.PatchMarginRight);

                if (lethalLeftColor == null && leftAccumulated >= remainingHp)
                    lethalLeftColor = segment.Color;

                leftIndex++;
            }

            HideSegments(state.LeftSegments, leftIndex);

            state.LastRender = new(
                rightIndex > 0,
                rightEdgeOffsetRight,
                creature.CurrentHp - remainingHp,
                remainingHp,
                lethalRightColor,
                lethalLeftColor);
        }

        public static void RefreshMiddleground(NHealthBar healthBar)
        {
            EnsureUiState(healthBar);

            var creature = CreatureRef(healthBar);
            var hpMiddleground = HpMiddlegroundRef(healthBar);

            if (creature.CurrentHp <= 0)
            {
                hpMiddleground.Visible = false;
                return;
            }

            hpMiddleground.Visible = true;
            var position = hpMiddleground.Position;
            position.X = 1f;
            hpMiddleground.Position = position;

            var currentHp = creature.CurrentHp;
            var maxHp = creature.MaxHp;
            if (currentHp == CurrentHpOnLastRefreshRef(healthBar) && maxHp == MaxHpOnLastRefreshRef(healthBar))
                return;

            CurrentHpOnLastRefreshRef(healthBar) = currentHp;
            MaxHpOnLastRefreshRef(healthBar) = maxHp;

            var render = UiStates[healthBar]?.LastRender ?? HealthBarForecastRenderResult.Empty;
            var targetOffsetRight = render.HasRightForecast
                ? render.RightForecastEdgeOffsetRight
                : HpForegroundRef(healthBar).OffsetRight;

            var shouldAnimateImmediately = targetOffsetRight >= hpMiddleground.OffsetRight;
            hpMiddleground.OffsetRight += 1f;

            MiddlegroundTweenRef(healthBar)?.Kill();
            var tween = healthBar.CreateTween();
            tween.TweenProperty(hpMiddleground, "offset_right", targetOffsetRight - 2f, 1.0)
                .SetDelay(shouldAnimateImmediately ? 0.0 : 1.0)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Expo);
            MiddlegroundTweenRef(healthBar) = tween;
        }

        public static void RefreshText(NHealthBar healthBar)
        {
            EnsureUiState(healthBar);

            var creature = CreatureRef(healthBar);
            var hpLabel = HpLabelRef(healthBar);
            var infinityTex = InfinityTexRef(healthBar);

            if (creature.CurrentHp <= 0)
            {
                hpLabel.AddThemeColorOverride("font_color", DefaultFontColor);
                hpLabel.AddThemeColorOverride("font_outline_color", DefaultFontOutlineColor);
                hpLabel.SetTextAutoSize(HealthBarDeadRef(healthBar).GetRawText());
                return;
            }

            if (creature.ShowsInfiniteHp)
            {
                infinityTex.Visible = creature.IsAlive;
                hpLabel.Visible = !infinityTex.Visible;
                return;
            }

            infinityTex.Visible = false;
            hpLabel.Visible = true;

            var render = UiStates[healthBar]?.LastRender ?? HealthBarForecastRenderResult.Empty;
            var lethalColor = render.LethalRightColor ?? render.LethalLeftColor;
            Color color;
            Color outlineColor;

            if (lethalColor.HasValue)
            {
                color = lethalColor.Value;
                outlineColor = DarkenForOutline(lethalColor.Value);
            }
            else if (creature.Block > 0 || (BlockTrackingCreatureRef(healthBar)?.Block ?? 0) > 0)
            {
                color = DefaultFontColor;
                outlineColor = BlockOutlineColor;
            }
            else
            {
                color = DefaultFontColor;
                outlineColor = DefaultFontOutlineColor;
            }

            hpLabel.AddThemeColorOverride("font_color", color);
            hpLabel.AddThemeColorOverride("font_outline_color", outlineColor);
            hpLabel.SetTextAutoSize($"{creature.CurrentHp}/{creature.MaxHp}");
        }

        private static IEnumerable<ForecastSegmentRenderInfo> BuildRightSegments(
            Creature creature,
            IEnumerable<HealthBarForecastRegistry.RegisteredHealthBarForecastSegment> registeredSegments)
        {
            if (creature.GetPower<PoisonPower>() is { } poison)
            {
                var amount = poison.CalculateTotalDamageNextTurn();
                if (amount > 0)
                    yield return new(
                        amount,
                        PoisonColor,
                        HealthBarForecastOrder.ForSideTurnStart(creature, creature.Side),
                        -2);
            }

            foreach (var registered in registeredSegments)
            {
                if (registered.Segment.Direction != HealthBarForecastGrowthDirection.FromRight)
                    continue;

                yield return new(
                    registered.Segment.Amount,
                    registered.Segment.Color,
                    registered.Segment.Order,
                    registered.SequenceOrder);
            }
        }

        private static IEnumerable<ForecastSegmentRenderInfo> BuildLeftSegments(
            Creature creature,
            IEnumerable<HealthBarForecastRegistry.RegisteredHealthBarForecastSegment> registeredSegments)
        {
            if (creature.HasPower<DoomPower>())
            {
                var amount = creature.GetPowerAmount<DoomPower>();
                if (amount > 0)
                    yield return new(
                        amount,
                        DoomColor,
                        HealthBarForecastOrder.ForSideTurnEnd(creature, creature.Side),
                        -2);
            }

            foreach (var registered in registeredSegments)
            {
                if (registered.Segment.Direction != HealthBarForecastGrowthDirection.FromLeft)
                    continue;

                yield return new(
                    registered.Segment.Amount,
                    registered.Segment.Color,
                    registered.Segment.Order,
                    registered.SequenceOrder);
            }
        }

        private static Control CreateContainer(string name)
        {
            var container = new Control
            {
                Name = name,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };

            container.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return container;
        }

        private static NinePatchRect CreateSegmentTemplate(NinePatchRect template)
        {
            var duplicate = (NinePatchRect)template.Duplicate();
            duplicate.Name = "RitsuForecastSegmentTemplate";
            duplicate.Visible = false;
            duplicate.Material = null;
            duplicate.SelfModulate = Colors.White;
            return duplicate;
        }

        private static void EnsureSegmentCount(
            List<NinePatchRect> segments,
            Control container,
            int requiredCount,
            NinePatchRect template)
        {
            while (segments.Count < requiredCount)
            {
                var segment = (NinePatchRect)template.Duplicate();
                segment.Name = $"RitsuForecastSegment{segments.Count}";
                segment.Visible = false;
                container.AddChild(segment);
                segments.Add(segment);
            }
        }

        private static void HideSegments(IEnumerable<NinePatchRect> segments, int startIndex = 0)
        {
            var index = 0;
            foreach (var segment in segments)
            {
                if (index++ < startIndex)
                    continue;

                segment.Visible = false;
            }
        }

        private static float GetMaxFgWidth(NHealthBar healthBar)
        {
            var expectedMaxFgWidth = ExpectedMaxFgWidthRef(healthBar);
            return expectedMaxFgWidth > 0f
                ? expectedMaxFgWidth
                : HpForegroundContainerRef(healthBar).Size.X;
        }

        private static float GetFgWidth(NHealthBar healthBar, int amount)
        {
            var creature = CreatureRef(healthBar);
            if (creature.MaxHp <= 0 || amount <= 0)
                return 0f;

            var width = (float)amount / creature.MaxHp * GetMaxFgWidth(healthBar);
            return Math.Max(width, creature.CurrentHp > 0 ? 12f : 0f);
        }

        private static Color DarkenForOutline(Color color)
        {
            return new(
                Math.Clamp(color.R * 0.3f, 0f, 1f),
                Math.Clamp(color.G * 0.3f, 0f, 1f),
                Math.Clamp(color.B * 0.3f, 0f, 1f));
        }

        private sealed class HealthBarForecastUiState(
            Control rightContainer,
            Control leftContainer,
            NinePatchRect template,
            List<NinePatchRect> rightSegments)
        {
            public Control RightContainer { get; } = rightContainer;
            public Control LeftContainer { get; } = leftContainer;
            public NinePatchRect Template { get; } = template;
            public List<NinePatchRect> RightSegments { get; } = rightSegments;
            public List<NinePatchRect> LeftSegments { get; } = [];
            public HealthBarForecastRenderResult LastRender { get; set; } = HealthBarForecastRenderResult.Empty;
        }

        private readonly record struct ForecastSegmentRenderInfo(
            int Amount,
            Color Color,
            int Order,
            long SequenceOrder);

        private readonly record struct HealthBarForecastRenderResult(
            bool HasRightForecast,
            float RightForecastEdgeOffsetRight,
            int TotalRightForecastDamage,
            int RemainingHpAfterRightForecast,
            Color? LethalRightColor,
            Color? LethalLeftColor)
        {
            public static HealthBarForecastRenderResult Empty => new(false, 0f, 0, 0, null, null);
        }
    }

    internal sealed class NHealthBarReadyForecastPatch : IPatchMethod
    {
        public static string PatchId => "health_bar_forecast_ready";
        public static string Description => "Attach shared forecast overlay containers to health bars";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NHealthBar), "_Ready")];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(NHealthBar __instance)
        {
            NHealthBarForecastPatchHelper.EnsureUiState(__instance);
        }
    }

    internal sealed class NHealthBarRefreshForegroundForecastPatch : IPatchMethod
    {
        public static string PatchId => "health_bar_forecast_refresh_foreground";
        public static string Description => "Render shared forecast segments on creature health bars";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NHealthBar), "RefreshForeground")];
        }

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(NHealthBar __instance)
        {
            NHealthBarForecastPatchHelper.RefreshForeground(__instance);
            return false;
        }
    }

    internal sealed class NHealthBarRefreshMiddlegroundForecastPatch : IPatchMethod
    {
        public static string PatchId => "health_bar_forecast_refresh_middleground";
        public static string Description => "Keep middleground animation aligned with shared forecast overlays";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NHealthBar), "RefreshMiddleground")];
        }

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(NHealthBar __instance)
        {
            NHealthBarForecastPatchHelper.RefreshMiddleground(__instance);
            return false;
        }
    }

    internal sealed class NHealthBarRefreshTextForecastPatch : IPatchMethod
    {
        public static string PatchId => "health_bar_forecast_refresh_text";
        public static string Description => "Apply shared forecast lethal tinting to health bar text";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NHealthBar), "RefreshText")];
        }

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(NHealthBar __instance)
        {
            NHealthBarForecastPatchHelper.RefreshText(__instance);
            return false;
        }
    }
}
