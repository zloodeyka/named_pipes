using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace NamedPipe
{
    class PipeServer
    {

        const string PIPE_NAME = "MyPipe";
        const int numberOfThreads = 4;
        const int N = (int)1e5;
        string sieveJson;

        public PipeServer()
        {
            int i;
            Thread[] servers = new Thread[numberOfThreads];
            Console.WriteLine("\n*** Named pipe server started ***\n");
            Console.WriteLine("Waiting for client connect...\n");
            int blockSize = (int)Math.Sqrt(N);
            Sieve sieve = new Sieve(blockSize);
            sieve.blockSize = blockSize;

            sieveJson = JsonConvert.SerializeObject(sieve);

            for (i = 0; i < numberOfThreads; i++)
            {
                servers[i] = new Thread(ServerThread);
                servers[i].Start();
            }

            Thread.Sleep(250);

            while (i > 0)
            {
                for (int j = 0; j < numberOfThreads; j++)
                {
                    if (servers[j] != null)
                    {
                        if (servers[j].Join(250))
                        {
                            PrintServerMessage($"Thread[{servers[j].ManagedThreadId}] finished.");
                            servers[j] = null;
                            i--;    // decrement the thread watch count
                        }
                    }
                }
            }
            Console.WriteLine("\nServer threads exhausted, exiting.");
        }

        public void ServerThread()
        {
            while(true)
            {
                using (NamedPipeServerStream pipeStream = new NamedPipeServerStream(PIPE_NAME, PipeDirection.InOut, numberOfThreads))
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    PrintServerMessage("Server created" + pipeStream.GetHashCode());
                    pipeStream.WaitForConnection();

                    PrintServerMessage($"Server connection established on thread[{threadId}]");
                    StreamReader reader = new StreamReader(pipeStream);
                    StreamWriter writer = new StreamWriter(pipeStream);

                    var tmp = reader.ReadLine();
                    PrintServerMessage(tmp);
                    if (tmp == "GetSieve")
                    {
                        writer.WriteLine(sieveJson);
                        writer.Flush();
                    }
                    else if (tmp == "GetRange")
                    {
                        writer.WriteLine(ResponseType.ProcessRange);
                        writer.Flush();
                        writer.WriteLine("1000 2000");
                        writer.Flush();
                    }
                    else
                    {
                        string primesJson = reader.ReadLine();
                        PrintServerMessage(primesJson);
                    }
                }
                PrintServerMessage("Server connection lost");
            }
        }

        public void PrintServerMessage(string msg)
        {
            Console.WriteLine($"Server: {msg}");
        }
    }

    enum QueryType
    {
        GetSieve = 0,
        GetRange = 1,
        SendPrimes
    }

    enum ResponseType
    {
        ProcessRange = 0,
        NoTasks = 1
    }
}
