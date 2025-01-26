using HarmonyLib;
using System;
using Verticality.Lib;
using Verticality.Moves.ChargedJump;
using Verticality.Moves.Climb;
using Verticality.Moves.Crawl;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Verticality
{
    public class VerticalityModSystem : ModSystem
    {
        public const string patchName = "com.profcupcake.verticality";
        public const string clientConfigFilename = "verticality-client.json";
        public const string crawlNetChannel = "verticality:crawl";

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

            GlobalConstants.BaseJumpForce = 8.2f;

            harmony.UnpatchAll(patchName);
        }
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("climb", typeof(EntityBehaviorClimb));
            api.RegisterEntityBehaviorClass("chargedjump", typeof(EntityBehaviorChargedJump));
            api.RegisterEntityBehaviorClass("crawl", typeof(EntityBehaviorCrawl));

            Config = new ConfigManager(api, "verticality.json", "verticality");

            api.Network.RegisterChannel(crawlNetChannel)
                .RegisterMessageType<IsCrawlingPacket>();

            if (api.Side == EnumAppSide.Server)
            {
                ((ICoreServerAPI)api).Network.GetChannel(crawlNetChannel)
                    .SetMessageHandler<IsCrawlingPacket>((IServerPlayer player, IsCrawlingPacket packet) =>
                    {
                        player.Entity.GetBehavior<EntityBehaviorCrawl>().IsCrawling = packet.isCrawling;
                    });
            }
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
            capi.Input.RegisterHotKey("crawl", "Crawl (Single-Key Option)", GlKeys.Z, HotkeyType.MovementControls);
        }

        
    }
}
