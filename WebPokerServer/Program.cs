using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebPokerServer
{//11000
    public class Player
    {
        public string login;
        public string name;
        public byte card1;
        public byte card2;
        public byte blind;
        public int money;
        public bool fold = false;
        public int bet;
    }

    class Program
    {
        static bool CheckTimeOk(DateTime Start, byte minuts)
        { return (DateTime.UtcNow - Start) < TimeSpan.FromMinutes(minuts); }
        static void ListnerTimer(object listner)
        {
            Thread.Sleep(10*60*1000);
            ((Socket)listner).Dispose();
            ((Socket)listner).Close();
            listner = null;
        }


        const int maxColClient = 5;
        static void Main(string[] args)
        {
            List<Socket> UsersSocketList = new List<Socket>();
            List<User> UsersList = new List<User>();
            List<Player> Players = new List<Player>();

            //Ввод числа игроков
            Console.WriteLine("Введите число игроков (от 2 до 5)");
            int ColClient = Console.Read() - (byte)'0';
            if (ColClient < 2)
                ColClient = 2;
            else if (ColClient > maxColClient)
            { ColClient = 5; }

            while (StartConnect(ColClient,ref UsersSocketList, ref UsersList))
            {

                Players = CreatePlayerList(UsersList);
                StartGame(UsersSocketList,ref Players);
            }
            Console.ReadLine();

        }
        static bool StartConnect(int ColClient,ref List<Socket> UsersSockets, ref List<User> Users)
        {
            bool result = true;
            try
            {
                //конечная локальная точка
                int Port = 11000; 
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[1];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Port);

                //Сoздаем сокет Ncp/Ip
                Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sListener.Bind(ipEndPoint);
                sListener.Listen(ColClient);

                //Сокет для локальной точки и прослушивание входящих сокетов
                UsersSockets = new List<Socket>();
                Users = new List<User>();

                //tiemer
                Thread thread1= new Thread(ListnerTimer);
                thread1.IsBackground = true;
                thread1.Start(sListener); //запускаем 
                while ((UsersSockets.Count() < ColClient))
                {

                    //!!!
                    Console.WriteLine("Ожидаем соединения через {0}", ipEndPoint);

                    //Ожидаем соединения
                    Socket handler = sListener.Accept();


                    //общаемся с клиентом
                    Users.Add( JsonConvert.DeserializeObject<User>(JsonHandle.ReciveString(handler)));

                    ///!!!
                    //Console.WriteLine("Login {0}", user.login);

                    //Отвечаем
                    Port++;
                    JsonHandle.SendObject(handler, Port);
                    ipEndPoint = new IPEndPoint(ipAddr, Port);

                    //close
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                    //Сoздаем сокет Tcp/Ip
                    Socket UserSender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    //Сокет подключение
                    try
                    {
                        UserSender.Connect(ipEndPoint);
                        UsersSockets.Add(UserSender);
                    }
                    catch
                    {
                        Port--;
                    }

                }
                //close
                thread1.Abort();
                sListener.Close();
                sListener = null;

                //проверка на число пользователей
                if (UsersSockets.Count() < 2)
                { 
                    result = false; 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        }

        static void SedToPlayers(List<Socket> UsersSockets, ref List<Player> PlayerList) 
        {
        
        
        }

        static List<Player> CreatePlayerList(List<User> UsersList)
        {
            string jsonObject = String.Empty;

            // десериализация из файла
            StreamReader Sr = new StreamReader("Player.txt");
            jsonObject = Sr.ReadToEnd();
            List<Player> allPlayer = JsonConvert.DeserializeObject<List<Player>>(jsonObject);
            Sr.Close();
            //allPlayer = new List<Player>();
            //чтение профилей
            List<Player> PlayersList = new List<Player>();

            bool NotInPlayer;
            foreach (User user in UsersList) 
            {
                NotInPlayer = true;
                foreach (Player player in allPlayer)
                {
                    if (user.login == player.login)
                    {
                        PlayersList.Add(player);
                        NotInPlayer = false;
                    }
                }
                if (NotInPlayer)  
                {
                    Player NewPlayer = new Player();
                    NewPlayer.login = user.login;
                    NewPlayer.money = 1000;
                    NewPlayer.name = user.name;
                    PlayersList.Add(NewPlayer);
                    allPlayer.Add(NewPlayer);
                }
            }
            string jsonObjectString = JsonConvert.SerializeObject(allPlayer, Formatting.Indented);

            // файл для записи сериализованного объекта
            StreamWriter Sw = new StreamWriter("Player.txt");
            Sw.Write(jsonObjectString);
            Sw.Flush();
            Sw.Close();

            return PlayersList;

        } 


        static void StartGame(List<Socket> UsersSockets, ref List<Player> PlayerList)
        {
            foreach (Player player in PlayerList) 
            {
            
            }

        }

    }
}
