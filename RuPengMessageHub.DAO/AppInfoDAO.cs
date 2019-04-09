using RuPengMessageHub.Models;
using System;
using System.Linq;

namespace RuPengMessageHub.DAO
{
    public class AppInfoDAO: IDAOSupport
    {
        public AppInfo[] GetAll()
        {
            //暂时写死，以后搞到数据库中
            AppInfo info1 = new AppInfo {
                Id = Guid.Parse("6CEB02AB-925D-47DF-9547-2437B952A204"),
                AppKey = "rupenggongkaike",
                AppSecret = "faafasd333u_6xx!aa",
                AppName = "如鹏公开课聊天室"

            };
            AppInfo info2 = new AppInfo
            {
                Id = Guid.Parse("6CEB02AB-925D-47DF-9547-2437B952A206"),
                AppKey = "rupengIM",
                AppSecret = "xmmx3@66_6xuu@aa",
                AppName = "如鹏聊天软件"

            };
            AppInfo[] appInfos = new AppInfo[] { info1, info2 };
            return appInfos;
        }

        public AppInfo GetByAppKey(string appKey)
        {
            //todo:以后如果要存到数据库里，一定要做好缓存，因为CheckAuthorizationFilter中在频繁调用
            var results = GetAll().Where(a => a.AppKey.Equals(appKey, StringComparison.OrdinalIgnoreCase));
            if(results.Count()>1)
            {
                throw new ApplicationException("appKey有重复");
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
