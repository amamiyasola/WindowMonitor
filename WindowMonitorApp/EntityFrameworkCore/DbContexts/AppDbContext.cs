using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using WindowMonitorApp.Enititys;

namespace WindowMonitorApp.EntityFrameworkCore.DbContexts
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options,IConfiguration configuration) : base(options)
        {
            this._configuration = configuration;
        }

        public DbSet<ComputerRunInfo> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 连接字符串
            string ConnString = Directory.GetCurrentDirectory() + "\\" + _configuration["ConnectionStrings:url"];
            // 连接SqlServer

            base.OnConfiguring(optionsBuilder); 
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ComputerRunInfo>().HasKey("Id");
            base.OnModelCreating(modelBuilder); 
        }
    }
}
