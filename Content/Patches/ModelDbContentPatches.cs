using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Content.Patches
{
    public class AllCharactersPatch : IPatchMethod
    {
        public static string PatchId => "modeldb_all_characters";
        public static string Description => "Append registered characters to ModelDb.AllCharacters";
        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), "get_AllCharacters")];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref IEnumerable<CharacterModel> __result)
        {
            __result = ModContentRegistry.AppendCharacters(__result);
        }
    }

    public class AllSharedEventsPatch : IPatchMethod
    {
        public static string PatchId => "modeldb_all_shared_events";
        public static string Description => "Append registered shared events to ModelDb.AllSharedEvents";
        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), "get_AllSharedEvents")];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref IEnumerable<EventModel> __result)
        {
            __result = ModContentRegistry.AppendSharedEvents(__result);
        }
    }

    public class AllEventsPatch : IPatchMethod
    {
        public static string PatchId => "modeldb_all_events";
        public static string Description => "Append registered shared events to ModelDb.AllEvents";
        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), "get_AllEvents")];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref IEnumerable<EventModel> __result)
        {
            __result = ModContentRegistry.AppendSharedEvents(__result);
        }
    }

    public class AllSharedAncientsPatch : IPatchMethod
    {
        public static string PatchId => "modeldb_all_shared_ancients";
        public static string Description => "Append registered shared ancients to ModelDb.AllSharedAncients";
        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), "get_AllSharedAncients")];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref IEnumerable<AncientEventModel> __result)
        {
            __result = ModContentRegistry.AppendSharedAncients(__result);
        }
    }

    public class AllAncientsPatch : IPatchMethod
    {
        public static string PatchId => "modeldb_all_ancients";
        public static string Description => "Append registered shared ancients to ModelDb.AllAncients";
        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), "get_AllAncients")];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref IEnumerable<AncientEventModel> __result)
        {
            __result = ModContentRegistry.AppendSharedAncients(__result);
        }
    }
}
