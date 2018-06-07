using System.ComponentModel;

namespace Cod4ServerBrowser.Models
{
    public class Cod4BrowseServer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsPasswordProtected { get; set; }
        public string HostName { get; set; }
        public string HostNameClean { get; set; }
        public string MapName { get; set; }
        public int MaxPlayers { get; set; }
        public int OnlinePlayers { get; set; }
        public string GameType { get; set; }
        public bool Voice { get; set; }
        public bool PunkBuster { get; set; }
    }
}
