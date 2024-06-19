using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence.Configurations;

internal sealed class SpotMateUserConfiguration: IEntityTypeConfiguration<SpotMateUser>
{
    public void Configure(EntityTypeBuilder<SpotMateUser> builder)
    {
        builder
            .HasMany(u => u.Interests)
            .WithMany(i => i.Users)
            .UsingEntity<UserInterest>();
    }
}