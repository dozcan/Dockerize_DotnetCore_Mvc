using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace WebApplication3.Models
{
    public class FormInput
    {
        public string name { get; set; }
        public string surname { get; set; }
        public IFormFile file { get; set; }
    }
}
