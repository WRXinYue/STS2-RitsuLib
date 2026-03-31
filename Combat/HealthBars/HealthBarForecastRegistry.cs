using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace STS2RitsuLib.Combat.HealthBars
{
    /// <summary>
    ///     Which side of the health bar a forecast segment should grow from.
    /// </summary>
    public enum HealthBarForecastGrowthDirection
    {
        /// <summary>
        ///     Grows inward from the current HP edge, like poison.
        /// </summary>
        FromRight = 0,

        /// <summary>
        ///     Grows outward from the empty side, like doom.
        /// </summary>
        FromLeft = 1,
    }

    /// <summary>
    ///     One forecast overlay segment for a creature health bar.
    /// </summary>
    /// <param name="Amount">HP amount represented by this segment.</param>
    /// <param name="Color">Overlay tint.</param>
    /// <param name="Direction">Which edge the segment grows from.</param>
    /// <param name="Order">
    ///     Lower values are rendered earlier in the chain.
    ///     For <see cref="HealthBarForecastGrowthDirection.FromRight" />, earlier segments stay closer to the current HP
    ///     edge; for <see cref="HealthBarForecastGrowthDirection.FromLeft" />, earlier segments stay closer to the empty
    ///     edge.
    /// </param>
    public readonly record struct HealthBarForecastSegment(
        int Amount,
        Color Color,
        HealthBarForecastGrowthDirection Direction,
        int Order = 0);

    /// <summary>
    ///     Helpers for common turn-relative ordering of forecast segments.
    /// </summary>
    public static class HealthBarForecastOrder
    {
        /// <summary>
        ///     Returns an order key for effects that trigger at the start of <paramref name="triggerSide" />'s turn.
        /// </summary>
        public static int ForSideTurnStart(Creature creature, CombatSide triggerSide)
        {
            ArgumentNullException.ThrowIfNull(creature);
            return creature.CombatState?.CurrentSide == triggerSide ? 1 : 0;
        }

        /// <summary>
        ///     Returns an order key for effects that trigger at the end of <paramref name="triggerSide" />'s turn.
        /// </summary>
        public static int ForSideTurnEnd(Creature creature, CombatSide triggerSide)
        {
            ArgumentNullException.ThrowIfNull(creature);
            return creature.CombatState?.CurrentSide == triggerSide ? 0 : 1;
        }
    }

    /// <summary>
    ///     Global registry of health bar forecast providers contributed by mods.
    /// </summary>
    public static class HealthBarForecastRegistry
    {
        private static readonly Lock SyncRoot = new();
        private static readonly Dictionary<(string ModId, string ProviderId), ProviderEntry> Providers = [];
        private static long _nextRegistrationOrder;

        /// <summary>
        ///     Registers or replaces a forecast provider for <paramref name="modId" />.
        /// </summary>
        public static void Register<TSource>(string modId, string? sourceId = null)
            where TSource : IHealthBarForecastSource, new()
        {
            Register(modId, sourceId ?? typeof(TSource).FullName ?? typeof(TSource).Name, new TSource());
        }

        /// <summary>
        ///     Registers or replaces a forecast source instance for <paramref name="modId" />.
        /// </summary>
        public static void Register(
            string modId,
            string sourceId,
            IHealthBarForecastSource source)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modId);
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
            ArgumentNullException.ThrowIfNull(source);

            lock (SyncRoot)
            {
                var key = (modId, sourceId);
                var registrationOrder = Providers.TryGetValue(key, out var existing)
                    ? existing.RegistrationOrder
                    : _nextRegistrationOrder++;

                Providers[key] = new(modId, sourceId, source, registrationOrder);
            }
        }

        /// <summary>
        ///     Removes a previously registered provider.
        /// </summary>
        public static bool Unregister(string modId, string sourceId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modId);
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);

            lock (SyncRoot)
            {
                return Providers.Remove((modId, sourceId));
            }
        }

        internal static IReadOnlyList<RegisteredHealthBarForecastSegment> GetSegments(Creature creature)
        {
            ArgumentNullException.ThrowIfNull(creature);

            var context = new HealthBarForecastContext(creature);
            List<RegisteredHealthBarForecastSegment> segments = [];

            var powerSequenceOrder = 0L;
            // ReSharper disable once SuspiciousTypeConversion.Global
            foreach (var source in creature.Powers.OfType<IHealthBarForecastSource>())
                AppendSegments(
                    source,
                    source.GetType().FullName ?? source.GetType().Name,
                    context,
                    powerSequenceOrder++,
                    segments);

            ProviderEntry[] snapshot;
            lock (SyncRoot)
            {
                snapshot = Providers.Values.OrderBy(entry => entry.RegistrationOrder).ToArray();
            }

            const long externalSourceOrderOffset = 1_000_000L;
            foreach (var entry in snapshot)
                AppendSegments(
                    entry.Source,
                    entry.SourceId,
                    context,
                    externalSourceOrderOffset + entry.RegistrationOrder,
                    segments,
                    entry.ModId);

            return segments;
        }

        private static void AppendSegments(
            IHealthBarForecastSource source,
            string sourceId,
            HealthBarForecastContext context,
            long sequenceOrder,
            List<RegisteredHealthBarForecastSegment> segments,
            string? modId = null)
        {
            try
            {
                var providedSegments = source.GetHealthBarForecastSegments(context);
                segments.AddRange(from segment in providedSegments
                    where segment.Amount > 0
                    select new RegisteredHealthBarForecastSegment(segment, sequenceOrder));
            }
            catch (Exception ex)
            {
                var ownerText = modId == null ? "runtime source" : $"mod '{modId}'";
                RitsuLibFramework.Logger.Warn(
                    $"[HealthBarForecast] Source '{sourceId}' from {ownerText} failed for creature '{context.Creature}': {ex}");
            }
        }

        internal readonly record struct RegisteredHealthBarForecastSegment(
            HealthBarForecastSegment Segment,
            long SequenceOrder);

        private readonly record struct ProviderEntry(
            string ModId,
            string SourceId,
            IHealthBarForecastSource Source,
            long RegistrationOrder);
    }
}
