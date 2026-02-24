using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Server;

namespace VSYASGUI
{
    /// <summary>
    /// Configuration for the API.
    /// </summary>
    internal class Config
    {
        public const string ConfigFileName = "VSYASGUI-config.json";
        public string BindURL { get; set; } = "http://127.0.0.1:8181/";
        public string ApiKey { get; set; } = "changeme";
        public int MaxConsoleEntriesCache { get; set; } = 10000;

        /// <summary>
        /// Load the config from the stored file, as defined by <see cref="ConfigFileName"/>, or create a new one at the location if it fails.
        /// </summary>
        /// <returns>A loaded or new config object.</returns>
        /// <exception cref="Exception">May be thrown if loading config fails.</exception>
        public static Config LoadOrCreate(ICoreServerAPI api)
        {
            try
            {
                Config loadedConfig = api.LoadModConfig<Config>(ConfigFileName);
                if (loadedConfig != null)
                {
                    return loadedConfig;
                }
                else
                {
                    Config newConfig = new Config();
                    api.StoreModConfig(newConfig, ConfigFileName);
                    return newConfig;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Save the config to disk.
        /// </summary>
        public void Save(ICoreServerAPI api)
        {
            try
            {
                api.StoreModConfig(this, ConfigFileName);
            }
            catch
            {
                throw;
            }
        }
    }
}
