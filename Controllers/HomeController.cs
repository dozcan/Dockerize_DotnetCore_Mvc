using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using System.Text;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using WebApplication3.Models;
using System.IO;
using System.Drawing;
using System.IO.Compression;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using ServiceStack.Redis;

using MongoDB.Driver.Core;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings _appSettings;
        public string url;
        public HomeController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            url = "http://" + _appSettings.node_api + ":" + _appSettings.node_port + "/";
        }
        public IActionResult Index()
        {
            try
            {
                var manager = new RedisManagerPool("localhost:6379");
                var data = HttpContext.Session.GetString("contractAddress");
                if (data == null)
                {
                    using (var client = manager.GetClient())
                    {
                        string contract;
                        contract = client.Get<string>("ContractAddress");
                        if(contract!=null)
                            HttpContext.Session.SetString("contractAddress", contract);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return View();
        }

        public async Task<IActionResult> AccountCreate()
        {
            var accountobj = new AccountCreateModel();
            try
            {
                var client = new HttpClient();
                HttpResponseMessage Task;

                Task = await client.GetAsync(url + "AccountCreate");
                var content = await Task.Content.ReadAsStringAsync();
                var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountCreateModelResponse>(content);
                accountobj.Key = ob.response.Key;
                accountobj.Account = ob.response.Account;
            }
            catch(Exception ex)
            {

            }
            return View(accountobj);
        }
 
        public async Task<IActionResult> DeployContract()
        {
            var accountobj = new DeployContractModel();
            try
            {
                var client = new HttpClient();
                HttpResponseMessage Task;

                Task = await client.GetAsync(url + "DeployContract");
                var content = await Task.Content.ReadAsStringAsync();

                var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseDeployContractModel>(content);           

                accountobj.Account = ob.response.Account;
                accountobj.Balance = ob.response.Balance;
                accountobj.Block = ob.response.Block;
                accountobj.Contract = ob.response.Contract;
                accountobj.Gas = ob.response.Gas;

                var add = new ContractAddress();
                add.address = ob.response.Contract;


                var manager = new RedisManagerPool("localhost:6379");
                using (var clientOps = manager.GetClient())
                {
                    clientOps.Set("contractAddress", ob.response.Contract);
                    HttpContext.Session.SetString("contractAddress", ob.response.Contract);
                }

                HttpContext.Session.SetString("contractAddress", ob.response.Contract);

            }
            catch(Exception ex)
            {

            }

            return View(accountobj);
        }
      
        public IActionResult FormInput()
        {
            return View();
        }

        public async Task<IActionResult> GetHash()
        {
            var arr = new List<Hashes>();
            int len=0;
            try
            {

                var manager = new RedisManagerPool("localhost:6379");

                using (var client = manager.GetClient())
                {
                    if (client.Get<string>("ContractAddress") != null)
                    {
                        var contract = client.Get<string>("ContractAddress").Split(";");
                        var data_hash = client.Get<string>("HashofBlockchainData").Split(";");
                        var block_hash = client.Get<string>("TransactionHash").Split(";");

                        len = contract.Length;

                        for (int i = 0; i < len; i++)
                        {
                            var _Hashes = new Hashes();
                            _Hashes.contractAddress = contract[i];
                            _Hashes.hashofBlockchainData = data_hash[i];
                            _Hashes.transactionHash = block_hash[i];
                            arr.Add(_Hashes);
                        }
                    }

                }

                if (len == 0)
                    ViewBag.count = "NOP";
                else
                    ViewBag.count = "OP";
            }
            catch(Exception ex)
            {


            }
            return View(arr);
        }

        [HttpPost]
        public async Task<IActionResult> Identity(FormInput file)
        {
            var accountobj = new IdentityModel();

            try
            {
                using (var fileStream = new FileStream(Path.Combine("", file.file.FileName), FileMode.Create))
                {
                    await file.file.CopyToAsync(fileStream);
                }

                var identityToBlockchain = CreatePayload(file.file.FileName, file.name, file.surname);

                var client = new HttpClient();

                var Task = await client.PostAsync(url + "identity", new StringContent(identityToBlockchain, Encoding.UTF8, "application/json"));

                var content = await Task.Content.ReadAsStringAsync();

                var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseIdentityModel>(content);
          
                accountobj.Account = ob.response.Account;
                accountobj.Data_hash = ob.response.Data_hash;
                accountobj.Transaction_hash = ob.response.Transaction_hash;

                var ContractAddress = HttpContext.Session.GetString("contractAddress");

                var _Hashes = new Hashes();
                _Hashes.contractAddress = ContractAddress;
                _Hashes.hashofBlockchainData = accountobj.Data_hash;
                _Hashes.transactionHash = accountobj.Transaction_hash;


                var manager = new RedisManagerPool("localhost:6379");
                using (var clientOps = manager.GetClient())
                {
                    var contract = clientOps.Get<string>("ContractAddress");
                    string c;
                    if (contract == null)
                        c =  _Hashes.contractAddress;
                    else
                        c = contract + ";" + _Hashes.contractAddress;

                    clientOps.SetValue("ContractAddress", c);

                    var data_hash = clientOps.Get<string>("HashofBlockchainData");
                    string d;
                    if(data_hash==null)
                        d = _Hashes.hashofBlockchainData;
                    else
                        d = data_hash + ";" + _Hashes.hashofBlockchainData;

                    clientOps.SetValue("HashofBlockchainData", d);

                    var block_hash = clientOps.Get<string>("TransactionHash");
                    string t;
                    if(block_hash==null)
                        t = _Hashes.transactionHash;
                    else
                        t = block_hash + ";" + _Hashes.transactionHash;

                    clientOps.SetValue("TransactionHash", t);
                }

            }
            catch(Exception ex)
            {

            }

            return View(accountobj);
        }

        public static string ConvertStringToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        public static string ConvertHexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }

        private  string CreatePayload(string path ,string name, string surname)
        {
            try
            {

                byte[] array = System.IO.File.ReadAllBytes(path);
                string arraystring = "";
                int i = 0;
                
                String base64 = Convert.ToBase64String(array).ToString();

                var hex = ConvertStringToHex(base64);
                //var str = ConvertHexToString(hex);


                var ob = new RequestIdentity();

                var data = new data();
                data.name = name;
                data.surname = surname;
                data.identity = hex;
                ob.data = data;
                ob.address = HttpContext.Session.GetString("contractAddress");   

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(ob);
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception("payload error");
            }

        }

        public async Task<IActionResult> GetBlockChainData(string contractAddress,string hashofBlockchainData)
        {
            var RequestHashGet = new RequestHashGet();
            RequestHashGet.address = contractAddress;
            RequestHashGet.hashofBlockchainData = hashofBlockchainData;

            string identityToBlockchain = Newtonsoft.Json.JsonConvert.SerializeObject(RequestHashGet);
            var client = new HttpClient();
            var Task = await client.PostAsync(url + "HashGet", new StringContent(identityToBlockchain, Encoding.UTF8, "application/json"));

            var content = await Task.Content.ReadAsStringAsync();
            var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseHashGetModel>(content);

            var _FormOutput = new FormOutput();
            _FormOutput.name = ob.response.data.name;
            _FormOutput.surname = ob.response.data.surname;
            var s = ob.response.data.identity;
            string a = s.Substring(2);
            _FormOutput.identity =  ConvertHexToString(a);



            string img = string.Format("data:image/jpeg;base64,{0}", _FormOutput.identity);
            ViewBag.img = img;
            return View(_FormOutput);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
