using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.TestWeb.ViewModels
{
    public class SendGroupMessageRequest
    {
        public string token { get; set; }
        public String toGroupId { get; set; }
        public String objectName { get; set; }
        public String content { get; set; }
    }
}
