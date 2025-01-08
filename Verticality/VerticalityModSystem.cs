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

            ConfigManager.api = api;

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            this.capi = capi;

            capi.Input.RegisterHotKey("climb", "Climb", GlKeys.LShift, HotkeyType.MovementControls);
        }
    }
}
