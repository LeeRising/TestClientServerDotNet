using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server
{
    public class Program
    {
        static void Main()
        {
            tcpConnectonCreator();
            Console.ReadKey(false);
        }

        static async void tcpConnectonCreator()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(getLocalIp()), 9858);
            TcpClient clientSocket = default(TcpClient);
            Console.WriteLine("Server Started");
            serverSocket.Start();
            try
            {
                while (true)
                {
                    clientSocket = await serverSocket.AcceptTcpClientAsync();

                    Console.WriteLine("\n" + @"\*** Accept connection ***/");
                    byte[] bytesFrom = new byte[4096];
                    NetworkStream networkStream = clientSocket.GetStream();
                    await networkStream.ReadAsync(bytesFrom, 0, bytesFrom.Length);
                    var dataFromClient = Encoding.UTF8.GetString(bytesFrom);
                    Console.WriteLine(@"\*** Get data from connection ***/");
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.LastIndexOf("$", StringComparison.Ordinal));
                    try
                    {
                        var addUser = new Incapsulation.ConnectionUser
                        {
                            UserName = dataFromClient.Split('$')[1].Split('=')[1],
                            UserTcpClient = clientSocket
                        };
                        if (!Incapsulation.connectedUsers.Exists(i => i.UserName == addUser.UserName))
                        {
                            Incapsulation.connectedUsers.Add(addUser);
                            new ConnectHandle().startClient(clientSocket, Incapsulation.connectedUsers);
                            Console.WriteLine($"User {dataFromClient.Split('$')[0].Split('=')[1]} [{clientSocket.Client.RemoteEndPoint}] connected");

                            Console.WriteLine(Incapsulation.connectedUsers.Count);
                        }
                        else
                        {
                            var text = "User already login";
                            Console.WriteLine(text);
                            ConnectHandle.sendSystemMessage($"type=systemcommand$text={text}",clientSocket);
                            clientSocket.Client.Close(0);
                            clientSocket = null;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                clientSocket?.Close();
                serverSocket.Stop();
                Console.WriteLine("exit");
                Console.ReadLine();
            }
        }
        static string getLocalIp()
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
    }

    public static class Incapsulation
    {
        public static List<ConnectionUser> connectedUsers { get; set; } = new List<ConnectionUser>();

        public class ConnectionUser
        {
            public string UserName { get; set; }
            public TcpClient UserTcpClient { get; set; }
        }
    }
}
