using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;

namespace WindowMonitorApp.Commons
{
    public class MyFreeSql
    {
        private readonly IConfiguration _configuration;
        public IFreeSql _freeSql;

        public MyFreeSql(IConfiguration configuration)
        {
            this._configuration = configuration;

            _freeSql = (IFreeSql)new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Sqlite, $"data source={Directory.GetCurrentDirectory()}\\{_configuration["ConnectionStrings:url2"]}")
        .UseMonitorCommand(cmd => Trace.WriteLine($"线程：{cmd.CommandText}\r\n"))
        .UseAutoSyncStructure(true) //自动创建、迁移实体表结构
        .UseNoneCommandParameter(true)
        .Build();
        }
    }
}
