using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence;

public sealed class ApplicationDbContext: IdentityDbContext<SpotMateUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public DbSet<Coordinate> Coordinates { get; init; }
    
    public DbSet<DailyStep> DailySteps { get; init; }
    public DbSet<FriendRequest> FriendRequests { get; init; }
    public DbSet<UserFriend> UserFriends { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    public DbSet<UserInterest> UserInterests { get; init; }
    public DbSet<Interest> Interests { get; init; }
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}