using Verticality.Moves.ChargedJump;
using Verticality.Moves.Climb;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Verticality
{
    public class VerticalityModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
            api.RegisterEntityBehaviorClass("chargedjump", typeof(EntityBehaviorChargedJump));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            capi.Input.RegisterHotKey("climb", "Climb", GlKeys.LAlt, HotkeyType.MovementControls);
        }
    }
}
