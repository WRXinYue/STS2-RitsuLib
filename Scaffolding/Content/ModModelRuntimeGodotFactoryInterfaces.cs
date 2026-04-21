using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace STS2RitsuLib.Scaffolding.Content
{
    /// <summary>
    ///     Runtime <see cref="NCreatureVisuals" /> for mod monsters. Subclass <see cref="ModMonsterTemplate" /> or implement
    ///     on the model type; non-null <see cref="TryCreateCreatureVisuals" /> replaces path-based
    ///     <c>CreateVisuals</c>.
    /// </summary>
    public interface IModMonsterCreatureVisualsFactory
    {
        /// <summary>
        ///     Combat visuals from code, or <c>null</c> to use asset paths.
        /// </summary>
        NCreatureVisuals? TryCreateCreatureVisuals();
    }

    /// <summary>
    ///     Runtime <see cref="NCreatureVisuals" /> for mod characters; same contract as
    ///     <see cref="IModMonsterCreatureVisualsFactory" />.
    /// </summary>
    public interface IModCharacterCreatureVisualsFactory
    {
        /// <summary>
        ///     Combat visuals from code, or <c>null</c> to use asset paths.
        /// </summary>
        NCreatureVisuals? TryCreateCreatureVisuals();
    }

    /// <summary>
    ///     Runtime encounter combat root <see cref="Control" />. Set <see cref="SuppliesEncounterCombatSceneFromFactory" />
    ///     when using a factory without <c>CustomEncounterScenePath</c> so <c>HasScene</c> stays correct.
    /// </summary>
    public interface IModEncounterCombatSceneFactory
    {
        /// <summary>
        ///     <c>true</c> when this encounter provides a scene only via the factory (no path).
        /// </summary>
        bool SuppliesEncounterCombatSceneFromFactory { get; }

        /// <summary>
        ///     Combat UI root from code, or <c>null</c> to load the default encounter scene.
        /// </summary>
        Control? TryCreateEncounterCombatScene();
    }

    /// <summary>
    ///     Runtime layout <see cref="PackedScene" /> for <see cref="MegaCrit.Sts2.Core.Models.EventModel.CreateScene" />.
    /// </summary>
    public interface IModEventLayoutPackedSceneFactory
    {
        /// <summary>
        ///     Layout scene from code, or <c>null</c> to resolve <c>LayoutScenePath</c>.
        /// </summary>
        PackedScene? TryCreateLayoutPackedScene();
    }

    /// <summary>
    ///     Runtime background <see cref="PackedScene" /> for
    ///     <see cref="MegaCrit.Sts2.Core.Models.EventModel.CreateBackgroundScene" />.
    /// </summary>
    public interface IModEventBackgroundPackedSceneFactory
    {
        /// <summary>
        ///     Background scene from code, or <c>null</c> to use path resolution.
        /// </summary>
        PackedScene? TryCreateBackgroundPackedScene();
    }

    /// <summary>
    ///     Runtime event VFX <see cref="Node2D" />. Use <see cref="SuppliesCustomEventVfx" /> when VFX is code-built and
    ///     there is no VFX scene file on disk.
    /// </summary>
    public interface IModEventVfxFactory
    {
        /// <summary>
        ///     <c>true</c> when <see cref="TryCreateEventVfx" /> should run instead of loading the default VFX path.
        /// </summary>
        bool SuppliesCustomEventVfx { get; }

        /// <summary>
        ///     VFX root from code, or <c>null</c> to fall through to path-based loading.
        /// </summary>
        Node2D? TryCreateEventVfx();
    }

    /// <summary>
    ///     Runtime orb presentation <see cref="Node2D" /> for <c>OrbModel.CreateSprite</c>. Match the node shape and animation
    ///     setup that vanilla expects (e.g. Spine idle) if other systems assume it.
    /// </summary>
    public interface IModOrbSpriteFactory
    {
        /// <summary>
        ///     Orb sprite node from code, or <c>null</c> to instantiate from the visuals scene path.
        /// </summary>
        Node2D? TryCreateOrbSprite();
    }

    /// <summary>
    ///     Runtime Spine <see cref="CreatureAnimator" /> factory for mod characters. Overrides the default vanilla
    ///     <c>GenerateAnimator</c> so mods can wire custom <see cref="AnimState" /> graphs (idle / hit / attack / cast /
    ///     die / relaxed) without subclassing <c>NCreature</c>. Prefer
    ///     <see cref="STS2RitsuLib.Scaffolding.Visuals.StateMachine.ModAnimStateMachines.Standard" /> for the standard
    ///     shape; return <see langword="null" /> to fall through to vanilla behaviour.
    /// </summary>
    public interface IModCharacterCreatureAnimatorFactory
    {
        /// <summary>
        ///     Returns a fully wired <see cref="CreatureAnimator" />, or <see langword="null" /> to defer to vanilla.
        /// </summary>
        /// <param name="controller">Spine controller attached to the character's combat visuals.</param>
        CreatureAnimator? TryCreateCreatureAnimator(MegaSprite controller);
    }

    /// <summary>
    ///     Runtime non-Spine <see cref="ModAnimStateMachine" /> factory for mod characters' combat visuals
    ///     (cue frame sequences, cue textures, Godot animation player, animated sprite). Implement when the model
    ///     needs automatic transitions (e.g. <c>attack</c> -&gt; <c>idle</c>) without a Spine skeleton.
    /// </summary>
    public interface IModCharacterNonSpineAnimationStateMachineFactory
    {
        /// <summary>
        ///     Builds a state machine bound to <paramref name="visualsRoot" />, or <see langword="null" /> to defer
        ///     to the single-shot playback path.
        /// </summary>
        /// <param name="visualsRoot">Combat visuals root (typically an <see cref="NCreatureVisuals" />).</param>
        /// <param name="character">Owning character model for cue lookup.</param>
        ModAnimStateMachine? TryCreateNonSpineAnimationStateMachine(Node visualsRoot, CharacterModel character);
    }

    /// <summary>
    ///     Runtime <see cref="ModAnimStateMachine" /> factory for mod characters in merchant / rest-site contexts.
    ///     Implement when the merchant visuals need state transitions rather than single-shot playback.
    /// </summary>
    public interface IModCharacterMerchantAnimationStateMachineFactory
    {
        /// <summary>
        ///     Builds a merchant-context state machine, or <see langword="null" /> to defer to the single-shot path.
        /// </summary>
        /// <param name="merchantRoot">Merchant character root.</param>
        /// <param name="character">Owning character model for cue lookup.</param>
        ModAnimStateMachine? TryCreateMerchantAnimationStateMachine(Node merchantRoot, CharacterModel character);
    }
}
