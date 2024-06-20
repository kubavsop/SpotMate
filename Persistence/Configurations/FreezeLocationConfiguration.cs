using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence.Configurations;

public sealed class FreezeLocationConfiguration: IEntityTypeConfiguration<FreezeLocation>
{
    public void Configure(EntityTypeBuilder<FreezeLocation> builder)
    {
        builder
            .HasOne(f => f.FreezerUser)
            .WithMany(u => u.FreezerUserLocations)
            .HasForeignKey(f => f.FreezerUserId)
            .IsRequired();
        
        builder
            .HasOne(f => f.User)
            .WithMany(u => u.UserLocations)
            .HasForeignKey(f => f.UserId)
            .IsRequired();
    }
}