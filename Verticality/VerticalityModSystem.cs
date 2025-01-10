using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Verticality
{
    public class VerticalityModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            capi.Input.RegisterHotKey("climb", "Climb", GlKeys.LShift, HotkeyType.MovementControls);
        }
    }
}
