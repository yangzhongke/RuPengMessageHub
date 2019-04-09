namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetAppKey(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("AppKey").Value;
        }

        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("UserId").Value;
        }

        public static string GetUserDisplayName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("DisplayName").Value;
        }

        /// <summary>
        /// 获得组Id，做App隔离用
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static String GetGroupName(this ClaimsPrincipal principal,string groupId)
        {
            string appKey = principal.GetAppKey();
            return $"App{appKey}_group{groupId}";
        }
    }
}
