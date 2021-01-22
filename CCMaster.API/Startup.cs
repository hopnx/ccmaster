using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using CCMaster.API.Hubs.Chat;
using CCMaster.API.Services;
using CCMaster.API.Models;
using CCMaster.API.Hubs.Login;
using CCMaster.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using CCMaster.API.Hubs;

namespace CCMaster.API
{
    public class Startup
    {
        public IHostEnvironment HostEnvironment { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            HostEnvironment = env;
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            ClientConfiguration client = new ClientConfiguration
            {
                Url = Configuration.GetValue<string>("clientConfiguration:url")
            };
            services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                         .SetIsOriginAllowed(origin => true) // allow any origin
                                                             //.WithOrigins(client.Url)
                        .AllowCredentials();
                });
            });
            services.AddControllers();
            services.AddSignalR();

            RedisConfiguration redis = new RedisConfiguration
            {
                Configuration = Configuration.GetValue<string>("redisConfiguration:url"),
                InstanceName = Configuration.GetValue<string>("redisConfiguration:instanceName")
            };
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = redis.Configuration;
                option.InstanceName = redis.InstanceName;
            });

            services.Configure<MongoConfiguration>(
                Configuration.GetSection("mongoConfiguration"));
            services.AddSingleton<IMongoConfiguration>(sp =>
                sp.GetRequiredService<IOptions<MongoConfiguration>>().Value);


            services.AddScoped<IChatService, ChatService>();
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IPlayerService, PlayerService>();
            services.AddSingleton<IBoardService, BoardService>();
            services.AddSingleton<IGameConfigService, GameConfigService>();
            services.AddSingleton<IGameService, GameService>();

            services.AddSingleton<ICommonService, CommonService>();

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("ClientPermission");
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>(ChatHub.Route);
                endpoints.MapHub<LoginHub>(LoginHub.Route);
                endpoints.MapHub<PlayerHub>(PlayerHub.Route);
                endpoints.MapHub<BoardHub>(BoardHub.Route);
                endpoints.MapHub<GameHub>(GameHub.Route);

                endpoints.MapHub<CommonHub>(CommonHub.Route);

            });
        }
    }
}
