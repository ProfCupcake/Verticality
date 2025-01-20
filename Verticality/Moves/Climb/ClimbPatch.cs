using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace Verticality.Moves.Climb
{
    [HarmonyPatch]
    public static class ClimbPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EntityBehaviorPlayerPhysics), nameof(EntityBehaviorPlayerPhysics.SetModules))]
        public static void SetGrabPModule(EntityBehaviorPlayerPhysics __instance)
        {
            List<PModule> physicsModules = Traverse.Create(__instance).Field("physicsModules").GetValue<List<PModule>>();
            physicsModules.Add(new PModuleGrab());
        }
    }
}
