using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence.Configurations;

public sealed class CoordinateConfiguration: IEntityTypeConfiguration<Coordinate>
{
    public void Configure(EntityTypeBuilder<Coordinate> builder)
    {
        builder
            .HasOne(c => c.User)
            .WithOne(u => u.LatestCoordinates)
            .HasForeignKey<Coordinate>();
    }
}