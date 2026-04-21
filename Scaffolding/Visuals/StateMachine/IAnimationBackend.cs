using Godot;

namespace STS2RitsuLib.Scaffolding.Visuals.StateMachine
{
    /// <summary>
    ///     Uniform driver surface required by <see cref="ModAnimStateMachine" /> so the same state graph can run on
    ///     Spine (<c>MegaSprite</c>), Godot <c>AnimationPlayer</c>, <c>AnimatedSprite2D</c>, or cue-frame-sequence
    ///     playback (see <see cref="STS2RitsuLib.Scaffolding.Visuals.Definition.VisualCueSet" />).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implementations raise <see cref="Started" />, <see cref="Completed" />, and <see cref="Interrupted" />
    ///         whenever the underlying system reports the corresponding events so the state machine can advance
    ///         <see cref="ModAnimState.NextState" />.
    ///     </para>
    ///     <para>
    ///         <see cref="Queue" /> is only meaningful for backends with true queue semantics (Spine); other backends
    ///         may forward it to <see cref="Play" /> or defer until <see cref="Completed" /> fires.
    ///     </para>
    /// </remarks>
    public interface IAnimationBackend
    {
        /// <summary>
        ///     Backend owner node (visuals root, merchant root, etc.); <see langword="null" /> when not applicable.
        /// </summary>
        Node? OwnerNode { get; }

        /// <summary>
        ///     Fired when the backend reports playback start for animation id <c>arg</c>.
        /// </summary>
        event Action<string>? Started;

        /// <summary>
        ///     Fired when the backend reports playback completion (loop cycle end or one-shot end) for id <c>arg</c>.
        /// </summary>
        event Action<string>? Completed;

        /// <summary>
        ///     Fired when the backend reports playback interruption for id <c>arg</c>.
        /// </summary>
        event Action<string>? Interrupted;

        /// <summary>
        ///     Returns <see langword="true" /> when the backend can play <paramref name="id" />.
        /// </summary>
        bool HasAnimation(string id);

        /// <summary>
        ///     Plays <paramref name="id" /> immediately (replaces any active animation).
        /// </summary>
        /// <param name="id">Animation id; must satisfy <see cref="HasAnimation" />.</param>
        /// <param name="loop">Loop hint; backends without looping support should treat this as a best-effort flag.</param>
        void Play(string id, bool loop);

        /// <summary>
        ///     Queues <paramref name="id" /> after the currently active animation. Non-queue backends may treat this
        ///     as a deferred <see cref="Play" /> triggered on the next <see cref="Completed" />.
        /// </summary>
        void Queue(string id, bool loop);
    }
}
