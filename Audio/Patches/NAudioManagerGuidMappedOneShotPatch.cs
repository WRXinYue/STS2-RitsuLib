using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.TestSupport;
using STS2RitsuLib.Audio.Internal;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Audio.Patches
{
    /// <summary>
    ///     When mod banks omit <c>.strings.bank</c>, Godot path lookup fails; GUID mappings from
    ///     <c>FmodStudioServer.TryLoadStudioGuidMappings</c> restore vanilla <c>NAudioManager</c> one-shots for mapped
    ///     <c>event:/…</c> paths.
    /// </summary>
    public class NAudioManagerGuidMappedOneShotPatch : IPatchMethod
    {
        /// <inheritdoc />
        public static string PatchId => "naudio_manager_guid_mapped_one_shot";

        /// <inheritdoc />
        public static string Description =>
            "Redirect NAudioManager one-shots to GUID-backed instances when paths are registered from guids.txt";

        /// <inheritdoc />
        public static bool IsCritical => false;

        /// <inheritdoc />
        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(NAudioManager), "PlayOneShot",
                    [typeof(string), typeof(Dictionary<string, float>), typeof(float)]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Harmony prefix: returns false to skip vanilla when a guid mapping handles playback.
        /// </summary>
        public static bool Prefix(NAudioManager __instance, string path, Dictionary<string, float> parameters,
            float volume)
        {
            _ = __instance;

            if (TestMode.IsOn)
                return true;

            if (string.IsNullOrEmpty(path) ||
                !FmodStudioGuidPathTable.TryGetStudioGuidForEventPath(path, out var mappedGuid))
                return true;

            if (FmodStudioDirectOneShots.TryFireOneShotForMappedEventPath(path, volume, parameters))
                return false;

            RitsuLibFramework.Logger.Warn(
                "[Audio] Mapped FMOD one-shot failed. " +
                FmodStudioMappedOneShotDiagnostics.BuildMappedOneShotFailureDetail(path, mappedGuid));

            return false;
        }
    }
}
