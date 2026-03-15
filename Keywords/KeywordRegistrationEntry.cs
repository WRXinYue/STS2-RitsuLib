namespace STS2RitsuLib.Keywords
{
    public sealed record KeywordRegistrationEntry(
        string Id,
        string TitleTable,
        string TitleKey,
        string DescriptionTable,
        string DescriptionKey,
        string? IconPath = null)
    {
        public void Register(ModKeywordRegistry registry)
        {
            registry.Register(Id, TitleTable, TitleKey, DescriptionTable, DescriptionKey, IconPath);
        }

        public static KeywordRegistrationEntry Card(string id, string locKeyPrefix, string? iconPath = null)
        {
            return new(
                id,
                "card_keywords",
                $"{locKeyPrefix}.title",
                "card_keywords",
                $"{locKeyPrefix}.description",
                iconPath);
        }
    }
}
