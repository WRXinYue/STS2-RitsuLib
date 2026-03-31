namespace STS2RitsuLib.Scaffolding.Content
{
    /// <summary>
    ///     Declarative pack step (timeline, unlocks, or other <see cref="ModContentPackContext" /> surfaces), like
    ///     <see cref="IContentRegistrationEntry" /> but for the full pack context.
    /// </summary>
    public interface IModContentPackEntry
    {
        /// <summary>
        ///     Runs this step during <see cref="ModContentPackBuilder.Apply" />.
        /// </summary>
        void Apply(ModContentPackContext context);
    }
}
