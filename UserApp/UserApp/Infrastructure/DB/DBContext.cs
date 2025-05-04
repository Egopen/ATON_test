using Microsoft.EntityFrameworkCore;
using UserApp.Domain.Services.Implemetations;
using UserApp.Infrastructure.DB.Models;

namespace UserApp.Infrastructure.DB
{
    public class DBContext:DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("DBPATH"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.User>(entity =>
            {

                entity.Property(e => e.CreatedOn)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ModifiedOn)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAddOrUpdate();
                entity.HasIndex(u => u.Login).IsUnique();
                entity.HasIndex(u => u.Guid).IsUnique();

            });
            modelBuilder.Entity<Models.User>().HasData(
                new User { Login="Admin1", Gender=0,Password=new HashService().HashString("admin123"), CreatedBy="Admin1",ModifiedBy="Admin1", Name="Admin",
                Admin=true}
                );
        }
    }
}
