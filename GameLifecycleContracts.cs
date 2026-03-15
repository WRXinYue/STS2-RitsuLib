using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib
{
    public readonly record struct EssentialInitializationStartingEvent(
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct EssentialInitializationCompletedEvent(
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct DeferredInitializationStartingEvent(
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct DeferredInitializationCompletedEvent(
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct ContentRegistrationClosedEvent(
        string Reason,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct ModelRegistryInitializingEvent(
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ModelRegistryInitializedEvent(
        int RegisteredModelTypeCount,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct ModelIdsInitializingEvent(
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ModelIdsInitializedEvent(
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct ModelPreloadingStartingEvent(
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ModelPreloadingCompletedEvent(
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct GameTreeEnteredEvent(
        NGame Game,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct GameReadyEvent(
        NGame Game,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct RunStartedEvent(
        RunState RunState,
        bool IsMultiplayer,
        bool IsDaily,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RunLoadedEvent(
        RunState RunState,
        bool IsMultiplayer,
        bool IsDaily,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RunEndedEvent(
        SerializableRun Run,
        bool IsVictory,
        bool IsAbandoned,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
