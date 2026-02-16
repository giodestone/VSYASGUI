using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace VSYASGUI_WFP_App
{
    /// <summary>
    /// User config for the app. Must be serializable into JSON.
    /// </summary>
    internal class Config
    {
        public static Config Instance { get; protected set; }

        public const string FileName = "settings.json";

        public string[] ApiKeys { get; set; }

        public string[] EndpointAddresses { get; set; }

        /// <summary>
        /// Tries to write the config file to disk in the same directory as the executable.
        /// </summary>
        /// <returns>True if succeeded, false if failed.</returns>
        public bool TrySave()
        {
            var pathToConfig = GetPathToConfig();
            if (pathToConfig == string.Empty)
                return false;
            try
            {
                var configSerialized = JsonSerializer.Serialize(this);
                File.WriteAllText(pathToConfig, configSerialized);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Failed to write correctly to file.");
                Console.WriteLine(e.Message);
                return false;
            }
            
        }

        /// <summary>
        /// Load the config from a file, or create a new one if non functional.<br/>
        /// 
        /// If there is a load error, returns false.
        /// </summary>
        public static bool LoadOrCreate()
        {
            var pathToConfig = GetPathToConfig();
            if (pathToConfig == string.Empty)
            {
                Instance = new Config();
                return false;
            }

            try
            {
                if (File.Exists(pathToConfig))
                {
                    var configFile = File.ReadAllText(pathToConfig);
                    Instance = JsonSerializer.Deserialize<Config>(configFile) ?? new Config();
                    return true;
                }
                else
                {
                    var conf = new Config();
                    conf.TrySave();
                    Instance = conf;
                    return true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to load, deserialise, or create the config file.");
                Console.WriteLine(e.Message);
                Instance = new Config();
                return false;
            }
        }

        /// <summary>
        /// Get the absolute path to the config file (or where it should be - existance is not checked).<br/>
        ///
        /// Returns string.Empty in rare circumstances.
        /// </summary>
        public static string GetPathToConfig()
        {
            string pathToConfig = string.Empty;

            try
            {
                pathToConfig = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Config.FileName;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Directory.GetCurrentDirectory() is null. The application was probably launched in strange ways or there is a permission issue.");
                Console.WriteLine(e.Message);
            }

            return pathToConfig;
        }
    }
}
