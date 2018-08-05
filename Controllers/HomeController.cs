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

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AccountCreate()
        {
            var client = new HttpClient();
            HttpResponseMessage Task;

            Task = await client.GetAsync("http://34.212.29.21:6000/AccountCreate");
            var content = await Task.Content.ReadAsStringAsync();

            var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountCreateModelResponse>(content);
            var accountobj = new AccountCreateModel();
            accountobj.Key = ob.response.Key;
            accountobj.Account = ob.response.Account;

            return View(accountobj);
        }

        public async Task<IActionResult> DeployContract()
        {
            var client = new HttpClient();
            HttpResponseMessage Task;

            Task = await client.GetAsync("http://34.212.29.21:6000/DeployContract");
            var content = await Task.Content.ReadAsStringAsync();

            var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseDeployContractModel>(content);
            var accountobj = new DeployContractModel();

            accountobj.Account = ob.response.Account;
            accountobj.Balance = ob.response.Balance;
            accountobj.Block = ob.response.Block;
            accountobj.Contract = ob.response.Contract;
            accountobj.Gas = ob.response.Gas;
            HttpContext.Session.SetString("contractAddress", accountobj.Contract);

            return View(accountobj);
        }
      
        public IActionResult FormInput()
        {
            return View();
        }

        public IActionResult GetHash()
        {
            var arr = new List<Hashes>();
            int i = 0;
            using (StreamReader sr = new StreamReader("hash.txt"))
            {
                String line = sr.ReadToEnd();
                var srt = line.Split(";");
                foreach(var s in srt) {
                    var subSrt = s.Split("-");
                    if (subSrt.Length < 2)
                        break;
                    var _Hashes = new Hashes();
                    _Hashes.contractAddress = subSrt[2];
                    _Hashes.hashofBlockchainData = subSrt[0];
                    _Hashes.transactionHash = subSrt[1];
                    arr.Add(_Hashes);
                    i++;
                    
                }
            }

            return View(arr);
        }

        [HttpPost]
        public async Task<IActionResult> Identity(FormInput file)
        {
            using (var fileStream = new FileStream(Path.Combine("", file.file.FileName), FileMode.Create))
            {
                await file.file.CopyToAsync(fileStream);
            }
          
            var identityToBlockchain = CreatePayload(file.file.FileName, file.name, file.surname);

            var client = new HttpClient();

            var Task = await client.PostAsync("http://34.212.29.21:6000/Identity", new StringContent(identityToBlockchain, Encoding.UTF8, "application/json"));

            var content = await Task.Content.ReadAsStringAsync();

            var ob = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseIdentityModel>(content);
            var accountobj = new IdentityModel();

            accountobj.Account = ob.response.Account;
            accountobj.Data_hash = ob.response.Data_hash;
            accountobj.Transaction_hash = ob.response.Transaction_hash;
            var ContractAddress = HttpContext.Session.GetString("contractAddress");

            string line = accountobj.Data_hash + "-" + accountobj.Transaction_hash + "-" + ContractAddress + ";";

            System.IO.File.AppendAllText("hash.txt", line);
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
            var Task = await client.PostAsync("http://34.212.29.21:6000/HashGet", new StringContent(identityToBlockchain, Encoding.UTF8, "application/json"));

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
