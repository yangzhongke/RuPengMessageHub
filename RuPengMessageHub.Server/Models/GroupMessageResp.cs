using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.Models
{
    public class GroupMessageResp
    {
        public string Id { get; set; }
        public string TargetGroupId { get; set; }
        public string FromUserId { get; set; }
        public string FromUserDisplayName { get; set; }
        public string ObjectName { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
    }
}
