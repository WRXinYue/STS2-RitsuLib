namespace STS2RitsuLib
{
    public interface IFrameworkLifecycleEvent
    {
        DateTimeOffset OccurredAtUtc { get; }
    }

    public interface IReplayableFrameworkLifecycleEvent : IFrameworkLifecycleEvent
    {
    }

    public readonly record struct FrameworkInitializingEvent(
        string FrameworkModId,
        string FrameworkVersion,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct FrameworkInitializedEvent(
        string FrameworkModId,
        bool IsActive,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct ProfileServicesInitializingEvent(
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProfileServicesInitializedEvent(
        int ProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public interface ILifecycleObserver
    {
        void OnEvent(IFrameworkLifecycleEvent evt);
    }

    internal sealed class FrameworkLifecycleSubscription(Action unsubscribe) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            unsubscribe();
        }
    }
}
