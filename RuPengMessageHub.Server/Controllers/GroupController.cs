using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RuPengMessageHub.Server.Models;
using RuPengMessageHub.Server.Settings;
using RuPengMessageHub.Server.ViewModels;
using StackExchange.Redis;

namespace RuPengMessageHub.Server.Controllers
{
    [Route("group")]
    [ApiController]
    //只有标注了[Authorize("Bearer")],框架才会解析Authentication报文头的值到Claims中
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GroupController : ControllerBase
    {
        private readonly IHubContext<MessageHub> Context;
        private readonly IOptions<RedisSetting> redisSetting;

        public GroupController(IHubContext<MessageHub> hubContext, IOptions<RedisSetting> redisSetting)
        {
            this.Context = hubContext;
            this.redisSetting = redisSetting;
        }

        [HttpPost]
        [Route("SendGroupMessage")]
        public async Task SendGroupMessageAsync(SendGroupMessageRequest req)
        {
            //发送消息一定不能写到Hub上，因为那样前端用户就能直接调用聊天服务器的Hub接口发送消息，就无法在应用层面做
            //内容的过滤了，发送消息一定要通过应用的服务器端调用GroupController.SendGroupMessageAsync来做，这样在应用服务器端可以做干预
            //Hub上通常只写获取消息等无潜在威胁或者不需要应用服务器处理的方法
            
            string userId = this.HttpContext.User.GetUserId();//不能用this.Context.UserIdentifier，因为他是包含AppKey前缀的
            string groupName = this.HttpContext.User.GetGroupName(req.toGroupId);
            string userDisplayName = this.HttpContext.User.GetUserDisplayName();
            bool containsUserId;
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration))
            {
                IDatabase db = redis.GetDatabase(redisSetting.Value.DbId);
                containsUserId = await db.SetContainsAsync($"{groupName}_Members", userId);
            }
            //检查用户是否属于这一组
            if (!containsUserId)
            {
                throw new HubException("UserId="+userId+" not in group:"+req.toGroupId);
            }

            GroupMessageResp msg = new GroupMessageResp();
            msg.Content = req.content;
            msg.DateTime = DateTime.Now;
            msg.FromUserDisplayName = userDisplayName;
            msg.FromUserId = userId;
            msg.ObjectName = req.objectName;
            msg.TargetGroupId = req.toGroupId;

            await this.Context.Clients.Group(groupName).SendAsync("OnGroupMessage", msg);
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration))
            {
                IDatabase db = redis.GetDatabase(redisSetting.Value.DbId);
                await db.ListLeftPushAsync($"{groupName}_Messages", msg.ToJsonString());
            }
        }

        [HttpPost]
        [Route("JoinGroup")]
        public async Task JoinGroupAsync(JoinGroupRequest req)
        {
            string userId = this.HttpContext.User.GetUserId();
            string appKey = this.HttpContext.User.GetAppKey();
            string groupName = this.HttpContext.User.GetGroupName(req.groupId);
            string authentication = this.HttpContext.Request.Headers["Authorization"];
            string token = authentication.Substring("Bearer ".Length);

            //redis连接字符串格式见https://www.cnblogs.com/ArvinZhao/p/6007043.html
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration))
            {
                IDatabase db = redis.GetDatabase(redisSetting.Value.DbId);//默认是访问db0数据库，可以通过方法参数指定数字访问不同的数据库
                string connectionId = await db.StringGetAsync(token + "_ConnectionId");

                //用groupId做组名
                await this.Context.Groups.AddToGroupAsync(connectionId, groupName);
                
                //如果组不存在，则创建组
                //如果组存在，则用新的组名覆盖旧的
                //加入成员
                await db.SetAddAsync($"{groupName}_Members", userId);
            }
        }
    }
}