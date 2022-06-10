using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBook.Entity;
using MyBook.Entity.Identity;

namespace MyBook.DataAccess.Seed;

public class UserRoleSeed : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasData(
            new UserRole
            {
                UserId   = Guid.Parse("4bee3a36-db98-4071-ad61-a61db810decb"),
                RoleId = Guid.Parse("6f17d951-3ad5-49f9-b333-2a37e367333d"),
            },
            new UserRole
            {
                UserId   = Guid.Parse("4bee3a36-db98-4071-ad61-a61db810decb"),
                RoleId = Guid.Parse("12d534a7-4535-4819-8704-bcfd7553ab46"),
            },
            new UserRole
            {
                UserId   = Guid.Parse("36211241-8aa8-408c-ae60-1459399441f0"),
                RoleId = Guid.Parse("6dc02633-d464-4f86-8575-4cb190d670a6"),
            },
            new UserRole
            {
                UserId   = Guid.Parse("36211241-8aa8-408c-ae60-1459399441f0"),
                RoleId = Guid.Parse("12d534a7-4535-4819-8704-bcfd7553ab46"),
            }
        );
    }
}