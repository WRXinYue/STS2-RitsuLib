using Godot;
using MegaCrit.Sts2.Core.Combat;

namespace STS2RitsuLib.Combat.HealthBars
{
    /// <summary>
    ///     Convenience helpers for building health bar forecast segments.
    /// </summary>
    public static class HealthBarForecasts
    {
        /// <summary>
        ///     Starts a general-purpose sequence builder for <paramref name="context" />.
        /// </summary>
        public static HealthBarForecastSequenceBuilder For(HealthBarForecastContext context)
        {
            return new(context);
        }

        /// <summary>
        ///     Starts a right-growing forecast lane with a fixed <paramref name="color" />.
        /// </summary>
        public static HealthBarForecastLaneBuilder FromRight(HealthBarForecastContext context, Color color)
        {
            return new(For(context), color, HealthBarForecastGrowthDirection.FromRight);
        }

        /// <summary>
        ///     Starts a left-growing forecast lane with a fixed <paramref name="color" />.
        /// </summary>
        public static HealthBarForecastLaneBuilder FromLeft(HealthBarForecastContext context, Color color)
        {
            return new(For(context), color, HealthBarForecastGrowthDirection.FromLeft);
        }

        /// <summary>
        ///     Returns a single segment when <paramref name="amount" /> is positive.
        /// </summary>
        public static IEnumerable<HealthBarForecastSegment> Single(
            int amount,
            Color color,
            HealthBarForecastGrowthDirection direction,
            int order,
            Material? overlayMaterial)
        {
            if (amount <= 0)
                return [];

            return [new(amount, color, direction, order, overlayMaterial)];
        }

        /// <summary>
        ///     Returns a single segment when <paramref name="amount" /> is positive, without a custom material.
        /// </summary>
        public static IEnumerable<HealthBarForecastSegment> Single(
            int amount,
            Color color,
            HealthBarForecastGrowthDirection direction,
            int order = 0)
            => Single(amount, color, direction, order, null);
    }

    /// <summary>
    ///     Mutable builder for one forecast source's ordered segment sequence.
    /// </summary>
    public sealed class HealthBarForecastSequenceBuilder(HealthBarForecastContext context)
    {
        private readonly List<HealthBarForecastSegment> _segments = [];

        /// <summary>
        ///     Forecast context associated with this sequence.
        /// </summary>
        public HealthBarForecastContext Context { get; } = context;

        /// <summary>
        ///     Appends a segment when <paramref name="amount" /> is positive.
        ///     Consecutive segments with identical color/direction/order/material are merged.
        /// </summary>
        public HealthBarForecastSequenceBuilder Add(
            int amount,
            Color color,
            HealthBarForecastGrowthDirection direction,
            int order,
            Material? overlayMaterial)
        {
            if (amount <= 0)
                return this;

            var segment = new HealthBarForecastSegment(amount, color, direction, order, overlayMaterial);
            if (_segments.Count > 0)
            {
                var last = _segments[^1];
                if (CanMerge(last, segment))
                {
                    _segments[^1] = last with { Amount = last.Amount + segment.Amount };
                    return this;
                }
            }

            _segments.Add(segment);
            return this;
        }

        /// <summary>
        ///     Appends a segment without a custom material.
        /// </summary>
        public HealthBarForecastSequenceBuilder Add(
            int amount,
            Color color,
            HealthBarForecastGrowthDirection direction,
            int order = 0)
            => Add(amount, color, direction, order, null);

        /// <summary>
        ///     Appends all positive amounts as consecutive segments.
        /// </summary>
        public HealthBarForecastSequenceBuilder AddRange(
            IEnumerable<int> amounts,
            Color color,
            HealthBarForecastGrowthDirection direction,
            int order,
            Material? overlayMaterial)
        {
            ArgumentNullException.ThrowIfNull(amounts);

            foreach (var amount in amounts)
                Add(amount, color, direction, order, overlayMaterial);

            return this;
        }

        /// <summary>
        ///     Appends all positive amounts as consecutive segments without a custom material.
        /// </summary>
        public HealthBarForecastSequenceBuilder AddRange(
            IEnumerable<int> amounts,
            Color color,
            HealthBarForecastGrowthDirection direction,
            int order = 0)
            => AddRange(amounts, color, direction, order, null);

        /// <summary>
        ///     Appends segments that trigger at the start of <paramref name="triggerSide" />'s turn.
        /// </summary>
        public HealthBarForecastSequenceBuilder AddSideTurnStart(
            CombatSide triggerSide,
            Color color,
            HealthBarForecastGrowthDirection direction,
            params int[] amounts)
        {
            return AddRange(
                amounts,
                color,
                direction,
                HealthBarForecastOrder.ForSideTurnStart(Context.Creature, triggerSide));
        }

