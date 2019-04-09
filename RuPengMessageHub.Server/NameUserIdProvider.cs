using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RuPengMessageHub.Server
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            ClaimsPrincipal principal = connection.User;
            string appKey = principal.GetAppKey();
            string userId = principal.GetUserId();
            return $"{appKey}_{userId}";
        }
    }
}