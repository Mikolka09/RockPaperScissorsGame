using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MessageProtocol
{
    public interface Data { }

    [Serializable]
    public class DataMessage: Data
    {
        public string Message { get; set; }
    }

    public class Transfer
    {
        private static BinaryFormatter formatter = new BinaryFormatter();

        public static void SendTCP(TcpClient client, Data data)
        {
            formatter.Serialize(client.GetStream(), data);
        }

        public static Data ReceiveTCP (TcpClient client)
        {
            return (Data)formatter.Deserialize(client.GetStream());
        }
    }
}
