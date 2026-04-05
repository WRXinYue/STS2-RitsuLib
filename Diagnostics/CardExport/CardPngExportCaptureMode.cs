namespace STS2RitsuLib.Diagnostics.CardExport
{
    /// <summary>
    ///     How much surrounding UI to include when rasterizing a card.
    /// </summary>
    public enum CardPngExportCaptureMode
    {
        /// <summary>
        ///     Only the <c>NCard</c> control (game-accurate card chrome, portrait, text).
        /// </summary>
        CardOnly,

        /// <summary>
        ///     Card plus a right-hand column that approximates hover tips: text tips use the real
        ///     <c>hover_tip.tscn</c>; card-reference tips render as a scaled mini <c>NCard</c>.
        ///     Layout is fixed (not the same global positioning as in-game tooltips).
        /// </summary>
        CardWithHoverTipsPanel,
    }
}
