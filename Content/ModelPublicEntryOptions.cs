namespace STS2RitsuLib.Content
{
    /// <summary>
    ///     Controls how <see cref="ModContentRegistry" /> assigns the patched public <c>ModelDb.GetEntry(Type)</c> string
    ///     (the stable segment used in saves and localization keys for RitsuLib-registered models).
    /// </summary>
    public readonly record struct ModelPublicEntryOptions
    {
        internal ModelPublicEntryOptions(ModelPublicEntryKind kind, string? value)
        {
            Kind = kind;
            Value = value;
        }

        /// <summary>
        ///     Uses the default rule: <c>&lt;MOD&gt;_&lt;CATEGORY&gt;_&lt;CLR_TYPE_NAME&gt;</c> (normalized).
        /// </summary>
        public static ModelPublicEntryOptions FromTypeName => default;

        internal ModelPublicEntryKind Kind { get; }

        internal string? Value { get; }

        /// <summary>
        ///     Replaces the CLR type-name segment with a stable author-chosen stem (normalized).
        ///     Final entry: <c>&lt;MOD&gt;_&lt;CATEGORY&gt;_&lt;STEM&gt;</c>.
        /// </summary>
        public static ModelPublicEntryOptions FromStem(string entryStem)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryStem);
            return new(ModelPublicEntryKind.Stem, entryStem);
        }

        /// <summary>
        ///     Uses the given public entry string verbatim after normalization (must match the patched entry format).
        /// </summary>
        public static ModelPublicEntryOptions FromFullPublicEntry(string fullPublicEntry)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullPublicEntry);
            return new(ModelPublicEntryKind.FullEntry, fullPublicEntry);
        }
    }

    internal enum ModelPublicEntryKind
    {
        FromTypeName = 0,
        Stem = 1,
        FullEntry = 2,
    }
}
