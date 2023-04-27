using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thunder.Data;
using Thunder.Models;

public class UserBalanceEntityConfiguration : IEntityTypeConfiguration<UserBalance>
{
    public void Configure(EntityTypeBuilder<UserBalance> builder)
    {
        // Set a one-to-one relationship between UserBalance and ApplicationUser
        builder.HasOne(ub => ub.User)
               .WithOne(u => u.UserBalance)
               .HasForeignKey<UserBalance>(ub => ub.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Specify the store type for the Balance property
        builder.Property(ub => ub.Balance)
               .HasColumnType("decimal(18, 2)");

        // You can add other configurations, such as unique constraints or indexes, if necessary
    }
}
