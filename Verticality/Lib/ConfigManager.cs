using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Verticality.Lib
{
    public static class ConfigManager
    {
        private const string ConfigFilename = "verticality.json";
        private const string NetChannel = "verticality";

        private static bool receivedConfig = false;

        private static ICoreAPI api;

        private static VerticalityModConfig _modConfig;

        public static VerticalityModConfig modConfig
        {
            get
            {
                if (_modConfig == null) { Reload(); }
                else if (api.Side == EnumAppSide.Client)
                {
                    if (!receivedConfig) Reload();
                }
                return _modConfig;
            }
            set
            {
                _modConfig = value;
            }
        }
        public static void Initialise(ICoreAPI api)
        {
            ConfigManager.api = api;
            api.Network.RegisterChannel(NetChannel)
                .RegisterMessageType<NetMessage_Request>()
                .RegisterMessageType<VerticalityModConfig>();

            switch (api.Side)
            {
                case (EnumAppSide.Client):
                    ((ICoreClientAPI)api).Network.GetChannel(NetChannel).SetMessageHandler<VerticalityModConfig>(ReceiveConfig);
                    break;
                case (EnumAppSide.Server):
                    ((ICoreServerAPI)api).Network.GetChannel(NetChannel).SetMessageHandler<NetMessage_Request>(SendConfig);
                    break;
            }
        }

        public static void Reload()
        {
            switch (api.Side)
            {
                case (EnumAppSide.Client):
                    _modConfig = new VerticalityModConfig();
                    RequestConfig();
                    break;
                case (EnumAppSide.Server):
                    _modConfig = api.LoadModConfig<VerticalityModConfig>(ConfigFilename);
                    if (_modConfig == null)
                    {
                        _modConfig = new VerticalityModConfig();
                        api.StoreModConfig(_modConfig, ConfigFilename);
                    }
                    BroadcastConfig();
                    break;
            }
        }
        public static void RequestConfig()
        {
            ((ICoreClientAPI)api).Network.GetChannel(NetChannel).SendPacket<NetMessage_Request>(new());
        }
        private static void ReceiveConfig(VerticalityModConfig packet)
        {
            modConfig = packet;
            receivedConfig = true;
        }
        private static void SendConfig(IServerPlayer fromPlayer, NetMessage_Request packet)
        {
            ((ICoreServerAPI)api).Network.GetChannel(NetChannel).SendPacket(modConfig, fromPlayer);
        }
        public static void BroadcastConfig()
        {
            ((ICoreServerAPI)api).Network.GetChannel(NetChannel).BroadcastPacket(modConfig);
        }

    }
    [ProtoContract]
    internal class NetMessage_Request
    {
    }
}
