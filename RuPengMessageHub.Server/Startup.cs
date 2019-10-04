using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RuPengMessageHub.DAO;
using RuPengMessageHub.Server.Helpers;
using RuPengMessageHub.Server.Settings;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var daoASM = Assembly.Load(new AssemblyName("RuPengMessageHub.DAO"));
            foreach (Type serviceType in daoASM.GetTypes()
                .Where(t => typeof(IDAOSupport).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract))
            {
                /*
                var interfaceTypes = serviceType.GetInterfaces();
                foreach (var interfaceType in interfaceTypes)
                {
                    services.AddSingleton(interfaceType, serviceType);
                }*/
                services.AddSingleton(serviceType);
            }

            //注册所有Filter
            var currentASM = typeof(Startup).Assembly;
            foreach (Type serviceType in currentASM.GetTypes()
                .Where(t => typeof(IFilterMetadata).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract))
            {
                services.AddSingleton(serviceType);
            }

            var config = new ConfigurationBuilder()
                    .AddInMemoryCollection()    //load configurations into memory
                    .SetBasePath(Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)  //指定加载的配置文件
                    .Build();    //编译成对象  

            //配置的注入https://www.cnblogs.com/skig/p/6079187.html
            services.AddOptions()
                .Configure<RedisSetting>(config.GetSection("Redis"))
                .Configure<BearerJWTSettings>(config.GetSection("BearerJWT"))
                .Configure<CorsSettings>(config.GetSection("Cors"));
            services.AddSingleton<IConfigurationRoot>(config);
            services.AddSingleton<RedisHelper>();
            services.AddHostedService<GroupMessageSenderService>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders();
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var sp = services.BuildServiceProvider();
                var jwtSetting = sp.GetService<IOptions<BearerJWTSettings>>();
                string jwtSecret = jwtSetting.Value.Secret;
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = jwtSetting.Value.Issuer,//ValidIssuer、ValidAudience必须设置，要和验证方JwtSecurityToken的构造函数的前两个参数一致
                    ValidAudience = jwtSetting.Value.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)) //Secret
                };
                //有些情况下JWT的值会放到queryString中的access_token，而不是在头的Authentication中，因此需要从querystring的access_token
                //读取出来设置到头上
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/messageHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddCors();
            services.AddSignalR();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //给SignalR的CallContext的UserIdentifier选择值用
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

            services.BuildServiceProvider();
       }

       // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
       public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
           IOptions<CorsSettings> corsSetting, ILoggerFactory loggerFactory)
       {
           if (env.IsDevelopment())
           {
               app.UseDeveloperExceptionPage();
           }

            //Startup.ConfigureServices() is called before Startup.Configure()
            //所以可以在Configure参数中注入，也可以app.ApplicationServices.GetRequiredService<IOptions<CorsSettings>>()
            //手动获取，但是对于读取配置文件必须注入IOptions<CorsSettings>，而不能直接注入CorsSettings

            app.UseAuthentication();//不能丢
            // Make sure the CORS middleware is ahead of SignalR.
            //注意顺序：UseCors、UseSignalR、UseMvc
            app.UseCors(builder =>
            {
                builder.WithOrigins(corsSetting.Value.AllowedOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST")
                    .AllowCredentials();
            });

            var webSocketOptions = new WebSocketOptions();
            foreach(var origin in corsSetting.Value.AllowedOrigins)
            {
                webSocketOptions.AllowedOrigins.Add(origin);
            }
            app.UseWebSockets(webSocketOptions);
           
            app.UseSignalR(routes =>
            {
                routes.MapHub<MessageHub>("/messageHub");
            });
             app.UseMvc();

            app.UseStaticFiles();
       }
   }
}
