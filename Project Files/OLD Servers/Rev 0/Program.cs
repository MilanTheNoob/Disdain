using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace GameServer
{
    class Program
    {
        public delegate void PacketHandler(Packet _packet);
        static TcpListener tcpListener;
        public static AccountInfo AccountData = new AccountInfo();

        public static string LobbyIP = "127.0.0.1";
        public static int LobbyPort = 26951;

        static void Main(string[] args)
        {
            var timer = new Timer(e => Save(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            Load();

            tcpListener = new TcpListener(IPAddress.Any, 26950);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Console.Title = "Game Server";
            Console.WriteLine("Server started on port 26590.");

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            new Client().Connect(_client);
        }

        static void MainThread()
        {
            DateTime _nextLoop = DateTime.Now;

            while (true)
            {
                while (_nextLoop < DateTime.Now)
                {
                    _nextLoop = _nextLoop.AddMilliseconds(1000f);

                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }

        static void Save()
        {
            if (AccountData.Accounts.Count == 0) { return; }
            if (File.Exists("data.file")) { File.Delete("data.file"); }

            Console.WriteLine("Saving, please do not turn off the server.");
            File.WriteAllText("data.file", JsonConvert.SerializeObject(AccountData));
        }

        static void Load()
        {
            if (!File.Exists("data.file")) { return; }
            Console.WriteLine("Loading, please do not turn off the server.");

            AccountData = JsonConvert.DeserializeObject<AccountInfo>(File.ReadAllText("data.file"));
        }
    }

    public class AccountInfo
    {
        public Dictionary<string, Account> Accounts = new Dictionary<string, Account>();
    }

    public class Account
    {
        public string Save;
        public bool HasGenerated;

        public string password;
        public int skin;
        public string name;
    }
}
