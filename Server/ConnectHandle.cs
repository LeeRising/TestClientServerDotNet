using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ConnectHandle
    {
        private TcpClient _clientSocket;
        public List<Task> TaskList { get; } = new List<Task>();
        public List<Incapsulation.ConnectionUser> ClientsList { get; private set; }

        public void startClient(TcpClient inClientSocket, List<Incapsulation.ConnectionUser> cList)
        {
            this._clientSocket = inClientSocket;
            this.ClientsList = cList;
            TaskList.Add(new Task(doDataExchange));
            TaskList[TaskList.Count - 1].Start();
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
                catch (Exception)
                {
                    break;
                }
            }
        }

        public static async void sendSystemMessage(string msg, TcpClient client)
        {
            NetworkStream broadcastStream = client.GetStream();
            var broadcastBytes = Encoding.UTF8.GetBytes(msg);
            broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
            await broadcastStream.FlushAsync();
        }

        void dataLineParser(string data)
        {
            string command = String.Empty,
                userLogin = String.Empty;
            var dataFromClientArray = data?.Split('$');
            if (dataFromClientArray == null) return;
            command = dataFromClientArray[0]?.Split('=')[1];
            userLogin = dataFromClientArray[1]?.Split('=')[1];
            if (command != "disconnect") return;
            var removeIndex = Incapsulation.connectedUsers.FindIndex(i => i.UserName == userLogin);
            Incapsulation.connectedUsers.RemoveAt(removeIndex);
            TaskList[removeIndex].Dispose();
            Console.WriteLine($"\nUser {userLogin} disconnected!\n");
        }
    }
}
