using System.Reflection;
using HarmonyLib;

namespace STS2RitsuLib.Patching.Models
{
    /// <summary>
    ///     Describes a runtime-discovered patch target and the Harmony methods to apply to it.
    /// </summary>
    public sealed class DynamicPatchInfo(
        string id,
        MethodBase originalMethod,
        HarmonyMethod? prefix = null,
        HarmonyMethod? postfix = null,
        HarmonyMethod? transpiler = null,
        HarmonyMethod? finalizer = null,
        bool isCritical = true,
        string? description = null)
    {
        public string Id { get; } = id;
        public MethodBase OriginalMethod { get; } = originalMethod;
        public HarmonyMethod? Prefix { get; } = prefix;
        public HarmonyMethod? Postfix { get; } = postfix;
        public HarmonyMethod? Transpiler { get; } = transpiler;
        public HarmonyMethod? Finalizer { get; } = finalizer;
        public bool IsCritical { get; } = isCritical;

        public string Description { get; } = string.IsNullOrWhiteSpace(description)
            ? $"Patch {originalMethod.DeclaringType?.Name}.{originalMethod.Name}"
            : description;

        public bool HasPatchMethods => Prefix != null || Postfix != null || Transpiler != null || Finalizer != null;

        public override string ToString()
        {
            return $"{Id}: {OriginalMethod.DeclaringType?.Name}.{OriginalMethod.Name}";
        }
    }
}
