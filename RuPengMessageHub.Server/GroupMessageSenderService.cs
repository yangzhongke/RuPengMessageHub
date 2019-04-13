using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RuPengMessageHub.Server.Helpers;
using RuPengMessageHub.Server.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server
{
    public class GroupMessageSenderService : BackgroundService
    {
        private readonly IHubContext<MessageHub> hubContext;
        private readonly RedisHelper redis;
        private readonly ILogger logger;

        public GroupMessageSenderService(IHubContext<MessageHub> hubContext, RedisHelper redis, ILoggerFactory logFactory)
        {
            this.hubContext = hubContext;
            this.redis = redis;
            this.logger = logFactory.CreateLogger(typeof(GroupMessageSenderService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = redis.GetDatabase();
            while (!stoppingToken.IsCancellationRequested)
            {
               try
                {
                    await DoExecuteAsync(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "GroupMessageSenderService异常");
                }
            }
        }

        private async Task DoExecuteAsync(StackExchange.Redis.IDatabase db)
        {
            //AllGroupsName保存的是组的名字
            var groupNames = await db.SetMembersAsync("AllGroupsName");
            List<Task> tasks = new List<Task>();
            foreach (string groupName in groupNames)
            {
                var task = ProcessGroupMessageAsync(db, groupName);
                tasks.Add(task);
            }
            //Task.WaitAll(tasks.ToArray());
            await Task.WhenAll(tasks.ToArray());
        }

        private async Task ProcessGroupMessageAsync(StackExchange.Redis.IDatabase db, string groupName)
        {
            List<GroupMessageResp> msgs = new List<GroupMessageResp>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //要么没有更多待发消息立即发给客户端
            //要么累积满1秒钟待发消息后发送给客户端
            while (true)
            {
                //弹出一条待发送消息
                string jsonStr = await db.ListRightPopAsync($"{groupName}_MessagesWaitToSent");
                if (jsonStr != null)
                {
                    //加入群发消息组
                    var msg = JsonConvert.DeserializeObject<GroupMessageResp>(jsonStr);
                    msgs.Add(msg);
                }
                else
                {
                    await Task.Delay(10);
                }
                //满一段时间的消息就向客户端发一组
                if (stopwatch.Elapsed > TimeSpan.FromSeconds(0.5))
                {
                    stopwatch.Stop();
                    //把积累的一组数据发给客户端
                    if (msgs.Any())
                    {
                        logger.LogWarning("过去了{0}准备向{1}发送一批消息,条数{2}", stopwatch.Elapsed,groupName, msgs.Count);
                        //如果一个周期内消息太多,则只发送一部分,其他的丢弃,以避免造成一次性发给客户端的消息太多
                        await this.hubContext.Clients.Group(groupName).SendAsync("OnGroupMessages", msgs.Take(20));
                        logger.LogWarning("完成向{0}发送一批消息,条数{1}", groupName, msgs.Count);
                    }
                    return;//本轮巡查完毕
                }
            }
        }
    }
}
