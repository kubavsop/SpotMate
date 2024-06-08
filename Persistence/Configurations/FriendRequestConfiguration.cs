using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence.Configurations;

public sealed class FriendRequestConfiguration: IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {

        
        builder
            .HasOne(r => r.ReceiverUser)
            .WithMany(u => u.ReceivedRequests)
            .HasForeignKey(r => r.ReceiverUserId)
            .IsRequired();
        
        builder
            .HasOne(r => r.SenderUser)
            .WithMany(u => u.SentRequests)
            .HasForeignKey(r => r.SenderUserId)
            .IsRequired();
    }
}