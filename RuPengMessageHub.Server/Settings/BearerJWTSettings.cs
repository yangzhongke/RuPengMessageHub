using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.Settings
{
    public class BearerJWTSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
