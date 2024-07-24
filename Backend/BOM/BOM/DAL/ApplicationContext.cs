using BOM.Models;
using Microsoft.EntityFrameworkCore;

namespace BOM.DAL
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Specification> Specification { get; set; }
        public DbSet<Order> Order { get; set; }

        public DbSet<Storage > Storage { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
           : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);
        }

    }
}

