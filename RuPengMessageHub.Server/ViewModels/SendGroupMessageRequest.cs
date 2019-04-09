using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.ViewModels
{
    public class SendGroupMessageRequest
    {
        public String toGroupId { get; set; }
        public String objectName { get; set; }
        public String content { get; set; }
    }
}
