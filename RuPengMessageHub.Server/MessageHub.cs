using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RuPengMessageHub.Server.Models;
using RuPengMessageHub.Server.Settings;
using RuPengMessageHub.Server.ViewModels;
using StackExchange.Redis;

namespace RuPengMessageHub.Server
{
    //必须设置，而且参数必须为Bearer
    //只有标注了[Authorize("Bearer")],框架才会解析Authentication报文头的值到Claims中
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessageHub: Hub
    {
        private IOptions<RedisSetting> redisSetting;
        public MessageHub(IOptions<RedisSetting> redisSetting)
        {
            this.redisSetting = redisSetting;
        }

        [HubMethodName("GetGroupMessages")]
        public async Task GetGroupMessagesAsync(String groupId)
        {
            //todo:要验证用户是否属于这个group
            string groupName = this.Context.User.GetGroupName(groupId);
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration))
            {
                IDatabase db = redis.GetDatabase(redisSetting.Value.DbId);
                var messages = (await db.ListRangeAsync($"{groupName}_Messages", 0, 100)).Reverse()
                    .Select(rv=>JsonConvert.DeserializeObject<GroupMessageResp>(rv));
                await this.Clients.Group(groupName).SendAsync("OnGroupMessages", messages);//批量消息
            }
        }

        /// <summary>
        /// 获取组成员，获取在线状态
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task GetGroupMembersAsync(string groupId)
        {
            //todo；
            //todo：检查是否是这一组的
        }


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            //把Token和ConnectionId的关系保存起来，供GroupController.JoinGroupAsync等需要获得当前连接的ConnecionId使用。
            //保存到服务器端，这样也避免把ConnectionId泄漏到客户端
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration))
            {
                IDatabase db = redis.GetDatabase(redisSetting.Value.DbId);
                var httpContext = this.Context.Features.Get<IHttpContextFeature>().HttpContext;
                string access_token = httpContext.Request.Query["access_token"];
                await db.StringSetAsync(access_token + "_ConnectionId", this.Context.ConnectionId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            //todo:保存在线离线状态
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration))
            {
                IDatabase db = redis.GetDatabase(redisSetting.Value.DbId);
                var httpContext = this.Context.Features.Get<IHttpContextFeature>().HttpContext;
                string access_token = httpContext.Request.Query["access_token"];
                await db.KeyDeleteAsync(access_token + "_ConnectionId");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
