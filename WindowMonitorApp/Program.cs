using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using WindowMoniterApp;
using System.Net;
using System.Diagnostics;

namespace WindowMonitorApp
{
    public class Program
    {


        public static async Task<int> Main(string[] args)
        {
            Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                         .MinimumLevel.Information()
#endif
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                         .Enrich.FromLogContext()
                         .WriteTo.Async(c => c.File($"{AppContext.BaseDirectory}/Logs/logs.txt",
                                                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                                                    rollingInterval: RollingInterval.Day,
                                                    retainedFileCountLimit: 20, encoding: Encoding.UTF8,
                                                    fileSizeLimitBytes: 200L * 1024 * 1024, // 200MB
                                                    rollOnFileSizeLimit: true))
                         .WriteTo.Async(c => c.Console())
                         .CreateLogger();

            try
            {
                //Task线程内未捕获异常处理事件
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                //非UI线程未捕获异常处理事件
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Log.Information("Starting console host.");
                await CreateHostBuilder(args).Build().RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("程序发生致命错误，将终止，请联系运营商！\n");
            }
            sbEx.Append("捕获未处理异常：");
            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            Log.Error(sbEx.ToString());
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved(); //设置该异常已察觉（这样处理后就不会引起程序崩溃）
            Log.Error("捕获线程内未处理异常" + e.Exception.Message);
        }

        internal static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
             .ConfigureAppConfiguration(configure =>
             {
                 configure.AddJsonFile("appsettings.json");  //无法热修改

             })
                   .UseSerilog()
                   .UseWindowsService()
                   .ConfigureWebHostDefaults((webBuilder) =>
                   {
                       webBuilder.UseStartup<Startup>().UseKestrel(opt =>
                       {
                           opt.Listen(IPAddress.Any, 50003);
                           opt.Limits.MaxRequestBodySize = null;
                       }).UseDefaultServiceProvider(opt => opt.ValidateScopes = false);
                   });
    }
}
