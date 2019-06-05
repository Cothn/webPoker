using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;

namespace WebPokerServer
{
    public static class JsonHandle
    {
        public static string ReciveString(Socket handler)
        {
            byte[] bytes = new byte[1024];
            int butesRec = handler.Receive(bytes);
            string jsonObject = Encoding.UTF8.GetString(bytes, 0, butesRec);

            return jsonObject;
        }
        public static void SendObject(Socket handler, object SendObject)
        {

            // сериализация
            string jsonObject = JsonConvert.SerializeObject(SendObject, Formatting.Indented);

            byte[] replyBuf = Encoding.UTF8.GetBytes(jsonObject);
            int bytesSent = handler.Send(replyBuf);

        }

    }
}

