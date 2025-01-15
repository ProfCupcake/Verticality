using HarmonyLib;
using Verticality.Lib;
using Verticality.Moves.ChargedJump;
using Verticality.Moves.Climb;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Verticality
{
    public class VerticalityModSystem : ModSystem
    {
        public const string patchName = "com.profcupcake.verticality";
        public const string clientConfigFilename = "verticality-client.json";

        Harmony harmony;
        public static ConfigManager Config
        {
            get; private set;
        }

        public static VerticalityClientModConfig ClientConfig
        {
            get; private set;
        }

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);

            harmony = new(patchName);
            harmony.PatchAll();
        }
        public override void Dispose()
        {
            base.Dispose();

            harmony.UnpatchAll(patchName);
        }
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
            api.RegisterEntityBehaviorClass("chargedjump", typeof(EntityBehaviorChargedJump));

            Config = new ConfigManager(api, "verticality.json", "verticality");
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            capi.Logger.Event("[verticality] trying to load client config");
            ClientConfig = capi.LoadModConfig<VerticalityClientModConfig>(clientConfigFilename);
            if (ClientConfig == null)
            {
                capi.Logger.Event("[verticality] generating new client config");
                ClientConfig = new VerticalityClientModConfig();
                capi.StoreModConfig(ClientConfig, clientConfigFilename);
            } else capi.Logger.Event("[verticality] client config loaded");

            capi.Input.RegisterHotKey("climb", "Climb", GlKeys.R, HotkeyType.MovementControls);
        }
    }
}
