﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Cod4.Core.Master;
using Cod4.Core.Messages;
using Cod4.Core.Models;
using Cod4.Core.Parse;
using Cod4ServerBrowser.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Input;

namespace Cod4ServerBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private UdpClient _udp;
        private Cod4MasterQuery _master;
        private List<Server> _serverIpCache = new List<Server>();
        public static ObservableCollection<Cod4BrowseServer> Servers { get; set; } = new ObservableCollection<Cod4BrowseServer>();
        public int PlayerCount { get; private set; }
        private ICollectionView SearchHelper { get; set; } = CollectionViewSource.GetDefaultView(Servers);
        public event PropertyChangedEventHandler PropertyChanged;
        public MainWindow()
        {
            InitializeComponent();
            Init();
            DataContext = this;
        }

        private void Init()
        {
            UdpReceiver();
            InitMasterServerRequestHandlers();

            InitUIEventHandlers();

            _master.OnGetServersCompleted += OnGetServersCompleted;
            RefreshServers();
        }

        private void InitMasterServerRequestHandlers()
        {
            _master = new Cod4MasterQuery();
            _master.OnGetServersRequestError += OnError;
            _master.OnError += OnError;
            if (!_master.Init())
            {
                MessageBox.Show("Error While Initializing");
                Application.Current.Shutdown();
            }
        }

        private void InitUIEventHandlers()
        {
            ServerSearch.SearchBox.TextChanged += (s, e) => SearchHelper.Filter = item => ((Cod4BrowseServer)item).HostName.ToLower().Contains(ServerSearch.SearchBox.Text.ToLower());
            PlayerSearch.SearchBox.TextChanged += (s, e) => SearchHelper.Filter = item => ((Cod4BrowseServer)item).Players.Any(p => p.name.ToLower().Contains(PlayerSearch.SearchBox.Text.ToLower()));
            QuickRefresh.Click += (s, e) => GetStatus(_serverIpCache);
            FullRefresh.Click += (s, e) => RefreshServers();
            DataGrid.PreviewMouseDown += DataGrid_PreviewMouseDown;

            Style rowStyle = new Style(typeof(DataGridRow));
            rowStyle.Setters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler(Row_DoubleClick)));
            DataGrid.RowStyle = rowStyle;
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;

                if (element?.DataContext is Cod4BrowseServer)
                {
                    if (grid.SelectedItem == (Cod4BrowseServer)((FrameworkElement)e.OriginalSource).DataContext)
                    {
                        grid.SelectedIndex = -1;
                        e.Handled = true;
                    }
                }
            }
        }

        private void DataGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            DataGrid.SelectedIndex = -1;
        }


        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGrid.SelectedIndex < 0) return;
            string connect = (DataGrid.SelectedItem as Cod4BrowseServer)?.Connect;
            Task.Run(() =>
            {
                Process game = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "iw3mp");
                if (game != null)
                {
                    game.Kill();
                    if (!game.WaitForExit(5000)) return;
                }
                Process.Start($"cod4://{connect}");
            });
        }

        private void RefreshServers() => _master.GetServers();

        private void OnGetServersCompleted(List<Server> servers)
        {
            _serverIpCache = servers;
            GetStatus(servers);
        }

        private void GetStatus(List<Server> servers)
        {
            Dispatcher.Invoke(() => Servers.Clear());
            PlayerCount = 0;
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
            // ReSharper disable once RedundantAssignment
            byte[] data = new byte[805];
            Task.Run(() =>
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    while (true)
                    {
                        try
                        {
                            data = _udp.Receive(ref sender);
                            // Console.WriteLine($"Packet #{count++} Received with [{data.Length} bytes] from { sender.Address}");
                            LogServer(new Source() { IP = sender.Address.ToString(), Port = sender.Port, Status = Encoding.Default.GetString(data) });
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                }
                catch (Exception e) { OnError(e.Message); }
            });
        }

        // Need better solution
        public long GetPing(Source server)
        {
            try
            {
                PingReply reply = new Ping().Send(server.IP, 5000);
                if (reply != null) return reply.RoundtripTime;
            }
            catch
            {
                // ignored
            }

            return 1;
        }

        public void LogServer(Source source, bool getdvars = true, bool getplayers = true)
        {
            Task.Run(() =>
            {
                ServerInfo result;
                try
                {
                    if (Parser.GetServerInfo(source, getdvars, getplayers, out result))
                    {
                        long ping = -1;
                        Dispatcher.Invoke(() =>
                        {
                            Servers.Add(new Cod4BrowseServer
                            {
                                IsPasswordProtected = result.Dvars.GetVal("pswrd") == "1",
                                HostName = Util.RemoveColours(result.Dvars.GetVal("sv_hostname")),
                                Connect = $"{source.IP}:{source.Port}",
                                HostNameClean = Util.RemoveColours(result.Dvars.GetVal("sv_hostname")),
                                MapName = Util.FriendlyMapName(result.Dvars.GetVal("mapname")),
                                Players = result.Players,
                                OnlinePlayers = result.Players.Count,
                                MaxPlayers = Regex.Match(result.Dvars.GetVal("sv_maxclients"), "^\\d{1,2}$").Success ? Int32.Parse(result.Dvars.GetVal("sv_maxclients")) : 0,
                                GameType = Util.FriendlyGametype(result.Dvars.GetVal("g_gametype")),
                                Voice = result.Dvars.GetVal("sv_voice") == "1",
                                PunkBuster = result.Dvars.GetVal("sv_punkbuster") == "1",
                                Ping = ping
                            });
                            PlayerCount += result.Players.Count;

                        });
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
    }
    public static class DictionaryExt
    {
        public static string GetVal(this Dictionary<string, string> value, string key)
        {
            return value.ContainsKey(key) ? value[key] : "Unknown";
        }
    }
}