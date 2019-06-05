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

    public enum TPokerAction { Check = 0, Rais = 1, Fold = 2};
    class Program
    {

        public static int AllMoney = 0;
        public static int tekBet = 5;
        public static int BigBlaind = 5;
        public static Cards cards;
        public static Random rand = new Random();
        public static int ColClient = 5;
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
            ColClient = Console.Read() - (byte)'0';
            if (ColClient < 2)
                ColClient = 2;
            else if (ColClient > maxColClient)
            { ColClient = 5; }

            while (StartConnect(ColClient, ref UsersSocketList, ref UsersList))
            {

                Players = CreatePlayerList(UsersList);
                StartGame(UsersSocketList, ref Players);
                SendToPlayers(UsersSocketList, Players);
                Game(UsersSocketList, ref Players);

                //string jsonObjectString = JsonConvert.SerializeObject(Players, Formatting.Indented);

                //// файл для записи сериализованного объекта
                //StreamWriter Sw = new StreamWriter("Player.txt");
                //Sw.Write(jsonObjectString);
                //Sw.Flush();
                //Sw.Close();
            }
            Console.ReadLine();

        }
        static bool StartConnect(int ColClient, ref List<Socket> UsersSockets, ref List<User> Users)
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

        static void SendToPlayers(List<Socket> UsersSockets, List<Player> PlayerList) 
        {
            string jsonObjectString = JsonConvert.SerializeObject(PlayerList, Formatting.Indented);

            // файл для записи сериализованного объекта
            StreamWriter Sw = new StreamWriter("PlayerTmp.json");
            Sw.Write(jsonObjectString);
            Sw.Flush();
            Sw.Close();
            for (int i = 0; i < UsersSockets.Count(); i++)
            {
                // десериализация из файла
                StreamReader Sr = new StreamReader("PlayerTmp.json");
                string jsonObject = Sr.ReadToEnd();
                List<Player> SendList = JsonConvert.DeserializeObject<List<Player>>(jsonObject);
                Sr.Close();
                for (int j = 0; j < PlayerList.Count(); j++)
                {
                    if (i != j)
                    {
                        SendList[j].card1 = "00";
                        SendList[j].card2 = "00";
                    }

                }
                JsonHandle.SendObject(UsersSockets[i], SendList);
            }
        }


                    playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));
                    Console.WriteLine("Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber] + "  _Table_  " + playerList[gamerNumber].table);
                    stop = JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler));

        static void Game(List<Socket> UsersSockets, ref List<Player> PlayerList)
        {
            TPokerAction pokerAction;
            bool stop = false;
            do{
                for (int i = 0; i < UsersSockets.Count(); i++)
                {
                    pokerAction = JsonConvert.DeserializeObject<TPokerAction>(JsonHandle.ReciveString(UsersSockets[i]));
                    switch {}
                

                }
            }while(!stop);
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
            int num;
            BigBlaind = ColClient;
            AllMoney = 0;
            tekBet = 5;
            cards = new Cards();
            foreach (Player player in PlayerList) 
            {
                player.bet += tekBet;
                player.money -= tekBet;
                AllMoney += tekBet;
                num = rand.Next(0, cards.Deck.Count());
                player.card1 = cards.Deck[num];
                cards.Deck.Remove(player.card1);
                num = rand.Next(0, cards.Deck.Count());
                player.card2 = cards.Deck[num];
                cards.Deck.Remove(player.card2);
            
            }
            num = BigBlaind % ColClient +1;
            PlayerList[num].bet += tekBet;
            PlayerList[num].money -= tekBet;
            AllMoney += tekBet;
            tekBet += tekBet;
        }

    }
}
