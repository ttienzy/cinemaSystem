using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    public class VnPaySettings
    {
        public string Vnp_TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string BaseUrl { get; set; }
        public string Vnp_Command { get; set; }
        public string Vnp_CurrCode { get; set; }
        public string Vnp_Version { get; set; }
        public string Vnp_Locale { get; set; }
    }
}
