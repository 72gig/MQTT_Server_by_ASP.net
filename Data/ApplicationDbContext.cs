using Microsoft.EntityFrameworkCore;


namespace MqttWatch.Data{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MqttTable> mqtt_table { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MqttTable>()
                .ToTable("mqtt_table");  // 映射到資料庫中的 mqtt_table
        }
    }

    public class MqttTable
    {
        public int Key_id { get; set;}
        public string topic { get; set;}
        public string payload { get; set;}
        public DateTime record_time { get; set;}
    }
}