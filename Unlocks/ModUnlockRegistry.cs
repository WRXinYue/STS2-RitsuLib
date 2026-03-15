using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Diagnostics;

namespace STS2RitsuLib.Unlocks
{
    public sealed class ModUnlockRegistry
    {
        private static readonly Lock SyncRoot = new();

        private static readonly Dictionary<string, ModUnlockRegistry> Registries =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<ModelId, string> RequiredEpochsByModelId = [];
        private static readonly List<PostRunEpochUnlockRule> PostRunRules = [];

        private string? _freezeReason;

        private ModUnlockRegistry(string modId)
        {
            ModId = modId;
        }

        public string ModId { get; }
        public static bool IsFrozen { get; private set; }

        public static ModUnlockRegistry For(string modId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modId);

            lock (SyncRoot)
            {
                if (Registries.TryGetValue(modId, out var registry))
                    return registry;

                registry = new(modId);
                Registries[modId] = registry;
                return registry;
            }
        }

        public void RequireEpoch<TModel, TEpoch>()
            where TModel : AbstractModel
            where TEpoch : EpochModel, new()
        {
            RequireEpoch(typeof(TModel), new TEpoch().Id);
        }

        public void RequireEpoch(Type modelType, string epochId)
        {
            EnsureMutable($"register unlock requirement for '{modelType.Name}'");
            ArgumentNullException.ThrowIfNull(modelType);
            ArgumentException.ThrowIfNullOrWhiteSpace(epochId);

            RegistrationConflictDetector.ThrowIfModelIdConflicts(modelType);
            var modelId = ModelDb.GetId(modelType);

            lock (SyncRoot)
            {
                RequiredEpochsByModelId[modelId] = epochId;
            }
        }

        public void UnlockEpochAfterRunAs<TCharacter, TEpoch>()
            where TCharacter : CharacterModel
            where TEpoch : EpochModel, new()
        {
            RegisterPostRunRule(
                PostRunEpochUnlockRule.Create(
                    new TEpoch().Id,
                    $"Unlock {typeof(TEpoch).Name} after finishing a run as {typeof(TCharacter).Name}",
                    context => context.CharacterId == ModelDb.GetId<TCharacter>()));
        }

        public void UnlockEpochAfterWinAs<TCharacter, TEpoch>()
            where TCharacter : CharacterModel
            where TEpoch : EpochModel, new()
        {
            RegisterPostRunRule(
                PostRunEpochUnlockRule.Create(
                    new TEpoch().Id,
                    $"Unlock {typeof(TEpoch).Name} after winning as {typeof(TCharacter).Name}",
                    context => context.IsVictory && context.CharacterId == ModelDb.GetId<TCharacter>()));
        }

        public void UnlockEpochAfterAscensionWin<TCharacter, TEpoch>(int ascensionLevel)
            where TCharacter : CharacterModel
            where TEpoch : EpochModel, new()
        {
            RegisterPostRunRule(
                PostRunEpochUnlockRule.Create(
                    new TEpoch().Id,
                    $"Unlock {typeof(TEpoch).Name} after winning at ascension {ascensionLevel} as {typeof(TCharacter).Name}",
                    context => context.IsVictory &&
                               context.CharacterId == ModelDb.GetId<TCharacter>() &&
                               context.AscensionLevel >= ascensionLevel));
        }

        public void UnlockEpochAfterRunCount<TEpoch>(int requiredRuns, bool requireVictory = false)
            where TEpoch : EpochModel, new()
        {
            RegisterPostRunRule(
                PostRunEpochUnlockRule.Create(
                    new TEpoch().Id,
                    $"Unlock {typeof(TEpoch).Name} after {requiredRuns} run(s)",
                    context => context.TotalRuns >= requiredRuns && (!requireVictory || context.IsVictory)));
        }

        public void RegisterPostRunRule(PostRunEpochUnlockRule rule)
        {
            EnsureMutable($"register post-run epoch rule '{rule.Description}'");
            ArgumentNullException.ThrowIfNull(rule);

            lock (SyncRoot)
            {
                PostRunRules.Add(rule);
            }
        }

