using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Verticality.Lib
{
    public class ConfigManager
    {
        private readonly string ConfigFilename;
        private readonly string NetChannel;

        private bool receivedConfig = false;

        private ICoreAPI api;
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        private VerticalityModConfig _modConfig;

        public VerticalityModConfig modConfig
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

        public ConfigManager(ICoreAPI api, string filename, string netchannel)
        {
            ConfigFilename = filename;
            NetChannel = netchannel;
            
            this.api = api;
            switch (api.Side)
            {
                case EnumAppSide.Client:
                    capi = api as ICoreClientAPI;
                    break;
                case EnumAppSide.Server:
                    sapi = api as ICoreServerAPI;
                    break;
            }

            api.Network.RegisterChannel(NetChannel)
                .RegisterMessageType<NetMessage_Request>()
                .RegisterMessageType<VerticalityModConfig>();

            switch (api.World.Side)
            {
                case (EnumAppSide.Client):
                    capi.Network.GetChannel(NetChannel).SetMessageHandler<VerticalityModConfig>(ReceiveConfig);
                    break;
                case (EnumAppSide.Server):
                    sapi.Network.GetChannel(NetChannel).SetMessageHandler<NetMessage_Request>(SendConfig);
                    Reload();
                    break;
            }
        }

        public void Reload()
        {
            switch (api.Side)
            {
                case (EnumAppSide.Client):
                    _modConfig = new VerticalityModConfig();
                    RequestConfig();
                    break;
                case (EnumAppSide.Server):
                    api.Logger.Event("[{0}] trying to load config", new object[] { NetChannel });
                    _modConfig = api.LoadModConfig<VerticalityModConfig>(ConfigFilename);
                    if (_modConfig == null)
                    {
                        api.Logger.Event("[{0}] generating new config", new object[] { NetChannel });
                        _modConfig = new VerticalityModConfig();
                        api.StoreModConfig(_modConfig, ConfigFilename);
                    }
                    BroadcastConfig();
                    break;
            }
        }
        public void RequestConfig()
        {
            capi.Network.GetChannel(NetChannel).SendPacket<NetMessage_Request>(new());
        }
        private void ReceiveConfig(VerticalityModConfig packet)
        {
            _modConfig = packet;
            receivedConfig = true;
            api.Logger.Event("[{0}] received mod config from server", new object[] { NetChannel });
        }
        private void SendConfig(IServerPlayer fromPlayer, NetMessage_Request packet)
        {
            api.Logger.Event("[{0}] sending mod config to client {1}", new object[] {NetChannel, fromPlayer.PlayerName});
            sapi.Network.GetChannel(NetChannel).SendPacket(modConfig, fromPlayer);
        }
        public void BroadcastConfig()
        {
            sapi.Network.GetChannel(NetChannel).BroadcastPacket(modConfig);
        }

    }
    [ProtoContract]
    internal class NetMessage_Request {}
}
