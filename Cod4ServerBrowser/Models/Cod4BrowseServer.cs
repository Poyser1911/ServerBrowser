using Cod4.Core.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace Cod4ServerBrowser.Models
{
    public class Cod4BrowseServer 
    {
        public bool IsPasswordProtected { get; set; }
        public string HostName { get; set; }
        public string Connect { get; set; }
        public string HostNameClean { get; set; }
        public string MapName { get; set; }
        public int MaxPlayers { get; set; }
        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();
        public int OnlinePlayers { get; set; }
        public string GameType { get; set; }
        public bool Voice { get; set; }
        public bool PunkBuster { get; set; }
    }
}
