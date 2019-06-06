﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

namespace WebPokerServer
{//11000
    public enum TPokerAction { Check = 0, Rais = 1, Fold = 2};
    class Program
    {

        public static int AllMoney = 0;
        public static int MaxBet = 5;
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
                SendToPlayersSecret(UsersSocketList, Players);
                Game(UsersSocketList, ref Players);
                GameResult(UsersSocketList, ref Players);

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
                int Port = 11006; 
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

        static void SendToPlayersSecret(List<Socket> UsersSockets, List<Player> PlayerList) 
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
                JsonHandle.SendObject(UsersSockets[i], true); //refresh
                Thread.Sleep(100);
                JsonHandle.SendObject(UsersSockets[i], SendList);
                Thread.Sleep(100);
            }
        }


        static void Game(List<Socket> UsersSockets, ref List<Player> PlayerList)
        {
            TPokerAction pokerAction;
            bool stop = false;
            bool raise = true;
            do{

                raise = false;
                for (int i = 0; i < UsersSockets.Count(); i++)
                {
                    if ((PlayerList[i].fold != true) && PlayerList[i].money != 0)
                    {
                        JsonHandle.SendObject(UsersSockets[i], false); //stop
                        Thread.Sleep(100);
                        JsonHandle.SendObject(UsersSockets[i], stop);
                        pokerAction = JsonConvert.DeserializeObject<TPokerAction>(JsonHandle.ReciveString(UsersSockets[i]));
                        switch (pokerAction)
                        {
                            case TPokerAction.Check:
                                PlayerList[i].money = PlayerList[i].money + PlayerList[i].MaxBet - MaxBet;
                                if (PlayerList[i].money > 0)
                                {
                                    AllMoney = AllMoney - PlayerList[i].MaxBet + MaxBet;
                                    PlayerList[i].MaxBet = MaxBet;
                                }
                                else
                                {
                                    AllMoney = AllMoney - PlayerList[i].MaxBet + MaxBet + PlayerList[i].money;
                                    PlayerList[i].MaxBet = MaxBet + PlayerList[i].money;
                                    PlayerList[i].money = 0;
                                }
                                break;
                            case TPokerAction.Rais:
                                int DopBet = JsonConvert.DeserializeObject<int>(JsonHandle.ReciveString(UsersSockets[i]));

                                if (PlayerList[i].MaxBet + DopBet < MaxBet)
                                { DopBet = MaxBet - PlayerList[i].MaxBet; }
                                PlayerList[i].money = PlayerList[i].money - DopBet;
                                if (PlayerList[i].money > 0)
                                {
                                    AllMoney += DopBet;
                                    PlayerList[i].MaxBet += DopBet;

                                }
                                else
                                {
                                    AllMoney = AllMoney + DopBet + PlayerList[i].money;
                                    PlayerList[i].MaxBet = PlayerList[i].MaxBet + DopBet + PlayerList[i].money;
                                    PlayerList[i].money = 0;
                                }
                                if (MaxBet < PlayerList[i].MaxBet)
                                {
                                    MaxBet = PlayerList[i].MaxBet;
                                    raise = true;
                                }
                                break;
                            default:
                                PlayerList[i].fold = true;
                                PlayerList[i].MaxBet = 0;
                                break;
                        }
                        PlayerList[i].MaxBet = MaxBet;
                        
                    }

                    SendToPlayersSecret(UsersSockets, PlayerList);
                }

                //new circle
                raise = raise && (PlayerList[0].MaxBet != MaxBet);
                stop = ((PlayerList[0].table.Length == 10) && (raise == false));
                if ((raise == false) && !stop)
                {
                    int num = rand.Next(0, cards.Deck.Count());
                    string TableCard = cards.Deck[num];
                    cards.Deck.Remove(TableCard);
                    if (PlayerList[0].table == "")
                    {
                        num = rand.Next(0, cards.Deck.Count());
                        TableCard += cards.Deck[num];
                        cards.Deck.Remove(cards.Deck[num]);
                        num = rand.Next(0, cards.Deck.Count());
                        TableCard += cards.Deck[num];
                        cards.Deck.Remove(cards.Deck[num]);
                    }
                    foreach(Player player in PlayerList)
                    {
                        player.table =  player.table + TableCard;
                    }
                }
                SendToPlayersSecret(UsersSockets, PlayerList);
                // stop game
            }while(!stop);
            for (int i = 0; i < UsersSockets.Count(); i++)
            {
                JsonHandle.SendObject(UsersSockets[i], false); //stop
                Thread.Sleep(100);
                JsonHandle.SendObject(UsersSockets[i], stop);
            }

        }



    public enum Ranks
    {
        A = 14, K = 13, Q = 12, J = 11,
        Ten = 10, Nine = 9, Eight = 8, Seven = 7, Six = 6,
        Five = 5, Four = 4, Tree = 3, Two = 2
    }
    public enum Suits { Hearts, Diamonds, Clubs, Spades }
    public enum Combinations
    {
        Uknown = 0, HighCard = 1, Pair = 2, TwoPairs = 3, Tree = 4,
        Streight = 5, Flash = 6, FullHouse = 7, Kare = 8, StreightFlash = 9
    }

        static int GameResult(List<Socket> UsersSockets, ref List<Player> PlayerList)
        {
            string winers = String.Empty;
            int Rang = 0;
            List<int> HeightList = new List<int>();
            for (int l = 0; l < PlayerList.Count; l = l+ 1)
            {
                string bufStr;
                bufStr = PlayerList[l].table + PlayerList[l].card1 + PlayerList[l].card2;
                for (int t = 0; t < 12; t = t+2)
                {

                    for (int o = t+2; o<12; o = o+2)
                    {
                        bufStr = bufStr.Substring(0, t) + bufStr.Substring(t + 2, t + o) + bufStr.Substring(t + o +2, bufStr.Length - t -o -2);



                        Char[] charArray = bufStr.ToCharArray();
                        for (int j = 0; j < charArray.Length; j++)
                        {
                            if (charArray[j].Equals('T'))
                                charArray[j] = (char)59;
                            if (charArray[j].Equals('J'))
                                charArray[j] = (char)60;
                            if (charArray[j].Equals('Q'))
                                charArray[j] = (char)61;
                            if (charArray[j].Equals('K'))
                                charArray[j] = (char)62;
                            if (charArray[j].Equals('A'))
                                charArray[j] = (char)63;
                        }
                        for (int j = 0; j < charArray.Length; j = j + 2)
                        {
                            for (int k = 0; k < charArray.Length - 3; k = k + 2)
                            {
                                if (charArray[k] > charArray[k + 2])
                                {
                                    Char buf = charArray[k + 2];
                                    charArray[k + 2] = charArray[k];
                                    charArray[k] = buf;
                                    buf = charArray[k + 1];
                                    charArray[k + 1] = charArray[k + 3];
                                    charArray[k + 3] = buf;
                                }


                            }

                        }


                        bool flash = false;
                        bool streight = false;
                        bool kare = false;
                        bool fullhouse = false;
                        bool tree = false;
                        bool twopairs = false;
                        bool highcard = true;
                        bool pair = false;

                        int[] len = new int[2] { 1, 0 }; //Для отслеживания двух пар и фулхауса
                        int num = 0;

                        for (int i = 2; i < charArray.Length; i = i + 2)
                        {
                            if (charArray[i] == charArray[i - 2]) len[num]++;
                            else
                                if ((len[num] > 1) && (num != 1))
                                {
                                    num++;
                                    len[num]++;
                                }
                                else if (len[num] <= 1) len[num] = 1;
                        }


                        //Работаем с полной комбинацией            

                        //Проверка на флеш
                        flash = true;
                        for (int i = 3; i < charArray.Length; i = i + 2)
                            if (charArray[i] != (charArray[1]))
                            {
                                flash = false;
                                break;
                            }

                        //Проверка на стрит
                        streight = true;
                        for (int i = 2; i < charArray.Length; i++)
                            if ((charArray[i - 2] - charArray[i] != 1) && (charArray[i - 2] - charArray[i] != 9))
                            {
                                streight = false;
                                break;
                            }

                        if ((len[0] == 2) || (len[1] == 2)) pair = true;
                        if ((len[0] == 3) || (len[1] == 3)) tree = true;
                        if ((len[0] == 2) && (len[1] == 2)) twopairs = true;
                        if (((len[0] == 2) && (len[1] == 3)) || ((len[0] == 3) && (len[1] == 2))) fullhouse = true;
                        if (len[0] == 4) kare = true;

                        if (streight && flash) { Rang =0;}
                        if (streight) { Rang  =0;}
                        if (flash) { Rang =0;}
                        if (kare) { Rang =0; }
                        if (fullhouse) { Rang =0; }
                        if (twopairs) { Rang =0; }
                        if (tree) { Rang =0;}
                        if (pair) { Rang =0; }
                        if (highcard) { Rang =0;}

                        
                    }
                    
                }
                return Rang;
            }
            return Rang;

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
            MaxBet = 5;
            cards = new Cards();
            foreach (Player player in PlayerList) 
            {
                player.MaxBet += MaxBet;
                player.money -= MaxBet;
                player.MaxBet = MaxBet;
                AllMoney += MaxBet;
                num = rand.Next(0, cards.Deck.Count());
                player.card1 = cards.Deck[num];
                cards.Deck.Remove(player.card1);
                num = rand.Next(0, cards.Deck.Count());
                player.card2 = cards.Deck[num];
                cards.Deck.Remove(player.card2);
            
            }
            num = BigBlaind % ColClient +1;
            PlayerList[num].MaxBet += MaxBet;
            PlayerList[num].money -= MaxBet;
            AllMoney += MaxBet;
            MaxBet += MaxBet;
        }

    }
}