        internal static void FreezeRegistrations(string reason)
        {
            lock (SyncRoot)
            {
                if (IsFrozen)
                    return;

                IsFrozen = true;
                foreach (var registry in Registries.Values)
                    registry._freezeReason = reason;
            }
        }

        internal static bool IsUnlocked(AbstractModel model, UnlockState unlockState)
        {
            lock (SyncRoot)
            {
                return !RequiredEpochsByModelId.TryGetValue(model.Id, out var epochId) ||
                       unlockState.ToSerializable().UnlockedEpochs.Contains(epochId) ||
                       SaveManager.Instance.IsEpochRevealed(epochId);
            }
        }

        internal static IEnumerable<TModel> FilterUnlocked<TModel>(IEnumerable<TModel> source, UnlockState unlockState)
            where TModel : AbstractModel
        {
            return source.Where(model => IsUnlocked(model, unlockState)).ToArray();
        }

        internal static void ProcessRunEnded(RunManager runManager, SerializableRun serializableRun, bool isVictory,
            bool isAbandoned)
        {
            ArgumentNullException.ThrowIfNull(runManager);
            ArgumentNullException.ThrowIfNull(serializableRun);

            var localPlayer = LocalContext.GetMe(serializableRun);
            if (localPlayer == null)
                return;

            PostRunEpochUnlockRule[] rules;
            lock (SyncRoot)
            {
                rules = PostRunRules.ToArray();
            }

            if (rules.Length == 0)
                return;

            if (localPlayer.CharacterId == null) return;
            var context = new PostRunUnlockContext(
                serializableRun,
                localPlayer,
                isVictory,
                isAbandoned,
                SaveManager.Instance.Progress.NumberOfRuns,
                SaveManager.Instance.Progress.Wins,
                localPlayer.CharacterId,
                serializableRun.Ascension);

            foreach (var rule in rules)
            {
                if (SaveManager.Instance.Progress.IsEpochObtained(rule.EpochId))
                    continue;

                if (!rule.ShouldUnlock(context))
                    continue;

                SaveManager.Instance.ObtainEpoch(rule.EpochId);
                if (!localPlayer.DiscoveredEpochs.Contains(rule.EpochId, StringComparer.Ordinal))
                    localPlayer.DiscoveredEpochs.Add(rule.EpochId);

                var livePlayer = LocalContext.GetMe(runManager.State);
                if (livePlayer != null && !livePlayer.DiscoveredEpochs.Contains(rule.EpochId, StringComparer.Ordinal))
                    livePlayer.DiscoveredEpochs.Add(rule.EpochId);

                RitsuLibFramework.Logger.Info(
                    $"[Unlocks] Obtained epoch '{rule.EpochId}' via post-run rule: {rule.Description}");
            }
        }

        private void EnsureMutable(string operation)
        {
            if (!IsFrozen)
                return;

            throw new InvalidOperationException(
                $"Cannot {operation} after unlock registration has been frozen ({_freezeReason ?? "unknown"}). " +
                "Register unlock rules from your mod initializer before model initialization.");
        }
    }

    public sealed record PostRunUnlockContext(
        SerializableRun Run,
        SerializablePlayer LocalPlayer,
        bool IsVictory,
        bool IsAbandoned,
        int TotalRuns,
        int TotalWins,
        ModelId CharacterId,
        int AscensionLevel);

    public sealed record PostRunEpochUnlockRule(
        string EpochId,
        string Description,
        Func<PostRunUnlockContext, bool> ShouldUnlock)
    {
        public static PostRunEpochUnlockRule Create(string epochId, string description,
            Func<PostRunUnlockContext, bool> shouldUnlock)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(epochId);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            ArgumentNullException.ThrowIfNull(shouldUnlock);
            return new(epochId, description, shouldUnlock);
        }
    }
}
