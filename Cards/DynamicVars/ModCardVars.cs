using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace STS2RitsuLib.Cards.DynamicVars
{
    public static class ModCardVars
    {
        public static IntVar Int(string name, decimal amount)
        {
            return new(name, amount);
        }

        public static StringVar String(string name, string value = "")
        {
            return new(name, value);
        }
    }
}
