using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence.Configurations;

public class UserFriendConfiguration: IEntityTypeConfiguration<UserFriend>
{
    public void Configure(EntityTypeBuilder<UserFriend> builder)
    {
        builder
            .HasOne(ur => ur.FirstUser)
            .WithMany()
            .HasForeignKey(uf => uf.FirstUserId)
            .IsRequired();
        
        builder
            .HasOne(ur => ur.SecondUser)
            .WithMany()
            .HasForeignKey(uf => uf.SecondUserId)
            .IsRequired();
    }
}