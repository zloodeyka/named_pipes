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
    class PipeClient
    {
        public string ClientId { get; set; }
        const string PIPE_NAME = "MyPipe";
        const int numberOfThreads = 4;
        const int N = (int)1e5;
        Sieve sieve = null;

        public void Start()
        {
            GetSieveInformation();
            while (ProcessRange() != -1){}
        }

        public int ProcessRange()
        {
            string range;
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation))
            {
                pipeStream.Connect();
                StreamWriter writer = new StreamWriter(pipeStream);
                writer.WriteLine(QueryType.GetRange);
                writer.Flush();

                StreamReader reader = new StreamReader(pipeStream);
                string response = reader.ReadLine();
                PrintClientMessage(response);
                if (response == "NoTasks")
                {
                    return -1;
                }
                range = reader.ReadLine();
                if (string.IsNullOrEmpty(range))
                {
                    return -1;
                }
            }
            string[] tmp = range.Split(' ');
            int start = int.Parse(tmp[0]);
            int end = int.Parse(tmp[1]);
            var primes = sieve.GetPrimesInRange(start, end);
            if (primes.Count > 0)
            {
                using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation))
                {
                    pipeStream.Connect();
                    StreamWriter writer = new StreamWriter(pipeStream);
                    writer.WriteLine(QueryType.SendPrimes);
                    writer.Flush();
                    writer.WriteLine(JsonConvert.SerializeObject(primes));
                    writer.Flush();
                }
            }
            return 0;
        }

        public void GetSieveInformation()
        {
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation))
            {

                pipeStream.Connect();
                PrintClientMessage("[Client] Pipe connection established");
                StreamWriter writer = new StreamWriter(pipeStream);
                writer.WriteLine(QueryType.GetSieve);
                writer.Flush();

                StreamReader reader = new StreamReader(pipeStream);
                var tmp = reader.ReadLine();
                PrintClientMessage(tmp);
                sieve = JsonConvert.DeserializeObject<Sieve>(tmp);
                pipeStream.Close();
                Thread.Sleep(4000);
            }
        }
        public void PrintClientMessage(string msg)
        {
            Console.WriteLine($"Client: {msg}");
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine(str);
            //Console.ResetColor();
        }
    }
}
