using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3.Models
{
    public class AppSettings
    {
        public string node_api { get; set; }
        public string node_port { get; set; }
        public string mongo_cs {get;set;}

        public string redis_ip { get; set; }
    }
}
