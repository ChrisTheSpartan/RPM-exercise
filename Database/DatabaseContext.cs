using System.Configuration;
using Microsoft.EntityFrameworkCore;
using RPM_exercise.Database;

namespace RPM_exercise
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
        {
        }

        public virtual DbSet<Price> Prices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConfigurationManager.AppSettings["dbConnection"]);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Price>(entity =>
            {
                entity.ToTable("Price");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Period)
                    .IsRequired();

                entity.Property(e => e.Value)
                     .IsRequired(); ;
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
