using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3.Models
{
    public class AccountCreateModel
    {
        public string Account { get; set; }
        public string Key { get; set; }
    }

    public class AccountCreateModelResponse
    {
        public bool success { get; set; }
        public AccountCreateModel response { get; set; }

    }
    public class ResponseDeployContractModel
    {
        public bool success { get; set; }
        public DeployContractModel response { get; set; }

    }
    public class DeployContractModel
    {
        public string Account { get; set; }
        public string Contract { get; set; }
        public string Balance { get; set; }
        public string Gas { get; set; }
        public string Block { get; set; }
    }


    public class ResponseIdentityModel
    {
        public bool success { get; set; }
        public IdentityModel response { get; set; }

    }
    public class IdentityModel
    {
        public string Account { get; set; }
        public string Data_hash { get; set; }
        public string Transaction_hash { get; set; }

    }

    public class ResponseHashGetModel
    {
        public bool success { get; set; }
        public HashGetModel response { get; set; }

    }

    public class HashGetModel
    {
        public data data { get; set; }
        public string hash_data { get; set; }
    }

    public class FormOutput
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string identity { get; set; }
    }

}
