using Microsoft.Extensions.Configuration;
using RuPengMessageHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuPengMessageHub.DAO
{
    public class AppInfoDAO: IDAOSupport
    {
        private IConfigurationRoot configRoot;
        private List<AppInfo> appInfos = new List<AppInfo>();

        public AppInfoDAO(IConfigurationRoot configRoot)
        {
            this.configRoot = configRoot;
            var appInfosSec = configRoot.GetSection("AppInfos");
            foreach (var appInfoSec in appInfosSec.GetChildren())
            {
                string id = appInfoSec.GetSection("Id").Value;
                string appKey = appInfoSec.GetSection("AppKey").Value;
                string appSecret = appInfoSec.GetSection("AppSecret").Value;
                string appName = appInfoSec.GetSection("AppName").Value;
                AppInfo info = new AppInfo
                {
                    Id = Guid.Parse(id),
                    AppKey = appKey,
                    AppSecret = appSecret,
                    AppName = appName
                };
                appInfos.Add(info);
            }
        }

        public AppInfo[] GetAll()
        {
            //todo: stored in Database in the next version
            return appInfos.ToArray();
        }

        public AppInfo GetByAppKey(string appKey)
        {
            //todo: if those data will be stored in Database in the next version,
            //cache must be added here, becuase CheckAuthorizationFilter will be invoked frequently
            var results = GetAll().Where(a => a.AppKey.Equals(appKey, StringComparison.OrdinalIgnoreCase));
            if(results.Count()>1)
            {
                throw new ApplicationException("appKey duplicated");
            }
            else if(results.Count()<=0)
            {
                return null;
            }
            else
            {
                return results.Single();
            }
        }
    }
}
