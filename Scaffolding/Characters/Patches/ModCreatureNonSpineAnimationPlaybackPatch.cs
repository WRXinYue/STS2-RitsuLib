using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters.Visuals;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace STS2RitsuLib.Scaffolding.Characters.Patches
{
    /// <summary>
    ///     When a creature has no Spine animator, routes <see cref="NCreature.SetAnimationTrigger" /> through
    ///     <see cref="ModCreatureVisualPlayback" /> (cue textures, Godot animators). Co-loading another library that
    ///     patches the same method may run both prefixes; prefer a single stack for creature visuals when possible.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When the character implements
    ///         <see cref="IModCharacterNonSpineAnimationStateMachineFactory" />, the trigger is routed through the
    ///         state machine's <see cref="ModAnimStateMachine.SetTrigger" /> so branches, any-state transitions and
    ///         <see cref="ModAnimState.NextState" /> apply. Otherwise the original single-shot playback path is used.
    ///     </para>
    /// </remarks>
    public class ModCreatureNonSpineAnimationPlaybackPatch : IPatchMethod
    {
        private static readonly ConditionalWeakTable<Node, StateMachineSlot> StateMachinesByVisuals = new();

        /// <inheritdoc cref="IPatchMethod.PatchId" />
        public static string PatchId => "mod_creature_non_spine_animation_playback";

        /// <inheritdoc cref="IPatchMethod.Description" />
        public static string Description =>
            "Play non-Spine combat creature cues via ModCreatureVisualPlayback for SetAnimationTrigger";

        /// <inheritdoc cref="IPatchMethod.IsCritical" />
        public static bool IsCritical => false;

        /// <inheritdoc cref="IPatchMethod.GetTargets" />
        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))];
        }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Returns <see langword="false" /> when playback handled the trigger (skip vanilla no-op / Spine path).
        /// </summary>
        public static bool Prefix(NCreature __instance, string trigger)
        {
            if (__instance.HasSpineAnimation)
                return true;

            if (TryRouteToStateMachine(__instance, trigger))
                return false;

            return !ModCreatureVisualPlayback.TryPlayFromCreatureAnimatorTrigger(__instance, trigger);
        }

        private static bool TryRouteToStateMachine(NCreature creature, string trigger)
        {
            var character = creature.Entity?.Player?.Character;
            if (character is not IModCharacterNonSpineAnimationStateMachineFactory factory)
                return false;

            var visuals = creature.Visuals;
            if (visuals == null || !GodotObject.IsInstanceValid(visuals))
                return false;

            var slot = StateMachinesByVisuals.GetValue(visuals,
                _ => new());

            slot.EnsureBuilt(factory, visuals, character);
            if (slot.StateMachine == null)
                return false;

            slot.StateMachine.SetTrigger(trigger);
            return true;
        }

        private sealed class StateMachineSlot
        {
            private bool _built;
            public ModAnimStateMachine? StateMachine { get; private set; }

            public void EnsureBuilt(IModCharacterNonSpineAnimationStateMachineFactory factory, Node visuals,
                CharacterModel character)
            {
                if (_built)
                    return;

                _built = true;
                StateMachine = factory.TryCreateNonSpineAnimationStateMachine(visuals, character);
            }
        }
    }
}
