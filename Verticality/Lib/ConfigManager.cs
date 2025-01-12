using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Verticality.Lib
{
    public class ConfigManager
    {
        private const string ConfigFilename = "verticality.json";
        private const string NetChannel = "verticality";

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

        public ConfigManager(ICoreAPI api)
        {
            this.api = api;
            api.Logger.Event("[verticality] initialising networking");
            switch (api.Side)
            {
                case EnumAppSide.Client:
                    capi = api as ICoreClientAPI;
                    api.Logger.Event("[verticality] network client init");
                    break;
                case EnumAppSide.Server:
                    sapi = api as ICoreServerAPI;
                    api.Logger.Event("[verticality] network server init");
                    break;
            }

            api.Network.RegisterChannel(NetChannel)
                .RegisterMessageType<NetMessage_Request>()
                .RegisterMessageType<VerticalityModConfig>();

            switch (api.World.Side)
            {
                case (EnumAppSide.Client):
                    capi.Network.GetChannel(NetChannel).SetMessageHandler<VerticalityModConfig>(ReceiveConfig);
                    api.Logger.Event("[verticality] client setMessageHandler set");
                    break;
                case (EnumAppSide.Server):
                    sapi.Network.GetChannel(NetChannel).SetMessageHandler<NetMessage_Request>(SendConfig);
                    api.Logger.Event("[verticality] server setMessageHandler set");
                    Reload();
                    break;
            }

            if (sapi != null) sapi.Logger.Event("[verticality] server side network init complete");
            if (capi != null) capi.Logger.Event("[verticality] client side network init complete");
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
                    api.Logger.Event("[verticality] trying to load config");
                    _modConfig = api.LoadModConfig<VerticalityModConfig>(ConfigFilename);
                    if (_modConfig == null)
                    {
                        api.Logger.Event("[verticality] generating new config");
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
            api.Logger.Event("[verticality] received mod config from server");
        }
        private void SendConfig(IServerPlayer fromPlayer, NetMessage_Request packet)
        {
            api.Logger.Event("[verticality] sending mod config to client {0}", new object[] {fromPlayer.PlayerName});
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
