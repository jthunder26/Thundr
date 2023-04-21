using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thunder.Models;

namespace Thunder.Data
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        { }
        public string FullName { get; set; }
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UpsOrder> UpsOrder { get; set; }
        public DbSet<ReturnAddress> ReturnAddress { get; set; }
        public DbSet<LabelDetails> LabelDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UpsOrder>().ToTable("UpsOrder");
            modelBuilder.Entity<ReturnAddress>().ToTable("ReturnAddress");
            modelBuilder.Entity<LabelDetails>().ToTable("LabelDetails");
            modelBuilder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
        }
    }

    public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName).HasMaxLength(400);
        }
    }
}