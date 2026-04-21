using Godot;

namespace STS2RitsuLib.Scaffolding.Visuals.StateMachine.Backends
{
    /// <summary>
    ///     <see cref="IAnimationBackend" /> dispatcher that selects the first child backend reporting
    ///     <see cref="IAnimationBackend.HasAnimation" /> for a given id and routes <see cref="Play" /> /
    ///     <see cref="Queue" /> to it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Priority follows insertion order. Typical wiring (cue frame sequences and static textures first,
    ///         then Spine, then Godot animation player, then animated sprite) is produced by
    ///         <see cref="STS2RitsuLib.Scaffolding.Visuals.StateMachine.ModAnimStateMachineBuilder" />.
    ///     </para>
    ///     <para>
    ///         Only one child is <c>active</c> at a time; switching to a different backend during
    ///         <see cref="Play" /> raises <see cref="Interrupted" /> for the previously active id.
    ///     </para>
    /// </remarks>
    public sealed class CompositeAnimationBackend : IAnimationBackend
    {
        private readonly IReadOnlyList<IAnimationBackend> _backends;
        private IAnimationBackend? _active;
        private string? _currentId;

        /// <summary>
        ///     Creates a composite from <paramref name="backends" /> (priority order).
        /// </summary>
        public CompositeAnimationBackend(IReadOnlyList<IAnimationBackend> backends, Node? ownerNode = null)
        {
            ArgumentNullException.ThrowIfNull(backends);
            if (backends.Count == 0)
                throw new ArgumentException("At least one backend is required.", nameof(backends));

            _backends = backends;
            OwnerNode = ownerNode ?? backends[0].OwnerNode;

            foreach (var backend in _backends)
            {
                backend.Started += id => OnChildStarted(backend, id);
                backend.Completed += id => OnChildCompleted(backend, id);
                backend.Interrupted += id => OnChildInterrupted(backend, id);
            }
        }

        /// <inheritdoc />
        public Node? OwnerNode { get; }

        /// <inheritdoc />
        public event Action<string>? Started;

        /// <inheritdoc />
        public event Action<string>? Completed;

        /// <inheritdoc />
        public event Action<string>? Interrupted;

        /// <inheritdoc />
        public bool HasAnimation(string id)
        {
            return _backends.Any(backend => backend.HasAnimation(id));
        }

        /// <inheritdoc />
        public void Play(string id, bool loop)
        {
            var chosen = Resolve(id);
            if (chosen == null)
                return;

            if (_active != null && !ReferenceEquals(_active, chosen) && _currentId != null)
                Interrupted?.Invoke(_currentId);

            _active = chosen;
            _currentId = id;
            chosen.Play(id, loop);
        }

        /// <inheritdoc />
        public void Queue(string id, bool loop)
        {
            var chosen = Resolve(id);
            if (chosen == null)
                return;

            if (ReferenceEquals(_active, chosen))
            {
                chosen.Queue(id, loop);
                return;
            }

            Play(id, loop);
        }

        private IAnimationBackend? Resolve(string id)
        {
            return _backends.FirstOrDefault(backend => backend.HasAnimation(id));
        }

        private void OnChildStarted(IAnimationBackend backend, string id)
        {
            if (!ReferenceEquals(backend, _active))
                return;

            Started?.Invoke(id);
        }

        private void OnChildCompleted(IAnimationBackend backend, string id)
        {
            if (!ReferenceEquals(backend, _active))
                return;

            Completed?.Invoke(id);
        }

        private void OnChildInterrupted(IAnimationBackend backend, string id)
        {
            if (!ReferenceEquals(backend, _active))
                return;

            Interrupted?.Invoke(id);
        }
    }
}
