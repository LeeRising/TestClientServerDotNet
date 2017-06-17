using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Server
{
    public class ConnectHandle
    {
        TcpClient _clientSocket;
        List<Thread> threadList = new List<Thread>();

        List<Incapsulation.ConnectionUser> _clientsList;

        public void startClient(TcpClient inClientSocket, List<Incapsulation.ConnectionUser> cList)
        {
            this._clientSocket = inClientSocket;
            this._clientsList = cList;
            threadList.Add(new Thread(doDataExchange));
            threadList[threadList.Count-1].Start();
        }

        private async void doDataExchange()
        {
            while (true)
            {
                try
                {
                    NetworkStream networkStream = _clientSocket.GetStream();
                    var bytesFrom = new byte[4096];
                    await networkStream.ReadAsync(bytesFrom, 0, bytesFrom.Length);
                    var dataFromClient = Encoding.UTF8.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.LastIndexOf("$", StringComparison.Ordinal));
                    dataLineParser(dataFromClient);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
            }
        }

        public static async void sendSystemMessage(string msg,TcpClient client)
        {
            NetworkStream broadcastStream = client.GetStream();
            var broadcastBytes = Encoding.UTF8.GetBytes(msg);
            broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
            await broadcastStream.FlushAsync();
        }

        void dataLineParser(string data)
        {
            string command = String.Empty, 
                userLogin = String.Empty, 
                uniqueKey = String.Empty, 
                text = String.Empty;
            var dataFromClientArray = data?.Split('$');
            if (dataFromClientArray == null) return;
            try
            {
                command = dataFromClientArray[0]?.Split('=')[1];
                userLogin = dataFromClientArray[1]?.Split('=')[1];
                uniqueKey = dataFromClientArray[2]?.Split('=')[1];
                text = dataFromClientArray[3]?.Split('=')[1];
            }
            catch (Exception)
            {
                // ignored
            }
            if (command == "disconnect")
            {
                if (userLogin != null)
                {
                    var removeIndex = Incapsulation.connectedUsers.FindIndex(i =>i.UserName == userLogin);
                    Incapsulation.connectedUsers.RemoveAt(removeIndex);
                    threadList[removeIndex].Abort();
                }
            }
            if (command == "sendmessage")
            {

            }
        }
    }
}
