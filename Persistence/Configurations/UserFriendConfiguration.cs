using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence.Configurations;

public class UserFriendConfiguration: IEntityTypeConfiguration<UserFriend>
{
    public void Configure(EntityTypeBuilder<UserFriend> builder)
    {
        builder
            .HasOne(ur => ur.User)
            .WithMany(u => u.Friends)
            .HasForeignKey(uf => uf.UserId)
            .IsRequired();
        
        builder
            .HasOne(ur => ur.Friend)
            .WithMany()
            .HasForeignKey(uf => uf.FriendId)
            .IsRequired();
    }
}