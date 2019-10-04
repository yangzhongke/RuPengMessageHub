using System;

namespace RuPengMessageHub.Models
{
    /// <summary>
    /// For Each Appliaction
    /// </summary>
    public class AppInfo
    {
        public Guid Id { get; set; }
        public string AppName { set; get; }
        public string AppKey { set; get; }
        public string AppSecret { set; get; }
    }
}
