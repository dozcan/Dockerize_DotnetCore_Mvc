using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3.Models
{
    public class Hashes
    {
        public string hashofBlockchainData { get; set;}
        public string transactionHash { get; set; }
        public string contractAddress { get; set; }

    }

    public class OriginalData
    {
        public string name { get; set; }
        public string surname { get; set; }
    }
}
