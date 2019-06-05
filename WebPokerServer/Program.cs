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
        private static byte clien = 0;
        static void Main(string[] args)
        {

            //конечная локальная точка
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            //Сoздаем сокет Ncp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //Сокет для локальной точки и прослушивание входящих сокетов
            try
            {

                sListener.Bind(ipEndPoint);
                sListener.Listen(5);
                
                //начинаем слушать
                while (true)
                {

                    //!!!
                    Console.WriteLine("Ожидаем соединения через {0}", ipEndPoint);

                    //Ожидаем соединения
                    Socket handler = sListener.Accept();
                    //общаемся с клиентом
                    User user = JsonConvert.DeserializeObject<User>(JsonHandle.ReciveString(handler));

                    Console.WriteLine("Login {0}", user.login);
                    //Отвечаем
                    JsonHandle.SendObject(handler, 11001);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

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
