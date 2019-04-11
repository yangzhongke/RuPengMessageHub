using Microsoft.AspNetCore.Mvc;
using RuPengMessageHub.NetSDK;
using RuPengMessageHub.TestWeb.ViewModels;
using System;
using System.Threading.Tasks;

namespace RuPengMessageHub.TestWeb.Controllers
{
    public class HomeController : Controller
    {
        public static readonly string msgHubServer = "http://localhost:11022/";
        public async Task<IActionResult> Index()
        {
            MsgHubClient client = new MsgHubClient(msgHubServer);
            var token = await client.GetTokenAsync("1", "yzk", "rupenggongkaike", DateTime.Now.ToFileTime(), "fasdfs@888_6xx!aa");
            ViewBag.token = token;
            return View();
        }

        public async Task<bool> SendGroupMessage([FromBody]SendGroupMessageRequest req)
        {
            string token = req.token;

            MsgHubClient client = new MsgHubClient(msgHubServer);
            await client.SendGroupMessageAsync(token, req.toGroupId, req.objectName, req.content);
            return true;
        }

        public async Task<bool> JoinGroup([FromBody]JoinGroupRequest req)
        {
            string token = req.token;
            //todo：要根据业务系统的要求，判断这个用户是否能加入这个Group
            MsgHubClient client = new MsgHubClient(msgHubServer);
            await client.JoinGroupAsync(token, req.groupId);
            return true;
        }
    }
}
