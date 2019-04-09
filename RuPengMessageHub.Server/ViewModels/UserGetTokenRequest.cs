using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.ViewModels
{
    public class UserGetTokenRequest
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string AppKey { get; set; }

        [Required]
        public long Timestamp { get; set; }

        [Required]
        public string Signature { get; set; }
    }
}
