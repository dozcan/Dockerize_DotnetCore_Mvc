using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3.Models
{
    public class RequestIdentity
    {
        public string address { get; set; }
        public data data { get; set; }
    }
    public class data
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string identity { get; set; }
    }

    public class RequestHashGet
    {
        public string address { get; set; }
        public string hashofBlockchainData{ get; set; }
    }

}
