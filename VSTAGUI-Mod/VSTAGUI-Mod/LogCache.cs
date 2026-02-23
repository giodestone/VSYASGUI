using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using VSYASGUI;

namespace VSYASGUI_Mod
{
    internal class LogCache
    {
        Config _Config;

        Queue<string> _Cache; // TODO: Remake this to better function as an array of chars to avoid addtional allocations and speed up traversal.
        uint lastLine = 0;

        public LogCache(ICoreServerAPI api, Config config) 
        {
            api.Logger.EntryAdded += OnLoggerEntryAdded;

            _Config = config;

            _Cache = new Queue<string>(_Config.MaxConsoleEntriesCache);
        }
        
        /// <summary>
        /// Get the full log. Expensive operation as it gets ALL cached lines.
        /// </summary>
        public string[] GetLog()
        {
            List<string> totalString = new List<string>(_Cache.Count);
            _Cache.Foreach(entry => totalString.Add(entry));
            return totalString.ToArray();
        }

        /// <summary>
        /// Callback for <see cref="ILogger.EntryAdded"/>.<br/>
        /// Caches lines for later retrival, until the max is exceeded as defiend by <see cref="Config.MaxConsoleEntriesCache"/>.
        /// </summary>
        private void OnLoggerEntryAdded(EnumLogType logType, string message, object[] args)
        {
            var time = DateTime.Now;
            _Cache.Enqueue(time.ToShortDateString() + " " + time.ToShortTimeString() + " [" + logType.ToString() + "] " + message);
            lastLine++;

            if (lastLine == uint.MaxValue)
                lastLine = 0;

            while (_Cache.Count > _Config.MaxConsoleEntriesCache)
            {
                _Cache.Dequeue();
                // TODO: This would probably be much better as a batch operation every x seconds.
            }
        }
    }
}
