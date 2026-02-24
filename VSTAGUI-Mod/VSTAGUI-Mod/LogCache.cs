using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory;
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
        long _FirstLine = 0; // TODO: long is an imperfect solution to the 32bit int overflow issue, as it will just overflow later.
        long _LastLine = 0;

        public LogCache(ICoreServerAPI api, Config config) 
        {
            api.Logger.EntryAdded += OnLoggerEntryAdded;

            _Config = config;

            _Cache = new Queue<string>(_Config.MaxConsoleEntriesCache);
        }
        
        /// <summary>
        /// Get the full log. Expensive operation as it gets ALL cached lines.
        /// </summary>
        public void GetLog(long fromLine, out List<string> lines, out long firstLineNumber, out long lastLineNumber)
        {
            if (fromLine >= _LastLine)
            {
                lines = new List<string>(0);
                fromLine = _LastLine;
                lastLineNumber = _LastLine;
            }

            List<string> filteredLines = new List<string>((int)(_LastLine - fromLine));

            // TODO: FIX THIS. IF ENTRIES GET PURGED THERE IS A MAJOR BUG. THERE NEEDS TO BE COMPENSATION FOR THE BEGINNING OF THE SEQUENCE.

            for (int i = 0; i < _Cache.Count; i++)
            {
                if (_FirstLine + i >= fromLine)
                    filteredLines.Add(_Cache.ElementAt(i));
            }

            _Cache.Foreach(entry => filteredLines.Add(entry));
            lines = filteredLines;
            firstLineNumber = fromLine;
            lastLineNumber = _LastLine;
        }

        /// <summary>
        /// Callback for <see cref="ILogger.EntryAdded"/>.<br/>
        /// Caches lines for later retrival, until the max is exceeded as defiend by <see cref="Config.MaxConsoleEntriesCache"/>.
        /// </summary>
        private void OnLoggerEntryAdded(EnumLogType logType, string message, object[] args)
        {
            var time = DateTime.Now;
            _Cache.Enqueue(time.ToShortDateString() + " " + time.ToShortTimeString() + " [" + logType.ToString() + "] " + string.Format(message, args));
            _LastLine++;

            if (_LastLine == uint.MaxValue)
                _LastLine = 0;

            while (_Cache.Count > _Config.MaxConsoleEntriesCache)
            {
                _FirstLine++;
                _Cache.Dequeue();
                // TODO: This would probably be much better as a batch operation every x seconds.
            }
        }
    }
}
