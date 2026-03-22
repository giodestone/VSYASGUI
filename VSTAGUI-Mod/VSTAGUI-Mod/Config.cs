using System;
using Vintagestory.API.Server;

namespace VSYASGUI_Mod
{
    /// <summary>
    /// Configuration for the server mod.
    /// </summary>
    internal class Config
    {
        public const string ConfigFileName = "VSYASGUI_Mod-config.json";
        public string BindURL { get; set; } = "https://127.0.0.1:8181/";
        public string ApiKey { get; set; } = "changeme";
        public int MaxConsoleEntriesCache { get; set; } = 10000;
        public int CPUUsagePollTimerMs { get; set; } = 2000;
        public bool EnableHttps { get; set; } = true;
        public string HttpsPrivateCertificateFileName { get; set; } = "key.pem";
        public string HttpsPublicCertificateFileName { get; set; } = "cert.pem";
        public int HttpsDefaultKeyDurationDays { get; set; } = 365*2;
        public bool HttpsRegenerateAfterExpiryOnRestart { get; set; } = true;


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
