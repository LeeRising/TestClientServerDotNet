using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Client
{
    public partial class MainWindow : Window
    {
        TcpClient _clientSocket;
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;
        string login = String.Empty;
        Thread ctThread;
        public MainWindow()
        {
            InitializeComponent();
            _clientSocket = new TcpClient();
        }
        string getLocalIp()
        {
#pragma warning disable 618
            IPHostEntry iphostentry = Dns.GetHostByName(Dns.GetHostName());
#pragma warning restore 618
            foreach (var ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.ToString().Contains("192.168"))
                    return ipaddress.ToString();
            }
            return "";
        }

        string dataParser(string data)
        {
            var type = data.Split('$')[0].Split('=')[1];
            if (type == "systemcommand")
            {
                return data.Split('$')[1].Split('=')[1];
            }
            return "";
        }
        async void getMessagefromServer()
        {
            while (true)
            {
                try
                {
                    var bytesFrom = new byte[4096];
                    await serverStream.ReadAsync(bytesFrom, 0, bytesFrom.Length);
                    var dataFromServer = Encoding.UTF8.GetString(bytesFrom);
                    dataFromServer = dataFromServer.Substring(0, dataFromServer.LastIndexOf("$", StringComparison.Ordinal));
                    await DataFromServerTb.Dispatcher.InvokeAsync(() =>
                    {
                        DataFromServerTb.AppendText(dataParser(dataFromServer) + Environment.NewLine);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    break;
                }
            }
        }
        private void connect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                login = USerNameTb.Text;
                _clientSocket?.Connect(getLocalIp(), 9858);
                if (_clientSocket != null) serverStream = _clientSocket.GetStream();
                var outStream = Encoding.UTF8.GetBytes($"userLogin={login}$uniqueKey={login}$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                DataFromServerTb.Text = String.Empty;
                ctThread = new Thread(getMessagefromServer);
                ctThread.Start();
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
                if (_clientSocket != null) serverStream = _clientSocket.GetStream();
                var outStream = Encoding.UTF8.GetBytes("command=disconnect" +
                                                       $"$userLogin={login}" +
                                                       "$uniqueKey=key" +
                                                       "$text=text" + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                _clientSocket?.Close();
                serverStream.Close();
                _clientSocket = new TcpClient();
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
                                                          $"$userLogin={login}" +
                                                          "$uniqueKey=key");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                _clientSocket.Close();
                serverStream.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
