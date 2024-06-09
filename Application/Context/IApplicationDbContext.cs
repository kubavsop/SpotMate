using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Context;

public interface IApplicationDbContext
{
    DbSet<FriendRequest> FriendRequests { get; }
    DbSet<UserFriend> UserFriends { get; }
    DbSet<SpotMateUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserInterest> UserInterests { get;  }
    DbSet<Interest> Interests { get; }
    public DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}