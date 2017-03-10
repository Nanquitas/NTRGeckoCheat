using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NTRGeckoCheat.Annotations;
using System.Threading.Tasks;
using NTRGeckoCheat.Gecko;

namespace NTRGeckoCheat
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region INOTIFY
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region UIProperties

        private string _statusText;
        private Brush _statusColor;
        private string _gameTitle;
        private string _gameTitleId;

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (value == _statusText) return;
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public Brush StatusColor
        {
            get { return _statusColor; }
            set
            {
                if (Equals(value, _statusColor)) return;
                _statusColor = value;
                OnPropertyChanged();
            }
        }
        public string GameTitle
        {
            get { return _gameTitle; }
            set
            {
                if (value == _gameTitle) return;
                _gameTitle = value;
                OnPropertyChanged();
            }
        }

        public string GameTitleId
        {
            get { return _gameTitleId; }
            set
            {
                if (value == _gameTitleId) return;
                _gameTitleId = value;
                OnPropertyChanged();
            }
        }
        #endregion

        private readonly Logger _logger;
        private TCPGecko _gecko;

        public MainWindow()
        {
            InitializeComponent();

            // Create Gecko
            _gecko = new TCPGecko("0.0.0.0", 5000);

            // Create Logger
            _logger = new Logger(LogRichTextBox);

            // Binding datacontext
            DataContext = this;

            // Update status
            UpdateStatus("Disconnected", Brushes.DarkRed);

            // Launch our background thread for fetching logs
            Thread t = new Thread(FetchHandlerLogs) { IsBackground = true };
            t.Start();
        }
        private void FetchHandlerLogs()
        {
            TCPGecko gecko = TCPGecko.Instance;

            while (true)
            {
                try
                {
                    // Pause the thread 1s
                    Thread.Sleep(1000);

                    if (gecko.Connected)
                    {
                        string log = gecko.LogRequest();

                        // If log isn't empty, add it to the debug logger
                        if (!String.IsNullOrEmpty(log))
                            _logger.Add(log);
                    }
                }
                catch (Exception ex)
                {
                    // Refresh our gecko's instance
                    gecko = TCPGecko.Instance;
                    // Update status as disconnected
                    UpdateStatus("Disconnected", Brushes.DarkRed);
                }
            }
        }

        private void UpdateStatus(string text, Brush color)
        {
            StatusText = text;
            StatusColor = color;
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            _logger.Clear();
            string ip = IpTextBox.Text;

            // If already connected disconnect
            if (_gecko.Connected)
            {
                _gecko.Disconnect();
                UpdateStatus("Disconnected", Brushes.DarkRed);
            }

            // Set Gecko host
            _gecko.Host = ip;

            // Try to connect
            try
            {
                // Connect in a task to avoid freezing the UI
                bool c = await Task.Run(() =>
                {
                    // Start from PID 0x26 to 0x50
                    for (int p = 0x26; p < 0x50; p++)
                    {
                        UpdateStatus("Connecting on port: " + (5000 + p), Brushes.DarkGray);
                        _gecko.Port = (5000 + p);
                        try
                        {
                            if (_gecko.Connect())
                            {
                                return true;
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    return false;
                });
                if (!c)
                    throw new Exception();

                // Connected
                UpdateStatus("Connected", Brushes.DarkGreen);

                // Fetch game's title ID
                uint type = _gecko.TitleTypeRequest();
                uint id = _gecko.TitleIDRequest();

                GameTitleId = $"{type:X08}{id:X08}";

                // Fetch game's code name)
                string gamename = _gecko.GameNameRequest();

                GameTitle = gamename;


            }
            catch (Exception ex)
            {
                UpdateStatus("Disconnected", Brushes.DarkRed);
            }
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            string cheat = CheatTextBox.Text;

            _gecko.SendCheat(cheat);
        }

        private void EnableButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_gecko.Connected) return;
            if (string.IsNullOrEmpty(CheatIdTextBox.Text)) return;

            int id = Convert.ToInt32(CheatIdTextBox.Text);

            _gecko.EnableCheat(id);
        }

        private void ListButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_gecko.Connected) return;

            _gecko.ListCheats();
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_gecko.Connected) return;
            if (string.IsNullOrEmpty(CheatIdTextBox.Text)) return;

            int id = Convert.ToInt32(CheatIdTextBox.Text);

            _gecko.RemoveCheat(id);
        }

        private void DisableButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_gecko.Connected) return;
            if (string.IsNullOrEmpty(CheatIdTextBox.Text)) return;

            int id = Convert.ToInt32(CheatIdTextBox.Text);

            _gecko.DisableCheat(id);
        }
    }
}
