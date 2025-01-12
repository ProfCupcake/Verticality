using Verticality.Lib;
using Verticality.Moves.ChargedJump;
using Verticality.Moves.Climb;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Verticality
{
    public class VerticalityModSystem : ModSystem
    {
        public static ConfigManager Config
        {
            get; private set;
        }
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
            api.RegisterEntityBehaviorClass("chargedjump", typeof(EntityBehaviorChargedJump));

            Config = new ConfigManager(api);
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            capi.Input.RegisterHotKey("climb", "Climb", GlKeys.LControl, HotkeyType.MovementControls);
        }
    }
}