        /// <summary>
        ///     Appends segments that trigger at the end of <paramref name="triggerSide" />'s turn.
        /// </summary>
        public HealthBarForecastSequenceBuilder AddSideTurnEnd(
            CombatSide triggerSide,
            Color color,
            HealthBarForecastGrowthDirection direction,
            params int[] amounts)
        {
            return AddRange(
                amounts,
                color,
                direction,
                HealthBarForecastOrder.ForSideTurnEnd(Context.Creature, triggerSide));
        }

        /// <summary>
        ///     Creates a fixed-color right-growing lane on this sequence.
        /// </summary>
        public HealthBarForecastLaneBuilder FromRight(Color color)
        {
            return new(this, color, HealthBarForecastGrowthDirection.FromRight);
        }

        /// <summary>
        ///     Creates a fixed-color left-growing lane on this sequence.
        /// </summary>
        public HealthBarForecastLaneBuilder FromLeft(Color color)
        {
            return new(this, color, HealthBarForecastGrowthDirection.FromLeft);
        }

        /// <summary>
        ///     Returns the built sequence snapshot.
        /// </summary>
        public IReadOnlyList<HealthBarForecastSegment> Build()
        {
            return _segments.Count == 0 ? [] : _segments.ToArray();
        }

        private static bool CanMerge(HealthBarForecastSegment left, HealthBarForecastSegment right)
        {
            return left.Color == right.Color &&
                   left.Direction == right.Direction &&
                   left.Order == right.Order &&
                   ReferenceEquals(left.OverlayMaterial, right.OverlayMaterial);
        }
    }

    /// <summary>
    ///     Convenience wrapper for the common case of one fixed-color forecast lane.
    /// </summary>
    public sealed class HealthBarForecastLaneBuilder(
        HealthBarForecastSequenceBuilder sequence,
        Color color,
        HealthBarForecastGrowthDirection direction)
    {
        /// <summary>
        ///     Parent sequence builder.
        /// </summary>
        public HealthBarForecastSequenceBuilder Sequence { get; } = sequence;

        /// <summary>
        ///     Appends a segment with explicit <paramref name="order" /> and optional <paramref name="overlayMaterial" />.
        /// </summary>
        public HealthBarForecastLaneBuilder Add(int amount, int order, Material? overlayMaterial)
        {
            Sequence.Add(amount, color, direction, order, overlayMaterial);
            return this;
        }

        /// <summary>
        ///     Appends a segment without a custom material.
        /// </summary>
        public HealthBarForecastLaneBuilder Add(int amount, int order = 0)
            => Add(amount, order, null);

        /// <summary>
        ///     Appends multiple segments with the same <paramref name="order" /> and optional <paramref name="overlayMaterial" />.
        /// </summary>
        public HealthBarForecastLaneBuilder AddRange(IEnumerable<int> amounts, int order, Material? overlayMaterial)
        {
            Sequence.AddRange(amounts, color, direction, order, overlayMaterial);
            return this;
        }

        /// <summary>
        ///     Appends multiple segments without a custom material.
        /// </summary>
        public HealthBarForecastLaneBuilder AddRange(IEnumerable<int> amounts, int order = 0)
            => AddRange(amounts, order, null);

        /// <summary>
        ///     Appends segments that trigger at the start of <paramref name="triggerSide" />'s turn.
        /// </summary>
        public HealthBarForecastLaneBuilder AtSideTurnStart(CombatSide triggerSide, params int[] amounts)
        {
            Sequence.AddSideTurnStart(triggerSide, color, direction, amounts);
            return this;
        }

        /// <summary>
        ///     Appends segments that trigger at the end of <paramref name="triggerSide" />'s turn.
        /// </summary>
        public HealthBarForecastLaneBuilder AtSideTurnEnd(CombatSide triggerSide, params int[] amounts)
        {
            Sequence.AddSideTurnEnd(triggerSide, color, direction, amounts);
            return this;
        }

        /// <summary>
        ///     Starts another right-growing lane on the same parent sequence.
        /// </summary>
        public HealthBarForecastLaneBuilder ThenFromRight(Color nextColor)
        {
            return Sequence.FromRight(nextColor);
        }

        /// <summary>
        ///     Starts another left-growing lane on the same parent sequence.
        /// </summary>
        public HealthBarForecastLaneBuilder ThenFromLeft(Color nextColor)
        {
            return Sequence.FromLeft(nextColor);
        }

        /// <summary>
        ///     Returns the built segment snapshot.
        /// </summary>
        public IReadOnlyList<HealthBarForecastSegment> Build()
        {
            return Sequence.Build();
        }
    }
}
