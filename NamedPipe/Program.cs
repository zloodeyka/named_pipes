using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Threading;
using System.IO;

namespace NamedPipe
{
    class Program
    {
        static int pipesCount = 4;
        static void Main(string[] args)
        {
            Thread server = new Thread(ServerThread);
            server.Start();

            Thread[] clients = new Thread[4];
            for (int i=0; i<4; i++)
            {
                clients[i] = new Thread(ClientThread);
                clients[i].Start();
            }
            Console.ReadKey();
        }

        public static void ServerThread()
        {
            PipeServer server = new PipeServer();
        }

        public static void ClientThread()
        {
            PipeClient client = new PipeClient();
            client.Start();
        }
    }
}
