using ElAmir.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElAmir.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<SC_Columns> SC_Columns { get; set; }
        public DbSet<SC_Visit_Pic> SC_Visit_Pic { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SC_Columns>().HasKey(x => x.SC_Customer_ID);

            modelBuilder.Entity<SC_Visit_Pic>().HasKey(e => e.SC_Visit_Pic_ID);

            modelBuilder.Entity<SC_Visit_Pic>()
            .Property(p => p.Insert_Date)
            .HasColumnType("DATETIME2");

            modelBuilder.Entity<QSCustomerBrandTarget>().HasKey(x => x.ID);

            modelBuilder.Entity<QSCustomerBrandTarget>()
                .Property(e => e.Target_Qty)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QSCustomerBrandTarget>()
                .Property(e => e.Target_SU)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QSCustomerBrandTarget>()
                .Property(e => e.target_LE)
                .HasPrecision(18, 2);

            

            base.OnModelCreating(modelBuilder);
        }
    }
}
