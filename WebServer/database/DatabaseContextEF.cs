using WebServer.database.Models;
using Microsoft.EntityFrameworkCore;

namespace WebServer.database
{
    public class DatabaseContextEF : DbContext
    {
        public DbSet<Meter> Meters { get; set; }
        public DbSet<MeterMapping> MeterMappings { get; set; }
        public DbSet<TcpMedia> TcpMedias{ get; set; }
        public DbSet<ComPortMedia> ComPortMedias{ get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"Data Source=.\\database\\IndustrialMeter.db");
    }
}
