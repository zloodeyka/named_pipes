using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipe
{
    class Sieve
    {
        public int blockSize { get; set; }
        public List<int> primes { get; set; }

        public Sieve(int sqrtn)
        {
            int[] lp = new int[sqrtn + 1];
            primes = new List<int>();

            for (int i = 2; i <= sqrtn; i++)
            {
                if (lp[i] == 0)
                {
                    lp[i] = i;
                    primes.Add(i);
                }
                for (int j = 0; j < primes.Count && i * primes[j] < sqrtn + 1; j++)
                {
                    lp[i * primes[j]] = primes[j];
                }
            }
        }

        public List<int> GetPrimesInRange(int start, int end)
        {
            List<int> blockPrimes = new List<int>();
            byte[] temporary_sieve = new byte[blockSize];

            for (int i = 0; i < primes.Count; i++)
            {
                int startInd = (start + primes[i] - 1) / primes[i];
                int j = Math.Max(startInd, 2) * primes[i] - start;
                while (j < blockSize)
                {
                    temporary_sieve[j] = 1;
                    j += primes[i];
                }
            }
            if (start == 0)
            {
                temporary_sieve[0] = temporary_sieve[1] = 1;
            }
            for (int i = 0; i < blockSize && start + i <= end; i++)
            {
                if (temporary_sieve[i] == 0)
                {
                    blockPrimes.Add(start + i);
                }
            }
            return blockPrimes;
        }
    }
}
