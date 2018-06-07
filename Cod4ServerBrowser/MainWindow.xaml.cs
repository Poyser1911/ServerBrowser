using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Cod4.Core.Master;
using Cod4.Core.Messages;
using Cod4.Core.Models;
using Cod4.Core.Parse;
using Cod4ServerBrowser.Models;

namespace Cod4ServerBrowser
{
    public class Test
    {
        public string HostName { get; set; }
        public string Map { get; set; }
        public string GameType { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static UdpClient _udp;
        private static List<ServerInfo> _servers = new List<ServerInfo>();
        private static Cod4MasterQuery _master;
        private static int _serversParsed = 0;
        public ObservableCollection<Cod4BrowseServer> Servers { get; set; } = new ObservableCollection<Cod4BrowseServer>();
        public MainWindow()
        {
            InitializeComponent();
            // Refresh.Click += (s, e) => RefreshServers();
            Init();
            DataContext = this;
        }

        private void Init()
        {
            UdpReceiver();
            _master = new Cod4MasterQuery();
            _master.OnGetServersRequestError += OnError;
            _master.OnError += OnError;
            if (!_master.Init())
                MessageBox.Show("Error While Initializing");
            _master.OnGetServersCompleted += Q_OnQueryCompleted;
            RefreshServers();
        }

        private void RefreshServers()
        {
            _master.GetServers();
        }
        private static void Q_OnQueryCompleted(List<Cod4.Core.Models.Server> servers)
        {
            Task.Run(() =>
            {
                foreach (Server server in servers)
                {
                    _udp.Send(MessageFactory.GetStatus, MessageFactory.GetStatus.Length,
                        new IPEndPoint(IPAddress.Parse(server.IP), Int32.Parse(server.Port)));
                    Thread.Sleep(1);
                }
            });
        }

        private void UdpReceiver()
        {
            _udp = new UdpClient(new IPEndPoint(IPAddress.Any, 28962));
            Task.Run(() =>
            {
                byte[] data = new byte[805];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    while (true)
                    {
                        try
                        {
                            data = _udp.Receive(ref sender);
                            // Console.WriteLine($"Packet #{count++} Received with [{data.Length} bytes] from { sender.Address}");

                            LogServer(new Source() { IP = sender.Address.ToString(), Port = sender.Port, Status = Encoding.Default.GetString(data) }, true, true);
                        }
                        catch (Exception) { }
                    }

                }
                catch (Exception e) { OnError(e.Message); }
            });
        }
        public void LogServer(Source source, bool getdvars = true, bool getplayers = true)
        {
            Task.Run(() =>
            {
                if (source.Status.Contains("punk"))
                {
                   // Console.WriteLine("Test");
                }
                ServerInfo result;
                try
                {
                    if (Parser.GetServerInfo(source, getdvars, getplayers, out result))
                    {
                        try
                        {
                            lock (_servers)
                            {
                                _servers.Add(result);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return;
                        }
                        _serversParsed++;
                        Dispatcher.Invoke(() =>
                        {
                            Servers.Add(new Cod4BrowseServer
                            {
                                IsPasswordProtected = result.Dvars.GetVal("pswrd") == "1",
                                HostName = RemoveColours(result.Dvars.GetVal("sv_hostname")),
                                HostNameClean = RemoveColours(result.Dvars.GetVal("sv_hostname")),
                                MapName = FriendlyMapName(result.Dvars.GetVal("mapname")),
                                OnlinePlayers = result.Players.Count,
                                MaxPlayers = Regex.Match(result.Dvars.GetVal("sv_maxclients"), "^\\d{1,2}$").Success ? Int32.Parse(result.Dvars.GetVal("sv_maxclients")) : 0,
                                GameType = FriendlyGametype(result.Dvars.GetVal("g_gametype")),
                                Voice = result.Dvars.GetVal("sv_voice") == "1",
                                PunkBuster = result.Dvars.GetVal("sv_punkbuster") == "1"
                            });
                            /*if (DataGrid.Items.Count > 0)
                            {
                                var border = VisualTreeHelper.GetChild(DataGrid, 0) as Decorator;
                                if (border != null)
                                {
                                    var scroll = border.Child as ScrollViewer;
                                    if (scroll != null) scroll.ScrollToEnd();
                                }
                            }*/
                            // ServerCount.Content = $"Servers: {_serversParsed}";
                        });
                        // Console.Write($"\rServers Prepared: {_serversParsed}");
                    }
                    else
                        OnError($"Error Getting Server Info For: {source.IP}:{source.Port}");

                }
                catch (Exception e)
                {
                    OnError($"Unexpected Error: {e.StackTrace}");
                }
            });
        }

        private void OnError(string errormessage)
        {
            Console.WriteLine(errormessage);
        }

        public static string RemoveColours(string s)
        {
            return Regex.Replace(s, @"\^\d", "");
        }

        public static string FriendlyGametype(string gametype)
        {
            switch (gametype)
            {
                case "dm": return "Free For All";
                case "war": return "Team Deathmatch";
                case "sd": return "Search And Destroy";
                case "sab": return "Sabotage";
                case "Koth": return "Headquarters";
                case "dom": return "Domination";
                case "ctf": return "Capture The Flag";
                case "cj": return "CodJump";
                case "obs_elim": return "Obscurity";
                case "deathrun": return "Deathrun";
                case "ktk": return "Kill The King";
                case "oitc": return "One in the Chamber";
                case "surv": return "Survival";
                case "hns": return "Hide and Seek";
                default: return gametype;
            }
        }

        public static string FriendlyMapName(string mapname)
        {
            if (Regex.Match(mapname, @"^([A-Za-z0-9]+[_]{1}[A-Za-z0-9]{2,100})+$").Success)
            {
                int startIndex = mapname.IndexOf('_') + 1;
                mapname = mapname.Substring(startIndex, mapname.Length - startIndex);
                return mapname.First().ToString().ToUpper() + mapname.Substring(1);
            }

            return mapname;
        }

    }
    public static class DictionaryExt
    {
        public static string GetVal(this Dictionary<string, string> value, string key)
        {
            return value.ContainsKey(key) ? value[key] : "Unknown";
        }
    }
}
