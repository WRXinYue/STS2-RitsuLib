using Godot;
using STS2RitsuLib.Audio.Internal;
using FileAccess = Godot.FileAccess;

namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     FMOD Studio bank and path probes. For gameplay sounds that should follow vanilla mixer settings, use
    ///     <see cref="GameFmod.Studio" /> instead.
    /// </summary>
    public static class FmodStudioServer
    {
        /// <summary>
        ///     Returns the Godot <c>FmodServer</c> singleton when present.
        /// </summary>
        public static GodotObject? TryGet()
        {
            return FmodStudioGateway.TryGetServer();
        }

        /// <summary>
        ///     Loads a bank from <paramref name="resourcePath" /> using <paramref name="mode" />.
        /// </summary>
        public static bool TryLoadBank(string resourcePath, FmodStudioLoadBankMode mode = FmodStudioLoadBankMode.Normal)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                RitsuLibFramework.Logger.Warn("[Audio] FMOD load_bank: empty path.");
                return false;
            }

            if (!FileAccess.FileExists(resourcePath))
            {
                RitsuLibFramework.Logger.Warn($"[Audio] FMOD load_bank: file not found: {resourcePath}");
                return false;
            }

            if (!FmodStudioGateway.TryCall(out var result, FmodStudioMethodNames.LoadBank, resourcePath, (int)mode))
                return false;

            if (result.VariantType == Variant.Type.Bool)
                return result.AsBool();

            if (result.VariantType == Variant.Type.Nil)
                return false;

            var bank = result.AsGodotObject();
            return bank is not null && GodotObject.IsInstanceValid(bank);
        }

        /// <summary>
        ///     Unloads a previously loaded bank.
        /// </summary>
        public static bool TryUnloadBank(string resourcePath)
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.UnloadBank, resourcePath);
        }

        /// <summary>
        ///     Null when the probe fails; otherwise whether the event path exists in loaded data.
        /// </summary>
        public static bool? TryCheckEventPath(string eventPath)
        {
            if (!FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CheckEventPath, eventPath))
                return null;

            return v.AsBool();
        }

        /// <summary>
        ///     Null when the probe fails; otherwise whether the bus path is valid.
        /// </summary>
        public static bool? TryCheckBusPath(string busPath)
        {
            if (!FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CheckBusPath, busPath))
                return null;

            return v.AsBool();
        }
    }
}
