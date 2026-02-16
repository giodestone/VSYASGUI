using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Gives data about a player from both the <see cref="IServerPlayer"/> and by extent <see cref="IPlayer"/> (where available).
    /// </summary>
    public class PlayerDetails : ResponseBase
    {
        /// <summary>
        /// <see cref="IPlayer.PlayerName"/>
        /// </summary>
        public string PlayerName { get; init; }

        /// <summary>
        /// <see cref="IPlayer.PlayerUID"/>
        /// </summary>
        public string PlayerUID { get; init; }

        /// <summary>
        /// <see cref="IServerPlayer.IpAddress"/>
        /// </summary>
        public string IpAddress { get; init; }

        /// <summary>
        /// <see cref="IServerPlayer.Ping"/>
        /// </summary>
        public float Ping { get; init; }

        /// <summary>
        /// <see cref="IServerPlayer.LanguageCode"/>
        /// </summary>
        public string LanguageCode { get; init; }

        /// <summary>
        /// <see cref="IPlayer.Role"/>
        /// </summary>
        public string PlayerRoleName { get; init; } // TODO: Set

        /// <summary>
        /// <see cref="IServerPlayer.CurrentChunkSentRadius"/>
        /// </summary>
        public int CurrentChunkSendRadius { get; init; } // TODO: Set

        /// <summary>
        /// <see cref="IServerPlayer.ConnectionState"/>
        /// </summary>
        public EnumClientState ConnectionState { get; init; }

        public static PlayerDetails FromServerPlayer(IServerPlayer player)
        {
            return new PlayerDetails()
            {
                PlayerName = player.PlayerName,
                PlayerUID = player.PlayerUID,
                IpAddress = player.IpAddress,
                Ping = player.Ping,
                LanguageCode = player.LanguageCode,
                PlayerRoleName = player.Role.ToString(),
                CurrentChunkSendRadius = player.CurrentChunkSentRadius,
                ConnectionState = player.ConnectionState,
            };
        }
    }
}
