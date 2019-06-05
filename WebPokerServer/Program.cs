using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace WebPokerServer
{//11000

    class Program
    {
        const byte MaxClient = 5; 
        private static int Port = 11000;
        public  static List<Socket> UsersSocket = new List<Socket>();
        static void Main(string[] args)
        {
            
            //конечная локальная точка
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Port);

            //Сoздаем сокет Ncp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sListener.Bind(ipEndPoint);
            sListener.Listen(5);
            //Сокет для локальной точки и прослушивание входящих сокетов
            try
            {


                
                //начинаем слушать
                while (UsersSocket.Count() < MaxClient)
                {

                    //!!!
                    Console.WriteLine("Ожидаем соединения через {0}", ipEndPoint);

                    //Ожидаем соединения
                    Socket handler = sListener.Accept();
                    //общаемся с клиентом
                    User user = JsonConvert.DeserializeObject<User>(JsonHandle.ReciveString(handler));

                    Console.WriteLine("Login {0}", user.login);
                    //Отвечаем
                    Port++;
                    JsonHandle.SendObject(handler, Port);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                    ipEndPoint = new IPEndPoint(ipAddr, Port);

                    //Сoздаем сокет Tcp/Ip
                    Socket UserSender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    //Сокет подключение
                    UserSender.Connect(ipEndPoint);
                    UsersSocket.Add(UserSender);

                }
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
