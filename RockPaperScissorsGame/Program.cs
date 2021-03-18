using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using MessageProtocol;
using System.Threading;

namespace RockPaperScissorsGame
{
    class Program
    {
        private static IPEndPoint IPEndPoint;
        private static string address = "127.0.0.1";
        private static int port = 1025;
        private static TcpListener server;
        private static string rulesFile = "rulesgame.txt";
        private static string rules = "";
        private static List<Player> players;
        private static object lck;
        private static Random rand;

        static void Main(string[] args)
        {
            IPEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            server = new TcpListener(IPEndPoint);
            players = new List<Player>();
            lck = new object();


            LoadRulesFile();
            server.Start(50);

            Console.WriteLine($"Сервер запустился: {IPEndPoint.Address} : {IPEndPoint.Port}");

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        TcpClient socket = server.AcceptTcpClient();
                        Task.Run(() =>
                        {
                            string name = ((DataMessage)Transfer.ReceiveTCP(socket)).Message;
                            Player player = new Player()
                            {
                                Name = name,
                                PlayerSocket = socket,
                                PlayerIPEnd = (IPEndPoint)socket.Client.LocalEndPoint,
                                countBalls = 0
                            };
                            players.Add(player);
                            Console.WriteLine($" К игре подключился игром с Именем: {player.Name} и IP: {player.PlayerIPEnd.Address}");
                            SendToPlayerMessage(player, rules);

                            PlayGame(player, socket);

                        });
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            });
            Console.ReadKey();
            Console.WriteLine("Работа сервера остановлена...");
            server.Stop();
        }

        private static void PlayGame(Player player, TcpClient socket)
        {
            Task.Run(() =>
            {
                int countServerBalss = 0;
                int raund = 1;
                while (raund < 6)
                {
                    rand = new Random();
                    string message = $"Игрок с Именем: {player.Name}, делайте свой ход (Камень-1, Ножницы-2, Бумага-3, Колодец-4): ";
                    SendToPlayerMessage(player, message);
                    string turn = ((DataMessage)Transfer.ReceiveTCP(socket)).Message;
                    int res = 0;
                    int playerTurn = Convert.ToInt32(turn);
                    int serverTurn = rand.Next(1, 4);
                    string messageTurnServer = "Ход копьютера: " + ((MoveOption)serverTurn).ToString();
                    SendToPlayerMessage(player, messageTurnServer);
                    switch (serverTurn)
                    {
                        case 1:
                            if (playerTurn == 1)
                            {
                                countServerBalss += 5;
                                player.countBalls += 5;
                                res = 0;
                            }
                            else if (playerTurn == 2)
                            {
                                countServerBalss += 10;
                                res = 1;
                            }
                            else if (playerTurn == 3)
                            {
                                player.countBalls += 10;
                                res = -1;
                            }
                            else if (playerTurn == 4)
                            {
                                player.countBalls += 10;
                                res = -1;
                            }
                            break;
                        case 2:
                            if (playerTurn == 2)
                            {
                                countServerBalss += 5;
                                player.countBalls += 5;
                                res = 0;
                            }
                            else if (playerTurn == 1)
                            {
                                player.countBalls += 10;
                                res = -1;
                            }
                            else if (playerTurn == 3)
                            {
                                countServerBalss += 10;
                                res = 1;
                            }
                            else if (playerTurn == 4)
                            {
                                player.countBalls += 10;
                                res = -1;
                            }
                            break;
                        case 3:
                            if (playerTurn == 3)
                            {
                                countServerBalss += 5;
                                player.countBalls += 5;
                                res = 0;
                            }
                            else if (playerTurn == 1)
                            {
                                countServerBalss += 10;
                                res = 1;
                            }
                            else if (playerTurn == 2)
                            {
                                player.countBalls += 10;
                                res = -1;
                            }
                            else if (playerTurn == 4)
                            {
                                countServerBalss += 10;
                                res = 1;
                            }
                            break;
                        case 4:
                            if (playerTurn == 4)
                            {
                                countServerBalss += 5;
                                player.countBalls += 5;
                                res = 0;
                            }
                            else if (playerTurn == 1)
                            {
                                countServerBalss += 10;
                                res = 1;
                            }
                            else if (playerTurn == 2)
                            {
                                countServerBalss += 10;
                                res = 1;
                            }
                            else if (playerTurn == 3)
                            {
                                player.countBalls += 10;
                                res = -1;
                            }
                            break;
                        default:
                            break;
                    }
                    if (res == 0)
                        SendToPlayerMessage(player, $"Раунд - {raund}: Ходы равные, ничья!");
                    if (res == 1)
                        SendToPlayerMessage(player, $"Раунд - {raund}: Победа Компьютера!");
                    if (res == -1)
                        SendToPlayerMessage(player, $"Раунд - {raund}: Победа Игрока - {player.Name}!");
                    Thread.Sleep(1000);
                    raund++;
                }
                if (countServerBalss > player.countBalls)
                    SendToPlayerMessage(player, $"Победу в Игре Одержал - Компьютер, Кол-во очков - {countServerBalss}!");
                else if (countServerBalss < player.countBalls)
                    SendToPlayerMessage(player, $"Победу в Игре Одержал - Игрок: {player.Name}, Кол-во очков - {player.countBalls}!");
                else
                    SendToPlayerMessage(player, "Победила Дружба - Ничья!");
            });
        }

        private static void LoadRulesFile()
        {
            using (StreamReader sr = new StreamReader(rulesFile, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    rules += sr.ReadLine();
                    rules += "\n";
                }
            }
        }

        private static void SendToPlayerMessage(Player player, string message)
        {
            lock (lck)
            {
                foreach (var item in players)
                {
                    if (item == player)
                        Transfer.SendTCP(item.PlayerSocket, new DataMessage() { Message = message });
                }
                Console.WriteLine($"Сообщение отправлено Игроку: {player.Name} и IP: {player.PlayerIPEnd.Address}");
            }
        }
    }

    public enum MoveOption { Камень = 1, Ножницы, Бумага, Колодец }

    public class Player
    {
        public TcpClient PlayerSocket { get; set; }
        public IPEndPoint PlayerIPEnd { get; set; }
        public string Name { get; set; }
        public int countBalls { get; set; }
    }
}
