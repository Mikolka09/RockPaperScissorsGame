using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MessageProtocol;
using System.Threading;

namespace PlayerGame
{
    class Program
    {
        private static IPEndPoint IPEndPoint;
        private static string address = "127.0.0.1";
        private static int port = 1025;
        private static TcpClient player;
        private static string message = "";
        private static string name = "";
        private static object lck;

        static void Main(string[] args)
        {
            IPEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            player = new TcpClient();
            player.Connect(IPEndPoint);
            lck = new object();

            Console.WriteLine("\tДОБРО ПОЖАЛОВАТЬ НА ИГРУ \"КАМЕНЬ, НОЖНИЦЫ, БУМАГА...\"");
            Console.WriteLine("\t-------------------------------------------------------\n");
            Console.Write("Для начала введите свое Имя: ");
            name = Console.ReadLine();
            Transfer.SendTCP(player, new DataMessage() { Message = name });
            Console.WriteLine();
            string message = ((DataMessage)Transfer.ReceiveTCP(player)).Message;
            Console.WriteLine(message);
            Console.WriteLine("\tИГРА НАЧАЛАСЬ!");

            Task task = Task.Run(() => Receive());

            SendMessage();

            Console.ReadKey();
        }

        private static void Receive()
        {
            while (true)
            {
                string message = ((DataMessage)Transfer.ReceiveTCP(player)).Message;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("\n" + message);
            }

        }

        private static void SendMessage()
        {
            int rand = 1;
            while (rand < 6)
            {

                bool res = true;
                while (res)
                {
                    message = Console.ReadLine();
                    if (Convert.ToInt32(message) > 4 || Convert.ToInt32(message) < 1)
                    {
                        Console.WriteLine("Ошибка, неправельный ввод числа!");
                        res = true;
                    }
                    else
                        res = false;
                }
                Console.WriteLine($"Игрок по имени {name} походил - {(MoveOption)Convert.ToInt32(message)}");
                Transfer.SendTCP(player, new DataMessage() { Message = message });

                rand++;
            }
        }
    }
    public enum MoveOption { Камень = 1, Ножницы, Бумага, Колодец }
}


