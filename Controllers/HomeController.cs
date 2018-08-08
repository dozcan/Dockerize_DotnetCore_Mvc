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
                var data = HttpContext.Session.GetString("contractAddress");
                if (data == null)
                {
                    var mongo = new MongoClient(_appSettings.mongo_cs);
                    var db = mongo.GetDatabase("default");
                    var ContractAddress = db.GetCollection<ContractAddressMongo>("ContractAddress");
                    var list = ContractAddress.AsQueryable();

                    foreach (var _l in list)
                    {
                        var ob = _l.address;
                        HttpContext.Session.SetString("contractAddress", ob);
                        break;
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

                var mongo = new MongoClient(_appSettings.mongo_cs);
                var db = mongo.GetDatabase("default");
                var ContractAddress = db.GetCollection<ContractAddress>("ContractAddress");
                ContractAddress.InsertOneAsync(add);
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
         
            try
            {
                int i = 0;
                var mongo = new MongoClient(_appSettings.mongo_cs);
                var db = mongo.GetDatabase("default");

                var _hashTableMongo = db.GetCollection<HashesMongo>("Hashes");
                var list = _hashTableMongo.AsQueryable();

                foreach (var _l in list)
                {
                    i++;
                    var _Hashes = new Hashes();
                    _Hashes.contractAddress = _l.contractAddress;
                    _Hashes.hashofBlockchainData = _l.hashofBlockchainData;
                    _Hashes.transactionHash = _l.transactionHash;
                    arr.Add(_Hashes);
                }
                if (i == 0)
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

                var mongo = new MongoClient(_appSettings.mongo_cs);
                var db = mongo.GetDatabase("default");
                var _hashTableMongo = db.GetCollection<Hashes>("Hashes");
                _hashTableMongo.InsertOneAsync(_Hashes);
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
