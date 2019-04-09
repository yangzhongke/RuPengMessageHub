using System;

namespace RuPengMessageHub.Models
{
    public class AppInfo
    {
        public Guid Id { get; set; }
        public string AppName { set; get; }
        public string AppKey { set; get; }
        public string AppSecret { set; get; }
    }
}
