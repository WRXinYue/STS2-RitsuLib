using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class ModEnchantmentTemplate : EnchantmentModel, IModEnchantmentAssetOverrides
    {
        public virtual EnchantmentAssetProfile AssetProfile => EnchantmentAssetProfile.Empty;
        public virtual string? CustomIconPath => AssetProfile.IconPath;
    }
}
