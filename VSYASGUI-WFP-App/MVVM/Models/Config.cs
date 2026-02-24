using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// User config for the app. Must be serializable into JSON.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Active instance of the config.
        /// </summary>
        public static Config Instance { get; protected set; }

        public const string FileName = "settings.json";

        /// <summary>
        /// API key which the user has selected.
        /// </summary>
        public string CurrentApiKey { get; set; } = "";
        /// <summary>
        /// History of API keys.
        /// </summary>
        public List<string> ApiKeyHistory { get; set; } = ["changeme", "test2"];

        /// <summary>
        /// Endpoint which the user has selected.
        /// </summary>
        public string CurrentEndpoint { get; set; } = ""; // e.g. http://127.0.0.1:8181/

        /// <summary>
        /// History of endpoint addresses.
        /// </summary>
        public List<string> EndpointAddresses { get; set; } = ["http://127.0.0.1:8181/", "test2"];

        /// <summary>
        /// Minimum interval for how often the server will be polled for realtime resources.
        /// </summary>
        public int ServerPollIntervalMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Text for an error box that says it has failed to load or create the config file.
        /// </summary>
        [JsonIgnore]
        public string FailedToCreateOrLoadConfigText => $"Unable to load or create the configuration file. \n\nNo user settings will be saved. \n\nTry:\n * Removing the config file at {Config.GetPathToConfig()}\n* Checking if your disk is full.\n* That you have write permissions.";

        /// <summary>
        /// Get a URL ready to have the API URI added.
        /// </summary>
        [JsonIgnore]
        public string GetUrlForApi { get => CurrentEndpoint.TrimEnd('/'); }

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
        /// <br/>
        /// <br/>
        /// See also: <seealso cref="FailedToCreateOrLoadConfigText"/>
        /// </summary>
        public static bool TryLoadOrCreate()
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

        /// <summary>
        /// Try to remove the existing config file.
        /// </summary>
        /// <remarks>
        /// Will not re-create instance, or recreate the file. You must use <see cref="TryLoadOrCreate"/>.
        /// </remarks>
        /// <returns>True if deletion was successful. False if it was not.</returns>
        public bool TryDeleteConfig()
        {
            try
            {
                File.Delete(GetPathToConfig());

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Failed to delete config file.");
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
