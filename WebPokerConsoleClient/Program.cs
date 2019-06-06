using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebPokerConsoleClient
{
    public enum TPokerAction { Check = 0, Rais = 1, Fold = 2};
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Введите Login");
                string login = Console.ReadLine();
                Console.WriteLine("Введите Name");
                string name = Console.ReadLine();

                //конечная локальная точка
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[1];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11006);

                //Сoздаем сокет Tcp/Ip
                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //Сокет подключение
                sender.Connect(ipEndPoint);

                User user = new User(login, name);
                //общаемся с сервером
                JsonHandle.SendObject(sender, user);

                //слушаем
                int port = (int)JsonConvert.DeserializeObject<int>(JsonHandle.ReciveString(sender));

                Console.WriteLine("port: {0}", port);

                //close
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

                //open
                Socket UserListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ipEndPoint = new IPEndPoint(ipAddr, port);

                //Сокет подключение
                UserListener.Bind(ipEndPoint);
                UserListener.Listen(1);
                Socket handler = UserListener.Accept();
                //Console.WriteLine("Ожидаем соединения через {0}", ipEndPoint);

                //раздача
                JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler));
                List<Player> playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));
                int gamerNumber =0;
                for( int i=0; i< playerList.Count(); i++)
                {
                    if (playerList[i].login == login)
                    {   
                        gamerNumber = i;
                        break;
                    }
                }
                Console.WriteLine("Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money);
                bool stop;
                while(JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler))) //refresh
                {
                    playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));
                    Console.WriteLine("Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "  _Table_  " + playerList[gamerNumber].table);
                }
                stop = JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler));
                while (!stop){
                    Console.WriteLine("Введите команду 0,1,2");
                    TPokerAction PokerAction = (TPokerAction)(Console.Read() - (byte)'0');
                    Console.ReadLine();
                    JsonHandle.SendObject(handler, PokerAction);
                    if (PokerAction == TPokerAction.Rais)
                    {
                        Console.WriteLine("Введите ставку :");
                        string buf = Console.ReadLine();
                        int bet = int.Parse(buf);
                        JsonHandle.SendObject(handler, bet);
                    }
                    while(JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler))) //refresh
                    {
                        playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));
                        Console.WriteLine("Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "  _Table_  " + playerList[gamerNumber].table);
                    }
                    stop = JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler));
                }
                List<string> winers = JsonConvert.DeserializeObject<List<string>>(JsonHandle.ReciveString(handler));
                for (int i = 0; i < winers.Count(); i++)
                {
                    Console.WriteLine(winers);
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }

        }
    }
}
