using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace Verticality
{
    internal static class ConfigManager
    {
        private static readonly string configFilename = "verticality.json";
        private static VerticalityModConfig privateModConfig;
        public static VerticalityModConfig modConfig
        {
            get
            {
                if (privateModConfig == null)
                {
                    if (!TryLoadConfig()) InitialiseConfig();
                }
                return privateModConfig;
            }
        }

        public static ICoreAPI api;

        private static bool TryLoadConfig()
        {
            privateModConfig = api.LoadModConfig<VerticalityModConfig>(configFilename);
            return privateModConfig != null;
        }

        private static void InitialiseConfig()
        {
            privateModConfig = new VerticalityModConfig();
            api.StoreModConfig(privateModConfig, configFilename);
        }
    }
    public class VerticalityModConfig
    {
        public float minHeight = 0.5f; // Minmum height from player's feet for a ledge to be climbable
        public float maxHeight = 2.5f; // Maximum height from player's feet for a ledge to be climbable
        public float climbDistance = 0.5f; // Distance player can move from grab point before they are detached
    }
}
