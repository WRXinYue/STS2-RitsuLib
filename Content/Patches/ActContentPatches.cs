using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Content.Patches
{
    /// <summary>
    ///     Bootstrap dynamic act patching after all mods are loaded but before ModelDb begins caching content.
    ///     This avoids hardcoding base-game acts and supports act/map mods from other assemblies.
    /// </summary>
    public class DynamicActContentPatchBootstrap : IPatchMethod
    {
        public static string PatchId => "dynamic_act_content_patch_bootstrap";

        public static string Description =>
            "Dynamically patch all loaded ActModel implementations for registered events and ancients";

        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), nameof(ModelDb.Init))];
        }

        public static void Prefix()
        {
            DynamicActContentPatcher.EnsurePatched();
        }
    }
}
