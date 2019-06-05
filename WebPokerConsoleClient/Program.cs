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
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

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
                Console.WriteLine("Ожидаем соединения через {0}", ipEndPoint);


                List<Player> playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));


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
