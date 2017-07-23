using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public partial class MainWindow : Window
    {
        private TcpClient _clientSocket;
        private NetworkStream _serverStream = default(NetworkStream);
        private string _login = string.Empty;
        private Task _task;
        public MainWindow()
        {
            InitializeComponent();
            _clientSocket = new TcpClient();
        }

        private string getLocalIp()
        {
#pragma warning disable 618
            var iphostentry = Dns.GetHostByName(Dns.GetHostName());
#pragma warning restore 618
            foreach (var ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.ToString().Contains("192.168"))
                    return ipaddress.ToString();
            }
            return "";
        }

        private string _dataParser(string data)
        {
            var type = data.Split('$')[0].Split('=')[1];
            return type == "systemcommand" ? data.Split('$')[1].Split('=')[1] : "";
        }

        private async void GetMessagefromServer()
        {
            while (true)
            {
                try
                {
                    var bytesFrom = new byte[4096];
                    await _serverStream.ReadAsync(bytesFrom, 0, bytesFrom.Length);
                    var dataFromServer = Encoding.UTF8.GetString(bytesFrom);
                    dataFromServer = dataFromServer.Substring(0, dataFromServer.LastIndexOf("$", StringComparison.Ordinal));
                    await DataFromServerTb.Dispatcher.InvokeAsync(() =>
                    {
                        DataFromServerTb.AppendText(_dataParser(dataFromServer) + Environment.NewLine);
                    });
                }
                catch (Exception)
                {
                    SetToNull();
                    break;
                }
            }
        }

        private void SetToNull()
        {
            _clientSocket?.Close();
            _serverStream.Close();
            _clientSocket = new TcpClient();
        }
        private void connect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _login = USerNameTb.Text;
                _clientSocket?.Connect(getLocalIp(), 9858);
                if (_clientSocket != null) _serverStream = _clientSocket.GetStream();
                var outStream = Encoding.UTF8.GetBytes($"userLogin={_login}$uniqueKey={_login}$");
                _serverStream.Write(outStream, 0, outStream.Length);
                _serverStream.Flush();
                DataFromServerTb.Text = string.Empty;
                _task = new Task(GetMessagefromServer);
                _task.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void disconnect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clientSocket != null) _serverStream = _clientSocket.GetStream();
                var outStream = Encoding.UTF8.GetBytes("command=disconnect" +
                                                       $"$userLogin={_login}" +
                                                       "$uniqueKey=key" +
                                                       "$text=text" + "$");
                _serverStream.Write(outStream, 0, outStream.Length);
                _serverStream.Flush();
                SetToNull();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            try
            {
                var outStream = Encoding.UTF8.GetBytes("command=disconnect" +
                                                          $"$userLogin={_login}" +
                                                          "$uniqueKey=key$");
                _serverStream.Write(outStream, 0, outStream.Length);
                _serverStream.Flush();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
