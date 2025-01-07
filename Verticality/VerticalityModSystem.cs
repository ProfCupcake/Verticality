using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Verticality
{
    public class VerticalityModSystem : ModSystem
    {
        ICoreAPI api;
        ICoreClientAPI capi;
        ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            this.capi = capi;

            capi.Input.RegisterHotKey("climb", "Climb", GlKeys.LShift, HotkeyType.MovementControls);
            capi.Input.SetHotKeyHandler("climb", OnKeyClimb);

        }
        private bool OnKeyClimb(KeyCombination combination)
        {
            EntityBehaviorClimb climbB = capi.World.Player.Entity.GetBehavior<EntityBehaviorClimb>();

            climbB.OnKeyClimb();

            return true;
        }
    }
}
