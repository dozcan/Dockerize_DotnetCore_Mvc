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

    public class ContractAddress
    {
        public string address { get; set; }
    }

    public class ContractAddressMongo
    {
        public Object Id { get; set; }
        public string address { get; set; }
    }


    public class HashesMongo
    {
        public Object Id { get; set; }
        public string hashofBlockchainData { get; set; }
        public string transactionHash { get; set; }
        public string contractAddress { get; set; }

    }

    public class OriginalData
    {
        public string name { get; set; }
        public string surname { get; set; }
    }
}
