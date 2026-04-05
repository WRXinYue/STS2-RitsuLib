namespace STS2RitsuLib.Diagnostics.CardExport
{
    /// <summary>
    ///     Parameters for a batch PNG export of <see cref="MegaCrit.Sts2.Core.Models.CardModel" /> instances.
    /// </summary>
    public readonly struct CardPngExportRequest
    {
        /// <summary>
        ///     Absolute or Godot <c>user://</c> / <c>res://</c> output directory. Invalid path characters in card ids are
        ///     stripped from file names.
        /// </summary>
        public string OutputDirectory { get; init; }

        /// <summary>
        ///     Uniform scale applied to the card (and panel layout). Values below 1 shrink; above 1 enlarge (e.g. 2 for
        ///     higher-resolution exports).
        /// </summary>
        public float Scale { get; init; }

        /// <summary>
        ///     Rasterization mode.
        /// </summary>
        public CardPngExportCaptureMode CaptureMode { get; init; }

        /// <summary>
        ///     When true, also exports an <c>_upgraded</c> PNG for cards where
        ///     <see cref="MegaCrit.Sts2.Core.Models.CardModel.IsUpgradable" /> is true.
        /// </summary>
        public bool IncludeUpgradedVariants { get; init; }

        /// <summary>
        ///     When set, only cards whose <see cref="MegaCrit.Sts2.Core.Models.ModelId.Entry" /> contains this substring
        ///     (ordinal ignore-case) are exported.
        /// </summary>
        public string? IdFilterSubstring { get; init; }

        /// <summary>
        ///     When positive, stops after this many <em>base</em> cards (upgraded variants do not count toward the cap).
        /// </summary>
        public int MaxBaseCards { get; init; }

        /// <summary>
        ///     When false (default), only exports cards that appear in the in-game card library
        ///     (<see cref="MegaCrit.Sts2.Core.Models.CardModel.ShouldShowInCardLibrary" />), matching the compendium set.
        ///     When true, also includes registered cards that are hidden from the library.
        /// </summary>
        public bool IncludeCardsHiddenFromLibrary { get; init; }

        /// <summary>
        ///     Defaults: <see cref="Scale" /> = 1, <see cref="CaptureMode" /> = <see cref="CardPngExportCaptureMode.CardOnly" />,
        ///     <see cref="IncludeUpgradedVariants" /> = true.
        /// </summary>
        public static CardPngExportRequest CreateDefault(string outputDirectory)
        {
            return new()
            {
                OutputDirectory = outputDirectory,
                Scale = 1f,
                CaptureMode = CardPngExportCaptureMode.CardOnly,
                IncludeUpgradedVariants = true,
                MaxBaseCards = 0,
            };
        }
    }
}
