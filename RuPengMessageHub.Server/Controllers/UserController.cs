using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RuPengMessageHub.DAO;
using RuPengMessageHub.Helpers;
using RuPengMessageHub.Server.Settings;
using RuPengMessageHub.Server.ViewModels;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private AppInfoDAO appInfoDAO;
        private IOptions<BearerJWTSettings> jwtSetting;

        public UserController(AppInfoDAO appInfoDAO, IOptions<BearerJWTSettings> jwtSetting)
        {
            this.appInfoDAO = appInfoDAO;
            this.jwtSetting = jwtSetting;
        }

        private IActionResult Create400Result(string msg)
        {
             return new ContentResult()
            {
                Content = msg,
                ContentType = "application/text",
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        [HttpPost()]
        [Route("GetToken")]
        public async Task<IActionResult> GetTokenAsync(UserGetTokenRequest req)
        {
            string jwtSecret = jwtSetting.Value.Secret;
            
            var appInfo = appInfoDAO.GetByAppKey(req.AppKey);
            if (appInfo == null)
            {
                return Create400Result("AppKey=" + req.AppKey + " not found!");
            }
            string appSecret = appInfo.AppSecret;

            //注意区分AppSecret和当前项目配置中的JWTSecret
            //1、appsettings.json配置文件中的JWTSecret是只有RuPengMessageHub.Server知道的
            //而使用这个平台的具体应用App服务器开发者是不知道的。
            //2、AppSecret则是具体应用App服务器开发者和RuPengMessageHub.Server配置协商好的，
            //App的用户是无法得知的

            //校验设备传过来的App相关的Secret是否一致，一致的标准就是计算签名
            String signature = SecurityHelper.GetHash(appSecret +  req.Timestamp);
            if (!signature.Equals(req.Signature, StringComparison.OrdinalIgnoreCase))
            {
                return Create400Result("Signature validation error");
            }   

            //Add Claims

            var claims = new[]
            {
                new Claim("AppKey", req.AppKey),
                new Claim("DisplayName", req.DisplayName),
                new Claim("UserId",req.UserId),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)); //Secret
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(jwtSetting.Value.Issuer,
                jwtSetting.Value.Audience,claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

            var  access_token = new JwtSecurityTokenHandler().WriteToken(token);
            return Content(access_token, "application/text"); 
        }

    }
}
