using Microsoft.AspNetCore.Mvc;
using RuPengMessageHub.NetSDK;
using RuPengMessageHub.TestWeb.ViewModels;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RuPengMessageHub.TestWeb.Controllers
{
    public class HomeController : Controller
    {
        public static readonly string msgHubServer = "http://localhost:11022/";//"https://msghub.rupeng.com/";//"http://localhost:11022/"

        private IHttpClientFactory httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var httpClient = httpClientFactory.CreateClient();
            MsgHubClient client = new MsgHubClient(msgHubServer, httpClientFactory);
            var token = await client.GetTokenAsync("1", "yzk", "rupenggongkaike", DateTime.Now.ToFileTime(), "wpstt@999_6xx!aa");
            ViewBag.token = token;
            return View();
        }

        public async Task<bool> SendGroupMessage([FromBody]SendGroupMessageRequest req)
        {
            string token = req.token;
            var httpClient = httpClientFactory.CreateClient();
            MsgHubClient client = new MsgHubClient(msgHubServer, httpClientFactory);
            await client.SendGroupMessageAsync(token, req.toGroupId, req.objectName, req.content);
            return true;
        }

        public async Task<bool> JoinGroup([FromBody]JoinGroupRequest req)
        {
            string token = req.token;
            var httpClient = httpClientFactory.CreateClient();
            //todo：要根据业务系统的要求，判断这个用户是否能加入这个Group
            MsgHubClient client = new MsgHubClient(msgHubServer, httpClientFactory);
            await client.JoinGroupAsync(token, req.groupId);
            return true;
        }
    }
}
