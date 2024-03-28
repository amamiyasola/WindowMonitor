
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WindowMoniterApp;
using WindowMonitorApp.Commons;
using WindowMonitorApp.EntityFrameworkCore.DbContexts;
using WindowMonitorApp.IServices;
using WindowMonitorApp.Services;

namespace WindowMonitorApp
{

    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IMonitorService, MonitorService>();
            services.AddSingleton<MyFreeSql>();
            services.AddHangfire(configuration => configuration
                                                  .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                                                  .UseSimpleAssemblyNameTypeSerializer()
                                                  .UseRecommendedSerializerSettings()
                                                  .UseLiteDbStorage(Directory.GetCurrentDirectory() + "\\" + _configuration["ConnectionStrings:url1"], new LiteDbStorageOptions
                                                  {

                                                  }));
            services.AddHostedService<WindowMonitorHostservice>();
            // 注册Swagger
            services.AddSwaggerGen(u =>
            {
                u.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "Ver:1.0.0",//版本
                    Title = "后台资源监控管理系统",//标题
                    Description = "后台资源监控管理系统：amamiyasola @20240328",//描述
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "xxx",
                        Email = "2222@qq.com"
                    }
                });
            });
            services.AddControllers();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var monitorService = app.ApplicationServices.GetService<IMonitorService>();
            monitorService.StartMonitor();
            //app.UseHangfireServer();
            app.UseSwagger();
            app.UseSwaggerUI(u =>
            {
                u.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI_v1");
            });

            app.UseHangfireDashboard();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
            });
        }
    }
}
