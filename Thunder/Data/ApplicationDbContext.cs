using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using Thunder.Models;

namespace Thunder.Data
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        { }
        public string? FullName { get; set; }
        public int UserBalance { get; set; }
        public string? StripeCustomerId { get; set; }        
    }


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UpsOrderDetails> UpsOrderDetails { get; set; }
        public DbSet<RateCosts> RateCosts { get; set; }
        public DbSet<ReturnAddress> ReturnAddress { get; set; }
        public DbSet<LabelDetails> LabelDetails { get; set; }
        public DbSet<UnfinishedLabel> UnfinishedLabel { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UpsOrderDetails>()
                .HasOne(uo => uo.UnfinishedLabel)
                .WithOne(ul => ul.UpsOrderDetails)
                .HasForeignKey<UnfinishedLabel>(ul => ul.LabelId);
            modelBuilder.Entity<ReturnAddress>().ToTable("RateCosts");
            modelBuilder.Entity<ReturnAddress>().ToTable("ReturnAddress");
            modelBuilder.Entity<LabelDetails>().ToTable("LabelDetails");
            modelBuilder.Entity<UnfinishedLabel>().ToTable("UnfinishedLabel");
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